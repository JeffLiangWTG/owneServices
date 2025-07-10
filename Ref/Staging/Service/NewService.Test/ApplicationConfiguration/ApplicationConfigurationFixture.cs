using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.NewService.Test
{
	[TestFixture]
	class ApplicationConfigurationFixture
	{
		[Test]
		public void FromXmlToApplicationAttribute()
		{
			var applicationAttributes = xmlToRefApplicationAttribute.GetApplicationAttributes(filePath);

			var first = applicationAttributes.ElementAt(0);
			Assert.That(first.RAA_AttributeName, Is.EqualTo("NexDocRESTReferenceDataEndPoint"));
			Assert.That(first.RAA_ConfigFilePath, Is.EqualTo("CargoWise.RefDbRepo.AUReferenceData.CmdLine.exe.config"));
			Assert.That(first.RAA_Value, Is.EqualTo(@"https://online.agriculture.gov.au/nexdoc-rs/api/public/v1/reference-data"));
			Assert.That(first.RAA_RAT_NKType, Is.EqualTo(""));
			Assert.That(first.RAA_JobGroup, Is.EqualTo("AU Customs"));

			var second = applicationAttributes.ElementAt(1);
			Assert.That(second.RAA_AttributeName, Is.EqualTo("NexDocRESTReferenceDataMaxAttempts"));
			Assert.That(second.RAA_ConfigFilePath, Is.EqualTo("CargoWise.RefDbRepo.AUReferenceData.CmdLine.exe.config"));
			Assert.That(second.RAA_Value, Is.EqualTo("5"));
			Assert.That(second.RAA_RAT_NKType, Is.EqualTo(""));
			Assert.That(first.RAA_JobGroup, Is.EqualTo("AU Customs"));

			var third = applicationAttributes.ElementAt(2);
			Assert.That(third.RAA_AttributeName, Is.EqualTo("NexDocRESTReferenceDataRetryDelay"));
			Assert.That(third.RAA_ConfigFilePath, Is.EqualTo("CargoWise.RefDbRepo.AUReferenceData.CmdLine.exe.config"));
			Assert.That(third.RAA_Value, Is.EqualTo("10000"));
			Assert.That(third.RAA_RAT_NKType, Is.EqualTo(""));
			Assert.That(first.RAA_JobGroup, Is.EqualTo("AU Customs"));
		}

		[Test]
		public void GetApplicationConfigurations()
		{
			var applicationConfigurations = xmlToRefApplicationAttribute.GetApplicationConfigurations(filePath);
			Assert.AreEqual(3, applicationConfigurations.Count());

			var applicationPath = applicationConfigurations.Select(x => x.ApplicationPath).ToList();
			applicationPath.Sort();
			Assert.AreEqual(@"..\UniversalXMLProducers\CargoWise.RefDbRepo.AUReferenceData.CmdLine.exe", applicationPath[0]);
			Assert.AreEqual(@"..\UniversalXMLProducers\CargoWise.RefDbRepo.BEReferenceData.CmdLine.exe", applicationPath[1]);
			Assert.AreEqual(@"..\UniversalXMLProducers\CargoWise.RefDbRepo.BRReferenceData.CmdLine.exe", applicationPath[2]);
		}

		XmlToRefApplicationAttribute xmlToRefApplicationAttribute;
		string filePath;

		[SetUp]
		public void SetUp()
		{
			var binFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			filePath = Path.Combine(binFolder, "ApplicationConfiguration", "ApplicationPathsAndDefaultConfigurations.xml");
			xmlToRefApplicationAttribute = new XmlToRefApplicationAttribute();
		}
	}
}
