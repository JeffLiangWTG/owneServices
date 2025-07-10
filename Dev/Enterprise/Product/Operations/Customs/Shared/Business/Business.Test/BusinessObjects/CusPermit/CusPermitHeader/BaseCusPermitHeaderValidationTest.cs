using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BaseCusPermitHeaderValidationTest : SharedCusPermitHeaderValidationTest<BaseCusPermitHeader>
	{
		public void TestValidateTransactionCategory()
		{
			PermitHeader.TransactionCategory = ZString.Empty;
			AssertNoMessageErrorContaining(PermitHeader.TransactionCategoryInfo, ListValidation.InvalidCodeMessageError);
			PermitHeader.TransactionCategory = "ZZZ";
			AssertHasMessageErrorContaining(PermitHeader.TransactionCategoryInfo, ListValidation.InvalidCodeMessageError);
			PermitHeader.TransactionCategory = PermitTransactionCategoryList.Codes.CUM;
			AssertNoMessageErrorContaining(PermitHeader.TransactionCategoryInfo, ListValidation.InvalidCodeMessageError);
			PermitHeader.TransactionCategory = PermitTransactionCategoryList.Codes.VAL;
			AssertNoMessageErrorContaining(PermitHeader.TransactionCategoryInfo, ListValidation.InvalidCodeMessageError);
		}

		#region Implementation

		protected override BaseCusPermitHeader GetNewPermitHeader(BusinessObjectFactory factory)
		{
			return Factory.NewWithValidTestData<BaseCusPermitHeader_ForTest>();
		}

		#endregion
	}
}
