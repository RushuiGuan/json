using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using Xunit;

namespace Albatross.Json.Test {
	public class JsonEditorCaseSensitiveTests {
		private static readonly JsonSerializerOptions options = new JsonSerializerOptions {
			PropertyNameCaseInsensitive = false,
		};
		public static IEnumerable<object[]> SetValueTestData => new List<object[]> {
			// Set simple property on object
			new object[] { "{}", new[] { "name" }, "test", """{"name":"test"}""" },
			// Update existing property
			new object[] { """{"name":"old"}""", new[] { "name" }, "new", """{"name":"new"}""" },
			// Set nested property (creates intermediate object)
			new object[] { "{}", new[] { "level1", "level2" }, "value", """{"level1":{"level2":"value"}}""" },
			// Set deeply nested property
			new object[] { "{}", new[] { "a", "b", "c" }, 123, """{"a":{"b":{"c":123}}}""" },
			// Set property on existing nested object
			new object[] { """{"parent":{}}""", new[] { "parent", "child" }, true, """{"parent":{"child":true}}""" },
			// Set numeric value
			new object[] { "{}", new[] { "count" }, 42, """{"count":42}""" },
			// Set boolean value
			new object[] { "{}", new[] { "enabled" }, false, """{"enabled":false}""" },
			// Set null value
			new object[] { """{"key":"value"}""", new[] { "key" }, null!, """{"key":null}""" },
			// Set array value
			new object[] { "{}", new[] { "items" }, new[] { 1, 2, 3 }, """{"items":[1,2,3]}""" },
			// Set object value
			new object[] { "{}", new[] { "config" }, new { host = "localhost", port = 8080 }, """{"config":{"host":"localhost","port":8080}}""" },
			// Update array element
			new object[] { """{"arr":[1,2,3]}""", new[] { "arr", "1" }, 99, """{"arr":[1,99,3]}""" },
			// Set property inside array element
			new object[] { """{"items":[{"id":1},{"id":2}]}""", new[] { "items", "0", "name" }, "first", """{"items":[{"id":1,"name":"first"},{"id":2}]}""" },
			// Empty path replaces entire value
			new object[] { """{"old":"data"}""", Array.Empty<string>(), "replaced", "\"replaced\"" },
			// Empty path with object value
			new object[] { """{"old":"data"}""", Array.Empty<string>(), new { newKey = "newValue" }, """{"newKey":"newValue"}""" },
			// overwrite value node with object
			new object[] { """{"name":"string"}""", new[] { "name", "nested" }, "value", """{"name":{"nested":"value"}}""" },
			new object[] { """{"root":"string"}""", new[] { "root", "key" }, "newValue", """{"root":{"key":"newValue"}}""" }
		};

		[Theory]
		[MemberData(nameof(SetValueTestData))]
		public void TestSetValue(string json, string[] path, object value, string expected) {
			var node = JsonNode.Parse(json);
			var result = Extensions.SetValue(node, path, value, options);
			var expectedFormatted = JsonNode.Parse(expected)?.ToJsonString(options);
			Assert.Equal(expectedFormatted, result?.ToJsonString(options));
		}
		[Fact]
		public void TestSetValue_InvalidArrayIndex_ThrowsException() {
			var json = """{"arr":[1,2,3]}""";
			var node = JsonNode.Parse(json);
			Assert.Throws<ArgumentException>(() => Extensions.SetValue(node, ["arr", "10"], "value", options));
		}

		[Fact]
		public void TestSetValue_InvalidArrayIndexFormat_ThrowsException() {
			var json = """{"arr":[1,2,3]}""";
			var node = JsonNode.Parse(json);
			Assert.Throws<ArgumentException>(() => Extensions.SetValue(node, ["arr", "invalid"], "value", options));
		}

		// Case-sensitive specific tests: different casing creates new properties

		[Fact]
		public void TestSetValue_CaseSensitive_DifferentCaseCreatesNewProperty() {
			var json = """{"Name":"old"}""";
			var node = JsonNode.Parse(json);
			var result = Extensions.SetValue(node, ["name"], "new", options);
			var resultString = result?.ToJsonString();
			// Both properties should exist
			Assert.Contains("\"Name\":\"old\"", resultString);
			Assert.Contains("\"name\":\"new\"", resultString);
		}

		[Fact]
		public void TestSetValue_CaseSensitive_UppercasePathCreatesNewProperty() {
			var json = """{"name":"old"}""";
			var node = JsonNode.Parse(json);
			var result = Extensions.SetValue(node, ["NAME"], "new", options);
			var resultString = result?.ToJsonString();
			// Both properties should exist
			Assert.Contains("\"name\":\"old\"", resultString);
			Assert.Contains("\"NAME\":\"new\"", resultString);
		}

