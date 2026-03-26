# Release Notes

## v1.0.0

Initial release of Albatross.Json library.

### Features

#### SetValue&lt;T&gt; Method
A generic method for setting values at specified paths within JSON node structures using `System.Text.Json.Nodes`.

```csharp
JsonNode? SetValue<T>(JsonNode? node, string[] path, T? value)
```

**Capabilities:**
- Set or update values at any depth using path segments
- Automatically creates intermediate `JsonObject` nodes for non-existent paths
- Supports array element access using numeric string indices (e.g., `"0"`, `"1"`)
- Overwrites existing value nodes when the path continues through them
- Handles null input nodes by creating a new `JsonObject`
- Returns the serialized value directly when path is empty (replaces entire node)

**Examples:**
```csharp
// Set nested property
var node = JsonNode.Parse("{}");
Extensions.SetValue(node, ["level1", "level2"], "value");
// Result: {"level1":{"level2":"value"}}

// Update array element
var node = JsonNode.Parse("""{"arr":[1,2,3]}""");
Extensions.SetValue(node, ["arr", "1"], 99);
// Result: {"arr":[1,99,3]}

// Overwrite value node with object
var node = JsonNode.Parse("""{"name":"string"}""");
Extensions.SetValue(node, ["name", "nested"], "value");
// Result: {"name":{"nested":"value"}}
```

### Requirements
- .NET 10.0

