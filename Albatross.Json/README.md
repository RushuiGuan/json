# Albatross.Json

Extension methods for `System.Text.Json` and `JsonNode` objects.

## Installation

```shell
dotnet add package Albatross.Json
```

## Features

- **SetValue** - Set values at any path within a JSON structure
- **Case-Sensitive/Insensitive** - Control property matching via `JsonSerializerOptions`
- **Auto Path Creation** - Intermediate objects created automatically
- **Array Support** - Navigate arrays using numeric string indices

## SetValue Method

```csharp
public static JsonNode? SetValue<T>(this JsonNode? node, string[] path, T? value, JsonSerializerOptions options)
```

## Case Sensitivity

**Case-Insensitive** - Matches properties regardless of case, preserves original casing:
```csharp
var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
var node = JsonNode.Parse("""{"UserName":"old"}""");
node = node.SetValue(["username"], "new", options);
// Result: {"UserName":"new"}
```

**Case-Sensitive** (default) - Different casing creates separate properties:
```csharp
var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = false };
var node = JsonNode.Parse("""{"Name":"old"}""");
node = node.SetValue(["name"], "new", options);
// Result: {"Name":"old","name":"new"}
```

## Examples

```csharp
using Albatross.Json;
using System.Text.Json;
using System.Text.Json.Nodes;

var options = new JsonSerializerOptions();

// Set nested property
var node = JsonNode.Parse("{}");
node = node.SetValue(["level1", "level2"], "value", options);
// Result: {"level1":{"level2":"value"}}

// Update array element
node = JsonNode.Parse("""{"arr":[1,2,3]}""");
node = node.SetValue(["arr", "1"], 99, options);
// Result: {"arr":[1,99,3]}

// Set property inside array element
node = JsonNode.Parse("""{"items":[{"id":1}]}""");
node = node.SetValue(["items", "0", "name"], "first", options);
// Result: {"items":[{"id":1,"name":"first"}]}
```

## Documentation

Full documentation: https://rushuiguan.github.io/json/
