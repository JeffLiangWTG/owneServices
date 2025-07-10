using System;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.DataTransfer.Testing
{
	[TestedType(typeof(PackLineValueObjectDataAdapter<PackLine, Xsd.Package>))]
	sealed class PackLineValueObjectDataAdapterTest : ValueObjectDataAdapterTest<PackLine, Xsd.Package>
	{
		public void TestImportPackLineContainers()
		{
			Xsd.Shipment shipmentValue = new Xsd.Shipment();
			shipmentValue.ShipmentDetails = new Xsd.ShipmentShipmentDetails();
			shipmentValue.ShipmentDetails.Packages = new Xsd.PackageCollection();
			Xsd.Package package = shipmentValue.ShipmentDetails.Packages.AddNew();
			package.ContainerNumber = "1234";

			CommonConsol consol = Factory.New<CommonConsol>();
			CommonContainer container = consol.Containers.AddNew();
			container.JC_ContainerNum = "1234";

			CommonShipment shipment = consol.Shipments.AddNew();
			PackLine packline = shipment.OuterPackLines.AddNew();
			PackLineValueObjectDataAdapter<PackLine, Xsd.Package> adapter = new PackLineValueObjectDataAdapter<PackLine, Xsd.Package>();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportFromValueObject(packline, package, context);
			AssertEquals("Container number should be populated", "1234", consol.Shipments[0].OuterPackLines[0].JL_Calc_ContainerNum);
			AssertEquals("packline custom attribute date 1", package.Custom.Date1, packline.JL_CustomDate1);
			AssertEquals("packline custom attribute date 2", package.Custom.Date2, packline.JL_CustomDate2);
			AssertEquals("packline custom attribute text 1", package.Custom.Text1, packline.JL_CustomAttrib1);
			AssertEquals("packline custom attribute text 2", package.Custom.Text2, packline.JL_CustomAttrib2);
			AssertEquals("packline custom attribute text 3", package.Custom.Text3, packline.JL_CustomAttrib3);
			AssertEquals("packline custom attribute text 4", package.Custom.Text4, packline.JL_CustomAttrib4);
			AssertEquals("packline custom attribute decimal 1", package.Custom.Decimal1, packline.JL_CustomDecimal1);
			AssertEquals("packline custom attribute decimal 2", package.Custom.Decimal2, packline.JL_CustomDecimal2);
			AssertEquals("packline custom attribute flag 1", false, packline.JL_CustomFlag1);
			AssertEquals("packline custom attribute flag 2", false, packline.JL_CustomFlag2);
		}

		public void TestImportPackLineDangerousGoods()
		{
			UNDGSubstance undg = Factory.New<UNDGSubstance>();
			undg.DG_MP = UNDGSubstanceLookups.MarinePollutantTypes.Depends_Code;
			undg.DG_TechName = "*";
			undg.DG_Code = "9999a";
			undg.DG_UNNO = "9999";
			undg.DG_Variant = "a";

			Xsd.Shipment shipmentValue = new Xsd.Shipment();
			shipmentValue.ShipmentDetails = new Xsd.ShipmentShipmentDetails();
			shipmentValue.ShipmentDetails.Packages = new Xsd.PackageCollection();
			Xsd.Package package = shipmentValue.ShipmentDetails.Packages.AddNew();
			var haz = package.DangerousGoods.AddNew();
			haz.UNDGCode = undg.DG_Code;
			haz.MarinePollutant = UNDGSubstanceLookups.MarinePollutantTypes.SevereMarinePollutant_Code;
			haz.TechnicalName = "some name";

			CommonShipment shipment = Factory.New<CommonShipment>();
			PackLine packline = shipment.OuterPackLines.AddNew();
			PackLineValueObjectDataAdapter<PackLine, Xsd.Package> adapter = new PackLineValueObjectDataAdapter<PackLine, Xsd.Package>();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportFromValueObject(packline, package, context);
			AssertEquals(haz.TechnicalName, packline.UNDGs[0].DI_TechnicalName);
			AssertEquals(haz.MarinePollutant, packline.UNDGs[0].DI_MPMarinePollutant);

			shipmentValue = new Xsd.Shipment();
			shipmentValue.ShipmentDetails = new Xsd.ShipmentShipmentDetails();
			shipmentValue.ShipmentDetails.Packages = new Xsd.PackageCollection();
			package = shipmentValue.ShipmentDetails.Packages.AddNew();
			haz = package.HazardousGoods;
			haz.UNDGCode = undg.DG_Code;
			haz.MarinePollutant = UNDGSubstanceLookups.MarinePollutantTypes.MarinePollutant_Code;
			haz.TechnicalName = "some other name";

			packline = shipment.OuterPackLines.AddNew();
			adapter.ImportFromValueObject(packline, package, context);
			AssertEquals(haz.TechnicalName, packline.UNDGs[0].DI_TechnicalName);
			AssertEquals(haz.MarinePollutant, packline.UNDGs[0].DI_MPMarinePollutant);
		}

		public void TestImportDecimals_OutOfRange()
		{
			var outOfSqlRange = 9876543210.1M;
			var expectedValue = 0M;

			var shipmentValue = new Xsd.Shipment();
			shipmentValue.ShipmentDetails = new Xsd.ShipmentShipmentDetails();
			shipmentValue.ShipmentDetails.Packages = new Xsd.PackageCollection();
			Xsd.Package package = shipmentValue.ShipmentDetails.Packages.AddNew();

			package.Volume.Value = outOfSqlRange;
			package.Weight.Value = outOfSqlRange;
			package.Custom.Decimal1 = outOfSqlRange;
			package.Custom.Decimal2 = outOfSqlRange;
			package.LoadingMeters = outOfSqlRange;

			var shipment = Factory.New<CommonShipment>();
			var packline = shipment.OuterPackLines.AddNew();
			var adapter = new PackLineValueObjectDataAdapter<PackLine, Xsd.Package>();
			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportFromValueObject(packline, package, context);

			AssertEquals("JL_ActualVolume", expectedValue, packline.JL_ActualVolume);
			AssertEquals("JL_ActualWeight", expectedValue, packline.JL_ActualWeight);
			AssertEquals("JL_CustomDecimal1", expectedValue, packline.JL_CustomDecimal1);
			AssertEquals("JL_CustomDecimal2", expectedValue, packline.JL_CustomDecimal2);
			AssertEquals("JL_LoadingMeters", expectedValue, packline.JL_LoadingMeters);

			var volume = 111.1M;
			var weight = 222.2M;
			var custom1 = 333.3M;
			var custom2 = 444.4M;
			var loadingMeters = 555.5M;

			package.Volume.Value = volume;
			package.Weight.Value = weight;
			package.Custom.Decimal1 = custom1;
			package.Custom.Decimal2 = custom2;
			package.LoadingMeters = loadingMeters;

			adapter.ImportFromValueObject(packline, package, context);

			AssertEquals("JL_ActualVolume", volume, packline.JL_ActualVolume);
			AssertEquals("JL_ActualWeight", weight, packline.JL_ActualWeight);
			AssertEquals("JL_CustomDecimal1", custom1, packline.JL_CustomDecimal1);
			AssertEquals("JL_CustomDecimal2", custom2, packline.JL_CustomDecimal2);
			AssertEquals("JL_LoadingMeters", loadingMeters, packline.JL_LoadingMeters);
		}

		public void TestExportCustomAttributes()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_HouseBill = "HOUSEBILL";
			PackLine packline = shipment.OuterPackLines.AddNew();
			packline.JL_CustomDate1 = ZDateTime.Now;
			packline.JL_Description = "goods";

			PackLineValueObjectDataAdapter<PackLine, Xsd.Package> adapter = new PackLineValueObjectDataAdapter<PackLine, Xsd.Package>();
			Xsd.Package packLineXSD = adapter.ExportToValueObject(packline, new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals("packline custom attribute date 1", packline.JL_CustomDate1, packLineXSD.Custom.Date1);
			AssertEquals("packline custom attribute date 2", packline.JL_CustomDate2, packLineXSD.Custom.Date2);
			AssertEquals("packline custom attribute text 1", packline.JL_CustomAttrib1, packLineXSD.Custom.Text1);
			AssertEquals("packline custom attribute text 2", packline.JL_CustomAttrib2, packLineXSD.Custom.Text2);
			AssertEquals("packline custom attribute text 3", packline.JL_CustomAttrib3, packLineXSD.Custom.Text3);
			AssertEquals("packline custom attribute text 4", packline.JL_CustomAttrib4, packLineXSD.Custom.Text4);
			AssertEquals("packline custom attribute decimal 1", packline.JL_CustomDecimal1, packLineXSD.Custom.Decimal1);
			AssertEquals("packline custom attribute decimal 2", packline.JL_CustomDecimal2, packLineXSD.Custom.Decimal2);
			AssertEquals("packline custom attribute flag 1", packline.JL_CustomFlag1 ? Xsd.TrueFalse.@true : Xsd.TrueFalse.@false, packLineXSD.Custom.Flag1);
			AssertEquals("packline custom attribute flag 2", packline.JL_CustomFlag2 ? Xsd.TrueFalse.@true : Xsd.TrueFalse.@false, packLineXSD.Custom.Flag2);
		}

		#region Overrides for base test

		protected override ValueObjectDataAdapter<PackLine, Xsd.Package> GetNewBizObjXmlDataAdapter()
		{
			return new PackLineValueObjectDataAdapter<PackLine, Xsd.Package>();
		}

		protected override string ExpectedRootCollectionElementName
		{
			get { return "Packages"; }
		}

		protected override string ExpectedRootElementName
		{
			get { return "Package"; }
		}

		protected override PackLine NewBusinessObject()
		{
			PackLine packline = Factory.New<PackLine>();
			packline.JL_F3_NKPackType = Core.Constants.PkgUnit.Bundle;
			return packline;
		}

		protected override void OnBeforeImportFromValueObjectForExportImportExportTest(BusinessObject bizObjOriginallyExportedFrom, BusinessObject bizObjToImportTo)
		{
			base.OnBeforeImportFromValueObjectForExportImportExportTest(bizObjOriginallyExportedFrom, bizObjToImportTo);
			PackLine packline = bizObjToImportTo as PackLine;
			packline.JL_CustomDate1 = ((PackLine)bizObjOriginallyExportedFrom).JL_CustomDate1;
		}

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			PackLine emptyPackLine = Factory.New<PackLine>();
			emptyPackLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Bundle;
			var expectedOutputFilename = resourceRetriever.Value.SaveResourceToFile("Enterprise.Freight.DataTransfer.Test.Freight.Testing.EmptyPackLine.xml", "EmptyPackLine.xml");
			return new BusinessObjectAndExpectedOutputFileName(emptyPackLine, expectedOutputFilename, ValidationKind.None, "Empty pack line");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample()
		{
			return GetEmptyBizObjSample();
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			CommonShipment shipment = consol.Shipments.AddNew();
			PackLine populatedPackLine = shipment.OuterPackLines.AddNew();

			UNDGSubstance undgSubstance = Factory.LoadTop1<UNDGSubstance>(new ZQuery(UNDGSubstanceSchema.DG_Code, "2478c"));
			undgSubstance.DG_PG = "III";
			undgSubstance.DG_MP = "S";

			populatedPackLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Basket;
			populatedPackLine.JL_PackageCount = 157;
			populatedPackLine.JL_ActualWeight = 182;
			populatedPackLine.JL_ActualWeightUQ = Core.Constants.Weight.Grams;
			populatedPackLine.JL_ActualVolume = 13.299m;
			populatedPackLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicFeet;
			populatedPackLine.JL_LoadingMeters = 10.24m;
			populatedPackLine.JL_Length = 56;
			populatedPackLine.JL_Width = 211;
			populatedPackLine.JL_Height = 203;
			populatedPackLine.JL_Description = "Description";
			populatedPackLine.JL_RN_NKOrigin = "AU";
			populatedPackLine.JL_UnitOfDimension = "MM";
			populatedPackLine.UNDGs.AddNew().DI_DG = undgSubstance.PK;
			populatedPackLine.JL_MarksAndNumbers = "MARKSANDNUMBERS";
			populatedPackLine.JL_RH_NKCommodityCode = "GEN";
			populatedPackLine.UNDGs.UNDGFlashPointManager.Value = ((ZDecimal)3.5m).ToString();
			populatedPackLine.JL_RefNumber = "REFNUM";
			populatedPackLine.JL_HarmonisedCode = "HRM";
			populatedPackLine.JL_CustomDate1 = new ZDateTime(2009, 1, 1, 12, 12, 30);
			populatedPackLine.JL_CustomDate2 = new ZDateTime(2009, 6, 1, 12, 12, 30);
			populatedPackLine.JL_CustomAttrib1 = "Custom text 1";
			populatedPackLine.JL_CustomAttrib2 = "Custom text 2";
			populatedPackLine.JL_CustomAttrib3 = "Custom text 3";
			populatedPackLine.JL_CustomAttrib4 = "Custom text 4";
			populatedPackLine.JL_CustomDecimal1 = 1234.56m;
			populatedPackLine.JL_CustomDecimal2 = 5678.91m;
			populatedPackLine.JL_CustomFlag1 = true;
			populatedPackLine.JL_CustomFlag2 = false;

			CommonContainer container = populatedPackLine.Containers.AddNew();
			container.JC_JK = consol.PK;
			populatedPackLine.CurrentConsol = consol;

			var expectedOutputFilename = resourceRetriever.Value.SaveResourceToFile("Enterprise.Freight.DataTransfer.Test.Freight.Testing.PopulatedPackLine.xml", "PopulatedPackLine.xml");
			return new BusinessObjectAndExpectedOutputFileName(populatedPackLine, expectedOutputFilename, ValidationKind.Xsd, "Populated pack line");
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects()
		{
			return Array.Empty<BusinessObjectAndExpectedOutputFileName>();
		}

		protected override string[] XmlNodesToExcludeFromCoverageTest
		{
			get
			{
				return new string[]
				{
					// handled by other data adapters
					"DGContact/Organisation",
					"DangerousGoods",
					"HazardousGoods",

					// ??
					"ContainerNumber",
					"Weight/Description",
					"Length/Description",
					"Volume/Description",
					"Height/Description",
					"Width/Description",
					"PackageID",
					"TransportRef",
					"PackageProducts"
				};
			}
		}

		protected override bool IsCreateOrUpdateFromValueObjectSupported
		{
			get { return false; }
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		#endregion
	}
}
