csprojcli NodeValue.Set -p SimpleNewCsProjUtf8_CRLF.csproj -g PropertyGroup -n PackageCertificateKeyFile -v hogehoge.pfx -dry false -output result_SimpleNewCsProjUtf8_CRLF.csproj -allowoverwrite true
csprojcli Node.Insert -p result_SimpleNewCsProjUtf8_CRLF.csproj -g PropertyGroup -n PackageCertificateThumbprint -v 1234567890ABCDEF -dry false -output result_SimpleNewCsProjUtf8_CRLF.csproj -allowoverwrite true
csprojcli Attribute.Insert -p result_SimpleNewCsProjUtf8_CRLF.csproj -g ItemGroup -n None -a Include -v hogehoge.pfx -dry false -output result_SimpleNewCsProjUtf8_CRLF.csproj -allowoverwrite true
csprojcli Group.Insert -p result_SimpleNewCsProjUtf8_CRLF.csproj -g TestGroup -dry false -output result_SimpleNewCsProjUtf8_CRLF.csproj -allowoverwrite true
csprojcli Node.Insert -p result_SimpleNewCsProjUtf8_CRLF.csproj -g TestGroup -n Foo -v Bar -dry false -output result_SimpleNewCsProjUtf8_CRLF.csproj -allowoverwrite true
