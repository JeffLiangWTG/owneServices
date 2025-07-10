using System;
using System.Text;
using NUnit.Framework;
using static CargoWise.RefDbRepo.SharedReferenceData.Services.Common.CommonHelper;

namespace CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models.Tests
{
	[TestFixture]
	sealed class AdditionalCodeTests
	{
		[Test]
		public void ModelValidation()
		{
			var model = new AdditionalCode() { OpType = MetaInfoOpTypes.Created, OpDate = DateTime.Now };
			var errorCollector = new StringBuilder();

			Assert.That(model.IsValid(errorCollector, "UnitTest"), Is.EqualTo(false));
			Assert.That(errorCollector.ToString(),
				Contains.Substring("AdditionalCode validation error")
				.And.Contains("Code is required")
				.And.Contains($"Source: 'UnitTest'"));

			errorCollector.Clear();
			model.Code = "ABC";
			Assert.That(model.IsValid(errorCollector, "UnitTest"), Is.EqualTo(true));
			Assert.That(errorCollector.ToString(), Is.Empty);

			errorCollector.Clear();
			Assert.That(model.IsValidForCreate(errorCollector, "UnitTest"), Is.EqualTo(false));
			Assert.That(errorCollector.ToString(),
				Contains.Substring("AdditionalCode validation error")
				.And.Contains("CodeType is required")
				.And.Contains("Description is required")
				.And.Contains($"Source: 'UnitTest'"));

			errorCollector.Clear();
			model.CodeType = "ABC";
			Assert.That(model.IsValidForCreate(errorCollector, "UnitTest"), Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Does.Not.Contain("CodeType is required"));

			errorCollector.Clear();
			model.Description = "ABC";
			Assert.That(model.IsValidForCreate(errorCollector, "UnitTest"), Is.EqualTo(true));
			Assert.That(errorCollector.ToString(), Is.Empty);
		}

		[Test]
		public void ModelIsInChapter()
		{
			var model = new AdditionalCode();

			Assert.That(model.IsInChapter(null), Is.EqualTo(true));
			Assert.That(model.IsInChapter(string.Empty), Is.EqualTo(true));
			Assert.That(model.IsInChapter("234334"), Is.EqualTo(true));
			Assert.That(model.IsInChapter("CHAPTER DOES NOT MATTER"), Is.EqualTo(true));
		}

		[Test]
		public void CalculatedDates()
		{
			var model = new AdditionalCode();

			Assert.That(model.CalcStartDate, Is.EqualTo(DefaultValues.MinimumDateTime));

			model.StartDate = new DateTime(1800, 2, 3);
			Assert.That(model.CalcStartDate, Is.EqualTo(DefaultValues.MinimumDateTime));

			var dtWithoutSeconds = new DateTime(2022, 9, 14, 15, 16, 0);
			model.StartDate = dtWithoutSeconds;
			Assert.That(model.CalcStartDate, Is.EqualTo(dtWithoutSeconds));

		}
	}
}
