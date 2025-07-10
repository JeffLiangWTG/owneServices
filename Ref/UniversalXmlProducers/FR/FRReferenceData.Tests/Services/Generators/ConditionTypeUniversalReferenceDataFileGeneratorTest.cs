using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.FRReferenceData.Services;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.FRReferenceData.Tests.Generators
{
	[TestFixture]
	class ConditionTypeUniversalReferenceDataFileGeneratorTest
	{
		[Test]
		public void TesNoExceptionThrownIfNoData()
		{
			UniversalDataHelper.SentEmails.Clear();

			var generator = new ConditionTypeUniversalReferenceDataFileGeneratorForTest_NoData();
			var error = new Errors();
			Assert.DoesNotThrow(() => generator.GenerateFiles(new DateTime(2022, 03, 01), ref error));
			Assert.That(UniversalDataHelper.SentEmails.Count == 1);
			Assert.That(UniversalDataHelper.SentEmails.Contains(new Email
			{
				From = "donotreply_refservice@wisetechglobal.com",
				To = ApplicationConfig.Instance.EmailRecipients,
				Subject = "No FR - Condition Types information was found.",
				Body = "RITA web site didn't return any result for condition types. You can manually check for Experts/Donnéees de référence/Type de measures on RITA web site."
			}));
		}

		[Test]
		public void TestGenerateFiles()
		{
			ApplicationConfig.Instance.DownloadDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\");
			ApplicationConfig.Instance.OutputDirectory = Path.GetTempPath();

			var expectedOutputFile = Path.Combine(ApplicationConfig.Instance.OutputDirectory, "FRConditionTypes.xml");
			File.Delete(expectedOutputFile);

			var generator = new ConditionTypeUniversalReferenceDataFileGeneratorForTest();
			var error = new Errors();
			generator.GenerateFiles(new DateTime(2022, 03, 01), ref error);

			Assert.IsTrue(File.Exists(expectedOutputFile));
			var outputFileContent = File.ReadAllText(expectedOutputFile);
			Assert.IsTrue(outputFileContent.Contains("<Schema>"));
			Assert.IsTrue(outputFileContent.Contains("<DataSource>FR - Condition Types</DataSource>"));
			Assert.IsTrue(outputFileContent.Contains("<UpdateType>Partial</UpdateType>"));
			Assert.IsTrue(outputFileContent.Contains(@"
  <Schema>
    <EntityType Name=""RefCusConditionType"" Data=""true"">
      <Key>
        <PropertyRef Name=""ZX2_ConditionType"" />
        <PropertyRef Name=""ZX2_ZZZ_NKDataGrouping"" />
      </Key>
      <Property Name=""ZX2_ConditionClass"" Type=""varchar"" MaxLength=""5"" ConstantValue=""CTRL"" />
      <Property Name=""ZX2_ConditionType"" Type=""varchar"" MaxLength=""6"" />
      <Property Name=""ZX2_Description"" Type=""nvarchar"" MaxLength=""500"" />
      <Property Name=""ZX2_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""FR"" />
    </EntityType>
  </Schema>"));
			Assert.IsTrue(outputFileContent.Contains(@"
  <RefCusConditionType>
    <ZX2_ConditionType>AAN</ZX2_ConditionType>
    <ZX2_Description>Alimentation animale</ZX2_Description>
  </RefCusConditionType>"));
			Assert.IsTrue(outputFileContent.Contains(@"
  <RefCusConditionType>
    <ZX2_ConditionType>TLP</ZX2_ConditionType>
    <ZX2_Description>Accises tabacs restriction mise en libre pratique</ZX2_Description>
  </RefCusConditionType>"));
		}
	}

	class ConditionTypeUniversalReferenceDataFileGeneratorForTest : ConditionTypeUniversalReferenceDataFileGenerator
	{
		protected override List<RefCusConditionType> GetDataCollection(DateTime publicationDate, ref Errors error)
		{
			var downloaderMock = new Mock<RITADataDownloader>();
			downloaderMock.Setup(x => x.GetFRConditionTypesWebPage()).Returns(File.ReadAllText(Path.Combine(ApplicationConfig.Instance.DownloadDirectory, "FRConditionTypesWebPage.html")));

			var provider = new RITADataProvider(downloaderMock.Object);
			var conditionTypes = provider.GetFRConditionTypes();

			if (!conditionTypes.Any())
			{
				Console.WriteLine("No condition type data could be downloaded from Customs. End of process");
				error = Errors.DownloadErr;
			}
			return conditionTypes;
		}
	}

	class ConditionTypeUniversalReferenceDataFileGeneratorForTest_NoData : ConditionTypeUniversalReferenceDataFileGenerator
	{
		protected override List<RefCusConditionType> GetDataCollection(DateTime publicationDate, ref Errors error) => new List<RefCusConditionType>();
	}
}
