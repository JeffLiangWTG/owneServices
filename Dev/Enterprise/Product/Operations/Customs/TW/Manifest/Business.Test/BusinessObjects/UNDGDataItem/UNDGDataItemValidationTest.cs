using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Manifest.Business.Testing
{
	sealed class UNDGDataItemValidationTest : BusinessObjectValidationTestCase
	{
		public void TestHasNotErrorsWithoutUNDGSubstanceOnSaving()
		{
			var item = Factory.New<UNDGDataItem>();
			item.DI_DG = ZGuid.Empty;
			CombineAssertions(() =>
			{
				AssertNoErrors(item.DI_IMOClassInfo);
			});
		}
	}
}
