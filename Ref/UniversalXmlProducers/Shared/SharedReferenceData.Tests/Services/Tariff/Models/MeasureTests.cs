using System;
using System.Text;
using NUnit.Framework;
using static CargoWise.RefDbRepo.SharedReferenceData.Services.Common.CommonHelper;

namespace CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models.Tests
{
	[TestFixture]
	sealed class MeasureTests
	{
		[Test]
		public void ModelValidation()
		{
			var model = new Measure() { OpType = MetaInfoOpTypes.Created, OpDate = DateTime.Now };
			var errorCollector = new StringBuilder();

			Assert.That(model.IsValid(errorCollector, "UnitTest"), Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Contains.Substring("Measure validation error.").And.Contains(" Source: 'UnitTest'"));

			errorCollector.Clear();
			model.ItemId = null;
			Assert.That(model.IsValid(errorCollector, "UnitTest"), Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Contains.Substring("A valid ItemId is required"));
			errorCollector.Clear();
			model.ItemId = "12345";
			Assert.That(model.IsValid(errorCollector, "UnitTest"), Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Contains.Substring("A valid ItemId is required"));
			errorCollector.Clear();
			model.ItemId = "ABCDEFGHIJ";
			Assert.That(model.IsValid(errorCollector, "UnitTest"), Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Contains.Substring("A valid ItemId is required"));
			errorCollector.Clear();
			model.ItemId = "1234554321";
			Assert.That(model.IsValid(errorCollector, "UnitTest"), Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Does.Not.Contain("A valid ItemId is required"));

			Assert.That(errorCollector.ToString(), Contains.Substring("RegulationId is required"));
			errorCollector.Clear();
			model.RegulationId = "ABC12345";
			Assert.That(model.IsValid(errorCollector, "UnitTest"), Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Does.Not.Contain("RegulationId is required"));

			Assert.That(errorCollector.ToString(), Contains.Substring("GeographicalArea is required"));
			errorCollector.Clear();
			model.GeographicalArea = "ABC";
			Assert.That(model.IsValid(errorCollector, "UnitTest"), Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Does.Not.Contain("GeographicalArea is required"));

			Assert.That(errorCollector.ToString(), Contains.Substring("MeasureType is required"));
			errorCollector.Clear();
			model.MeasureType = "ABC";
			Assert.That(model.IsValid(errorCollector, "UnitTest"), Is.EqualTo(true));
			Assert.That(errorCollector.ToString(), Does.Not.Contain("MeasureType is required"));
		}

		[Test]
		public void ModelIsInChapter()
		{
			var model = new Measure();
			var chapter = "12";

			Assert.That(model.IsInChapter(chapter), Is.EqualTo(false));
			model.ItemId = null;
			Assert.That(model.IsInChapter(chapter), Is.EqualTo(false));
			model.ItemId = "1234554321";
			Assert.That(model.IsInChapter(chapter), Is.EqualTo(true));
			model.ItemId = "5432112345";
			Assert.That(model.IsInChapter(chapter), Is.EqualTo(false));
		}

		[Test]
		public void CalculatedDates()
		{
			var measure = new Measure();

			Assert.That(measure.CalcStartDate, Is.EqualTo(DefaultValues.MinimumDateTime));
			Assert.That(measure.CalcEndDate, Is.EqualTo(DefaultValues.MaximumDateTime));
			Assert.That(measure.CalcNomenclatureStartDate, Is.EqualTo(DefaultValues.MinimumDateTime));
			Assert.That(measure.CalcNomenclatureEndDate, Is.EqualTo(DefaultValues.MaximumDateTime));

			measure.StartDate = new DateTime(1800, 2, 3);
			measure.EndDate = new DateTime(5050, 04, 05);
			measure.NomenclatureStartDate = new DateTime(1800, 2, 3);
			measure.NomenclatureEndDate = new DateTime(5050, 04, 05);

			Assert.That(measure.CalcStartDate, Is.EqualTo(DefaultValues.MinimumDateTime));
			Assert.That(measure.CalcEndDate, Is.EqualTo(DefaultValues.MaximumDateTime));
			Assert.That(measure.CalcNomenclatureStartDate, Is.EqualTo(DefaultValues.MinimumDateTime));
			Assert.That(measure.CalcNomenclatureEndDate, Is.EqualTo(DefaultValues.MaximumDateTime));

			var dtWithSeconds = new DateTime(2022, 9, 14, 15, 16, 17);
			var dtWithoutSeconds = new DateTime(2022, 9, 14, 15, 16, 0);

			measure.StartDate = dtWithoutSeconds;
			measure.EndDate = dtWithoutSeconds;
			measure.NomenclatureStartDate = dtWithoutSeconds;
			measure.NomenclatureEndDate = dtWithoutSeconds;

			Assert.That(measure.CalcStartDate, Is.EqualTo(dtWithoutSeconds));
			Assert.That(measure.CalcEndDate, Is.EqualTo(dtWithoutSeconds));
			Assert.That(measure.CalcNomenclatureStartDate, Is.EqualTo(dtWithoutSeconds));
			Assert.That(measure.CalcNomenclatureEndDate, Is.EqualTo(dtWithoutSeconds));
		}
	}
}
