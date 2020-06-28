namespace CsProjEditor
{
    public class CsProjAttribute
    {
        public string Name { get; }
        public string Value { get; set; }
        public CsProjAttribute(string name) => (Name, Value) = (name, null);
        public CsProjAttribute(string name, string value) => (Name, Value) = (name, value);
    }
}
