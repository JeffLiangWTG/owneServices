using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	abstract class JobConfigurationLookupsExtensionsTest<T> : TestCaseWithFactory where T : IJobConfiguration
	{
		public void TestGetJobTypeList()
		{
			AssertOptionsAsExpected(CreateBusinessObject(), c => JobConfigurationLookupsExtensions.GetJobTypeList(), new[]
			{
				// Put in ALPHABETICAL ORDER by CODE
				(JobInvoicingConsumerTypes.AgentBookingCode, "Standalone Transport Booking Booked via CBA (obsolete)"),
				(JobInvoicingConsumerTypes.AgencyDetentionInvoiceCode, "Liner & Agency Detention Invoice"),
				(JobInvoicingConsumerTypes.CusMAWBCode, "Air Cargo"),
				(JobInvoicingConsumerTypes.AgencyBookingCode, "Liner & Agency Booking"),
				(JobInvoicingConsumerTypes.AgencyBillOfLadingCode, "Liner & Agency Bill Of Lading"),
				(JobInvoicingConsumerTypes.CTOCusExportHAWBCode, "Air Cargo CTO Export HAWB"),
				(JobInvoicingConsumerTypes.CTOCusImportHAWBCode, "Air Cargo CTO Import HAWB"),
				(JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All, "Any Job Type"),
				(JobInvoicingConsumerTypes.AgencySundryChargesCode, "Liner & Agency Sundry Charges"),
				(JobInvoicingConsumerTypes.TransportBookingWithAgentCode, "Standalone Transport Booking Booked via CBA"),
				(JobInvoicingConsumerTypes.AgencyVoyageAccountingCode, "Liner & Agency Voyage Accounting"),
				(JobInvoicingConsumerTypes.MasterAWBCode, "Master Air Waybill"),
				(JobInvoicingConsumerTypes.BrokerageCode, "Declaration Job"),
				(JobInvoicingConsumerTypes.CAeManifestCode,  "e-Manifest Forwarder (CA)"),
				(JobInvoicingConsumerTypes.CFSLoadListCode, "CFS Load List"),
				(JobInvoicingConsumerTypes.CFSShipmentCode, "CFS Shipment"),
				(JobInvoicingConsumerTypes.FCLStorageCode, "FCL Container Storage"),
				(JobInvoicingConsumerTypes.CTOCusMAWBCode, "Air Cargo CTO"),
				(JobInvoicingConsumerTypes.ForwardingConsolCode, "Consol"),
				(JobInvoicingConsumerTypes.GatewayConsolCode, "Gateway Consol"),
				(JobInvoicingConsumerTypes.ImporterSecurityFilingCode, "Importer Security Filing"),
				(JobInvoicingConsumerTypes.BRLPCOCode, "BR LPCO"),
				(JobInvoicingConsumerTypes.TransportConsignmentCode, "Land Transport Consignment"),
				(JobInvoicingConsumerTypes.eManifestCode,  "e-Manifest"),
				(JobInvoicingConsumerTypes.MNRWorkOrderHeaderJobCode, "Container Yard Work Order"),
				(JobInvoicingConsumerTypes.CustomsTransitNCTSCode, "Customs Transit (NCTS)"),
				(JobInvoicingConsumerTypes.PostClearanceBrokerageCode, "Post Clearance Declaration Job"),
				(JobInvoicingConsumerTypes.QuotedBookingCode, "Quick Booking"),
				(JobInvoicingConsumerTypes.ShipmentCode, "Shipment"),
				(JobInvoicingConsumerTypes.CustomsTemporaryStorageCode, "Customs Temporary Storage"),
				(JobInvoicingConsumerTypes.TransportBookingCode, "Standalone Transport Booking"),
				(JobInvoicingConsumerTypes.TransportBookingConsignmentCode, "Transport Booking Consignment"),
				(JobInvoicingConsumerTypes.TransitDispatchCode, "Transit Dispatch"),
				(JobInvoicingConsumerTypes.TransitDispatchLoadListCode, "Transit Dispatch Load List"),
				(JobInvoicingConsumerTypes.TransitDispatchTransportationUnitCode, "Transit Dispatch Transportation Unit"),
				(JobInvoicingConsumerTypes.TransitReceiveCode, "Transit Receive"),
				(JobInvoicingConsumerTypes.LocalCartageCode, "Port Transport"),
				(JobInvoicingConsumerTypes.TransitReceiveTransportationUnitCode, "Transit Receive Transportation Unit"),
				(JobInvoicingConsumerTypes.CusUnderbondCode, "Air Cargo Outturn"),
				(JobInvoicingConsumerTypes.WarehouseInwardsCode, "Warehouse Receive"),
				(JobInvoicingConsumerTypes.WorkItemCode, "Work Item"),
				(JobInvoicingConsumerTypes.ProjectCode, "Project"),
				(JobInvoicingConsumerTypes.WorkRequestCode, "Customer Service Ticket"),
				(JobInvoicingConsumerTypes.WarehouseOutwardsCode, "Warehouse Release"),
				(JobInvoicingConsumerTypes.WarehouseStocktakeCode, "Warehouse Stocktake"),
				(JobInvoicingConsumerTypes.WarehouseAdHocServiceJobCode, "Warehouse Ad Hoc Service Job"),
				(JobInvoicingConsumerTypes.WarehouseStorageCode, "Warehouse Periodic"),
				(JobInvoicingConsumerTypes.WarehouseVASOrderCode, "Warehouse VAS Order"),
				(JobInvoicingConsumerTypes.CYDAdHocServiceOrderJobCode, "Container Yard Service Order"),
				(JobInvoicingConsumerTypes.CYDPeriodicInvoicingJobCode, "Container Yard Periodic Invoicing"),
				(JobInvoicingConsumerTypes.CYDReceiveAdviceJobCode, "Container Yard Pre-Arrival Instruction"),
				(JobInvoicingConsumerTypes.CYDReleaseAdviceJobCode, "Container Yard Release Order"),
				(JobInvoicingConsumerTypes.CYDTransportationUnitJobCode, "Container Yard Transportation Unit"),
				// Put in ALPHABETICAL ORDER by CODE
			});
		}

		public void TestGetDirectionList()
		{
			var bo = CreateBusinessObject();

			var jobTypesCase1 = new[]
			{
				JobInvoicingConsumerTypes.Shipment.Code,
				JobInvoicingConsumerTypes.GatewayConsol.Code,
				JobInvoicingConsumerTypes.CFSShipment.Code,
				JobInvoicingConsumerTypes.CFSLoadList.Code,
				JobInvoicingConsumerTypes.FCLStorage.Code,
				JobInvoicingConsumerTypes.QuotedBooking.Code,
				JobInvoicingConsumerTypes.ForwardingConsol.Code,
				JobInvoicingConsumerTypes.AgencyBillOfLading.Code,
			};

			foreach (var jobType in jobTypesCase1)
			{
				SetJobType(bo, jobType);
				AssertOptionsAsExpected(bo, c => c.GetDirectionList(), new[]
				{
					(Constants.FreightShipmentDirection.Code.All, Constants.FreightShipmentDirection.Description.All.ToString()),
					(Constants.FreightShipmentDirection.Code.Import, Constants.FreightShipmentDirection.Description.Import.ToString()),
					(Constants.FreightShipmentDirection.Code.Export, Constants.FreightShipmentDirection.Description.Export.ToString()),
					(Constants.FreightShipmentDirection.Code.Domestic, Constants.FreightShipmentDirection.Description.Domestic.ToString()),
					(Constants.FreightShipmentDirection.Code.Other, Constants.FreightShipmentDirection.Description.Other.ToString()),
				});
			}

			var jobTypesCase2 = new[]
			{
				JobInvoicingConsumerTypes.LocalCartage.Code,
			};

			foreach (var jobType in jobTypesCase2)
			{
				SetJobType(bo, jobType);
				AssertOptionsAsExpected(bo, c => c.GetDirectionList(), new[]
				{
					(Constants.FreightShipmentDirection.Code.All, Constants.FreightShipmentDirection.Description.All.ToString()),
					(Constants.FreightShipmentDirection.Code.Import, Constants.FreightShipmentDirection.Description.Import.ToString()),
					(Constants.FreightShipmentDirection.Code.Export, Constants.FreightShipmentDirection.Description.Export.ToString()),
					(Constants.CartageDirection.Destination, Constants.CartageDirectionDescription.Destination.ToString()),
					(Constants.CartageDirection.LineHaul, Constants.CartageDirectionDescription.LineHaul.ToString()),
					(Constants.CartageDirection.Origin, Constants.CartageDirectionDescription.Origin.ToString()),
					(Constants.CartageDirection.Local, Constants.CartageDirectionDescription.Local.ToString()),
				});
			}

			var otherJobTypes = JobConfigurationLookupsExtensions.GetJobTypeList()
				.Cast<CodeDescriptionPair>().Select(x => x.Code)
				.Except(jobTypesCase1)
				.Except(jobTypesCase2)
				.Except(new string[] { JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All });

			foreach (var jobType in otherJobTypes)
			{
				SetJobType(bo, jobType);
				AssertOptionsAsExpected(bo, c => c.GetDirectionList(), new[]
				{
					(Constants.FreightShipmentDirection.Code.All, Constants.FreightShipmentDirection.Description.All.ToString()),
				});
			}
		}

		public void TestGetDirectionListForAllJobTypes()
		{
			var bo = CreateBusinessObject();
			SetJobType(bo, JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All);

			if (bo.IncludeOptionsForAllJobTypes)
			{
				AssertOptionsAsExpected(bo, c => c.GetDirectionList(), new[] {
					(Constants.FreightShipmentDirection.Code.All, Constants.FreightShipmentDirection.Description.All.ToString()),
					(Constants.FreightShipmentDirection.Code.Import, Constants.FreightShipmentDirection.Description.Import.ToString()),
					(Constants.FreightShipmentDirection.Code.Export, Constants.FreightShipmentDirection.Description.Export.ToString()),
					(Constants.FreightShipmentDirection.Code.Domestic, Constants.FreightShipmentDirection.Description.Domestic.ToString()),
					(Constants.FreightShipmentDirection.Code.Other, Constants.FreightShipmentDirection.Description.Other.ToString())
				});
			}

			if (!bo.IncludeOptionsForAllJobTypes)
			{
				AssertOptionsAsExpected(bo, c => c.GetDirectionList(), new[]
				{
					(Constants.FreightShipmentDirection.Code.All, Constants.FreightShipmentDirection.Description.All.ToString())
				});
			}
		}

		public void TestGetModeList()
		{
			var bo = CreateBusinessObject();
			var jobTypesCase1 = new[]
			{
				JobInvoicingConsumerTypes.Shipment.Code,
				JobInvoicingConsumerTypes.ForwardingConsol.Code,
				JobInvoicingConsumerTypes.GatewayConsol.Code,
			};

			foreach (var jobType in jobTypesCase1)
			{
				SetJobType(bo, jobType);
				AssertOptionsAsExpected(bo, c => c.GetTransportModeList(), new[]
				{
					(JobConfigurationSelectorLookups.ModeAdditionalCodes.All, Constants.TransportModeDescriptions.All),
					(Constants.TransportModes.Air, Constants.TransportModeDescriptions.Air.ToString()),
					(Constants.TransportModes.Sea, Constants.TransportModeDescriptions.Sea.ToString()),
					(Constants.TransportModes.SeaAir, Constants.TransportModeDescriptions.SeaAir.ToString()),
					(Constants.TransportModes.AirSea, Constants.TransportModeDescriptions.AirSea.ToString()),
					(Constants.TransportModes.Road, Constants.TransportModeDescriptions.Road.ToString()),
					(Constants.TransportModes.Rail, Constants.TransportModeDescriptions.Rail.ToString()),
					(Constants.TransportModes.Courier, Constants.TransportModeDescriptions.Courier.ToString()),
				});
			}

			var jobTypesCase2 = new[]
			{
				JobInvoicingConsumerTypes.CFSLoadList.Code,
				JobInvoicingConsumerTypes.FCLStorage.Code,
				JobInvoicingConsumerTypes.CFSShipment.Code,
				JobInvoicingConsumerTypes.LocalCartage.Code,
			};

			foreach (var jobType in jobTypesCase2)
			{
				SetJobType(bo, jobType);
				AssertOptionsAsExpected(bo, c => c.GetTransportModeList(), new[]
				{
					(JobConfigurationSelectorLookups.ModeAdditionalCodes.All, Constants.TransportModeDescriptions.All.ToString()),
					(Constants.TransportModes.Air, Constants.TransportModeDescriptions.Air.ToString()),
					(Constants.TransportModes.Sea, Constants.TransportModeDescriptions.Sea.ToString()),
					(Constants.TransportModes.Road, Constants.TransportModeDescriptions.Road.ToString()),
					(Constants.TransportModes.Rail, Constants.TransportModeDescriptions.Rail.ToString()),
				});
			}

			var jobTypesCase3 = new[]
			{
				JobInvoicingConsumerTypes.QuotedBooking.Code,
			};

			foreach (var jobType in jobTypesCase3)
			{
				SetJobType(bo, jobType);
				AssertOptionsAsExpected(bo, c => c.GetTransportModeList(), new[]
				{
					(JobConfigurationSelectorLookups.ModeAdditionalCodes.All, Constants.TransportModeDescriptions.All.ToString()),
					(Constants.TransportModes.Air, Constants.TransportModeDescriptions.Air.ToString()),
					(Constants.TransportModes.Sea, Constants.TransportModeDescriptions.Sea.ToString()),
					(Constants.TransportModes.Road, Constants.TransportModeDescriptions.Road.ToString()),
					(Constants.TransportModes.Rail, Constants.TransportModeDescriptions.Rail.ToString()),
					(Constants.TransportModes.Courier, Constants.TransportModeDescriptions.Courier.ToString()),
				});
			}

			var otherJobTypes = JobConfigurationLookupsExtensions.GetJobTypeList()
				.Cast<CodeDescriptionPair>().Select(x => x.Code)
				.Except(jobTypesCase1)
				.Except(jobTypesCase2)
				.Except(jobTypesCase3)
				.Except(new string[] { JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All });

			foreach (var jobType in otherJobTypes)
			{
				SetJobType(bo, jobType);
				AssertOptionsAsExpected(bo, c => c.GetTransportModeList(), new[]
				{
					(JobConfigurationSelectorLookups.ModeAdditionalCodes.All, Constants.TransportModeDescriptions.All.ToString())
				});
			}
		}

		public void TestGetTransportModeListForAllJobTypes()
		{
			var bo = CreateBusinessObject();
			SetJobType(bo, JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All);

			if (bo.IncludeOptionsForAllJobTypes)
			{
				AssertOptionsAsExpected(bo, c => c.GetTransportModeList(), new[] {
					(JobConfigurationSelectorLookups.ModeAdditionalCodes.All, Constants.TransportModeDescriptions.All.ToString()),
					(Constants.TransportModes.Air, Constants.TransportModeDescriptions.Air.ToString()),
					(Constants.TransportModes.Sea, Constants.TransportModeDescriptions.Sea.ToString()),
					(Constants.TransportModes.Road, Constants.TransportModeDescriptions.Road.ToString()),
					(Constants.TransportModes.Rail, Constants.TransportModeDescriptions.Rail.ToString()),
					(Constants.TransportModes.Courier, Constants.TransportModeDescriptions.Courier.ToString())
				});
			}

			if (!bo.IncludeOptionsForAllJobTypes)
			{
				AssertOptionsAsExpected(bo, c => c.GetTransportModeList(), new[] {
					(JobConfigurationSelectorLookups.ModeAdditionalCodes.All, Constants.TransportModeDescriptions.All.ToString())
				});
			}
		}

		public void TestPreferenceList()
		{
			var expectedPreferencesByJobTypes = new Dictionary<string, (string, string)[]>()
			{
				[JobInvoicingConsumerTypes.Shipment.Code] = new (string, string)[]
				{
					(Constants.JobBillingExchangeRatePreference.Code.TodaysRate, Constants.JobBillingExchangeRatePreference.Description.TodaysRate),
					(Constants.JobBillingExchangeRatePreference.Code.ConsolExchangeRate, Constants.JobBillingExchangeRatePreference.Description.ConsolExchangeRate),
					(Constants.JobBillingExchangeRatePreference.Code.HistoricalRateFromActualArrivalDate, Constants.JobBillingExchangeRatePreference.Description.HistoricalRateFromActualArrivalDate),
					(Constants.JobBillingExchangeRatePreference.Code.HistoricalRateFromActualDepartureDate, Constants.JobBillingExchangeRatePreference.Description.HistoricalRateFromActualDepartureDate),
					(Constants.JobBillingExchangeRatePreference.Code.HistoricalRateFromActualArrivalAtLoadPortDate, Constants.JobBillingExchangeRatePreference.Description.HistoricalRateFromActualArrivalAtLoadPortDate),
					(Constants.JobBillingExchangeRatePreference.Code.HistoricalRateFromEstimatedArrivalDate, Constants.JobBillingExchangeRatePreference.Description.HistoricalRateFromEstimatedArrivalDate),
					(Constants.JobBillingExchangeRatePreference.Code.HistoricalRateFromEstimatedDepartureDate, Constants.JobBillingExchangeRatePreference.Description.HistoricalRateFromEstimatedDepartureDate),
					(Constants.JobBillingExchangeRatePreference.Code.HistoricalRateFromEstimatedArrivalAtLoadPortDate, Constants.JobBillingExchangeRatePreference.Description.HistoricalRateFromEstimatedArrivalAtLoadPortDate),
					(Constants.JobBillingExchangeRatePreference.Code.HouseBillIssueDate, Constants.JobBillingExchangeRatePreference.Description.HouseBillIssueDate),
					(Constants.JobBillingExchangeRatePreference.Code.ShipmentOrBrokeragePickupDate, Constants.JobBillingExchangeRatePreference.Description.ShipmentOrBrokeragePickupDate),
					(Constants.JobBillingExchangeRatePreference.Code.ShipmentOrBrokerageDeliveryDate, Constants.JobBillingExchangeRatePreference.Description.ShipmentOrBrokerageDeliveryDate),
				},
				[JobInvoicingConsumerTypes.Brokerage.Code] = new (string, string)[]
				{
					(Constants.JobBillingExchangeRatePreference.Code.TodaysRate, Constants.JobBillingExchangeRatePreference.Description.TodaysRate),
					(Constants.JobBillingExchangeRatePreference.Code.ShipmentOrBrokeragePickupDate, Constants.JobBillingExchangeRatePreference.Description.ShipmentOrBrokeragePickupDate),
					(Constants.JobBillingExchangeRatePreference.Code.ShipmentOrBrokerageDeliveryDate, Constants.JobBillingExchangeRatePreference.Description.ShipmentOrBrokerageDeliveryDate),
				},
				[JobInvoicingConsumerTypes.LocalCartage.Code] = new (string, string)[]
				{
					(Constants.JobBillingExchangeRatePreference.Code.TodaysRate, Constants.JobBillingExchangeRatePreference.Description.TodaysRate),
					(Constants.JobBillingExchangeRatePreference.Code.PickupDate, Constants.JobBillingExchangeRatePreference.Description.PickupDate),
					(Constants.JobBillingExchangeRatePreference.Code.DeliveryDate, Constants.JobBillingExchangeRatePreference.Description.DeliveryDate)
				},
				[JobInvoicingConsumerTypes.QuotedBooking.Code] = new (string, string)[]
				{
					(Constants.JobBillingExchangeRatePreference.Code.TodaysRate, Constants.JobBillingExchangeRatePreference.Description.TodaysRate),
					(Constants.JobBillingExchangeRatePreference.Code.HistoricalRateFromActualArrivalDate, Constants.JobBillingExchangeRatePreference.Description.HistoricalRateFromActualArrivalDate),
					(Constants.JobBillingExchangeRatePreference.Code.HistoricalRateFromActualDepartureDate, Constants.JobBillingExchangeRatePreference.Description.HistoricalRateFromActualDepartureDate),
					(Constants.JobBillingExchangeRatePreference.Code.HistoricalRateFromEstimatedArrivalDate, Constants.JobBillingExchangeRatePreference.Description.HistoricalRateFromEstimatedArrivalDate),
					(Constants.JobBillingExchangeRatePreference.Code.HistoricalRateFromEstimatedDepartureDate, Constants.JobBillingExchangeRatePreference.Description.HistoricalRateFromEstimatedDepartureDate),
				},
				[JobInvoicingConsumerTypes.GatewayConsol.Code] = new (string, string)[]
				{
					(Constants.JobBillingExchangeRatePreference.Code.TodaysRate, Constants.JobBillingExchangeRatePreference.Description.TodaysRate),
					(Constants.JobBillingExchangeRatePreference.Code.HistoricalRateFromActualArrivalDate, Constants.JobBillingExchangeRatePreference.Description.HistoricalRateFromActualArrivalDate),
					(Constants.JobBillingExchangeRatePreference.Code.HistoricalRateFromActualDepartureDate, Constants.JobBillingExchangeRatePreference.Description.HistoricalRateFromActualDepartureDate),
					(Constants.JobBillingExchangeRatePreference.Code.HistoricalRateFromEstimatedArrivalDate, Constants.JobBillingExchangeRatePreference.Description.HistoricalRateFromEstimatedArrivalDate),
					(Constants.JobBillingExchangeRatePreference.Code.HistoricalRateFromEstimatedDepartureDate, Constants.JobBillingExchangeRatePreference.Description.HistoricalRateFromEstimatedDepartureDate),
				},
				[JobInvoicingConsumerTypes.AgencyBillOfLading.Code] = new (string, string)[]
				{
					(Constants.JobBillingExchangeRatePreference.Code.TodaysRate, Constants.JobBillingExchangeRatePreference.Description.TodaysRate),
					(Constants.JobBillingExchangeRatePreference.Code.HistoricalRateFromActualArrivalDate, Constants.JobBillingExchangeRatePreference.Description.HistoricalRateFromActualArrivalDate),
					(Constants.JobBillingExchangeRatePreference.Code.HistoricalRateFromActualDepartureDate, Constants.JobBillingExchangeRatePreference.Description.HistoricalRateFromActualDepartureDate),
					(Constants.JobBillingExchangeRatePreference.Code.HistoricalRateFromEstimatedArrivalDate, Constants.JobBillingExchangeRatePreference.Description.HistoricalRateFromEstimatedArrivalDate),
					(Constants.JobBillingExchangeRatePreference.Code.HistoricalRateFromEstimatedDepartureDate, Constants.JobBillingExchangeRatePreference.Description.HistoricalRateFromEstimatedDepartureDate),
				},
				[JobInvoicingConsumerTypes.AgencyBooking.Code] = new (string, string)[]
				{
					(Constants.JobBillingExchangeRatePreference.Code.TodaysRate, Constants.JobBillingExchangeRatePreference.Description.TodaysRate),
					(Constants.JobBillingExchangeRatePreference.Code.HistoricalRateFromActualArrivalDate, Constants.JobBillingExchangeRatePreference.Description.HistoricalRateFromActualArrivalDate),
					(Constants.JobBillingExchangeRatePreference.Code.HistoricalRateFromActualDepartureDate, Constants.JobBillingExchangeRatePreference.Description.HistoricalRateFromActualDepartureDate),
					(Constants.JobBillingExchangeRatePreference.Code.HistoricalRateFromEstimatedArrivalDate, Constants.JobBillingExchangeRatePreference.Description.HistoricalRateFromEstimatedArrivalDate),
					(Constants.JobBillingExchangeRatePreference.Code.HistoricalRateFromEstimatedDepartureDate, Constants.JobBillingExchangeRatePreference.Description.HistoricalRateFromEstimatedDepartureDate),
				},
				[JobInvoicingConsumerTypes.ForwardingConsol.Code] = new (string, string)[]
				{
					(Constants.JobBillingExchangeRatePreference.Code.TodaysRate, Constants.JobBillingExchangeRatePreference.Description.TodaysRate),
					(Constants.JobBillingExchangeRatePreference.Code.ConsolExchangeRate, Constants.JobBillingExchangeRatePreference.Description.ConsolExchangeRate),
				},
			};

			var bo = CreateBusinessObject();
			foreach (var jobType in expectedPreferencesByJobTypes.Keys)
			{
				SetJobType(bo, jobType);
				AssertOptionsAsExpected(bo, c => c.GetPreferenceList(), expectedPreferencesByJobTypes[jobType]);
			}

			var otherJobTypes = JobConfigurationLookupsExtensions.GetJobTypeList().Cast<CodeDescriptionPair>().Select(x => x.Code).Where(c => !expectedPreferencesByJobTypes.ContainsKey(c));

			foreach (var jobType in otherJobTypes)
			{
				SetJobType(bo, jobType);
				AssertOptionsAsExpected(bo, c => c.GetPreferenceList(), new (string, string)[]
				{
					(Constants.JobBillingExchangeRatePreference.Code.TodaysRate, Constants.JobBillingExchangeRatePreference.Description.TodaysRate),
				});
			}
		}

		public void TestDirectionReadOnly()
		{
			var jobConfiguration = CreateBusinessObject();

			AssertReadonly("SHP", false);
			AssertReadonly("CLL", false);
			AssertReadonly("CSH", false);
			AssertReadonly("FCN", false);
			AssertReadonly("GCN", false);
			AssertReadonly("AGS", false);

			using (NUnit.Framework.TestingState.SuspendIsRunningTests())
			{
				AssertReadonly("!@#", true);
			}

			AssertReadonly("", true);

			AssertReadonly("BRK", true);
			AssertReadonly("AWB", true);

			void AssertReadonly(string jobType, bool expectedResult)
			{
				SetJobType(jobConfiguration, jobType);
				AssertEquals(expectedResult, jobConfiguration.GetDirection_ReadOnly());
			}
		}

		public void TestModeReadOnly()
		{
			var jobConfiguration = CreateBusinessObject();

			AssertReadonly("SHP", false);
			AssertReadonly("CLL", false);
			AssertReadonly("CSH", false);
			AssertReadonly("FCN", false);
			AssertReadonly("GCN", false);

			using (NUnit.Framework.TestingState.SuspendIsRunningTests())
			{
				AssertReadonly("!@#", true);
			}

			AssertReadonly("", true);
			AssertReadonly("BRK", true);
			AssertReadonly("AWB", true);

			void AssertReadonly(string jobType, bool expectedResult)
			{
				SetJobType(jobConfiguration, jobType);
				AssertEquals(expectedResult, jobConfiguration.GetTransportMode_ReadOnly());
			}
		}

		#region Implementation

		void AssertOptionsAsExpected(T jobConfiguration, Func<IJobConfiguration, CodeDescriptionPairList> codePairListGetter, (string, string)[] expectedResults)
		{
			var optionsActual = codePairListGetter(jobConfiguration).Cast<CodeDescriptionPair>().Select(c => (c.Code, c.Description)).ToArray();

			AssertArrayEqualsByElements($" JobType:{jobConfiguration.JobType}", expectedResults, optionsActual);
		}

		protected abstract T CreateBusinessObject();
		protected abstract void SetJobType(T target, string jobType);

		#endregion
	}

	class GeneralJobConfigurationLookupsExtensionsTest : JobConfigurationLookupsExtensionsTest<GeneralJobConfigurationLookupsExtensionsTest.DummyJobConfiguration>
	{
		protected override DummyJobConfiguration CreateBusinessObject() => new DummyJobConfiguration();

		protected override void SetJobType(DummyJobConfiguration target, string jobType)
		{
			target.JobType = jobType;
		}

		internal class DummyJobConfiguration : IJobConfiguration
		{
			public ZString JobType { get; set; }
			public ZString ServiceDirection { get; }
			public ZString TransportMode { get; }
			public bool IncludeOptionsForAllJobTypes => false;
		}
	}

	class DummyJobConfiguration_NotIncludeOptionsForAllJobTypes : JobConfigurationLookupsExtensionsTest<DummyJobConfiguration_NotIncludeOptionsForAllJobTypes.DummyJobConfiguration>
	{
		protected override DummyJobConfiguration CreateBusinessObject() => new DummyJobConfiguration();

		protected override void SetJobType(DummyJobConfiguration target, string jobType)
		{
			target.JobType = jobType;
		}

		internal class DummyJobConfiguration : IJobConfiguration
		{
			public ZString JobType { get; set; }
			public ZString ServiceDirection { get; }
			public ZString TransportMode { get; }
			public bool IncludeOptionsForAllJobTypes => false;
		}
	}
}
