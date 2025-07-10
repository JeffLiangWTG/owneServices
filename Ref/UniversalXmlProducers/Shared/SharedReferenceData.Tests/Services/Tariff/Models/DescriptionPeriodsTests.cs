using System;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.SharedReferenceData.Tests;
using NUnit.Framework;
using static CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models.DescriptionPeriods;

namespace CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models.Tests
{
	[TestFixture]
	class DescriptionPeriodsTests
	{
		[TestCase(0, 1, "1980-1-1T12:13:00", 1, "Original description")]
		[TestCase(1, 1, "1980-1-1T12:13:00", 1, "New description")]
		[TestCase(2, 1, "1982-1-1T10:20:00", 1, "Original description")]
		[TestCase(3, 2, "1984-3-2T01:00:00", 1, "Description for new period")]
		public void ProcessUpdate(int skip, int periodCount, string periodStartDate, int descriptionCount, string description)
		{
			var instance = new DescriptionPeriods("test");
			instance.SetDescriptions(new[] { new DescriptionModel { HJID = "9999", Description = "Original description" } });

			var element = TestHelper.GetXmlElement("Test", "CargoWise.RefDbRepo.SharedReferenceData.Tests.Services.Tariff.TestFiles.Input.UT_DescriptionPeriods.xml", skip);
			instance.ProcessUpdate(element);

			var periods = instance.Values.ToList();
			Assert.That(periods, Has.Count.EqualTo(periodCount));
			Assert.That(periods[periodCount - 1].StartDate, Is.EqualTo(DateTime.Parse(periodStartDate, CultureInfo.InvariantCulture)));

			var descriptions = periods[periodCount - 1].Descriptions.ToList();
			Assert.That(descriptions, Has.Count.EqualTo(descriptionCount));
			Assert.That(descriptions[descriptionCount - 1].Description, Is.EqualTo(description));
		}
	}
}
