using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	[CountrySpecificTest("NZ")]
	public class BillValidatorTestClass : TestCaseWithDummy
	{
		public void TestAirlinePrefixCheckCatersForSpecialNZFlightTerm()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			declaration.JE_VoyageFlightNo = "";
			declaration.JE_MasterBill = "08298739457";
			AssertNoWarning(declaration.JE_MasterBillInfo, Enterprise.Customs.Business.BillValidator.AirlinePrefixValidationMessage);

			declaration.JE_VoyageFlightNo = "VARIOUS";
			declaration.JE_MasterBill = "08654297913";
			AssertNoWarning(declaration.JE_MasterBillInfo, Enterprise.Customs.Business.BillValidator.AirlinePrefixValidationMessage);

			declaration.JE_VoyageFlightNo = "QF1";
			declaration.JE_MasterBill = "08600238210";
			AssertHasWarning(declaration.JE_MasterBillInfo, Enterprise.Customs.Business.BillValidator.AirlinePrefixValidationMessage);

			declaration.JE_MasterBill = "08100492874";
			AssertNoWarning(declaration.JE_MasterBillInfo, Enterprise.Customs.Business.BillValidator.AirlinePrefixValidationMessage);
		}
	}
}

