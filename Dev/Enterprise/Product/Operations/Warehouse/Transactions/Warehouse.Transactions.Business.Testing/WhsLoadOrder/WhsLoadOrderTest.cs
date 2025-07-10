using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsLoadOrder))]
	class WhsLoadOrderTest : WhsBusinessObjectTestCase
	{
		public void TestCanDelete()
		{
			AssertEquals("Not allowed to delete dbo.WhsLoadOrder", false, GetNewBusinessObject().CanDelete);
		}

		public void TestLoad()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD01", data.Part1, 10m);
			Factory.Save();

			AssertEquals("Precondition", false, order.WD_WLO_PlannedLoad.IsValid);
			AssertEquals("Precondition", false, order.TransportCoDocAddress.ReadOnly);

			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, "WL01", "CDS", startTime: DateTimeOffset.Now);
			Helper.CreatePickNew(order);
			var packageJob = order.PackageJob;
			var package = packageJob.Packages.AddNew("BOX", 1);
			Helper.CreateLoadPkgPackagePivot(package.PK, load);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadOrder = newFactory.Load<WhsLoadOrder>(new ZQuery(WhsLoadOrderSchema.WOV_WD_Docket, order.PK)).Single();
			var loadInNewFactory = loadOrder.Load;
			AssertNotNull(loadInNewFactory);
			AssertEquals(load.PK, loadInNewFactory.PK);
		}

		#region Implementation

		protected override bool IsDeleteSupported() => false;

		#endregion
	}
}
