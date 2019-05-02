using FluentAssertions;
using System;
using System.Collections.Generic;
using System.IO;
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
            var testData = Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "testdata"), "SimpleOldCsProjUtf8*.csproj");
            var tempFolder = Path.Combine(Path.GetTempPath(), nameof(FileTest));
            if (!Directory.Exists(tempFolder))
                Directory.CreateDirectory(tempFolder);
            this.tempFolder = tempFolder;

            // Copy csproj to temp
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

        [Fact]
        public void SaveShouldbeTest()
        {
            foreach (var csprojPath in tempPaths)
            {
                var csproj = CsProjEditor.Load(csprojPath);
            }
        }
    }
}
