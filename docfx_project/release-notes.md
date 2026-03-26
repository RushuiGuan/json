# Release Notes

Albatross.Json provides extension methods for `System.Text.Json` to simplify path-based JSON manipulation with support for case-sensitive and case-insensitive property matching.

## v1.0.0

- `SetValue<T>` extension method for setting values at paths within JSON node structures
- Case-sensitive and case-insensitive property matching via `JsonSerializerOptions.PropertyNameCaseInsensitive`
- Automatic intermediate object creation for non-existent paths
- Array element access via numeric string indices
