using FluentAssertions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Xunit;

namespace CsProjEditor.Tests
{
    public class NodeTests //: IDisposable
    {
        private readonly string tempFolder;
        private readonly List<string> tempPaths = new List<string>();

        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8CRLF.csproj")]
        [InlineData("testdata/SimpleOldCsProjUtf8LF.csproj")]
        public void ExistsTest(string csprojPath)
        {
            var csproj = CsProjEditor.Load(csprojPath);
            csproj.ExistsNode("PropertyGroup", "ProjectGuid").Should().BeTrue();
            csproj.ExistsNode("PropertyGroup", "HogemogeNotExists").Should().BeFalse();
            csproj.ExistsNode("ItemGroup", "SDKReference").Should().BeTrue();
            csproj.ExistsNode("ItemGroup", "HogemogeNotExists").Should().BeFalse();
            csproj.ExistsNode("Target", "Copy").Should().BeTrue();
            csproj.ExistsNode("Target", "HogemogeNotExists").Should().BeFalse();
        }

        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8CRLF.csproj")]
        [InlineData("testdata/SimpleOldCsProjUtf8LF.csproj")]
        public void InsertTest(string csprojPath)
        {
            var csproj = CsProjEditor.Load(csprojPath);
            csproj.ExistsNode("PropertyGroup", "Hogemoge").Should().BeFalse();
            csproj.InsertNode("PropertyGroup", "Hogemoge", "value");
            csproj.ExistsNode("PropertyGroup", "Hogemoge").Should().BeTrue();
            csproj.ExistsNodeValue("PropertyGroup", "Hogemoge", "value").Should().BeTrue();
        }

        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8CRLF.csproj")]
        [InlineData("testdata/SimpleOldCsProjUtf8LF.csproj")]
        public void InsertFailTest(string csprojPath)
        {
            // none existing group insertion will be throw
            var csproj = CsProjEditor.Load(csprojPath);
            Assert.Throws<NullReferenceException>(() => csproj.InsertNode("Hogemoge", "Hogemoge", "value"));
        }

        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8CRLF.csproj")]
        [InlineData("testdata/SimpleOldCsProjUtf8LF.csproj")]
        public void ReplaceTest(string csprojPath)
        {
            var csproj = CsProjEditor.Load(csprojPath);
            // simple replacement
            csproj.ExistsNode("PropertyGroup", "OutputType").Should().BeTrue();
            csproj.ReplaceNode("PropertyGroup", "OutputType", "Hogemoge");
            csproj.ExistsNode("PropertyGroup", "Hogemoge").Should().BeTrue();

            // replacement can specify which letter to replace with via `pattern`.
            // In this case, node name `ProjectGuid` will replace `Guid` with `Hogemoge`, so the resuld must be `ProjectHogemoge`.
            csproj.ExistsNode("PropertyGroup", "ProjectGuid").Should().BeTrue();
            csproj.ReplaceNode("PropertyGroup", "ProjectGuid", "Guid", "Hogemoge");
            csproj.ExistsNode("PropertyGroup", "ProjectHogemoge").Should().BeTrue();
        }

        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8CRLF.csproj")]
        [InlineData("testdata/SimpleOldCsProjUtf8LF.csproj")]
        public void ReplaceFailTest(string csprojPath)
        {
            var csproj = CsProjEditor.Load(csprojPath);

            // not exists node will not do any.
            csproj.ExistsNode("PropertyGroup", "OutputTypeHoge").Should().BeFalse();
            csproj.ReplaceNode("PropertyGroup", "OutputTypeHoge", "Hogemoge");
            csproj.ExistsNode("PropertyGroup", "Hogemoge").Should().BeFalse();

            csproj.ExistsNode("PropertyGroup", "ProjectGuidHoge").Should().BeFalse();
            csproj.ReplaceNode("PropertyGroup", "ProjectGuidHoge", "Guid", "Hogemoge");
            csproj.ExistsNode("PropertyGroup", "ProjectGuidHoge").Should().BeFalse();

            csproj.ExistsNode("PropertyGroup", "Out").Should().BeFalse();
            csproj.ReplaceNode("PropertyGroup", "Out", "Hogemoge");
            csproj.ExistsNode("PropertyGroup", "Hogemoge").Should().BeFalse();
        }

        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8CRLF.csproj")]
        [InlineData("testdata/SimpleOldCsProjUtf8LF.csproj")]
        public void RemoveTest(string csprojPath)
        {
            var csproj = CsProjEditor.Load(csprojPath);

            // Remove with leave blank line test.
            var beforeCount = csproj.Root.ToString().Split(csproj.Eol.ToEolString()).Length;
            csproj.ExistsNode("PropertyGroup", "OutputType").Should().BeTrue();
            csproj.RemoveNode("PropertyGroup", "OutputType", true);
            csproj.ExistsNode("PropertyGroup", "OutputType").Should().BeFalse();
            var afterCount = csproj.Root.ToString().Split(csproj.Eol.ToEolString()).Length;
            afterCount.Should().Be(beforeCount);

            // Remove node and remove blank line test.
            csproj.ExistsNode("PropertyGroup", "ProjectGuid").Should().BeTrue();
            csproj.RemoveNode("PropertyGroup", "ProjectGuid");
            csproj.ExistsNode("PropertyGroup", "ProjectGuid").Should().BeFalse();
            // remove blankline will remove all blank line. In this case, previous remove's blank line also removed.
            csproj.Root.ToString().Split(csproj.Eol.ToEolString()).Length.Should().Be(beforeCount - 1 - 1);
        }
    }
}
