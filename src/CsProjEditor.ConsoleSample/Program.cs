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
            csproj.Replace("PropertyGroup", "PackageCertificateKeyFile", pfx);
            csproj.Insert("PropertyGroup", "PackageCertificateThumbprint", thumbprint);
            csproj.Insert("PropertyGroup", "GenerateAppInstallerFile", "False");
            csproj.Insert("PropertyGroup", "AppxAutoIncrementPackageRevision", "True");
            csproj.Insert("PropertyGroup", "AppxSymbolPackageEnabled", "False");
            csproj.Insert("PropertyGroup", "AppxBundle", "Always");
            csproj.Insert("PropertyGroup", "AppxBundlePlatforms", "x86");
            csproj.Insert("PropertyGroup", "AppInstallerUpdateFrequency", "1");
            csproj.Insert("PropertyGroup", "AppInstallerCheckForUpdateFrequency", "OnApplicationRun");
            csproj.InsertAttribute("ItemGroup", "None", "Include", pfx);
            csproj.InsertAttribute("ItemGroup", "None", "Include", "Package.StoreAssociation.xml");

            // save
            csproj.Save(path);
        }
    }
}
