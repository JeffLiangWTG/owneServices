using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(ServiceLevelFilterBusinessObject))]
	sealed class ServiceLevelFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new ServiceLevelFilterBusinessObject();
		}

		public void TestGetIsSystem()
		{
			var dummy = Factory.NewWithValidTestData<RefServiceLevel>();

			var systemService = Factory.New<RefServiceLevel>();
			var systemNonService = Factory.New<RefServiceLevel>();

			systemService.RS_IsSystem = true;
			systemService.RS_IsSystem = false;

			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = filterBizO["RS_IsSystem"];

			var coll = new RefServiceLevelCollection(Factory, filterBizO.Filter);

			AssertEquals(true, coll.Contains(systemService));
			AssertEquals(true, coll.Contains(systemNonService));
		}

		#endregion

		#region TestGetCarrierQuery

		public void TestGetCarrierQuery()
		{
			OrgHeader transportCo = Factory.NewWithValidTestData<OrgHeader>();
			transportCo.OH_IsTransportClient = true;

			OrgCarrierServiceLevel carrierServiceLevel = transportCo.MiscServ.CarrierServiceLevels.AddNew();
			carrierServiceLevel.PL_Code = "ABC";

			RefServiceLevel serviceLevelAbc = Factory.New<RefServiceLevel>();
			RefServiceLevel serviceLevelXyz = Factory.New<RefServiceLevel>();
			serviceLevelAbc.RS_Code = "ABC";
			serviceLevelXyz.RS_Code = "XYZ";
			Factory.Save();

			ServiceLevelFilterBusinessObject filterStripBizO = (ServiceLevelFilterBusinessObject)GetNewFilterStripBusinessObject();

			ModuleGuidFilter filter = (ModuleGuidFilter)filterStripBizO["Service Levels supported by Carrier"];
			filter.IsActive = true;
			filter.Property = transportCo.PK;

			RefServiceLevelCollection collection = new RefServiceLevelCollection(Factory, filterStripBizO.Filter);
			AssertEquals(true, collection.Contains(serviceLevelAbc));
			AssertEquals(false, collection.Contains(serviceLevelXyz));

			IList<RefServiceLevel> findings = new List<RefServiceLevel>(collection.Find(new ZQuery(RefServiceLevelSchema.RS_Code, "STD")));
			AssertEquals(1, findings.Count);
		}

		#endregion

		#region TestFiltersExistence

		public void TestFiltersExistenceWhenCalculateDeliveryDueDateByTransportModeIsTurnedOn()
		{
			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty,
					   Guid.Empty, Guid.Empty,
					   new CalculateDeliveryDueDateOptions { IsActive = true, TransportModes = CalculateDeliveryDueDateTransportModeCollection() }))
			{
				var filterBizO = GetNewFilterStripBusinessObject();

				AssertNotNull(
					filterBizO[ServiceLevelFilterBusinessObject.FilterConstants.ServiceLevelsSupportedByCarrier]);
				AssertNotNull(filterBizO[ServiceLevelFilterBusinessObject.FilterConstants.Code]);
				AssertNotNull(filterBizO[ServiceLevelFilterBusinessObject.FilterConstants.Description]);
				AssertNotNull(filterBizO[ServiceLevelFilterBusinessObject.FilterConstants.ServiceDeliveryType]);
				AssertNotNull(filterBizO[ServiceLevelFilterBusinessObject.FilterConstants.ServiceDeliveryPercentage]);
				AssertNotNull(filterBizO[ServiceLevelFilterBusinessObject.FilterConstants.IsGateway]);
				AssertNotNull(filterBizO[ServiceLevelFilterBusinessObject.FilterConstants.DeliverOnWeekend]);
				AssertNotNull(filterBizO[ServiceLevelFilterBusinessObject.FilterConstants.DefaultTransitTime]);
				AssertNotNull(filterBizO[ServiceLevelFilterBusinessObject.FilterConstants.DefaultArrivalTime]);
				AssertNotNull(filterBizO[ServiceLevelFilterBusinessObject.FilterConstants.ServiceDeliveryDueTime]);
			}
		}

		public void TestFiltersExistenceWhenCalculateDeliveryDueDateByTransportModeIsTurnedOff()
		{
			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty,
					   Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = false }))
			{
				var filterBizO = GetNewFilterStripBusinessObject();

				AssertNotNull(
					filterBizO[ServiceLevelFilterBusinessObject.FilterConstants.ServiceLevelsSupportedByCarrier]);
				AssertNotNull(filterBizO[ServiceLevelFilterBusinessObject.FilterConstants.Code]);
				AssertNotNull(filterBizO[ServiceLevelFilterBusinessObject.FilterConstants.Description]);
				AssertNotNull(filterBizO[ServiceLevelFilterBusinessObject.FilterConstants.ServiceDeliveryType]);
				AssertNotNull(filterBizO[ServiceLevelFilterBusinessObject.FilterConstants.ServiceDeliveryPercentage]);
				AssertNotNull(filterBizO[ServiceLevelFilterBusinessObject.FilterConstants.IsGateway]);
				AssertNull(filterBizO[ServiceLevelFilterBusinessObject.FilterConstants.DeliverOnWeekend]);
				AssertNull(filterBizO[ServiceLevelFilterBusinessObject.FilterConstants.DefaultTransitTime]);
				AssertNull(filterBizO[ServiceLevelFilterBusinessObject.FilterConstants.DefaultArrivalTime]);
				AssertNull(filterBizO[ServiceLevelFilterBusinessObject.FilterConstants.ServiceDeliveryDueTime]);
			}
		}

		#endregion

		#region TestDeliverOnWeekend

		public void TestDeliverOnWeekend()
		{
			var serviceLevel1 = Factory.NewWithValidTestData<RefServiceLevel>();
			serviceLevel1.RS_Code = "ABC";
			serviceLevel1.RS_DeliverOnSaturday = true;
			serviceLevel1.RS_DeliverOnSunday = true;

			var serviceLevel2 = Factory.NewWithValidTestData<RefServiceLevel>();
			serviceLevel2.RS_Code = "XYZ";
			serviceLevel2.RS_DeliverOnSaturday = false;
			serviceLevel2.RS_DeliverOnSunday = false;

			var serviceLevel3 = Factory.NewWithValidTestData<RefServiceLevel>();
			serviceLevel3.RS_Code = "XZY";
			serviceLevel3.RS_DeliverOnSaturday = true;
			serviceLevel3.RS_DeliverOnSunday = false;

			Factory.Save();

			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty,
					   Guid.Empty, Guid.Empty,
					   new CalculateDeliveryDueDateOptions
					   {
						   IsActive = true, TransportModes = CalculateDeliveryDueDateTransportModeCollection()
					   }))
			{
				var filterBizO = GetNewFilterStripBusinessObject();
				var deliverOnWeekendFilter =
					(ModuleFlagsFilter)filterBizO[ServiceLevelFilterBusinessObject.FilterConstants.DeliverOnWeekend];
				deliverOnWeekendFilter.Property0 = true;
				deliverOnWeekendFilter.IsActive = true;

				var results = new RefServiceLevelCollection(Factory).Find(filterBizO.Filter);
				AssertContainsExactElementsInAnyOrder(new[] { serviceLevel1, serviceLevel3 }, results);
			}
		}

		#endregion

		#region TestDefaultArrivalTime

		public void TestDefaultArrivalTime()
		{
			var serviceLevel1 = Factory.New<RefServiceLevel>();
			serviceLevel1.RS_Code = "ABC";
			serviceLevel1.RS_DefaultArrivalTime = new ZDateTime(1900, 1, 1, 8, 0, 0);

			var serviceLevel2 = Factory.New<RefServiceLevel>();
			serviceLevel2.RS_Code = "XYZ";
			serviceLevel2.RS_DefaultArrivalTime = new ZDateTime(1900, 1, 1, 12, 0, 0);

			var serviceLevel3 = Factory.New<RefServiceLevel>();
			serviceLevel3.RS_Code = "XZY";
			serviceLevel3.RS_DefaultArrivalTime = new ZDateTime(1900, 1, 1, 18, 0, 0);

			Factory.Save();

			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty,
					   Guid.Empty, Guid.Empty,
					   new CalculateDeliveryDueDateOptions
					   {
						   IsActive = true, TransportModes = CalculateDeliveryDueDateTransportModeCollection()
					   }))
			{
				var filterBizO = GetNewFilterStripBusinessObject();

				var arrivalTimeFilter =
					(ModuleTimeFilter)filterBizO[ServiceLevelFilterBusinessObject.FilterConstants.DefaultArrivalTime];
				arrivalTimeFilter.IsActive = true;
				arrivalTimeFilter.PropertySearch = ModuleTimeFilter.SpecifiedTimeRange;

				arrivalTimeFilter.Property1 = new ZTime(7, 0);
				arrivalTimeFilter.Property2 = new ZTime(13, 0);

				var results = new RefServiceLevelCollection(Factory, filterBizO.Filter);
				AssertContainsExactElementsInAnyOrder(new[] { serviceLevel1, serviceLevel2 }, results);

				arrivalTimeFilter.Property1 = new ZTime(18, 0);
				arrivalTimeFilter.Property2 = new ZTime(9, 0);

				results = new RefServiceLevelCollection(Factory, filterBizO.Filter);
				AssertContainsExactElementsInAnyOrder(new[] { serviceLevel1, serviceLevel3 }, results);
			}
		}

		#endregion

		#region TestDefaultDeliveryDueTime

		public void TestDefaultDeliveryDueTime()
		{
			var serviceLevel1 = Factory.New<RefServiceLevel>();
			serviceLevel1.RS_Code = "ABC";
			serviceLevel1.RS_DefaultDeliveryDueTime = new ZDateTime(1900, 1, 1, 8, 0, 0);

			var serviceLevel2 = Factory.New<RefServiceLevel>();
			serviceLevel2.RS_Code = "XYZ";
			serviceLevel2.RS_DefaultDeliveryDueTime = new ZDateTime(1900, 1, 1, 12, 0, 0);

			var serviceLevel3 = Factory.New<RefServiceLevel>();
			serviceLevel3.RS_Code = "XZY";
			serviceLevel3.RS_DefaultDeliveryDueTime = new ZDateTime(1900, 1, 1, 18, 0, 0);

			Factory.Save();

			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty,
					   Guid.Empty, Guid.Empty,
					   new CalculateDeliveryDueDateOptions
					   {
						   IsActive = true, TransportModes = CalculateDeliveryDueDateTransportModeCollection()
					   }))
			{
				var filterBizO = GetNewFilterStripBusinessObject();

				var deliveryDueTimeFilter =
					(ModuleTimeFilter)filterBizO[
						ServiceLevelFilterBusinessObject.FilterConstants.ServiceDeliveryDueTime];
				deliveryDueTimeFilter.IsActive = true;
				deliveryDueTimeFilter.PropertySearch = ModuleTimeFilter.SpecifiedTimeRange;

				deliveryDueTimeFilter.Property1 = new ZTime(7, 0);
				deliveryDueTimeFilter.Property2 = new ZTime(13, 0);

				var results = new RefServiceLevelCollection(Factory, filterBizO.Filter);
				AssertContainsExactElementsInAnyOrder(new[] { serviceLevel1, serviceLevel2 }, results);

				deliveryDueTimeFilter.Property1 = new ZTime(18, 0);
				deliveryDueTimeFilter.Property2 = new ZTime(9, 0);

				results = new RefServiceLevelCollection(Factory, filterBizO.Filter);
				AssertContainsExactElementsInAnyOrder(new[] { serviceLevel1, serviceLevel3 }, results);
			}
		}

		#endregion

		#region TestDefaultTransitTime

		public void TestDefaultTransitTime()
		{
			var serviceLevel1 = Factory.NewWithValidTestData<RefServiceLevel>();
			serviceLevel1.RS_DefaultTransitHours = 10;

			var serviceLevel2 = Factory.NewWithValidTestData<RefServiceLevel>();
			serviceLevel2.RS_DefaultTransitHours = 30;

			var serviceLevel3 = Factory.NewWithValidTestData<RefServiceLevel>();
			serviceLevel3.RS_DefaultTransitHours = 50;

			Factory.Save();

			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty,
					   Guid.Empty, Guid.Empty,
					   new CalculateDeliveryDueDateOptions
					   {
						   IsActive = true, TransportModes = CalculateDeliveryDueDateTransportModeCollection()
					   }))
			{
				var filterBizO = GetNewFilterStripBusinessObject();

				var transitTimeFilter =
					(TransitTimeModuleFilter)filterBizO[
						ServiceLevelFilterBusinessObject.FilterConstants.DefaultTransitTime];
				transitTimeFilter.IsActive = true;

				transitTimeFilter.ComparisonOperator = "Greater than";
				transitTimeFilter.TransitDays = 1;
				transitTimeFilter.TransitHours = 4;
				transitTimeFilter.IsActive = true;

				var results = new RefServiceLevelCollection(Factory).Find(filterBizO.Filter);

				AssertContainsExactElementsInAnyOrder(new[] { serviceLevel2, serviceLevel3 }, results);
			}
		}

		#endregion

		static CalculateDeliveryDueDateTransportModeCollection CalculateDeliveryDueDateTransportModeCollection()
		{
			var transportModes = new CalculateDeliveryDueDateTransportModeCollection();
			transportModes.Add(Core.Constants.TransportModes.Air, Core.Constants.TransportModeDescriptions.Air, true);
			return transportModes;
		}
	}
}
