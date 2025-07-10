using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.UniversalCopy;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Test
{
	[TestedType(typeof(RateOneOffPackLine))]
	public class RateOneOffPackLineTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetBusinessObjectForTest();
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetBusinessObjectForTest();
		}

		public void TestExcludedUniversalCopyFields()
		{
			var elementType = typeof(RateOneOffPackLine);
			var copyTemplateTree = new CopyTemplateTree(elementType, elementType, BusinessObjectCopyManager.CopyTreeConfiguration);
			var innerNode = copyTemplateTree.InnerNode as EntityCopyTemplateNode;

			var cusEntryLineNode = innerNode.Nodes.FirstOrDefault(n => n.Name == "WidthImperial") as PropertyCopyTemplateNode;
			AssertNull("WidthImperial should be excluded from Universal Copy", cusEntryLineNode);

			var cusEntryLinePKNode = innerNode.Nodes.FirstOrDefault(n => n.Name == "HeightImperial") as PropertyCopyTemplateNode;
			AssertNull("HeightImperial should be excluded from Universal Copy", cusEntryLinePKNode);

			var cusEntryInstructionNode = innerNode.Nodes.FirstOrDefault(n => n.Name == "LengthImperial") as PropertyCopyTemplateNode;
			AssertNull("LengthImperial should be excluded from Universal Copy", cusEntryInstructionNode);
		}

		public void TestParentLooseVolume()
		{
			var rate = Factory.New<Quote>();
			rate.TH_OneTimeQuote = true;
			rate.CurrentOneOffQuote.TT_TransportMode = Constants.TransportModes.Sea;
			rate.CurrentOneOffQuote.TT_ContainerMode = Constants.ContainerModes.LCL;
			var loose1 = rate.CurrentOneOffQuote.LooseCargo.AddNew();

			rate.CurrentOneOffQuote.TT_UnitOfVolume = Constants.Volume.CubicMetres;
			loose1.TPL_Height = 3;
			loose1.TPL_Width = 2;
			loose1.TPL_Length = 4;
			loose1.TPL_PackLineCount = 3;
			loose1.TPL_VolumeUQ = Constants.Volume.CubicMetres;
			AssertEquals(72m, loose1.TPL_Volume);
			AssertEquals(72m, rate.CurrentOneOffQuote.TT_ActualVolume);

			rate.CurrentOneOffQuote.TT_UnitOfVolume = Constants.Volume.CubicFeet;
			loose1.TPL_VolumeUQ = Constants.Volume.CubicFeet;
			loose1.TPL_Height = 54;
			loose1.TPL_Width = 75;
			loose1.TPL_Length = 12;
			loose1.TPL_PackLineCount = 2;
			loose1.TPL_DimensionUQ = Constants.Length.Inches;
			AssertEquals(56.25m, loose1.TPL_Volume);
			AssertEquals(56.25m, rate.CurrentOneOffQuote.TT_ActualVolume);
		}

		public void TestParentLooseWeightAndVolumeAfterDelete()
		{
			var rate = Factory.New<Quote>();
			rate.TH_OneTimeQuote = true;
			rate.CurrentOneOffQuote.TT_TransportMode = Constants.TransportModes.Sea;
			rate.CurrentOneOffQuote.TT_ContainerMode = Constants.ContainerModes.LCL;
			rate.CurrentOneOffQuote.TT_UnitOfWeight = Constants.Weight.Kilograms;
			rate.CurrentOneOffQuote.TT_UnitOfVolume = Constants.Volume.CubicMetres;

			var loose1 = rate.CurrentOneOffQuote.LooseCargo.AddNew();
			var loose2 = rate.CurrentOneOffQuote.LooseCargo.AddNew();

			loose1.TPL_Weight = 3;
			loose2.TPL_Weight = 13;

			loose1.TPL_Volume = 11;
			loose2.TPL_Volume = 22;

			AssertEquals(16m, rate.CurrentOneOffQuote.TT_ActualWeight);
			AssertEquals(33m, rate.CurrentOneOffQuote.TT_ActualVolume);

			loose1.Delete();

			AssertEquals(13m, rate.CurrentOneOffQuote.TT_ActualWeight);
			AssertEquals(22m, rate.CurrentOneOffQuote.TT_ActualVolume);
		}

		public void TestSetImperialSize_UnitOfDimensionIsValid_UpdateSizeAccordingToUnitOfDimension()
		{
			var container = CreatePackLine(Core.Constants.Length.Metres);

			container.HeightImperial = "10'0\"";
			container.WidthImperial = "72'2\"";
			container.LengthImperial = "65'7\"";

			AssertEquals(3.048m, container.TPL_Height);
			AssertEquals(21.996m, container.TPL_Width);
			AssertEquals(19.990m, container.TPL_Length);

			container.HeightImperial = "452434\"";
			container.WidthImperial = "54352\"";
			container.LengthImperial = "545327\"";

			AssertEquals(11491.824m, container.TPL_Height);
			AssertEquals(1380.541m, container.TPL_Width);
			AssertEquals(13851.306m, container.TPL_Length);
		}

		public void TestSetImperialSize_UnitOfDimensionIsNotValid_DoNotUpdateSize()
		{
			var container = CreatePackLine("SV");

			container.TPL_Height = 10m;
			container.TPL_Width = 20m;
			container.TPL_Length = 30m;

			container.HeightImperial = "10'0\"";
			container.WidthImperial = "72'2\"";
			container.LengthImperial = "65'7\"";

			AssertEquals(10m, container.TPL_Height);
			AssertEquals(20m, container.TPL_Width);
			AssertEquals(30m, container.TPL_Length);
		}

		public void TestGetImperialSize_UnitOfDimensionIsValid_ReturnImperialSizeAccodungToUnitOfDimentsion()
		{
			var container = CreatePackLine(Core.Constants.Length.Metres);

			container.TPL_Height = 3.048m;
			container.TPL_Width = 22m;
			container.TPL_Length = 20m;

			RatingDataRegistry.Instance.OneOffQuoteImperialUnitsInchesOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			AssertEquals("10'0\"", container.HeightImperial);
			AssertEquals("72'2\"", container.WidthImperial);
			AssertEquals("65'7\"", container.LengthImperial);

			RatingDataRegistry.Instance.OneOffQuoteImperialUnitsInchesOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			AssertEquals("120\"", container.HeightImperial);
			AssertEquals("866\"", container.WidthImperial);
			AssertEquals("787\"", container.LengthImperial);
		}

		public void TestGetImperialSize_UnitOfDimensionIsNotValid_ReturnZeroImperialSize()
		{
			var container = CreatePackLine("SV");

			container.TPL_Height = 10m;
			container.TPL_Width = 20m;
			container.TPL_Length = 30m;

			AssertEquals(ZString.Empty, container.HeightImperial);
			AssertEquals(ZString.Empty, container.WidthImperial);
			AssertEquals(ZString.Empty, container.LengthImperial);
		}

		public void TestSetUnitOfDimension_NewUnitOfDimensionIsValid_UpdateImperialSizeAccordingToNewUnitOfDimension()
		{
			var container = CreatePackLine(Core.Constants.Length.Metres);

			container.TPL_Height = 6m;
			container.TPL_Width = 66m;
			container.TPL_Length = 666m;

			AssertEquals("Precondition", "19'8\"", container.HeightImperial);
			AssertEquals("Precondition", "216'6\"", container.WidthImperial);
			AssertEquals("Precondition", "2185'0\"", container.LengthImperial);

			container.TPL_DimensionUQ = Core.Constants.Length.Kilometres;

			AssertEquals("19685'0\"", container.HeightImperial);
			AssertEquals("216535'5\"", container.WidthImperial);
			AssertEquals("2185039'4\"", container.LengthImperial);
		}

		public void TestSetUnitOfDimension_NewUnitOfDimensionIsNotValid_ResetImperialSizeToZero()
		{
			var container = CreatePackLine(Core.Constants.Length.Metres);

			container.TPL_Height = 6m;
			container.TPL_Width = 66m;
			container.TPL_Length = 666m;

			AssertEquals("Precondition", "19'8\"", container.HeightImperial);
			AssertEquals("Precondition", "216'6\"", container.WidthImperial);
			AssertEquals("Precondition", "2185'0\"", container.LengthImperial);

			container.TPL_DimensionUQ = "SV";

			AssertEquals(ZString.Empty, container.HeightImperial);
			AssertEquals(ZString.Empty, container.WidthImperial);
			AssertEquals(ZString.Empty, container.LengthImperial);
		}

		[ExpectNoExceptions]
		public void TestEnterImperialValues()
		{
			var rate = Factory.New<Quote>();
			rate.TH_OneTimeQuote = true;

			var container = rate.CurrentOneOffQuote.LooseCargo.AddNew();

			container.HeightImperial = "10'10''";
			AssertEquals("10'10\"", container.HeightImperial);

			container.HeightImperial = "3'1\"";
			AssertEquals("3'1\"", container.HeightImperial);

			container.HeightImperial = "1'2";
			AssertEquals("1'2\"", container.HeightImperial);

			container.HeightImperial = "3'";
			AssertEquals("3'0\"", container.HeightImperial);

			container.HeightImperial = "12''";
			AssertEquals("1'0\"", container.HeightImperial);

			container.HeightImperial = "11";
			AssertEquals("0'11\"", container.HeightImperial);

			container.HeightImperial = "7";
			AssertEquals("0'7\"", container.HeightImperial);

			container.HeightImperial = "72";
			AssertEquals("6'0\"", container.HeightImperial);

			container.HeightImperial = " 6";
			AssertEquals("0'6\"", container.HeightImperial);

			container.HeightImperial = "12'' ";
			AssertEquals("1'0\"", container.HeightImperial);

			container.HeightImperial = " 8 ";
			AssertEquals("0'8\"", container.HeightImperial);

			container.HeightImperial = "       ";
			AssertEquals("0'0\"", container.HeightImperial);

			container.HeightImperial = "      0'07\"   ";
			AssertEquals("0'7\"", container.HeightImperial);

			container.HeightImperial = "      0'09''   ";
			AssertEquals("0'9\"", container.HeightImperial);

			container.HeightImperial = "ABC 1'22'' aaa";
			AssertEquals("0'9\"", container.HeightImperial);

			container.HeightImperial = "x1'22''";
			AssertEquals("0'9\"", container.HeightImperial);

			container.HeightImperial = "1'x22''";
			AssertEquals("0'9\"", container.HeightImperial);

			container.HeightImperial = "1'22''aaa";
			AssertEquals("0'9\"", container.HeightImperial);

			container.HeightImperial = "";
			AssertEquals("0'0\"", container.HeightImperial);

			container.HeightImperial = " 42 ";
			AssertEquals("3'6\"", container.HeightImperial);

			container.HeightImperial = null;
			AssertEquals("0'0\"", container.HeightImperial);
		}

		[ExpectNoExceptions]
		public void TestEnterImperialValuesSupportedScenarios()
		{
			var rate = Factory.New<Quote>();
			rate.TH_OneTimeQuote = true;

			var container = rate.CurrentOneOffQuote.LooseCargo.AddNew();

			container.HeightImperial = "6'3''";
			AssertEquals("6'3\"", container.HeightImperial);

			container.HeightImperial = "3'2\"";
			AssertEquals("3'2\"", container.HeightImperial);

			container.HeightImperial = "4'2";
			AssertEquals("4'2\"", container.HeightImperial);

			container.HeightImperial = "4'";
			AssertEquals("4'0\"", container.HeightImperial);

			container.HeightImperial = "8";
			AssertEquals("0'8\"", container.HeightImperial);

			container.HeightImperial = "7''";
			AssertEquals("0'7\"", container.HeightImperial);

			container.HeightImperial = "9\"";
			AssertEquals("0'9\"", container.HeightImperial);
		}

		#region TestSetupDefaultWeightAndDimensionsForPackType

		public void TestSetupDefaultWeightAndDimensionsForPackType()
		{
			var packType = Factory.New<RefPackType>();
			packType.F3_Code = "BAG";
			packType.F3_Description = "BAGGS";
			packType.F3_Length = 100;
			packType.F3_Height = 200;
			packType.F3_Width = 300;
			packType.F3_UnitOfDimension = "KM";
			packType.F3_Weight = 400;
			packType.F3_UnitOfWeight = "T";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_FullName = "CONSIGNEE";
			consignee.OH_Code = "CONS";
			consignee.OH_IsConsignor = false;
			consignee.OH_IsConsignee = true;

			var currentPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
			var otherCountryFilter = new ZQuery(RefCountrySchema.RN_Code, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			var otherCountry = Factory.LoadTop1<RefCountry>(otherCountryFilter);
			var otherUnloco1 = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, otherCountry.RN_Code));
			otherCountryFilter.AddToFilter(RefCountrySchema.RN_Code, SQLComparisonOperator.NotEqual, otherCountry.RN_Code);
			var otherCountry2 = Factory.LoadTop1<RefCountry>(otherCountryFilter);
			var otherUnloco2 = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, otherCountry2.RN_Code));
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_RL_NKClosestPort = otherUnloco1.RL_Code;

			var link1 = consignor.BuyerLinks.AddNew(consignee);
			link1.OL_RN_NKImporterCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var link2 = consignor.BuyerLinks.AddNew(consignee);
			link2.OL_RN_NKImporterCountry = otherCountry.RN_Code;

			var quote = Factory.New<Quote>();
			quote.TH_OneTimeQuote = true;
			quote.CurrentOneOffQuote.DeliveryDocAddress.OrganisationPK = consignee.PK;
			quote.CurrentOneOffQuote.PickUpDocAddress.OrganisationPK = consignor.PK;
			var rateOneOffShipment = quote.CurrentOneOffQuote;
			rateOneOffShipment.TT_RL_NKDeliveryLocation = currentPort.RL_Code;

			var packLine = rateOneOffShipment.LooseCargo.AddNew();
			packLine.TPL_F3_NKPackType = "BAG";

			packLine.TPL_PackLineCount = 2;
			AssertEquals(packLine.TPL_F3_NKPackType, packType.F3_Code);
			AssertEquals(100m, packLine.TPL_Length);
			AssertEquals(200m, packLine.TPL_Height);
			AssertEquals(300m, packLine.TPL_Width);
			AssertEquals("KM", packLine.TPL_DimensionUQ);
			AssertEquals(800m, packLine.TPL_Weight);
			AssertEquals("T", packLine.TPL_WeightUQ);

			packType.F3_Length = 101;
			packType.F3_Height = 201;
			packType.F3_Width = 301;
			packType.F3_Weight = 0;

			packLine.TPL_PackLineCount = 3;
			AssertEquals(101m, packLine.TPL_Length);
			AssertEquals(201m, packLine.TPL_Height);
			AssertEquals(301m, packLine.TPL_Width);
			AssertEquals(800m, packLine.TPL_Weight);

			var packageDetails1 = link1.PackPivots.AddNew();
			packageDetails1.Q0_F3 = packType.PK;
			packageDetails1.Q0_Length = 0;
			packageDetails1.Q0_Height = 20;
			packageDetails1.Q0_Width = 0;
			packageDetails1.Q0_UnitOfDimension = "CM";
			packageDetails1.Q0_Weight = 0;
			packageDetails1.Q0_UnitOfWeight = "KG";
			var packageDetails2 = link2.PackPivots.AddNew();
			packageDetails2.Q0_F3 = packType.PK;
			packageDetails2.Q0_Length = 1;
			packageDetails2.Q0_Height = 30;
			packageDetails2.Q0_Width = 2;
			packageDetails2.Q0_UnitOfDimension = "CM";
			packageDetails2.Q0_Weight = 3;
			packageDetails2.Q0_UnitOfWeight = "KG";

			packType.F3_Weight = 400;

			packLine.TPL_PackLineCount = 4;
			AssertEquals(packType.F3_Code, packLine.TPL_F3_NKPackType);
			AssertEquals(0m, packLine.TPL_Length);
			AssertEquals(20m, packLine.TPL_Height);
			AssertEquals(0m, packLine.TPL_Width);
			AssertEquals("CM", packLine.TPL_DimensionUQ);
			AssertEquals(1600m, packLine.TPL_Weight);
			AssertEquals("T", packLine.TPL_WeightUQ);

			packageDetails1.Q0_Weight = 40;
			packageDetails1.Q0_Height = 0;
			packageDetails1.Q0_Length = 0;
			packageDetails1.Q0_Width = 0;

			packLine.TPL_PackLineCount = 5;
			AssertEquals(101m, packLine.TPL_Length);
			AssertEquals(201m, packLine.TPL_Height);
			AssertEquals(301m, packLine.TPL_Width);
			AssertEquals("KM", packLine.TPL_DimensionUQ);
			AssertEquals(200m, packLine.TPL_Weight);
			AssertEquals("KG", packLine.TPL_WeightUQ);

			rateOneOffShipment.TT_RL_NKDeliveryLocation = otherUnloco2.RL_Code;
			packLine.TPL_PackLineCount = 6;
			AssertEquals(1m, packLine.TPL_Length);
			AssertEquals(30m, packLine.TPL_Height);
			AssertEquals(2m, packLine.TPL_Width);
			AssertEquals("CM", packLine.TPL_DimensionUQ);
			AssertEquals(18m, packLine.TPL_Weight);
			AssertEquals("KG", packLine.TPL_WeightUQ);

			consignee.OH_RL_NKClosestPort = otherUnloco2.RL_Code;
			packLine.TPL_PackLineCount = 7;

			AssertEquals(101m, packLine.TPL_Length);
			AssertEquals(201m, packLine.TPL_Height);
			AssertEquals(301m, packLine.TPL_Width);
			AssertEquals("KM", packLine.TPL_DimensionUQ);
			AssertEquals(2800m, packLine.TPL_Weight);
			AssertEquals("T", packLine.TPL_WeightUQ);
		}

		#endregion

		public void TestRounding()
		{
			var quote = Factory.New<Quote>();
			quote.TH_OneTimeQuote = true;
			quote.CurrentOneOffQuote.TT_TransportMode = Constants.TransportModes.Sea;
			quote.CurrentOneOffQuote.TT_ContainerMode = Constants.ContainerModes.LCL;
			quote.CurrentOneOffQuote.TT_UnitOfWeight = Constants.Weight.Kilograms;
			quote.CurrentOneOffQuote.TT_UnitOfVolume = Constants.Volume.CubicMetres;

			var collection = new DefaultNumberOfDecimalsCollection(Module.Freight);
			RateOneOffShipmentTest.AddDefaultNumberOfDecimals(collection, Constants.TransportModes.Sea, Constants.Weight.Kilograms, numberOfDecimals: 2, RoundingModes.Up);
			RateOneOffShipmentTest.AddDefaultNumberOfDecimals(collection, Constants.TransportModes.Sea, Constants.Weight.Pounds, numberOfDecimals: 1, RoundingModes.Up);
			RateOneOffShipmentTest.AddDefaultNumberOfDecimals(collection, Constants.TransportModes.Sea, Constants.Volume.CubicMetres, numberOfDecimals: 2, RoundingModes.Down);
			RateOneOffShipmentTest.AddDefaultNumberOfDecimals(collection, Constants.TransportModes.Sea, Constants.Volume.CubicFeet, numberOfDecimals: 1, RoundingModes.Down);

			using (FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				var loose = quote.CurrentOneOffQuote.LooseCargo.AddNew();
				loose.TPL_Weight = 1.1234;
				loose.TPL_Volume = 5.6789;

				CombineAssertions("GIVEN matching DefaultNumberOfDecimalPlaces=2 THEN Weight & Volume should be rounded to 2 decimal places", () =>
				{
					AssertEquals("Weight", 1.13m, loose.TPL_Weight);
					AssertEquals("Volume", 5.67m, loose.TPL_Volume);
				});
			}
		}

		public void TestRounding_WhenAddingNewPacklineOnOOQ_ThenVolumeShouldBeRoundedUpCorrectly()
		{
			AssertVolumeMeasurement(1, RoundingModes.Up, 1, 277, 107, 85, 2.6m, "Unrounded volume is 2.519315, should be rounded up to 2.6");
			AssertVolumeMeasurement(2, RoundingModes.Up, 1, 277, 107, 85, 2.52m, "Unrounded volume is 2.519315, should be rounded up to 2.52");
			AssertVolumeMeasurement(3, RoundingModes.Up, 1, 277, 107, 85, 2.520m, "Unrounded volume is 2.519315, should be rounded up to 2.520");
		}

		public void TestRounding_WhenAddingNewPacklineOnOOQ_ThenVolumeShouldBeRoundedDownCorrectly()
		{
			AssertVolumeMeasurement(1, RoundingModes.Down, 1, 277, 107, 85, 2.5m, "Unrounded volume is 2.519315, should be rounded down to 2.5");
			AssertVolumeMeasurement(2, RoundingModes.Down, 1, 277, 107, 85, 2.51m, "Unrounded volume is 2.519315, should be rounded down to 2.51");
			AssertVolumeMeasurement(3, RoundingModes.Down, 1, 277, 107, 85, 2.519m, "Unrounded volume is 2.519315, should be rounded down to2.519");
		}

		public void TestLooseCargoContainerType()
		{
			var gp20 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			var gp40 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");

			var quote = Factory.New<Quote>();
			quote.TH_OneTimeQuote = true;
			quote.CurrentOneOffQuote.TT_TransportMode = Constants.RateMode.ULD;

			var looseCargo = quote.CurrentOneOffQuote.LooseCargo.AddNew();
			AssertEquals("Loose Cargo Container Type will be empty when TC_RC has not been set.", ZString.Empty, looseCargo.LooseCargoContainerType);

			looseCargo.TPL_RC_RefContainer = ZGuid.NewZGuid(); // Some Invalid Contianer Type
			AssertEquals("Loose Cargo Container Type will be empty when TC_RC is not valid.", ZString.Empty, looseCargo.LooseCargoContainerType);

			looseCargo.TPL_RC_RefContainer = gp20.PK;
			AssertEquals("Loose Cargo Container Type will be RefContainer Code of the RefContainer that it's PK is assigned to TC_RC.", "20GP", looseCargo.LooseCargoContainerType);

			looseCargo.LooseCargoContainerType = "40GP";
			AssertEquals("TC_RC will be RefContainer Code of the RefContainer that it's Code is assigned to LooseCargoContainerType.", gp40.PK, looseCargo.TPL_RC_RefContainer);

			looseCargo.LooseCargoContainerType = "XXXY";
			AssertEquals("TC_RC will be empty when setting LooseCargoContainerType to an invalid packLine type.", ZGuid.Empty, looseCargo.TPL_RC_RefContainer);
		}

		void AssertVolumeMeasurement(ZInt numberOfDecimals, ZString roundingMode, ZShort packLineCount, ZDecimal length, ZDecimal width, ZDecimal height, ZDecimal expectedVolume, string message)
		{
			var quote = Factory.New<Quote>();
			quote.TH_OneTimeQuote = true;
			quote.CurrentOneOffQuote.TT_TransportMode = Constants.TransportModes.Sea;
			quote.CurrentOneOffQuote.TT_ContainerMode = Constants.ContainerModes.LCL;
			quote.CurrentOneOffQuote.TT_UnitOfVolume = Constants.Volume.CubicMetres;

			var collection = new DefaultNumberOfDecimalsCollection(Module.Freight);
			RateOneOffShipmentTest.AddDefaultNumberOfDecimals(collection, Constants.TransportModes.Sea, Constants.Volume.CubicMetres, numberOfDecimals, roundingMode);

			using (FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				var loose = quote.CurrentOneOffQuote.LooseCargo.AddNew();
				loose.TPL_DimensionUQ = Enterprise.Core.Constants.Length.Centimetres;
				loose.TPL_PackLineCount = packLineCount;
				loose.TPL_Length = length;
				loose.TPL_Width = width;
				loose.TPL_Height = height;

				AssertEquals(message, expectedVolume, loose.TPL_Volume);
			}
		}

		#region implementation

		RateOneOffPackLine GetBusinessObjectForTest()
		{
			var result = Factory.NewWithValidTestData<RateOneOffPackLine>();
			return result;
		}

		RateOneOffPackLine CreatePackLine(ZString unitOfDimension)
		{
			var rate = Factory.New<Quote>();
			rate.TH_OneTimeQuote = true;

			var packLine = rate.CurrentOneOffQuote.LooseCargo.AddNew();
			packLine.TPL_DimensionUQ = unitOfDimension;

			return packLine;
		}

		#endregion
	}
}
