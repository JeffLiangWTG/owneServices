using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Loaders.Tests
{
	[TestFixture]
	sealed class MeasureTypeTests : BaseLoaderTests<MeasureType>
	{
		public override void ConvertXmlElementToModelAsserts(MeasureType model)
		{
			Assert.That(model.HJID, Is.EqualTo("10940"));
			Assert.That(model.Id, Is.EqualTo("123"));
			Assert.That(model.TradeMovementCode, Is.EqualTo("0"));
			Assert.That(model.MeasureTypeSeries, Is.EqualTo("A"));
			Assert.That(model.Description, Is.EqualTo("Measure Type Unit Test"));
			Assert.That(model.StartDate, Is.EqualTo(new DateTime(1991, 03, 08)));
			Assert.That(model.EndDate.Value, Is.EqualTo(new DateTime(2050, 12, 31, 11, 12, 13)));
		}

		public override void LoadDataAsserts(List<MeasureType> models, string errors)
		{
			Assert.That(models.Count, Is.EqualTo(3));
			Assert.That(errors,
				Contains.Substring("MeasureTypeId is required")
				.And.Contains("UT_LoaderTests_002.xml"));

			Assert.That(models.Any(x => x.Id == "123"), Is.True);
			Assert.That(models.Any(x => x.Id == "142"), Is.True);
			Assert.That(models.Any(x => x.Id == "278"), Is.True);
			Assert.That(models.Any(x => string.IsNullOrEmpty(x.Id)), Is.False);
		}

		public override void ModelIsValidCore(MeasureType model, StringBuilder errorCollector, string source)
		{
			var result = model.IsValid(errorCollector, source);

			Assert.That(result, Is.EqualTo(false));
			Assert.That(errorCollector.ToString(),
				Contains.Substring("MeasureType validation error")
				.And.Contains("MeasureTypeId is required")
				.And.Contains("Description is required")
				.And.Contains($"Source: '{source}'"));

			errorCollector.Clear();
			model.Id = "ABC";
			Assert.That(model.IsValid(errorCollector, source), Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Is.Not.Empty);

			errorCollector.Clear();
			model.Description = "Measurey Type";
			Assert.That(model.IsValid(errorCollector, source), Is.EqualTo(true));
			Assert.That(errorCollector.ToString(), Is.Empty);
		}

		public override ITestLoader<MeasureType> CreateTestLoader() => new MeasureTypeLoaderTester();
		class MeasureTypeLoaderTester : MeasureTypeLoader, ITestLoader<MeasureType>
		{
			string ITestLoader<MeasureType>.XmlElementName => base.ElementName;

			MeasureType ITestLoader<MeasureType>.ConvertXElementToModel(XElement element)
			{
				var model = CreateMinimumModelFromXElement(element);
				LoadMetaInfo(model, element);
				model.ProcessUpdate(element);
				return model;
			}

			List<MeasureType> ITestLoader<MeasureType>.LoadData(List<FileDetails> files, StringBuilder errorCollector) => base.LoadData(files, errorCollector).Cast<MeasureType>().ToList();
		}
	}
}
