using System.Data;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	class OrderDatesProxyFromShipmentTest : Freight.Business.Testing.BaseFreightTest
	{
		public void TestDepartureActual()
		{
			fTransport.JW_ATD = TestShipmentDate;
			TestOrderWouldGetShipmentDateOnAttaching(Events.Departure, true);

			Factory.Save();
			fTransport.JW_ATD = TestNewShipmentDate;
			AssertEquals("Order date should not be auto-updated when already-attached shipment with an updated date is saved", TestShipmentDate, fOrder.GetMilestoneActualDate(Events.Departure).ToZDateTime());
		}

		public void TestArrivalActual()
		{
			fTransport.JW_ATA = TestShipmentDate;
			TestOrderWouldGetShipmentDateOnAttaching(Events.Arrival, true);

			Factory.Save();
			fTransport.JW_ATA = TestNewShipmentDate;
			AssertEquals("Order date should not be auto-updated when already-attached shipment with an updated date is saved", TestShipmentDate, fOrder.GetMilestoneActualDate(Events.Arrival).ToZDateTime());
		}

		public void TestCustomsCommencedActual()
		{
			var log1 = fShipment.Logs.AddNew(Events.CustomsCommenced, TestShipmentDate.ToOffset());
			TestOrderWouldGetShipmentDateOnAttaching(Events.CustomsCommenced, true);

			var log2 = fShipment.Logs.AddNew(Events.CustomsCommenced, TestNewShipmentDate.ToOffset());
			AssertEquals("Order date should not be auto-updated when already-attached shipment with an updated date is saved", TestShipmentDate, fOrder.GetMilestoneActualDate(Events.CustomsCommenced).ToZDateTime());
		}

		public void TestCustomsClearedActual()
		{
			var log1 = fShipment.Logs.AddNew(Events.CustomsCleared, TestShipmentDate.ToOffset());
			TestOrderWouldGetShipmentDateOnAttaching(Events.CustomsCleared, true);

			var log2 = fShipment.Logs.AddNew(Events.CustomsCleared, TestNewShipmentDate.ToOffset());
			AssertEquals("Order date should not be auto-updated when already-attached shipment with an updated date is saved", TestShipmentDate, fOrder.GetMilestoneActualDate(Events.CustomsCleared).ToZDateTime());
		}

		public void TestCargoAvailableActual_FCL()
		{
			fShipment.JS_PackingMode = Constants.ContainerModes.FCL;
			fShipment.DocsAndCartage.JP_FCLAvailable = TestShipmentDate;
			fShipment.DocsAndCartage.JP_LCLAvailable = ZDateTime.Empty;
			TestOrderWouldGetShipmentDateOnAttaching(Events.CargoAvailable, true);

			Factory.Save();
			fShipment.DocsAndCartage.JP_FCLAvailable = TestNewShipmentDate;
			AssertEquals("Order date should not be auto-updated when already-attached shipment with an updated date is saved", TestShipmentDate, fOrder.GetMilestoneActualDate(Events.CargoAvailable).ToZDateTime());
		}

		public void TestCargoAvailableActual_LCL()
		{
			fShipment.JS_PackingMode = Constants.ContainerModes.LCL;
			fShipment.DocsAndCartage.JP_FCLAvailable = ZDateTime.Empty;
			fShipment.DocsAndCartage.JP_LCLAvailable = TestShipmentDate;
			TestOrderWouldGetShipmentDateOnAttaching(Events.CargoAvailable, true);

			Factory.Save();
			fShipment.DocsAndCartage.JP_LCLAvailable = TestNewShipmentDate;
			AssertEquals("Order date should not be auto-updated when already-attached shipment with an updated date is saved", TestShipmentDate, fOrder.GetMilestoneActualDate(Events.CargoAvailable).ToZDateTime());
		}

		public void TestCargoAvailableActual_AIR()
		{
			fShipment.JS_TransportMode = Constants.TransportModes.Air;
			fShipment.DocsAndCartage.JP_FCLAvailable = ZDateTime.Empty;
			fShipment.DocsAndCartage.JP_LCLAvailable = TestShipmentDate;
			TestOrderWouldGetShipmentDateOnAttaching(Events.CargoAvailable, true);

			Factory.Save();
			fShipment.DocsAndCartage.JP_LCLAvailable = TestNewShipmentDate;
			AssertEquals("Order date should not be auto-updated when already-attached shipment with an updated date is saved", TestShipmentDate, fOrder.GetMilestoneActualDate(Events.CargoAvailable).ToZDateTime());
		}

		public void TestDeliveryCartageAdvisedActual()
		{
			fShipment.DocsAndCartage.JP_DeliveryCartageAdvised = TestShipmentDate;
			TestOrderWouldGetShipmentDateOnAttaching(Events.DeliveryCartageAdvised, true);
		}

		public void TestDeliveryCartageCompleteFinalisedActual()
		{
			fShipment.DocsAndCartage.JP_DeliveryCartageCompleted = TestShipmentDate;
			TestOrderWouldGetShipmentDateOnAttaching(Events.DeliveryCartageCompleteFinalised, true);
		}

		public void TestEstimatedCustomsCommenced_NotPopulatedFromDeclarationOnSave()
		{
			Order order = Factory.NewWithValidTestData<Order>();
			BusinessObject declaration = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration)));
			Factory.Save();

			order.JD_JE = declaration.PK;
			AssertEquals("CustomsCommenced milestone populated when declaration is attached", false, order.GetMilestoneEstimatedDate(Events.CustomsCommenced).IsEmpty);
			order.UpdateEventEstimate(Events.CustomsCommenced, ZDateTimeOffset.Empty);
			Factory.Save();
			AssertEquals("When CustomsCommenced is explicitly emptied, it remains so after save", true, order.GetMilestoneEstimatedDate(Events.CustomsCommenced).IsEmpty);
		}

		public void TestNoteContextsForRelatedNotes()
		{
			TestOrder order = Factory.New<TestOrder>();
			var shipment = Factory.New<ForwardingShipment>();
			Assert("Should always be Forwarding module", (order.NoteContextsForRelatedNotes.Module & StmNoteContextModule.F) != 0);
			Assert("Always have 'Forwarding, Brokerage, CFS and Orders' module option", (order.NoteContextsForRelatedNotes.Module & StmNoteContextModule.I) != 0);
			Assert("Always have 'Shipment and Declaration' module option", (order.NoteContextsForRelatedNotes.Module & StmNoteContextModule.E) != 0);
			Assert("Should always be Orders module", (order.NoteContextsForRelatedNotes.Module & StmNoteContextModule.O) != 0);
			Assert("Not be air yet", (order.NoteContextsForRelatedNotes.FreightMode & StmNoteContextFreightMode.I) == 0);

			order.JD_TransportMode = Constants.TransportModes.Air;
			Assert("Should be air", (order.NoteContextsForRelatedNotes.FreightMode & StmNoteContextFreightMode.I) != 0);

			order.JD_JS = shipment.PK;
			Assert("Should still be SHP", (order.NoteContextsForRelatedNotes.Module & StmNoteContextModule.F) != 0);
			Assert("Should still have 'Forwarding, Brokerage, CFS and Orders' module option", (order.NoteContextsForRelatedNotes.Module & StmNoteContextModule.I) != 0);
			Assert("Should still have 'Shipment and Declaration' module option", (order.NoteContextsForRelatedNotes.Module & StmNoteContextModule.E) != 0);
			Assert("Should still be ORD", (order.NoteContextsForRelatedNotes.Module & StmNoteContextModule.O) != 0);
		}

		public void TestAttachingDeclarationsAndShipments()
		{
			Order order = Factory.New<Order>();
			BusinessObject declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			var shipment = Factory.New<ForwardingShipment>();

			order.JD_JE = declaration.PK;

			order.JD_JS = shipment.PK;
			AssertEquals("Attached declaration should be reset to empty", true, order.JD_JE.IsEmpty);

			order.JD_JE = declaration.PK;
			AssertEquals("Attached shipment should be reset to empty", true, order.JD_JS.IsEmpty);

			declaration[JobDeclarationSchema.JE_JS.Name] = shipment.PK;

			order.JD_JS = shipment.PK;
			AssertEquals("Attached declaration should be set as the shipment is part of the declaration", order.JD_JE, declaration.PK);
			AssertEquals("Attached shipment should be set as the declaration is part of the shipment", shipment.PK, order.JD_JS);

			order.JD_JS = ZGuid.Empty;
			AssertEquals("Setting one empty should set the other empty as well", true, order.JD_JE.IsEmpty);

			order.JD_JE = declaration.PK;
			AssertEquals("Attached declaration should be set as the shipment is part of the declaration", order.JD_JE, declaration.PK);
			AssertEquals("Attached shipment should be set as the declaration is part of the shipment", shipment.PK, order.JD_JS);

			order.JD_JE = ZGuid.Empty;
			AssertEquals("Setting one empty should set the other empty as well", true, order.JD_JS.IsEmpty);
		}

		public void TestJD_OrderNumberAndSplit()
		{
			Order order = Factory.New<Order>();
			order.JD_OrderNumber = "order";
			order.JD_OrderNumberSplit = 0;
			AssertEquals("order", order.JD_OrderNumberAndSplit);

			order.JD_OrderNumberSplit = 2;
			AssertEquals("order-2", order.JD_OrderNumberAndSplit);

			order.JD_OrderNumberSplit = 11;
			AssertEquals("order-11", order.JD_OrderNumberAndSplit);
		}

		public void TestOrderSplitNumber()
		{
			BusinessObjectFactory bizFactory = new BusinessObjectFactory();
			OrgHeader org1 = bizFactory.New<OrgHeader>();

			org1.OH_FullName = "Org";
			org1.OH_Code = "ORGAU";
			org1.Addresses[0].OA_Address1 = "Org";

			Order dummyOrder = bizFactory.New<Order>();
			dummyOrder.BuyerPK = org1.PK;
			dummyOrder.SupplierPK = org1.PK;
			dummyOrder.JD_OrderNumber = "123";
			dummyOrder.JD_OrderNumberSplit = 6;

			Order order1 = bizFactory.New<Order>();
			var org2 = Factory.LoadTop1<OrgHeader>(new ZQuery());
			order1.BuyerPK = org2.PK;
			order1.SupplierPK = org2.PK;
			order1.JD_OrderNumber = "11231";
			Order split1 = order1.SplitOrder(CreateOrderType.Split);
			bizFactory.Save();
			Order split2 = order1.SplitOrder(CreateOrderType.Split);
			AssertEquals("Shouldn't have errors on split no", false, split2.JD_OrderNumberSplitInfo.HasErrors());
			AssertEquals("Shouldn't have errors on Order no", false, split2.JD_OrderNumberInfo.HasErrors());
			AssertEquals("Should be 1 greater than the highest split of this OrderNo/Buyer combo", split1.JD_OrderNumberSplit + 1, split2.JD_OrderNumberSplit);

			split2.JD_IsCancelled = true;
			bizFactory.Save();	
			Order split3 = order1.SplitOrder(CreateOrderType.Split);
			AssertEquals("Should have no errors on split no", false, split3.JD_OrderNumberSplitInfo.HasErrors());
			AssertEquals("Should have no errors on order no", false, split3.JD_OrderNumberInfo.HasErrors());
			AssertEquals("Should be 1 greater than the highest split of All orders (including cancelled) for order/buyer combo", split2.JD_OrderNumberSplit + 1, split3.JD_OrderNumberSplit);
		}

		#region Implementation

		Order fOrder;
		ForwardingShipment fShipment;
		CommonConsol fConsol;
		Transport fTransport;
		BusinessObject fDeclaration;

		static readonly ZDateTime TestOrderDate = new ZDateTime(2000, 1, 1);
		static readonly ZDateTime TestShipmentDate = new ZDateTime(2000, 12, 31);
		static readonly ZDateTime TestNewShipmentDate = new ZDateTime(2000, 12, 25);

		protected override void SetUp()
		{
			base.SetUp();
			fOrder = Factory.New<Order>();
			fOrder.JD_OrderNumber = "123";
			fOrder.BuyerPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			fShipment = Factory.New<ForwardingShipment>();
			fConsol = fShipment.Consols.AddNew();
			fConsol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			fTransport = fConsol.Transports[0];
			fDeclaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			fDeclaration[JobDeclarationSchema.Constants.JE_JS] = fShipment.PK;
		}

		void TestOrderWouldGetShipmentDateOnAttaching(Event eventType, bool isActual)
		{
			if (isActual)
			{
				fOrder.UpdateEvent(eventType, TestOrderDate.ToOffset());
			}
			else
			{
				fOrder.UpdateEventEstimate(eventType, TestOrderDate.ToOffset());
			}

			fOrder.JD_JS = ZGuid.Empty;
			AssertEquals("Order with shipment Detached", TestOrderDate, (isActual ? fOrder.GetMilestoneActualDate(eventType) : fOrder.GetMilestoneEstimatedDate(eventType)).ToZDateTime());

			fOrder.JD_JS = fShipment.PK;
			AssertEquals("Order with shipment attached", TestShipmentDate, (isActual ? fOrder.GetMilestoneActualDate(eventType) : fOrder.GetMilestoneEstimatedDate(eventType)).ToZDateTime());
		}

		protected class TestOrder : Order
		{
			public TestOrder(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public new StmNoteContexts NoteContextsForRelatedNotes
			{
				get { return base.NoteContextsForRelatedNotes; }
			}
		}

		#endregion
	}
}
