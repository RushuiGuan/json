using System.Text.Json;
using System.Text.Json.Nodes;

namespace Albatross.Json {
	public static class Extensions {
		public static readonly JsonSerializerOptions Options = new() {
			WriteIndented = true,
			IndentCharacter = ' ',
			IndentSize = 4
		};

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
		public static JsonNode? SetValue<T>(JsonNode? node, string[] path, T? value) {
			if (node == null) { node = new JsonObject(); }
			var serializedValue = System.Text.Json.JsonSerializer.SerializeToNode(value, Options);
			if (path.Length == 0) {
				return serializedValue;
			}
			JsonNode current = node;
			JsonNode? parent = null;
			string? parentKey = null;

			for (int i = 0; i < path.Length - 1; i++) {
				string key = path[i];
				if (current is JsonObject obj) {
					if (!obj.TryGetPropertyValue(key, out var next) || next is null || next is JsonValue) {
						next = new JsonObject();
						obj[key] = next;
					}
					parent = current;
					parentKey = key;
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
				finalObj[finalKey] = serializedValue;
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