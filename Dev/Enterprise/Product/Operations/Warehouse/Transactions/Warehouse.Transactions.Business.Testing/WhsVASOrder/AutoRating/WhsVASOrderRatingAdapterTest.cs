using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsVASOrderRatingAdapterTest : WhsTestCaseWithFactory
	{
		#region IAutoRating Members

		#region TestIAutoRatingJobServices

		public void TestIAutoRatingJobServices()
		{
			var client = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("Warehouse");
			var serviceArea = Helper.CreateServiceAreaForVASOrder(whs);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, client);
			var autoRating = (IAutoRating)new WhsVASOrderRatingAdapter(vasOrder);

			var servicesWrapper = autoRating.JobServices;
			var allServiceTypes = vasOrder.Services.AddNew().Lookups.JobServiceType_List;

			foreach (CodeDescriptionPair serviceType in allServiceTypes)
			{
				AssertEquals(true,
					servicesWrapper.Contains(ChargeCodeGroupList.Codes.WHSAdHocServiceJob, serviceType.Code));
			}

			AssertNotEquals("List should not be cached.", autoRating.JobServices, servicesWrapper);
			AssertEquals(allServiceTypes.Count, servicesWrapper.Count);
		}

		#endregion

		#region TestIAutoRatingChargeCodeGroups

		public void TestIAutoRatingChargeCodeGroups()
		{
			var client = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("Warehouse");
			var serviceArea = Helper.CreateServiceAreaForVASOrder(whs);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, client);
			var autoRating = new WhsVASOrderRatingAdapter(vasOrder);

			var ccList = autoRating.ChargeCodeGroups;
			AssertEquals("Should cache and return same object.", ccList, autoRating.ChargeCodeGroups);
		}

		#endregion

		#region TestIAutoRatingPopulateChargeCodeGroups

		public void TestIAutoRatingPopulateChargeCodeGroups()
		{
			var client = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("Warehouse");
			var serviceArea = Helper.CreateServiceAreaForVASOrder(whs);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, client);
			var autoRating = new WhsVASOrderRatingAdapter(vasOrder);

			AssertContainsExactElementsInAnyOrder(new[] { ChargeCodeGroupList.Codes.WHSAdHocServiceJob },
				autoRating.ChargeCodeGroups);
		}

		#endregion

		#region TestIAutoRatingRateTypeToUse

		public void TestIAutoRatingRateTypeToUse()
		{
			var client = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("Warehouse");
			var serviceArea = Helper.CreateServiceAreaForVASOrder(whs);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, client);
			var autoRating = (IAutoRating)new WhsVASOrderRatingAdapter(vasOrder);

			AssertEquals(RateType.Warehouse, autoRating.RateTypeToUse);
		}

		#endregion

		#region TestIAutoRatingStatusInformation

		public void TestIAutoRatingStatusInformation()
		{
			var client = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("Warehouse");
			var serviceArea = Helper.CreateServiceAreaForVASOrder(whs);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, client);
			var autoRating = (IAutoRating)new WhsVASOrderRatingAdapter(vasOrder);
			var info = autoRating.StatusInformation;

			AssertEquals(true, info.CanExecute);
			AssertEquals(ZString.Empty, info.Message);
		}

		#endregion

		#region TestIAutoRatingConsumerType

		public void TestIAutoRatingConsumerType()
		{
			var client = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("Warehouse");
			var serviceArea = Helper.CreateServiceAreaForVASOrder(whs);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, client);
			var autoRating = (IAutoRating)new WhsVASOrderRatingAdapter(vasOrder);

			AssertEquals(JobInvoicingConsumerTypes.WarehouseVASOrder,
				autoRating.InvoicingSupporter.ConsumerType);
		}

		#endregion

		#region TestIAutoRatingMergeCharges

		public void TestIAutoRatingMergeCharges()
		{
			var client = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("Warehouse");
			var serviceArea = Helper.CreateServiceAreaForVASOrder(whs);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, client);
			var autoRating = (IAutoRating)new WhsVASOrderRatingAdapter(vasOrder);

			AssertEquals(MergeChargeOptions.WithinAdapter, autoRating.MergeCharges);
		}

		#endregion

		#region TestAdapterTypeAndID

		public void TestAdapterTypeAndID()
		{
			var client = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("Warehouse");
			var serviceArea = Helper.CreateServiceAreaForVASOrder(whs);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, client);
			var adapter = (IAutoRating)new WhsVASOrderRatingAdapter(vasOrder);

			AssertEquals(AdapterType.WarehouseAdHocService, adapter.AdapterType);
			AssertEquals(vasOrder.WVO_JobID, adapter.OperationalJobCode);
		}

		#endregion

		#endregion

		#region IAutoRatingFreightInfo Members

		#region TestIAutoRatingFreightInfo_FreightMode

		public void TestIAutoRatingFreightInfo_FreightMode()
		{
			var client = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("Warehouse");
			var serviceArea = Helper.CreateServiceAreaForVASOrder(whs);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, client);
			var autoRating = (IAutoRatingFreightInfo)new WhsVASOrderRatingAdapter(vasOrder);

			AssertEquals(FreightMode.UKN, autoRating.FreightMode);
		}

		#endregion

		#region TestIAutoRatingFreightInfo_HouseBillReleaseType

		public void TestIAutoRatingFreightInfo_HouseBillReleaseType()
		{
			var client = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("Warehouse");
			var serviceArea = Helper.CreateServiceAreaForVASOrder(whs);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, client);
			var autoRating = (IAutoRatingFreightInfo)new WhsVASOrderRatingAdapter(vasOrder);

			AssertEquals(ZString.Empty, autoRating.HousebillReleaseType);
		}

		#endregion

		#region TestIAutoRatingFreightInfo_Time

		public void TestIAutoRatingFreightInfo_Time()
		{
			var client = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("Warehouse", "A", 1, 2);
			var product = Helper.CreateProduct("P1", client);
			var serviceArea = Helper.CreateServiceAreaForVASOrder(whs);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, client);
			Helper.CreateWhsVASOrderLine(vasOrder, product, 5m);
			Helper.CreateWhsReceiveWithInventory(client, whs, "R1", product, 5m);
			Factory.Save();
			var initialTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			initialTransfer.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();
			vasOrder.MarkVASOrderAsCompleted(Notify);
			Factory.Save();

			WhsTransfer returnTransfer;
			using (Helper.GetPutawayEngineManagerForVASTransferLineMock())
			{
				returnTransfer = vasOrder.GetOrCreateReturnTransfer(Notify);
			}

			returnTransfer.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();
			AssertNotEquals("Precondition: VAS Order should have finalised date.", ZDateTime.Invalid,
				vasOrder.WVO_FinalizedTimeUtc);

			var autoRating = (IAutoRating)new WhsVASOrderRatingAdapter(vasOrder);
			var rateableMeasures = (RateableMeasureSet)autoRating.RateableMeasures;
			var timeInfo = rateableMeasures.Time;
			AssertEquals(autoRating.JobDatesProvider.GetJobDateByType(JobDateTypes.Codes.ArrivalDate).Day,
				timeInfo.Span.Days);
			AssertEquals(0, timeInfo.Span.Minutes);
			AssertEquals(0, timeInfo.Span.Hours);
		}

		public void TestIAutoRatingFreightInfo_SpecialServiceTime()
		{
			var client = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("Warehouse");
			var serviceArea = Helper.CreateServiceAreaForVASOrder(whs);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, client);
			var autoRating = new WhsVASOrderRatingAdapter(vasOrder);
			var service = vasOrder.Services.AddNew();
			service.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Fumigation;
			service.ES_Completed = ZDateTime.Today;
			service.ES_Duration = new ZDateTime(ZDateTime.Now.Year, 1, 1).AddHours(2);

			var chargeCodeFilter = new ZQuery(
				new ZQuery(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK),
				new ZQuery(AccChargeCodeSchema.AC_Code, "DFUMI"));

			var chargeCode = Factory.Load<AccChargeCode>(chargeCodeFilter).FirstOrDefault();
			chargeCode.AC_ChargeGroup = autoRating.ChargeCodeGroups[0];

			var timeInfo = autoRating.JobServices.Time(chargeCode);
			AssertEquals(2d, timeInfo.Span.TotalHours);
		}

		#endregion

		#region TestIAutoRatingFreightInfo_Measures

		public void TestIAutoRatingFreightInfo_Measures()
		{
			var client = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("Warehouse", "A", 1, 2);
			var product = Helper.CreateProduct("P1", client);
			var serviceArea = Helper.CreateServiceAreaForVASOrder(whs);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, client);
			Helper.CreateWhsVASOrderLine(vasOrder, product, 5m);
			Helper.CreateWhsReceiveWithInventory(client, whs, "R1", product, 5m);
			Factory.Save();
			var initialTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			initialTransfer.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();
			vasOrder.MarkVASOrderAsCompleted(Notify);
			Factory.Save();

			WhsTransfer returnTransfer;
			using (Helper.GetPutawayEngineManagerForVASTransferLineMock())
			{
				returnTransfer = vasOrder.GetOrCreateReturnTransfer(Notify);
			}

			returnTransfer.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();
			AssertNotEquals("Precondition: VAS Order should have finalised date.", ZDateTime.Invalid,
				vasOrder.WVO_FinalizedTimeUtc);

			var autoRating = (IAutoRating)new WhsVASOrderRatingAdapter(vasOrder);
			var rateableMeasures = (RateableMeasureSet)autoRating.RateableMeasures;
			AssertEquals(
				"Without measure for services, the Invoice Detail report will not group the charges correctly.", "SV",
				rateableMeasures.GetUnit(MeasureType.Unidentified));
		}

		#endregion

		#region TestIAutoRatingFreightInfo_ServiceLevel

		public void TestIAutoRatingFreightInfo_ServiceLevel()
		{
			var client = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("Warehouse");
			var serviceArea = Helper.CreateServiceAreaForVASOrder(whs);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, client);
			var autoRating = (IAutoRating)new WhsVASOrderRatingAdapter(vasOrder);

			AssertNotNull(autoRating.ServiceLevel);
		}

		#endregion

		#region TestIAutoRatingFreightInfo_WharfCTOAddress

		public void TestIAutoRatingFreightInfo_WharfCTOAddress()
		{
			var client = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("Warehouse");
			var serviceArea = Helper.CreateServiceAreaForVASOrder(whs);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, client);
			var autoRating = (IAutoRatingFreightInfo)new WhsVASOrderRatingAdapter(vasOrder);

			AssertEquals(vasOrder.Warehouse.WW_OA_WarehouseAddress, autoRating.WharfCTOAddress.PK);
		}

		#endregion

		#endregion

		#region IAutoRatingLocations Members

		#region TestIAutoRatingLocationsOrigin

		public void TestIAutoRatingLocationsOrigin()
		{
			var client = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("Warehouse");
			var serviceArea = Helper.CreateServiceAreaForVASOrder(whs);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, client);
			var autoRating = (IAutoRatingLocations)new WhsVASOrderRatingAdapter(vasOrder);

			AssertNull(autoRating.Origin);
		}

		#endregion

		#region TestIAutoRatingLocationsDestination

		public void TestIAutoRatingLocationsDestination()
		{
			var client = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("Warehouse");
			var serviceArea = Helper.CreateServiceAreaForVASOrder(whs);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, client);
			var autoRating = (IAutoRatingLocations)new WhsVASOrderRatingAdapter(vasOrder);

			AssertNull(autoRating.Destination);
		}

		#endregion

		#region TestIAutoRatingLocationsVia

		public void TestIAutoRatingLocationsVia()
		{
			var client = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("Warehouse");
			var serviceArea = Helper.CreateServiceAreaForVASOrder(whs);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, client);
			var autoRating = (IAutoRatingLocations)new WhsVASOrderRatingAdapter(vasOrder);

			AssertNull(autoRating.GetVia(CostSell.Cost));
			AssertNull(autoRating.GetVia(CostSell.Revenue));
		}

		#endregion

		#endregion

		#region IAutoRatingOrganisations Members

		public void IAutoRatingOrganisations_Consignor()
		{
			var client = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("Warehouse");
			var serviceArea = Helper.CreateServiceAreaForVASOrder(whs);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, client);
			var autoRating = (IAutoRating)new WhsVASOrderRatingAdapter(vasOrder);

			AssertNull(autoRating.DebtorOrgs[Registry.Business.RatingDebtorOrgTypes.CNR]);
		}

		public void IAutoRatingOrganisations_Consignee()
		{
			var client = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("Warehouse");
			var serviceArea = Helper.CreateServiceAreaForVASOrder(whs);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, client);
			var autoRating = (IAutoRating)new WhsVASOrderRatingAdapter(vasOrder);

			AssertNull(autoRating.DebtorOrgs[Registry.Business.RatingDebtorOrgTypes.CNE]);
		}

		public void IAutoRatingOrganisations_PickupCartageEquipment()
		{
			var client = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("Warehouse");
			var serviceArea = Helper.CreateServiceAreaForVASOrder(whs);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, client);
			var autoRating = (IAutoRatingOrganisations)new WhsVASOrderRatingAdapter(vasOrder);

			AssertEquals(ZString.Empty, autoRating.PickupCartageEquipment);
		}

		public void IAutoRatingOrganisations_PickupAddress()
		{
			var client = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("Warehouse");
			var serviceArea = Helper.CreateServiceAreaForVASOrder(whs);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, client);
			var autoRating = (IAutoRatingOrganisations)new WhsVASOrderRatingAdapter(vasOrder);

			AssertNull(autoRating.PickupAddress);
		}

		public void IAutoRatingOrganisations_DeliveryAddress()
		{
			var client = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("Warehouse");
			var serviceArea = Helper.CreateServiceAreaForVASOrder(whs);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, client);
			var autoRating = (IAutoRatingOrganisations)new WhsVASOrderRatingAdapter(vasOrder);

			AssertNull(autoRating.DeliveryAddress);
		}

		public void IAutoRatingOrganisations_DeliveryCartageEquipment()
		{
			var client = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("Warehouse");
			var serviceArea = Helper.CreateServiceAreaForVASOrder(whs);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, client);
			var autoRating = (IAutoRatingOrganisations)new WhsVASOrderRatingAdapter(vasOrder);

			AssertEquals(ZString.Empty, autoRating.DeliveryCartageEquipment);
		}

		public void IAutoRatingOrganisations_Carrier()
		{
			var client = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("Warehouse");
			var serviceArea = Helper.CreateServiceAreaForVASOrder(whs);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, client);
			var autoRating = (IAutoRatingOrganisations)new WhsVASOrderRatingAdapter(vasOrder);

			AssertNull(autoRating.Carrier);
		}

		public void IAutoRatingOrganisations_TransportProviders()
		{
			var client = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("Warehouse");
			var serviceArea = Helper.CreateServiceAreaForVASOrder(whs);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, client);
			var autoRating = (IAutoRatingOrganisations)new WhsVASOrderRatingAdapter(vasOrder);

			AssertNull(autoRating.Creditors);
		}

		public void IAutoRatingOrganisations_ImportBroker()
		{
			var client = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("Warehouse");
			var serviceArea = Helper.CreateServiceAreaForVASOrder(whs);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, client);
			var autoRating = (IAutoRatingOrganisations)new WhsVASOrderRatingAdapter(vasOrder);

			AssertNull(autoRating.ImportBroker);
		}

		public void IAutoRatingOrganisations_JobDatesProvider()
		{
			var client = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("Warehouse");
			var serviceArea = Helper.CreateServiceAreaForVASOrder(whs);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, client);
			var autoRating = (IAutoRatingOrganisations)new WhsVASOrderRatingAdapter(vasOrder);

			AssertType<WhsAdHocServiceJobJobDatesProvider>(((IAutoRating)autoRating).JobDatesProvider);
		}

		public void IAutoRatingOrganisations_ExportBroker()
		{
			var client = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("Warehouse");
			var serviceArea = Helper.CreateServiceAreaForVASOrder(whs);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, client);
			var autoRating = (IAutoRatingOrganisations)new WhsVASOrderRatingAdapter(vasOrder);

			AssertNull(autoRating.ExportBroker);
		}

		#endregion

		#region IAutoRatingWarehouseInfo Members

		public void TestIAutoRatingWarehouseInfo_WarehousePK()
		{
			var client = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("Warehouse");
			var serviceArea = Helper.CreateServiceAreaForVASOrder(whs);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, client);
			var autoRating = new WhsVASOrderRatingAdapter(vasOrder);

			AssertEquals(vasOrder.WarehousePK, ((IAutoRatingWarehouseInfo)autoRating).WarehousePK);
		}

		public void TestIAutoRatingWarehouseInfo_WarehouseFallbackConsignorForFilterOnly()
		{
			var client = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("Warehouse");
			var serviceArea = Helper.CreateServiceAreaForVASOrder(whs);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, client);
			var autoRating = new WhsVASOrderRatingAdapter(vasOrder);

			AssertEquals(null, ((IAutoRatingWarehouseInfo)autoRating).WarehouseFallbackConsignorForFilterOnly);
		}

		#endregion
	}
}
