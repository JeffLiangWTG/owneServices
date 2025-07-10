using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Integration.Reference;
using Moq;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class RefAirlineSpecialHandlingCodeValidationTest : BusinessObjectValidationTestCase
	{
		public void Test_RHC_CodeLength()
		{
			var airline = Factory.New<RefAirline>();
			Factory.Save();

			var refAirLineSpecialHand = Factory.New<RefAirlineSpecialHandlingCode>();
			refAirLineSpecialHand.RHC_RM_Airline = airline.PK;
			refAirLineSpecialHand.RHC_Code = refAirLineSpecialHand.RHC_Code.Substring(0, 2);
			refAirLineSpecialHand.Validation.ValidateRHC_Code();

			AssertHasErrors("Length of RHC_Code is not equal to 3", refAirLineSpecialHand.RHC_CodeInfo);
		}

		public void TestIfRHCCodeIsInIATAList()
		{
			var list = new AccountTypeCodeDescriptionPairList();
			list.AddPair("YY");

			var mock = new Mock<IIATASpecialHandlingCodesProvider>();
			mock.Setup(x => x.GetCodeDescriptionPairList()).Returns(list);

			var refAirLine = Factory.NewWithValidTestData<RefAirlineSpecialHandlingCode>();
			refAirLine.RHC_Code = "YY";

			AssertHasErrors("This Special Handling Code is already defined in the IATA approved code list", refAirLine.RHC_CodeInfo);
		}
	}
}
