using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using Xunit;

namespace Albatross.Json.Test {
	public class Extensions_SetValue_InitOnly_CaseSensitive {
		private static readonly JsonSerializerOptions options = new() {
			PropertyNameCaseInsensitive = false,
		};

		public static IEnumerable<object[]> NonNullObjectPropertyData => new List<object[]> {
			new object[] { """{"name":"existing"}""", new[] { "name" }, "new-value", """{"name":"existing"}""" },
			new object[] { """{"count":42}""", new[] { "count" }, 99, """{"count":42}""" },
			new object[] { """{"enabled":true}""", new[] { "enabled" }, false, """{"enabled":true}""" },
			new object[] { """{"parent":{"child":"old"}}""", new[] { "parent", "child" }, "new", """{"parent":{"child":"old"}}""" },
		};

		[Theory]
		[MemberData(nameof(NonNullObjectPropertyData))]
		public void ExistingNonNullObjectProperty_SkipsWrite(string json, string[] path, object value, string expected) {
			var node = JsonNode.Parse(json);
			var result = Extensions.SetValue(node, path, value, options, initOnly: true);
			Assert.Equal(expected, result?.ToJsonString(options));
		}

		public static IEnumerable<object[]> MissingOrNullObjectPropertyData => new List<object[]> {
			new object[] { "{}", new[] { "name" }, "test", """{"name":"test"}""" },
			new object[] { """{"name":null}""", new[] { "name" }, "new-value", """{"name":"new-value"}""" },
			new object[] { "{}", new[] { "a", "b" }, "value", """{"a":{"b":"value"}}""" },
			new object[] { """{"parent":{}}""", new[] { "parent", "child" }, "value", """{"parent":{"child":"value"}}""" },
		};

		[Theory]
		[MemberData(nameof(MissingOrNullObjectPropertyData))]
		public void MissingOrNullObjectProperty_WritesValue(string json, string[] path, object value, string expected) {
			var node = JsonNode.Parse(json);
			var result = Extensions.SetValue(node, path, value, options, initOnly: true);
			var expectedFormatted = JsonNode.Parse(expected)?.ToJsonString(options);
			Assert.Equal(expectedFormatted, result?.ToJsonString(options));
		}

		[Fact]
		public void ArrayElement_NonNullExisting_SkipsWrite() {
			var node = JsonNode.Parse("""{"arr":[1,2,3]}""");
			var result = Extensions.SetValue(node, ["arr", "1"], 99, options, initOnly: true);
			Assert.Equal("""{"arr":[1,2,3]}""", result?.ToJsonString(options));
		}

		[Fact]
		public void ArrayElement_NullExisting_WritesValue() {
			var node = JsonNode.Parse("""{"arr":[null,2,3]}""");
			var result = Extensions.SetValue(node, ["arr", "0"], 99, options, initOnly: true);
			Assert.Equal("""{"arr":[99,2,3]}""", result?.ToJsonString(options));
		}

		[Fact]
		public void ArrayElement_OutOfBounds_AppendsValue() {
			var node = JsonNode.Parse("""{"arr":[1,2,3]}""");
			var result = Extensions.SetValue(node, ["arr", "10"], 99, options, initOnly: true);
			Assert.Equal("""{"arr":[1,2,3,99]}""", result?.ToJsonString(options));
		}

		[Fact]
		public void ExistingValueDifferentCase_WritesNewProperty() {
			var node = JsonNode.Parse("""{"Name":"existing"}""");
			var result = Extensions.SetValue(node, ["name"], "new-value", options, initOnly: true);
			var resultString = result?.ToJsonString(options);
			Assert.Contains("\"Name\":\"existing\"", resultString);
			Assert.Contains("\"name\":\"new-value\"", resultString);
		}
	}
}
