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
	sealed class BaseRegulationTests : BaseLoaderTests<BaseRegulation>
	{
		public override void LoadDataAsserts(List<BaseRegulation> models, string errors)
		{
			Assert.That(models.Count, Is.EqualTo(3));
			Assert.That(errors,
				Contains.Substring("RegulationId is required")
				.And.Contains("UT_LoaderTests_002.xml"));

			Assert.That(models.Any(x => x.RegulationId == "BR000001"), Is.True);
			Assert.That(models.Any(x => x.RegulationId == "BR000002"), Is.True);
			Assert.That(models.Any(x => x.RegulationId == "C9603440"), Is.True);
			Assert.That(models.Any(x => string.IsNullOrEmpty(x.RegulationId)), Is.False);
		}

		public override void ModelIsValidCore(BaseRegulation model, StringBuilder errorCollector, string source)
		{
			var result = model.IsValid(errorCollector, source);

			Assert.That(result, Is.EqualTo(false));
			Assert.That(errorCollector.ToString(),
				Contains.Substring("BaseRegulation validation error")
				.And.Contains("RegulationId is required")
				.And.Contains("StartDate is required")
				.And.Contains($"Source: '{source}'"));

			errorCollector.Clear();
			model.RegulationId = "ABC";
			Assert.That(model.IsValid(errorCollector, source), Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Is.Not.Empty);

			errorCollector.Clear();
			model.StartDate = new DateTime();
			Assert.That(model.IsValid(errorCollector, source), Is.EqualTo(true));
			Assert.That(errorCollector.ToString(), Is.Empty);
		}

		public override void ConvertXmlElementToModelAsserts(BaseRegulation model)
		{
			Assert.That(model.HJID, Is.EqualTo("1000"));
			Assert.That(model.RegulationId, Is.EqualTo("BR000001"));
			Assert.That(model.RegulationRoleTypeId, Is.EqualTo("1"));
			Assert.That(model.StartDate, Is.EqualTo(new DateTime(1991, 03, 08)));
			Assert.That(model.EndDate.Value, Is.EqualTo(new DateTime(1991, 12, 31, 11, 12, 13)));
			Assert.That(model.EffectiveEndDate.Value, Is.EqualTo(new DateTime(1991, 12, 31, 23, 59, 59)));

			Assert.That(model.CompleteAbrogationRegulationId, Is.EqualTo("CAR00001"));
			Assert.That(model.ExplicitAbrogationRegulationId, Is.EqualTo("EAR00001"));
			Assert.That(model.RelatedAntidumpingRegulationId, Is.EqualTo("RADR0001"));
			Assert.That(model.ReplacementIndicator, Is.EqualTo(1));
		}

		public override ITestLoader<BaseRegulation> CreateTestLoader() => new BaseRegulationLoaderTester();
		class BaseRegulationLoaderTester : BaseRegulationLoader, ITestLoader<BaseRegulation>
		{
			string ITestLoader<BaseRegulation>.XmlElementName => base.ElementName;

			BaseRegulation ITestLoader<BaseRegulation>.ConvertXElementToModel(XElement element)
			{
				var model = CreateMinimumModelFromXElement(element);
				LoadMetaInfo(model, element);
				model.ProcessUpdate(element);
				return model;
			}

			List<BaseRegulation> ITestLoader<BaseRegulation>.LoadData(List<FileDetails> files, StringBuilder errorCollector) => base.LoadData(files, errorCollector).Cast<BaseRegulation>().ToList();
		}
	}
}
