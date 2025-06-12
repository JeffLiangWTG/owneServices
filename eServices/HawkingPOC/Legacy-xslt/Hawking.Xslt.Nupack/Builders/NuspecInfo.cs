using System;
using System.Xml.Linq;
using Microsoft.Extensions.Configuration;

namespace Hawking.Xslt.Nupack.Builders
{
    public class NuspecInfo : INuspecInfo
    {
        public NuspecInfo()
        {
            Version = "1.0.0";
            Authors = "WTG eServices";
            Owners = "WiseTech Global";
            Copyright = $"Copyright ©{DateTime.Now.ToString("yyyy")} WiseTech Global";
            TargetFramework = "NETCoreApp2.1";
        }

        public NuspecInfo(IConfigurationRoot iconfigurationRoot) : this()
        {
            TargetFramework = iconfigurationRoot["TargetFramework"];
        }

        public string Id { get; set; }
        public string Version { get; set; }
        public string Title { get; set; }
        public string Authors { get; set; }
        public string Owners { get; set; }
        public bool RequireLicenseAcceptance { get; set; }
        public string Description { get; set; }
        public string Copyright { get; set; }
        public string TargetFramework { get; set; }

        public void Save(string filePath)
        {
            var document = new XDocument();
            var package = new XElement("package");
            var metadata = new XElement("metadata");
            package.Add(metadata);
            document.Add(package);

            metadata.Add(new XElement(nameof(Id).ToLower(), Id));
            metadata.Add(new XElement(nameof(Version).ToLower(), Version));
            metadata.Add(new XElement(nameof(Title).ToLower(), Title));
            metadata.Add(new XElement(nameof(Authors).ToLower(), Authors));
            metadata.Add(new XElement(nameof(Owners).ToLower(), Owners));
            metadata.Add(new XElement(nameof(RequireLicenseAcceptance).ToLower(), RequireLicenseAcceptance));
            metadata.Add(new XElement(nameof(Description).ToLower(), Description));
            metadata.Add(new XElement(nameof(Copyright).ToLower(), Copyright));

            var dependencies = new XElement("dependencies");
            metadata.Add(dependencies);

            var groupTargetFramework = new XElement("group");
            groupTargetFramework.SetAttributeValue("targetFramework", TargetFramework);
            dependencies.Add(groupTargetFramework);

            document.Save(filePath);
        }
    }
}
