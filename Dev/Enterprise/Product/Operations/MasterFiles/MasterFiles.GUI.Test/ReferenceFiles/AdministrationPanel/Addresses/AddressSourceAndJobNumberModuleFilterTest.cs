using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Forwarding;
using static Enterprise.MasterFiles.GUI.AddressesFilterBusinessObject;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(AddressSourceAndJobNumberModuleFilter))]
	class AddressSourceAndJobNumberModuleFilterTest : ModuleTextFilterTest
	{
		public void TestFilter_All()
		{
			var uniqStr = Guid.NewGuid().ToString();

			var dtbBooking1 = Factory.NewWithValidTestData(ObjectFactory.GetType<IDtbBooking>(), TestBusinessObjectKind.MinimumRequiredToSave);
			var jobDocAddress1 = Factory.NewWithValidTestData<JobDocAddress>(TestBusinessObjectKind.MinimumRequiredToSave);
			jobDocAddress1.E2_ParentID = dtbBooking1.PK;
			jobDocAddress1.E2_ParentTableCode = DtbBookingSchema.Constants.Prefix;
			jobDocAddress1.E2_Address1 = uniqStr;

			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType<IForwardingShipment>(), TestBusinessObjectKind.MinimumRequiredToSave);
			var jobDocAddress2 = Factory.NewWithValidTestData<JobDocAddress>(TestBusinessObjectKind.MinimumRequiredToSave);
			jobDocAddress2.E2_ParentID = shipment.PK;
			jobDocAddress2.E2_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobDocAddress2.E2_Address1 = uniqStr;

			var dtbBookingConsolidation = Factory.NewWithValidTestData(ObjectFactory.GetType<IDtbBookingConsolidation>(), TestBusinessObjectKind.MinimumRequiredToSave);
			dtbBookingConsolidation[DtbBookingConsolidationSchema.Constants.KB_ParentTableCode] = JobShipmentSchema.Constants.Prefix;
			dtbBookingConsolidation[DtbBookingConsolidationSchema.Constants.KB_ParentID] = shipment.PK;
			var jobDocAddress3 = Factory.NewWithValidTestData<JobDocAddress>(TestBusinessObjectKind.MinimumRequiredToSave);
			jobDocAddress3.E2_ParentID = dtbBookingConsolidation.PK;
			jobDocAddress3.E2_ParentTableCode = DtbBookingConsolidationSchema.Constants.Prefix;
			jobDocAddress3.E2_Address1 = uniqStr;

			var dtbBooking2 = Factory.NewWithValidTestData(ObjectFactory.GetType<IDtbBooking>(), TestBusinessObjectKind.MinimumRequiredToSave);
			dtbBooking2[DtbBookingSchema.Constants.KM_KB_Booking] = dtbBookingConsolidation[DtbBookingConsolidationSchema.Constants.PK];
			var jobDocAddress4 = Factory.NewWithValidTestData<JobDocAddress>(TestBusinessObjectKind.MinimumRequiredToSave);
			jobDocAddress4.E2_ParentID = dtbBooking2.PK;
			jobDocAddress4.E2_ParentTableCode = DtbBookingSchema.Constants.Prefix;
			jobDocAddress4.E2_Address1 = uniqStr;

			var dtbBookingInstruction = Factory.NewWithValidTestData(ObjectFactory.GetType<IDtbBookingInstruction>(), TestBusinessObjectKind.MinimumRequiredToSave);
			dtbBookingInstruction[DtbBookingInstructionSchema.KN_KM_BookingMovement] = dtbBooking2.PK;
			var jobDocAddress5 = Factory.NewWithValidTestData<JobDocAddress>(TestBusinessObjectKind.MinimumRequiredToSave);
			jobDocAddress5.E2_ParentID = dtbBookingInstruction.PK;
			jobDocAddress5.E2_ParentTableCode = DtbBookingInstructionSchema.Constants.Prefix;
			jobDocAddress5.E2_Address1 = uniqStr;

			Factory.Save();

			var filter = new AddressesFilterBusinessObject();

			((ModuleTextFilter)filter[AddressFilterConstants.Address1]).Property = uniqStr;
			((ModuleTextFilter)filter[AddressFilterConstants.Address1]).IsActive = true;

			var addressSourceAndJobNumberFilter = filter[AddressFilterConstants.AddressSourceAndJobNumber] as AddressSourceAndJobNumberModuleFilter;
			AssertNotNull("Filter should exist", addressSourceAndJobNumberFilter);

			addressSourceAndJobNumberFilter.IsActive = true;
			addressSourceAndJobNumberFilter.AddressSourceCode = "";
			addressSourceAndJobNumberFilter.JobNumber = shipment[JobShipmentSchema.JS_UniqueConsignRef].ToString();
			var collection = new MDMAdminPanelAddressCollection(Factory, filter.Filter);
			collection.Load();
			AssertEquals(0, collection.Count);

			addressSourceAndJobNumberFilter.AddressSourceCode = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;
			collection = new MDMAdminPanelAddressCollection(Factory, filter.Filter);
			collection.Load();
			AssertEquals(4, collection.Count);
			AssertCollectionContains(jobDocAddress2.PK, collection.GetPKs());
			AssertCollectionContains(jobDocAddress3.PK, collection.GetPKs());
			AssertCollectionContains(jobDocAddress4.PK, collection.GetPKs());
			AssertCollectionContains(jobDocAddress5.PK, collection.GetPKs());

			addressSourceAndJobNumberFilter.JobNumber = "";
			collection = new MDMAdminPanelAddressCollection(Factory, filter.Filter);
			collection.Load();
			AssertEquals(4, collection.Count);
			AssertCollectionContains(jobDocAddress2.PK, collection.GetPKs());
			AssertCollectionContains(jobDocAddress3.PK, collection.GetPKs());
			AssertCollectionContains(jobDocAddress4.PK, collection.GetPKs());
			AssertCollectionContains(jobDocAddress5.PK, collection.GetPKs());

			addressSourceAndJobNumberFilter.JobNumber = shipment[JobShipmentSchema.JS_UniqueConsignRef].ToString();
			addressSourceAndJobNumberFilter.AddressSourceCode = WorkflowDescriptors.DtbBookingWorkflowDescriptorCode;
			collection = new MDMAdminPanelAddressCollection(Factory, filter.Filter);
			collection.Load();
			AssertEquals(0, collection.Count);

			addressSourceAndJobNumberFilter.JobNumber = dtbBooking1[DtbBookingSchema.KM_JobID].ToString();
			collection = new MDMAdminPanelAddressCollection(Factory, filter.Filter);
			collection.Load();
			AssertEquals(1, collection.Count);
			AssertCollectionContains(jobDocAddress1.PK, collection.GetPKs());

			addressSourceAndJobNumberFilter.JobNumber = "";
			collection = new MDMAdminPanelAddressCollection(Factory, filter.Filter);
			collection.Load();
			AssertEquals(3, collection.Count);
			AssertCollectionContains(jobDocAddress1.PK, collection.GetPKs());
			AssertCollectionContains(jobDocAddress4.PK, collection.GetPKs());
			AssertCollectionContains(jobDocAddress5.PK, collection.GetPKs());
		}

		public void TestFilter_WithIsBlankOperator()
		{
			var uniqStr = Guid.NewGuid().ToString();

			var dtbBooking = Factory.NewWithValidTestData(ObjectFactory.GetType<IDtbBooking>(), TestBusinessObjectKind.MinimumRequiredToSave);
			var jobDocAddress1 = Factory.NewWithValidTestData<JobDocAddress>(TestBusinessObjectKind.MinimumRequiredToSave);
			jobDocAddress1.E2_ParentID = dtbBooking.PK;
			jobDocAddress1.E2_ParentTableCode = DtbBookingSchema.Constants.Prefix;
			jobDocAddress1.E2_Address1 = uniqStr;

			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType<IForwardingShipment>(), TestBusinessObjectKind.MinimumRequiredToSave);
			var jobDocAddress2 = Factory.NewWithValidTestData<JobDocAddress>(TestBusinessObjectKind.MinimumRequiredToSave);
			jobDocAddress2.E2_ParentID = shipment.PK;
			jobDocAddress2.E2_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobDocAddress2.E2_Address1 = uniqStr;
			Factory.Save();

			var filter = new AddressesFilterBusinessObject();

			((ModuleTextFilter)filter[AddressFilterConstants.Address1]).Property = uniqStr;
			((ModuleTextFilter)filter[AddressFilterConstants.Address1]).IsActive = true;

			var addressSourceAndJobNumberFilter = filter[AddressFilterConstants.AddressSourceAndJobNumber] as AddressSourceAndJobNumberModuleFilter;
			AssertNotNull("Filter should exist", addressSourceAndJobNumberFilter);

			addressSourceAndJobNumberFilter.IsActive = true;
			addressSourceAndJobNumberFilter.AddressSourceCode = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;
			addressSourceAndJobNumberFilter.JobNumber = shipment[JobShipmentSchema.JS_UniqueConsignRef].ToString();
			AssertEquals("Precondition", shipment[JobShipmentSchema.JS_UniqueConsignRef].ToString(), addressSourceAndJobNumberFilter.JobNumber);

			addressSourceAndJobNumberFilter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			AssertEquals(true, addressSourceAndJobNumberFilter.JobNumberInfo.ReadOnly);
			AssertEquals(ZString.Empty, addressSourceAndJobNumberFilter.JobNumber);
			var collection = new MDMAdminPanelAddressCollection(Factory, filter.Filter);
			collection.Load();
			AssertEquals(0, collection.Count);

			addressSourceAndJobNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			AssertEquals(false, addressSourceAndJobNumberFilter.JobNumberInfo.ReadOnly);
			AssertEquals(shipment[JobShipmentSchema.JS_UniqueConsignRef].ToString(), addressSourceAndJobNumberFilter.JobNumber);
			collection = new MDMAdminPanelAddressCollection(Factory, filter.Filter);
			collection.Load();
			AssertEquals(1, collection.Count);
			AssertCollectionContains(jobDocAddress2.PK, collection.GetPKs());
		}

		public void TestSerialize_Deserialize_PropertiesFromToXml()
		{
			var filterStripBizO = new DummyFilterStripBusinessObject();
			var filter = new AddressSourceAndJobNumberModuleFilter("TEST");

			filterStripBizO.AddModuleFilterForTest(filter);
			var strip = filterStripBizO.FilterStrips.AddNew();
			strip.FilterDescription = filter.Description;
			filter.AddressSourceCode = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;
			filter.JobNumber = "ABC";

			var savedLayout = new DataGridLayoutManager().SavePreconfiguredLayout(filterStripBizO, "layoutName", false, false, SaveColumnLayout.Ignore);

			strip.FilterDescription = "";
			strip.Delete();

			var loadedFilter = (AddressSourceAndJobNumberModuleFilter)filterStripBizO[filter.Description];

			filterStripBizO.LoadLayout(savedLayout);

			AssertEquals("Staff Assignment Person", WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode, loadedFilter.AddressSourceCode);
			AssertEquals("Staff Assignment Role", "ABC", loadedFilter.JobNumber);
		}

		public void TestAddressSourcePairList()
		{
			var filter = new AddressSourceAndJobNumberModuleFilter("TEST");
			AssertContainsExactElementsInAnyOrder(AddressSourceType.AddressSourcePairList, filter.AddressSourceList);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new AddressSourceAndJobNumberModuleFilter("TEST");
		}

		#endregion
	}
}
