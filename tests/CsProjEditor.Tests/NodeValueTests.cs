using FluentAssertions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Xunit;

namespace CsProjEditor.Tests
{
    public class NodeValueTests
    {
        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8CRLF.csproj")]
        [InlineData("testdata/SimpleOldCsProjUtf8LF.csproj")]
        public void GetTest(string csprojPath)
        {
            var csproj = CsProjEditor.Load(csprojPath);
            csproj.ExistsNode("PropertyGroup", "ProjectGuid").Should().BeTrue();
            csproj.GetNodeValue("PropertyGroup", "ProjectGuid").Should().BeEquivalentTo("{721f98d5-49a4-41a0-8bd2-76ef253c61dc}");
        }

        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8CRLF.csproj")]
        [InlineData("testdata/SimpleOldCsProjUtf8LF.csproj")]
        public void GetTFailest(string csprojPath)
        {
            var csproj = CsProjEditor.Load(csprojPath);
            csproj.ExistsNode("PropertyGroup", "ProjectGuidHomge").Should().BeFalse();
            csproj.GetNodeValue("PropertyGroup", "ProjectGuidHomge").Should().BeEmpty();
        }

        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8CRLF.csproj", "\n")]
        [InlineData("testdata/SimpleOldCsProjUtf8LF.csproj", "\n")]
        public void ExistsTest(string csprojPath, string eol)
        {
            var csproj = CsProjEditor.Load(csprojPath);
            csproj.ExistsNodeValue("PropertyGroup", "ProjectGuid", "{721f98d5-49a4-41a0-8bd2-76ef253c61dc}").Should().BeTrue();
            var val = "<Name>Visual C++ 2015 Runtime for Universal Windows Platform Apps</Name>".Replace("<Name>", $"{eol}      ").Replace("</Name>", $"{eol}    ");
            csproj.ExistsNodeValue("ItemGroup", "SDKReference", val).Should().BeTrue();
            csproj.ExistsNodeValue("Target", "Copy", "").Should().BeTrue();
        }

        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8CRLF.csproj")]
        [InlineData("testdata/SimpleOldCsProjUtf8LF.csproj")]
        public void ExistsFailTest(string csprojPath)
        {
            var csproj = CsProjEditor.Load(csprojPath);
            csproj.ExistsNodeValue("PropertyGroup", "ProjectGuid", "NOT_EXISTS}").Should().BeFalse();
            csproj.ExistsNodeValue("ItemGroup", "SDKReference", "NOT_EXISTS").Should().BeFalse();
            csproj.ExistsNodeValue("Target", "Copy", "NOT_EXISTS").Should().BeFalse();
        }

        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8CRLF.csproj")]
        [InlineData("testdata/SimpleOldCsProjUtf8LF.csproj")]
        public void SetTest(string csprojPath)
        {
            var csproj = CsProjEditor.Load(csprojPath);
            csproj.ExistsNode("PropertyGroup", "Hogemoge").Should().BeFalse();
            csproj.InsertNode("PropertyGroup", "Hogemoge", "value");
            csproj.SetNodeValue("PropertyGroup", "Hogemoge", "set");
            csproj.ExistsNode("PropertyGroup", "Hogemoge").Should().BeTrue();
            csproj.ExistsNodeValue("PropertyGroup", "Hogemoge", "set").Should().BeTrue();
        }

        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8CRLF.csproj")]
        [InlineData("testdata/SimpleOldCsProjUtf8LF.csproj")]
        public void SetFailTest(string csprojPath)
        {
            // none existing group insertion will not throw and do nothing.
            var csproj = CsProjEditor.Load(csprojPath);
            csproj.ExistsNode("Hogemoge", "Hogemoge").Should().BeFalse();
            csproj.SetNodeValue("Hogemoge", "Hogemoge", "value");
            csproj.ExistsNode("Hogemoge", "Hogemoge").Should().BeFalse();
        }

        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8CRLF.csproj")]
        [InlineData("testdata/SimpleOldCsProjUtf8LF.csproj")]
        public void AppendTest(string csprojPath)
        {
            var csproj = CsProjEditor.Load(csprojPath);
            csproj.ExistsNode("PropertyGroup", "Hogemoge").Should().BeFalse();
            csproj.InsertNode("PropertyGroup", "Hogemoge", "value");
            csproj.AppendNodeValue("PropertyGroup", "Hogemoge", "append");
            csproj.ExistsNode("PropertyGroup", "Hogemoge").Should().BeTrue();
            csproj.ExistsNodeValue("PropertyGroup", "Hogemoge", "valueappend").Should().BeTrue();
        }

        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8CRLF.csproj")]
        [InlineData("testdata/SimpleOldCsProjUtf8LF.csproj")]
        public void AppendFailTest(string csprojPath)
        {
            // none existing group insertion will not throw and do nothing.
            var csproj = CsProjEditor.Load(csprojPath);
            csproj.ExistsNode("Hogemoge", "Hogemoge").Should().BeFalse();
            csproj.AppendNodeValue("Hogemoge", "Hogemoge", "value");
            csproj.ExistsNode("Hogemoge", "Hogemoge").Should().BeFalse();
        }

        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8CRLF.csproj")]
        [InlineData("testdata/SimpleOldCsProjUtf8LF.csproj")]
        public void PrependTest(string csprojPath)
        {
            var csproj = CsProjEditor.Load(csprojPath);
            csproj.ExistsNode("PropertyGroup", "Hogemoge").Should().BeFalse();
            csproj.InsertNode("PropertyGroup", "Hogemoge", "value");
            csproj.PrependNodeValue("PropertyGroup", "Hogemoge", "prepend");
            csproj.ExistsNode("PropertyGroup", "Hogemoge").Should().BeTrue();
            csproj.ExistsNodeValue("PropertyGroup", "Hogemoge", "prependvalue").Should().BeTrue();
        }

        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8CRLF.csproj")]
        [InlineData("testdata/SimpleOldCsProjUtf8LF.csproj")]
        public void PrependFailTest(string csprojPath)
        {
            // none existing group insertion will not throw and do nothing.
            var csproj = CsProjEditor.Load(csprojPath);
            csproj.ExistsNode("Hogemoge", "Hogemoge").Should().BeFalse();
            csproj.PrependNodeValue("Hogemoge", "Hogemoge", "value");
            csproj.ExistsNode("Hogemoge", "Hogemoge").Should().BeFalse();
        }

        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8CRLF.csproj")]
        [InlineData("testdata/SimpleOldCsProjUtf8LF.csproj")]
        public void ReplaceTest(string csprojPath)
        {
            var csproj = CsProjEditor.Load(csprojPath);
            // simple replacement
            csproj.ExistsNode("PropertyGroup", "OutputType").Should().BeTrue();
            csproj.ExistsNodeValue("PropertyGroup", "OutputType", "AppContainerExe").Should().BeTrue();
            csproj.ReplaceNodeValue("PropertyGroup", "OutputType", "AppContainerExe", "Hogemoge");
            csproj.ExistsNodeValue("PropertyGroup", "OutputType", "Hogemoge").Should().BeTrue();

            // replacement can specify which letter to replace with via `pattern`.
            // In this case, node name `ProjectGuid` will replace `Guid` with `Hogemoge`, so the resuld must be `ProjectHogemoge`.
            csproj.ExistsNode("PropertyGroup", "ProjectGuid").Should().BeTrue();
            var values = csproj.GetNodeValue("PropertyGroup", "ProjectGuid");
            foreach (var val in values)
            {
                csproj.ExistsNodeValue("PropertyGroup", "ProjectGuid", val).Should().BeTrue();
                csproj.ReplaceNodeValue("PropertyGroup", "ProjectGuid", val, "721f98d5", "Hogemoge");
                csproj.ExistsNodeValue("PropertyGroup", "ProjectGuid", val.Replace("721f98d5", "Hogemoge")).Should().BeTrue();
            }
        }

        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8CRLF.csproj")]
        [InlineData("testdata/SimpleOldCsProjUtf8LF.csproj")]
        public void ReplaceFailTest(string csprojPath)
        {
            var csproj = CsProjEditor.Load(csprojPath);

            // not exists node will not do any.
            csproj.ExistsNode("PropertyGroup", "OutputTypeHoge").Should().BeFalse();
            csproj.ReplaceNodeValue("PropertyGroup", "OutputTypeHoge", "Hogemoge", "Fugafuga");
            csproj.ExistsNode("PropertyGroup", "Hogemoge").Should().BeFalse();
            csproj.ExistsNodeValue("PropertyGroup", "Hogemoge", "Fugafuga").Should().BeFalse();

            // not exists value will not do any.
            csproj.ExistsNode("PropertyGroup", "ProjectGuid").Should().BeTrue();
            csproj.ReplaceNodeValue("PropertyGroup", "ProjectGuid", "Hogemoge", "Guid");
            csproj.ExistsNode("PropertyGroup", "ProjectGuid").Should().BeTrue();
            csproj.ExistsNodeValue("PropertyGroup", "ProjectGuid", "Guid").Should().BeFalse();
        }

        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8CRLF.csproj")]
        [InlineData("testdata/SimpleOldCsProjUtf8LF.csproj")]
        public void RemoveTest(string csprojPath)
        {
            var csproj = CsProjEditor.Load(csprojPath);

            // Remove with leave blank line test.
            csproj.ExistsNode("PropertyGroup", "OutputType").Should().BeTrue();
            csproj.ExistsNodeValue("PropertyGroup", "OutputType", "AppContainerExe").Should().BeTrue();
            csproj.RemoveNodeValue("PropertyGroup", "OutputType");
            csproj.ExistsNode("PropertyGroup", "OutputType").Should().BeTrue();
            csproj.ExistsNodeValue("PropertyGroup", "OutputType", "").Should().BeTrue();
        }

        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8CRLF.csproj")]
        [InlineData("testdata/SimpleOldCsProjUtf8LF.csproj")]
        public void RemoveFailTest(string csprojPath)
        {
            var csproj = CsProjEditor.Load(csprojPath);

            // not exists node will not do any.
            csproj.ExistsNode("PropertyGroup", "OutputTypeHoge").Should().BeFalse();
            csproj.ExistsNodeValue("PropertyGroup", "OutputTypeHoge", "AppContainerExe").Should().BeFalse();
            csproj.RemoveNodeValue("PropertyGroup", "OutputTypeHoge");
            csproj.ExistsNode("PropertyGroup", "OutputTypeHoge").Should().BeFalse();
        }
    }
}
