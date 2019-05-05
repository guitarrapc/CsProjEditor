using MicroBatchFramework;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using CsProjEditor;
using System.Linq;

namespace dotnetcsproj
{
    // make UWP Store associated csproj.
    // MEMO: APP pfx and Package.StoreAssociation.xml should prepare/exists in advanced. (with manual relation on Visual Studio)
    class Program
    {
        static async Task Main(string[] args)
            => await BatchHost.CreateDefaultBuilder()
                .RunBatchEngineAsync(args);
    }

    public class ProjectBatchBase : BatchBase
    {
        protected void Save(Project csproj, string path, string output, bool allowoverwrite)
        {
            if (string.IsNullOrEmpty(output)) output = "modify_" + path;
            if (File.Exists(output) && !allowoverwrite)
                throw new IOException($"Output path {output} already exists. Please use `-allowoverwrite true`.");
            csproj.Save(output);
            this.Context.Logger.LogInformation($"complete! new csproj generated at {output}");
        }
    }

    [Command("node")]
    public class Node : ProjectBatchBase
    {
        [Command("get", "get node for the group.")]
        public void Get(
            [Option("p", "path of csproj.")]string path,
            [Option("g", "group of nodes. eg. PropertyGroup")]string group,
            [Option("n", "name of node")]string node)
        {
            var results = Project.Load(path).GetNode(group, node);

            if (!results.Any()) this.Context.Logger.LogInformation($"node `{node}` not found.");
            foreach (var item in results)
            {
                this.Context.Logger.LogInformation($"{group}: {item}");
            }
        }
        [Command("exists", "check specified node is exists.")]
        public void Exists(
            [Option("p", "path of csproj.")]string path,
            [Option("g", "group of nodes. eg. PropertyGroup")]string group,
            [Option("n", "name of node")]string node)
        {
            var item = Project.Load(path).ExistsNode(group, node);
            this.Context.Logger.LogInformation(item.ToString());
        }
        [Command("insert", "insert specified node.")]
        public void Insert(
            [Option("p", "path of csproj.")]string path,
            [Option("g", "group of nodes. eg. PropertyGroup")]string group,
            [Option("n", "name of node")]string node,
            [Option("v", "value of node")]string value = "",
            bool dry = true,
            string output = "",
            bool allowoverwrite = false)
        {
            var csproj = Project.Load(path);
            csproj.InsertNode(group, node, value);
            if (dry)
            {
                this.Context.Logger.LogInformation(csproj.ToString());
                return;
            }
            Save(csproj, path, output, allowoverwrite);
        }
        [Command("relace", "replace specified node.")]
        public void Replace(
            [Option("p", "path of csproj.")]string path,
            [Option("g", "group of nodes. eg. PropertyGroup")]string group,
            [Option("n", "name of node")]string node,
            string pattern,
            string replacement,
            bool dry = true,
            string output = "",
            bool allowoverwrite = false)
        {
            var csproj = Project.Load(path);
            csproj.ReplaceNode(group, node, pattern, replacement);
            if (dry)
            {
                this.Context.Logger.LogInformation(csproj.ToString());
                return;
            }
            Save(csproj, path, output, allowoverwrite);
        }

        [Command("remove", "remove specified node.")]
        public void Remove(
            [Option("p", "path of csproj.")]string path,
            [Option("g", "group of nodes. eg. PropertyGroup")]string group,
            [Option("n", "name of node")]string node,
            bool dry = true,
            string output = "",
            bool allowoverwrite = false)
        {
            var csproj = Project.Load(path);
            csproj.RemoveNode(group, node);
            if (dry)
            {
                this.Context.Logger.LogInformation(csproj.ToString());
                return;
            }
            Save(csproj, path, output, allowoverwrite);
        }
    }

