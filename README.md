## CsProjEditor

This library offers csproj element/attribute operation without EnvDTE.
Currently in development.

## What will do

Load csproj file and create virtual dom, operate with it and write it out.

## TODO

* [x] Detect: EOL
* [x] Detect: UTF8 BOM
* [x] Detect: Space for each element
* [x] Detect: NameSpace (for old csproj, equivalent for new csproj)
* [x] Detect: Declaration (for old csproj, equivalent for new csproj)
* [x] Element: Insert
* [ ] Element: Remove
* [x] Element: Value Replace
* [x] Attribute: Insert
* [ ] Attribute: Remove
* [ ] Attribute: Value Replace
* [x] Load: load csproj as XPath
* [x] Write: write with original utf8 encoding, keep BOM.
* [x] Write: add empty line if last line is value.
