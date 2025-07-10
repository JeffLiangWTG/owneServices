using System;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Module;
using Enterprise.Freight.Forwarding.Module.Testing;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.GUI;
using Enterprise.Tracking.Business.Shipments;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingShipmentFilterBusinessObject))]
	[HttpContextEnabledTest]
	sealed class TrackingShipmentFilterBusinessObjectTest : ShipmentFilterBusinessObjectTest
	{
		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new TrackingShipmentFilterBusinessObject();
		}

		#endregion

		#region Overrides

		#region TestStatusFilter

		public override void TestBaseFilter()
		{
			SetupBaseFilterTestData();

			AssertNotNull(FilterStripBizO.Filter);

			ModuleFlagsFilter showBookings = (ModuleFlagsFilter)FilterStripBizO[JobShipmentFilterBusinessObject.Descriptions.ShowTranshipCrossTradeNoConsol];
			ModuleFlagsFilter hiddenFwdRegisteredOrBooking = (ModuleFlagsFilter)FilterStripBizO[JobShipmentFilterBusinessObject.Descriptions.HiddenFwdRegistered];

			AssertNotNull(showBookings);
			AssertNotNull(hiddenFwdRegisteredOrBooking);
			AssertEquals(hiddenFwdRegisteredOrBooking.Visibility, FilterVisibility.AlwaysAppliedAndHidden);
			AssertEquals(showBookings.Visibility, FilterVisibility.AlwaysApplied);

			ForwardingShipmentCollection shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
			shipments.Load();

			Assert("FC-: Expect collection to contain Shipment1", shipments.Contains(Shipment1));
			Assert("FCB: Expect collection to contain Shipment2", shipments.Contains(Shipment2));
			Assert("F--: Expect collection to contain Shipment3", shipments.Contains(Shipment3));
			Assert("--B: Expect collection not to contain Shipment4 because it is not Forward Registered", !shipments.Contains(Shipment4));
			Assert("-C-: Expect collection not to contain Shipment5 because it is not Forward Registered", !shipments.Contains(Shipment5));
		}

		#endregion

		public void TestGetModuleFiltersWhenTrackingCustomFilterNamesClashWithReservedNames()
		{
			var customFieldName = "ETA";
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "SHP";

			var columnDef = template.GenCustomColumnDefinitions.AddNew();
			columnDef.XC_Name = customFieldName;
			columnDef.XC_Type = AddOnColumnDataType.Codes.String;

			Factory.Save();

			var collection = new TrackingShipmentFilterBusinessObject().ModuleFilters;

			AssertNotNull(collection[customFieldName]);
			AssertNotNull(collection[customFieldName + " " + WorkflowCustomFieldsFilter.WorkflowCustomFieldDescriptionDuplicateSuffix]);
		}

		public void TestDuplicateStatus()
		{
			ProcessTaskTemplate template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = JobInvoicingConsumerTypes.Shipment.Code;

			GenCustomColumnDefinition def11 = template1.GenCustomColumnDefinitions.AddNew();
			def11.XC_Name = "Status";
			def11.XC_Type = AddOnColumnDataType.Codes.String;

			Factory.Save();

			ModuleFilterCollection filterCollection = new TrackingShipmentFilterBusinessObject().ModuleFilters;
			var statusFilterName = def11.XC_Name + " " + WorkflowCustomFieldsFilter.WorkflowCustomFieldDescriptionDuplicateSuffix;

			AssertNotNull(filterCollection[statusFilterName]);
			AssertEquals("Workflow Custom Fields", filterCollection[statusFilterName].Category.Description.GetUnresolvedString());

			if (ZArchitecture.Environment.Globals.IsWeb)
			{
				AssertNotNull(filterCollection["Status (Web)"]);
				AssertEquals("Status and Flags", filterCollection["Status (Web)"].Category.Description.GetUnresolvedString());
				AssertEquals("Status (Web)", filterCollection["Status (Web)"].MultilingualDescription.GetUnresolvedString());
			}
			else
			{
				AssertNotNull(filterCollection["Status"]);
				AssertEquals("Status and Flags", filterCollection["Status"].Category.Description.GetUnresolvedString());
				AssertEquals("Status", filterCollection["Status"].MultilingualDescription.GetUnresolvedString());
			}
		}

		public void TestStatusFilter()
		{
			Shipment1.DocsAndCartage.JP_DeliveryCartageCompleted = DateTime.Now.AddDays(-1);
			Shipment2.DocsAndCartage.JP_DeliveryCartageCompleted = DateTime.Now.AddDays(-5);
			((IBusinessObjectInternals)Shipment3.DocsAndCartage).Row["JP_DeliveryCartageCompleted"] = DBNull.Value;
			((IBusinessObjectInternals)Shipment4.DocsAndCartage).Row["JP_DeliveryCartageCompleted"] = DBNull.Value;
			((IBusinessObjectInternals)Shipment5.DocsAndCartage).Row["JP_DeliveryCartageCompleted"] = DBNull.Value;

			Factory.Save();

			TrackingShipmentFilterBusinessObject filter = new TrackingShipmentFilterBusinessObject();
			((ModuleTextFilter)filter["Status"]).Property = ShipmentStatus.Codes.Delivered;
			((ModuleTextFilter)filter["Status"]).IsActive = true;
			TrackingShipmentCollection collection = new TrackingShipmentCollection(new BusinessObjectFactory());
			collection.Load(filter.Filter);

			AssertEquals(2, collection.Count);

			((ModuleTextFilter)filter["Status"]).Property = ShipmentStatus.Codes.Undelivered;
			((ModuleTextFilter)filter["Status"]).IsActive = true;
			collection.Load(filter.Filter);

			AssertEquals(4, collection.Count);

			((ModuleTextFilter)filter["Status"]).Property = ShipmentStatus.Codes.All;
			((ModuleTextFilter)filter["Status"]).IsActive = true;
			collection.Load(filter.Filter);

			AssertEquals(6, collection.Count);
		}

		public void TestCreatedTimeFilter()
		{
			Shipment1.JS_SystemCreateTimeUtc = new DateTime(2009, 01, 02);
			Shipment2.JS_SystemCreateTimeUtc = new DateTime(2009, 01, 25);

			Factory.Save();

			var filter = new TrackingShipmentFilterBusinessObject();
			var collection = new TrackingShipmentCollection(new BusinessObjectFactory());

			var dateFilter = (ModuleDateFilter)filter[TrackingShipmentFilterBusinessObject.Descriptions.AuditInformation.CreatedTime];

			dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			dateFilter.Property1 = new DateTime(2009, 01, 01);
			dateFilter.Property2 = new DateTime(2009, 01, 03);
			dateFilter.IsActive = true;
			collection.Load(filter.Filter);

			AssertEquals(1, collection.Count);

			dateFilter.Property2 = new DateTime(2009, 02, 01);
			collection.Load(filter.Filter);

			AssertEquals(2, collection.Count);
		}

		public void TestCustomerCommonReferencesFilter()
		{
			Shipment1.JS_BookingReference = "test1";

			var item = Shipment2.DocsAndCartage.OrderItems.AddNew();
			item.JT_OrderReference = "test12";

			var packlines = Shipment3.OuterPackLines.AddNew();
			packlines.JL_RefNumber = "test123";

			var packlines2 = Shipment4.OuterPackLines.AddNew();
			packlines2.JL_CustomAttrib1 = "test12344";

			var order = Factory.New<Order>();
			order.JD_OrderNumber = "test12345";
			order.JD_JS = Shipment5.PK;
			order.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;

			Factory.Save();

			TrackingShipmentFilterBusinessObject filter = new TrackingShipmentFilterBusinessObject();
			TrackingShipmentCollection collection = new TrackingShipmentCollection(new BusinessObjectFactory());
			collection.Load(filter.Filter);

			AssertEquals("Should be 6 shipments.", 6, collection.Count);

			((ModuleNumberFilter)filter["Customer Common References"]).Property = "test1";
			((ModuleNumberFilter)filter["Customer Common References"]).IsActive = true;
			collection.Load(filter.Filter);

			AssertEquals("Should be 5 shipments.", 5, collection.Count);
			Assert("Should contain Shipment1", collection.Contains(Shipment1));
			Assert("Should contain Shipment2", collection.Contains(Shipment2));
			Assert("Should contain Shipment3", collection.Contains(Shipment3));
			Assert("Should contain Shipment4", collection.Contains(Shipment4));
			Assert("Should contain Shipment5", collection.Contains(Shipment5));

			((ModuleNumberFilter)filter["Customer Common References"]).Property = "test12";
			((ModuleNumberFilter)filter["Customer Common References"]).IsActive = true;
			collection.Load(filter.Filter);

			AssertEquals("Should be 4 shipments.", 4, collection.Count);
			Assert("Should contain Shipment2", collection.Contains(Shipment2));
			Assert("Should contain Shipment3", collection.Contains(Shipment3));
			Assert("Should contain Shipment4", collection.Contains(Shipment4));
			Assert("Should contain Shipment5", collection.Contains(Shipment5));

			((ModuleNumberFilter)filter["Customer Common References"]).Property = "test123";
			((ModuleNumberFilter)filter["Customer Common References"]).IsActive = true;
			collection.Load(filter.Filter);

			AssertEquals("Should be 3 shipments.", 3, collection.Count);
			Assert("Should contain Shipment3", collection.Contains(Shipment3));
			Assert("Should contain Shipment4", collection.Contains(Shipment4));
			Assert("Should contain Shipment5", collection.Contains(Shipment5));

			((ModuleNumberFilter)filter["Customer Common References"]).Property = "test1234";
			((ModuleNumberFilter)filter["Customer Common References"]).IsActive = true;
			collection.Load(filter.Filter);

			AssertEquals("Should be 2 shipments.", 2, collection.Count);
			Assert("Should contain Shipment4", collection.Contains(Shipment4));
			Assert("Should contain Shipment5", collection.Contains(Shipment5));

			((ModuleNumberFilter)filter["Customer Common References"]).Property = "test12345";
			((ModuleNumberFilter)filter["Customer Common References"]).IsActive = true;
			collection.Load(filter.Filter);

			AssertEquals("Should be 1 shipment.", 1, collection.Count);
			Assert("Should contain Shipment5", collection.Contains(Shipment5));

			((ModuleNumberFilter)filter["Customer Common References"]).Property = "test123456";
			((ModuleNumberFilter)filter["Customer Common References"]).IsActive = true;
			collection.Load(filter.Filter);

			AssertEquals("Should be 0 shipments.", 0, collection.Count);
		}

		public void TestRemoveExclusivity()
		{
			AssertRemoveExclusivity(true);
		}

		public void TestRemoveExclusivity_False()
		{
			AssertRemoveExclusivity(false);
		}

		void AssertRemoveExclusivity(bool removeExclusivity)
		{
			var filter = new TrackingShipmentFilterBusinessObject();
			filter.RemoveExclusivity = removeExclusivity;

			var shipmentNoFilter = (ModuleFountainFilter)filter["Shipment #"];
			shipmentNoFilter.Property = "1001";
			shipmentNoFilter.IsActive = true;
			var orderNoFilter = (ModuleNumberFilter)filter["Order #"];
			orderNoFilter.Property = "1001";
			orderNoFilter.IsActive = true;

			if (removeExclusivity)
			{
				AssertContains("Shipment # filter is not exclusive, so filter should contain Order #", " JD_OrderNumber", filter.Filter.LiteralTextADO);
			}
			else
			{
				AssertNotContains("Shipment # filter is exclusive, so filter should not contain Order #", " JD_OrderNumber", filter.Filter.LiteralTextADO);
			}
		}

		public override void TestLoadDischargeAlwaysVisible()
		{
			Assert("This tests only shipment functionality", true);
		}

		#endregion
	}
}
