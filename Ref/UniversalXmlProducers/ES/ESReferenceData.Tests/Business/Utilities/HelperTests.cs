using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.ESReferenceData.Business;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ESReferenceData.Tests;

class HelperTests : TestCase
{
	[Test]
	public void ExportToXMLFile()
	{
		var dateProvider = new Mock<IDateTimeProvider>();
		dateProvider.Setup(x => x.CurrentLocalDate).Returns(new DateTime(1996, 2, 26));

		var outputFile = Path.Combine(FileHelper.OutputFolder, "Output_Test_File.xml");
		Helper.ExportToXMLFile("TEST", outputFile, GetXmlWriterConfiguration(), dateProvider.Object.CurrentLocalDate, GetCodeListForTest());

		var expectedXML = TestHelper.ReadManifestResourceContent($"CargoWise.RefDbRepo.ESReferenceData.Tests.Business.Utilities.TestFiles.Output.RefCusCodeListZZ_ES_HelperTest.xml");
		var xml = File.ReadAllText(outputFile);
		Assert.That(xml, Is.EqualTo(expectedXML));
	}

	List<RefCusCodeList> GetCodeListForTest() => [ new () { ZZD_Code = "TEST" } ];

	XmlWriterConfiguration GetXmlWriterConfiguration()
	{
		var writerConfiguration = new XmlWriterConfiguration();
		var codeListConfiguration = new EntityTypeConfiguration<RefCusCodeList>(true);
		codeListConfiguration.IncludeColumn(x => x.ZZD_Code, true);
		codeListConfiguration.IncludeColumnWithConstantValue(x => x.ZZD_StartDate, false, Constants.MinimumDateTime);
		codeListConfiguration.IncludeColumnWithConstantValue(x => x.ZZD_EndDate, false, Constants.MaximumDateTime);
		codeListConfiguration.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, Constants.CountryCode);
		writerConfiguration.IncludeEntityTypeConfiguration(codeListConfiguration);
		return writerConfiguration;
	}

	[Test]
	public void GetDateTime()
	{
		var dateTimeAsString = "25/03/1999";
		var dateFormat = "dd/MM/yyyy";
		var (successfullyParsed, dateTime) = Helper.GetDateTime(dateTimeAsString, dateFormat);

		Assert.That(successfullyParsed, Is.EqualTo(true));
		Assert.That(dateTime, Is.EqualTo(new DateTime(1999, 3, 25)));

		var wrongFormat = "dd-MM-yyyy";
		(successfullyParsed, dateTime) = Helper.GetDateTime(dateTimeAsString, wrongFormat);
		Assert.That(successfullyParsed, Is.EqualTo(false));
		Assert.That(dateTime, Is.EqualTo(DateTime.MinValue));

		var wrongDateTimeAsString = "25_03_1999";
		(successfullyParsed, dateTime) = Helper.GetDateTime(wrongDateTimeAsString, dateFormat);
		Assert.That(successfullyParsed, Is.EqualTo(false));
		Assert.That(dateTime, Is.EqualTo(DateTime.MinValue));
	}

	[Test]
	public void GetDateWithEsFormat()
	{
		var date = new DateTime(1999, 3, 25);
		Assert.That(Helper.GetDateWithEsFormat(date), Is.EqualTo("25-03-1999"));
	}

	protected override string TestClassName => nameof(HelperTests);
}
