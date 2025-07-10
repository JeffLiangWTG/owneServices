using CargoWise.EntityFramework;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class AddInfoJobComInvoiceLineValidation_INPTest : AddInfoJobComInvoiceLineValidation_CUSDECTest
	{
		public void TestLSP()
		{
			Validation.ValidateSG_LastSellingPrice();
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_LastSellingPriceInfo.HasMessageError(AddInfoJobComInvoiceLineValidation_CUSDEC.LastSellingPriceRequired));
			InvoiceLine.Declaration.SG_SupplyIndicator = SupplyIndicatorCodeList.Codes.Y;
			Validation.ValidateSG_LastSellingPrice();
			AssertEquals(true, AddInfoJobComInvoiceLine.SG_LastSellingPriceInfo.HasMessageError(AddInfoJobComInvoiceLineValidation_CUSDEC.LastSellingPriceRequired));
			AddInfoJobComInvoiceLine.SG_LastSellingPrice = 10m;
			Validation.ValidateSG_LastSellingPrice();
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_LastSellingPriceInfo.HasMessageError(AddInfoJobComInvoiceLineValidation_CUSDEC.LastSellingPriceRequired));
		}

		public void TestEndUseDescription()
		{
			AddInfoJobComInvoiceLine.InvoiceLine.JI_Tariff = tariff.ZZ1_TariffCode;
			AddInfoJobComInvoiceLine.SG_EndUseDescription = "";
			Validation.ValidateSG_EndUseDescription();
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_EndUseDescriptionInfo.HasMessageErrors());
			AddInfoJobComInvoiceLine.SG_EndUseDescription = "End use entered";
			Validation.ValidateSG_EndUseDescription();
			AssertEquals("End use is not required for INP declaration", true, AddInfoJobComInvoiceLine.SG_EndUseDescriptionInfo.HasMessageErrors());
		}

		public void TestCheckSG_TotalDutiableWGTVOLQTY()
		{
			var bW1 = SGPlacesRefCusCodeListTestDataHelper.CreateFACSGPlace(Factory, "BW1", SGCPlaces.Constants.PremiseType.BondedWarehouse, "BW1");
			Factory.Save();
			InvoiceLine.SG_TotalDutiableWGTVOLQTY = 0;
			AssertNoMessageErrors(InvoiceLine.SG_TotalDutiableWGTVOLQTYInfo);
			InvoiceLine.Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.APS;
			InvoiceLine.AddInfoValidation.ValidateSG_TotalDutiableWGTVOLQTY();
			AssertNoMessageErrors(InvoiceLine.SG_TotalDutiableWGTVOLQTYInfo);
			InvoiceLine.Declaration.SG_US_NKPlaceOfReceipt = "BW1";
			InvoiceLine.AddInfoValidation.ValidateSG_TotalDutiableWGTVOLQTY();
			AssertHasMessageError(InvoiceLine.SG_TotalDutiableWGTVOLQTYInfo, "For APS declarations with goods bonded into or released from a Bonded Warehouse, specify the Total Qty/Wgt/Vol");
			InvoiceLine.SG_TotalDutiableWGTVOLQTY = 200;
			InvoiceLine.AddInfoValidation.ValidateSG_TotalDutiableWGTVOLQTY();
			AssertNoMessageErrors(InvoiceLine.SG_TotalDutiableWGTVOLQTYInfo);
		}

		protected override string MessageType
		{
			get
			{
				return MessageTypeCodeList.Codes.INP;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Singapore, Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			tariff = helper.LoadOrCreateNewTariff(tariffType, "85023921");
			Factory.Save();
		}

		UniversalReferenceTestDataHelper helper;
		TariffView tariff;
	}
}
