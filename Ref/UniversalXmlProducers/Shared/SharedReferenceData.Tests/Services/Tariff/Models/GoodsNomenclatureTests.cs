using System;
using System.Text;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Common;
using Moq;
using NUnit.Framework;
using static CargoWise.RefDbRepo.SharedReferenceData.Services.Common.CommonHelper;

namespace CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models.Tests
{
	[TestFixture]
	sealed class GoodsNomenclatureTests
	{
		[Test]
		public void ModelValidation()
		{
			var model = new GoodsNomenclature() { OpType = MetaInfoOpTypes.Created, OpDate = DateTime.Now };
			var errorCollector = new StringBuilder();

			model.SectionNumber = 1;
			model.Description = "Testing";
			Assert.That(model.IsValid(errorCollector, "UnitTest"), Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Contains.Substring("GoodsNomenclature validation error.").And.Contains("A valid ItemId is required").And.Contains(" Source: 'UnitTest'"));

			errorCollector.Clear();
			model.ItemId = null;
			Assert.That(model.IsValid(errorCollector, "UnitTest"), Is.EqualTo(false));
			model.ItemId = "12345";
			Assert.That(model.IsValid(errorCollector, "UnitTest"), Is.EqualTo(false));
			model.ItemId = "ABCDEFGHIJ";
			Assert.That(model.IsValid(errorCollector, "UnitTest"), Is.EqualTo(false));

			errorCollector.Clear();
			model.ItemId = "1234554321";
			Assert.That(model.IsValid(errorCollector, "UnitTest"), Is.EqualTo(true));
			Assert.That(errorCollector.ToString(), Is.Empty);

			errorCollector.Clear();
			model.SectionNumber = 1;
			model.Description = "";
			Assert.That(model.IsValid(errorCollector, "UnitTest"), Is.EqualTo(true));
			Assert.That(errorCollector.ToString(), Does.Not.Contain("Description is required."));
		}

		[Test]
		public void ModelIsInChapter()
		{
			var model = new GoodsNomenclature();
			var chapter = "12";

			model.SectionNumber = 1;
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
			var dateTimeProvider = new Mock<IDateTimeProvider>();
			var testDate = new DateTime(2020, 12, 28, 12, 13, 14);

			dateTimeProvider.Setup(x => x.UTCDateTime).Returns(testDate);
			dateTimeProvider.Setup(x => x.UTCHistoricalDate).Returns(testDate.AddYears(-1));

			var model = new GoodsNomenclature();
			Assert.That(model.CalcStartDate, Is.EqualTo(CommonHelper.DefaultValues.MinimumDateTime));
			Assert.That(model.CalcEndDate, Is.EqualTo(CommonHelper.DefaultValues.MaximumDateTime));
			Assert.That(model.CalcActiveDate(dateTimeProvider.Object), Is.EqualTo(new DateTime(2020, 12, 28)));

			model.StartDate = new DateTime(2020, 11, 14, 15, 16, 17);
			model.EndDate = new DateTime(2021, 2, 25, 15, 16, 17);

			Assert.That(model.CalcStartDate, Is.EqualTo(new DateTime(2020, 11, 14, 15, 16, 0)));
			Assert.That(model.CalcEndDate, Is.EqualTo(new DateTime(2021, 2, 25, 15, 16, 0)));
			Assert.That(model.CalcActiveDate(dateTimeProvider.Object), Is.EqualTo(new DateTime(2021, 2, 25, 15, 16, 0)));

			model.StartDate = DateTime.MinValue;
			model.EndDate = DateTime.MaxValue;

			Assert.That(model.CalcStartDate, Is.EqualTo(CommonHelper.DefaultValues.MinimumDateTime));
			Assert.That(model.CalcEndDate, Is.EqualTo(CommonHelper.DefaultValues.MaximumDateTime));
			Assert.That(model.CalcActiveDate(dateTimeProvider.Object), Is.EqualTo(CommonHelper.DefaultValues.MaximumDateTime));
		}
	}
}
