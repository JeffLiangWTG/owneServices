using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(OrderDocManagerInfo))]
	sealed class OrderDocManagerInfoTest : DocManagerInfoTestCase
	{
		#region TestRelatedWarehouseReceiveIsRetrieved

		public void TestRelatedWarehouseReceiveIsRetrieved()
		{
			var order = Factory.New<Order>();
			var warehouseReceive = Factory.New<IWhsReceive>();
			var orderReceivePivot = (BusinessObject)Factory.New<IWhsDocketJobPivot>();
			orderReceivePivot[WhsDocketJobPivotSchema.WV_ParentId] = order.PK;
			orderReceivePivot[WhsDocketJobPivotSchema.WV_ParentTableCode] = order.TablePrefix;
			orderReceivePivot[WhsDocketJobPivotSchema.WV_WD_Docket] = warehouseReceive.PK;
			orderReceivePivot[WhsDocketJobPivotSchema.WV_DocketType] = warehouseReceive.WD_DocketType;

			AssertContainsExactElementsInAnyOrder(new[] { (BusinessObject)warehouseReceive }, ((IDocManagerSupport)order).DocManagerInfo.RelatedObjects);
		}

		public void TestRelatedBusinessOrderModule()
		{
			var order = Factory.NewWithValidTestData<Order>();
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_Code = "BUYER_1";
			buyer.Addresses.AddNew().FillWithValidTestData();
			buyer.Addresses.AddNew().FillWithValidTestData();
			buyer.Contacts.AddNew().FillWithValidTestData();
			buyer.Contacts.AddNew().FillWithValidTestData();
			order.JD_OA_BuyerAddress = buyer.Addresses[1].PK;
			order.JD_OC_BuyerContact = buyer.Contacts[1].PK;
			var orderLine = order.OrderLines.AddNew();
			orderLine.JO_ItemPrice = 2;

			var bookingParty = Factory.NewWithValidTestData<OrgHeader>();
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.Addresses.AddNew().FillWithValidTestData();
			supplier.Addresses.AddNew().FillWithValidTestData();
			supplier.Contacts.AddNew().FillWithValidTestData();
			supplier.Contacts.AddNew().FillWithValidTestData();

			var booking = Factory.New<JobSupplierBooking>();
			booking.JSB_TransportMode = Constants.TransportModes.Sea;
			booking.JSB_BookingId = "JSB00001";
			booking.JSB_LoadMode = "CY";
			booking.JSB_RL_NKLoadPort = "AUSYD";
			booking.JSB_RL_NKDischargePort = "CNCAN";
			booking.JSB_OH_BookingParty = bookingParty.PK;
			booking.JSB_BookedOnDate = ZDate.Today;
			booking.JSB_Status = "PLC";
			booking.JSB_IncoTerm = Constants.IncoTerms.ExWorks;
			booking.SupplierAddress.OrganisationPK = supplier.PK;
			booking.SupplierAddress.E2_OA_Address = supplier.Addresses[1].PK;
			booking.SupplierAddress.ContactPK = supplier.Contacts[1].PK;

			var bookingLine = booking.SupplierBookingLines.AddNew();
			bookingLine.FillWithValidTestData();
			bookingLine.JSL_JSB_Booking = booking.PK;
			bookingLine.JSL_JO_OrderLine = orderLine.PK;
			bookingLine.JSL_BookedQuantity = 6;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;

			var etd = ZDateTime.Today;
			var eta = ZDateTime.Today.AddDays(2);

			var transport = consol.Transports[0];
			transport.JW_ETD = etd;
			transport.JW_ETA = eta;

			var container1 = consol.Containers.AddNew();
			container1.FillWithValidTestData();
			container1.JC_JSB_SupplierBooking = booking.PK;

			var loadListHeader = Factory.NewWithValidTestData<CYContainerLoadList>();
			loadListHeader.CLH_JSB_Booking = booking.PK;
			loadListHeader.CLH_LoadListId = "CLL00001";
			loadListHeader.CLH_OH_LoadListParty = bookingParty.PK;
			loadListHeader.CLH_Status = "SHP";

			var loadListLine = loadListHeader.LoadListLines.AddNew();
			loadListLine.CLL_JSL_BookingLine = bookingLine.PK;
			loadListLine.CLL_JC_Container = container1.PK;

			loadListLine.CLL_Volume = 2.1;
			loadListLine.CLL_VolumeUnit = "M3";
			loadListLine.CLL_Weight = 4.3;
			loadListLine.CLL_WeightUnit = "KG";
			loadListLine.CLL_LoadSequence = 5;
			loadListLine.CLL_Packages = 3;
			loadListLine.CLL_F3_NKPackagesUnit = "PKG";
			loadListLine.CLL_RH_NKCommodityCode = "GEN";
			loadListLine.CLL_HarmonizedCode = "HAR";
			loadListLine.CLL_ReferenceNumber = "RN0001";
			loadListLine.CLL_PackedQuantity = 6;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = booking.JSB_TransportMode;
			shipment.JS_PackingMode = booking.JSB_TransportMode == Constants.TransportModes.Air ? Constants.ContainerModes.ULD : Constants.ContainerModes.FCL;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = booking.SupplierAddress.OrganisationPK;
			shipment.ConsignorDocumentaryAddress.ContactPK = booking.SupplierAddress.ContactPK;
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = booking.SupplierAddress.E2_OA_Address;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = order.Buyer.PK;
			shipment.ConsigneeDocumentaryAddress.ContactPK = order.JD_OC_BuyerContact;
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = order.JD_OA_BuyerAddress;
			shipment.JS_RL_NKOrigin = booking.JSB_RL_NKLoadPort;
			shipment.JS_RL_NKDestination = booking.JSB_RL_NKDischargePort;
			shipment.JS_E_DEP = consol.MostInterestingTransportForBinding.Cast<Transport>().FirstOrDefault()?.JW_ETD ?? ZDateTime.Empty;
			shipment.JS_E_ARV = consol.MostInterestingTransportForBinding.Cast<Transport>().FirstOrDefault()?.JW_ETA ?? ZDateTime.Empty;
			shipment.JS_INCO = booking.JSB_IncoTerm;

			var packLine = shipment.OuterPackLines.AddNew();
			loadListLine.CLL_JL_PackLine = packLine.PK;
			packLine.JL_JC = loadListLine.CLL_JC_Container;
			packLine.JL_ActualVolume = loadListLine.CLL_Volume;
			packLine.JL_ActualVolumeUQ = loadListLine.CLL_VolumeUnit;
			packLine.JL_ActualWeight = loadListLine.CLL_Weight;
			packLine.JL_ActualWeightUQ = loadListLine.CLL_WeightUnit;
			packLine.JL_ContainerPackingOrder = loadListLine.CLL_LoadSequence;
			packLine.JL_PackageCount = loadListLine.CLL_Packages;
			packLine.JL_F3_NKPackType = loadListLine.CLL_F3_NKPackagesUnit;
			packLine.JL_RH_NKCommodityCode = loadListLine.CLL_RH_NKCommodityCode;
			packLine.JL_HarmonisedCode = loadListLine.CLL_HarmonizedCode;
			packLine.JL_RefNumber = loadListLine.CLL_ReferenceNumber;

			Factory.Save();

			AssertContainsExactElementsInAnyOrder(new BusinessObject[] { shipment, consol, booking, loadListHeader }, ((IDocManagerSupport)order).DocManagerInfo.RelatedObjects);
			CombineAssertions(() =>
			{
				var relatedObjs = ((IDocManagerSupport)shipment).DocManagerInfo.RelatedObjects;
				AssertCollectionContains(order, relatedObjs);
				AssertCollectionContains(consol, relatedObjs);
				AssertCollectionContains(booking, relatedObjs);
				AssertCollectionContains(loadListHeader, relatedObjs);
			});
		}

		#endregion

		#region Implementation

		public override BusinessObject GetEmptyParentBusinessObject()
		{
			return Factory.New<Order>();
		}

		public override BusinessObject GetPopulatedParentBusinessObject()
		{
			var order = Factory.New<Order>();
			var warehouseReceive = (BusinessObject)Factory.New<IWhsReceive>();
			var orderReceivePivot = (BusinessObject)Factory.New<IWhsDocketJobPivot>();
			orderReceivePivot[WhsDocketJobPivotSchema.WV_ParentId] = order.PK;
			orderReceivePivot[WhsDocketJobPivotSchema.WV_ParentTableCode] = order.TablePrefix;
			orderReceivePivot[WhsDocketJobPivotSchema.WV_WD_Docket] = warehouseReceive.PK;

			return order;
		}

		#endregion
	}
}
