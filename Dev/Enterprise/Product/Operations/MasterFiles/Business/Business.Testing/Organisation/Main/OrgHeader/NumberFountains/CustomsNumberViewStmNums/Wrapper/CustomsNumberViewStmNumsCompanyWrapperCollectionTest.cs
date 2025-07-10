using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(CustomsNumberViewStmNumsCompanyWrapperCollection))]
	public class CustomsNumberViewStmNumsCompanyWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CustomsNumberViewStmNumsCompanyWrapperCollection>
	{
		protected override CustomsNumberViewStmNumsCompanyWrapperCollection GetCollectionToTest()
		{
			return provider.CustomsNumberWrappers;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var stmNum = provider.CustomsNumbers.AddNew();
			return provider.GetOrCreateWrapper(stmNum);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new CustomsNumberViewStmNumsCompanyProviderForTest(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, GlbCompany.CurrentCompany.PK, true, true);
		}

		CustomsNumberViewStmNumsCompanyProvider provider;
	}
}
