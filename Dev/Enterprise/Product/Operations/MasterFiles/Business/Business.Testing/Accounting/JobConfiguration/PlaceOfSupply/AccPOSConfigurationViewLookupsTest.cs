using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs.Shared;

namespace Enterprise.MasterFiles.Business.Testing.Accounting
{
	sealed class AccPOSConfigurationViewLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestChargeTypeList()
		{
			AssertNotNull(PlaceOfSupplyConfig.Lookups.ChargeTypeList);
			Assert(PlaceOfSupplyConfig.Lookups.ChargeTypeList.ContainsCode(AccPOSChargeTypeList.Codes.CostAndRevenue));
			Assert(PlaceOfSupplyConfig.Lookups.ChargeTypeList.ContainsCode(AccPOSChargeTypeList.Codes.Cost));
			Assert(PlaceOfSupplyConfig.Lookups.ChargeTypeList.ContainsCode(AccPOSChargeTypeList.Codes.Revenue));
			AssertEquals(3, PlaceOfSupplyConfig.Lookups.ChargeTypeList.Count);
		}

		public void TestJobTypeList()
		{
			AssertNotNull(PlaceOfSupplyConfig.Lookups.JobTypeList);
			Assert(PlaceOfSupplyConfig.Lookups.JobTypeList.ContainsCode(JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All));
			Assert(PlaceOfSupplyConfig.Lookups.JobTypeList.ContainsCode(JobInvoicingConsumerTypes.ShipmentCode));
			Assert(PlaceOfSupplyConfig.Lookups.JobTypeList.ContainsCode(JobInvoicingConsumerTypes.ForwardingConsolCode));
			Assert(PlaceOfSupplyConfig.Lookups.JobTypeList.ContainsCode(JobInvoicingConsumerTypes.BrokerageCode));
			Assert(PlaceOfSupplyConfig.Lookups.JobTypeList.ContainsCode(JobInvoicingConsumerTypes.QuotedBookingCode));
			Assert(PlaceOfSupplyConfig.Lookups.JobTypeList.ContainsCode(JobInvoicingConsumerTypes.WorkItemCode));
			AssertEquals(6, PlaceOfSupplyConfig.Lookups.JobTypeList.Count);
		}

		public void TestIncoTermList()
		{
			foreach (var jobType in new[] { JobInvoicingConsumerTypes.ShipmentCode, JobInvoicingConsumerTypes.BrokerageCode, JobInvoicingConsumerTypes.QuotedBookingCode, JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All })
			{
				PlaceOfSupplyConfig.PSC_JobType = jobType;
				AssertNotNull(PlaceOfSupplyConfig.Lookups.IncoTermList);
				AssertEquals("Incoterms provided for Job Type " + jobType, new IncoTermsCodeDescriptionPairList(IncoTermsListType.ActiveIncoTerms).CodesAsString, PlaceOfSupplyConfig.Lookups.IncoTermList.CodesAsString);
			}

			foreach (var notSupportedJobType in new[] { JobInvoicingConsumerTypes.ForwardingConsolCode, JobInvoicingConsumerTypes.WorkItemCode })
			{
				PlaceOfSupplyConfig.PSC_JobType = notSupportedJobType;
				AssertNotNull(PlaceOfSupplyConfig.Lookups.IncoTermList);
				AssertEquals("No Incoterms for Job Type: " + notSupportedJobType, 0, PlaceOfSupplyConfig.Lookups.IncoTermList.Count);
			}
		}

		public void TestServiceDirectionList()
		{
			AssertNotNull(PlaceOfSupplyConfig.Lookups.ServiceDirectionList);
			Assert(PlaceOfSupplyConfig.Lookups.ServiceDirectionList.ContainsCode(Constants.FreightShipmentDirection.Code.All));
			Assert(PlaceOfSupplyConfig.Lookups.ServiceDirectionList.ContainsCode(Constants.FreightShipmentDirection.Code.Import));
			Assert(PlaceOfSupplyConfig.Lookups.ServiceDirectionList.ContainsCode(Constants.FreightShipmentDirection.Code.Export));
			Assert(PlaceOfSupplyConfig.Lookups.ServiceDirectionList.ContainsCode(Constants.FreightShipmentDirection.Code.Domestic));
			Assert(PlaceOfSupplyConfig.Lookups.ServiceDirectionList.ContainsCode(Constants.FreightShipmentDirection.Code.Other));
			AssertEquals(5, PlaceOfSupplyConfig.Lookups.ServiceDirectionList.Count);
		}

