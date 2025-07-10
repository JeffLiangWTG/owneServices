using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(CustomsNumberViewStmNumsWrapper))]
	sealed class CustomsNumberViewStmNumsWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestProperties()
		{
			var stmNums = Factory.New<CustomsNumberViewStmNums>();
			stmNums.Provider = new CustomsNumberViewStmNumsCompanyProviderForTest(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, GlbCompany.CurrentCompany.PK, true, false);
			stmNums.SN_Type = "CEN";
			stmNums.SN_FountainName = "BOB NUMBER";
			var wrapper = new CustomsNumberViewStmNumsWrapper(stmNums);

			AssertEquals("IsInDatabase", true, wrapper.IsInDatabase);
			AssertEquals("TablePrefix", ViewStmNumsSchema.Constants.Prefix, wrapper.TablePrefix);
			AssertEquals("TableName", ViewStmNumsSchema.Constants.TableName, wrapper.TableName);
			AssertEquals("PK", stmNums.PK, wrapper.PK);
			AssertEquals(wrapper, Factory.Load<CustomsNumberViewStmNumsWrapper>(stmNums.PK));
			AssertEquals("wrapper.Detail", "Range Type: CEN, Name: BOB NUMBER", wrapper.Detail);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var stmNums = CustomsNumberViewStmNumsCompanyProviderForTest.New(Factory).CustomsNumbers.AddNew();
			return new CustomsNumberViewStmNumsWrapper(stmNums);
		}
	}
}
