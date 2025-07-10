using CargoWise.EntityFramework;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class AddInfoJobComInvoiceLineValidation_IPTTest : AddInfoJobComInvoiceLineValidation_CUSDECTest
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
			AssertEquals("End use is not required for IPT declaration", true, AddInfoJobComInvoiceLine.SG_EndUseDescriptionInfo.HasMessageErrors());
		}

		protected override string MessageType
		{
			get
			{
				return MessageTypeCodeList.Codes.IPT;
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
