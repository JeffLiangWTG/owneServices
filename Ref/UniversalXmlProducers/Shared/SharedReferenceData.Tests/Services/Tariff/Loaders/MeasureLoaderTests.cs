using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;
using CargoWise.RefDbRepo.SharedReferenceData.Tests;
using NUnit.Framework;
using static CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Helpers.Tests.TestHelperClasses;

namespace CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Loaders.Tests
{
	[TestFixture]
	sealed class MeasureLoaderTests
	{
		[Test]
		public void ConvertXmlElementToModel()
		{
			var element = TestHelper.GetXmlElement("Measure", "CargoWise.RefDbRepo.SharedReferenceData.Tests.Services.Tariff.TestFiles.Input.UT_Measure_001.xml");
			var model = sharedLoader.ConvertXElementToModel(element);

			Assert.That(model.ItemId, Is.EqualTo("0811109010"));
			Assert.That(model.RegulationId, Is.EqualTo("R9307300"));
			Assert.That(model.RegulationRoleTypeId, Is.EqualTo("1"));
			Assert.That(model.StartDate, Is.EqualTo(new DateTime(1993, 04, 01)));
			Assert.That(model.EndDate, Is.EqualTo(new DateTime(4712, 12, 31, 23, 59, 59)));
			Assert.That(model.MeasureType, Is.EqualTo("690"));
			Assert.That(model.GeographicalArea, Is.EqualTo("PL"));
			Assert.That(model.OrderNumber, Is.EqualTo("123456"));
			Assert.That(model.AdditionalCode, Is.EqualTo("501"));
			Assert.That(model.AdditionalCodeType, Is.EqualTo("2"));
			Assert.That(model.Components, Is.Not.Null.And.Not.Empty);
			Assert.That(model.Conditions, Is.Not.Null.And.Empty);
			Assert.That(model.ExcludedGeographicalAreas, Is.Not.Null);

			Assert.That(model.ExcludedGeographicalAreas.Count, Is.EqualTo(2));
			Assert.That(model.ExcludedGeographicalAreas.Contains("CN"));
			Assert.That(model.ExcludedGeographicalAreas.Contains("2012"));

			Assert.That(model.Footnotes, Is.Not.Null);
			Assert.That(model.Footnotes.Count, Is.EqualTo(2));
			Assert.That(model.Footnotes.Contains("CD647"));
			Assert.That(model.Footnotes.Contains("AB123"));

			element = TestHelper.GetXmlElement("Measure", "CargoWise.RefDbRepo.SharedReferenceData.Tests.Services.Tariff.TestFiles.Input.UT_Measure_001.xml", 1);
			model = sharedLoader.ConvertXElementToModel(element);

			Assert.That(model.Components, Is.Not.Null);
			Assert.That(model.Components.Count, Is.EqualTo(4));
			Assert.That(model.ExcludedGeographicalAreas, Is.Not.Null.And.Empty);

			//Check Components
			var comList = model.Components.ToList();
			Assert.That(comList[0].DutyAmount, Is.EqualTo(12.0));
			Assert.That(comList[0].DutyExpression, Is.EqualTo("19"));
			Assert.That(comList[0].MeasurementUnit, Is.EqualTo("DTN"));
			Assert.That(comList[0].MeasurementUnitQualifier, Is.EqualTo("N"));
			Assert.That(comList[0].MonetaryUnit, Is.EqualTo("XEA"));

			// Duplicate / Dodgy Data cleanup
			Assert.That(comList[1].HJID, Is.EqualTo("1234567"));
			Assert.That(comList[1].DutyAmount, Is.EqualTo(123.45));

			element = TestHelper.GetXmlElement("Measure", "CargoWise.RefDbRepo.SharedReferenceData.Tests.Services.Tariff.TestFiles.Input.UT_Measure_001.xml", 2);
			model = sharedLoader.ConvertXElementToModel(element);

			Assert.That(model.Conditions, Is.Not.Null);
			Assert.That(model.Conditions.Count, Is.EqualTo(6));

			var conList = model.Conditions.ToList();
			//Duplicate / Dodgy Data cleanup
			Assert.That(conList[5].HJID, Is.EqualTo("1234567"));
			Assert.That(conList[5].MeasurementUnit, Is.EqualTo("DTN"));

			//Check Conditions
			Assert.That(conList[0].SequenceNumber, Is.EqualTo(1));
			Assert.That(conList[0].DutyAmount, Is.EqualTo(1.0650));
			Assert.That(conList[0].MeasureAction, Is.EqualTo("01"));
			Assert.That(conList[0].ConditionCode, Is.EqualTo("M"));
			Assert.That(conList[0].MeasurementUnit, Is.EqualTo("DTN"));
			Assert.That(conList[0].MeasurementUnitQualifier, Is.EqualTo("Z"));
			Assert.That(conList[0].MonetaryUnit, Is.EqualTo("EUR"));

			//Check Condition-Components
			Assert.That(conList[0].Components, Is.Not.Null);
			Assert.That(conList[0].Components.Count, Is.EqualTo(1));
			var conComList = conList[0].Components.ToList();
			Assert.That(conComList[0].DutyAmount, Is.EqualTo(0.0));
			Assert.That(conComList[0].DutyExpression, Is.EqualTo("01"));
			Assert.That(conComList[0].MeasurementUnit, Is.EqualTo("DTN"));
			Assert.That(conComList[0].MeasurementUnitQualifier, Is.EqualTo("Z"));
			Assert.That(conComList[0].MonetaryUnit, Is.EqualTo("EUR"));

			Assert.That(conList[1].Components, Is.Not.Null);
			Assert.That(conList[1].Components.Count, Is.EqualTo(2));
			Assert.That(conList[5].Components, Is.Not.Null);
			Assert.That(conList[5].Components.Count, Is.EqualTo(0));

			element = TestHelper.GetXmlElement("Measure", "CargoWise.RefDbRepo.SharedReferenceData.Tests.Services.Tariff.TestFiles.Input.UT_Measure_001.xml", 3);
			model = sharedLoader.ConvertXElementToModel(element);

			conList = model.Conditions.ToList();
			Assert.That(model.Components, Is.Not.Null);
			Assert.That(model.Components.Count, Is.EqualTo(2));
			Assert.That(model.Conditions, Is.Not.Null);
			Assert.That(model.Conditions.Count, Is.EqualTo(3));

			Assert.That(conList[0].CertificateCode, Is.EqualTo("088"));
			Assert.That(conList[0].CertificateTypeCode, Is.EqualTo("Y"));
			Assert.That(conList[0].DutyAmount, Is.Null);
		}

