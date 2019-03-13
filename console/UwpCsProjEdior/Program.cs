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
        private readonly ILogger<UwpCsProjEdior> logger;
        public UwpCsProjEdior(ILogger<UwpCsProjEdior> logger)
        {
            this.logger = logger;
        }

        [Command("run")]
        public void Run(string path, string pfx, string thumbnail, string output, bool allowoverwrite = false)
        {
            // run
            var csproj = CsProjEditor.Load(path);
            Modify(csproj, pfx, thumbnail);

            // save
            if (File.Exists(output) && !allowoverwrite)
            {
                throw new IOException($"Output path {output} already exists. Please use allowoverwrite to overwrite.");
            }
            csproj.Save(output);
            logger.LogInformation($"Run complete, generated {output}");
        }

        [Command("dryrun")]
        public void DryRun(string path, string pfx, string thumbnail)
        {
            var csproj = CsProjEditor.Load(path);
            Modify(csproj, pfx, thumbnail);

            // output inmemory xml
            logger.LogInformation(csproj.ToString());
        }

        private void Modify(CsProjEditor csproj, string pfx, string thumbnail)
        {
            // edit
            csproj.Replace("PropertyGroup", "PackageCertificateKeyFile", pfx);
            csproj.Insert("PropertyGroup", "PackageCertificateThumbprint", thumbnail);
            csproj.Insert("PropertyGroup", "GenerateAppInstallerFile", "False");
            csproj.Insert("PropertyGroup", "AppxAutoIncrementPackageRevision", "True");
            csproj.Insert("PropertyGroup", "AppxSymbolPackageEnabled", "False");
            csproj.Insert("PropertyGroup", "AppxBundle", "Always");
            csproj.Insert("PropertyGroup", "AppxBundlePlatforms", "x86");
            csproj.Insert("PropertyGroup", "AppInstallerUpdateFrequency", "1");
            csproj.Insert("PropertyGroup", "AppInstallerCheckForUpdateFrequency", "OnApplicationRun");
            csproj.InsertAttribute("ItemGroup", "None", "Include", pfx);
            csproj.InsertAttribute("ItemGroup", "None", "Include", "Package.StoreAssociation.xml");
        }
    }
}
