using System.Text.Json;
using System.Text.Json.Nodes;

namespace Albatross.Json {
	public static class Extensions {
		private static string? FindPropertyKey(JsonObject obj, string key, bool caseInsensitive) {
			if (!caseInsensitive) {
				return obj.ContainsKey(key) ? key : null;
			}
			foreach (var prop in obj) {
				if (string.Equals(prop.Key, key, StringComparison.OrdinalIgnoreCase)) {
					return prop.Key;
				}
			}
			return null;
		}

		/// <summary>
		/// Sets a value at the specified path within a JSON node tree, creating or reshaping intermediate nodes as needed.
		/// </summary>
		/// <param name="node">Root of the tree to modify. A new <see cref="JsonObject"/> is created when null.</param>
		/// <param name="path">
		/// Dot-free path segments to navigate. Integer strings are treated as array indices;
		/// all other strings are treated as object property names.
		/// </param>
		/// <param name="value">Value to serialize and place at the target path.</param>
		/// <param name="options">Serializer options applied to both navigation (case sensitivity) and value serialization.</param>
		/// <param name="initOnly">
		/// When true, skips the write if a non-null value already exists at the target path.
		/// Useful for setting defaults without overwriting existing data.
		/// </param>
		/// <returns>
		/// The modified root node. When <paramref name="path"/> is empty, returns the serialized
		/// <paramref name="value"/> directly, replacing the root.
		/// </returns>
		/// <remarks>
		/// Node reshaping rules applied during traversal:
		/// <list type="bullet">
		///   <item>Missing object properties and null/value intermediate nodes are replaced with a new <see cref="JsonObject"/>.</item>
		///   <item>Array + integer index in bounds: navigates or sets that element.</item>
		///   <item>Array + integer index out of bounds: appends one element to the end.</item>
		///   <item>Array + non-integer key: replaces the array with a <see cref="JsonObject"/> — the path segment implies object structure.</item>
		/// </list>
		/// </remarks>
		public static JsonNode? SetValue<T>(this JsonNode? node, string[] path, T? value, JsonSerializerOptions options, bool initOnly = false) {
			if (node == null) { node = new JsonObject(); }
			var serializedValue = JsonSerializer.SerializeToNode(value, options);
			if (path.Length == 0) {
				return serializedValue;
			}
			JsonNode current = node;
			JsonNode? parent = null;
			string? parentKey = null;

			for (int i = 0; i < path.Length - 1; i++) {
				string key = path[i];
				if (current is JsonObject obj) {
					string actualKey = FindPropertyKey(obj, key, options.PropertyNameCaseInsensitive) ?? key;
					if (!obj.TryGetPropertyValue(actualKey, out var next) || next is null || next is JsonValue) {
						if (actualKey != key) { obj.Remove(actualKey); }
						next = new JsonObject();
						obj[key] = next;
					} else if (actualKey != key) {
						obj.Remove(actualKey);
						obj[key] = next;
					}
					parent = current;
					parentKey = key;
					current = next;
				} else if (current is JsonArray arr) {
					if (int.TryParse(key, out int index)) {
						if (index < 0) { throw new ArgumentException($"Negative array index: {key}"); }
						JsonNode next;
						if (index < arr.Count) {
							var elem = arr[index];
							if (elem is null || elem is JsonValue) {
								elem = new JsonObject();
								arr[index] = elem;
							}
							next = elem;
						} else {
							next = new JsonObject();
							arr.Add(next);
						}
						parent = current;
						parentKey = key;
						current = next;
					} else {
						// non-integer key: replace array with JsonObject and retry
						var replacement = new JsonObject();
						if (parent is JsonObject parentObj) {
							parentObj[parentKey!] = replacement;
						} else if (parent is JsonArray parentArr && int.TryParse(parentKey, out int idx)) {
							parentArr[idx] = replacement;
						} else {
							node = replacement;
						}
						current = replacement;
						i--;
					}
				} else {
					var replacement = new JsonObject();
					if (parent is JsonObject parentObj) {
						parentObj[parentKey!] = replacement;
					} else if (parent is JsonArray parentArr && int.TryParse(parentKey, out int idx)) {
						parentArr[idx] = replacement;
					} else {
						node = replacement;
					}
					current = replacement;
					i--;
				}
			}

			string finalKey = path[^1];
			if (current is JsonObject finalObj) {
				string actualFinalKey = FindPropertyKey(finalObj, finalKey, options.PropertyNameCaseInsensitive) ?? finalKey;
				if (initOnly && finalObj.TryGetPropertyValue(actualFinalKey, out var existing) && existing != null) {
					return node;
				}
				if (actualFinalKey != finalKey) { finalObj.Remove(actualFinalKey); }
				finalObj[finalKey] = serializedValue;
			} else if (current is JsonArray finalArr) {
				if (int.TryParse(finalKey, out int index)) {
					if (index < 0) { throw new ArgumentException($"Negative array index: {finalKey}"); }
					if (index < finalArr.Count) {
						if (initOnly && finalArr[index] != null) { return node; }
						finalArr[index] = serializedValue;
					} else {
						finalArr.Add(serializedValue);
					}
				} else {
					// non-integer key: replace array with JsonObject
					var replacement = new JsonObject { [finalKey] = serializedValue };
					if (parent is JsonObject parentObj) {
						parentObj[parentKey!] = replacement;
					} else if (parent is JsonArray parentArr && int.TryParse(parentKey, out int idx)) {
						parentArr[idx] = replacement;
					} else {
						return replacement;
					}
				}
			} else {
				var replacement = new JsonObject { [finalKey] = serializedValue };
				if (parent is JsonObject parentObj) {
					parentObj[parentKey!] = replacement;
				} else if (parent is JsonArray parentArr && int.TryParse(parentKey, out int idx)) {
					parentArr[idx] = replacement;
				} else {
					return replacement;
				}
			}
			return node;
		}
	}
}
