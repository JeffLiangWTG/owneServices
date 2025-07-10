using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USCustomsNumberViewStmNumsWrapperValidationTest : CargoWise.EntityFramework.Testing.BusinessObjectValidationTestCase
	{
		public void TestCheckAppliesTo()
		{
			var stmNum = Factory.New<CustomsNumberViewStmNums>();
			var wrapper = new USCustomsNumberViewStmNumsWrapper(stmNum);
			wrapper.SN_Type = NumberRangeTypeList.Codes.CustomsEntry;
			wrapper.AppliesTo = ZString.Empty;
			AssertHasErrorContaining(wrapper.AppliesToInfo, MandatoryValidation.MustBeEntered);
			wrapper.AppliesTo = "XJ5";
			AssertNoErrorContaining(wrapper.AppliesToInfo, MandatoryValidation.MustBeEntered);
		}
	}
}
