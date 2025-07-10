using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class AttachGenericOrdersUserControlBashFetchTest : TestCaseWithFactory
	{
		public void TestBashFetchForView_TransportMode()
		{
			BashFetchForView("TransportMode", 0);
		}

		public void TestBashFetchForView_ETD()
		{
			// ProcessTasks: 1
			BashFetchForView("ETD", 1);
		}

		public void TestBashFetchForView_ETA()
		{
			// ProcessTasks: 1
			BashFetchForView("ETA", 1);
		}

		public void TestBashFetchForView_RequiredExWorks()
		{
			BashFetchForView("RequiredExWorks", 0);
		}

		public void TestBashFetchForView_RequiredInStore()
		{
			BashFetchForView("RequiredInStore", 0);
		}

		public void TestBashFetchForView_TotalWeight()
		{
			// JobOrderLine: 1
			BashFetchForView("TotalWeight", 1);
		}

		public void TestBashFetchForView_TotalVolume()
		{
			// JobOrderLine: 1
			BashFetchForView("TotalVolume", 1);
		}

		public void TestBashFetchForView_TotalPacks()
		{
			// JobOrderLine: 1
			BashFetchForView("TotalPacks", 1);
		}

		public void TestBashFetchForView_QuantityRemaining()
		{
			// JobOrderLine: 1
			BashFetchForView("QuantityRemaining", 1);
		}

		public void TestBashFetchForView_QuantityInvoiced()
		{
			// JobOrderLine: 1
			BashFetchForView("QuantityInvoiced", 1);
		}

		public void TestBashFetchForView_QuantityOrdered()
		{
			// JobOrderLine: 1
			BashFetchForView("QuantityOrdered", 1);
		}

		public void TestBashFetchForView_QuantityReceived()
		{
			// JobOrderLine: 1
			BashFetchForView("QuantityReceived", 1);
		}

		public void TestBashFetchForView_BuyerOrgCode()
		{
			// OrgAddress: 1
			// OrgHeader: 12
			BashFetchForView("BuyerOrgCode", 13);
		}

		public void TestBashFetchForView_SupplierOrgCode()
		{
			// OrgAddress: 1
			// OrgHeader: 12
			BashFetchForView("SupplierOrgCode", 13);
		}

		static void BashFetchForView(string bindToString, int maxDbHits)
		{
			var factory = new BusinessObjectFactory();

			var shipment = factory.LoadTop1<ForwardingShipment>(new ZQuery(JobShipmentSchema.PK, CreateOrders()));
			var orders = shipment.GenericOrders;

			factory.ResetDatabaseLoadCount();

			var fetchStrategy = new OrderCollectionFetchStrategy(orders);
			fetchStrategy.FetchForView((orders as IEnumerable<BusinessObject>).ToArray(), new[] { new TableColumn("", bindToString) });

			foreach (var order in orders.OfType<IAttachedOrder>())
			{
				HitProperty(order, bindToString);
			}

			AssertMaxDbHits(maxDbHits, factory);
		}

		static void HitProperty(object root, string pathStr)
		{
			var path = pathStr.Split(new[] { '.', '+' }, StringSplitOptions.RemoveEmptyEntries);

			var o = root;

			for (var i = 0; i < path.Length; i++)
			{
				if (o == null)
				{
					var message = string.Join("+", path, 0, i) + " returned null, you should populate your data better";
					throw new ApplicationException(message);
				}

				o = typeof(IAttachedOrder).InvokeMember(path[i], BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance, null, o, Array.Empty<object>());
			}
		}

		static ZGuid CreateOrders()
		{
			var factory = new BusinessObjectFactory();
			var shipment = factory.New<ForwardingShipment>();

			for (var i = 0; i < 12; i++)
			{
				var order = factory.New<Order>();
				order.JD_ActualVolume = 100;
				order.JD_OrderNumber = "Number" + i;
				order.JD_ActualWeight = 150;
				order.JD_ActualVolume = 1000;
				order.JD_ActualVolume = 1000;
				order.JD_ActualVolume = 1000;

				var buyer = factory.New<OrgHeader>();
				buyer.OH_Code = "AUSBUYE" + i;
				buyer.OH_RL_NKClosestPort = "AUSYD";

				var supplier = factory.New<OrgHeader>();
				supplier.OH_Code = "NZSUPPLIE" + i;
				supplier.OH_RL_NKClosestPort = "NZAKL";

				order.BuyerPK = buyer.PK;
				order.SupplierPK = supplier.PK;

				var orderline1 = order.OrderLines.AddNew();
				orderline1.JO_QtyReceived = 1;
				orderline1.JO_QtyInvoiced = 1;

				order.JD_JS = shipment.PK;
			}

			factory.Save();

			return shipment.PK;
		}
	}
}
