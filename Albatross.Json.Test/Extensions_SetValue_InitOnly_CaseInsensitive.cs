using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using Xunit;

namespace Albatross.Json.Test {
	public class Extensions_SetValue_InitOnly_CaseInsensitive {
		private static readonly JsonSerializerOptions options = new() {
			PropertyNameCaseInsensitive = true,
		};

		public static IEnumerable<object[]> NonNullObjectPropertyData => new List<object[]> {
			// Exact key match
			new object[] { """{"name":"existing"}""", new[] { "name" }, "new-value", """{"name":"existing"}""" },
			// Different-case key match
			new object[] { """{"Name":"existing"}""", new[] { "name" }, "new-value", """{"Name":"existing"}""" },
			new object[] { """{"NAME":"existing"}""", new[] { "name" }, "new-value", """{"NAME":"existing"}""" },
			// Nested path: intermediate key "Parent" is renamed to "parent" during traversal,
			// but the final write to "child" is skipped because "Child" matches and is non-null.
			new object[] { """{"Parent":{"Child":"old"}}""", new[] { "parent", "child" }, "new", """{"parent":{"Child":"old"}}""" },

		};

		[Theory]
		[MemberData(nameof(NonNullObjectPropertyData))]
		public void ExistingNonNullObjectProperty_SkipsWrite(string json, string[] path, object value, string expected) {
			var node = JsonNode.Parse(json);
			var result = Extensions.SetValue(node, path, value, options, initOnly: true);
			Assert.Equal(expected, result?.ToJsonString(options));
		}

		public static IEnumerable<object[]> MissingOrNullObjectPropertyData => new List<object[]> {
			// No key exists
			new object[] { "{}", new[] { "name" }, "test", """{"name":"test"}""" },
			// Key exists with null value (exact case)
			new object[] { """{"name":null}""", new[] { "name" }, "new-value", """{"name":"new-value"}""" },
			// Key exists with null value (different case)
			new object[] { """{"Name":null}""", new[] { "name" }, "new-value", """{"name":"new-value"}""" },
			// Nested path, target missing: intermediate "Parent" is renamed to "parent" during traversal.
			new object[] { """{"Parent":{}}""", new[] { "parent", "child" }, "value", """{"parent":{"child":"value"}}""" },
		};

		[Theory]
		[MemberData(nameof(MissingOrNullObjectPropertyData))]
		public void MissingOrNullObjectProperty_WritesValue(string json, string[] path, object value, string expected) {
			var node = JsonNode.Parse(json);
			var result = Extensions.SetValue(node, path, value, options, initOnly: true);
			var expectedFormatted = JsonNode.Parse(expected)?.ToJsonString(options);
			Assert.Equal(expectedFormatted, result?.ToJsonString(options));
		}
	}
}