		public void TestTransportModeList()
		{
			PlaceOfSupplyConfig.PSC_JobType = JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All;
			assertOLoockUpTransportType();

			PlaceOfSupplyConfig.PSC_JobType = JobInvoicingConsumerTypes.ShipmentCode;
			assertOLoockUpTransportType();

			PlaceOfSupplyConfig.PSC_JobType = JobInvoicingConsumerTypes.ForwardingConsolCode;
			assertOLoockUpTransportType();

			void assertOLoockUpTransportType()
			{
				AssertNotNull(PlaceOfSupplyConfig.Lookups.TransportModeList);
				var transportTypes = new CodeDescriptionPairList(OLookUpEditType.TransportType);
				AssertEquals("ALL, " + transportTypes.CodesAsString, PlaceOfSupplyConfig.Lookups.TransportModeList.CodesAsString);
			}

			PlaceOfSupplyConfig.PSC_JobType = JobInvoicingConsumerTypes.BrokerageCode;
			AssertNotNull(PlaceOfSupplyConfig.Lookups.TransportModeList);
			Assert(PlaceOfSupplyConfig.Lookups.TransportModeList.Count > 1);
			var customsTransportTypes = (Activator.CreateInstance(ObjectFactory.GetType<IDeclarationTransportModeCodeDescriptionPairProvider>()) as ICodeDescriptionPairListProvider).CodeDescriptionPairList;
			AssertEquals("ALL, " + customsTransportTypes.CodesAsString, PlaceOfSupplyConfig.Lookups.TransportModeList.CodesAsString);

			PlaceOfSupplyConfig.PSC_JobType = JobInvoicingConsumerTypes.QuotedBookingCode;
			AssertNotNull(PlaceOfSupplyConfig.Lookups.TransportModeList);
			Assert(PlaceOfSupplyConfig.Lookups.TransportModeList.ContainsCode(Constants.TransportModes.Air));
			Assert(PlaceOfSupplyConfig.Lookups.TransportModeList.ContainsCode(Constants.TransportModes.Sea));
			Assert(PlaceOfSupplyConfig.Lookups.TransportModeList.ContainsCode(Constants.TransportModes.Road));
			Assert(PlaceOfSupplyConfig.Lookups.TransportModeList.ContainsCode(Constants.TransportModes.Rail));
			Assert(PlaceOfSupplyConfig.Lookups.TransportModeList.ContainsCode(Constants.TransportModes.Courier));
			AssertEquals(6, PlaceOfSupplyConfig.Lookups.TransportModeList.Count);
		}

		public void TestTaxRegistrationTypeList()
		{
			AssertNotNull(PlaceOfSupplyConfig.Lookups.TaxRegistrationTypeList);
			Assert(PlaceOfSupplyConfig.Lookups.TaxRegistrationTypeList.ContainsCode(AccPOSTaxRegistrationList.Codes.Unregistered));
			Assert(PlaceOfSupplyConfig.Lookups.TaxRegistrationTypeList.ContainsCode(AccPOSTaxRegistrationList.Codes.ForeignOrganization));
			Assert(PlaceOfSupplyConfig.Lookups.TaxRegistrationTypeList.ContainsCode(AccPOSTaxRegistrationList.Codes.LocalCustomer));
			AssertEquals(3, PlaceOfSupplyConfig.Lookups.TaxRegistrationTypeList.Count);
		}

