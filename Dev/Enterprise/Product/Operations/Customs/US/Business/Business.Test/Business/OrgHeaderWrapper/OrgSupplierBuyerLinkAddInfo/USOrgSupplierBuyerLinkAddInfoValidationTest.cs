using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	public class USOrgSupplierBuyerLinkAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckZO_FirstSale()
		{
			var link = Factory.New<OrgSupplierBuyerLink>();
			var addInfo = new USOrgSupplierBuyerLinkAddInfo(link.GetAddInfo());
			addInfo.ZO_FirstSale = "H";
			AssertHasMessageErrorContaining(addInfo.ZO_FirstSaleInfo, ListValidation.InvalidCodeMessageError);

			addInfo.ZO_FirstSale = YesNoDefaultList.Codes.Yes;
			AssertNoMessageErrorContaining(addInfo.ZO_FirstSaleInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckZO_OtherReconIndicator()
		{
			var link = Factory.New<OrgSupplierBuyerLink>();
			var addInfo = new USOrgSupplierBuyerLinkAddInfo(link.GetAddInfo());
			addInfo.ZO_OtherReconIndicator = "~";
			AssertHasMessageError(addInfo.ZO_OtherReconIndicatorInfo, ListValidation.InvalidCodeMessageError);

			addInfo.ZO_OtherReconIndicator = ReconIssueCodeList.Codes.Class9802Recon;
			AssertNoMessageError(addInfo.ZO_OtherReconIndicatorInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckZO_AESUltConsigneeType()
		{
			var link = Factory.New<OrgSupplierBuyerLink>();
			var addInfo = new USOrgSupplierBuyerLinkAddInfo(link.GetAddInfo());
			addInfo.ZO_AESUltConsigneeType = "~";
			AssertHasMessageError(addInfo.ZO_AESUltConsigneeTypeInfo, ListValidation.InvalidCodeMessageError);

			addInfo.ZO_AESUltConsigneeType = UltimateConsigneeTypeList.Codes.DirectConsumer;
			AssertNoMessageError(addInfo.ZO_AESUltConsigneeTypeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckZO_EntryType()
		{
			var link = Factory.New<OrgSupplierBuyerLink>();
			var addInfo = new USOrgSupplierBuyerLinkAddInfo(link.GetAddInfo());
			addInfo.ZO_EntryType = "~";
			AssertHasMessageError(addInfo.ZO_EntryTypeInfo, ListValidation.InvalidCodeMessageError);

			addInfo.ZO_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			AssertNoMessageError(addInfo.ZO_EntryTypeInfo, ListValidation.InvalidCodeMessageError);
		}
	}
}
