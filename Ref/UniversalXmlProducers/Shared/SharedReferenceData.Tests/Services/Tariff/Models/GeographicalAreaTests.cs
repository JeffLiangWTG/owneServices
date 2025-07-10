using System;
using System.Text;
using NUnit.Framework;
using static CargoWise.RefDbRepo.SharedReferenceData.Services.Common.CommonHelper;

namespace CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models.Tests
{
	[TestFixture]
	sealed class GeographicalAreaTests
	{
		[Test]
		public void ModelValidation()
		{
			var model = new GeographicalArea() { OpType = MetaInfoOpTypes.Created, OpDate = new DateTime() };
			var errorCollector = new StringBuilder();

			Assert.That(model.IsValid(errorCollector, "UnitTest"), Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Contains.Substring("GeographicalArea validation error.")
				.And.Contains("GeographicalAreaId is required")
				.And.Contains(" Source: 'UnitTest'"));

			errorCollector.Clear();
			model.GeographicalAreaId = "12345";
			Assert.That(model.IsValid(errorCollector, "UnitTest"), Is.EqualTo(true));
			Assert.That(errorCollector.ToString(), Is.Empty);

			errorCollector.Clear();
			Assert.That(model.IsValidForCreate(errorCollector, "UnitTest"), Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Contains.Substring("GeographicalArea validation error.")
				.And.Contains("Description is required")
				.And.Contains(" Source: 'UnitTest'"));

			errorCollector.Clear();
			model.Description = "ABC 123";
			Assert.That(model.IsValidForCreate(errorCollector, "UnitTest"), Is.EqualTo(true));
			Assert.That(errorCollector.ToString(), Does.Not.Contain("Description is required"));

			errorCollector.Clear();
			var country = new GeographicalArea.GeographicalAreaCountry { HJID = "1" };
			model.SetCountries(new[] { country });
			Assert.That(model.IsValidForCreate(errorCollector, "UnitTest"), Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Contains.Substring("GeographicalAreaHjid is required."));

			errorCollector.Clear();
			country.GeographicalAreaHjid = "12345";
			Assert.That(model.IsValidForCreate(errorCollector, "UnitTest"), Is.EqualTo(true));
			Assert.That(errorCollector.ToString(), Is.Empty);
		}

		[Test]
		public void ModelIsInChapter()
		{
			var model = new GeographicalArea();

			Assert.That(model.IsInChapter(null), Is.EqualTo(true));
			Assert.That(model.IsInChapter(string.Empty), Is.EqualTo(true));
			Assert.That(model.IsInChapter("234334"), Is.EqualTo(true));
			Assert.That(model.IsInChapter("CHAPTER DOES NOT MATTER"), Is.EqualTo(true));
		}

		[Test]
		public void CalculatedDates()
		{
			var model = new GeographicalArea();

			Assert.That(model.CalcStartDate, Is.EqualTo(DefaultValues.MinimumDateTime));
			Assert.That(model.CalcEndDate, Is.EqualTo(DefaultValues.MaximumDateTime));

			model.StartDate = new DateTime(1800, 2, 3);
			model.EndDate = new DateTime(5050, 04, 05);

			Assert.That(model.CalcStartDate, Is.EqualTo(DefaultValues.MinimumDateTime));
			Assert.That(model.CalcEndDate, Is.EqualTo(DefaultValues.MaximumDateTime));

			var dtWithSeconds = new DateTime(2022, 9, 14, 15, 16, 17);
			var dtWithoutSeconds = new DateTime(2022, 9, 14, 15, 16, 0);

			model.StartDate = dtWithSeconds;
			model.EndDate = dtWithSeconds;

			Assert.That(model.CalcStartDate, Is.EqualTo(dtWithoutSeconds));
			Assert.That(model.CalcEndDate, Is.EqualTo(dtWithoutSeconds));
		}
	}
}
