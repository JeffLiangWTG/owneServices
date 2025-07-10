using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusHouseContPackInvoiceHeaderPivotValidationTest : BusinessObjectValidationTestCase
	{
		public void TestNumberOfPacks()
		{
			var chz = Factory.New<InvoiceHeaderPackagePivot>();
			chz.CHZ_NumberOfPacks = 1;
			AssertNoMessageErrorContaining(chz.CHZ_NumberOfPacksInfo, MandatoryValidation.YouHaveNotEntered);

			chz.CHZ_NumberOfPacks = 0;
			AssertHasMessageErrorContaining(chz.CHZ_NumberOfPacksInfo, MandatoryValidation.YouHaveNotEntered);
		}
	}
}
