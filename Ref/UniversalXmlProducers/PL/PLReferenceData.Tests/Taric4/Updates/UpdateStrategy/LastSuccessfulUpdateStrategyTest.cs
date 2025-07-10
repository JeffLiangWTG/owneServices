using NUnit.Framework;
using System.Globalization;
using System.IO;
using System;
using System.Linq;
using CargoWise.RefDbRepo.PLReferenceData.Business.Taric4.Updates.UpdateStrategy;

namespace CargoWise.RefDbRepo.PLReferenceData.Tests.Taric4.Updates.UpdateStrategy;

[TestFixture]
sealed class LastSuccessfulUpdateStrategyTest
{
	[TestCase(null, false, "file does not exist")]
	[TestCase(" ", false, "file is empty")]
	[TestCase("2024-01-13ABC", false, "file contains invalid data")]
	[TestCase("14/01/2024 11:32:57", false, "file contains pl-PL date time format")]
	[TestCase("01/15/2024 11:32:57 AM", true, "file contains en-US date time format")]
	[TestCase("2024-01-16T22:33:44", true, "file contains GregorianCalendar data")]
	[TestCase("2024-01-17", true, "file contains date")]
	[TestCase("2024-01-18T01:02:03", true, "file contains date time")]
	public void TestGetLastSuccessfulExecutionDate(string fileContent, bool contentIsValidData, string message)
	{
		CultureInfo.CurrentCulture = new CultureInfo("en-US", false);

		var tempFile = "nonExistingFile.txt";

		if (fileContent is not null)
		{
			tempFile = Path.GetTempFileName();
			File.WriteAllText(tempFile, fileContent);
		}

		var result = new LastSuccessfulUpdateStrategy(tempFile).FromDate;

		var expected = contentIsValidData
			? DateTime.Parse(fileContent, CultureInfo.CurrentCulture).Date
			: DateTime.UtcNow.AddDays(-1).Date;

		Assert.AreEqual(expected, result, message);

		if (File.Exists(tempFile))
		{
			File.Delete(tempFile);
		}
	}

	[Test]
	public void TestUpdateLastSuccessfulUpdateDate()
	{
		CultureInfo.CurrentCulture = new CultureInfo("en-US", false);

		var tempFile = Path.GetTempFileName();

		var expectedDate = DateTime.UtcNow.Date;
		new LastSuccessfulUpdateStrategy(tempFile).OnSuccessfulUpdate();

		var result = File.ReadAllLines(tempFile).First();
		Assert.AreEqual(expectedDate, DateTime.Parse(result, CultureInfo.CurrentCulture).Date);

		if (File.Exists(tempFile))
		{
			File.Delete(tempFile);
		}
	}

}
