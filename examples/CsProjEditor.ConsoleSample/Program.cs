using System;

namespace CsProjEditor.ConsoleSample
{
    // make UWP Store associated csproj.
    // MEMO: APP pfx and Package.StoreAssociation.xml should prepare/exists in advanced. (with manual relation on Visual Studio)
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 3) throw new ArgumentOutOfRangeException($@"3 arguments must pass to console.
0: PATH\TO\test.csproj 
1: MY_UWP_STORE.pfx
2: PFX_THUMBNAIL
");

            var path = args[0];
            var pfx = args[1];
            var thumbprint = args[2];

            // prepare
            var csproj = CsProjEditor.Load(path);

            // edit
            csproj.ReplaceValue("PropertyGroup", "PackageCertificateKeyFile", pfx);
            csproj.InsertNode("PropertyGroup", "PackageCertificateThumbprint", thumbprint);
            csproj.InsertNode("PropertyGroup", "GenerateAppInstallerFile", "False");
            csproj.InsertNode("PropertyGroup", "AppxAutoIncrementPackageRevision", "True");
            csproj.InsertNode("PropertyGroup", "AppxSymbolPackageEnabled", "False");
            csproj.InsertNode("PropertyGroup", "AppxBundle", "Always");
            csproj.InsertNode("PropertyGroup", "AppxBundlePlatforms", "x86");
            csproj.InsertNode("PropertyGroup", "AppInstallerUpdateFrequency", "1");
            csproj.InsertNode("PropertyGroup", "AppInstallerCheckForUpdateFrequency", "OnApplicationRun");
            csproj.InsertAttribute("ItemGroup", "None", "Include", pfx);
            csproj.InsertAttribute("ItemGroup", "None", "Include", "Package.StoreAssociation.xml");

            // save
            csproj.Save(path);
        }
    }
}
