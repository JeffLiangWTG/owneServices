using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class TradersRemarkValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_Description()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeCodeList.Codes.COO;
			var tradersRemark1 = declaration.TradersRemarks.AddNew();
			tradersRemark1.CSI_Description = "traders remark 1";
			AssertHasMessageError(tradersRemark1.CSI_DescriptionInfo, "Traders Remarks are NOT sent in a stand-alone Certificate of Origin (COO) declaration. If you need to send free text information for a COO, you should enter it in the Additional Information field on the Certificate of Origin tab.");

			declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			tradersRemark1.Validation.ValidateAll();
			AssertNoMessageError(tradersRemark1.CSI_DescriptionInfo, "Traders Remarks are NOT sent in a stand-alone Certificate of Origin (COO) declaration. If you need to send free text information for a COO, you should enter it in the Additional Information field on the Certificate of Origin tab.");

			var tradersRemark2 = declaration.TradersRemarks.AddNew();
			tradersRemark2.CSI_Description = "traders remark 2";
			var tradersRemark3 = declaration.TradersRemarks.AddNew();
			tradersRemark3.CSI_Description = "traders remark 3";

			tradersRemark2.Validation.ValidateAll();
			tradersRemark3.Validation.ValidateAll();
			AssertHasRowMessageError(tradersRemark3, "Only two lines of Traders Remarks are allowed, except for Outward (OUT) with COO.");

			declaration.SG_ApplicationProductType = ApplicationProductTypeCodeList.Codes.TX;
			tradersRemark3.Validation.ValidateAll();
			AssertNoRowMessageError(tradersRemark3, "Only two lines of Traders Remarks are allowed, except for Outward (OUT) with COO.");
		}
	}
}