    [Command("nodevalue")]
    public class NodeValue : ProjectBatchBase
    {
        [Command("get", "get value for the node.")]
        public void Get(
            [Option("p", "path of csproj.")]string path,
            [Option("g", "group of nodes. eg. PropertyGroup")]string group,
            [Option("n", "name of node")]string node)
        {
            var results = Project.Load(path).GetNodeValue(group, node);

            if (!results.Any()) this.Context.Logger.LogInformation($"node `{node}` not found.");
            foreach (var item in results)
            {
                this.Context.Logger.LogInformation($"{group}.{node}: {item}");
            }
        }
        [Command("exists", "check specified node's value is exists.")]
        public void Exists(
            [Option("p", "path of csproj.")]string path,
            [Option("g", "group of nodes. eg. PropertyGroup")]string group,
            [Option("n", "name of node")]string node,
            [Option("v", "value of node")]string value)
        {
            var item = Project.Load(path).ExistsNodeValue(group, node, value);
            this.Context.Logger.LogInformation(item.ToString());
        }
        [Command("set", "set specified node value.")]
        public void Set(
            [Option("p", "path of csproj.")]string path,
            [Option("g", "group of nodes. eg. PropertyGroup")]string group,
            [Option("n", "name of node")]string node,
            [Option("v", "value of node")]string value,
            bool dry = true,
            string output = "",
            bool allowoverwrite = false)
        {
            var csproj = Project.Load(path);
            csproj.SetNodeValue(group, node, value);
            if (dry)
            {
                this.Context.Logger.LogInformation(csproj.ToString());
                return;
            }
            Save(csproj, path, output, allowoverwrite);
        }
        [Command("append", "append specified node value.")]
        public void Append([Option("p", "path of csproj.")]string path,
            [Option("g", "group of nodes. eg. PropertyGroup")]string group,
            [Option("n", "name of node")]string node,
            [Option("v", "value of node")]string value,
            string append,
            bool dry = true,
            string output = "",
            bool allowoverwrite = false)
        {
            var csproj = Project.Load(path);
            csproj.AppendNodeValue(group, node, value, append);
            if (dry)
            {
                this.Context.Logger.LogInformation(csproj.ToString());
                return;
            }
            Save(csproj, path, output, allowoverwrite);
        }
        [Command("prepend", "preppend specified node value.")]
        public void Prepend(
            [Option("p", "path of csproj.")]string path,
            [Option("g", "group of nodes. eg. PropertyGroup")]string group,
            [Option("n", "name of node")]string node,
            [Option("v", "value of node")]string value,
            string prepend,
            bool dry = true,
            string output = "",
            bool allowoverwrite = false)
        {
            var csproj = Project.Load(path);
            csproj.PrependNodeValue(group, node, value, prepend);
            if (dry)
            {
                this.Context.Logger.LogInformation(csproj.ToString());
                return;
            }
            Save(csproj, path, output, allowoverwrite);
        }
        [Command("replace", "replace specified node value.")]
        public void Replace(
            [Option("p", "path of csproj.")]string path,
            [Option("g", "group of nodes. eg. PropertyGroup")]string group,
            [Option("n", "name of node")]string node,
            [Option("v", "value of node")]string value,
            string pattern,
            string replacement,
            bool dry = true,
            string output = "",
            bool allowoverwrite = false)
        {
            var csproj = Project.Load(path);
            csproj.ReplaceNodeValue(group, node, value, pattern, replacement);
            if (dry)
            {
                this.Context.Logger.LogInformation(csproj.ToString());
                return;
            }
            Save(csproj, path, output, allowoverwrite);
        }
        [Command("remove", "remove specified node value.")]
        public void Remove([Option("p", "path of csproj.")]string path,
            [Option("g", "group of nodes. eg. PropertyGroup")]string group,
            [Option("n", "name of node")]string node,
            [Option("v", "value of node")]string value,
            bool dry = true,
            string output = "",
            bool allowoverwrite = false)
        {
            var csproj = Project.Load(path);
            csproj.RemoveNodeValue(group, node);
            if (dry)
            {
                this.Context.Logger.LogInformation(csproj.ToString());
                return;
            }
            Save(csproj, path, output, allowoverwrite);
        }
    }

