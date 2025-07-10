using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.TR.Manifest.Business.Testing
{
	public class AsycudaBillForMasterChildValidationTest : BusinessObjectValidationTestCase
	{
		public void TestPortOfLoadingAndDischargeNeedsIATA()
		{
			using (var helper = new ProviderTestHelper(Factory))
			{
				var header = helper.GetProviderHeader();
				header.AMA_TransportMode = "AIR";
				header.AMA_RL_NKPortOfLoading = "";
				header.AMA_RL_NKPortOfDischarge = "";
				CombineAssertions("When both Load & Discharge Ports are empty", () =>
				{
					AssertHasErrorContaining(header.MasterBill.ABL_RL_NKPortOfLoadingInfo, "The manifest has to load from or discharge in one of the supported countries or tranship through one.");
					AssertHasErrorContaining(header.MasterBill.ABL_RL_NKPortOfDischargeInfo, "The manifest has to load from or discharge in one of the supported countries or tranship through one.");
				});
				header.AMA_RL_NKPortOfLoading = "TRALI";
				CombineAssertions("When one of them(Load here) is filled not including IATA code", () =>
				{
					AssertNoErrorContaining(header.MasterBill.ABL_RL_NKPortOfLoadingInfo, "The manifest has to load from or discharge in one of the supported countries or tranship through one.");
					AssertNoErrorContaining(header.MasterBill.ABL_RL_NKPortOfDischargeInfo, "The manifest has to load from or discharge in one of the supported countries or tranship through one.");
					AssertHasMessageError(header.MasterBill.ABL_RL_NKPortOfDischargeInfo, "You have not entered a value.");
					AssertHasErrorContaining(header.MasterBill.ABL_RL_NKPortOfLoadingInfo, "Selected Port Of Loading cannot be used for AIR transport");
					AssertNoMessageError(header.MasterBill.ABL_RL_NKPortOfLoadingInfo, "You have not entered a value.");
				});
				header.AMA_RL_NKPortOfDischarge = "TRAJI";
				CombineAssertions("When the other one(Discharge here) also filled including IATA code", () =>
				{
					AssertNoMessageError(header.MasterBill.ABL_RL_NKPortOfDischargeInfo, "You have not entered a value.");
					AssertNoErrorContaining(header.MasterBill.ABL_RL_NKPortOfDischargeInfo, "Selected Port Of Loading cannot be used for AIR transport");
				});
				header.AMA_RL_NKPortOfLoading = "TRAFY";
				CombineAssertions("When both of them filled including IATA code", () =>
				{
					AssertNoMessageError(header.MasterBill.ABL_RL_NKPortOfLoadingInfo, "You have not entered a value.");
					AssertNoMessageError(header.MasterBill.ABL_RL_NKPortOfDischargeInfo, "You have not entered a value.");
					AssertNoErrorContaining(header.MasterBill.ABL_RL_NKPortOfLoadingInfo, "Selected Port Of Loading cannot be used for AIR transport");
					AssertNoErrorContaining(header.MasterBill.ABL_RL_NKPortOfDischargeInfo, "Selected Port Of Loading cannot be used for AIR transport");
				});
			}
		}

		public void TestCheckAMA_RL_NKPortOfLoading()
		{
			using (var helper = new ProviderTestHelper(Factory))
			{
				var header = helper.GetProviderHeader();
				CombineAssertions("Port of Loading", () =>
				{
					header.AMA_ManifestType = TRManifestTypes.Codes.TESLIM;
					header.AMA_RL_NKPortOfLoading = ZString.Empty;
					AssertHasMessageErrorContaining("TESLIM error, change value ABL_RL_NKPortOfLoading", header.MasterBill.ABL_RL_NKPortOfLoadingInfo, MandatoryValidation.YouHaveNotEntered);
					header.AMA_ManifestType = TRManifestTypes.Codes.GRUPAJ;
					header.AMA_RL_NKPortOfLoading = "TRIST";
					AssertNoMessageErrorContaining("GRUPAJ No error, change value ABL_RL_NKPortOfLoading", header.MasterBill.ABL_RL_NKPortOfLoadingInfo, MandatoryValidation.YouHaveNotEntered);
				});
			}
		}

		public void TestABL_E_DEP()
		{
			using (var helper = new ProviderTestHelper(Factory))
			{
				var header = helper.GetProviderHeader();
				header.AMA_ManifestType = TRManifestTypes.Codes.GRUPAJ;
				header.AMA_E_DEP = ZDateTime.Empty;
				AssertNoMessageErrors(header.MasterBill.ABL_E_DEPInfo);
				header.AMA_ManifestType = TRManifestTypes.Codes.DENIHR;
				header.AMA_E_DEP = ZDateTime.Empty;
				AssertNoMessageErrors(header.MasterBill.ABL_E_DEPInfo);
			}
		}

		public void TestABL_E_ARV()
		{
			using (var helper = new ProviderTestHelper(Factory))
			{
				var header = helper.GetProviderHeader();
				header.AMA_ManifestType = TRManifestTypes.Codes.GRUPAJ;
				header.AMA_E_ARV = ZDateTime.Empty;
				AssertNoMessageErrors(header.MasterBill.ABL_E_ARVInfo);
			}
		}
	}
}
