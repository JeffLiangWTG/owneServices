using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.RefDbRepo.Common.Argument;
using YamlDotNet.RepresentationModel;

namespace CargoWise.RefDbRepo.EdmxGen
{
	public static class YamlHelper
	{
		public static YamlMappingNode GetYamlMapping(string path)
		{
			Argument.NotNullOrEmpty(path, nameof(path));

			using (var reader = new StreamReader(GetYamlAsStream(path)))
			{
				var yaml = new YamlStream();
				yaml.Load(reader);

				var document = yaml.Documents?.FirstOrDefault();

				return (YamlMappingNode)document.RootNode;
			}
		}

		static Stream GetYamlAsStream(string path)
		{
			var getStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(Assembly.GetExecutingAssembly().GetManifestResourceNames().FirstOrDefault(o => o.EndsWith(path, System.StringComparison.OrdinalIgnoreCase)));
			return getStream;
		}

		public static YamlMappingNode ToYamlMappingNode(this YamlNode node)
		{
			return (YamlMappingNode)node;
		}

		public static YamlSequenceNode ToYamlSequenceNode(this YamlNode node)
		{
			return (YamlSequenceNode)node;
		}
	}
}
