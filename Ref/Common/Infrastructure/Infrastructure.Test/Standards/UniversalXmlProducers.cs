using System;
using System.IO;
using System.Linq;
using System.Xml;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.Infrastructure.Test.Standards
{
	[TestFixture]
	class UniversalXmlProducers
	{
		[Test]
		public void AllTestConfigsHaveSpecialCharactersOnServices()
		{
			var knownNotAllowedAddresses = new[] { @"http://www-refdbrepoupdatetest.sand.wtg.zone", @"http://www-refdbrepotest.sand.wtg.zone" };
			var rootDirectory = Path.Combine(localDirectory, @"..\..");
			var configFiles = Directory.GetFiles(rootDirectory, "*.config", SearchOption.AllDirectories)
				.Where(x => !x.EndsWith("quartz.config")).Select(x => Path.GetFullPath(x)).ToList();
			foreach (var configFile in configFiles)
			{
				var xmlDocument = new XmlDocument();
				xmlDocument.Load(configFile);

				if (xmlDocument != null)
				{
					var appSettings = xmlDocument.GetElementsByTagName("appSettings")?.Item(0);
					if (appSettings != null)
					{
						var configurations = appSettings.ChildNodes;
						foreach (XmlNode config in configurations)
						{
							if (config.Attributes == null)
							{
								continue;
							}
							var value = config.Attributes["value"]?.Value;
							if (value != null)
							{
								foreach (var knowNotAllowedAddress in knownNotAllowedAddresses)
								{
									if (value.StartsWith(knowNotAllowedAddress, StringComparison.InvariantCultureIgnoreCase))
									{
										Assert.Fail($"The configuration file {configFile} has an incorrect setting. The service test url needs an special treatment, check the existent deploy.Test.config files");
									}
								}
							}
						}
					}
				}
			}
		}

		[SetUp]
		public void Setup()
		{
			localDirectory = Path.GetDirectoryName(GetType().Assembly.Location);
			producerProgramsDirectory = Path.Combine(localDirectory, @"..\..\UniversalXMLProducers\");
		}
		string localDirectory;
		string producerProgramsDirectory;
	}
}
