using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.MasterFiles.Module.Testing
{
	public abstract class HasSalesRelationFilterTestCase<T> : ModuleFilterTestCase<T> where T : HasSalesRelationFilter
	{
		#region Constructor

		public abstract void TestConstructor();

		#endregion

		#region IsActive

		public void TestSetToActiveShouldDefaultAny()
		{
			Filter.IsActive = false;
			Filter.TypeProperty = "";

			Filter.IsActive = true;
			AssertEquals(SalesRelationActivityFilterHelper.AnySalesRelationTypeCode, Filter.TypeProperty);
		}

		#endregion

		#region IsExpensiveQuery

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		#endregion

		#region DefaultCategory

		protected override FilterCategory ExpectedDefaultCategory
		{
			get { return FilterCategories.SalesRelationActivity; }
		}

		#endregion

		#region Clear / IsEmpty

		public abstract void TestClear();

		public void TestIsEmpty()
		{
			Filter.TypeProperty = ZString.Empty;
			AssertEquals(true, Filter.IsEmpty);

			Filter.TypeProperty = SalesRelationActivityFilterHelper.AnySalesRelationTypeCode;
			AssertEquals(false, Filter.IsEmpty);
		}

		public override void TestQueryIsEmptyByDefault()
		{
			AssertEquals("Query has value of 'ANY' by default", false, Filter.IsEmpty);
		}

		#endregion

		#region Query

		public abstract void TestGetQuery();

		public abstract void TestQueryGetsRefreshedWhenFilterIsReused();

		#endregion
	}
}
