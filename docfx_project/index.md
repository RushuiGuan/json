---
_layout: landing
---

# Albatross.Json

A .NET library providing extension methods for `System.Text.Json` and `JsonNode` objects.

## Features

- **SetValue Extension Method** - Set values at any path within a JSON structure
- **Case-Sensitive and Case-Insensitive Support** - Control property name matching behavior via `JsonSerializerOptions`
- **Automatic Path Creation** - Intermediate objects are created automatically when navigating non-existent paths
- **Array Support** - Navigate and modify array elements using numeric string indices

## Installation

```bash
dotnet add package Albatross.Json
```

## SetValue Extension Method

Sets a value at the specified path within a JSON node structure.

```csharp
public static JsonNode? SetValue<T>(this JsonNode? node, string[] path, T? value, JsonSerializerOptions options)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| `node` | The root JSON node. If null, a new JsonObject is created. |
| `path` | The path segments to navigate. Use numeric strings for array indices. |
| `value` | The value to set at the specified path. |
| `options` | JsonSerializerOptions controlling serialization and case sensitivity. |

### Behavior

- If path is empty, returns the serialized value directly (replaces entire node)
- Creates intermediate JsonObjects automatically when navigating non-existent paths
- Overwrites existing value nodes (JsonValue) with JsonObjects when the path continues through them
- For arrays, path segments must be valid integer indices within bounds
- Throws `ArgumentException` when an array index is out of bounds or not a valid integer

## Case Sensitivity

The `PropertyNameCaseInsensitive` option in `JsonSerializerOptions` controls property name matching:

### Case-Insensitive Mode

When `PropertyNameCaseInsensitive = true`, property lookups ignore case and preserve original property names:

```csharp
var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
var node = JsonNode.Parse("""{"UserName":"old"}""");

node = node.SetValue(["username"], "new", options);
// Result: {"UserName":"new"}
// Original "UserName" casing is preserved
```

### Case-Sensitive Mode

When `PropertyNameCaseInsensitive = false` (default), different casing creates separate properties:

```csharp
var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = false };
var node = JsonNode.Parse("""{"Name":"old"}""");

node = node.SetValue(["name"], "new", options);
// Result: {"Name":"old","name":"new"}
// Both properties coexist
```

## Examples

### Set Simple Property

```csharp
var node = JsonNode.Parse("{}");
node = node.SetValue(["name"], "test", options);
// Result: {"name":"test"}
```

### Set Nested Property

```csharp
var node = JsonNode.Parse("{}");
node = node.SetValue(["level1", "level2"], "value", options);
// Result: {"level1":{"level2":"value"}}
```

### Update Array Element

```csharp
var node = JsonNode.Parse("""{"arr":[1,2,3]}""");
node = node.SetValue(["arr", "1"], 99, options);
// Result: {"arr":[1,99,3]}
```

### Set Property Inside Array Element

```csharp
var node = JsonNode.Parse("""{"items":[{"id":1},{"id":2}]}""");
node = node.SetValue(["items", "0", "name"], "first", options);
// Result: {"items":[{"id":1,"name":"first"},{"id":2}]}
```

### Replace Entire Node

```csharp
var node = JsonNode.Parse("""{"old":"data"}""");
node = node.SetValue(Array.Empty<string>(), "replaced", options);
// Result: "replaced"
```
