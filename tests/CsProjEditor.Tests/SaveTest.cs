using FluentAssertions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using Xunit;

namespace CsProjEditor.Tests
{
    public class SaveTest : IDisposable
    {
        private readonly string tempFolder;
        private readonly List<string> tempPaths = new List<string>();

        /// <summary>
        /// Setup
        /// </summary>
        public SaveTest()
        {
            // ready temp folder
            var tempFolder = Path.Combine(Path.GetTempPath(), nameof(FileTest));
            if (!Directory.Exists(tempFolder))
                Directory.CreateDirectory(tempFolder);
            this.tempFolder = tempFolder;

            // Copy csproj to temp
            var testData = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "testdata"), "SimpleOldCsProjUtf8*.csproj")
                : Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "testdata"), "SimpleOldCsProjUtf8_LF.csproj");
            foreach (var item in testData)
            {
                // Gen temp path
                var temp = Path.Combine(tempFolder, Path.GetFileName(item));
                tempPaths.Add(temp);
                if (!File.Exists(temp))
                {
                    File.Copy(item, temp, true);
                }
            }
        }

        /// <summary>
        /// Teardown
        /// </summary>
        public void Dispose()
        {
            // Remove temp path
            Directory.Delete(tempFolder, true);
        }

        [Theory]
        [InlineData("temp.csproj", "compare.csproj")]
        public void SaveShouldbeTest(string tempPath, string resultPath)
        {
            foreach (var csprojPath in tempPaths)
            {
                var csproj = Project.Load(csprojPath);
                // Node / NodeValue
                csproj.ExistsNode("PropertyGroup", "Hogemoge").Should().BeFalse();
                csproj.InsertNode("PropertyGroup", "Hogemoge", "fugafuga");
                csproj.ExistsNodeValue("PropertyGroup", "Hogemoge", "fugafuga").Should().BeTrue();

                // Attribute / AttributeValue
                csproj.ExistsNode("ItemGroup", "Hogemoge").Should().BeFalse();
                csproj.InsertNode("ItemGroup", "Hogemoge", "fugafuga");
                csproj.ExistsAttribute("ItemGroup", "Hogemoge", new CsProjAttribute("Copy")).Should().BeFalse();
                csproj.SetAttribute("ItemGroup", "Hogemoge", new CsProjAttribute("Copy", "hogemoge"), e => !e.HasAttributes);
                csproj.ExistsAttributeValue("ItemGroup", "Hogemoge", "Copy", "hogemoge").Should().BeTrue();

                // save
                var parent = Directory.GetParent(csprojPath);
                var savePath = Path.Combine(parent.FullName, tempPath);
                csproj.Save(savePath);

                // check save and ensure
                var save = Project.Load(savePath);
                save.ExistsNodeValue("PropertyGroup", "Hogemoge", "fugafuga").Should().BeTrue();
                save.ExistsNodeValue("ItemGroup", "Hogemoge", "fugafuga").Should().BeTrue();
                save.ExistsAttributeValue("ItemGroup", "Hogemoge", "Copy", "hogemoge").Should().BeTrue();

                // remove and ensure same as original
                save.RemoveNode("PropertyGroup", "Hogemoge", false);
                save.RemoveNode("ItemGroup", "Hogemoge", false);
                save.ExistsNode("PropertyGroup", "Hogemoge").Should().BeFalse();
                save.ExistsNode("ItemGroup", "Hogemoge").Should().BeFalse();
                var comparePath = Path.Combine(parent.FullName, resultPath);
                save.Save(comparePath);
                var compare = Project.Load(comparePath);
                // make sure comment line will be removed when Removenode with leaveBlankline = false
                Project.Load(csprojPath).ToString().Should().BeEquivalentTo(compare.ToString());
            }
        }
    }
}
