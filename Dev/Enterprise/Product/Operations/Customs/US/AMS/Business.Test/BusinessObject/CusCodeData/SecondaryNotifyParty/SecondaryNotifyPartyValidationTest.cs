using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	class SecondaryNotifyPartyValidationTest : CargoWise.EntityFramework.Testing.BusinessObjectValidationTestCase
	{
		public void TestCheckCY_Code()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var snp = bill.SecondaryNotifyParties.AddNew();
			snp.CY_Code = "!@";
			AssertNoNotifications(snp.CY_CodeInfo);
			snp.CY_Code = ZString.Empty;
			AssertNoNotifications(snp.CY_CodeInfo);
		}

		public void TestCheckCY_Data()
		{
			var scac1 = Factory.New<USCarrierCombined>();
			scac1.UI_Code = "Z!!1";
			var scac2 = Factory.New<USCarrierCombined>();
			scac2.UI_Code = "Z!!2";
			Factory.Save();
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			bill.ValidationModes = ValidationModes.InventoryRecord;
			header.BH_CarrierSCAC = "Z!!1";

			var snp1 = bill.SecondaryNotifyParties.AddNew();
			var snp2 = bill.SecondaryNotifyParties.AddNew();
			AssertEquals((short)1, snp1.CY_Order);
			AssertEquals((short)2, snp2.CY_Order);
			bill.B0_BillStatus = BillOfLadingStatusIndicatorList.Codes.Sec321HouseBill;
			snp1.CY_Data = ZString.Empty;
			AssertNoMessageError(snp1.CY_DataInfo, ValidationConstants.SecondaryNotifyParty.FirstSNPShouldBeCarrierSCACForNVOCC.ToString());
			AssertNoMessageErrorContaining(snp1.CY_DataInfo, ListValidation.InvalidCodeMessageError);

			snp1.CY_Data = "Z!!3";
			AssertNoMessageError(snp1.CY_DataInfo, ValidationConstants.SecondaryNotifyParty.FirstSNPShouldBeCarrierSCACForNVOCC.ToString());
			AssertHasMessageErrorContaining(snp1.CY_DataInfo, ListValidation.InvalidCodeMessageError);

			snp1.CY_Data = "Z!!2";
			AssertNoMessageError(snp1.CY_DataInfo, ValidationConstants.SecondaryNotifyParty.FirstSNPShouldBeCarrierSCACForNVOCC.ToString());
			AssertNoMessageErrorContaining(snp1.CY_DataInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrors(snp1.CY_DataInfo);

			bill.B0_BillStatus = BillOfLadingStatusIndicatorList.Codes.HouseBill;
			snp1.CY_Data = "Z!!2";
			AssertNoMessageError(snp1.CY_DataInfo, ValidationConstants.SecondaryNotifyParty.FirstSNPShouldBeCarrierSCACForNVOCC.ToString());
			AssertNoMessageErrorContaining(snp1.CY_DataInfo, ListValidation.InvalidCodeMessageError);

			snp1.CY_Data = "Z!!1";
			AssertNoMessageError(snp1.CY_DataInfo, ValidationConstants.SecondaryNotifyParty.FirstSNPShouldBeCarrierSCACForNVOCC.ToString());
			AssertNoMessageErrorContaining(snp1.CY_DataInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrors(snp1.CY_DataInfo);

			snp1.CY_Data = "Z!!3";
			AssertNoMessageError(snp1.CY_DataInfo, ValidationConstants.SecondaryNotifyParty.FirstSNPShouldBeCarrierSCACForNVOCC.ToString());
			AssertHasMessageErrorContaining(snp1.CY_DataInfo, ListValidation.InvalidCodeMessageError);

			snp1.CY_Order = 2;
			AssertNoMessageError(snp1.CY_DataInfo, ValidationConstants.SecondaryNotifyParty.FirstSNPShouldBeCarrierSCACForNVOCC.ToString());
			AssertHasMessageErrorContaining(snp1.CY_DataInfo, ListValidation.InvalidCodeMessageError);

			snp1.CY_Data = ZString.Empty;
			AssertNoMessageError(snp1.CY_DataInfo, ValidationConstants.SecondaryNotifyParty.FirstSNPShouldBeCarrierSCACForNVOCC.ToString());
			AssertNoMessageErrorContaining(snp1.CY_DataInfo, ListValidation.InvalidCodeMessageError);

			snp1.CY_Order = 1;
			AssertHasMessageError(snp1.CY_DataInfo, ValidationConstants.SecondaryNotifyParty.FirstSNPShouldBeCarrierSCACForNVOCC.ToString());
			AssertNoMessageErrorContaining(snp1.CY_DataInfo, ListValidation.InvalidCodeMessageError);

			bill.B0_BillStatus = BillOfLadingStatusIndicatorList.Codes.FROB;
			snp1.CY_Data = ZString.Empty;
			AssertHasMessageError(snp1.CY_DataInfo, ValidationConstants.SecondaryNotifyParty.FirstSNPShouldBeCarrierSCACForNVOCC.ToString());
			AssertNoMessageErrorContaining(snp1.CY_DataInfo, ListValidation.InvalidCodeMessageError);

			snp1.CY_Data = "Z!!3";
			AssertNoMessageError(snp1.CY_DataInfo, ValidationConstants.SecondaryNotifyParty.FirstSNPShouldBeCarrierSCACForNVOCC.ToString());
			AssertHasMessageErrorContaining(snp1.CY_DataInfo, ListValidation.InvalidCodeMessageError);

			snp1.CY_Data = "Z!!2";
			AssertNoMessageError(snp1.CY_DataInfo, ValidationConstants.SecondaryNotifyParty.FirstSNPShouldBeCarrierSCACForNVOCC.ToString());
			AssertNoMessageErrorContaining(snp1.CY_DataInfo, ListValidation.InvalidCodeMessageError);

			snp1.CY_Data = "Z!!1";
			AssertNoMessageError(snp1.CY_DataInfo, ValidationConstants.SecondaryNotifyParty.FirstSNPShouldBeCarrierSCACForNVOCC.ToString());
			AssertNoMessageErrorContaining(snp1.CY_DataInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrors(snp1.CY_DataInfo);

			bill.ValidationModes = ValidationModes.None;
			snp1.CY_Data = ZString.Empty;
			AssertNoMessageError(snp1.CY_DataInfo, ValidationConstants.SecondaryNotifyParty.FirstSNPShouldBeCarrierSCACForNVOCC.ToString());
			AssertNoMessageErrorContaining(snp1.CY_DataInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrors(snp1.CY_DataInfo);

			snp1.CY_Data = "Z!!3";
			AssertNoMessageError(snp1.CY_DataInfo, ValidationConstants.SecondaryNotifyParty.FirstSNPShouldBeCarrierSCACForNVOCC.ToString());
			AssertNoMessageErrorContaining(snp1.CY_DataInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrors(snp1.CY_DataInfo);
		}
	}
}
