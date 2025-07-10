using System;
using System.Globalization;
using System.IO;
using System.Xml.Linq;
using CargoWise.RefDbRepo.PLReferenceData.Business.Helpers;
using CargoWise.RefDbRepo.PLReferenceData.Business.Quota;
using CargoWise.RefDbRepo.PLReferenceData.Services.Interfaces;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.PLReferenceData.Tests.Quota;

[TestFixture]
sealed class QuotaUniversalReferenceDataXmlGeneratorTest
{
	[Test]
	public void TestGenerateUniversalTariff_RefCusQuota()
	{
		CultureInfo.CurrentCulture = new CultureInfo("en-US", false);
		var dateTimeProviderMock = Mock.Of<IDateTimeProvider>(x => x.CurrentLocalDateTime == new DateTime(2021, 1, 1));
		var rawInputData = XDocument.Load(TestHelper.GetManifestResourceStream("CargoWise.RefDbRepo.PLReferenceData.Tests.Quota.TestFiles.Input.singleRefCusQuota.xml"));
		var parsedInputData = XmlParser.Deserialize<IsztarHistoryResponse>(rawInputData);

		var resultData = PLQuotaExtractor.GeneratePLQuotaData(parsedInputData, dateTimeProviderMock);
		var tempOutput = Path.GetTempFileName();
		QuotaUniversalReferenceDataXmlGenerator.GenerateReferenceDataXml(new DateTime(2010, 11, 11), resultData, tempOutput);

		var expected = XDocument.Load(TestHelper.GetManifestResourceStream("CargoWise.RefDbRepo.PLReferenceData.Tests.Quota.TestFiles.Output.TestGenerateRefCusQuota.xml"));
		var result = XDocument.Load(tempOutput);

		Assert.AreEqual(expected.ToString(), result.ToString());
	}
}
