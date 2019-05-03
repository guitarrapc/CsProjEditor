## CsProjEditor

This library offers csproj element/attribute operation without EnvDTE.
Currently in development.

## What will do

Load csproj file and create virtual dom, operate with it and write it out.

## Limitation

* Accept utf8 and utf8bom encodings only.
* Accept file path or stream only. (no parsing string.)

## Implementations

* [x] File: Load (csproj load entrypoint)
* [x] File: ToString override.
* [x] File: Detect UTF8 Bom.
* [x] File: Detect EOL.
* [x] File: Save with original utf8 encoding, keep BOM.
* [x] File: Save will add empty line if last line is value.
* [x] Utils: Get NameSpace (for old csproj, equivalent for new csproj)
* [x] Utils: Get Declaration (for old csproj, equivalent for new csproj)
* [x] Utils: Get space for each node
* [x] Filter: filter node, nodevalue and attribute and pass as XElement
* [x] Node: Get
* [x] Node: Exists
* [x] Node: Insert
* [x] Node: Remove
* [x] Node: Replace
* [x] NodeValue: Get
* [x] NodeValue: Exists
* [x] NodeValue: Set
* [x] NodeValue: Append
* [x] NodeValue: Prepend
* [x] NodeValue: Remove
* [x] NodeValue: Replace
* [x] Attribute: Get
* [x] Attribute: Exists
* [x] Attribute: Insert
* [x] Attribute: Set
* [x] Attribute: Remove
* [x] Attribute: Replace
* [x] AttributeValue: Get
* [x] AttributeValue: Exists
* [x] AttributeValue: Set
* [x] AttributeValue: Append
* [x] AttributeValue: Prepend
* [x] AttributeValue: Remove
* [x] AttributeValue: Replace
* [x] Tests: Node
* [x] Tests: NodeValue
* [x] Tests: Attribute
* [x] Tests: AttributeValue
* [x] Tests: Save
