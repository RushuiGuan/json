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
		/// Sets a value at the specified path within a JSON node structure.
		/// </summary>
		/// <typeparam name="T">The type of the value to set.</typeparam>
		/// <param name="node">The root JSON node. If null, a new JsonObject is created.</param>
		/// <param name="path">The path segments to navigate. Use numeric strings for array indices.</param>
		/// <param name="value">The value to set at the specified path.</param>
		/// <returns>
		/// The modified root node, or the serialized value if path is empty.
		/// </returns>
		/// <remarks>
		/// Behavior:
		/// <list type="bullet">
		///   <item>If path is empty, returns the serialized value directly (replaces entire node).</item>
		///   <item>Creates intermediate JsonObjects automatically when navigating non-existent paths.</item>
		///   <item>Overwrites existing value nodes (JsonValue) with JsonObjects when the path continues through them.</item>
		///   <item>For arrays, path segments must be valid integer indices within bounds.</item>
		/// </list>
		/// </remarks>
		/// <exception cref="ArgumentException">Thrown when an array index is out of bounds or not a valid integer.</exception>
		public static JsonNode? SetValue<T>(this JsonNode? node, string[] path, T? value, JsonSerializerOptions options) {
			if (node == null) { node = new JsonObject(); }
			var serializedValue = System.Text.Json.JsonSerializer.SerializeToNode(value, options);
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
						next = new JsonObject();
						obj[actualKey] = next;
					}
					parent = current;
					parentKey = actualKey;
					current = next;
				} else if (current is JsonArray arr) {
					if (int.TryParse(key, out int index) && index >= 0 && index < arr.Count) {
						var next = arr[index];
						if (next is null || next is JsonValue) {
							next = new JsonObject();
							arr[index] = next;
						}
						parent = current;
						parentKey = key;
						current = next;
					} else {
						throw new ArgumentException($"Invalid array index: {key}");
					}
				} else {
					// current is a JsonValue - replace it with JsonObject
					var replacement = new JsonObject();
					if (parent is JsonObject parentObj) {
						parentObj[parentKey!] = replacement;
					} else if (parent is JsonArray parentArr && int.TryParse(parentKey, out int idx)) {
						parentArr[idx] = replacement;
					}
					current = replacement;
					i--; // retry this path segment with the new object
				}
			}

			string finalKey = path[^1];
			if (current is JsonObject finalObj) {
				string actualFinalKey = FindPropertyKey(finalObj, finalKey, options.PropertyNameCaseInsensitive) ?? finalKey;
				finalObj[actualFinalKey] = serializedValue;
			} else if (current is JsonArray finalArr) {
				if (int.TryParse(finalKey, out int index) && index >= 0 && index < finalArr.Count) {
					finalArr[index] = serializedValue;
				} else {
					throw new ArgumentException($"Invalid array index: {finalKey}");
				}
			} else {
				// current is a JsonValue - replace it with JsonObject containing the final key
				var replacement = new JsonObject { [finalKey] = serializedValue };
				if (parent is JsonObject parentObj) {
					parentObj[parentKey!] = replacement;
				} else if (parent is JsonArray parentArr && int.TryParse(parentKey, out int idx)) {
					parentArr[idx] = replacement;
				}
			}
			return node;
		}
	}
}