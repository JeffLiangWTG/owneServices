using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class AddInfoJobComInvoiceLineValidationTest : SGAddInfoValidationTest
	{
		public void TestInmostPackQuantity()
		{
			Validation.ValidateSG_InmostPackQuantity();
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_InmostPackQuantityInfo.HasMessageErrors());
			AddInfoJobComInvoiceLine.SG_InmostPackQuantity = -1;
			Validation.ValidateSG_InmostPackQuantity();
			AssertEquals(true, AddInfoJobComInvoiceLine.SG_InmostPackQuantityInfo.HasErrors());
			AddInfoJobComInvoiceLine.SG_InmostPackQuantity = 1;
			Validation.ValidateSG_InmostPackQuantity();
			AssertHasMessageError("Packing units must be entered in sequence", AddInfoJobComInvoiceLine.SG_InmostPackQuantityInfo, AddInfoJobComInvoiceLineValidation.MustBeInSequence);
			AddInfoJobComInvoiceLine.SG_OuterPackQuantity = 4;
			AddInfoJobComInvoiceLine.SG_InPackQuantity = 3;
			AddInfoJobComInvoiceLine.SG_InnerPackQuantity = 2;
			Validation.ValidateSG_InmostPackQuantity();
			AssertNoMessageError("Packing units must be entered in sequence", AddInfoJobComInvoiceLine.SG_InmostPackQuantityInfo, AddInfoJobComInvoiceLineValidation.MustBeInSequence);
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_InmostPackQuantityInfo.HasMessageErrors());
			AddInfoJobComInvoiceLine.SG_InmostPackQuantity = 99999999;
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_InmostPackQuantityInfo.HasErrors());
			AddInfoJobComInvoiceLine.SG_InmostPackQuantity = 100000000;
			AssertEquals(true, AddInfoJobComInvoiceLine.SG_InmostPackQuantityInfo.HasErrors());
		}

		public void TestInmostQuantity()
		{
			Validation.ValidateSG_InmostPackQuantity();
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_InmostPackQuantityInfo.HasMessageErrors());
			AddInfoJobComInvoiceLine.SG_InmostPackQuantityUnit = UnitOfQuantityCodeList.Codes.CEN;
			Validation.ValidateSG_InmostPackQuantity();
			AssertEquals(true, AddInfoJobComInvoiceLine.SG_InmostPackQuantityInfo.HasWarnings());
			AddInfoJobComInvoiceLine.SG_InmostPackQuantity = -1;
			Validation.ValidateSG_InmostPackQuantity();
			AssertEquals(true, AddInfoJobComInvoiceLine.SG_InmostPackQuantityInfo.HasErrors());
			AddInfoJobComInvoiceLine.SG_InmostPackQuantity = 1;
			AssertHasMessageError("Packing units must be entered in sequence", AddInfoJobComInvoiceLine.SG_InmostPackQuantityInfo, AddInfoJobComInvoiceLineValidation.MustBeInSequence);
			AddInfoJobComInvoiceLine.SG_OuterPackQuantity = 4;
			AddInfoJobComInvoiceLine.SG_InPackQuantity = 3;
			AddInfoJobComInvoiceLine.SG_InnerPackQuantity = 2;
			Validation.ValidateSG_InmostPackQuantity();
			AssertNoMessageError("Packing units must be entered in sequence", AddInfoJobComInvoiceLine.SG_InmostPackQuantityInfo, AddInfoJobComInvoiceLineValidation.MustBeInSequence);
			Validation.ValidateSG_InmostPackQuantity();
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_InmostPackQuantityInfo.HasNotifications());
		}

		public void TestInmostUQ()
		{
			Validation.ValidateSG_InmostPackQuantityUnit();
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_InmostPackQuantityUnitInfo.HasMessageErrors());
			AddInfoJobComInvoiceLine.SG_InmostPackQuantity = 1;
			Validation.ValidateSG_InmostPackQuantityUnit();
			AssertEquals(true, AddInfoJobComInvoiceLine.SG_InmostPackQuantityUnitInfo.HasWarnings());
			AddInfoJobComInvoiceLine.SG_InmostPackQuantityUnit = UnitOfQuantityCodeList.Codes.CEN;
			Validation.ValidateSG_InmostPackQuantityUnit();
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_InmostPackQuantityUnitInfo.HasNotifications());
		}

		public void TestInnerQuantity()
		{
			Validation.ValidateSG_InnerPackQuantity();
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_InnerPackQuantityInfo.HasMessageErrors());
			AddInfoJobComInvoiceLine.SG_InnerPackQuantityUnit = UnitOfQuantityCodeList.Codes.CEN;
			Validation.ValidateSG_InnerPackQuantity();
			AssertEquals(true, AddInfoJobComInvoiceLine.SG_InnerPackQuantityInfo.HasWarnings());
			AddInfoJobComInvoiceLine.SG_InnerPackQuantity = -1;
			Validation.ValidateSG_InnerPackQuantity();
			AssertEquals(true, AddInfoJobComInvoiceLine.SG_InnerPackQuantityInfo.HasErrors());
			AddInfoJobComInvoiceLine.SG_InnerPackQuantity = 1;
			Validation.ValidateSG_InnerPackQuantity();
			AssertHasMessageError("Packing units must be entered in sequence", AddInfoJobComInvoiceLine.SG_InnerPackQuantityInfo, AddInfoJobComInvoiceLineValidation.MustBeInSequence);
			AddInfoJobComInvoiceLine.SG_OuterPackQuantity = 4;
			AddInfoJobComInvoiceLine.SG_InPackQuantity = 3;
			Validation.ValidateSG_InnerPackQuantity();
			AssertNoMessageError("Packing units must be entered in sequence", AddInfoJobComInvoiceLine.SG_InnerPackQuantityInfo, AddInfoJobComInvoiceLineValidation.MustBeInSequence);
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_InnerPackQuantityInfo.HasNotifications());
			AddInfoJobComInvoiceLine.SG_InnerPackQuantity = 99999999;
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_InnerPackQuantityInfo.HasErrors());
			AddInfoJobComInvoiceLine.SG_InnerPackQuantity = 100000000;
			AssertEquals(true, AddInfoJobComInvoiceLine.SG_InnerPackQuantityInfo.HasErrors());
		}

		public void TestInnerUQ()
		{
			Validation.ValidateSG_InnerPackQuantityUnit();
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_InnerPackQuantityUnitInfo.HasMessageErrors());
			AddInfoJobComInvoiceLine.SG_InnerPackQuantity = 1;
			Validation.ValidateSG_InnerPackQuantityUnit();
			AssertEquals(true, AddInfoJobComInvoiceLine.SG_InnerPackQuantityUnitInfo.HasWarnings());
			AddInfoJobComInvoiceLine.SG_InnerPackQuantityUnit = UnitOfQuantityCodeList.Codes.CEN;
			Validation.ValidateSG_InnerPackQuantityUnit();
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_InnerPackQuantityUnitInfo.HasNotifications());
		}

		public void TestInQuantity()
		{
			Validation.ValidateSG_InPackQuantity();
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_InPackQuantityInfo.HasMessageErrors());
			AddInfoJobComInvoiceLine.SG_InPackQuantityUnit = UnitOfQuantityCodeList.Codes.CEN;
			Validation.ValidateSG_InPackQuantity();
			AssertEquals(true, AddInfoJobComInvoiceLine.SG_InPackQuantityInfo.HasWarnings());
			AddInfoJobComInvoiceLine.SG_InPackQuantity = -1;
			Validation.ValidateSG_InPackQuantity();
			AssertEquals(true, AddInfoJobComInvoiceLine.SG_InPackQuantityInfo.HasErrors());
			AddInfoJobComInvoiceLine.SG_InPackQuantity = 1;
			Validation.ValidateSG_InPackQuantity();
			AssertHasMessageError("Packing units must be entered in sequence", AddInfoJobComInvoiceLine.SG_InPackQuantityInfo, AddInfoJobComInvoiceLineValidation.MustBeInSequence);
			AddInfoJobComInvoiceLine.SG_OuterPackQuantity = 4;
			Validation.ValidateSG_InPackQuantity();
			AssertNoMessageError("Packing units must be entered in sequence", AddInfoJobComInvoiceLine.SG_InPackQuantityInfo, AddInfoJobComInvoiceLineValidation.MustBeInSequence);
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_InPackQuantityInfo.HasNotifications());
			AddInfoJobComInvoiceLine.SG_InPackQuantity = 99999999;
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_InPackQuantityInfo.HasErrors());
			AddInfoJobComInvoiceLine.SG_InPackQuantity = 100000000;
			AssertEquals(true, AddInfoJobComInvoiceLine.SG_InPackQuantityInfo.HasErrors());
		}

		public void TestInUQ()
		{
			Validation.ValidateSG_InPackQuantityUnit();
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_InPackQuantityUnitInfo.HasMessageErrors());
			AddInfoJobComInvoiceLine.SG_InPackQuantity = 1;
			Validation.ValidateSG_InPackQuantityUnit();
			AssertEquals(true, AddInfoJobComInvoiceLine.SG_InPackQuantityUnitInfo.HasWarnings());
			AddInfoJobComInvoiceLine.SG_InPackQuantityUnit = UnitOfQuantityCodeList.Codes.CEN;
			Validation.ValidateSG_InPackQuantityUnit();
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_InPackQuantityUnitInfo.HasNotifications());
		}

		public virtual void TestOuterQuantity()
		{
			Validation.ValidateSG_OuterPackQuantity();
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_OuterPackQuantityInfo.HasMessageErrors());
			AddInfoJobComInvoiceLine.SG_OuterPackQuantityUnit = UnitOfQuantityCodeList.Codes.CEN;
			Validation.ValidateSG_OuterPackQuantity();
			AssertEquals(true, AddInfoJobComInvoiceLine.SG_OuterPackQuantityInfo.HasWarnings());
			AddInfoJobComInvoiceLine.SG_OuterPackQuantity = -1;
			Validation.ValidateSG_OuterPackQuantity();
			AssertEquals(true, AddInfoJobComInvoiceLine.SG_OuterPackQuantityInfo.HasErrors());
			AddInfoJobComInvoiceLine.SG_OuterPackQuantity = 1;
			Validation.ValidateSG_OuterPackQuantity();
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_OuterPackQuantityInfo.HasNotifications());
			AddInfoJobComInvoiceLine.SG_OuterPackQuantity = 99999999;
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_OuterPackQuantityInfo.HasErrors());
			AddInfoJobComInvoiceLine.SG_OuterPackQuantity = 100000000;
			AssertEquals(true, AddInfoJobComInvoiceLine.SG_OuterPackQuantityInfo.HasErrors());
		}

		public void TestOuterUQ()
		{
			Validation.ValidateSG_OuterPackQuantityUnit();
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_OuterPackQuantityUnitInfo.HasMessageErrors());
			AddInfoJobComInvoiceLine.SG_OuterPackQuantity = 1;
			Validation.ValidateSG_OuterPackQuantityUnit();
			AssertEquals(true, AddInfoJobComInvoiceLine.SG_OuterPackQuantityUnitInfo.HasWarnings());
			AddInfoJobComInvoiceLine.SG_OuterPackQuantityUnit = UnitOfQuantityCodeList.Codes.CEN;
			Validation.ValidateSG_OuterPackQuantityUnit();
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_OuterPackQuantityUnitInfo.HasNotifications());
		}

		public void TestUnitDutiableWGTVOLQTY()
		{
			Validation.ValidateSG_UnitDutiableWGTVOLQTY();
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_UnitDutiableWGTVOLQTYInfo.HasMessageErrors());
			AddInfoJobComInvoiceLine.SG_UnitDutiableWGTVOLQTYUnit = UnitOfQuantityCodeList.Codes.CEN;
			Validation.ValidateSG_UnitDutiableWGTVOLQTY();
			AssertEquals(true, AddInfoJobComInvoiceLine.SG_UnitDutiableWGTVOLQTYInfo.HasWarnings());
			AddInfoJobComInvoiceLine.SG_UnitDutiableWGTVOLQTY = -1;
			Validation.ValidateSG_UnitDutiableWGTVOLQTY();
			AssertEquals(true, AddInfoJobComInvoiceLine.SG_UnitDutiableWGTVOLQTYInfo.HasErrors());
			AddInfoJobComInvoiceLine.SG_UnitDutiableWGTVOLQTY = 1;
			Validation.ValidateSG_UnitDutiableWGTVOLQTY();
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_UnitDutiableWGTVOLQTYInfo.HasNotifications());
		}

		[NUnit.Framework.TestDate(2007, 12, 31)]
		public void TestUnitDutiableWGTVOLQTYUnit()
		{
			Validation.ValidateSG_UnitDutiableWGTVOLQTYUnit();
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_UnitDutiableWGTVOLQTYUnitInfo.HasMessageErrors());
			AddInfoJobComInvoiceLine.SG_UnitDutiableWGTVOLQTY = 1;
			Validation.ValidateSG_UnitDutiableWGTVOLQTYUnit();
			AssertEquals(true, AddInfoJobComInvoiceLine.SG_UnitDutiableWGTVOLQTYUnitInfo.HasMessageErrors());
			AddInfoJobComInvoiceLine.SG_UnitDutiableWGTVOLQTYUnit = UnitOfQuantityCodeList.Codes.KGM;
			Validation.ValidateSG_UnitDutiableWGTVOLQTYUnit();
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_UnitDutiableWGTVOLQTYUnitInfo.HasNotifications());
		}

		[NUnit.Framework.TestDate(2014, 05, 01)]
		public void TestUnitDutiableWGTVOLQTYUnit2()
		{
			AddInfoJobComInvoiceLine.InvoiceLine.JI_Tariff = "22030010";
			AddInfoJobComInvoiceLine.SG_UnitDutiableWGTVOLQTYUnit = UnitOfQuantityCodeList.Codes.LTR;
			Validation.ValidateSG_UnitDutiableWGTVOLQTYUnit();
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_UnitDutiableWGTVOLQTYUnitInfo.HasMessageErrors());
			AddInfoJobComInvoiceLine.InvoiceLine.JI_Tariff = "01011000";
			AddInfoJobComInvoiceLine.SG_UnitDutiableWGTVOLQTYUnit = UnitOfQuantityCodeList.Codes.KGM;
			Validation.ValidateSG_UnitDutiableWGTVOLQTYUnit();
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_UnitDutiableWGTVOLQTYUnitInfo.HasMessageErrors());
			AddInfoJobComInvoiceLine.InvoiceLine.JI_Tariff = "87032392";
			AddInfoJobComInvoiceLine.SG_UnitDutiableWGTVOLQTYUnit = UnitOfQuantityCodeList.Codes.CTN;
			Validation.ValidateSG_UnitDutiableWGTVOLQTYUnit();
			AssertEquals(true, AddInfoJobComInvoiceLine.SG_UnitDutiableWGTVOLQTYUnitInfo.HasWarnings());
			AssertHasWarning(AddInfoJobComInvoiceLine.SG_UnitDutiableWGTVOLQTYUnitInfo, "For dutiable commodities, UQ is generally required as one of: " + UnitOfQuantityCodeList.Codes.DAL + ", " + UnitOfQuantityCodeList.Codes.KGM + ", " + UnitOfQuantityCodeList.Codes.LTR + ", " + UnitOfQuantityCodeList.Codes.NMB + " or " + UnitOfQuantityCodeList.Codes.STK + ". You have not entered a valid code.");
		}

		public void TestTotalDutiableWGTVOLQTY()
		{
			Validation.ValidateSG_TotalDutiableWGTVOLQTY();
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_TotalDutiableWGTVOLQTYInfo.HasMessageErrors());
			AddInfoJobComInvoiceLine.SG_TotalDutiableWGTVOLQTYUnit = UnitOfQuantityCodeList.Codes.CEN;
			Validation.ValidateSG_TotalDutiableWGTVOLQTY();
			AssertEquals(true, AddInfoJobComInvoiceLine.SG_TotalDutiableWGTVOLQTYInfo.HasWarnings());
			AddInfoJobComInvoiceLine.SG_TotalDutiableWGTVOLQTY = -1;
			Validation.ValidateSG_TotalDutiableWGTVOLQTY();
			AssertEquals(true, AddInfoJobComInvoiceLine.SG_TotalDutiableWGTVOLQTYInfo.HasErrors());
			AddInfoJobComInvoiceLine.SG_TotalDutiableWGTVOLQTY = 1;
			Validation.ValidateSG_TotalDutiableWGTVOLQTY();
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_TotalDutiableWGTVOLQTYInfo.HasNotifications());
		}

		[NUnit.Framework.TestDate(2007, 12, 31)]
		public void TestTotalDutiableWGTVOLQTYUnit()
		{
			Validation.ValidateSG_TotalDutiableWGTVOLQTYUnit();
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_TotalDutiableWGTVOLQTYUnitInfo.HasMessageErrors());
			AddInfoJobComInvoiceLine.SG_TotalDutiableWGTVOLQTY = 1;
			Validation.ValidateSG_TotalDutiableWGTVOLQTYUnit();
			AssertEquals(true, AddInfoJobComInvoiceLine.SG_TotalDutiableWGTVOLQTYUnitInfo.HasMessageErrors());
			AddInfoJobComInvoiceLine.SG_TotalDutiableWGTVOLQTYUnit = UnitOfQuantityCodeList.Codes.KGM;
			Validation.ValidateSG_TotalDutiableWGTVOLQTYUnit();
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_TotalDutiableWGTVOLQTYUnitInfo.HasNotifications());
		}

		[NUnit.Framework.TestDate(2014, 05, 01)]
		public void TestTotalDutiableWGTVOLQTYUnit2()
		{
			AddInfoJobComInvoiceLine.InvoiceLine.JI_Tariff = "22030010";
			AddInfoJobComInvoiceLine.SG_TotalDutiableWGTVOLQTYUnit = UnitOfQuantityCodeList.Codes.LTR;
			Validation.ValidateSG_TotalDutiableWGTVOLQTYUnit();
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_TotalDutiableWGTVOLQTYUnitInfo.HasMessageErrors());
			AddInfoJobComInvoiceLine.InvoiceLine.JI_Tariff = "01011000";
			AddInfoJobComInvoiceLine.SG_TotalDutiableWGTVOLQTYUnit = UnitOfQuantityCodeList.Codes.KGM;
			Validation.ValidateSG_TotalDutiableWGTVOLQTYUnit();
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_TotalDutiableWGTVOLQTYUnitInfo.HasMessageErrors());
			AddInfoJobComInvoiceLine.InvoiceLine.JI_Tariff = "87032392";
			AddInfoJobComInvoiceLine.SG_TotalDutiableWGTVOLQTYUnit = UnitOfQuantityCodeList.Codes.CTN;
			Validation.ValidateSG_TotalDutiableWGTVOLQTYUnit();
			AssertEquals(true, AddInfoJobComInvoiceLine.SG_TotalDutiableWGTVOLQTYUnitInfo.HasWarnings());
			AssertHasWarning(AddInfoJobComInvoiceLine.SG_TotalDutiableWGTVOLQTYUnitInfo, "For dutiable commodities, UQ is generally required as one of: " + UnitOfQuantityCodeList.Codes.DAL + ", " + UnitOfQuantityCodeList.Codes.KGM + ", " + UnitOfQuantityCodeList.Codes.LTR + ", " + UnitOfQuantityCodeList.Codes.NMB + " or " + UnitOfQuantityCodeList.Codes.STK + ". You have not entered a valid code.");
		}

		public void TestCurrentLotNumber()
		{
			SGPlacesRefCusCodeListTestDataHelper.CreateSGPlaces(Factory);
			SGPlacesRefCusCodeListTestDataHelper.CreateFACSGPlace(Factory, "BW1", SGCPlaces.Constants.PremiseType.BondedWarehouse, "BW1");
			SGPlacesRefCusCodeListTestDataHelper.CreateFACSGPlace(Factory, "LW1", SGCPlaces.Constants.PremiseType.LicensedWarehouse, "LW1");
			Factory.Save();
			Validation.ValidateSG_LotNo();
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_LotNoInfo.HasMessageErrors());
			Declaration.SG_US_NKPlaceOfStorage = "BW1";
			Validation.ValidateSG_LotNo();
			AssertEquals(true, AddInfoJobComInvoiceLine.SG_LotNoInfo.HasMessageErrors());
			AddInfoJobComInvoiceLine.InvoiceLine.Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.BKT;
			Validation.ValidateSG_LotNo();
			AssertEquals(true, AddInfoJobComInvoiceLine.SG_LotNoInfo.HasMessageErrors());
			AddInfoJobComInvoiceLine.InvoiceLine.Declaration.JE_MessageSubType = "";
			AddInfoJobComInvoiceLine.InvoiceLine.Declaration.SG_US_NKPlaceOfStorage = "";
			AddInfoJobComInvoiceLine.InvoiceLine.JI_Tariff = "00000000";
			Validation.ValidateSG_LotNo();
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_LotNoInfo.HasMessageErrors());
			AddInfoJobComInvoiceLine.InvoiceLine.JI_Tariff = "01019010";
			Validation.ValidateSG_LotNo();
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_LotNoInfo.HasMessageErrors());
			AddInfoJobComInvoiceLine.InvoiceLine.JI_Tariff = "21069061";
			Validation.ValidateSG_LotNo();
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_LotNoInfo.HasMessageErrors());
			AddInfoJobComInvoiceLine.InvoiceLine.Declaration.SG_US_NKPlaceOfStorage = "LW1";
			Validation.ValidateSG_LotNo();
			AssertEquals(true, AddInfoJobComInvoiceLine.SG_LotNoInfo.HasMessageErrors());
			AddInfoJobComInvoiceLine.InvoiceLine.Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.BKT;
			Validation.ValidateSG_LotNo();
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_LotNoInfo.HasMessageErrors());
			AddInfoJobComInvoiceLine.InvoiceLine.Declaration.JE_MessageSubType = "";
			AddInfoJobComInvoiceLine.SG_LotNo = "LOTNO";
			Validation.ValidateSG_LotNo();
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_LotNoInfo.HasMessageErrors());
		}

		public void TestAlcoholPercentage()
		{
			AddInfoJobComInvoiceLine.SG_PercAlcohol = 0;
			Validation.ValidateSG_PercAlcohol();
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_PercAlcoholInfo.HasMessageErrors());
			AddInfoJobComInvoiceLine.InvoiceLine.JI_Tariff = "22051020";
			Validation.ValidateSG_PercAlcohol();
			AssertEquals(true, AddInfoJobComInvoiceLine.SG_PercAlcoholInfo.HasMessageErrors());
		}

		[NUnit.Framework.TestDate(2014, 05, 01)]
		public void TestAlcoholPercentage2()
		{
			AddInfoJobComInvoiceLine.InvoiceLine.JI_Tariff = "22043020";
			Validation.ValidateSG_PercAlcohol();
			AssertEquals(true, AddInfoJobComInvoiceLine.SG_PercAlcoholInfo.HasMessageErrors());
			AddInfoJobComInvoiceLine.SG_PercAlcohol = 10;
			Validation.ValidateSG_PercAlcohol();
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_PercAlcoholInfo.HasMessageErrors());
			AddInfoJobComInvoiceLine.SG_PercAlcohol = -1;
			Validation.ValidateSG_PercAlcohol();
			AssertEquals(true, AddInfoJobComInvoiceLine.SG_PercAlcoholInfo.HasErrors());
			AddInfoJobComInvoiceLine.SG_PercAlcohol = 101;
			Validation.ValidateSG_PercAlcohol();
			AssertEquals(true, AddInfoJobComInvoiceLine.SG_PercAlcoholInfo.HasWarnings());
		}

		public void TestTobaccoMultiplier()
		{
			AddInfoJobComInvoiceLine.SG_TobaccoMultiplier = -1;
			Validation.ValidateSG_TobaccoMultiplier();
			AssertEquals(true, AddInfoJobComInvoiceLine.SG_TobaccoMultiplierInfo.HasErrors());
			AddInfoJobComInvoiceLine.SG_TobaccoMultiplier = 101;
			Validation.ValidateSG_TobaccoMultiplier();
			AssertEquals(true, AddInfoJobComInvoiceLine.SG_TobaccoMultiplierInfo.HasWarnings());
			AddInfoJobComInvoiceLine.SG_TobaccoMultiplier = 10;
			Validation.ValidateSG_TobaccoMultiplier();
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_TobaccoMultiplierInfo.HasNotifications());
		}

		public void TestDutyPercentageRate()
		{
			AddInfoJobComInvoiceLine.SG_DutyPercentageRate = -1;
			Validation.ValidateSG_DutyPercentageRate();
			AssertEquals(true, AddInfoJobComInvoiceLine.SG_DutyPercentageRateInfo.HasErrors());
			AddInfoJobComInvoiceLine.SG_DutyPercentageRate = 101;
			Validation.ValidateSG_DutyPercentageRate();
			AssertEquals(true, AddInfoJobComInvoiceLine.SG_DutyPercentageRateInfo.HasWarnings());
			AddInfoJobComInvoiceLine.SG_DutyPercentageRate = 10;
			Validation.ValidateSG_DutyPercentageRate();
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_DutyPercentageRateInfo.HasNotifications());
		}

		public void TestExcisePercentageRate()
		{
			AddInfoJobComInvoiceLine.SG_ExcisePercentageRate = -1;
			Validation.ValidateSG_ExcisePercentageRate();
			AssertEquals(true, AddInfoJobComInvoiceLine.SG_ExcisePercentageRateInfo.HasErrors());
			AddInfoJobComInvoiceLine.SG_ExcisePercentageRate = 101;
			Validation.ValidateSG_ExcisePercentageRate();
			AssertEquals(true, AddInfoJobComInvoiceLine.SG_ExcisePercentageRateInfo.HasWarnings());
			AddInfoJobComInvoiceLine.SG_ExcisePercentageRate = 10;
			Validation.ValidateSG_ExcisePercentageRate();
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_ExcisePercentageRateInfo.HasNotifications());
		}

		public void TestDutyUnitRate()
		{
			AddInfoJobComInvoiceLine.SG_DutyUnitRate = -1;
			Validation.ValidateSG_DutyUnitRate();
			AssertEquals(true, AddInfoJobComInvoiceLine.SG_DutyUnitRateInfo.HasErrors());
			AddInfoJobComInvoiceLine.SG_DutyUnitRate = 1;
			Validation.ValidateSG_DutyUnitRate();
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_DutyUnitRateInfo.HasErrors());
		}

		public void TestExciseUnitRate()
		{
			AddInfoJobComInvoiceLine.SG_ExciseUnitRate = -1;
			Validation.ValidateSG_ExciseUnitRate();
			AssertEquals(true, AddInfoJobComInvoiceLine.SG_ExciseUnitRateInfo.HasErrors());
			AddInfoJobComInvoiceLine.SG_ExciseUnitRate = 1;
			Validation.ValidateSG_ExciseUnitRate();
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_ExciseUnitRateInfo.HasErrors());
		}

		public void TestPercContent()
		{
			AddInfoJobComInvoiceLine.SG_PercContent = -1;
			Validation.ValidateSG_PercContent();
			AssertEquals(true, AddInfoJobComInvoiceLine.SG_PercContentInfo.HasErrors());
			AddInfoJobComInvoiceLine.SG_PercContent = 101;
			Validation.ValidateSG_PercContent();
			AssertEquals(true, AddInfoJobComInvoiceLine.SG_PercContentInfo.HasWarnings());
			AddInfoJobComInvoiceLine.SG_PercContent = 10;
			Validation.ValidateSG_PercContent();
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_PercContentInfo.HasNotifications());
		}

		public void TestCertItemQuantity()
		{
			AddInfoJobComInvoiceLine.SG_CertItemQuantityUnit = UnitOfQuantityCodeList.Codes.CEN;
			Validation.ValidateSG_CertItemQuantity();
			AssertEquals(true, AddInfoJobComInvoiceLine.SG_CertItemQuantityInfo.HasWarnings());
			AddInfoJobComInvoiceLine.SG_CertItemQuantity = -1;
			Validation.ValidateSG_CertItemQuantity();
			AssertEquals(true, AddInfoJobComInvoiceLine.SG_CertItemQuantityInfo.HasErrors());
			AddInfoJobComInvoiceLine.SG_CertItemQuantity = 10;
			Validation.ValidateSG_CertItemQuantity();
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_CertItemQuantityInfo.HasNotifications());
			AddInfoJobComInvoiceLine.SG_CertItemQuantity = 99999999999.9999m;
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_OuterPackQuantityInfo.HasErrors());
			AddInfoJobComInvoiceLine.SG_CertItemQuantity = 100000000000m;
			AssertEquals(true, AddInfoJobComInvoiceLine.SG_CertItemQuantityInfo.HasErrors());
		}

		public void TestCertItemQuantityUnit()
		{
			AddInfoJobComInvoiceLine.SG_CertItemQuantity = 1;
			Validation.ValidateSG_CertItemQuantityUnit();
			AssertEquals(true, AddInfoJobComInvoiceLine.SG_CertItemQuantityUnitInfo.HasWarnings());
			AddInfoJobComInvoiceLine.SG_CertItemQuantityUnit = "A";
			Validation.ValidateSG_CertItemQuantityUnit();
			AssertEquals(true, AddInfoJobComInvoiceLine.SG_CertItemQuantityUnitInfo.HasMessageErrors());
			AddInfoJobComInvoiceLine.SG_CertItemQuantityUnit = UnitOfQuantityCodeList.Codes.CEN;
			Validation.ValidateSG_CertItemQuantityUnit();
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_CertItemQuantityUnitInfo.HasNotifications());
		}

		public void TestCheckSG_CertItemValue()
		{
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			Declaration.SG_ApplicationProductType = ApplicationProductTypeCodeList.Codes.TX;
			Declaration.SG_Cert1Type = "9";
			AddInfoJobComInvoiceLine.SG_CertItemValue = -1;
			AssertHasErrorContaining(AddInfoJobComInvoiceLine.SG_CertItemValueInfo, MandatoryValidation.ValueCannotBeNegative);
			AddInfoJobComInvoiceLine.SG_CertItemValue = 10;
			AssertNoErrorContaining(AddInfoJobComInvoiceLine.SG_CertItemValueInfo, MandatoryValidation.ValueCannotBeNegative);
			Declaration.SG_Cert1Type = "4";
			Validation.ValidateSG_CertItemValue();
			AssertHasMessageErrorContaining(AddInfoJobComInvoiceLine.SG_CertItemValueInfo, "Item value. Certificate Item value is not applicable for this Certificate Type declaration.");
			Declaration.SG_Cert1Type = "9";
			Validation.ValidateSG_CertItemValue();
			AssertNoMessageErrorContaining(AddInfoJobComInvoiceLine.SG_CertItemValueInfo, "Item value. Certificate Item value is not applicable for this Certificate Type declaration.");
		}

		public void TestOriginCriterionForOUT()
		{
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			Declaration.SG_ApplicationProductType = ApplicationProductTypeCodeList.Codes.TX;
			Declaration.SG_Cert1Type = "1";
			Validation.ValidateSG_CertOriginCriterion1();
			AssertHasMessageErrorContaining(AddInfoJobComInvoiceLine.SG_CertOriginCriterion1Info, "Origin Criterion. It is mandatory to specify Origin Criterion details for this Certificate Type.");
			AddInfoJobComInvoiceLine.SG_CertOriginCriterion1 = "CWC";
			AssertNoMessageErrorContaining(AddInfoJobComInvoiceLine.SG_CertOriginCriterion1Info, "Origin Criterion. It is mandatory to specify Origin Criterion details for this Certificate Type.");
			Declaration.SG_Cert1Type = "9";
			Validation.ValidateSG_CertOriginCriterion1();
			AssertHasMessageErrorContaining(AddInfoJobComInvoiceLine.SG_CertOriginCriterion1Info, "Origin Criterion. Origin Criterion details are not applicable for this Certificate Type declaration.");
			AddInfoJobComInvoiceLine.SG_CertOriginCriterion1 = ZString.Empty;
			Validation.ValidateSG_CertOriginCriterion1();
			AssertNoMessageErrorContaining(AddInfoJobComInvoiceLine.SG_CertOriginCriterion1Info, "Origin Criterion. Origin Criterion details are not applicable for this Certificate Type declaration.");
			Declaration.SG_Cert1Type = "16";
			AddInfoJobComInvoiceLine.SG_CertOriginCriterion1 = "TEST";
			AssertNoMessageErrors(AddInfoJobComInvoiceLine.SG_CertOriginCriterion1Info);
		}

		public void TestEngineCapacityPower()
		{
			AddInfoJobComInvoiceLine.SG_EngineCapacityPowerUnit = EngineCapacityCodeList.Codes.CC;
			Validation.ValidateSG_EngineCapacityPower();
			AssertEquals(true, AddInfoJobComInvoiceLine.SG_EngineCapacityPowerInfo.HasWarnings());
			AddInfoJobComInvoiceLine.SG_EngineCapacityPower = -1;
			Validation.ValidateSG_EngineCapacityPower();
			AssertEquals(true, AddInfoJobComInvoiceLine.SG_EngineCapacityPowerInfo.HasErrors());
			AddInfoJobComInvoiceLine.SG_EngineCapacityPower = 10000;
			Validation.ValidateSG_EngineCapacityPower();
			AssertEquals(true, AddInfoJobComInvoiceLine.SG_EngineCapacityPowerInfo.HasNotifications());
			AddInfoJobComInvoiceLine.SG_EngineCapacityPower = 10;
			Validation.ValidateSG_EngineCapacityPower();
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_EngineCapacityPowerInfo.HasNotifications());
		}

		public void TestEngineCapacityPowerUnit()
		{
			AddInfoJobComInvoiceLine.SG_EngineCapacityPower = 1;
			Validation.ValidateSG_EngineCapacityPowerUnit();
			AssertEquals(true, AddInfoJobComInvoiceLine.SG_EngineCapacityPowerUnitInfo.HasWarnings());
			AddInfoJobComInvoiceLine.SG_EngineCapacityPowerUnit = "A";
			Validation.ValidateSG_EngineCapacityPowerUnit();
			AssertEquals(true, AddInfoJobComInvoiceLine.SG_EngineCapacityPowerUnitInfo.HasMessageErrors());
			AddInfoJobComInvoiceLine.SG_EngineCapacityPowerUnit = EngineCapacityCodeList.Codes.CC;
			Validation.ValidateSG_EngineCapacityPowerUnit();
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_EngineCapacityPowerUnitInfo.HasNotifications());
		}

		public void TestTextileQuotaQuantity()
		{
			AddInfoJobComInvoiceLine.SG_TextileQuotaQuantityUnit = UnitOfQuantityCodeList.Codes.CEN;
			Validation.ValidateSG_TextileQuotaQuantity();
			AssertEquals(true, AddInfoJobComInvoiceLine.SG_TextileQuotaQuantityInfo.HasWarnings());
			AddInfoJobComInvoiceLine.SG_TextileQuotaQuantity = -1;
			Validation.ValidateSG_TextileQuotaQuantity();
			AssertEquals(true, AddInfoJobComInvoiceLine.SG_TextileQuotaQuantityInfo.HasErrors());
			AddInfoJobComInvoiceLine.SG_TextileQuotaQuantity = 10;
			Validation.ValidateSG_TextileQuotaQuantity();
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_TextileQuotaQuantityInfo.HasNotifications());
		}

		public void TestESNDPIndicator()
		{
			AddInfoJobComInvoiceLine.SG_ESNDPIndicator = "A";
			Validation.ValidateSG_ESNDPIndicator();
			AssertEquals(true, AddInfoJobComInvoiceLine.SG_ESNDPIndicatorInfo.HasMessageErrors());
			AddInfoJobComInvoiceLine.SG_ESNDPIndicator = MarkingCodeList.Codes.HW;
			Validation.ValidateSG_ESNDPIndicator();
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_ESNDPIndicatorInfo.HasMessageErrors());
		}

		public void TestTariffCommodityType()
		{
			AddInfoJobComInvoiceLine.SG_TariffCommodityType = "A";
			Validation.ValidateSG_TariffCommodityType();
			AssertEquals(true, AddInfoJobComInvoiceLine.SG_TariffCommodityTypeInfo.HasMessageErrors());
			AddInfoJobComInvoiceLine.SG_TariffCommodityType = CommodityTypeList.Codes.Alcohol;
			Validation.ValidateSG_TariffCommodityType();
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_TariffCommodityTypeInfo.HasMessageErrors());
		}

		public void TestIsStrategicGoods()
		{
			InvoiceLine.Declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			InvoiceLine.SG_IsStrategic = false;
			AssertEquals(false, InvoiceLine.SG_CategoryCodeInfo.HasMessageErrors());
			AssertEquals(false, InvoiceLine.SG_StrategicGoodsProductCodeQuantityUnitInfo.HasMessageErrors());
			AssertEquals(false, InvoiceLine.SG_EndUseCode1Info.HasMessageErrors());
			AssertEquals(false, InvoiceLine.SG_EndUseCode2Info.HasMessageErrors());
			AssertEquals(false, InvoiceLine.SG_EndUseCode3Info.HasMessageErrors());
			AssertEquals(false, InvoiceLine.SG_EndUseDescriptionInfo.HasMessageErrors());
			InvoiceLine.SG_IsStrategic = true;
			AssertEquals(true, InvoiceLine.SG_IsStrategicInfo.HasMessageErrors());
			InvoiceLine.Declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			InvoiceLine.SG_IsStrategic = false;
			AssertEquals(false, InvoiceLine.SG_IsStrategicInfo.HasMessageErrors());
			InvoiceLine.SG_IsStrategic = true;
			AssertEquals(false, InvoiceLine.SG_IsStrategicInfo.HasMessageErrors());
			AssertEquals(true, InvoiceLine.SG_CategoryCodeInfo.HasMessageErrors());
			AssertEquals(true, InvoiceLine.SG_EndUseDescriptionInfo.HasMessageErrors());
		}

		public void TestStrategicGoodsProductQty()
		{
			InvoiceLine.Declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			InvoiceLine.SG_IsStrategic = true;
			InvoiceLine.SG_StrategicGoodsProductCodeQuantity = 5;
			InvoiceLine.SG_StrategicGoodsProductCodeQuantityUnit = "";
			AssertHasMessageErrorContaining(InvoiceLine.SG_StrategicGoodsProductCodeQuantityUnitInfo, "Product Qty Unit of Qty must be entered when entering quantity.");

			InvoiceLine.SG_StrategicGoodsProductCodeQuantityUnit = "XXX";
			AssertHasMessageError(InvoiceLine.SG_StrategicGoodsProductCodeQuantityUnitInfo, ListValidation.InvalidCodeMessageError);

			InvoiceLine.SG_StrategicGoodsProductCodeQuantityUnit = ProductCodeUQList.Codes.BAG;
			AssertNoMessageErrors(InvoiceLine.SG_StrategicGoodsProductCodeQuantityUnitInfo);
		}

		public void TestStrategicGoodsProvidesWarning()
		{
			InvoiceLine.Declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			InvoiceLine.SG_IsStrategic = true;
			AssertEquals(true, InvoiceLine.SG_IsStrategicInfo.HasWarning("Please note that Strategic Goods control is based on the actual product description and technical specifications of the goods, and not by the HS Code. Traders are advised to refer to the Strategic Goods (Control) Order for up-to-date information in relation to the definitions and scope of goods subject to controls."));
		}

		public void TestConsigneeErrorOnStrategicGoods()
		{
			InvoiceLine.Declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			InvoiceLine.SG_IsStrategic = true;
			Validation.ValidateSG_IsStrategic();
			AssertEquals(true, AddInfoJobComInvoiceLine.InvoiceLine.Declaration.JE_OH_ConsigneeInfo.HasMessageErrors());
			OrgHeader consignee = Factory.LoadTop1<OrgHeader>(new ZQuery());
			consignee.OH_IsConsignee = true;
			AddInfoJobComInvoiceLine.InvoiceLine.Declaration.JE_OH_Consignee = consignee.PK;
			Validation.ValidateSG_IsStrategic();
			AssertEquals(false, AddInfoJobComInvoiceLine.InvoiceLine.Declaration.JE_OH_ConsigneeInfo.HasMessageErrors());
		}

		public void TestStrategicCategoryCode()
		{
			InvoiceLine.Declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			InvoiceLine.SG_IsStrategic = true;
			Validation.ValidateSG_CategoryCode();
			AssertEquals(false, AddInfoJobComInvoiceLine.InvoiceLine.SG_CategoryCodeInfo.HasWarnings());
			InvoiceLine.SG_StrategicGoodsCategory = StrategicGoodsCategoryList.Codes.Cat0;
			InvoiceLine.SG_CategoryCode = "DL0A001a";
			Validation.ValidateSG_CategoryCode();
			AssertEquals(true, AddInfoJobComInvoiceLine.InvoiceLine.SG_CategoryCodeInfo.HasWarnings());
			InvoiceLine.SG_CategoryCode = "DL0A001";
			Validation.ValidateSG_CategoryCode();
			AssertEquals(false, AddInfoJobComInvoiceLine.InvoiceLine.SG_CategoryCodeInfo.HasWarnings());
		}

		public void TestCheckSG_CertHSCode()
		{
			Declaration.SG_Cert1Type = "9";
			AddInfoJobComInvoiceLine.SG_CertOriginCriterion1 = OriginCriterionCodeList.Codes.GSPFormA_W;
			Validation.ValidateSG_CertHSCode();
			AssertEquals(true, AddInfoJobComInvoiceLine.SG_CertHSCodeInfo.HasMessageError("When using this Origin Criterion, HS Code is required."));
			AddInfoJobComInvoiceLine.SG_CertHSCode = "837012";
			Validation.ValidateSG_CertHSCode();
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_CertHSCodeInfo.HasMessageError("When using this Origin Criterion, HS Code is required."));
		}

		public void TestCheckSG_PercContent()
		{
			Declaration.SG_Cert1Type = "16";
			AddInfoJobComInvoiceLine.SG_CertOriginCriterion1 = "ACFTA";
			Validation.ValidateSG_PercContent();
			AssertEquals(true, AddInfoJobComInvoiceLine.SG_PercContentInfo.HasWarning("When using this Origin Criterion, % Content may be required."));
			AddInfoJobComInvoiceLine.SG_PercContent = 45;
			Validation.ValidateSG_PercContent();
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_PercContentInfo.HasWarning("When using this Origin Criterion, % Content may be required."));
		}

		public void TestCheckSG_SG_FirstRegistrationDate()
		{
			InvoiceLine.SG_FirstRegistrationDate = ZDateTime.Today.AddYears(-11);
			AssertNoErrors(InvoiceLine.SG_FirstRegistrationDateInfo);
			AssertHasWarningContaining(InvoiceLine.SG_FirstRegistrationDateInfo, "is more than 10 years old");
		}

		#region Overrides
		protected override AddInfo GetNewAddInfo()
		{
			return AddInfoJobComInvoiceLine;
		}

		#endregion
		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Singapore, Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();

			var tariff1 = helper.LoadOrCreateNewTariff(tariffType, "87032392");
			helper.CreateTariffAttribute(SGConstants.Attributes.Names.CommodityType, CommodityTypeList.Codes.Alcohol, tariff1);
			var tariff2 = helper.LoadOrCreateNewTariff(tariffType, "21069061");
			helper.CreateTariffAttribute(SGConstants.Attributes.Names.CommodityType, CommodityTypeList.Codes.Vehicle, tariff2);
			var tariff3 = helper.LoadOrCreateNewTariff(tariffType, "22051020");
			helper.CreateTariffUOM(tariff3, Constants.UnitOfMeasureTypes.StatisticalUOMType, "LTR");
			helper.CreateTariffUOM(tariff3, Constants.UnitOfMeasureTypes.AdditionalUOMType, SGConstants.LPA);
			var tariff4 = helper.LoadOrCreateNewTariff(tariffType, "22043020");
			helper.CreateTariffUOM(tariff4, Constants.UnitOfMeasureTypes.StatisticalUOMType, "LTR");
			helper.CreateTariffUOM(tariff4, Constants.UnitOfMeasureTypes.AdditionalUOMType, SGConstants.LPA);
			Factory.Save();

			SGCertificateTypeHelper.CreateCertificateTypes(Factory);
			Factory.Save();
		}

		UniversalReferenceTestDataHelper helper;
		#region Declaration
		protected JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = MessageType;
				}

				return declaration;
			}
		}

		JobDeclaration declaration;
		protected virtual string MessageType
		{
			get
			{
				return "";
			}
		}

		#endregion
		#region InvoiceHeader
		protected JobComInvoiceHeader InvoiceHeader
		{
			get
			{
				return invoiceHeader ?? (invoiceHeader = Declaration.Invoices.AddNew());
			}
		}

		JobComInvoiceHeader invoiceHeader;
		#endregion
		#region InvoiceLine
		protected JobComInvoiceLine InvoiceLine
		{
			get
			{
				return invoiceLine ?? (invoiceLine = InvoiceHeader.JobComInvoiceLines.AddNew());
			}
		}

		JobComInvoiceLine invoiceLine;
		#endregion
		#region Validation
		protected AddInfoJobComInvoiceLineValidation Validation
		{
			get
			{
				return AddInfoJobComInvoiceLine.Validation;
			}
		}

		protected AddInfoJobComInvoiceLine AddInfoJobComInvoiceLine
		{
			get
			{
				return addInfoJobComInvoiceLine ?? (addInfoJobComInvoiceLine = new AddInfoJobComInvoiceLine(InvoiceLine.JI_AddInfoInfo));
			}
		}

		AddInfoJobComInvoiceLine addInfoJobComInvoiceLine;
		#endregion
		#endregion
	}
}
