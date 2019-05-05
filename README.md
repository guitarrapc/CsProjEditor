[![Build Status](https://cloud.drone.io/api/badges/guitarrapc/CsProjEditor/status.svg)](https://cloud.drone.io/guitarrapc/CsProjEditor) [![codecov](https://codecov.io/gh/guitarrapc/CsProjEditor/branch/master/graph/badge.svg)](https://codecov.io/gh/guitarrapc/CsProjEditor)

## CsProjEditor

This library offers csproj element/attribute operation without EnvDTE.
Currently in development.

## What will do

Load csproj file and create virtual dom, operate with it and write it out.

## How to use

```chsarp
// load csproj
var csproj = CsprojEditor.Load("your.csproj");

// edit
csproj.SetNodeValue("PropertyGroup", "PackageCertificateKeyFile", pfx);
csproj.InsertNode("PropertyGroup", "PackageCertificateThumbprint", thumbprint);
csproj.InsertNode("PropertyGroup", "GenerateAppInstallerFile", "False");
csproj.InsertNode("PropertyGroup", "AppxAutoIncrementPackageRevision", "True");
csproj.InsertNode("PropertyGroup", "AppxSymbolPackageEnabled", "False");
csproj.InsertNode("PropertyGroup", "AppxBundle", "Always");
csproj.InsertNode("PropertyGroup", "AppxBundlePlatforms", "x86");
csproj.InsertNode("PropertyGroup", "AppInstallerUpdateFrequency", "1");
csproj.InsertNode("PropertyGroup", "AppInstallerCheckForUpdateFrequency", "OnApplicationRun");
csproj.InsertAttribute("ItemGroup", "None", "Include", pfx, e => !e.HasAttributes);
csproj.InsertAttribute("ItemGroup", "None", "Include", "Package.StoreAssociation.xml", e => !e.HasAttributes);

// save
csproj.Save(path);
```

## Limitation

* Accept utf8 and utf8bom encodings only.
* Accept file path or stream only. (no parsing string.)

## Implementations

* [x] File: Load
* [x] File: ToString override.
* [x] File: Detect UTF8 Bom.
* [x] File: Detect EOL.
* [x] File: Save with original utf8 encoding, keep BOM.
* [x] File: Save will add empty line if last line is value.
* [x] XmlUtils: Get NameSpace (for old csproj, equivalent for new csproj)
* [x] XmlUtils: Get Declaration (for old csproj, equivalent for new csproj)
* [x] XmlUtils: Get space for each node
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
