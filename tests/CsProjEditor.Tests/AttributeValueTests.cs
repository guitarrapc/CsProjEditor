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
        [InlineData("testdata/SimpleNewCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_LF.csproj")]
        public void GetTest(string csprojPath)
        {
            // </Project>
            //   <ItemGroup>
            //    <None/>
            //   </ItemGroup>
            // </Project>

            var csproj = Project.Load(csprojPath);
            csproj.ExistsNode("ItemGroup", "None").Should().BeTrue();
            csproj.GetAttributeValue("ItemGroup", "None", "Include").Should().BeEquivalentTo(new[] { "project.json", "sample.json" });
        }

        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleOldCsProjUtf8_LF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_LF.csproj")]
        public void GetFailest(string csprojPath)
        {
            var csproj = Project.Load(csprojPath);
            csproj.ExistsNode("ItemGroup", "NoneX").Should().BeFalse();
            csproj.GetAttributeValue("ItemGroup", "NoneX", "Include").Should().BeEmpty();
        }

        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleOldCsProjUtf8_LF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_LF.csproj")]
        public void ExistsTest(string csprojPath)
        {
            // <Project>
            //   <ItemGroup>
            //    <None Include="project.json" />
            //    <Compile Include="App.cs" />
            //   </ItemGroup>
            //   <Target>
            //    <Message Importance="high" />
            //   </Target>
            // </Project>
            var csproj = Project.Load(csprojPath);
            csproj.ExistsNode("ItemGroup", "None").Should().BeTrue();
            csproj.ExistsAttributeValue("ItemGroup", "None", "Include", "project.json").Should().BeTrue();
            csproj.ExistsNode("ItemGroup", "Compile").Should().BeTrue();
            csproj.ExistsAttributeValue("ItemGroup", "Compile", "Include", "App.cs").Should().BeTrue();
            csproj.ExistsNode("Target", "Message").Should().BeTrue();
            csproj.ExistsAttributeValue("Target", "Message", "Importance", "high").Should().BeTrue();
        }

        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleOldCsProjUtf8_LF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_LF.csproj")]
        public void ExistsFailTest(string csprojPath)
        {
            var csproj = Project.Load(csprojPath);
            // none existing attribute
            csproj.ExistsNode("ItemGroup", "None").Should().BeTrue();
            csproj.ExistsAttributeValue("ItemGroup", "None", "Include", "Microsoft.VCLibs, Version=").Should().BeFalse();
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
        [InlineData("testdata/SimpleNewCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_LF.csproj")]
        public void SetTest(string csprojPath)
        {
            var csproj = Project.Load(csprojPath);
            csproj.ExistsNode("PropertyGroup", "Fugafuga").Should().BeFalse();
            csproj.InsertNode("PropertyGroup", "Fugafuga", "value");
            csproj.InsertAttribute("PropertyGroup", "Fugafuga", "set", "hoge", e => !e.HasAttributes);
            csproj.SetAttributeValue("PropertyGroup", "Fugafuga", "set", "fuga");
            csproj.ExistsNode("PropertyGroup", "Fugafuga").Should().BeTrue();
            csproj.ExistsAttribute("PropertyGroup", "Fugafuga", "set").Should().BeTrue();
            csproj.ExistsAttributeValue("PropertyGroup", "Fugafuga", "set", "hoge").Should().BeFalse();
            csproj.ExistsAttributeValue("PropertyGroup", "Fugafuga", "set", "fuga").Should().BeTrue();

            // value match
            csproj.ExistsNode("PropertyGroup", "Hogemoge").Should().BeFalse();
            csproj.InsertNode("PropertyGroup", "Hogemoge", "value");
            csproj.InsertAttribute("PropertyGroup", "Hogemoge", "set", "hoge", e => !e.HasAttributes);
            csproj.SetAttributeValue("PropertyGroup", "Hogemoge", "set", "hoge", "fuga");
            csproj.ExistsNode("PropertyGroup", "Hogemoge").Should().BeTrue();
            csproj.ExistsAttribute("PropertyGroup", "Hogemoge", "set").Should().BeTrue();
            csproj.ExistsAttributeValue("PropertyGroup", "Hogemoge", "set", "hoge").Should().BeFalse();
            csproj.ExistsAttributeValue("PropertyGroup", "Hogemoge", "set", "fuga").Should().BeTrue();

            // value not match will not effect
            csproj.SetAttributeValue("PropertyGroup", "Hogemoge", "set", "hoge", "piyo");
            csproj.ExistsAttributeValue("PropertyGroup", "Hogemoge", "set", "piyo").Should().BeFalse();
            csproj.ExistsAttributeValue("PropertyGroup", "Hogemoge", "set", "fuga").Should().BeTrue();
        }

        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleOldCsProjUtf8_LF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_LF.csproj")]
        public void SetFailTest(string csprojPath)
        {
            // none existing group insertion will not throw and do nothing.
            var csproj = Project.Load(csprojPath);
            csproj.SetAttributeValue("Hogemoge", "Hogemoge", "set", "hoge", "fuga");
            csproj.ExistsAttributeValue("PropertyGroup", "Hogemoge", "set", "fuga").Should().BeFalse();
        }

        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleOldCsProjUtf8_LF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_LF.csproj")]
        public void AppendTest(string csprojPath)
        {
            var csproj = Project.Load(csprojPath);
            csproj.ExistsNode("PropertyGroup", "Hogemoge").Should().BeFalse();
            csproj.InsertNode("PropertyGroup", "Hogemoge", "value");
            csproj.InsertAttribute("PropertyGroup", "Hogemoge", "set", "hoge", e => !e.HasAttributes);
            csproj.AppendAttributeValue("PropertyGroup", "Hogemoge", "set", "hoge", "fuga");
            csproj.ExistsNode("PropertyGroup", "Hogemoge").Should().BeTrue();
            csproj.ExistsAttribute("PropertyGroup", "Hogemoge", "set").Should().BeTrue();
            csproj.ExistsAttributeValue("PropertyGroup", "Hogemoge", "set", "hoge").Should().BeFalse();
            csproj.ExistsAttributeValue("PropertyGroup", "Hogemoge", "set", "hogefuga").Should().BeTrue();

            // value not match will not effect
            csproj.AppendAttributeValue("PropertyGroup", "Hogemoge", "set", "hoge", "piyo");
            csproj.ExistsAttributeValue("PropertyGroup", "Hogemoge", "set", "hogepiyo").Should().BeFalse();
            csproj.ExistsAttributeValue("PropertyGroup", "Hogemoge", "set", "hogefuga").Should().BeTrue();
        }

        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleOldCsProjUtf8_LF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_LF.csproj")]
        public void AppendFailTest(string csprojPath)
        {
            // none existing group insertion will not throw and do nothing.
            var csproj = Project.Load(csprojPath);
            csproj.AppendAttributeValue("Hogemoge", "Hogemoge", "set", "hoge", "fuga");
            csproj.ExistsAttributeValue("PropertyGroup", "Hogemoge", "set", "hogefuga").Should().BeFalse();
        }

        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleOldCsProjUtf8_LF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_LF.csproj")]
        public void PrependTest(string csprojPath)
        {
            var csproj = Project.Load(csprojPath);
            csproj.ExistsNode("PropertyGroup", "Hogemoge").Should().BeFalse();
            csproj.InsertNode("PropertyGroup", "Hogemoge", "value");
            csproj.InsertAttribute("PropertyGroup", "Hogemoge", "set", "hoge", e => !e.HasAttributes);
            csproj.PrependAttributeValue("PropertyGroup", "Hogemoge", "set", "hoge", "fuga");
            csproj.ExistsNode("PropertyGroup", "Hogemoge").Should().BeTrue();
            csproj.ExistsAttribute("PropertyGroup", "Hogemoge", "set").Should().BeTrue();
            csproj.ExistsAttributeValue("PropertyGroup", "Hogemoge", "set", "hoge").Should().BeFalse();
            csproj.ExistsAttributeValue("PropertyGroup", "Hogemoge", "set", "fugahoge").Should().BeTrue();

            // value not match will not effect
            csproj.PrependAttributeValue("PropertyGroup", "Hogemoge", "set", "hoge", "piyo");
            csproj.ExistsAttributeValue("PropertyGroup", "Hogemoge", "set", "piyohoge").Should().BeFalse();
            csproj.ExistsAttributeValue("PropertyGroup", "Hogemoge", "set", "fugahoge").Should().BeTrue();
        }

        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleOldCsProjUtf8_LF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_LF.csproj")]
        public void PrependFailTest(string csprojPath)
        {
            // none existing group insertion will not throw and do nothing.
            var csproj = Project.Load(csprojPath);
            csproj.PrependAttributeValue("Hogemoge", "Hogemoge", "set", "hoge", "fuga");
            csproj.ExistsAttributeValue("PropertyGroup", "Hogemoge", "set", "fugahoge").Should().BeFalse();
        }

        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleOldCsProjUtf8_LF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_LF.csproj")]
        public void ReplaceTest(string csprojPath)
        {
            var csproj = Project.Load(csprojPath);
            // simple replacement
            csproj.ExistsNode("ItemGroup", "None").Should().BeTrue();
            csproj.ExistsAttributeValue("ItemGroup", "None", "Include", "project.json").Should().BeTrue();
            csproj.ReplaceAttributeValue("ItemGroup", "None", "Include", "project.json", "Hogemoge");
            csproj.ExistsAttributeValue("ItemGroup", "None", "Include", "Hogemoge").Should().BeTrue();

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
        [InlineData("testdata/SimpleNewCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_LF.csproj")]
        public void ReplaceFailTest(string csprojPath)
        {
            var csproj = Project.Load(csprojPath);

            // not exists attribute will not do any.
            csproj.ExistsNode("ItemGroup", "None").Should().BeTrue();
            csproj.ReplaceAttribute("ItemGroup", "None", "IncludeX", "Microsoft.VCLibs, Version=14.0", "Hogemoge");
            csproj.ExistsAttributeValue("ItemGroup", "None", "Include", "Hogemoge").Should().BeFalse();

            // not exists node will not do any
            csproj.ExistsNode("ItemGroup", "NoneX").Should().BeFalse();
            csproj.ReplaceAttribute("ItemGroup", "NoneX", "Include", "Microsoft.VCLibs, Version=14.0", "Hogemoge");
            csproj.ExistsAttributeValue("ItemGroup", "NoneX", "Include", "Hogemoge").Should().BeFalse();
        }

        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleOldCsProjUtf8_LF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_LF.csproj")]
        public void RemoveTest(string csprojPath)
        {
            var csproj = Project.Load(csprojPath);

            csproj.ExistsNode("ItemGroup", "None").Should().BeTrue();
            csproj.ExistsAttributeValue("ItemGroup", "None", "Include", "project.json").Should().BeTrue();
            csproj.RemoveAttributeValue("ItemGroup", "None", "Include", "project.json");
            csproj.ExistsAttributeValue("ItemGroup", "None", "Include", "project.json").Should().BeFalse();
            csproj.ExistsAttributeValue("ItemGroup", "None", "Include", "").Should().BeTrue();
        }

        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleOldCsProjUtf8_LF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_LF.csproj")]
        public void RemoveFailTest(string csprojPath)
        {
            var csproj = Project.Load(csprojPath);

            // not exists node will not do any.
            csproj.ExistsNode("ItemGroup", "NoneX").Should().BeFalse();
            csproj.ExistsAttributeValue("ItemGroup", "NoneX", "Include", "Microsoft.VCLibs, Version=14.0").Should().BeFalse();
            csproj.RemoveAttributeValue("ItemGroup", "NoneX", "Include", "Microsoft.VCLibs, Version=14.0");
            csproj.ExistsAttributeValue("ItemGroup", "NoneX", "Include", "Microsoft.VCLibs, Version=14.0").Should().BeFalse();
            csproj.ExistsAttributeValue("ItemGroup", "NoneX", "Include", "").Should().BeFalse();
        }
    }
}
