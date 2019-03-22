using MicroBatchFramework;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CsProjEditor.UwpCsProjEdior
{
    // make UWP Store associated csproj.
    // MEMO: APP pfx and Package.StoreAssociation.xml should prepare/exists in advanced. (with manual relation on Visual Studio)
    class Program
    {
        static async Task Main(string[] args)
        {
            await new HostBuilder().RunBatchEngineAsync<UwpCsProjEdior>(args);
        }
    }

    public class UwpCsProjEdior : BatchBase
    {
        [Command("run")]
        public void Run(string path, string pfx, string thumbnail, string output, bool allowoverwrite = false)
        {
            this.Context.Logger.LogInformation($"command: 'run'. Parameters are as follows.");
            this.Context.Logger.LogInformation($"{nameof(path)}: {path}");
            this.Context.Logger.LogInformation($"{nameof(pfx)}: {pfx}");
            this.Context.Logger.LogInformation($"{nameof(thumbnail)}: {thumbnail}");
            this.Context.Logger.LogInformation($"{nameof(output)}: {output}");
            this.Context.Logger.LogInformation($"{nameof(allowoverwrite)}: {allowoverwrite}");
            this.Context.Logger.LogInformation("\n");

            // run
            this.Context.Logger.LogInformation($"begin editing csproj.");
            var csproj = CsProjEditor.Load(path);
            Modify(csproj, pfx, thumbnail);

            // save
            this.Context.Logger.LogInformation($"saving generated csproj to {output}");
            if (File.Exists(output) && !allowoverwrite)
            {
                throw new IOException($"Output path {output} already exists. Please use allowoverwrite to overwrite.");
            }
            csproj.Save(output);
            this.Context.Logger.LogInformation($"complete! new csproj generated at {output}");
        }

        [Command("dryrun")]
        public void DryRun(string path, string pfx, string thumbnail)
        {
            this.Context.Logger.LogInformation($"command: 'dryrun'. Parameters are as follows.");
            this.Context.Logger.LogInformation($"{nameof(path)}: {path}");
            this.Context.Logger.LogInformation($"{nameof(pfx)}: {pfx}");
            this.Context.Logger.LogInformation($"{nameof(thumbnail)}: {thumbnail}");

            // run
            this.Context.Logger.LogInformation($"begin editing csproj\n");
            var csproj = CsProjEditor.Load(path);
            Modify(csproj, pfx, thumbnail);

            // output inmemory xml
            this.Context.Logger.LogInformation("complete! generated csproj will be follows.");
            this.Context.Logger.LogInformation("========== FROM HERE ==========");
            this.Context.Logger.LogInformation(csproj.ToString());
            this.Context.Logger.LogInformation("========== UNTIL HERE ==========");
        }

        private void Modify(CsProjEditor csproj, string pfx, string thumbnail)
        {
            // edit
            csproj.SetValue("PropertyGroup", "PackageCertificateKeyFile", pfx);
            csproj.InsertNode("PropertyGroup", "PackageCertificateThumbprint", thumbnail);
            csproj.InsertNode("PropertyGroup", "GenerateAppInstallerFile", "False");
            csproj.InsertNode("PropertyGroup", "AppxAutoIncrementPackageRevision", "True");
            csproj.InsertNode("PropertyGroup", "AppxSymbolPackageEnabled", "False");
            csproj.InsertNode("PropertyGroup", "AppxBundle", "Always");
            csproj.InsertNode("PropertyGroup", "AppxBundlePlatforms", "x86");
            csproj.InsertNode("PropertyGroup", "AppInstallerUpdateFrequency", "1");
            csproj.InsertNode("PropertyGroup", "AppInstallerCheckForUpdateFrequency", "OnApplicationRun");
            csproj.InsertAttribute("ItemGroup", "None", "Include", pfx);
            csproj.InsertAttribute("ItemGroup", "None", "Include", "Package.StoreAssociation.xml");
        }
    }
}
