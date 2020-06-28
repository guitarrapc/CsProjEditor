using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace CsProjEditor.Tests
{
    public class ImportTest
    {
        [Theory]
        [InlineData("testdata/Import_LF.csproj")]
        public void GetTest(string csprojPath)
        {
            var csproj = Project.Load(csprojPath);
            csproj.ExistsGroup("Import").Should().BeFalse();
            csproj.InsertGroup("Import");
            var result = csproj.ToString();
            csproj.ExistsGroup("Import").Should().BeTrue();
        }
    }
}
