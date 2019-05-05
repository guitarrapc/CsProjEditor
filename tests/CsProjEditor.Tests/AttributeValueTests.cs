using FluentAssertions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Xunit;

namespace CsProjEditor.Tests
{
    public class AttributeValueTests
    {
        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleOldCsProjUtf8_LF.csproj")]
        public void GetTest(string csprojPath)
        {
            var csproj = CsProjEditor.Load(csprojPath);
            csproj.ExistsNode("ItemGroup", "SDKReference").Should().BeTrue();
            csproj.GetAttributeValue("ItemGroup", "SDKReference", "Include").Should().BeEquivalentTo(new[] { "Microsoft.VCLibs, Version=14.0", "WindowsMobile, Version=10.0.17134.0" });
        }

        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleOldCsProjUtf8_LF.csproj")]
        public void GetFailest(string csprojPath)
        {
            var csproj = CsProjEditor.Load(csprojPath);
            csproj.ExistsNode("ItemGroup", "SDKReferenceX").Should().BeFalse();
            csproj.GetAttributeValue("ItemGroup", "SDKReferenceX", "Include").Should().BeEmpty();
        }

        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleOldCsProjUtf8_LF.csproj")]
        public void ExistsTest(string csprojPath)
        {
            var csproj = CsProjEditor.Load(csprojPath);
            csproj.ExistsNode("ItemGroup", "SDKReference").Should().BeTrue();
            csproj.ExistsAttributeValue("ItemGroup", "SDKReference", "Include", "Microsoft.VCLibs, Version=14.0").Should().BeTrue();
            csproj.ExistsNode("ItemGroup", "Compile").Should().BeTrue();
            csproj.ExistsAttributeValue("ItemGroup", "Compile", "Include", "App.cs").Should().BeTrue();
            csproj.ExistsNode("ItemGroup", "None").Should().BeTrue();
            csproj.ExistsAttributeValue("ItemGroup", "None", "Include", "project.json").Should().BeTrue();
            csproj.ExistsNode("Target", "Message").Should().BeTrue();
            csproj.ExistsAttributeValue("Target", "Message", "Importance", "high").Should().BeTrue();
        }

        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleOldCsProjUtf8_LF.csproj")]
        public void ExistsFailTest(string csprojPath)
        {
            var csproj = CsProjEditor.Load(csprojPath);
            // none existing attribute
            csproj.ExistsNode("ItemGroup", "SDKReference").Should().BeTrue();
            csproj.ExistsAttributeValue("ItemGroup", "SDKReference", "Include", "Microsoft.VCLibs, Version=").Should().BeFalse();
            csproj.ExistsNode("ItemGroup", "Compile").Should().BeTrue();
            csproj.ExistsAttributeValue("ItemGroup", "Compile", "Include", "App.csxxxxx").Should().BeFalse();
            // none existing node
            csproj.ExistsNode("ItemGroup", "NoneX").Should().BeFalse();
            csproj.ExistsAttributeValue("ItemGroup", "NoneX", "Include", "project.json").Should().BeFalse();
            csproj.ExistsNode("TargetA", "Message").Should().BeFalse();
            csproj.ExistsAttributeValue("TargetA", "Message", "Importance", "high").Should().BeFalse();
        }

        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleOldCsProjUtf8_LF.csproj")]
        public void SetTest(string csprojPath)
        {
            var csproj = CsProjEditor.Load(csprojPath);
            csproj.ExistsNode("PropertyGroup", "Hogemoge").Should().BeFalse();
            csproj.InsertNode("PropertyGroup", "Hogemoge", "value");
            csproj.InsertAttribute("PropertyGroup", "Hogemoge", "set", "hoge", e => !e.HasAttributes);
            csproj.SetAttributeValue("PropertyGroup", "Hogemoge", "set", "hoge", "fuga");
            csproj.ExistsNode("PropertyGroup", "Hogemoge").Should().BeTrue();
            csproj.ExistsAttribute("PropertyGroup", "Hogemoge", "set").Should().BeTrue();
            csproj.ExistsAttributeValue("PropertyGroup", "Hogemoge", "set", "hoge").Should().BeFalse();
            csproj.ExistsAttributeValue("PropertyGroup", "Hogemoge", "set", "fuga").Should().BeTrue();
        }

        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleOldCsProjUtf8_LF.csproj")]
        public void SetFailTest(string csprojPath)
        {
            // none existing group insertion will not throw and do nothing.
            var csproj = CsProjEditor.Load(csprojPath);
            csproj.SetAttributeValue("Hogemoge", "Hogemoge", "set", "hoge", "fuga");
            csproj.ExistsAttributeValue("PropertyGroup", "Hogemoge", "set", "fuga").Should().BeFalse();
        }

        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleOldCsProjUtf8_LF.csproj")]
        public void AppendTest(string csprojPath)
        {
            var csproj = CsProjEditor.Load(csprojPath);
            csproj.ExistsNode("PropertyGroup", "Hogemoge").Should().BeFalse();
            csproj.InsertNode("PropertyGroup", "Hogemoge", "value");
            csproj.InsertAttribute("PropertyGroup", "Hogemoge", "set", "hoge", e => !e.HasAttributes);
            csproj.AppendAttributeValue("PropertyGroup", "Hogemoge", "set", "hoge", "fuga");
            csproj.ExistsNode("PropertyGroup", "Hogemoge").Should().BeTrue();
            csproj.ExistsAttribute("PropertyGroup", "Hogemoge", "set").Should().BeTrue();
            csproj.ExistsAttributeValue("PropertyGroup", "Hogemoge", "set", "hoge").Should().BeFalse();
            csproj.ExistsAttributeValue("PropertyGroup", "Hogemoge", "set", "hogefuga").Should().BeTrue();
        }

        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleOldCsProjUtf8_LF.csproj")]
        public void AppendFailTest(string csprojPath)
        {
            // none existing group insertion will not throw and do nothing.
            var csproj = CsProjEditor.Load(csprojPath);
            csproj.AppendAttributeValue("Hogemoge", "Hogemoge", "set", "hoge", "fuga");
            csproj.ExistsAttributeValue("PropertyGroup", "Hogemoge", "set", "hogefuga").Should().BeFalse();
        }

        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleOldCsProjUtf8_LF.csproj")]
        public void PrependTest(string csprojPath)
        {
            var csproj = CsProjEditor.Load(csprojPath);
            csproj.ExistsNode("PropertyGroup", "Hogemoge").Should().BeFalse();
            csproj.InsertNode("PropertyGroup", "Hogemoge", "value");
            csproj.InsertAttribute("PropertyGroup", "Hogemoge", "set", "hoge", e => !e.HasAttributes);
            csproj.PrependAttributeValue("PropertyGroup", "Hogemoge", "set", "hoge", "fuga");
            csproj.ExistsNode("PropertyGroup", "Hogemoge").Should().BeTrue();
            csproj.ExistsAttribute("PropertyGroup", "Hogemoge", "set").Should().BeTrue();
            csproj.ExistsAttributeValue("PropertyGroup", "Hogemoge", "set", "hoge").Should().BeFalse();
            csproj.ExistsAttributeValue("PropertyGroup", "Hogemoge", "set", "fugahoge").Should().BeTrue();
        }

        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleOldCsProjUtf8_LF.csproj")]
        public void PrependFailTest(string csprojPath)
        {
            // none existing group insertion will not throw and do nothing.
            var csproj = CsProjEditor.Load(csprojPath);
            csproj.PrependAttributeValue("Hogemoge", "Hogemoge", "set", "hoge", "fuga");
            csproj.ExistsAttributeValue("PropertyGroup", "Hogemoge", "set", "fugahoge").Should().BeFalse();
        }

        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleOldCsProjUtf8_LF.csproj")]
        public void ReplaceTest(string csprojPath)
        {
            var csproj = CsProjEditor.Load(csprojPath);
            // simple replacement
            csproj.ExistsNode("ItemGroup", "SDKReference").Should().BeTrue();
            csproj.ExistsAttributeValue("ItemGroup", "SDKReference", "Include", "Microsoft.VCLibs, Version=14.0").Should().BeTrue();
            csproj.ReplaceAttributeValue("ItemGroup", "SDKReference", "Include", "Microsoft.VCLibs, Version=14.0", "Hogemoge");
            csproj.ExistsAttributeValue("ItemGroup", "SDKReference", "Include", "Hogemoge").Should().BeTrue();

            // replacement can specify which letter to replace with via `pattern`.
            // In this case, node name `ProjectGuid` will replace `Guid` with `Hogemoge`, so the resuld must be `ProjectHogemoge`.
            csproj.ExistsNode("ItemGroup", "Compile").Should().BeTrue();
            csproj.ExistsAttributeValue("ItemGroup", "Compile", "Include", "App.cs").Should().BeTrue();
            csproj.ReplaceAttributeValue("ItemGroup", "Compile", "Include", "App.cs", "App", "Hoge");
            csproj.ExistsAttributeValue("ItemGroup", "Compile", "Include", "Hoge.cs").Should().BeTrue();
        }

        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleOldCsProjUtf8_LF.csproj")]
        public void ReplaceFailTest(string csprojPath)
        {
            var csproj = CsProjEditor.Load(csprojPath);

            // not exists attribute will not do any.
            csproj.ExistsNode("ItemGroup", "SDKReference").Should().BeTrue();
            csproj.ReplaceAttribute("ItemGroup", "SDKReference", "IncludeX", "Microsoft.VCLibs, Version=14.0", "Hogemoge");
            csproj.ExistsAttributeValue("ItemGroup", "SDKReference", "Include", "Hogemoge").Should().BeFalse();

            // not exists node will not do any
            csproj.ExistsNode("ItemGroup", "SDKReferenceX").Should().BeFalse();
            csproj.ReplaceAttribute("ItemGroup", "SDKReferenceX", "Include", "Microsoft.VCLibs, Version=14.0", "Hogemoge");
            csproj.ExistsAttributeValue("ItemGroup", "SDKReferenceX", "Include", "Hogemoge").Should().BeFalse();
        }

        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleOldCsProjUtf8_LF.csproj")]
        public void RemoveTest(string csprojPath)
        {
            var csproj = CsProjEditor.Load(csprojPath);

            csproj.ExistsNode("ItemGroup", "SDKReference").Should().BeTrue();
            csproj.ExistsAttributeValue("ItemGroup", "SDKReference", "Include", "Microsoft.VCLibs, Version=14.0").Should().BeTrue();
            csproj.RemoveAttributeValue("ItemGroup", "SDKReference", "Include", "Microsoft.VCLibs, Version=14.0");
            csproj.ExistsAttributeValue("ItemGroup", "SDKReference", "Include", "Microsoft.VCLibs, Version=14.0").Should().BeFalse();
            csproj.ExistsAttributeValue("ItemGroup", "SDKReference", "Include", "").Should().BeTrue();
        }

        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleOldCsProjUtf8_LF.csproj")]
        public void RemoveFailTest(string csprojPath)
        {
            var csproj = CsProjEditor.Load(csprojPath);

            // not exists node will not do any.
            csproj.ExistsNode("ItemGroup", "SDKReferenceX").Should().BeFalse();
            csproj.ExistsAttributeValue("ItemGroup", "SDKReferenceX", "Include", "Microsoft.VCLibs, Version=14.0").Should().BeFalse();
            csproj.RemoveAttributeValue("ItemGroup", "SDKReferenceX", "Include", "Microsoft.VCLibs, Version=14.0");
            csproj.ExistsAttributeValue("ItemGroup", "SDKReferenceX", "Include", "Microsoft.VCLibs, Version=14.0").Should().BeFalse();
            csproj.ExistsAttributeValue("ItemGroup", "SDKReferenceX", "Include", "").Should().BeFalse();
        }
    }
}
