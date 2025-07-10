using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Customs;
using Constants = Enterprise.Core.Constants;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;
using ShipmentTypes = Enterprise.Core.Constants.ShipmentTypes;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingShipment.BaseForwardingShipmentInvoicingSupporter))]
	sealed class BaseForwardingShipmentInvoicingSupporterTest : JobInvoicingSupporterTest
	{
		public void TestGetLocationsFromDeclarationSupporters()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "Org1";

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_FullName = "Org2";

			var branchAU = Factory.NewWithValidTestData<GlbBranch>();
			branchAU.GB_RN_NKCountryCode = "AU";

			var branchUS = Factory.NewWithValidTestData<GlbBranch>();
			branchUS.GB_RN_NKCountryCode = "US";

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var declarationInCurrentBranch = Factory.New<IBaseJobDeclaration>();

			shipment.JS_RL_NKOrigin = "AUMEL";
			shipment.JS_RL_NKDestination = "USCHI";

			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("US"))
			{
				var supporter = new ForwardingShipment.BaseForwardingShipmentInvoicingSupporter(shipment);
				AssertEquals("USCHI", supporter.Destination.RL_Code);
				AssertEquals("AUMEL", supporter.Origin.RL_Code);

				declarationInCurrentBranch.JE_JS = shipment.PK;
				declarationInCurrentBranch.JE_OH_Supplier = org2.PK;
				declarationInCurrentBranch.JE_RL_NKOrigin = "AUSYD";
				declarationInCurrentBranch.JE_RL_NKFinalDestination = "USNYC";
				declarationInCurrentBranch.JE_GB = GlbBranch.CurrentBranch.PK.ToGuid();

				Factory.Save();

				using (AccountingMasterFilesRegistry.Instance.UseBrokerageLocationsForBilling.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					supporter = new ForwardingShipment.BaseForwardingShipmentInvoicingSupporter(shipment);
					AssertEquals("USNYC", supporter.Destination.RL_Code);
					AssertEquals("AUSYD", supporter.Origin.RL_Code);
				}

				using (AccountingMasterFilesRegistry.Instance.UseBrokerageLocationsForBilling.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
				{
					supporter = new ForwardingShipment.BaseForwardingShipmentInvoicingSupporter(shipment);
					AssertEquals("USCHI", supporter.Destination.RL_Code);
					AssertEquals("AUMEL", supporter.Origin.RL_Code);
				}
			}
		}

		#region IControllingCustomerSalesRepDefaulting test cases

		public void TestIsAllowedToDefaultSalesRepFromControllingCustomer()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var supporter = new ForwardingShipment.BaseForwardingShipmentInvoicingSupporter(shipment) as ISalesRepDefaultingFromControllingCustomer;

			using (FreightDataRegistry.Instance.ControllingCustomerUseSalesRep.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				Assert(!supporter.IsAllowedToDefaultSalesRepFromControllingCustomer);
			}

			using (FreightDataRegistry.Instance.ControllingCustomerUseSalesRep.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				Assert(supporter.IsAllowedToDefaultSalesRepFromControllingCustomer);
			}
		}

		public void TestNotifyControllingCustomerChanged()
		{
			var numberOfOnControllingCustomerChangedCalls = 0;
			EventHandler handler = (sender, e) => numberOfOnControllingCustomerChangedCalls++;

			var shipment = Factory.New<ForwardingShipment>();

			var controllingCustomerOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ControllingCustomerAddress.E2_OA_Address = controllingCustomerOrg2.MainAddress.PK;
			AssertEquals(0, numberOfOnControllingCustomerChangedCalls);

			var supporter = new ForwardingShipment.BaseForwardingShipmentInvoicingSupporter(shipment) as ISalesRepDefaultingFromControllingCustomer;
			supporter.NotifyControllingCustomerChanged(handler);

			AssertNoExceptionThrown(() => supporter.NotifyControllingCustomerChanged(handler));
			AssertErrorReporting(() => supporter.NotifyControllingCustomerChanged((sender, e) => numberOfOnControllingCustomerChangedCalls++));
			AssertNoExceptionThrown(() => supporter.NotifyControllingCustomerChanged(null));
			AssertNoExceptionThrown(() => supporter.NotifyControllingCustomerChanged(handler));

			var notifyPartyOrg = Factory.NewWithValidTestData<OrgHeader>();
			shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = notifyPartyOrg.MainAddress.PK;
			AssertEquals(0, numberOfOnControllingCustomerChangedCalls);

			var controllingCustomerOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ControllingCustomerAddress.E2_OA_Address = controllingCustomerOrg1.MainAddress.PK;
			AssertEquals("Only one call expected to avoid multiple subscriptions.", 1, numberOfOnControllingCustomerChangedCalls);

			void AssertErrorReporting(Action testCase)
			{
				AssertEquals("Precondition: TotalErrorCount", 0, ErrorReporter.TotalErrorCount);
				testCase();
				AssertEquals("TotalErrorCount", 1, ErrorReporter.TotalErrorCount);
				AssertContains("LastKeyReported", "NotifyControllingCustomerChangedCalledWithDifferentDelegates_4", ErrorReporter.LastKeyReported);
				AssertContains("LastMessageReported", "NotifyControllingCustomerChanged can't be called with different delegates", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
				AssertEquals("Postcondition: TotalErrorCount", 0, ErrorReporter.TotalErrorCount);
			}
		}

		[TestDate(2019, 12, 20, 11, 12, 13)]
		public void TestNotifyControllingCustomerChangedDeveloperErrorMessage()
		{
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(ZGuid.NewZGuid(), CriticalValidationInfoCollectorServiceKeyType.JobConstructorStackTrace);

			var shipment = Factory.New<ForwardingShipment>();
			var job = new JobHeader.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();
			Factory.Save();

			var factory1 = new BusinessObjectFactory();
			factory1.SetContext(BusinessContext.InvoicingPluginGUIExcludingConsol);
			var shipmentInFactory1 = factory1.Load<ForwardingShipment>(shipment.PK);
			var jobInFactory1 = factory1.Load<JobHeaderForTest>(job.PK);
			var supporter = new ForwardingShipment.BaseForwardingShipmentInvoicingSupporter(shipmentInFactory1) as ISalesRepDefaultingFromControllingCustomer;
			supporter.NotifyControllingCustomerChanged(jobInFactory1.NotifyControllingCustomerChangedHandler);
			AssertEquals("LastKeyReported", string.Empty, ErrorReporter.LastKeyReported);
			AssertEquals("LastMessageReported", string.Empty, ErrorReporter.LastMessageReported);

			var factory2 = new BusinessObjectFactory();
			factory2.SetContext(BusinessContext.InvoicingPluginGUIExcludingConsol);
			var jobInFactory2 = factory2.Load<JobHeaderForTest>(job.PK);
			supporter.NotifyControllingCustomerChanged(jobInFactory2.NotifyControllingCustomerChangedHandler);
			AssertContains("LastKeyReported", "NotifyControllingCustomerChangedCalledWithDifferentDelegates_4", ErrorReporter.LastKeyReported);
			AssertContains("Message should contain shipment information", $@"NotifyControllingCustomerChanged can't be called with different delegates. All such calls are ignored.

Current Login Company PK: {Env.CurrentCompanyPK}
Current Login Company Code: {Env.CurrentCompany.Code}
Shipment PK: {shipment.PK}
Shipment Number: {shipment.JS_UniqueConsignRef}
Shipment Factory: {shipmentInFactory1.Factory._Instance}
Shipment Business Contexts: Factory Level : (InvoicingPluginGUIExcludingConsol)", ErrorReporter.LastMessageReported);

			AssertContains("Message should contain subscribed job information", $@"
Subscribed Job PK: {jobInFactory1.PK}
Subscribed Job Factory: {jobInFactory1.Factory._Instance}
Subscribed Job Is In Database: True
Subscribed Job Is Deleted: False
Subscribed Job Business Contexts: Factory Level : (InvoicingPluginGUIExcludingConsol)
Subscribed Job Number: {jobInFactory1.JH_JobNum}
Subscribed Job Company PK: {Env.CurrentCompanyPK}
Subscribed Job Company Code: {Env.CurrentCompany.Code}
Subscribed Job Parent Table Code and ID : JS - [{shipment.PK}]
Subscribed Job Header Constructor Stacktrace : 
JobConstructorStackTrace:
Job Created Time: 2019-12-20 11:12:13.000
   at System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)", ErrorReporter.LastMessageReported);

			AssertContains("Message should contain subsequent job information", $@"
Subsequent Job PK: {jobInFactory2.PK}
Subsequent Job Factory: {jobInFactory2.Factory._Instance}
Subsequent Job Is In Database: True
Subsequent Job Is Deleted: False
Subsequent Job Business Contexts: Factory Level : (InvoicingPluginGUIExcludingConsol)
Subsequent Job Number: {jobInFactory2.JH_JobNum}
Subsequent Job Company PK: {Env.CurrentCompanyPK}
Subsequent Job Company Code: {Env.CurrentCompany.Code}
Subsequent Job Parent Table Code and ID : JS - [{shipment.PK}]
Subsequent Job Header Constructor Stacktrace : 
JobConstructorStackTrace:
Job Created Time: 2019-12-20 11:12:13.000
   at System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)", ErrorReporter.LastMessageReported);

			ErrorReporter.Instance.Clear();
		}

		public void TestGetReasonNotToAllowAutoRate_WhenAutorateRevenue_ShouldNotCheckClientContractNumberRules()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var supporter = new ForwardingShipment.BaseForwardingShipmentInvoicingSupporter(shipment);

			var numbers = shipment.Numbers;
			numbers.AddOrSkipContractNumber("AAA", "CLC", countryCode: "AU");
			var newNumber = numbers.AddNew();
			newNumber.CE_EntryType = "CLC";
			newNumber.CE_RN_NKCountryCode = "US";
			newNumber.CE_EntryNum = "BBB";

			var result = supporter.GetReasonNotToAllowAutoRate(AutoRateOptions.AutorateRevenue);
			AssertEquals("There should be 2 CLCs", 2, numbers.Count);
			AssertNullOrEmpty("No error about CLCs", result);
		}

		class JobHeaderForTest : JobHeader
		{
			public JobHeaderForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override bool IsChargesCollectionLoaded => false;

			internal void DummyMethod() { }

			internal EventHandler NotifyControllingCustomerChangedHandler => ((sender, e) => DummyMethod());
		}

		#endregion

		#region TestIncludeInConsolCosting

		public void TestIncludeInConsolCosting()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var coloadShipment = Factory.New<ForwardingShipment>();
			coloadShipment.JS_UniqueConsignRef = "SHP123";

			var supporter = new ForwardingShipment.BaseForwardingShipmentInvoicingSupporter(shipment);

			shipment.IsCancelled = true;
			AssertEquals("When shipment is cancelled it should not be included into Consol Costing.", false, supporter.IncludeInConsolCosting(true));
			AssertEquals("When shipment is cancelled it should not be included into Consol Costing.", false, supporter.IncludeInConsolCosting(false));

			shipment.IsCancelled = false;
			AssertEquals("When shipment is not cancelled it should be included into Consol Costing.", true, supporter.IncludeInConsolCosting(true));
			AssertEquals("When shipment is not cancelled it should be included into Consol Costing.", true, supporter.IncludeInConsolCosting(false));

			shipment.JS_JS_ColoadMasterShipment = coloadShipment.PK;
			AssertEquals("When shipment has a coload shipment, then we should include it into Consol Costing depending on if we should Include Related Shipments.", true, supporter.IncludeInConsolCosting(true));
			AssertEquals("When shipment has a coload shipment, then we should include it into Consol Costing depending on if we should Include Related Shipments.", false, supporter.IncludeInConsolCosting(false));
		}

		public void TestOverriddenDefaultLocalClient()
		{
			var consignor = Factory.New<OrgHeader>();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;

			var supporter = new ForwardingShipment.BaseForwardingShipmentInvoicingSupporter(shipment);

			AssertNull(supporter.OverriddenDefaultLocalClient);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValueLegacy;
			supporter = new ForwardingShipment.BaseForwardingShipmentInvoicingSupporter(shipment);

			AssertNotNull(supporter.OverriddenDefaultLocalClient);
			AssertEquals("OverriddenDefaultLocalClient", consignor.PK, supporter.OverriddenDefaultLocalClient.PK);

			var relatedParty = Factory.New<OrgHeader>();
			consignor.SetRelatedParty(relatedParty, RelatedPartyTypeList.Codes.InvoiceFreightJobsTo, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.All, ZString.Empty);

			supporter = new ForwardingShipment.BaseForwardingShipmentInvoicingSupporter(shipment);

			AssertNotNull(supporter.OverriddenDefaultLocalClient);
			AssertEquals("OverriddenDefaultLocalClient", consignor.PickupFreightBillTo.PK, supporter.OverriddenDefaultLocalClient.PK);
		}

		#endregion

		public void TestServiceDirection()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			ForwardingShipment.BaseForwardingShipmentInvoicingSupporter supporter = new ForwardingShipment.BaseForwardingShipmentInvoicingSupporter(shipment);
			Assert(supporter.IsExport);
			Assert(!supporter.IsImport);
			Assert(!supporter.IsCrossTrade);
			Assert(!supporter.IsDomestic);
			AssertEquals(OrgConstants.ServiceDirection.Code.Export, supporter.ServiceDirection);

			shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUSYD";
			supporter = new ForwardingShipment.BaseForwardingShipmentInvoicingSupporter(shipment);
			Assert(!supporter.IsExport);
			Assert(supporter.IsImport);
			Assert(!supporter.IsCrossTrade);
			Assert(!supporter.IsDomestic);
			AssertEquals(OrgConstants.ServiceDirection.Code.Import, supporter.ServiceDirection);

			shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "USLAX";
			supporter = new ForwardingShipment.BaseForwardingShipmentInvoicingSupporter(shipment);
			Assert(!supporter.IsExport);
			Assert(!supporter.IsImport);
			Assert(!supporter.IsCrossTrade);
			Assert(supporter.IsDomestic);
			AssertEquals(OrgConstants.ServiceDirection.Code.Domestic, supporter.ServiceDirection);

			shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "INBOM";
			shipment.JS_RL_NKDestination = "USLAX";
			supporter = new ForwardingShipment.BaseForwardingShipmentInvoicingSupporter(shipment);
			Assert(!supporter.IsExport);
			Assert(!supporter.IsImport);
			Assert(supporter.IsCrossTrade);
			Assert(!supporter.IsDomestic);
			AssertEquals(OrgConstants.ServiceDirection.Code.CrossTrade, supporter.ServiceDirection);
		}

		public void TestAgentsFromConsol()
		{
			AccountingMasterFilesRegistry.Instance.DefaultOSAgentFromPickupAgent.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			var consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			var consol1SendingAgent = Factory.NewWithValidTestData<OrgHeader>();
			consol1.JK_OA_SendingForwarderAddress = consol1SendingAgent.Addresses.MainAddress.PK;
			var consol1ReceivingAgent = Factory.NewWithValidTestData<OrgHeader>();
			consol1.JK_OA_ReceivingForwarderAddress = consol1ReceivingAgent.Addresses.MainAddress.PK;

			var transport1 = (consol1.Transports.FirstOrDefault() ?? consol1.Transports.AddNew()) as Transport;
			transport1.JW_RL_NKLoadPort = "NZAKL";
			transport1.JW_RL_NKDiscPort = "AUSYD";
			transport1.JW_ATD = ZDateTime.Now;
			transport1.JW_ATA = ZDateTime.Now.AddDays(1);

			var consol2 = Factory.NewWithValidTestData<ForwardingConsol>();
			var consol2SendingAgent = Factory.NewWithValidTestData<OrgHeader>();
			consol2.JK_OA_SendingForwarderAddress = consol2SendingAgent.Addresses.MainAddress.PK;
			var consol2ReceivingAgent = Factory.NewWithValidTestData<OrgHeader>();
			consol2.JK_OA_ReceivingForwarderAddress = consol2ReceivingAgent.Addresses.MainAddress.PK;

			var transport2 = (consol2.Transports.FirstOrDefault() ?? consol2.Transports.AddNew()) as Transport;
			transport2.JW_RL_NKLoadPort = "AUSYD";
			transport2.JW_RL_NKDiscPort = "SGSIN";
			transport2.JW_ATD = ZDateTime.Now.AddDays(2);
			transport2.JW_ATA = ZDateTime.Now.AddDays(3);

			shipment.Consols.Add(consol1);
			shipment.Consols.Add(consol2);

			Factory.Save();

			AssertEquals(consol1.PK, shipment.Consols.GetEarliestConsol().PK);
			AssertEquals(consol2.PK, shipment.Consols.GetLatestConsol().PK);
			AssertEquals(consol1SendingAgent.PK, shipment.InvoicingSupporter.EarliestSendingAgent.PK);
			AssertEquals(consol2ReceivingAgent.PK, shipment.InvoicingSupporter.LatestReceivingAgent.PK);

			var pickupAgent = Factory.NewWithValidTestData<OrgHeader>();
			shipment.PickupAgentPK = pickupAgent.PK;
			var deliveryAgent = Factory.NewWithValidTestData<OrgHeader>();
			shipment.JS_OH_DeliveryAgent = deliveryAgent.PK;

			AssertEquals(pickupAgent.PK, shipment.InvoicingSupporter.EarliestSendingAgent.PK);
			AssertEquals(deliveryAgent.PK, shipment.InvoicingSupporter.LatestReceivingAgent.PK);

			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "SGSIN";
			transport1.JW_ATD = ZDateTime.Now.AddDays(2);
			transport1.JW_ATA = ZDateTime.Now.AddDays(3);

			transport2.JW_RL_NKLoadPort = "NZAKL";
			transport2.JW_RL_NKDiscPort = "AUSYD";
			transport2.JW_ATD = ZDateTime.Now;
			transport2.JW_ATA = ZDateTime.Now.AddDays(1);

			AssertEquals(consol2.PK, shipment.Consols.GetEarliestConsol().PK);
			AssertEquals(consol1.PK, shipment.Consols.GetLatestConsol().PK);
			AssertEquals(pickupAgent.PK, shipment.InvoicingSupporter.EarliestSendingAgent.PK);
			AssertEquals(deliveryAgent.PK, shipment.InvoicingSupporter.LatestReceivingAgent.PK);

			shipment.PickupAgentPK = ZGuid.Empty;
			shipment.JS_OH_DeliveryAgent = ZGuid.Empty;

			AssertEquals(consol2SendingAgent.PK, shipment.InvoicingSupporter.EarliestSendingAgent.PK);
			AssertEquals(consol1ReceivingAgent.PK, shipment.InvoicingSupporter.LatestReceivingAgent.PK);
		}

		public void TestPickupAgentOnlyUsedForReceivingAgentIfRegistryAllows()
		{
			AccountingMasterFilesRegistry.Instance.DefaultOSAgentFromPickupAgent.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var sendingAgent = Factory.NewWithValidTestData<OrgHeader>();
			consol.JK_OA_SendingForwarderAddress = sendingAgent.Addresses.MainAddress.PK;

			var shipment = consol.Shipments.AddNew();
			var pickupAgent = Factory.NewWithValidTestData<OrgHeader>();
			shipment.PickupAgentPK = pickupAgent.PK;

			Factory.Save();

			AssertEquals(pickupAgent.PK, shipment.InvoicingSupporter.EarliestSendingAgent.PK);
			AccountingMasterFilesRegistry.Instance.DefaultOSAgentFromPickupAgent.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(sendingAgent.PK, shipment.InvoicingSupporter.EarliestSendingAgent.PK);
		}

		public void TestControllingCustomer()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var shipment = Factory.New<ForwardingShipment>();
			shipment.DocAddresses.CreateWithAddressType(DocAddressType.ControllingCustomer).E2_OA_Address = org.MainAddress.PK;

			var supporter = new ForwardingShipment.BaseForwardingShipmentInvoicingSupporter(shipment);
			AssertEquals("ControllingCustomer PK", org.PK, supporter.ControllingCustomer.PK);
		}

		public void TestControllingAgent()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var shipment = Factory.New<ForwardingShipment>();
			shipment.DocAddresses.CreateWithAddressType(DocAddressType.ControllingAgent).E2_OA_Address = org.MainAddress.PK;

			var supporter = new ForwardingShipment.BaseForwardingShipmentInvoicingSupporter(shipment);
			AssertEquals("ControllingAgent PK", org.PK, supporter.ControllingAgent.PK);
		}

		public void TestHasContainerCostShare()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var shipment = Factory.New<ForwardingShipment>();
			var consol = shipment.Consols.AddNew();

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "AAA";
			var service = container1.Services.AddNew();
			service.ES_ServiceCode = null;

			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_Description = "waffles";
			packLine1.JL_PackageCount = 33;
			packLine1.SetContainer(container1.PK);

			var supporter = new ForwardingShipment.BaseForwardingShipmentInvoicingSupporter(shipment);
			AssertNoExceptionThrown("System.NullReferenceException shouldn't be thrown", () => supporter.HasContainerCostShare(null));
			AssertNoExceptionThrown("System.NullReferenceException shouldn't be thrown", () => supporter.HasContainerCostShare(chargeCode));
		}

		public void TestBookingContainersForBinding()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var container1 = Factory.New<ForwardingContainer>();
			container1.JC_JS_FCLBookingOnlyLink = shipment.PK;
			var container2 = Factory.New<ForwardingContainer>();
			container2.JC_JS_FCLBookingOnlyLink = shipment.PK;
			Factory.Save();

			var bookingContainers = shipment.BookingContainersForBinding;
			AssertArrayEqualsByElements(new ForwardingContainer[] { container1, container2 }, bookingContainers.ToArray());
		}

		public override void TestCustomsEntryNumberType()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.CustomsEntryNumberType = "TF";
			var supporter = new ForwardingShipment.BaseForwardingShipmentInvoicingSupporter(shipment);

			AssertEquals("CustomsEntryNumberType", "TF", supporter.CustomsEntryNumberType);
		}

		public override void TestCommunityTransitStatus()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_CommunityTransitStatus = "TF";
			var supporter = new ForwardingShipment.BaseForwardingShipmentInvoicingSupporter(shipment);

			AssertEquals("CommunityTransitStatus", "TF", supporter.CommunityTransitStatus);
		}

		public void TestCreateAccountingJobOnSavingOfOperationsJob()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var coLoadShipment = Factory.New<ForwardingShipment>();
			var supporter = new ForwardingShipment.BaseForwardingShipmentInvoicingSupporter(shipment);
			var validCoLoadShipmentTypes = typeof(ShipmentTypes).GetFields()
				.Select(type => type.GetValue(null) as string)
				.Where(type => !type.Equals("HLS") && !type.Equals("HVL"));

			AssertEquals("Pre-condition", false, supporter.CreateAccountingJobOnSavingOfOperationsJob);

			shipment.JS_JS_ColoadMasterShipment = coLoadShipment.PK;

			Factory.Save();

			foreach (var shipmentType in validCoLoadShipmentTypes)
			{
				shipment.CoLoadMasterShipment.JS_ShipmentType = shipmentType;

				if (shipmentType.Equals(Constants.ShipmentTypes.BuyersConsolLead))
				{
					AssertEquals(supporter.CreateAccountingJobOnSavingOfOperationsJob, true);
				}
				else
				{
					AssertEquals(supporter.CreateAccountingJobOnSavingOfOperationsJob, false);
				}
			}
		}

		#region CheckTotalsDiffer

		public void TestCheckTotalsDiffer_WithCorrectInners_DoesNotFireEventAndDoesNotUpdateTotalPackCount()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_TotalPackageCount = 2;

			var item1 = Factory.New<IHVLVItem>();
			var item2 = Factory.New<IHVLVItem>();
			item1.HVI_JS_LoadedOnShipment = shipment.PK;
			item2.HVI_JS_LoadedOnShipment = shipment.PK;

			var eventHandlerWasCalled = false;
			shipment.UpdateShipmentTotalsPackQuantityVariation += (sender, args) => eventHandlerWasCalled = true;

			AssertEquals("Precondition:", shipment.JS_TotalPackageCount, shipment.HVLVItemCount);

			shipment.CheckTotalsDiffer();

			CombineAssertions(() =>
			{
				AssertEquals("Inners value not changed", 2, shipment.JS_TotalPackageCount);
				AssertEquals("Should not have fired event", false, eventHandlerWasCalled);
			});
		}

		public void TestCheckTotalsDiffer_WithDifferentInners_FiresEventAndUpdatesTotalPackCount()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_TotalPackageCount = 1;

			var consignmentHeader = Factory.LoadTop1<IHVLVConsignmentHeader>(new ZQuery(HVLVConsignmentHeaderSchema.HCH_JS_Shipment, shipment.PK)) as BusinessObject;

			var consignment = Factory.New<IHVLVConsignment>() as BusinessObject;
			consignment[HVLVConsignmentSchema.HVC_HCH_Header.Name] = consignmentHeader.PK;

			var item1 = Factory.New<IHVLVItem>();
			item1.HVI_HVC_Consignment = consignment.PK;
			item1.HVI_JS_LoadedOnShipment = shipment.PK;

			var item2 = Factory.New<IHVLVItem>();
			item2.HVI_HVC_Consignment = consignment.PK;
			item2.HVI_JS_LoadedOnShipment = shipment.PK;

			var item3 = Factory.New<IHVLVItem>();
			item3.HVI_HVC_Consignment = consignment.PK;
			item3.HVI_JS_LoadedOnShipment = shipment.PK;

			Factory.Save();

			var eventHandlerWasCalled = false;
			shipment.UpdateShipmentTotalsPackQuantityVariation += (sender, args) => eventHandlerWasCalled = true;

			AssertNotEquals("Precondition:", shipment.JS_TotalPackageCount, shipment.HVLVItemCount);

			shipment.CheckTotalsDiffer();

			CombineAssertions(() =>
			{
				AssertEquals("Inners value updated", 3, shipment.JS_TotalPackageCount);
				AssertEquals("Should have fired event", true, eventHandlerWasCalled);
			});
		}

		public void TestCheckTotalsDiffer_WithCancelledEvent_FiresEventAndDoesNotUpdateTotalPackCount()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_TotalPackageCount = 1;

			var consignmentHeader = Factory.LoadTop1<IHVLVConsignmentHeader>(new ZQuery(HVLVConsignmentHeaderSchema.HCH_JS_Shipment, shipment.PK)) as BusinessObject;

			var consignment = Factory.New<IHVLVConsignment>() as BusinessObject;
			consignment[HVLVConsignmentSchema.HVC_HCH_Header.Name] = consignmentHeader.PK;

			var item1 = Factory.New<IHVLVItem>();
			item1.HVI_HVC_Consignment = consignment.PK;
			item1.HVI_JS_LoadedOnShipment = shipment.PK;

			var item2 = Factory.New<IHVLVItem>();
			item2.HVI_HVC_Consignment = consignment.PK;
			item2.HVI_JS_LoadedOnShipment = shipment.PK;

			var item3 = Factory.New<IHVLVItem>();
			item3.HVI_HVC_Consignment = consignment.PK;
			item3.HVI_JS_LoadedOnShipment = shipment.PK;

			Factory.Save();

			var eventHandlerWasCalled = false;
			shipment.UpdateShipmentTotalsPackQuantityVariation += new CancelEventHandler((sender, e) => { e.Cancel = true; eventHandlerWasCalled = true; });

			AssertNotEquals("Precondition:", shipment.JS_TotalPackageCount, shipment.HVLVItemCount);

			shipment.CheckTotalsDiffer();

			CombineAssertions(() =>
			{
				AssertEquals("Inners value not updated as event cancelled", 1, shipment.JS_TotalPackageCount);
				AssertEquals("Should have fired event", true, eventHandlerWasCalled);
			});
		}

		public void TestCheckTotalsDiffer_NotHVLVType_DoesNotFireEventAndDoesNotUpdateTotalPackCount()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.AssemblyMaster;
			shipment.JS_TotalPackageCount = 1;

			var item1 = Factory.New<IHVLVItem>();
			var item2 = Factory.New<IHVLVItem>();
			var item3 = Factory.New<IHVLVItem>();
			item1.HVI_JS_LoadedOnShipment = shipment.PK;
			item2.HVI_JS_LoadedOnShipment = shipment.PK;
			item3.HVI_JS_LoadedOnShipment = shipment.PK;

			var eventHandlerWasCalled = false;
			shipment.UpdateShipmentTotalsPackQuantityVariation += (sender, args) => eventHandlerWasCalled = true;

			AssertNotEquals("Precondition:", shipment.JS_TotalPackageCount, shipment.HVLVItemCount);

			shipment.CheckTotalsDiffer();

			CombineAssertions(() =>
			{
				AssertEquals("Inners value not updated", 1, shipment.JS_TotalPackageCount);
				AssertEquals("Should not have fired event", false, eventHandlerWasCalled);
			});
		}

		#endregion

		#region NVOCC House Bill of Lading (HBL)

		public void TestIsElectronicShippingInstructionReceived()
		{
			var shipment = Factory.New<ForwardingShipment>();
			AssertEquals(false, shipment.IsElectronicShippingInstructionReceived);

			var log = shipment.Logs.AddNew(Events.StatusUpdated, new KeyValuePair<string, string>(Params.Type, Constants.EventReferenceMessageTypes.ShipmentStatus));
			AssertEquals(false, shipment.IsElectronicShippingInstructionReceived);

			log.UpdateReference($"|{Params.Type}={Constants.EventReferenceMessageTypes.ShipmentStatus}|{Params.New}={ShipmentStatusList.Codes.ElectronicShippingInstruction}");
			AssertEquals(true, shipment.IsElectronicShippingInstructionReceived);

			shipment.Logs.AddNew(Events.StatusUpdated, new KeyValuePair<string, string>(Params.Type, Constants.EventReferenceMessageTypes.ShipmentStatus), new KeyValuePair<string, string>(Params.New, ShipmentStatusList.Codes.Confirmed));
			AssertEquals(true, shipment.IsElectronicShippingInstructionReceived);

			log.Cancel();
			AssertEquals(false, shipment.IsElectronicShippingInstructionReceived);
		}

		#endregion

		#region NVOCC IsElectronicBookingReceived

		public void TestIsElectronicBookingReceived()
		{
			var shipment = Factory.New<ForwardingShipment>();
			AssertEquals(false, shipment.IsElectronicBookingReceived);

			var log = shipment.Logs.AddNew(Events.StatusUpdated, new KeyValuePair<string, string>(Params.Type, Constants.EventReferenceMessageTypes.ShipmentStatus));
			AssertEquals(false, shipment.IsElectronicBookingReceived);

			log.UpdateReference($"|{Params.Type}={Constants.EventReferenceMessageTypes.ShipmentStatus}|{Params.New}={ShipmentStatusList.Codes.ElectronicBooking}");
			AssertEquals(true, shipment.IsElectronicBookingReceived);

			log.Cancel();
			AssertEquals(false, shipment.IsElectronicBookingReceived);
		}

		#endregion

		#region Menu Item Filter

		public void TestHasHIRAndStartWithSHPNumber()
		{
			var masterShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;

			AssertEquals("Master Shipment not has HIR Number starting with SHP", false, masterShipment.HasHIRAndStartWithSHPNumber);
			AssertEquals("Not has Related Shipments", false, masterShipment.CoLoadShipments.OfType<ForwardingShipment>().Any(coloadShipment => coloadShipment.HasHIRAndStartWithSHPNumber));

			var masterShipmentNumber = masterShipment.Numbers.AddNew();
			masterShipmentNumber.CE_EntryNum = "SHP1223";
			masterShipmentNumber.CE_EntryType = CustomsReferenceNumberType.eHubInterchangeReference.HIR;
			AssertEquals("Master Shipment has HIR Number starting with SHP", true, masterShipment.HasHIRAndStartWithSHPNumber);
			AssertEquals("Not has Related Shipments", false, masterShipment.CoLoadShipments.OfType<ForwardingShipment>().Any(coloadShipment => coloadShipment.HasHIRAndStartWithSHPNumber));

			var coloadShipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			masterShipment.CoLoadShipments.Add(coloadShipment1);
			var coloadShipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			masterShipment.CoLoadShipments.Add(coloadShipment2);

			AssertEquals("Related Shipments not have Reference Numbers", false, masterShipment.CoLoadShipments.OfType<ForwardingShipment>().Any(coloadShipment => coloadShipment.HasHIRAndStartWithSHPNumber));

			var coloadShipment1Number1 = coloadShipment1.Numbers.AddNew();
			coloadShipment1Number1.CE_EntryNum = "SHP1223";
			coloadShipment1Number1.CE_EntryType = "UFO";
			AssertEquals("Related Shipments not have correct Reference Numbers", false, masterShipment.CoLoadShipments.OfType<ForwardingShipment>().Any(coloadShipment => coloadShipment.HasHIRAndStartWithSHPNumber));

			var coloadShipment2Number2 = coloadShipment1.Numbers.AddNew();
			coloadShipment2Number2.CE_EntryNum = "1223";
			coloadShipment2Number2.CE_EntryType = CustomsReferenceNumberType.eHubInterchangeReference.HIR;
			AssertEquals("Related Shipments not have correct Reference Numbers", false, masterShipment.CoLoadShipments.OfType<ForwardingShipment>().Any(coloadShipment => coloadShipment.HasHIRAndStartWithSHPNumber));

			var coloadShipment1Number = coloadShipment1.Numbers.AddNew();
			coloadShipment1Number.CE_EntryNum = "SHP1223";
			coloadShipment1Number.CE_EntryType = CustomsReferenceNumberType.eHubInterchangeReference.HIR;
			AssertEquals("Should be true", true, masterShipment.CoLoadShipments.OfType<ForwardingShipment>().Any(coloadShipment => coloadShipment.HasHIRAndStartWithSHPNumber));
		}

		#endregion

		#region Implementation

		protected override IJobInvoicingPlugIn GetNewBusinessObject()
		{
			ForwardingShipment forwardingShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			return forwardingShipment;
		}

		#endregion
	}
}