		[Test]
		public void IsValidElement()
		{
			var element = TestHelper.GetXmlElement("Measure", "CargoWise.RefDbRepo.SharedReferenceData.Tests.Services.Tariff.TestFiles.Input.UT_Measure_001.xml", skip: 1);
			var isValid = sharedLoader.IsValidElement(element);
			Assert.That(isValid, Is.True, "2105001000 has goodsNomenclature & measureComponent and should be valid");

			element = TestHelper.GetXmlElement("Measure", "CargoWise.RefDbRepo.SharedReferenceData.Tests.Services.Tariff.TestFiles.Input.UT_Measure_001.xml", skip: 7);
			isValid = sharedLoader.IsValidElement(element);
			Assert.That(isValid, Is.False, "hjid 2264740 does not have goodsNomenclature and should not be valid");
		}

		[Test]
		public void ProcessXml_PartialUpdate()
		{
			var tempFolder = Directory.CreateTempSubdirectory().FullName;
			try
			{
				var filePath = Path.Combine(tempFolder, "UT_Measure_002.xml");
				TestHelper.SimulateDownload(filePath, "CargoWise.RefDbRepo.SharedReferenceData.Tests.Services.Tariff.TestFiles.Input.UT_Measure_002.xml");

				var files = new List<FileDetails>()
				{
					new FileDetails
					{
						Content = new ContentDetails { ExecutionDate = new DateTime(2024, 12, 01, 10, 9, 9) },
						Filename = filePath
					}
				};

				var errorCollector = new System.Text.StringBuilder();
				var models = sharedLoader.ProcessXml(string.Empty, files, errorCollector, _ => { });

				Assert.That(models, Has.Count.EqualTo(1), "models");
				var model = (Measure)models[0];
				var components = model.Components.Select(x => x.HJID);
				Assert.That(components, Is.EquivalentTo(new[] { "5555", "7777" }), "model.Components");

				var component7777 = model.Components.Single(x => x.HJID == "7777");
				Assert.That(component7777.DutyAmount, Is.EqualTo(8m), "Updated Component 7777 DutyAmount");

				Assert.That(model.Conditions.Count, Is.EqualTo(1), "model.Conditions");
				var condition = model.Conditions.Single();

				Assert.That(condition.DutyAmount, Is.EqualTo(0.1750m), "Updated condition DutyAmount");
				Assert.That(condition.Components.Count(), Is.EqualTo(1), "Updated condition Components count");

				var conditionComponent = condition.Components.Single();
				Assert.That(conditionComponent.DutyAmount, Is.EqualTo(1m), "conditionComponent.DutyAmount");

				Assert.That(model.ExcludedGeographicalAreas.Count(), Is.EqualTo(2), "ExcludedGeographicalAreas count");
				Assert.That(model.ExcludedGeographicalAreas, Is.EquivalentTo(new[] { "CH", "GZ" }), "ExcludedGeographicalAreas");

				Assert.That(model.Footnotes.Count(), Is.EqualTo(2), "Footnotes count");
				Assert.That(model.Footnotes, Is.EquivalentTo(new[] { "381", "380" }), "Footnotes");
			}
			finally
			{
				Directory.Delete(tempFolder, true);
			}
		}

		#region Setup
		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			sharedLoader = new MeasureLoaderTester();
		}

		MeasureLoaderTester sharedLoader;
		#endregion
	}
}
