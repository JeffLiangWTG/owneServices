using System;
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
	public class WhsAdHocServiceJobRatingAdapterTest : WhsTestCaseWithFactory
	{
		#region IAutoRating Members

		#region TestIAutoRatingJobServices

		public void TestIAutoRatingJobServices()
		{
			var autoRating = (IAutoRating)new WhsAdHocServiceJobRatingAdapter(AdHocServiceJob);

			var servicesWrapper = autoRating.JobServices;
			var allServiceTypes = AdHocServiceJob.Services.AddNew().Lookups.JobServiceType_List;

			foreach (CodeDescriptionPair serviceType in allServiceTypes)
			{
				AssertEquals(true, servicesWrapper.Contains(ChargeCodeGroupList.Codes.WHSAdHocServiceJob, serviceType.Code));
			}

			AssertNotEquals("List should not be cached.", autoRating.JobServices, servicesWrapper);
			AssertEquals(allServiceTypes.Count, servicesWrapper.Count);

			AdHocServiceJob.Delete();
		}

		#endregion

		#region TestIAutoRatingChargeCodeGroups

		public void TestIAutoRatingChargeCodeGroups()
		{
			var autoRating = new WhsAdHocServiceJobRatingAdapter(AdHocServiceJob);

			var ccList = autoRating.ChargeCodeGroups;
			AssertNotNull(ccList);
			// assert cached
			AssertEquals(ccList, autoRating.ChargeCodeGroups);

			AdHocServiceJob.Delete();
		}

		#endregion

		#region TestIAutoRatingPopulateChargeCodeGroups

		public void TestIAutoRatingPopulateChargeCodeGroups()
		{
			var autoRating = new WhsAdHocServiceJobRatingAdapter(AdHocServiceJob);
			AssertEquals(1, autoRating.ChargeCodeGroups.Count);
			AssertCollectionContains(ChargeCodeGroupList.Codes.WHSAdHocServiceJob, autoRating.ChargeCodeGroups);

			AdHocServiceJob.Delete();
		}

		#endregion

		#region TestIAutoRatingRateTypeToUse
		public void TestIAutoRatingRateTypeToUse()
		{
			var autoRating = (IAutoRating)new WhsAdHocServiceJobRatingAdapter(AdHocServiceJob);
			AssertEquals(RateType.Warehouse, autoRating.RateTypeToUse);

			AdHocServiceJob.Delete();
		}

		#endregion

		#region TestIAutoRatingStatusInformation

		public void TestIAutoRatingStatusInformation()
		{
			var autoRating = (IAutoRating)new WhsAdHocServiceJobRatingAdapter(AdHocServiceJob);
			var info = autoRating.StatusInformation;

			AssertNotNull(info);
			AssertEquals(true, info.CanExecute);
			AssertEquals(ZString.Empty, info.Message);

			AdHocServiceJob.Delete();
		}

		#endregion

		#region TestIAutoRatingConsumerType

		public void TestIAutoRatingConsumerType()
		{
			var autoRating = (IAutoRating)new WhsAdHocServiceJobRatingAdapter(AdHocServiceJob);
			AssertEquals(JobInvoicingConsumerTypes.WarehouseAdHocServiceJob, autoRating.InvoicingSupporter.ConsumerType);

			AdHocServiceJob.Delete();
		}

		#endregion

		#region TestIAutoRatingMergeCharges

		public void TestIAutoRatingMergeCharges()
		{
			var autoRating = (IAutoRating)new WhsAdHocServiceJobRatingAdapter(AdHocServiceJob);
			AssertEquals(MergeChargeOptions.WithinAdapter, autoRating.MergeCharges);

			adHocServiceJob.Delete();
		}

		#endregion

		#region TestAdapterTypeAndID

		public void TestAdapterTypeAndID()
		{
			var adapter = (IAutoRating)new WhsAdHocServiceJobRatingAdapter(AdHocServiceJob);
			AssertEquals(AdapterType.WarehouseAdHocService, adapter.AdapterType);
			AssertEquals(AdHocServiceJob.WSJ_JobNumber, adapter.OperationalJobCode);

			AdHocServiceJob.Delete();
		}

		#endregion

		#endregion

		#region IAutoRatingFreightInfo Members

		#region TestIAutoRatingFreightInfo_FreightMode

		public void TestIAutoRatingFreightInfo_FreightMode()
		{
			var autoRating = (IAutoRatingFreightInfo)new WhsAdHocServiceJobRatingAdapter(AdHocServiceJob);
			AssertEquals(FreightMode.UKN, autoRating.FreightMode);

			AdHocServiceJob.Delete();
		}

		#endregion

		#region TestIAutoRatingFreightInfo_HouseBillReleaseType

		public void TestIAutoRatingFreightInfo_HouseBillReleaseType()
		{
			var autoRating = (IAutoRatingFreightInfo)new WhsAdHocServiceJobRatingAdapter(AdHocServiceJob);
			AssertEquals(ZString.Empty, autoRating.HousebillReleaseType);

			AdHocServiceJob.Delete();
		}

		#endregion

		#region TestIAutoRatingFreightInfo_Time

		public void TestIAutoRatingFreightInfo_Time()
		{
			var autoRating = new WhsAdHocServiceJobRatingAdapter(AdHocServiceJob);
			var timeInfo = ((RateableMeasureSet)autoRating.RateableMeasures).Time;
			AssertEquals(autoRating.JobDatesProvider.GetJobDateByType(JobDateTypes.Codes.ArrivalDate).Day, timeInfo.Span.Days);
			AssertEquals(0, timeInfo.Span.Minutes);
			AssertEquals(0, timeInfo.Span.Hours);

			AdHocServiceJob.Delete();
		}

		public void TestIAutoRatingFreightInfo_SpecialServiceTime()
		{
			var autoRating = new WhsAdHocServiceJobRatingAdapter(AdHocServiceJob);
			var service = AdHocServiceJob.Services.AddNew();
			service.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Fumigation;
			service.ES_Completed = ZDateTime.Today;
			service.ES_Duration = new DateTime(DateTime.Now.Year, 1, 1).AddHours(2);

			var chargeCodeFilter = new ZQuery(
				new ZQuery(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK),
				new ZQuery(AccChargeCodeSchema.AC_Code, "DFUMI"));

			var chargeCode = Factory.Load<AccChargeCode>(chargeCodeFilter).FirstOrDefault();
			chargeCode.AC_ChargeGroup = autoRating.ChargeCodeGroups[0];

			var timeInfo = autoRating.JobServices.Time(chargeCode);
			AssertEquals(2d, timeInfo.Span.TotalHours);

			AdHocServiceJob.Delete();
		}

		#endregion

		#region TestIAutoRatingFreightInfo_Measures

		public void TestIAutoRatingFreightInfo_Measures()
		{
			var autoRating = new WhsAdHocServiceJobRatingAdapter(AdHocServiceJob);
			var rateableMeasures = (RateableMeasureSet)autoRating.RateableMeasures;
			AssertEquals("Without measure for services, the Invoice Detail report will not group the charges correctly.", "SV", rateableMeasures.GetUnit(MeasureType.Unidentified));

			AdHocServiceJob.Delete();
		}

		#endregion

		#region TestIAutoRatingFreightInfo_ServiceLevel

		public void TestIAutoRatingFreightInfo_ServiceLevel()
		{
			var autoRating = (IAutoRating)new WhsAdHocServiceJobRatingAdapter(AdHocServiceJob);
			AssertNotNull(autoRating.ServiceLevel);

			adHocServiceJob.Delete();
		}

		#endregion

		#region TestIAutoRatingFreightInfo_WharfCTOAddress

		public void TestIAutoRatingFreightInfo_WharfCTOAddress()
		{
			var autoRating = (IAutoRatingFreightInfo)new WhsAdHocServiceJobRatingAdapter(AdHocServiceJob);
			AssertEquals(AdHocServiceJob.Warehouse.WW_OA_WarehouseAddress, autoRating.WharfCTOAddress.PK);

			AdHocServiceJob.Delete();
		}

		#endregion

		#endregion

		#region IAutoRatingLocations Members

		#region TestIAutoRatingLocationsOrigin

		public void TestIAutoRatingLocationsOrigin()
		{
			var autoRating = (IAutoRatingLocations)new WhsAdHocServiceJobRatingAdapter(AdHocServiceJob);
			AssertNull(autoRating.Origin);

			AdHocServiceJob.Delete();
		}

		#endregion

		#region TestIAutoRatingLocationsDestination

		public void TestIAutoRatingLocationsDestination()
		{
			var autoRating = (IAutoRatingLocations)new WhsAdHocServiceJobRatingAdapter(AdHocServiceJob);
			AssertNull(autoRating.Destination);

			AdHocServiceJob.Delete();
		}

		#endregion

		#region TestIAutoRatingLocationsVia

		public void TestIAutoRatingLocationsVia()
		{
			var autoRating = (IAutoRatingLocations)new WhsAdHocServiceJobRatingAdapter(AdHocServiceJob);
			AssertNull(autoRating.GetVia(CostSell.Cost));
			AssertNull(autoRating.GetVia(CostSell.Revenue));

			AdHocServiceJob.Delete();
		}

		#endregion

		#endregion

		#region IAutoRatingOrganisations Members

		public void IAutoRatingOrganisations_Consignor()
		{
			var autoRating = (IAutoRating)new WhsAdHocServiceJobRatingAdapter(AdHocServiceJob);
			AssertEquals(null, autoRating.DebtorOrgs[Registry.Business.RatingDebtorOrgTypes.CNR]);
		}

		public void IAutoRatingOrganisations_Consignee()
		{
			var autoRating = (IAutoRating)new WhsAdHocServiceJobRatingAdapter(AdHocServiceJob);
			AssertEquals(null, autoRating.DebtorOrgs[Registry.Business.RatingDebtorOrgTypes.CNE]);
		}

		public void IAutoRatingOrganisations_PickupCartageEquipment()
		{
			var autoRating = (IAutoRatingOrganisations)new WhsAdHocServiceJobRatingAdapter(AdHocServiceJob);
			AssertEquals(ZString.Empty, autoRating.PickupCartageEquipment);
		}

		public void IAutoRatingOrganisations_PickupAddress()
		{
			var autoRating = (IAutoRatingOrganisations)new WhsAdHocServiceJobRatingAdapter(AdHocServiceJob);
			AssertEquals(null, autoRating.PickupAddress);
		}

		public void IAutoRatingOrganisations_DeliveryAddress()
		{
			var autoRating = (IAutoRatingOrganisations)new WhsAdHocServiceJobRatingAdapter(AdHocServiceJob);
			AssertEquals(null, autoRating.DeliveryAddress);
		}

		public void IAutoRatingOrganisations_DeliveryCartageEquipment()
		{
			var autoRating = (IAutoRatingOrganisations)new WhsAdHocServiceJobRatingAdapter(AdHocServiceJob);
			AssertEquals(ZString.Empty, autoRating.DeliveryCartageEquipment);
		}

		public void IAutoRatingOrganisations_Carrier()
		{
			var autoRating = (IAutoRatingOrganisations)new WhsAdHocServiceJobRatingAdapter(AdHocServiceJob);
			AssertEquals(null, autoRating.Carrier);
		}

		public void IAutoRatingOrganisations_TransportProviders()
		{
			var autoRating = (IAutoRatingOrganisations)new WhsAdHocServiceJobRatingAdapter(AdHocServiceJob);
			AssertEquals(null, autoRating.Creditors);
		}

		public void IAutoRatingOrganisations_ImportBroker()
		{
			var autoRating = (IAutoRatingOrganisations)new WhsAdHocServiceJobRatingAdapter(AdHocServiceJob);
			AssertNull(autoRating.ImportBroker);
		}

		public void IAutoRatingOrganisations_JobDatesProvider()
		{
			var autoRating = (IAutoRatingOrganisations)new WhsAdHocServiceJobRatingAdapter(AdHocServiceJob);
			AssertType<WhsAdHocServiceJobJobDatesProvider>(((IAutoRating)autoRating).JobDatesProvider);
		}

		public void IAutoRatingOrganisations_ExportBroker()
		{
			var autoRating = (IAutoRatingOrganisations)new WhsAdHocServiceJobRatingAdapter(AdHocServiceJob);
			AssertNull(autoRating.ExportBroker);
		}

		#endregion

		#region IAutoRatingWarehouseInfo Members

		public void TestIAutoRatingWarehouseInfo_WarehousePK()
		{
			var autoRating = new WhsAdHocServiceJobRatingAdapter(AdHocServiceJob);
			AssertEquals(AdHocServiceJob.WSJ_WW_Whs, ((IAutoRatingWarehouseInfo)autoRating).WarehousePK);

			AdHocServiceJob.Delete();
		}

		public void TestIAutoRatingWarehouseInfo_WarehouseFallbackConsignorForFilterOnly()
		{
			var autoRating = new WhsAdHocServiceJobRatingAdapter(AdHocServiceJob);
			AssertEquals(null, ((IAutoRatingWarehouseInfo)autoRating).WarehouseFallbackConsignorForFilterOnly);

			AdHocServiceJob.Delete();
		}

		#endregion

		#region Implementation

		WhsAdHocServiceJob AdHocServiceJob
		{
			get
			{
				if (adHocServiceJob == null)
				{
					var data = new TestDataSimpleEnvironment(Factory);
					adHocServiceJob = Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org1, ZDateTime.Today);
				}
				return adHocServiceJob;
			}
		}

		WhsAdHocServiceJob adHocServiceJob;

		#endregion

	}
}