		[Fact]
		public void TestSetValue_CaseSensitive_NestedDifferentCaseCreatesNewPath() {
			var json = """{"Parent":{"Child":"old"}}""";
			var node = JsonNode.Parse(json);
			var result = Extensions.SetValue(node, ["parent", "child"], "new", options);
			var resultString = result?.ToJsonString();
			// Original nested structure preserved, new structure created
			Assert.Contains("\"Parent\":{\"Child\":\"old\"}", resultString);
			Assert.Contains("\"parent\":{\"child\":\"new\"}", resultString);
		}

		[Fact]
		public void TestSetValue_CaseSensitive_MixedCaseNestedCreatesNewPath() {
			var json = """{"Config":{"Host":"localhost"}}""";
			var node = JsonNode.Parse(json);
			var result = Extensions.SetValue(node, ["config", "host"], "127.0.0.1", options);
			var resultString = result?.ToJsonString();
			// Original preserved, new path created
			Assert.Contains("\"Config\":{\"Host\":\"localhost\"}", resultString);
			Assert.Contains("\"config\":{\"host\":\"127.0.0.1\"}", resultString);
		}

		[Fact]
		public void TestSetValue_CaseSensitive_ArrayParentDifferentCaseCreatesNewProperty() {
			var json = """{"Items":[1,2,3]}""";
			var node = JsonNode.Parse(json);
			var result = Extensions.SetValue(node, ["items", "1"], 99, options);
			var resultString = result?.ToJsonString();
			// Original array preserved, new object created with numeric key
			Assert.Contains("\"Items\":[1,2,3]", resultString);
			Assert.Contains("\"items\":{\"1\":99}", resultString);
		}

		[Fact]
		public void TestSetValue_CaseSensitive_MultiplePropertiesWithDifferentCasing() {
			var json = """{"Name":"John"}""";
			var node = JsonNode.Parse(json);
			var result = Extensions.SetValue(node, ["name"], "Jane", options);
			result = Extensions.SetValue(result, ["NAME"], "Bob", options);
			result = Extensions.SetValue(result, ["nAmE"], "Alice", options);
			var resultString = result?.ToJsonString();
			// All four variations should exist
			Assert.Contains("\"Name\":\"John\"", resultString);
			Assert.Contains("\"name\":\"Jane\"", resultString);
			Assert.Contains("\"NAME\":\"Bob\"", resultString);
			Assert.Contains("\"nAmE\":\"Alice\"", resultString);
		}

		[Fact]
		public void TestSetValue_CaseSensitive_ExactMatchUpdatesProperty() {
			var json = """{"UserName":"old"}""";
			var node = JsonNode.Parse(json);
			var result = Extensions.SetValue(node, ["UserName"], "new", options);
			var resultString = result?.ToJsonString();
			// Only one property should exist with updated value
			Assert.Equal("""{"UserName":"new"}""", resultString);
		}

		[Fact]
		public void TestSetValue_CaseSensitive_DeeplyNestedDifferentCaseCreatesNewPath() {
			var json = """{"Level1":{"Level2":{"Level3":"old"}}}""";
			var node = JsonNode.Parse(json);
			var result = Extensions.SetValue(node, ["level1", "level2", "level3"], "new", options);
			var resultString = result?.ToJsonString();
			// Original path preserved
			Assert.Contains("\"Level1\":{\"Level2\":{\"Level3\":\"old\"}}", resultString);
			// New path created
			Assert.Contains("\"level1\":{\"level2\":{\"level3\":\"new\"}}", resultString);
		}

		[Fact]
		public void TestSetValue_CaseSensitive_PartialMatchCreatesNewBranch() {
			var json = """{"Parent":{"Child":"old"}}""";
			var node = JsonNode.Parse(json);
			// Same parent (exact match), different child casing
			var result = Extensions.SetValue(node, ["Parent", "child"], "new", options);
			var resultString = result?.ToJsonString();
			// Original child preserved, new child added
			Assert.Contains("\"Child\":\"old\"", resultString);
			Assert.Contains("\"child\":\"new\"", resultString);
		}

		[Fact]
		public void TestSetValue_CaseSensitive_ArrayElementPropertyDifferentCase() {
			var json = """{"users":[{"Name":"Bob"}]}""";
			var node = JsonNode.Parse(json);
			var result = Extensions.SetValue(node, ["users", "0", "name"], "Alice", options);
			var resultString = result?.ToJsonString();
			// Original Name preserved, new name added
			Assert.Contains("\"Name\":\"Bob\"", resultString);
			Assert.Contains("\"name\":\"Alice\"", resultString);
		}
	}
}
