using FluentAssertions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Xunit;

namespace CsProjEditor.Tests
{
    public class AttributeTests
    {
        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleOldCsProjUtf8_LF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_LF.csproj")]
        public void GetTest(string csprojPath)
        {
            // <Project>
            //   <ItemGroup>
            //     <None />
            //   </ItemGroup>
            // </Project>

            var csproj = Project.Load(csprojPath);
            csproj.ExistsNode("ItemGroup", "None").Should().BeTrue();
            csproj.GetAttribute("ItemGroup", "None").Should().Contain("Include");
            csproj.GetAttribute("ItemGroup", "None").Should().BeEquivalentTo(new[] { "Include", "Include" });
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
            csproj.GetAttribute("ItemGroup", "NoneX").Should().BeEmpty();
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
            //     <None Include/>
            //     <Compile Include/>
            //   </ItemGroup>
            //   <Target>
            //     <Message Importance/>
            //   </Target>
            // </Project>

            var csproj = Project.Load(csprojPath);
            csproj.ExistsNode("ItemGroup", "None").Should().BeTrue();
            csproj.ExistsAttribute("ItemGroup", "None", "Include").Should().BeTrue();
            csproj.ExistsNode("ItemGroup", "Compile").Should().BeTrue();
            csproj.ExistsAttribute("ItemGroup", "Compile", "Include").Should().BeTrue();
            csproj.ExistsNode("ItemGroup", "None").Should().BeTrue();
            csproj.ExistsAttribute("ItemGroup", "None", "Include").Should().BeTrue();
            csproj.ExistsNode("Target", "Message").Should().BeTrue();
            csproj.ExistsAttribute("Target", "Message", "Importance").Should().BeTrue();
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
            csproj.ExistsAttribute("ItemGroup", "None", "IncludeX").Should().BeFalse();
            csproj.ExistsNode("ItemGroup", "Compile").Should().BeTrue();
            csproj.ExistsAttribute("ItemGroup", "Compile", "IncludeX").Should().BeFalse();
            // none existing node
            csproj.ExistsNode("ItemGroup", "NoneX").Should().BeFalse();
            csproj.ExistsAttribute("ItemGroup", "NoneX", "Include").Should().BeFalse();
            csproj.ExistsNode("TargetA", "Message").Should().BeFalse();
            csproj.ExistsAttribute("TargetA", "Message", "Importance").Should().BeFalse();
        }

        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleOldCsProjUtf8_LF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_LF.csproj")]
        public void InsertTest(string csprojPath)
        {
            // <Project>
            //   <PropertyGroup>
            //     <Hogemoge>value</Hogemoge>
            //     <Hogemoge Fugafuga="Value"/>
            //     <OutputType Fugafuga="Value"/>
            //   </PropertyGroup>
            // </Project>

            var csproj = Project.Load(csprojPath);
            csproj.ExistsNode("PropertyGroup", "Hogemoge").Should().BeFalse();
            csproj.InsertNode("PropertyGroup", "Hogemoge", "value");
            csproj.InsertAttribute("PropertyGroup", "Hogemoge", "Fugafuga", "Value", e => !e.HasAttributes);
            csproj.ExistsAttributeValue("PropertyGroup", "Hogemoge", "Fugafuga", "Value").Should().BeTrue();

            csproj.ExistsNode("PropertyGroup", "OutputType").Should().BeTrue();
            csproj.InsertAttribute("PropertyGroup", "OutputType", "Fugafuga", "Value", e => !e.HasAttributes);
            csproj.ExistsAttributeValue("PropertyGroup", "OutputType", "Fugafuga", "Value").Should().BeTrue();
            var x = csproj.ToString();
            // Insert will generate node and attribute
            csproj.GetNode("PropertyGroup", "OutputType").Should().BeEquivalentTo(new[] { "OutputType", "OutputType" });
        }

        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleOldCsProjUtf8_LF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_LF.csproj")]
        public void InsertFailTest(string csprojPath)
        {
            // none existing group insertion will be throw
            var csproj = Project.Load(csprojPath);
            Assert.Throws<System.ArgumentNullException>(() => csproj.InsertAttribute("Hogemoge", "Hogemoge", "Fugafuga", "Value", e => !e.HasAttributes));
        }

        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleOldCsProjUtf8_LF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_LF.csproj")]
        public void SetTest(string csprojPath)
        {
            // <Project>
            //   <PropertyGroup>
            //     <Hogemoge Fugafuga="Value">value</Hogemoge>
            //   </PropertyGroup>
            // </Project>

            var csproj = Project.Load(csprojPath);
            csproj.ExistsNode("PropertyGroup", "Hogemoge").Should().BeFalse();
            csproj.InsertNode("PropertyGroup", "Hogemoge", "value");
            csproj.SetAttribute("PropertyGroup", "Hogemoge", "Fugafuga", "Value", e => !e.HasAttributes);
            csproj.ExistsAttributeValue("PropertyGroup", "Hogemoge", "Fugafuga", "Value").Should().BeTrue();

            csproj.ExistsNode("PropertyGroup", "OutputType").Should().BeTrue();
            csproj.SetAttribute("PropertyGroup", "OutputType", "Fugafuga", "Value", e => !e.HasAttributes);
            csproj.ExistsAttributeValue("PropertyGroup", "OutputType", "Fugafuga", "Value").Should().BeTrue();
            var x = csproj.ToString();
            // Insert will generate node and attribute
            csproj.GetNode("PropertyGroup", "OutputType").Should().BeEquivalentTo(new[] { "OutputType" });
        }

        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleOldCsProjUtf8_LF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_LF.csproj")]
        public void SetFailTest(string csprojPath)
        {
            // none existing group insertion will be throw
            var csproj = Project.Load(csprojPath);
            csproj.SetAttribute("Hogemoge", "Hogemoge", "Fugafuga", "Value", e => !e.HasAttributes);
            csproj.ExistsNode("Hogemoge", "Hogemoge").Should().BeFalse();
        }

        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleOldCsProjUtf8_LF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_LF.csproj")]
        public void ReplaceTest(string csprojPath)
        {
            // before
            // <Project>
            //   <ItemGroup>
            //     <None Include="project.json" />
            //   </ItemGroup>
            // </Project>

            // after
            // <Project>
            //   <ItemGroup>
            //     <None Hogemoge="project.json" />
            //   </ItemGroup>
            // </Project>

            var csproj = Project.Load(csprojPath);
            // simple replacement
            csproj.ExistsNode("ItemGroup", "None").Should().BeTrue();
            csproj.ExistsAttribute("ItemGroup", "None", "Include").Should().BeTrue();
            csproj.ReplaceAttribute("ItemGroup", "None", "Include", "project.json", "Hogemoge");
            var x = csproj.ToString();
            csproj.ExistsAttribute("ItemGroup", "None", "Hogemoge").Should().BeTrue();
        }

        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleOldCsProjUtf8_LF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_LF.csproj")]
        public void ReplaceWithPatternTest(string csprojPath)
        {
            // before
            // <Project>
            //   <ItemGroup>
            //     <Compile Include="App.cs" />
            //   </ItemGroup>
            // </Project>

            // after
            // <Project>
            //   <ItemGroup>
            //     <Compile Exclude="App.cs" />
            //   </ItemGroup>
            // </Project>

            var csproj = Project.Load(csprojPath);
            // replacement can specify which letter to replace with via `pattern`.
            // In this case, node name `ProjectGuid` will replace `Guid` with `Hogemoge`, so the resuld must be `ProjectHogemoge`.
            csproj.ExistsNode("ItemGroup", "Compile").Should().BeTrue();
            csproj.ReplaceAttribute("ItemGroup", "Compile", "Include", "App.cs", "In", "Ex");
            csproj.ExistsAttribute("ItemGroup", "Compile", "Exclude").Should().BeTrue();
            var x = csproj.ToString();
            csproj.ExistsAttributeValue("ItemGroup", "Compile", "Exclude", "App.cs").Should().BeTrue();
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
            // before
            // <Project>
            //   <ItemGroup>
            //     <None Include="project.json" />
            //   </ItemGroup>
            // </Project>

            // after
            // <Project>
            //   <ItemGroup>
            //     <None />
            //   </ItemGroup>
            // </Project>
            var csproj = Project.Load(csprojPath);

            csproj.ExistsNode("ItemGroup", "None").Should().BeTrue();
            csproj.ExistsAttributeValue("ItemGroup", "None", "Include", "project.json").Should().BeTrue();
            csproj.RemoveAttribute("ItemGroup", "None", "Include", "project.json");
            var x = csproj.ToString();
            csproj.ExistsAttributeValue("ItemGroup", "None", "Include", "project.json").Should().BeFalse();
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
            csproj.RemoveAttribute("ItemGroup", "NoneX", "Include", "Microsoft.VCLibs, Version=14.0");
            csproj.ExistsAttributeValue("ItemGroup", "NoneX", "Include", "Microsoft.VCLibs, Version=14.0").Should().BeFalse();
        }
    }
}
