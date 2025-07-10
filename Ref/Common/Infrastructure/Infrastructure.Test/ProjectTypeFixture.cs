using System;
using System.IO;
using System.Xml;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.Infrastructure.Test
{
	[TestFixture]
	public class ProjectTypeFixture
	{
		[Test]
		public void CheckIfAllProjectsAreDotNetFrameworkOrNetCoreStandard()
		{
			var rootPath = TestSourcePathHelper.DATTestSupplementaryContentPath;
			var buildXml = new XmlDocument();
			buildXml.Load(Path.Combine(rootPath, "Build.xml"));

			var nodes = buildXml.GetElementsByTagName("Solution");
			foreach (XmlNode node in nodes)
			{
				var slnFile = Path.Combine(rootPath, node.Attributes["Filename"].Value.Trim());
				AssertProjectType(slnFile);
			}
		}

		static void AssertProjectType(string filePath)
		{
			var contents = File.ReadAllLines(filePath);
			foreach (var line in contents)
			{
				if (!line.Trim().StartsWith("Project(", StringComparison.InvariantCultureIgnoreCase) || !line.Contains(".csproj"))
				{
					continue;
				}
				if (line.Trim().StartsWith("Global", StringComparison.InvariantCultureIgnoreCase))
				{
					break;
				}

				var subLine = line.Substring(line.IndexOf('=') + 1).Trim();
				subLine = subLine.Substring(0, subLine.IndexOf("\",", StringComparison.InvariantCultureIgnoreCase)).Trim();
				var projectType = line.Trim().Substring(line.IndexOf('{'), line.IndexOf('}') - line.IndexOf('{') + 1);
				var projectName = subLine.Replace("\"", "").Trim();

				Assert.That(projectType, Is.EqualTo(DotNetProjectTypeId).Or.EqualTo(NetCoreStandardProjectTypeId), $"C# Project Type should be .NET:{DotNetProjectTypeId} or .NET Core/Standard:{NetCoreStandardProjectTypeId}. Solution:{filePath}, Project:{projectName}");
			}
		}

		const string DotNetProjectTypeId = "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}";
		const string NetCoreStandardProjectTypeId = "{9A19103F-16F7-4668-BE54-9A1E7A4F7556}";
	}
}
