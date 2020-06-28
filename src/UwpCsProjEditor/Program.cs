using MicroBatchFramework;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
            => await BatchHost.CreateDefaultBuilder()
                .RunBatchEngineAsync<CsProjEdior>(args);
    }

    public class CsProjEdior : BatchBase
    {
        [Command("store-publish", "modify csproj as uwp store publish modify.")]
        public void Run(string path, string pfx, string thumbnail, string output, bool allowoverwrite = false)
        {
            this.Context.Logger.LogInformation($"command: 'uwp-store-publish'. Parameters are as follows.");
            this.Context.Logger.LogInformation($"{nameof(path)}: {path}");
            this.Context.Logger.LogInformation($"{nameof(pfx)}: {pfx}");
            this.Context.Logger.LogInformation($"{nameof(thumbnail)}: {thumbnail}");
            this.Context.Logger.LogInformation($"{nameof(output)}: {output}");
            this.Context.Logger.LogInformation($"{nameof(allowoverwrite)}: {allowoverwrite}");
            this.Context.Logger.LogInformation("\n");

            // run
            this.Context.Logger.LogInformation($"begin editing csproj.");
            var csproj = Project.Load(path);
            Modify(csproj, pfx, thumbnail);

            // save
            this.Context.Logger.LogInformation($"saving generated csproj to {output}");
            if (File.Exists(output) && !allowoverwrite)
            {
                throw new IOException($"Output path {output} already exists. Please use `-allowoverwrite true`.");
            }
            csproj.Save(output);
            this.Context.Logger.LogInformation($"complete! new csproj generated at {output}");
        }

        [Command("dryrun-store-publish", "dryrun: modify csproj as uwp store publish modify.")]
        public void DryRun(string path, string pfx, string thumbnail)
        {
            this.Context.Logger.LogInformation($"command: 'dry-uwp-store-publish'. Parameters are as follows.");
            this.Context.Logger.LogInformation($"{nameof(path)}: {path}");
            this.Context.Logger.LogInformation($"{nameof(pfx)}: {pfx}");
            this.Context.Logger.LogInformation($"{nameof(thumbnail)}: {thumbnail}");

            // run
            this.Context.Logger.LogInformation($"begin editing csproj\n");
            var csproj = CsProjEditor.Project.Load(path);
            Modify(csproj, pfx, thumbnail);

            // output inmemory xml
            this.Context.Logger.LogInformation("complete! generated csproj will be follows.");
            this.Context.Logger.LogInformation("========== FROM HERE ==========");
            this.Context.Logger.LogInformation(csproj.ToString());
            this.Context.Logger.LogInformation("========== UNTIL HERE ==========");
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
