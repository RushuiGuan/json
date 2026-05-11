using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using Xunit;

namespace Albatross.Json.Test {
	public class Extensions_SetValue_CaseInsensitive {
		private static readonly JsonSerializerOptions options = new JsonSerializerOptions {
			PropertyNameCaseInsensitive = true,
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		};

		public static IEnumerable<object[]> SetValueTestData => new List<object[]> {
			new object[] { "null", new string[0], 1, "1" },
			new object[] { "{}", new string[0], 1, "1" },
			new object[] { "null", new[] { "a" }, 1, """{"a": 1}""" },
			new object[] { "{}", new[] { "a" }, 1, """{"a": 1}""" },
			new object[] { "null", new string[0], new Data("x"), """{"name": "x"}""" },
			new object[] { "{}", new string[0], new Data("x"), """{"name": "x"}""" },
			new object[] { "[1,2,3]", new[] { "a" }, 1, """{"a": 1}""" },
			new object[] { """{"a":1}""", new[] { "a" }, 2, """{"a": 2}""" },
			new object[] { """{"a":1}""", new[] { "A" }, 2, """{"A": 2}""" },
			new object[] { """{"a":1}""", new[] { "a" }, null, """{"a": null}""" },
			new object[] { """{"a":1}""", new[] { "a" }, new[] { 1, 2, 3 }, """{"a": [1, 2, 3]}""" },
			new object[] { """{"Parent":{"Child":"old"}}""", new[] { "parent", "child" }, "new", """{"parent":{"child":"new"}}""" },
			new object[] { """{"Parent":{"Child":"old"}}""", new[] { "parent", "child" }, "old", """{"parent":{"child":"old"}}""" },
			new object[] { "[1,2,3]", new[] { "0" }, 1, "[1, 2, 3]" },
			new object[] { "[1,2,3]", new[] { "0" }, 2, "[2, 2, 3]" },
			new object[] { "[1,2,3]", new[] { "100" }, 4, "[1, 2, 3, 4]" },
			new object[] { "[1,2,3]", new[] { "3" }, 4, "[1, 2, 3, 4]" },
			new object[] { """{"Users":[{"Id":1},{"Id":2}]}""", new[] { "users", "0", "name" }, "Alice", """{"users":[{"Id":1,"name":"Alice"},{"Id":2}]}""" },
			new object[] { """{"Users":[{"Id":1},{"Id":2}]}""", new[] { "users", "name" }, "Alice", """{"users":{"name":"Alice"} }""" },
			new object[] { """{"Users":[{"Name":"Bob"}]}""", new[] { "users", "0", "name" }, "Alice", """{"users":[{"name":"Alice"}]}""" },
			new object[] { """{"Level1":{"Level2":{"Level3":"old"}}}""", new[] { "level1", "LEVEL2", "Level3" }, "new", """{"level1":{"LEVEL2":{"Level3":"new"}}}""" },
			new object[] { """{"Users":[{"Id":1},{"Id":2}]}""", new[] { "users", "2", "name" }, "Alice", """{"users":[{"Id":1},{"Id":2}, {"name": "Alice"}]}""" },
			new object[] { """{"Users":[{"Id":1},{"Id":2}]}""", new[] { "users", "100", "name" }, "Alice", """{"users":[{"Id":1},{"Id":2}, {"name": "Alice"}]}""" },
		};

		[Theory]
		[MemberData(nameof(SetValueTestData))]
		public void TestSetValue(string json, string[] path, object? value, string expected) {
			var node = JsonNode.Parse(json);
			var result = Extensions.SetValue(node, path, value, options);
			var expectedFormatted = JsonNode.Parse(expected)?.ToJsonString(options);
			Assert.Equal(expectedFormatted, result?.ToJsonString(options));
		}

		[Theory]
		[InlineData("[1,2,3]", 4, "-1")]
		public void TestSetValueException(string json, object? value, params string[] path) {
			Assert.Throws<ArgumentException>(() => {
				var node = JsonNode.Parse(json);
				var result = Extensions.SetValue(node, path, value, options);
			});
		}
	}
}
