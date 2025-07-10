using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;
using CargoWise.RefDbRepo.SharedReferenceData.Tests;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Loaders.Tests
{
	[TestFixture]
	sealed class GoodsNomenclatureLoaderTests
	{
		[Test]
		public void ConvertXmlElementToModel()
		{
			var element = TestHelper.GetXmlElement("GoodsNomenclature", "CargoWise.RefDbRepo.SharedReferenceData.Tests.Services.Tariff.TestFiles.Input.UT_Goodsnomenclature_001.xml");
			var model = sharedLoader.ConvertXElementToModel(element);

			Assert.That(model, Is.Not.Null);
			Assert.That(model.Description, Is.EqualTo("Unit Test GM 001"));
			Assert.That(model.ItemId, Is.EqualTo("2710203200"));
			Assert.That(model.Indent, Is.EqualTo(1));
			Assert.That(model.StartDate, Is.Not.Null.And.EqualTo(new DateTime(2020, 01, 01, 01, 02, 03)));
			Assert.That(model.EndDate, Is.Not.Null.And.EqualTo(new DateTime(2021, 12, 31, 23, 59, 59)));
			Assert.That(model.IsSection, Is.EqualTo(false));
		}

		[Test]
		public void ConvertXmlElementToModelWithPartialData()
		{
			var element = TestHelper.GetXmlElement("GoodsNomenclature", "CargoWise.RefDbRepo.SharedReferenceData.Tests.Services.Tariff.TestFiles.Input.UT_Goodsnomenclature_001.xml", 1);
			var model = sharedLoader.ConvertXElementToModel(element);

			Assert.That(model, Is.Not.Null);
			Assert.That(model.Description, Is.EqualTo("Unit Test GM 002"));
			Assert.That(model.ItemId, Is.EqualTo("2710203800"));
			Assert.That(model.Indent, Is.EqualTo(3));
			Assert.That(model.StartDate, Is.Not.Null.And.EqualTo(new DateTime(2020, 01, 01, 00, 00, 00)));
			Assert.That(model.EndDate, Is.Null);

			element = TestHelper.GetXmlElement("GoodsNomenclature", "CargoWise.RefDbRepo.SharedReferenceData.Tests.Services.Tariff.TestFiles.Input.UT_Goodsnomenclature_001.xml", 3);
			model = sharedLoader.ConvertXElementToModel(element);

			Assert.That(model, Is.Not.Null);
			Assert.That(model.Description, Is.Null);
			Assert.That(model.ItemId, Is.EqualTo("4707303000"));
			Assert.That(model.Indent, Is.EqualTo(2));
			Assert.That(model.StartDate, Is.Not.Null.And.EqualTo(new DateTime(2020, 01, 01, 00, 00, 00)));
			Assert.That(model.EndDate, Is.Null);
		}

		[Test]
		public void IsValidElement()
		{
			var element = TestHelper.GetXmlElement("GoodsNomenclature", "CargoWise.RefDbRepo.SharedReferenceData.Tests.Services.Tariff.TestFiles.Input.UT_Goodsnomenclature_001.xml");
			var isValid = sharedLoader.IsValidElement(element);
			Assert.That(isValid, Is.True, "2710203200 has goodsNomenclatureDescriptionPeriod and should be valid");

			element = TestHelper.GetXmlElement("GoodsNomenclature", "CargoWise.RefDbRepo.SharedReferenceData.Tests.Services.Tariff.TestFiles.Input.UT_Goodsnomenclature_001.xml", skip: 4);
			isValid = sharedLoader.IsValidElement(element);
			Assert.That(isValid, Is.False, "2207100020 does not have goodsNomenclatureDescriptionPeriod and should not be valid");

			element = TestHelper.GetXmlElement("GoodsNomenclature", "CargoWise.RefDbRepo.SharedReferenceData.Tests.Services.Tariff.TestFiles.Input.UT_Goodsnomenclature_001.xml", skip: 5);
			isValid = sharedLoader.IsValidElement(element);
			Assert.That(isValid, Is.False, "7210708091 does not have goodsNomenclatureIndents and should not be valid");

			element = TestHelper.GetXmlElement("GoodsNomenclature", "CargoWise.RefDbRepo.SharedReferenceData.Tests.Services.Tariff.TestFiles.Input.UT_Goodsnomenclature_001.xml", skip: 6);
			isValid = sharedLoader.IsValidElement(element);
			Assert.That(isValid, Is.True, "2207100020/U does not have goodsNomenclatureDescriptionPeriod and should be valid because it's an update");

			element = TestHelper.GetXmlElement("GoodsNomenclature", "CargoWise.RefDbRepo.SharedReferenceData.Tests.Services.Tariff.TestFiles.Input.UT_Goodsnomenclature_001.xml", skip: 7);
			isValid = sharedLoader.IsValidElement(element);
			Assert.That(isValid, Is.True, "7210708091/U does not have goodsNomenclatureIndents and should be valid because it's an update");
		}

		[Test]
		public void UpdateEmpty()
		{
			CreateTestModel();
			var element = TestHelper.GetXmlElement("GoodsNomenclature", "CargoWise.RefDbRepo.SharedReferenceData.Tests.Services.Tariff.TestFiles.Input.UT_Goodsnomenclature_003.xml");
			sharedLoader.ProcessElement(element);

			var updatedModel = (GoodsNomenclature)sharedLoader.Models["1234"];
			Assert.That(updatedModel.OpType, Is.EqualTo("C"), "OpType should not be changed");
			Assert.That(updatedModel.OpDate, Is.EqualTo(new DateTime(2024, 02, 21, 10, 16, 00)), "OpDate should be updated");
			Assert.That(updatedModel.HJID, Is.EqualTo("1234"), "HJID should not be changed");
			Assert.That(updatedModel.Description, Is.EqualTo("Description"), "Description should not be changed");
			Assert.That(updatedModel.StartDate, Is.EqualTo(new DateTime(2023, 1, 1)), "StartDate should not be changed");
			Assert.That(updatedModel.EndDate, Is.EqualTo(new DateTime(2024, 12, 31, 23, 59, 59)), "EndDate should not be changed");
			Assert.That(updatedModel.ProductLineSuffix, Is.EqualTo("80"), "ProductLineSuffix should not be changed");
			Assert.That(updatedModel.Indent, Is.EqualTo(5), "Indent should not be changed");
		}

		[Test]
		public void UpdateDescription()
		{
			CreateTestModel();
			var element = TestHelper.GetXmlElement("GoodsNomenclature", "CargoWise.RefDbRepo.SharedReferenceData.Tests.Services.Tariff.TestFiles.Input.UT_Goodsnomenclature_003.xml", 1);
			sharedLoader.ProcessElement(element);

			var updatedModel = (GoodsNomenclature)sharedLoader.Models["1234"];
			Assert.That(updatedModel.OpType, Is.EqualTo("C"), "OpType should not be changed");
			Assert.That(updatedModel.OpDate, Is.EqualTo(new DateTime(2024, 02, 21, 10, 16, 01)), "OpDate should be updated");
			Assert.That(updatedModel.HJID, Is.EqualTo("1234"), "HJID should not be changed");
			Assert.That(updatedModel.Description, Is.EqualTo("Updated description"), "Description should be updated");
			Assert.That(updatedModel.StartDate, Is.EqualTo(new DateTime(2023, 1, 1)), "StartDate should not be changed");
			Assert.That(updatedModel.EndDate, Is.EqualTo(new DateTime(2024, 12, 31, 23, 59, 59)), "EndDate should not be changed");
			Assert.That(updatedModel.ProductLineSuffix, Is.EqualTo("80"), "ProductLineSuffix should not be changed");
			Assert.That(updatedModel.Indent, Is.EqualTo(5), "Indent should not be changed");
		}

		[Test]
		public void UpdateStartDate()
		{
			CreateTestModel();
			var element = TestHelper.GetXmlElement("GoodsNomenclature", "CargoWise.RefDbRepo.SharedReferenceData.Tests.Services.Tariff.TestFiles.Input.UT_Goodsnomenclature_003.xml", 2);
			sharedLoader.ProcessElement(element);

			var updatedModel = (GoodsNomenclature)sharedLoader.Models["1234"];
			Assert.That(updatedModel.OpType, Is.EqualTo("C"), "OpType should not be changed");
			Assert.That(updatedModel.OpDate, Is.EqualTo(new DateTime(2024, 02, 21, 10, 16, 02)), "OpDate should be updated");
			Assert.That(updatedModel.HJID, Is.EqualTo("1234"), "HJID should not be changed");
			Assert.That(updatedModel.Description, Is.EqualTo("Description"), "Description should not be changed");
			Assert.That(updatedModel.StartDate, Is.EqualTo(new DateTime(2023, 4, 3, 10, 01, 10)), "StartDate should be updated");
			Assert.That(updatedModel.EndDate, Is.EqualTo(new DateTime(2024, 12, 31, 23, 59, 59)), "EndDate should not be changed");
			Assert.That(updatedModel.ProductLineSuffix, Is.EqualTo("80"), "ProductLineSuffix should not be changed");
			Assert.That(updatedModel.Indent, Is.EqualTo(5), "Indent should not be changed");
		}

		[Test]
		public void UpdateEndDate()
		{
			CreateTestModel();
			var element = TestHelper.GetXmlElement("GoodsNomenclature", "CargoWise.RefDbRepo.SharedReferenceData.Tests.Services.Tariff.TestFiles.Input.UT_Goodsnomenclature_003.xml", 3);
			sharedLoader.ProcessElement(element);

			var updatedModel = (GoodsNomenclature)sharedLoader.Models["1234"];
			Assert.That(updatedModel.OpType, Is.EqualTo("C"), "OpType should not be changed");
			Assert.That(updatedModel.OpDate, Is.EqualTo(new DateTime(2024, 02, 21, 10, 16, 03)), "OpDate should be updated");
			Assert.That(updatedModel.HJID, Is.EqualTo("1234"), "HJID should not be changed");
			Assert.That(updatedModel.Description, Is.EqualTo("Description"), "Description should not be changed");
			Assert.That(updatedModel.StartDate, Is.EqualTo(new DateTime(2023, 1, 1)), "StartDate should not be changed");
			Assert.That(updatedModel.EndDate, Is.EqualTo(new DateTime(2023, 11, 30, 23, 59, 59)), "EndDate should be updated");
			Assert.That(updatedModel.ProductLineSuffix, Is.EqualTo("80"), "ProductLineSuffix should not be changed");
			Assert.That(updatedModel.Indent, Is.EqualTo(5), "Indent should not be changed");
		}

		[Test]
		public void UpdateIndent()
		{
			CreateTestModel();
			var element = TestHelper.GetXmlElement("GoodsNomenclature", "CargoWise.RefDbRepo.SharedReferenceData.Tests.Services.Tariff.TestFiles.Input.UT_Goodsnomenclature_003.xml", 4);
			sharedLoader.ProcessElement(element);

			var updatedModel = sharedLoader.Models["1234"];
			Assert.That(updatedModel.OpType, Is.EqualTo("C"), "OpType should not be changed");
			Assert.That(updatedModel.OpDate, Is.EqualTo(new DateTime(2024, 02, 21, 10, 16, 04)), "OpDate should be updated");
			Assert.That(updatedModel.HJID, Is.EqualTo("1234"), "HJID should not be changed");
			Assert.That(updatedModel.Description, Is.EqualTo("Description"), "Description should not be changed");
			Assert.That(updatedModel.StartDate, Is.EqualTo(new DateTime(2023, 1, 1)), "StartDate should not be changed");
			Assert.That(updatedModel.EndDate, Is.EqualTo(new DateTime(2024, 12, 31, 23, 59, 59)), "EndDate should not be changed");
			Assert.That(updatedModel.ProductLineSuffix, Is.EqualTo("80"), "ProductLineSuffix should not be changed");
			Assert.That(updatedModel.Indent, Is.EqualTo(4), "Indent should be updated");
		}

		[Test]
		public void Update()
		{
			CreateTestModel();
			var element = TestHelper.GetXmlElement("GoodsNomenclature", "CargoWise.RefDbRepo.SharedReferenceData.Tests.Services.Tariff.TestFiles.Input.UT_Goodsnomenclature_003.xml", 8);
			sharedLoader.ProcessElement(element);

			var updatedModel = sharedLoader.Models["1234"];
			Assert.That(updatedModel.OpType, Is.EqualTo("C"), "OpType should not be changed");
			Assert.That(updatedModel.OpDate, Is.EqualTo(new DateTime(2024, 02, 21, 10, 16, 08)), "OpDate should be updated");
			Assert.That(updatedModel.HJID, Is.EqualTo("1234"), "HJID should not be changed");
			Assert.That(updatedModel.Description, Is.EqualTo("Updated description"), "Description should be updated");
			Assert.That(updatedModel.StartDate, Is.EqualTo(new DateTime(1971, 12, 31, 0, 0, 0)), "StartDate should be updated");
			Assert.That(updatedModel.EndDate, Is.EqualTo(new DateTime(2020, 12, 31, 23, 59, 59)), "EndDate should be updated");
			Assert.That(updatedModel.ProductLineSuffix, Is.EqualTo("80"), "ProductLineSuffix should not be changed");
			Assert.That(updatedModel.Indent, Is.EqualTo(4), "Indent should be updated");
		}

		[Test]
		public void UpdateNotFound()
		{
			CreateTestModel();
			var element = TestHelper.GetXmlElement("GoodsNomenclature", "CargoWise.RefDbRepo.SharedReferenceData.Tests.Services.Tariff.TestFiles.Input.UT_Goodsnomenclature_003.xml", 5);

			var hasData = sharedLoader.ProcessElement(element);
			Assert.That(hasData, Is.EqualTo(false));
		}

		[Test]
		public void UpdateUpdateOlderThanModel()
		{
			CreateTestModel();
			sharedLoader.Models["1234"].OpDate = new DateTime(2024, 12, 31, 23, 59, 59);
			sharedLoader.ErrorCollector.Clear();
			var element = TestHelper.GetXmlElement("GoodsNomenclature", "CargoWise.RefDbRepo.SharedReferenceData.Tests.Services.Tariff.TestFiles.Input.UT_Goodsnomenclature_003.xml", 8);
			var hasData = sharedLoader.ProcessElement(element);

			Assert.That(hasData, Is.EqualTo(false));
			Assert.That(sharedLoader.ErrorCollector.ToString(), Is.Empty);

			var updatedModel = sharedLoader.Models["1234"];
			Assert.That(updatedModel.OpType, Is.EqualTo("C"), "OpType should not be changed");
			Assert.That(updatedModel.OpDate, Is.EqualTo(new DateTime(2024, 12, 31, 23, 59, 59)), "OpDate should not be changed");
			Assert.That(updatedModel.HJID, Is.EqualTo("1234"), "HJID should not be changed");
			Assert.That(updatedModel.Description, Is.EqualTo("Description"), "Description should not be changed");
			Assert.That(updatedModel.StartDate, Is.EqualTo(new DateTime(2023, 1, 1)), "StartDate should not be changed");
			Assert.That(updatedModel.EndDate, Is.EqualTo(new DateTime(2024, 12, 31, 23, 59, 59)), "EndDate should not be changed");
			Assert.That(updatedModel.ProductLineSuffix, Is.EqualTo("80"), "ProductLineSuffix should not be changed");
			Assert.That(updatedModel.Indent, Is.EqualTo(5), "Indent should not be changed");
		}

		[Test]
		public void UpdateUpdateForDeletedModel()
		{
			CreateTestModel();
			sharedLoader.Models["1234"].OpType = "D";
			sharedLoader.ErrorCollector.Clear();
			var element = TestHelper.GetXmlElement("GoodsNomenclature", "CargoWise.RefDbRepo.SharedReferenceData.Tests.Services.Tariff.TestFiles.Input.UT_Goodsnomenclature_003.xml", 8);

			var hasData = sharedLoader.ProcessElement(element);
			Assert.That(hasData, Is.EqualTo(false));
			//Assert.That(sharedLoader.ErrorCollector.ToString(), Does.Contain("Update for model 1234 (0102030405 80) found OpType D"));

			var model = sharedLoader.Models["1234"];
			Assert.That(model.OpType, Is.EqualTo("D"), "OpType should not be changed");
			Assert.That(model.OpDate, Is.EqualTo(new DateTime(2022, 12, 15, 14, 12, 10)), "OpDate should not be changed");
			Assert.That(model.HJID, Is.EqualTo("1234"), "HJID should not be changed");
			Assert.That(model.Description, Is.EqualTo("Description"), "Description should not be changed");
			Assert.That(model.StartDate, Is.EqualTo(new DateTime(2023, 1, 1)), "StartDate should not be changed");
			Assert.That(model.EndDate, Is.EqualTo(new DateTime(2024, 12, 31, 23, 59, 59)), "EndDate should not be changed");
			Assert.That(model.ProductLineSuffix, Is.EqualTo("80"), "ProductLineSuffix should not be changed");
			Assert.That(model.Indent, Is.EqualTo(5), "Indent should not be changed");
		}

		[Test]
		public void DoNotBlankDescriptionForUpdateWithDateOnly()
		{
			CreateTestModel();
			var element = TestHelper.GetXmlElement("GoodsNomenclature", "CargoWise.RefDbRepo.SharedReferenceData.Tests.Services.Tariff.TestFiles.Input.UT_Goodsnomenclature_003.xml", 6);

			var hasData = sharedLoader.ProcessElement(element);
			Assert.That(hasData, Is.EqualTo(true));

			var updatedModel = sharedLoader.Models["1234"];
			Assert.That(updatedModel.OpType, Is.EqualTo("C"), "OpType should not be changed");
			Assert.That(updatedModel.OpDate, Is.EqualTo(new DateTime(2024, 02, 21, 10, 16, 06)), "OpDate should be updated");
			Assert.That(updatedModel.HJID, Is.EqualTo("1234"), "HJID should not be changed");
			Assert.That(updatedModel.Description, Is.EqualTo("Description"), "Description should not be changed");
			Assert.That(updatedModel.StartDate, Is.EqualTo(new DateTime(2023, 1, 1)), "StartDate should not be changed");
			Assert.That(updatedModel.EndDate, Is.EqualTo(new DateTime(2024, 12, 31, 23, 59, 59)), "EndDate should not be changed");
			Assert.That(updatedModel.ProductLineSuffix, Is.EqualTo("80"), "ProductLineSuffix should not be changed");
			Assert.That(updatedModel.Indent, Is.EqualTo(5), "Indent should not be changed");
		}

		[Test]
		public void SimpleSuccessor()
		{
			CreateTestModel();
			sharedLoader.Models.Add(new[] { new GoodsNomenclature
			{
				HJID = "123",
				OpType = "C",
				OpDate = new DateTime(1980, 1, 1, 0, 0, 0),
				ItemId = "0102030400",
				Description = "Description",
				StartDate = new DateTime(2023, 1, 1),
				ProductLineSuffix = "80",
				Indent = 4
			} });
			sharedLoader.Models["1234"].EndDate = null;

			var element = TestHelper.GetXmlElement("GoodsNomenclature", "CargoWise.RefDbRepo.SharedReferenceData.Tests.Services.Tariff.TestFiles.Input.UT_Goodsnomenclature_003.xml", 7);
			var hasData = sharedLoader.ProcessElement(element);
			Assert.That(hasData, Is.EqualTo(true));

			var updatedAbsorbed = (GoodsNomenclature)sharedLoader.Models["1234"];
			Assert.That(updatedAbsorbed.OpType, Is.EqualTo("C"), "OpType should not be changed");
			Assert.That(updatedAbsorbed.OpDate, Is.EqualTo(new DateTime(2024, 02, 21, 10, 16, 07)), "OpDate should be updated");
			Assert.That(updatedAbsorbed.HJID, Is.EqualTo("1234"), "HJID should not be changed");
			Assert.That(updatedAbsorbed.Description, Is.EqualTo("Description"), "Description should not be changed");
			Assert.That(updatedAbsorbed.StartDate, Is.EqualTo(new DateTime(2023, 1, 1)), "StartDate should not be changed");
			Assert.That(updatedAbsorbed.EndDate, Is.EqualTo(new DateTime(2024, 12, 31, 23, 59, 59)), "EndDate should be updated");
			Assert.That(updatedAbsorbed.ProductLineSuffix, Is.EqualTo("80"), "ProductLineSuffix should not be changed");
			Assert.That(updatedAbsorbed.Indent, Is.EqualTo(5), "Indent should not be changed");

			var updatedParent = (GoodsNomenclature)sharedLoader.Models["123"];
			Assert.That(updatedParent.EndDate, Is.EqualTo(new DateTime(2024, 12, 31, 23, 59, 59)), "Old parent EndDate should be updated");

			Assert.That(sharedLoader.Models.Values.Select(x => x.HJID), Does.Contain("13835860"), "New GoodsNomenclature record should be created for the successor");
			var successor = (GoodsNomenclature)sharedLoader.Models["13835860"];
			Assert.That(successor.OpType, Is.EqualTo("C"), "Successor OpType");
			Assert.That(successor.OpDate, Is.EqualTo(new DateTime(2024, 02, 14, 13, 51, 15)), "Successor OpDate");
			Assert.That(successor.HJID, Is.EqualTo("13835860"), "Successor HJID");
			Assert.That(successor.ItemId, Is.EqualTo("0102030400"), "Successor ItemId copied from original");
			Assert.That(successor.Description, Is.EqualTo("Description"), "Successor Description copied from original");
			Assert.That(successor.StartDate, Is.EqualTo(new DateTime(2025, 1, 1, 0, 0, 0)), "Successor StartDate");
			Assert.That(successor.EndDate, Is.Null, "Successor should not have EndDate");
			Assert.That(successor.ProductLineSuffix, Is.EqualTo("80"), "Successor ProductLineSuffix copied from original");
			Assert.That(successor.Indent, Is.EqualTo(4), "Successor Indent copied from original");
		}

		void CreateTestModel()
		{
			sharedLoader.Models.Clear();
			sharedLoader.Models.Add(new List<GoodsNomenclature>
			{
				new GoodsNomenclature
				{
					HJID = "1234",
					OpType = "C",
					OpDate = new DateTime(2022, 12, 15, 14, 12, 10),
					ItemId = "0102030405",
					Description = "Description",
					StartDate = new DateTime(2023, 1, 1),
					EndDate = new DateTime(2024, 12, 31, 23, 59, 59),
					ProductLineSuffix = "80",
					Indent = 5
				}
			});
		}

		#region Setup
		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			sharedLoader = new GoodsNomenclatureLoaderTester();
		}

		class GoodsNomenclatureLoaderTester : GoodsNomenclatureLoader
		{
			public GoodsNomenclatureLoaderTester()
			{
				_ = ProcessXml(string.Empty, new List<IFileDetails>(), new StringBuilder(), x => { });
			}

			public GoodsNomenclature ConvertXElementToModel(XElement element)
			{
				var model = CreateMinimumModelFromXElement(element);
				LoadMetaInfo(model, element);
				model.ProcessUpdate(element);
				return model;
			}

			public new bool IsValidElement(XElement element) => base.IsValidElement(element);

			public new bool ProcessElement(XElement element) => base.ProcessElement(element);

			public new UpdatableElementList<GoodsNomenclature> Models => base.Models;
			public new StringBuilder ErrorCollector => base.ErrorCollector;
		}

		GoodsNomenclatureLoaderTester sharedLoader;
		#endregion
	}
}
