## CsProjEditor

This library offers csproj element/attribute operation without EnvDTE.
Currently in development.

## What will do

Load csproj file and create virtual dom, operate with it and write it out.

## TODO

Implementation

* [x] Detect: EOL
* [x] Detect: UTF8 BOM
* [x] Detect: Space for each node
* [x] Detect: NameSpace (for old csproj, equivalent for new csproj)
* [x] Detect: Declaration (for old csproj, equivalent for new csproj)
* [x] Node: Insert
* [ ] Node: Replace
* [x] Node: Remove
* [x] Node: Value Set
* [x] Node: Value Prepend
* [x] Node: Value Apend
* [x] Node: Value Replace
* [x] Node: Value Remove
* [x] Attribute: Insert
* [ ] Attribute: Remove
* [ ] Attribute: Replace
* [x] Load: load csproj as XPath
* [x] Write: write with original utf8 encoding, keep BOM.
* [x] Write: add empty line if last line is value.

Test for following

* [ ] eolstring test
* [ ] output test
* [ ] GetUtf8String test
* [ ] GetFileEncoding test
* [ ] declaration test - old csproj (contains) / new csproj (missing)
* [ ] Save test
* [ ] Replace test
* [ ] Insert test
* [ ] InsertAttribute test
* [ ] GetIntentSpace test

Test: Prepare mock data - old csproj

* [ ] utf8 crlf (unity gen)
* [ ] utf8 lf
* [ ] utf8bom crlf
* [ ] utf8bom lf

Test: Prepare mock data - new csproj

* [ ] utf8 crlf
* [ ] utf8 lf
* [ ] utf8bom crlf
* [ ] utf8bom lf
