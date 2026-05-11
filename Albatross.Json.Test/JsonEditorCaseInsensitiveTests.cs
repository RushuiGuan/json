using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using Xunit;

namespace Albatross.Json.Test {
	public class JsonEditorCaseInsensitiveTests {
		private static readonly JsonSerializerOptions options = new JsonSerializerOptions {
			PropertyNameCaseInsensitive = true,
		};

		public static IEnumerable<object[]> SetValueTestData => new List<object[]> {
			new object[] { "null", new[] { "a" }, 1, """{"a": 1}""" },
			new object[] { "{}", new[] { "a" }, 1, """{"a": 1}""" },
			new object[] { "[1,2,3]", new[] { "a" }, 1, """{"a": 1}""" },
			new object[] { """{"a":1}""", new[] { "a" }, 2, """{"a": 2}""" },
			new object[] { """{"a":1}""", new[] { "A" }, 2, """{"A": 2}""" },
			new object[] { """{"a":1}""", new[] { "a" }, null, """{"a": null}""" },
			new object[] { """{"a":1}""", new[] { "a" }, new[] { 1, 2, 3 }, """{"a": [1, 2, 3]}""" },
			new object[] { "[1,2,3]", new[] { "0" }, 1, "[1, 2, 3]" },
			new object[] { "[1,2,3]", new[] { "0" }, 2, "[2, 2, 3]" },
			new object[] { "[1,2,3]", new[] { "100" }, 4, "[1, 2, 3, 4]" },
			new object[] { "[1,2,3]", new[] { "3" }, 4, "[1, 2, 3, 4]" },

			// // Case-insensitive: nested property with different casing
			// new object[] { """{"Parent":{"Child":"old"}}""", new[] { "parent", "child" }, "new", """{"Parent":{"Child":"new"}}""" },
			// // Case-insensitive: deeply nested with mixed casing
			// new object[] { """{"Level1":{"Level2":{"Level3":"old"}}}""", new[] { "level1", "LEVEL2", "Level3" }, "new", """{"Level1":{"Level2":{"Level3":"new"}}}""" },
			// // Case-insensitive: update existing nested object property
			// new object[] { """{"Config":{"Host":"localhost"}}""", new[] { "config", "host" }, "127.0.0.1", """{"Config":{"Host":"127.0.0.1"}}""" },
			// // Case-insensitive: set new property on existing object accessed via different case
			// new object[] { """{"Settings":{}}""", new[] { "settings", "theme" }, "dark", """{"Settings":{"theme":"dark"}}""" },
			// // Case-insensitive: array access with case-insensitive parent
			// new object[] { """{"Items":[1,2,3]}""", new[] { "items", "1" }, 99, """{"Items":[1,99,3]}""" },
			// // Case-insensitive: set property inside array element with case-insensitive path
			// new object[] { """{"Users":[{"Id":1},{"Id":2}]}""", new[] { "users", "0", "name" }, "Alice", """{"Users":[{"Id":1,"name":"Alice"},{"Id":2}]}""" },
			// // Case-insensitive: update property inside array element with case-insensitive matching
			// new object[] { """{"Users":[{"Name":"Bob"}]}""", new[] { "users", "0", "name" }, "Alice", """{"Users":[{"Name":"Alice"}]}""" },
			// // Case-insensitive: PascalCase JSON with camelCase path
			// new object[] { """{"UserProfile":{"EmailAddress":"old@test.com"}}""", new[] { "userProfile", "emailAddress" }, "new@test.com", """{"UserProfile":{"EmailAddress":"new@test.com"}}""" },
			// // Case-insensitive: overwrite value node with object using different case
			// new object[] { """{"Name":"string"}""", new[] { "name", "nested" }, "value", """{"Name":{"nested":"value"}}""" },
			// // Case-insensitive: create new property when no match (exact case used)
			// new object[] { "{}", new[] { "NewProperty" }, "value", """{"NewProperty":"value"}""" },
		};

		[Theory]
		[MemberData(nameof(SetValueTestData))]
		public void TestSetValue(string json, string[] path, object? value, string expected) {
			var node = JsonNode.Parse(json);
			var result = Extensions.SetValue(node, path, value, options);
			var expectedFormatted = JsonNode.Parse(expected)?.ToJsonString(options);
			Assert.Equal(expectedFormatted, result?.ToJsonString(options));
		}

		[InlineData("[1,2,3]", 4, "-1")]
		public void TestSetValueException(string json, object? value, params string[] path) {
			Assert.Throws<ArgumentException>(() => {
				var node = JsonNode.Parse(json);
				var result = Extensions.SetValue(node, path, value, options);
			});
		}

		[Fact]
		public void SetValue_OutOfBoundArrayIndex_ExpandsArray() {
			var json = """{"Arr":[1,2,3]}""";
			var node = JsonNode.Parse(json);
			var result = Extensions.SetValue(node, ["arr", "10"], "value", options);
			Assert.Equal("""{"arr":[1,2,3,"value"]}""", result?.ToJsonString(options));
		}

		[Fact]
		public void SetValue_InvalidArrayIndexFormat_ReplacesArrayWithObject() {
			var json = """{"Arr":[1,2,3]}""";
			var node = JsonNode.Parse(json);
			var result = Extensions.SetValue(node, ["arr", "invalid"], "value", options);
			Assert.Equal("""{"arr":{"invalid":"value"}}""", result?.ToJsonString(options));
		}

		[Fact]
		public void TestSetValue_CaseInsensitive_InputCasingWins() {
			var json = """{"UserName":"old"}""";
			var node = JsonNode.Parse(json);
			var result = Extensions.SetValue(node, ["username"], "new", options);
			Assert.Contains("username", result?.ToJsonString());
			Assert.DoesNotContain("UserName", result?.ToJsonString());
		}

		[Fact]
		public void TestSetValue_CaseInsensitive_MultiplePropertiesDifferentCase() {
			var json = """{"Name":"John","AGE":30,"Address":"123 Main St"}""";
			var node = JsonNode.Parse(json);
			var result = Extensions.SetValue(node, ["name"], "Jane", options);
			result = Extensions.SetValue(result, ["age"], 25, options);
			result = Extensions.SetValue(result, ["address"], "456 Oak Ave", options);
			var resultString = result?.ToJsonString();
			Assert.Contains("name", resultString);
			Assert.Contains("age", resultString);
			Assert.Contains("address", resultString);
		}

		[Fact]
		public void TestSetValue_CaseInsensitive_NestedObjectTraversal() {
			var json = """{"Root":{"Branch":{"Leaf":"old"}}}""";
			var node = JsonNode.Parse(json);
			var result = Extensions.SetValue(node, ["ROOT", "BRANCH", "LEAF"], "new", options);
			var resultString = result?.ToJsonString();
			Assert.Contains("\"LEAF\":\"new\"", resultString);
			Assert.Contains("ROOT", resultString);
			Assert.Contains("BRANCH", resultString);
		}
	}
}