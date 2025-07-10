using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(RemoveHoldAllPackagesApplicator))]
	public class RemoveHoldAllPackagesApplicatorTest : OperationalActionMethodApplicatorTest
	{
		#region TestAction

		public void TestAction()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 5m);
			order.WD_DocketID = "W00000002";

			var orders = new[] { order };
			ApplyApplicator(orders, string.Format(@"WARNING: {0} [HL W00000002] - cannot remove hold of packages as it has not been Picked.".Trim(), order.HumanReadableName));

			var pick = Helper.CreatePickNew(order);
			var package1 = order.PackageJob.Packages.AddNew();
			package1.KP_IsHeld = true;
			var package2 = order.PackageJob.Packages.AddNew();
			package2.KP_IsHeld = false;
			var package3 = order.PackageJob.Packages.AddNew();
			package3.KP_IsHeld = true;

			orders = new[] { order };
			ApplyApplicator(orders, string.Format(
@"INFO: {0} [HL W00000002] - all the packages are without hold now.".Trim(), order.HumanReadableName));
			AssertEquals("All the packages should be without hold", false, order.PackageJob.Packages.Any(p => p.KP_IsHeld));

			order.Pick.FinaliseAllOrders();
			orders = new[] { order };
			ApplyApplicator(orders, string.Format(
@"WARNING: {0} [HL W00000002] - is already finalized and cannot remove hold of packages.".Trim(), order.HumanReadableName));

			var cancelledOrder = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "2", data.Part1, 5m);
			cancelledOrder.WD_DocketStatus = DocketStatus.Codes.Cancelled;
			cancelledOrder.WD_DocketID = "W00000003";
			orders = new[] { cancelledOrder };
			ApplyApplicator(orders, string.Format(
@"WARNING: {0} [HL W00000003] - is canceled and cannot remove hold of packages.".Trim(), cancelledOrder.HumanReadableName));
		}

		#endregion

		#region TestDBHits_RemoveHoldAllPackagesApplicator

		public void TestDBHits_RemoveHoldAllPackagesApplicator()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var numberOfOrders = 30;
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, numberOfOrders * 10m);
			Factory.Save();

			for (int i = 0; i < numberOfOrders; i++)
			{
				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, i.ToString(), data.Part1, 10m);
				Helper.CreatePickNew(order);
				var package = order.PackageJob.Packages.AddNew();
				package.KP_IsHeld = true;
				AssertEquals($"Precondition: package {i + 1} is held.", true, package.KP_IsHeld);
			}

			Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var ordersInNewFactory = newFactory.Load<WhsOrder>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Order));
			AssertEquals("Precondition: Correct number of Orders are loaded.", numberOfOrders, ordersInNewFactory.Length);

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ ProcessTasksSchema.Constants.TableName, 1 },
				{ ProcessTaskTemplateSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsPickSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ PkgPackageSchema.Constants.TableName, 1 },
				{ PkgPackageJobSchema.Constants.TableName, 1 },
				{ StmEventSchema.Constants.TableName, 1 }
			};

			var expectedMessage = new ZStringBuilder();
			foreach (var order in ordersInNewFactory)
			{
				expectedMessage.AppendLine($"INFO: Warehouse Order {order.WD_DocketID} [HL {order.WD_DocketID}] - all the packages are without hold now.");
			}

			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, newFactory))
			{
				ApplyApplicator(ordersInNewFactory, expectedMessage.ToStringWithNewLineBetweenAppends());
			}

			AssertEquals("All packages are not held anymore.", false, ordersInNewFactory.SelectMany(order => order.PackageJob.Packages).Any(package => package.KP_IsHeld));
		}

		#endregion

		#region Implementation

		protected WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions helper;

		#endregion
	}
}
