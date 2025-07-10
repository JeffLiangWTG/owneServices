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
	sealed class MeasureConditionCodeLoaderTests : BaseLoaderTests<MeasureConditionCode>
	{
		public override void ConvertXmlElementToModelAsserts(MeasureConditionCode model)
		{
			Assert.That(model.HJID, Is.EqualTo("74523"));
			Assert.That(model.Id, Is.EqualTo("A"));
			Assert.That(model.Description, Is.EqualTo("Presentation of an anti-dumping/countervailing document"));
			Assert.That(model.StartDate, Is.EqualTo(new DateTime(1995, 01, 01)));
			Assert.That(model.EndDate.Value, Is.EqualTo(new DateTime(2050, 12, 31, 11, 12, 13)));
		}

		public override void LoadDataAsserts(List<MeasureConditionCode> models, string errors)
		{
			Assert.That(models.Count, Is.EqualTo(3));
			Assert.That(errors,
				Contains.Substring("ConditionCode is required")
				.And.Contains("UT_LoaderTests_002.xml"));

			Assert.That(models.Any(x => x.Id == "A"), Is.True);
			Assert.That(models.Any(x => x.Id == "B"), Is.True);
			Assert.That(models.Any(x => x.Id == "C"), Is.True);
			Assert.That(models.Any(x => string.IsNullOrEmpty(x.Id)), Is.False);
		}

		public override void ModelIsValidCore(MeasureConditionCode model, StringBuilder errorCollector, string source)
		{
			var result = model.IsValid(errorCollector, source);

			Assert.That(result, Is.EqualTo(false));
			Assert.That(errorCollector.ToString(),
				Contains.Substring("MeasureConditionCode validation error")
				.And.Contains("ConditionCode is required")
				.And.Contains("Description is required")
				.And.Contains($"Source: '{source}'"));

			errorCollector.Clear();
			model.Id = "A";
			Assert.That(model.IsValid(errorCollector, source), Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Is.Not.Empty);

			errorCollector.Clear();
			model.Description = "Condition Code";
			Assert.That(model.IsValid(errorCollector, source), Is.EqualTo(true));
			Assert.That(errorCollector.ToString(), Is.Empty);
		}

		public override ITestLoader<MeasureConditionCode> CreateTestLoader() => new MeasureConditionCodeLoaderTester();
		class MeasureConditionCodeLoaderTester : MeasureConditionCodeLoader, ITestLoader<MeasureConditionCode>
		{
			string ITestLoader<MeasureConditionCode>.XmlElementName => base.ElementName;

			MeasureConditionCode ITestLoader<MeasureConditionCode>.ConvertXElementToModel(XElement element)
			{
				var model = CreateMinimumModelFromXElement(element);
				LoadMetaInfo(model, element);
				model.ProcessUpdate(element);
				return model;
			}

			List<MeasureConditionCode> ITestLoader<MeasureConditionCode>.LoadData(List<FileDetails> files, StringBuilder errorCollector) => base.LoadData(files, errorCollector).Cast<MeasureConditionCode>().ToList();
		}
	}
}
