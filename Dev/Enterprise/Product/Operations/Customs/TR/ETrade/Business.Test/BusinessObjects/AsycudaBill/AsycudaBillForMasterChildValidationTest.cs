using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Helper;

namespace Enterprise.Customs.TR.ETrade.Business.Testing
{
	sealed class AsycudaBillForMasterChildValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckABL_Procedure()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.ProcedureCode = Core.Constants.CountryCodes.Turkey;
			header.AMA_Nature = ShipmentTypeList.Codes.Export22;
			header.ProcedureCode = ZString.Empty;
			AssertHasMessageErrorContaining(header.ProcedureCodeInfo, MandatoryValidation.YouHaveNotEntered);
			header.ProcedureCode = "ABC";
			AssertHasMessageErrorContaining(header.ProcedureCodeInfo, ListValidation.InvalidCodeMessageError);
			header.ProcedureCode = "999";
			AssertHasMessageErrorContaining(header.ProcedureCodeInfo, ListValidation.InvalidCodeMessageError);
		}
	}
}
