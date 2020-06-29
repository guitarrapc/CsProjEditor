using ConsoleAppFramework;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Threading.Tasks;
using CsProjEditor;

namespace UwpCsProjEdior
{
    // make UWP Store associated csproj.
    // MEMO: APP pfx and Package.StoreAssociation.xml should prepare/exists in advanced. (with manual relation on Visual Studio)
    class Program
    {
        static async Task Main(string[] args)
            => await Host.CreateDefaultBuilder()
                .RunConsoleAppFrameworkAsync<CsProjEdior>(args);
    }

    public class CsProjEdior : ConsoleAppBase
    {
        [Command("store-publish", "modify csproj as uwp store publish modify.")]
        public void Run(string path, string pfx, string thumbnail, string output, bool allowoverwrite = false)
        {
            Console.WriteLine($"command: 'uwp-store-publish'. Parameters are as follows.");
            Console.WriteLine($"{nameof(path)}: {path}");
            Console.WriteLine($"{nameof(pfx)}: {pfx}");
            Console.WriteLine($"{nameof(thumbnail)}: {thumbnail}");
            Console.WriteLine($"{nameof(output)}: {output}");
            Console.WriteLine($"{nameof(allowoverwrite)}: {allowoverwrite}");
            Console.WriteLine("\n");

            // run
            Console.WriteLine($"begin editing csproj.");
            var csproj = Project.Load(path);
            Modify(csproj, pfx, thumbnail);

            // save
            Console.WriteLine($"saving generated csproj to {output}");
            if (File.Exists(output) && !allowoverwrite)
            {
                throw new IOException($"Output path {output} already exists. Please use `-allowoverwrite true`.");
            }
            csproj.Save(output);
            Console.WriteLine($"complete! new csproj generated at {output}");
        }

        [Command("dryrun-store-publish", "dryrun: modify csproj as uwp store publish modify.")]
        public void DryRun(string path, string pfx, string thumbnail)
        {
            Console.WriteLine($"command: 'dry-uwp-store-publish'. Parameters are as follows.");
            Console.WriteLine($"{nameof(path)}: {path}");
            Console.WriteLine($"{nameof(pfx)}: {pfx}");
            Console.WriteLine($"{nameof(thumbnail)}: {thumbnail}");

            // run
            Console.WriteLine($"begin editing csproj\n");
            var csproj = CsProjEditor.Project.Load(path);
            Modify(csproj, pfx, thumbnail);

            // output inmemory xml
            Console.WriteLine("complete! generated csproj will be follows.");
            Console.WriteLine("========== FROM HERE ==========");
            Console.WriteLine(csproj.ToString());
            Console.WriteLine("========== UNTIL HERE ==========");
        }

        private void Modify(CsProjEditor.Project csproj, string pfx, string thumbnail)
        {
            // edit
            csproj.SetNodeValue("PropertyGroup", "PackageCertificateKeyFile", pfx);
            csproj.InsertNode("PropertyGroup", "PackageCertificateThumbprint", thumbnail);
            csproj.InsertNode("PropertyGroup", "GenerateAppInstallerFile", "False");
            csproj.InsertNode("PropertyGroup", "AppxAutoIncrementPackageRevision", "True");
            csproj.InsertNode("PropertyGroup", "AppxSymbolPackageEnabled", "False");
            csproj.InsertNode("PropertyGroup", "AppxBundle", "Always");
            csproj.InsertNode("PropertyGroup", "AppxBundlePlatforms", "x86");
            csproj.InsertNode("PropertyGroup", "AppInstallerUpdateFrequency", "1");
            csproj.InsertNode("PropertyGroup", "AppInstallerCheckForUpdateFrequency", "OnApplicationRun");
            csproj.InsertAttribute("ItemGroup", "None", new CsProjAttribute("Include", pfx), x => !x.HasAttributes);
            csproj.InsertAttribute("ItemGroup", "None", new CsProjAttribute("Include", "Package.StoreAssociation.xml"), x => !x.HasAttributes);
        }
    }
}