    [Command("attribute")]
    public class Attribute : ProjectBatchBase
    {
        [Command("get", "get attribute for the group.")]
        public void Get(
            [Option("p", "path of csproj.")]string path,
            [Option("g", "group of nodes. eg. PropertyGroup")]string group,
            [Option("n", "name of node")]string node)
        {
            var results = Project.Load(path).GetAttribute(group, node);

            if (!results.Any()) this.Context.Logger.LogInformation($"node `{node}` not found.");
            foreach (var item in results)
            {
                this.Context.Logger.LogInformation($"{group}: {item}");
            }
        }
        [Command("exists", "check specified attribute is exists.")]
        public void Exists(
            [Option("p", "path of csproj.")]string path,
            [Option("g", "group of nodes. eg. PropertyGroup")]string group,
            [Option("n", "name of node")]string node,
            [Option("a", "attribute of node")]string attribute)
        {
            var item = Project.Load(path).ExistsAttribute(group, node, attribute);
            this.Context.Logger.LogInformation(item.ToString());
        }
        [Command("insert", "insert specified attribute.")]
        public void Insert(
            [Option("p", "path of csproj.")]string path,
            [Option("g", "group of nodes. eg. PropertyGroup")]string group,
            [Option("n", "name of node")]string node,
            [Option("a", "attribute of node")]string attribute,
            [Option("v", "value of attribute")]string value = "",
            bool dry = true,
            string output = "",
            bool allowoverwrite = false)
        {
            var csproj = Project.Load(path);
            csproj.InsertAttribute(group, node, attribute, value, e => !e.HasAttributes);
            if (dry)
            {
                this.Context.Logger.LogInformation(csproj.ToString());
                return;
            }
            Save(csproj, path, output, allowoverwrite);
        }
        [Command("relace", "replace specified attribute with node.")]
        public void Replace(
            [Option("p", "path of csproj.")]string path,
            [Option("g", "group of nodes. eg. PropertyGroup")]string group,
            [Option("n", "name of node")]string node,
            [Option("a", "attribute of node")]string attribute,
            [Option("v", "value of attribute")]string value,
            string pattern,
            string replacement,
            bool dry = true,
            string output = "",
            bool allowoverwrite = false)
        {
            var csproj = Project.Load(path);
            csproj.ReplaceAttribute(group, node, attribute, value, pattern, replacement);
            if (dry)
            {
                this.Context.Logger.LogInformation(csproj.ToString());
                return;
            }
            Save(csproj, path, output, allowoverwrite);
        }

        [Command("remove", "remove specified attribute.")]
        public void Remove(
            [Option("p", "path of csproj.")]string path,
            [Option("g", "group of nodes. eg. PropertyGroup")]string group,
            [Option("n", "name of node")]string node,
            [Option("a", "attribute of node")]string attribute,
            bool dry = true,
            string output = "",
            bool allowoverwrite = false)
        {
            var csproj = Project.Load(path);
            csproj.RemoveAttribute(group, node, attribute);
            if (dry)
            {
                this.Context.Logger.LogInformation(csproj.ToString());
                return;
            }
            Save(csproj, path, output, allowoverwrite);
        }
    }