		public void TestBranchList()
		{
			var defaultBranch = GlbBranch.CurrentBranch;

			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_Code = "CO1";
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_Code = "B01";
			branch1.GB_GC = company1.PK;
			var branch11 = Factory.NewWithValidTestData<GlbBranch>();
			branch11.GB_Code = "B11";
			branch11.GB_GC = company1.PK;

			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.GC_Code = "CO2";
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch2.GB_Code = "B02";
			branch2.GB_GC = company2.PK;
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var config = new BusinessObjectFactory().New<AccPOSConfiguration>();
				config.PSC_ParentTableCode = GlbCompanySchema.Constants.Prefix;
				config.PSC_GC = company1.PK;

				AssertNotNull(config.Lookups.BranchList);
				AssertEquals("The first list item should be 'All Branches'", ZString.Empty, config.Lookups.BranchList[0].Code);
				Assert(config.Lookups.BranchList.ContainsCode(branch1.GB_Code));
				Assert(config.Lookups.BranchList.ContainsCode(branch11.GB_Code));
				Assert(!config.Lookups.BranchList.ContainsCode(defaultBranch.GB_Code));
				AssertEquals(3, config.Lookups.BranchList.Count);
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch2.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var config = new BusinessObjectFactory().New<AccPOSConfiguration>();
				config.PSC_ParentTableCode = GlbCompanySchema.Constants.Prefix;
				config.PSC_GC = company2.PK;

				AssertNotNull(config.Lookups.BranchList);
				AssertEquals("The first list item should be 'All Branches'", ZString.Empty, config.Lookups.BranchList[0].Code);
				Assert(config.Lookups.BranchList.ContainsCode(branch2.GB_Code));
				Assert(!config.Lookups.BranchList.ContainsCode(defaultBranch.GB_Code));
				AssertEquals(2, config.Lookups.BranchList.Count);
			}
		}

		public void TestSupplyTypeList()
		{
			AssertNotNull(PlaceOfSupplyConfig.Lookups.SupplyTypeList);
			var supplyTypesInRegistry = AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesList.Value.GetActiveCodeDescriptionPairList();
			Assert("Pre-condition: Have some active Supply Types", supplyTypesInRegistry.Count > 0);

			AssertContainsExactElementsInExactOrder(supplyTypesInRegistry.GetAllCodesZString(), PlaceOfSupplyConfig.Lookups.SupplyTypeList.GetAllCodesZString());
		}

		public void TestPlaceOfSupplyRuleList()
		{
			PlaceOfSupplyConfig.PSC_JobType = JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All;
			AssertNotNull(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList);
			Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.BillToPartyLocation));
			Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.SupplierLocation));
			AssertEquals(2, PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.Count);

			PlaceOfSupplyConfig.PSC_JobType = JobInvoicingConsumerTypes.ShipmentCode;
			AssertNotNull(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList);
			Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.BillToPartyLocation));
			Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.SupplierLocation));
			Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.PickupCFS));
			Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.DeliveryCFS));
			Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.PickupLocation));
			Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.DeliveryLocation));
			Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.PickupAgent));
			Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.DeliveryAgent));
			Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.Origin));
			Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.Destination));
			Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.PickupTransitWarehouse));
			Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.DeliveryTransitWarehouse));
			Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.LastPortOfDischargeInCompanyCountry));
			Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.FirstPortOfLoadingInCompanyCountry));
			Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.ConsolPortOfLoading));
			Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.ConsolPortOfDischarge));
			Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.ConsolSendingAgent));
			Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.ConsolReceivingAgent));
			AssertEquals(18, PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.Count);

			PlaceOfSupplyConfig.PSC_JobType = JobInvoicingConsumerTypes.ForwardingConsolCode;
			AssertNotNull(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList);
			Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.BillToPartyLocation));
			Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.SupplierLocation));
			Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.ConsolPortOfLoading));
			Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.ConsolPortOfDischarge));
			Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.ConsolSendingAgent));
			Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.ConsolReceivingAgent));
			Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.ArrivalCFS));
			Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.DepartureCFS));
			Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.ArrivalCTO));
			Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.DepartureCTO));
			AssertEquals(10, PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.Count);

			PlaceOfSupplyConfig.PSC_JobType = JobInvoicingConsumerTypes.BrokerageCode;
			AssertNotNull(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList);
			Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.BillToPartyLocation));
			Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.SupplierLocation));
			Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.PortOfLoading));
			Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.PortOfDischarge));
			Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.PortOfFirstArrival));
			Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.PortOfOrigin));
			Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.FinalDestination));
			Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.FirstPortOfLoadingInCompanyCountry));
			Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.LastPortOfDischargeInCompanyCountry));
			Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.PickupLocation));
			Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.DeliveryLocation));
			AssertEquals(11, PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.Count);

			PlaceOfSupplyConfig.PSC_JobType = JobInvoicingConsumerTypes.QuotedBookingCode;
			AssertNotNull(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList);
			Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.BillToPartyLocation));
			Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.SupplierLocation));
			Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.Origin));
			Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.Destination));
			Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.PickupLocation));
			Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.PickupCFS));
			Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.DeliveryCFS));
			Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.Load));
			Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.Discharge));
			Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.PickupCTO));
			Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.DeliveryCTO));
			AssertEquals(11, PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.Count);

			PlaceOfSupplyConfig.PSC_JobType = JobInvoicingConsumerTypes.WorkItemCode;
			AssertNotNull(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList);
			Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.BillToPartyLocation));
			Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.SupplierLocation));
			Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.CountryRegionPort));
			AssertEquals(3, PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.Count);
		}

		public void TestPlaceOfSupplyRuleList_WithPredefinedRuleEnabled()
		{
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.PredefinedRule.Code))
			{
				PlaceOfSupplyConfig.PSC_JobType = JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All;
				AssertNotNull(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList);
				Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.BillToPartyLocation));
				Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.SupplierLocation));
				Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.OtherTerritories));
				AssertEquals(3, PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.Count);

				PlaceOfSupplyConfig.PSC_JobType = JobInvoicingConsumerTypes.ShipmentCode;
				AssertNotNull(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList);
				Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.BillToPartyLocation));
				Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.SupplierLocation));
				Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.OtherTerritories));
				Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.PickupCFS));
				Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.DeliveryCFS));
				Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.PickupLocation));
				Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.DeliveryLocation));
				Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.PickupAgent));
				Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.DeliveryAgent));
				Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.Origin));
				Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.Destination));
				Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.PickupTransitWarehouse));
				Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.DeliveryTransitWarehouse));
				Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.LastPortOfDischargeInCompanyCountry));
				Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.FirstPortOfLoadingInCompanyCountry));
				Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.ConsolPortOfLoading));
				Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.ConsolPortOfDischarge));
				Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.ConsolSendingAgent));
				Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.ConsolReceivingAgent));
				AssertEquals(19, PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.Count);

				PlaceOfSupplyConfig.PSC_JobType = JobInvoicingConsumerTypes.ForwardingConsolCode;
				AssertNotNull(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList);
				Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.BillToPartyLocation));
				Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.OtherTerritories));
				Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.SupplierLocation));
				Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.ConsolPortOfLoading));
				Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.ConsolPortOfDischarge));
				Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.ConsolSendingAgent));
				Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.ConsolReceivingAgent));
				Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.ArrivalCFS));
				Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.DepartureCFS));
				Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.ArrivalCTO));
				Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.DepartureCTO));
				AssertEquals(11, PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.Count);

				PlaceOfSupplyConfig.PSC_JobType = JobInvoicingConsumerTypes.BrokerageCode;
				AssertNotNull(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList);
				Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.BillToPartyLocation));
				Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.SupplierLocation));
				Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.OtherTerritories));
				Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.PortOfLoading));
				Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.PortOfDischarge));
				Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.PortOfFirstArrival));
				Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.PortOfOrigin));
				Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.FinalDestination));
				Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.FirstPortOfLoadingInCompanyCountry));
				Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.LastPortOfDischargeInCompanyCountry));
				Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.PickupLocation));
				Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.DeliveryLocation));
				AssertEquals(12, PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.Count);

				PlaceOfSupplyConfig.PSC_JobType = JobInvoicingConsumerTypes.QuotedBookingCode;
				AssertNotNull(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList);
				Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.BillToPartyLocation));
				Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.SupplierLocation));
				Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.OtherTerritories));
				Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.Origin));
				Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.Destination));
				Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.PickupLocation));
				Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.PickupCFS));
				Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.DeliveryCFS));
				Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.Load));
				Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.Discharge));
				Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.PickupCTO));
				Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.DeliveryCTO));
				AssertEquals(12, PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.Count);

				PlaceOfSupplyConfig.PSC_JobType = JobInvoicingConsumerTypes.WorkItemCode;
				AssertNotNull(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList);
				Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.BillToPartyLocation));
				Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.SupplierLocation));
				Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.OtherTerritories));
				Assert(PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.ContainsCode(AccPOSRuleList.Codes.CountryRegionPort));
				AssertEquals(4, PlaceOfSupplyConfig.Lookups.PlaceOfSupplyRuleList.Count);
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			PlaceOfSupplyConfig = Factory.NewWithValidTestData<AccPOSConfiguration>();
		}
		AccPOSConfiguration PlaceOfSupplyConfig;

		#endregion
	}
}

