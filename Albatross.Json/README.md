# Albatross.Json

Path-based JSON value manipulation for System.Text.Json.

## Installation
```shell
dotnet add package Albatross.Json
```

## Usage

### SetValue
Set or update values at any path within a JSON structure.

```csharp
using Albatross.Json;
using System.Text.Json.Nodes;

// Set nested property (creates intermediate objects automatically)
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

## Requirements
- .NET 10.0