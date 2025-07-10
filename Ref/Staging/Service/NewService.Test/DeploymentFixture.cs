using System.IO;
using System.Linq;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.NewService.Test
{
	[TestFixture]
	internal class DeploymentFixture
	{
		[Test]
		public void PublishFile()
		{
			var publishFiles = Directory.GetFiles(Path.Combine(TestSourcePathHelper.DATTestSupplementaryContentPath, "Staging\\Service\\NewService"), "*.pubxml", SearchOption.AllDirectories);
			Assert.Greater(publishFiles.Length, 5);
			foreach (var publishFile in publishFiles)
			{
				var e = XElement.Load(publishFile);
				var filesToPublish = e.Descendants().Where(x => x.Name.LocalName == "ResolvedFileToPublish");
				Assert.AreEqual(5, filesToPublish.Count(), $"{publishFile} does not have exact ResolvedFileToPublish elements");
				foreach (var fileToPublish in filesToPublish)
				{
					var fileRelativePathToPublish = fileToPublish.Elements().FirstOrDefault(x => x.Name.LocalName == "RelativePath");
					Assert.NotNull(fileRelativePathToPublish, "RelativePath should not null");
					switch (fileRelativePathToPublish.Value)
					{
						case "WTGZone-Root-CA-1.cer":
						case "ApplicationPathsAndDefaultConfigurations.xml":
						case "english.all.3class.distsim.crf.ser.gz":
						case "english.all.3class.distsim.prop":
						case "ConnectionStrings.config.json":
							break;
						default:
							Assert.Fail($"Unregonised {fileRelativePathToPublish.Value}, please add to this test");
							break;

					}
				}
			}
		}
	}
}