    [Command("attributevalue")]
    public class AttributeValue : ProjectBatchBase
    {
        [Command("get", "get value for the attribute.")]
        public void Get(
            [Option("p", "path of csproj.")]string path,
            [Option("g", "group of nodes. eg. PropertyGroup")]string group,
            [Option("n", "name of node")]string node,
            [Option("a", "attribute of node")]string attribute)
        {
            var results = Project.Load(path).GetAttributeValue(group, node, attribute);

            if (!results.Any()) this.Context.Logger.LogInformation($"node `{node}` not found.");
            foreach (var item in results)
            {
                this.Context.Logger.LogInformation($"{group}.{node}: {item}");
            }
        }
        [Command("exists", "check specified attribute's value is exists.")]
        public void Exists(
            [Option("p", "path of csproj.")]string path,
            [Option("g", "group of nodes. eg. PropertyGroup")]string group,
            [Option("n", "name of node")]string node,
            [Option("a", "attribute of node")]string attribute,
            [Option("v", "value of attribute")]string value)
        {
            var item = Project.Load(path).ExistsAttributeValue(group, node, attribute, value);
            this.Context.Logger.LogInformation(item.ToString());
        }
        [Command("set", "set specified attribute value.")]
        public void Set(
            [Option("p", "path of csproj.")]string path,
            [Option("g", "group of nodes. eg. PropertyGroup")]string group,
            [Option("n", "name of node")]string node,
            [Option("a", "attribute of node")]string attribute,
            [Option("v", "value of attribute")]string value,
            bool dry = true,
            string output = "",
            bool allowoverwrite = false)
        {
            var csproj = Project.Load(path);
            csproj.SetAttributeValue(group, node, attribute, value);
            if (dry)
            {
                this.Context.Logger.LogInformation(csproj.ToString());
                return;
            }
            Save(csproj, path, output, allowoverwrite);
        }
        [Command("append", "append specified attribute value.")]
        public void Append([Option("p", "path of csproj.")]string path,
            [Option("g", "group of nodes. eg. PropertyGroup")]string group,
            [Option("n", "name of node")]string node,
            [Option("a", "attribute of node")]string attribute,
            [Option("v", "value of attribute")]string value,
            string append = "",
            bool dry = true,
            string output = "",
            bool allowoverwrite = false)
        {
            var csproj = Project.Load(path);
            csproj.AppendAttributeValue(group, node, attribute, value, append);
            if (dry)
            {
                this.Context.Logger.LogInformation(csproj.ToString());
                return;
            }
            Save(csproj, path, output, allowoverwrite);
        }
        [Command("prepend", "preppend specified attribute value.")]
        public void Prepend(
            [Option("p", "path of csproj.")]string path,
            [Option("g", "group of nodes. eg. PropertyGroup")]string group,
            [Option("n", "name of node")]string node,
            [Option("a", "attribute of node")]string attribute,
            [Option("v", "value of attribute")]string value,
            string prepend = "",
            bool dry = true,
            string output = "",
            bool allowoverwrite = false)
        {
            var csproj = Project.Load(path);
            csproj.PrependAttributeValue(group, node, attribute, value, prepend);
            if (dry)
            {
                this.Context.Logger.LogInformation(csproj.ToString());
                return;
            }
            Save(csproj, path, output, allowoverwrite);
        }
        [Command("replace", "replace specified attribute value.")]
        public void Replace(
            [Option("p", "path of csproj.")]string path,
            [Option("g", "group of nodes. eg. PropertyGroup")]string group,
            [Option("n", "name of node")]string node,
            [Option("a", "attribute of node")]string attribute,
            [Option("v", "value of attribute")]string value,
            string pattern,
            string replacement,
            bool dry = true,
            string output = "",
            bool allowoverwrite = false)
        {
            var csproj = Project.Load(path);
            csproj.ReplaceAttributeValue(group, node, attribute, value, pattern, replacement);
            if (dry)
            {
                this.Context.Logger.LogInformation(csproj.ToString());
                return;
            }
            Save(csproj, path, output, allowoverwrite);
        }
        [Command("remove", "remove specified attribute value.")]
        public void Remove([Option("p", "path of csproj.")]string path,
            [Option("g", "group of nodes. eg. PropertyGroup")]string group,
            [Option("n", "name of node")]string node,
            [Option("a", "attribute of node")]string attribute,
            [Option("v", "value of attribute")]string value,
            bool dry = true,
            string output = "",
            bool allowoverwrite = false)
        {
            var csproj = Project.Load(path);
            csproj.RemoveAttributeValue(group, node, attribute, value);
            if (dry)
            {
                this.Context.Logger.LogInformation(csproj.ToString());
                return;
            }
            Save(csproj, path, output, allowoverwrite);
        }
    }
}
