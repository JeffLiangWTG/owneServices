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
	sealed class ModificationRegulationTests : BaseLoaderTests<ModificationRegulation>
	{
		public override void LoadDataAsserts(List<ModificationRegulation> models, string errors)
		{
			Assert.That(models.Count, Is.EqualTo(3));
			Assert.That(errors,
				Contains.Substring("RegulationId is required")
				.And.Contains("UT_LoaderTests_002.xml"));

			Assert.That(models.Any(x => x.RegulationId == "MR00001"), Is.True);
			Assert.That(models.Any(x => x.RegulationId == "MR000002"), Is.True);
			Assert.That(models.Any(x => x.RegulationId == "R1204250"), Is.True);
			Assert.That(models.Any(x => string.IsNullOrEmpty(x.RegulationId)), Is.False);
		}

		public override void ModelIsValidCore(ModificationRegulation model, StringBuilder errorCollector, string source)
		{
			var result = model.IsValid(errorCollector, source);

			Assert.That(result, Is.EqualTo(false));
			Assert.That(errorCollector.ToString(),
				Contains.Substring("ModificationRegulation validation error")
				.And.Contains("RegulationId is required")
				.And.Contains("StartDate is required")
				.And.Contains("BaseRegulationId is required")
				.And.Contains($"Source: '{source}'"));

			errorCollector.Clear();
			model.RegulationId = "ABC";
			Assert.That(model.IsValid(errorCollector, source), Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Is.Not.Empty);

			errorCollector.Clear();
			model.StartDate = new DateTime();
			Assert.That(model.IsValid(errorCollector, source), Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Is.Not.Empty);

			errorCollector.Clear();
			model.BaseRegulationId = "XYZ";
			Assert.That(model.IsValid(errorCollector, source), Is.EqualTo(true));
			Assert.That(errorCollector.ToString(), Is.Empty);
		}

		public override void ConvertXmlElementToModelAsserts(ModificationRegulation model)
		{
			Assert.That(model.HJID, Is.EqualTo("45401"));
			Assert.That(model.RegulationId, Is.EqualTo("MR00001"));
			Assert.That(model.RegulationRoleTypeId, Is.EqualTo("4"));
			Assert.That(model.EffectiveEndDate.Value, Is.EqualTo(new DateTime(1995, 12, 31, 23, 59, 59)));
			Assert.That(model.StartDate, Is.EqualTo(new DateTime(1995, 07, 01)));
			Assert.That(model.EndDate.Value, Is.EqualTo(new DateTime(1995, 12, 31, 23, 59, 59)));
			Assert.That(model.BaseRegulationId, Is.EqualTo("BR000001"));
			Assert.That(model.ReplacementIndicator, Is.EqualTo(1));
		}

		public override ITestLoader<ModificationRegulation> CreateTestLoader() => new ModificationRegulationLoaderTester();
		class ModificationRegulationLoaderTester : ModificationRegulationLoader, ITestLoader<ModificationRegulation>
		{
			string ITestLoader<ModificationRegulation>.XmlElementName => base.ElementName;

			ModificationRegulation ITestLoader<ModificationRegulation>.ConvertXElementToModel(XElement element)
			{
				var model = CreateMinimumModelFromXElement(element);
				LoadMetaInfo(model, element);
				model.ProcessUpdate(element);
				return model;
			}

			List<ModificationRegulation> ITestLoader<ModificationRegulation>.LoadData(List<FileDetails> files, StringBuilder errorCollector) => base.LoadData(files, errorCollector).Cast<ModificationRegulation>().ToList();
		}
	}
}
