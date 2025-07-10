using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class AddInfoJobDeclarationValidation_TNPTest : AddInfoCUSDECValidationTest
	{
		public void TestRemovalStartDate()
		{
			Validation.ValidateSG_RemovalStartDate();
			AssertEquals(false, AddInfoJobDeclaration.SG_RemovalStartDateInfo.HasMessageErrors());
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.REM;
			Validation.ValidateSG_RemovalStartDate();
			AssertEquals(true, AddInfoJobDeclaration.SG_RemovalStartDateInfo.HasMessageErrors());
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.BRE;
			Validation.ValidateSG_RemovalStartDate();
			AssertEquals(true, AddInfoJobDeclaration.SG_RemovalStartDateInfo.HasMessageErrors());
			AddInfoJobDeclaration.SG_RemovalStartDate = ZDateTime.Today;
			Validation.ValidateSG_RemovalStartDate();
			AssertEquals(false, AddInfoJobDeclaration.SG_RemovalStartDateInfo.HasMessageErrors());
		}

		public void TestCountryOfFinalDestination()
		{
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.Four;
			Validation.ValidateSG_RN_NKFinalDestination();
			AssertEquals(true, Declaration.SG_RN_NKFinalDestinationInfo.HasMessageErrors());
			Declaration.SG_RN_NKFinalDestination = Core.Constants.CountryCodes.Slovakia;
			Validation.ValidateSG_RN_NKFinalDestination();
			AssertEquals(false, Declaration.SG_RN_NKFinalDestinationInfo.HasMessageErrors());
			Declaration.SG_IsSeaStore = true;
			Declaration.SG_RN_NKFinalDestination = "";
			Validation.ValidateSG_RN_NKFinalDestination();
			AssertEquals(false, Declaration.SG_RN_NKFinalDestinationInfo.HasMessageErrors());
		}

		protected override string MessageType => MessageTypeCodeList.Codes.TNP;
	}
}
