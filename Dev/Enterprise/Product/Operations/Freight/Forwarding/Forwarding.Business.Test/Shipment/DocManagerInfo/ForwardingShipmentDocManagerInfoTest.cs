using System;
using System.Collections;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingShipmentDocManagerInfo))]
	public class ForwardingShipmentDocManagerInfoTest : ShipmentDocManagerInfoTest
	{
		public void TestAllRelatedWarehouseOrdersAreRetrieved()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var whsOrder = Factory.NewWithValidTestData(ObjectFactory.GetType<IWhsOrder>());
			var whsOrderShipmentPivot = Factory.New(ObjectFactory.GetType<IWhsDocketJobPivot>());

			whsOrderShipmentPivot[WhsDocketJobPivotSchema.WV_ParentId] = shipment.PK;
			whsOrderShipmentPivot[WhsDocketJobPivotSchema.WV_ParentTableCode] = shipment.TablePrefix;
			whsOrderShipmentPivot[WhsDocketJobPivotSchema.WV_WD_Docket] = whsOrder.PK;
			whsOrderShipmentPivot[WhsDocketJobPivotSchema.WV_DocketType] = ((IWhsDocket)whsOrder).WD_DocketType;

			Factory.Save();

			AssertContainsExactElementsInAnyOrder(new[] { whsOrder }, shipment.DocManagerInfo.RelatedObjects);
		}

		public void TestGetRelatedObjects_WhsReceive()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var receive = Factory.NewWithValidTestData(ObjectFactory.GetType<IWhsReceive>());
			var whsReceiveShipmentPivot = Factory.New(ObjectFactory.GetType<IWhsDocketJobPivot>());

			whsReceiveShipmentPivot[WhsDocketJobPivotSchema.WV_ParentId] = shipment.PK;
			whsReceiveShipmentPivot[WhsDocketJobPivotSchema.WV_ParentTableCode] = shipment.TablePrefix;
			whsReceiveShipmentPivot[WhsDocketJobPivotSchema.WV_WD_Docket] = receive.PK;
			whsReceiveShipmentPivot[WhsDocketJobPivotSchema.WV_DocketType] = ((IWhsDocket)receive).WD_DocketType;

			AssertContainsExactElementsInAnyOrder(new [] { receive }, shipment.DocManagerInfo.RelatedObjects);
		}

		[TestDate(2008, 02, 05)]
		[RunInExtraTransaction]
		public void TestGenerateStorageNumber()
		{
			Env.NumberFountains.StorageNumber.SetNext(Factory, 2100);
			BillOfLadingNumberCustomisation newCustomisation = new BillOfLadingNumberCustomisation();
			BillOfLadingNumberCustomisationElement element = newCustomisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.YearAsDigit];
			element.Include = true;
			element.Order = 1;
			element = newCustomisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.SequenceNumber];
			element.Include = true;
			element.Order = 50;
			element.Detail = "3";
			FreightDataRegistry.Instance.StorageNumberCustomisation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newCustomisation);

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			Enterprise.Registry.Business.FreightDataRegistry.Instance.StorageNumberButtonActivation.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, GlbDepartment.CurrentDepartment.PK.ToGuid(), true);

			Assert(shipment.DocManagerInfo.SupportsStorageNumberGeneration);
			AssertEquals("82100", shipment.DocManagerInfo.GenerateStorageNumber());
		}

		public override BusinessObject GetEmptyParentBusinessObject()
		{
			return Factory.New<ForwardingShipment>();
		}

		public override BusinessObject GetPopulatedParentBusinessObject()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001234";
			Order = shipment.AttachedOrders.AddNew();

			return shipment;
		}

		public override void TestAllRelatedObjectsRetrieved()
		{
			ForwardingShipment shipment = (ForwardingShipment)GetPopulatedParentBusinessObject();
			AssertEquals("Should have the order in the related business objects", true, ((IList)shipment.DocManagerInfo.RelatedObjects).Contains(Order));
		}

		public void TestAllRelatedObjectsRetrievedForGbAirShipmentWithCcsukAwb()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			var mawb = Factory.New<Enterprise.Integration.Customs.GB.CCSUK.ICusMAWB>();
			mawb.CM_JK = consol.PK;
			var hawb = Factory.New<Enterprise.Integration.Customs.GB.CCSUK.ICusHAWB>();
			hawb.CS_CM = ((BusinessObject)mawb).PK;
			hawb.CS_JS = shipment.PK;
			var ediMessage = EDIMessageTestFactory.New(Factory);
			ediMessage.EM_LinkedObject = hawb as BusinessObject;
			var pksOfExpectedObjects = new List<ZGuid>() { ((BusinessObject)mawb).PK, ((BusinessObject)hawb).PK, consol.PK, shipment.PK, ediMessage.PK };
			foreach (var relatedObject in new ForwardingShipmentDocManagerInfo(shipment).RelatedObjects)
			{
				AssertCollectionContains(relatedObject.PK, pksOfExpectedObjects);
			}
		}

		public void TestRelatedObjectsForEuNcts()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			var nctsHeader = Factory.New<Enterprise.Integration.Customs.EU.NCTS.ICusInBondHeader>();
			nctsHeader.BH_ParentID = shipment.PK;
			nctsHeader.BH_ParentTableCode = shipment.TablePrefix;
			((BusinessObject)nctsHeader)[CusInBondHeaderSchema.BH_HeaderType] = "D";
			var relatedObjects = new ForwardingShipmentDocManagerInfo(shipment).RelatedObjects;
			CombineAssertions(() =>
			{
				AssertCollectionContains("NctsHeader", nctsHeader, relatedObjects);
				AssertCollectionContains("Departure movement header", relatedObjects, bo => bo is Enterprise.Integration.Customs.EU.NCTS.IDepartureMovementHeader departureMovement && departureMovement.BM_BH == nctsHeader.PK);
			});
		}

		public void TestRelatedObjectsForUSInBond()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			var inBondHeader = Factory.New<Enterprise.Integration.Customs.US.InBond.ICusInBondHeader>();
			inBondHeader.BH_ParentID = shipment.PK;
			inBondHeader.BH_ParentTableCode = shipment.TablePrefix;
			var relatedObjects = new ForwardingShipmentDocManagerInfo(shipment).RelatedObjects;
			AssertCollectionContains(inBondHeader, relatedObjects);
		}

		public void TestAllRelatedObjectsRetrievedForCusExitDetail()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();

			var exitHeader = Factory.New<Enterprise.Integration.Customs.EU.ICusExitControlHeader>();
			exitHeader.CEH_ParentID = shipment.PK;
			exitHeader.CEH_ParentTableCode = shipment.TablePrefix;
			exitHeader.CEH_ReferenceNumber = "ref";
			var exitDetail = Factory.New<Enterprise.Integration.Customs.EU.ICusExitDetail>();
			exitDetail.CED_CEH = exitHeader.PK;
			Factory.Save();

			var relatedObjects = new ForwardingShipmentDocManagerInfo(shipment).RelatedObjects;
			AssertCollectionContains(exitDetail, relatedObjects);
		}

		public void TestAllRelatedObjectsRetrievedForCusExitHeader()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var exitHeader = Factory.New<Enterprise.Integration.Customs.EUExitControl.ICusExitHeader>();
			exitHeader.CXH_ParentID = shipment.PK;
			exitHeader.CXH_ParentTableCode = shipment.TablePrefix;
			((BusinessObject)exitHeader).FillWithValidTestData();

			var exitConsignment = Factory.New<Enterprise.Integration.Customs.EUExitControl.ICusExitConsignment>();
			exitConsignment.CXC_CXH_Header = exitHeader.PK;
			((BusinessObject)exitConsignment).FillWithValidTestData();

			var exitReport = Factory.New<Enterprise.Integration.Customs.EUExitControl.ICusExitReport>();
			exitReport.CER_CXH_Header = exitHeader.PK;
			((BusinessObject)exitReport).FillWithValidTestData();

			Factory.Save();

			var relatedObjects = shipment.DocManagerInfo.RelatedObjects;
			AssertCollectionContains(exitHeader, relatedObjects);
			AssertCollectionContains(exitReport, relatedObjects);
			AssertCollectionContains(exitConsignment, relatedObjects);
		}

		public void TestAllRelatedObjectsRetrievedForCusExitHeader_MultipleUnsavedCusExitHeader_NoException()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var exitHeader = Factory.New<Enterprise.Integration.Customs.EUExitControl.ICusExitHeader>();
			exitHeader.CXH_ParentID = shipment.PK;
			exitHeader.CXH_ParentTableCode = shipment.TablePrefix;
			((BusinessObject)exitHeader).FillWithValidTestData();

			var exitHeader2 = Factory.New<Enterprise.Integration.Customs.EUExitControl.ICusExitHeader>();
			exitHeader2.CXH_ParentID = shipment.PK;
			exitHeader2.CXH_ParentTableCode = shipment.TablePrefix;
			((BusinessObject)exitHeader).FillWithValidTestData();

			var exitConsignment = Factory.New<Enterprise.Integration.Customs.EUExitControl.ICusExitConsignment>();
			exitConsignment.CXC_CXH_Header = exitHeader.PK;
			((BusinessObject)exitConsignment).FillWithValidTestData();

			var exitReport = Factory.New<Enterprise.Integration.Customs.EUExitControl.ICusExitReport>();
			exitReport.CER_CXH_Header = exitHeader.PK;
			((BusinessObject)exitReport).FillWithValidTestData();

			AssertNoExceptionThrown(() => _ = shipment.DocManagerInfo.RelatedObjects);
		}

		Order Order;
	}
}
