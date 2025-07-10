using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ManifestBase;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.ForwarderManifest.Business.Test
{
	[TestedType(typeof(USExportAsycudaManifestHeaderValidation))]
	public class USExportAsycudaManifestHeaderValidationTest : ManifestBase.Testing.AsycudaTransferHeaderValidationTest
	{
		public void TestCheckAMA_RL_NKPortOfFinaldeparture()
		{
			var locoCode = new ZString("USXXX");
			RefUNLOCOTestDataHelper.CreateLocoMapIfNotExists(Factory, "3900", locoCode, USLocoMapSystemUsageList.Codes.SCD, true);
			RefUNLOCOTestDataHelper.CreateLocoMapIfNotExists(Factory, "3899", locoCode, USLocoMapSystemUsageList.Codes.SCD, false);
			RefUNLOCOTestDataHelper.CreateLocoMapIfNotExists(Factory, "3901", locoCode, USLocoMapSystemUsageList.Codes.SCD, false);
			Factory.Save();

			var usExpHeader = Factory.New<USExportAsycudaManifestHeader>();
			AssertNoMessageErrorContaining(usExpHeader.AMA_RL_NKPortOfFinalDepartureInfo, ListValidation.InvalidCodeMessageError);

			usExpHeader.AMA_RL_NKPortOfFinalDeparture = "*";
			usExpHeader.Validation.ValidateAMA_RL_NKPortOfFinalDeparture();
			AssertHasMessageErrorContaining(usExpHeader.AMA_RL_NKPortOfFinalDepartureInfo, ListValidation.InvalidCodeMessageError);
			usExpHeader.AMA_RL_NKPortOfFinalDeparture = locoCode;
			usExpHeader.Validation.ValidateAMA_RL_NKPortOfFinalDeparture();
			AssertNoMessageErrorContaining(usExpHeader.AMA_RL_NKPortOfFinalDepartureInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckMasterBOL()
		{
			var messageError = "Both the Issuer Code and Bill of Lading Number should be completed if you are entering a master bill. If a master bill is entered here, all bills entered on the Bills tab will then be treated as house bills.\nIf you wish to enter Direct Bills on the Bills tab, please leave both the Issuer Code and Bill of Lading Number on this Header tab blank.";
			var usExpHeader = Factory.New<USExportAsycudaManifestHeader>();
			var masterBill = usExpHeader.MasterBill;

			AssertNoMessageErrorContaining(usExpHeader.MasterBOLInfo, messageError);

			usExpHeader.MasterBOL = "12345678";
			AssertHasMessageErrorContaining(usExpHeader.MasterBOLInfo, messageError);

			masterBill.ABL_BillIssuer = "TEST";
			AssertNoMessageErrorContaining(usExpHeader.MasterBOLInfo, messageError);

			masterBill.ABL_BillIssuer = ZString.Empty;
			AssertHasMessageErrorContaining(usExpHeader.MasterBOLInfo, messageError);

			usExpHeader.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			usExpHeader.Validation.ValidateMasterBOL();
			AssertNoMessageErrorContaining(usExpHeader.MasterBOLInfo, messageError);
		}
	}
}
