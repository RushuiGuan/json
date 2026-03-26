using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using Xunit;

namespace Albatross.Json.Test {
	public class TestJsonEditor {
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
			var result = Extensions.SetValue(node, path, value);
			var expectedFormatted = JsonNode.Parse(expected)?.ToJsonString(Extensions.Options);
			Assert.Equal(expectedFormatted, result?.ToJsonString(Extensions.Options));
		}
		[Fact]
		public void TestSetValue_InvalidArrayIndex_ThrowsException() {
			var json = """{"arr":[1,2,3]}""";
			var node = JsonNode.Parse(json);
			Assert.Throws<ArgumentException>(() => Extensions.SetValue(node, ["arr", "10"], "value"));
		}
		[Fact]
		public void TestSetValue_InvalidArrayIndexFormat_ThrowsException() {
			var json = """{"arr":[1,2,3]}""";
			var node = JsonNode.Parse(json);
			Assert.Throws<ArgumentException>(() => Extensions.SetValue(node, ["arr", "invalid"], "value"));
		}
	}
}
