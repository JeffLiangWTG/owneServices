using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsLoadPkgPackagePivot))]
	class WhsLoadPkgPackagePivotTest : WhsBusinessObjectTestCase
	{
		public void TestIsLoaded()
		{
			var pivot = Factory.New<WhsLoadPkgPackagePivot>();
			AssertEquals("Empty Pivot should be Not loaded", false, pivot.IsLoaded);

			pivot.WLP_GS_NKLoadingUser = "E";
			pivot.WLP_LoadedTime = ZDateTimeOffset.Now;
			AssertEquals("Pivot with valid LoadingTime should be loaded", true, pivot.IsLoaded);

			pivot.WLP_GS_NKUnloadingUser = "E";
			pivot.WLP_UnloadedTime = ZDateTimeOffset.Now;
			AssertEquals("Pivot with valid LoadingTime and valid UnloadingTime should be Not loaded", false, pivot.IsLoaded);

			pivot.WLP_GS_NKLoadingUser = ZString.Empty;
			pivot.WLP_LoadedTime = ZDateTimeOffset.Empty;
			AssertEquals("Pivot with valid UnloadingTime should be Not loaded", false, pivot.IsLoaded);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var pivot = (WhsLoadPkgPackagePivot)base.GetNewBusinessObjectForDeleteTest(factory);
			pivot.Load.WLO_StartTime = DateTimeOffset.Now;
			pivot.Load.WLO_TransportationUnitNumber = "ABC456";
			return pivot;
		}
	}
}
