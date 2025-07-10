using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.GBReferenceData.Services.CDSPortData.Config;
using NUnit.Framework;
using static CargoWise.RefDbRepo.GBReferenceData.Tests.CDSPortData.TestHelperClasses;

namespace CargoWise.RefDbRepo.GBReferenceData.Tests.CDSPortData
{
	[TestFixture]
	public class DataProcessorTests
	{
		[Test]
		public void ConvertContent()
		{
			var source = new CDSPortSource { Code = "ABC", CodeColumn = 0, DescriptionColumns = new[] { 1, 2 }, AdditionalInfoColumn = 3 };
			var processor = new VirtualDataProcessor(source);
			var models = processor.Extract().ToList();

			Assert.That(models, Is.Not.Null);
			Assert.That(models.Count, Is.EqualTo(4));
			Assert.That(models[0].Code, Is.EqualTo("A1"));
			Assert.That(models[0].Description, Is.EqualTo("A2 A3"));
			Assert.That(models[0].AdditionalInfo, Is.EqualTo("A4"));

			Assert.That(models[1].Code, Is.EqualTo("B1"));
			Assert.That(models[2].Code, Is.EqualTo("C1"));
			Assert.That(models[3].Code, Is.EqualTo("VALID"));
		}

		[Test]
		public void CodeColumn()
		{
			var source = new CDSPortSource { Code = "ABC", CodeColumn = 0, DescriptionColumns = new[] { 1, 2 }, AdditionalInfoColumn = 3 };
			var processor = new VirtualDataProcessor(source);
			var models = processor.Extract().ToList();
			Assert.That(models, Is.Not.Null);
			Assert.That(models[0].Code, Is.EqualTo("A1"));

			source.CodeColumn = 2;
			models = processor.Extract().ToList();
			Assert.That(models, Is.Not.Null);
			Assert.That(models[0].Code, Is.EqualTo("A3"));
		}

		[Test]
		public void DescriptionColumns()
		{
			var source = new CDSPortSource { Code = "ABC", CodeColumn = 0, DescriptionColumns = new[] { 1, 2 }, AdditionalInfoColumn = 3 };
			var processor = new VirtualDataProcessor(source);
			var models = processor.Extract().ToList();
			Assert.That(models, Is.Not.Null);
			Assert.That(models[0].Description, Is.EqualTo("A2 A3"));

			source.DescriptionColumns = new[] { 4, 3, 2, 1, 0, -1 };
			models = processor.Extract().ToList();
			Assert.That(models, Is.Not.Null);
			Assert.That(models[0].Description, Is.EqualTo("A4 A3 A2 A1"));
		}

		[Test]
		public void AdditionalInfoColumn()
		{
			var source = new CDSPortSource { Code = "ABC", CodeColumn = 0, DescriptionColumns = new[] { 1, 2 }, AdditionalInfoColumn = 3 };
			var processor = new VirtualDataProcessor(source);
			var models = processor.Extract().ToList();
			Assert.That(models, Is.Not.Null);
			Assert.That(models[0].AdditionalInfo, Is.EqualTo("A4"));

			source.AdditionalInfoColumn = 0;
			models = processor.Extract().ToList();
			Assert.That(models, Is.Not.Null);
			Assert.That(models[0].AdditionalInfo, Is.EqualTo("A1"));

			source.AdditionalInfoColumn = -1;
			models = processor.Extract().ToList();
			Assert.That(models, Is.Not.Null);
			Assert.That(models[0].AdditionalInfo, Is.EqualTo(""));
		}

		[Test]
		public void NoContent()
		{
			var source = new CDSPortSource { Code = "ABC", CodeColumn = 0, DescriptionColumns = new[] { 1, 2 }, AdditionalInfoColumn = 3 };
			var processor = new VirtualDataProcessor(source);
			processor.TestHasContent = false;
			var models = processor.Extract().ToList();

			Assert.That(models, Is.Not.Null);
			Assert.That(models.Count, Is.EqualTo(0));

			processor.TestHasContent = true;
			processor.TestReadContent = new List<string[]>();
			models = processor.Extract().ToList();

			Assert.That(models, Is.Not.Null);
			Assert.That(models.Count, Is.EqualTo(0));
		}

		[Test]
		public void ValidateLine()
		{
			var source = new CDSPortSource { Code = "ABC", CodeColumn = 0, DescriptionColumns = new[] { 1, 2 }, AdditionalInfoColumn = 3 };
			var processor = new VirtualDataProcessor(source);
			var models = processor.Extract().ToList();
			Assert.That(models, Is.Not.Null);
			Assert.That(models.Any(x => x.Code == "VALID"), "Expecting VALID");

			source.CodeColumn = 1;
			models = processor.Extract().ToList();
			Assert.That(models, Is.Not.Null);
			Assert.That(!models.Any(x => x.Code == "INVALID ITEM"), "Not expecting INVALID ITEM");
		}
	}
}
