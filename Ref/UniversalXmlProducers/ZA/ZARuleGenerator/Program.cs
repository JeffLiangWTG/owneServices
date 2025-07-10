using System;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ZARuleGenerator
{
	class Program
	{
		static void Main(string[] args)
		{
			var outputPath = Configuration["OutputPath"];
			var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

			var sourcePath = new DirectoryInfo(Path.Combine(binPath, "Res"));
			var res = sourcePath.GetFiles("*Rules.xml");

			if (!Directory.Exists(outputPath))
			{
				Directory.CreateDirectory(outputPath);
			}

			foreach (var f in res)
			{
				var xDoc = XDocument.Load(f.FullName);
				var publishDate = xDoc.Root.Element(XName.Get("PublicationTime"));
				if (publishDate != null)
				{
					publishDate.Value = DateTime.UtcNow.ToString("s");
				}
				xDoc.Save(Path.Combine(outputPath, Path.GetFileName(f.Name)));
			}
		}

		static IConfiguration Configuration
		{
			get
			{
				if (configuration == null)
				{
					var configurationBuilder = new ConfigurationBuilder();
					configuration = configurationBuilder.Build();
				}
				return configuration;
			}
		}
		static IConfiguration configuration;
	}
}
