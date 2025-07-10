using System;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MarketingManager.ServiceTask.Testing
{
	[TestedType(typeof(SalesTradeLanesSynchronisationOrgQueue.OrgCache))]
	public class OrgCacheTest : BusinessObjectBaseTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var row = DataUtils.GetDataTableFromQuery(TestConnection, "select newid() OH_PK, getdate() LastSyncEventTime, 1 HasSyncAll;").Rows[0];
			return new SalesTradeLanesSynchronisationOrgQueue.OrgCache(Factory, row);
		}

		public void TestConstructor()
		{
			var dataTable = DataUtils.GetDataTableFromQuery(TestConnection, @"
select cast(null as uniqueidentifier) OH_PK, cast(null as datetime) LastSyncEventTime, 1 HasSyncAll
union
select cast('28c72338-1a55-40e1-9e7c-2f1a3d2cbdce' as uniqueidentifier) OH_PK, cast('2018-1-1' as datetime) LastSyncEventTime, 0 HasSyncAll
");
			var obj1 = new SalesTradeLanesSynchronisationOrgQueue.OrgCache(Factory, dataTable.Rows[0]);
			AssertEquals(Guid.Empty, obj1.OH_PK);
			AssertEquals(ZDateTime.Empty, obj1.LastSyncEventTime);
			AssertEquals(ZBool.True, obj1.HasSyncAll);

			var obj2 = new SalesTradeLanesSynchronisationOrgQueue.OrgCache(Factory, dataTable.Rows[1]);
			AssertEquals(Guid.Parse("28c72338-1a55-40e1-9e7c-2f1a3d2cbdce"), obj2.OH_PK);
			AssertEquals(new ZDateTime(2018, 1, 1), obj2.LastSyncEventTime);
			AssertEquals(ZBool.False, obj2.HasSyncAll);
		}
	}
}
