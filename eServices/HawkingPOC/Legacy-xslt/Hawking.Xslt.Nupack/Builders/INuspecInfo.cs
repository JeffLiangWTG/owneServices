namespace Hawking.Xslt.Nupack.Builders
{
    public interface INuspecInfo
    {
        string Id { get; set; }
        string Version { get; set; }
        string Title { get; set; }
        string Authors { get; set; }
        string Owners { get; set; }
        bool RequireLicenseAcceptance { get; set; }
        string Description { get; set; }
        string Copyright { get; set; }
        string TargetFramework { get; set; }

        void Save(string filePath);
    }
}
