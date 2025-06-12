namespace CargoWise.eHub.DataAccess.Integration
{
    public struct ContextProperty
    {
        public ContextProperty(string name, string uri, string value)
        {
            Name = name;
            Namespace = uri;
            Value = value;
        }

        public readonly string Name;
        public readonly string Namespace;
        public readonly string Value;
    }
}
