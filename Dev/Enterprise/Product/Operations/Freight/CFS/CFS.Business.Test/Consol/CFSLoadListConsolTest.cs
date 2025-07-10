using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.CFS.Business.Testing
{
	public class CFSLoadListConsolTest : CommonConsolTest2
	{
		public void TestSetDomainContext()
		{
			var factory = new BusinessObjectFactory();

			AssertEquals("domain context has not been set", FreightDomainContext.Unspecified, factory.GetFreightDomainContext());

			_ = factory.NewWithValidTestData<CFSLoadListConsol>();

			AssertEquals("domain context has been set", FreightDomainContext.CFS, factory.GetFreightDomainContext());
		}

		public void TestLoadListIsForwardRegistered_ThenUnForwardRegistered_UnsavedForwardingTasksShouldBeDeleted()
		{
			var loadList = Factory.New<CFSLoadListConsol>();
			loadList.JK_IsForwarding = true;

			var rogueForwardingTask = Factory.New<Enterprise.Integration.Forwarding.IForwardingConsolProcessTask>() as ProcessTask;
			rogueForwardingTask.P9_ParentID = loadList.PK;
			rogueForwardingTask.P9_ParentTableCode = loadList.TablePrefix;

			loadList.JK_IsForwarding = false;
			AssertNoExceptionThrown(() =>
			{
				(loadList as IWorkflowProvider).WorkflowItems.Load();
			});

			AssertEquals("Unsaved forwarding task should have been deleted", true, rogueForwardingTask.IsDeleted);
		}

		public void TestLoadListIsForwardRegistered_ProcessTasksMustNotBeCreatedOnSave()
		{
			var workflowTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			workflowTemplate.P0_ProcessType = JobInvoicingConsumerTypes.CFSLoadList.Code;

			workflowTemplate.WorkflowItems.AddNew();

			Factory.Save();

			var loadList = Factory.New<CFSLoadListConsol>();
			loadList.FillWithValidTestData();

			var workflowItems = (loadList as IWorkflowProvider).WorkflowItems;

			AssertEquals("No tasks initially", 0, workflowItems.Count);

			loadList.JK_IsForwarding = true;
			AssertEquals("Prerequisite", true, loadList.HasChanges);

			Factory.Save();

			AssertEquals("Tasks should NOT be created from the template on save", 0, workflowItems.Count);
		}

		public void TestLoadListIsForwardRegistered_RemoveAllCFSProcessTasks()
		{
			var loadList = Factory.New<CFSLoadListConsol>();
			loadList.FillWithValidTestData();

			var workflowItems = (loadList as IWorkflowProvider).WorkflowItems;
			var processTask = workflowItems.AddNew();

			Factory.Save();

			loadList.JK_IsForwarding = true;

			Assert("Prerequisite", processTask.IsDeleted);
			AssertEquals("Delete all CFS load list process tasks when CFS load list becomes a dual(forwarding consol) job", 0, workflowItems.Count);
		}

		public void TestDepotAddressIsUnpackForDomesticLoadList()
		{
			var loadList = Factory.New<CFSLoadListConsol>();
			loadList.JK_RL_NKLoadPort = "AUMEL";
			loadList.JK_RL_NKDischargePort = "AUSYD";

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			loadList.JK_OA_UnpackDepotAddress = org1.MainAddress.PK;
			AssertEquals(org1.MainAddress.PK, loadList.JK_OA_DepotAddress);
			AssertEquals(org1.PK, loadList.DepotPK);

			loadList.JK_OA_DepotAddress = org2.MainAddress.PK;
			AssertEquals(org2.MainAddress.PK, loadList.JK_OA_UnpackDepotAddress);

			loadList.DepotPK = org1.PK;
			AssertEquals(org1.MainAddress.PK, loadList.JK_OA_UnpackDepotAddress);
		}

		public void TestWayBillNumberAndOrderBillNumber()
		{
			var loadList = Factory.New<CFSLoadListConsol>();
			var iLoadList = (ICartageParent)loadList;
			AssertEquals("iLoadList.OrderReferenceNumber", "", iLoadList.OrderReferenceNumber);
			AssertEquals("iLoadList.WayBillNumber)", "", iLoadList.WayBillNumber);

			loadList.JK_AgentsReference = "ORDERREFERENCENUMBER";
			loadList.JK_MasterBillNum = "WAYBILLNUMBER";

			AssertEquals("iLoadList.OrderReferenceNumber", "ORDERREFERENCENUMBER", iLoadList.OrderReferenceNumber);
			AssertEquals("iLoadList.WayBillNumber", "WAYBILLNUMBER", iLoadList.WayBillNumber);
		}

		public void TestParentCartageUniqueIndex()
		{
			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			cartage.JJ_JX_Sailing = this.SydLaxSector.PK;
			cartage.LocalClientAddressPK = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			var container1 = cartage.ContainerBookedMoves.AddNew().Container;
			var container2 = cartage.ContainerBookedMoves.AddNew().Container;
			container1.JC_ContainerNum = "CONT1";
			container2.JC_ContainerNum = "CONT2";

			var cartageJobHeader = new JobHeader.Loader(cartage).TryLoadOrCreateWithMutex();
			cartage.JJ_ConsignmentID = "T11112222";

			Factory.Save();

			var loadList1Factory = new BusinessObjectFactory();
			var loadList2Factory = new BusinessObjectFactory();
			var loadList1 = loadList1Factory.New<CFSLoadListConsol>();
			var loadList2 = loadList2Factory.New<CFSLoadListConsol>();
			loadList1.PopulateFromCartage(cartage, new CommonContainer[] { container1 });
			loadList2.PopulateFromCartage(cartage, new CommonContainer[] { container2 });

			var uniqueIndexQuery = new ZQuery(JobConsolSchema.JK_UniqueConsignRef, SQLComparisonOperator.StartsWith, "T11112222/L");
			CFSLoadListConsol[] loadLists = loadList2Factory.Load<CFSLoadListConsol>(uniqueIndexQuery);

			using (GetFactoryIsolater(loadList1Factory))
			using (GetFactoryIsolater(loadList2Factory))
			{
				loadList1Factory.Save();
				AssertEquals("Should be T11112222/L1", "T11112222/L1", loadList1.JK_UniqueConsignRef);
				var container1InNewFactory = loadList1Factory.Load<CFSContainer>(container1.PK);
				Assert(container1InNewFactory.JC_IsCFSRegistered);
			}

			try
			{
				loadList2Factory.Save();
				Fail("Save Should have failed");
			}
			catch (ZSaveException e)
			{
				ZExceptionReporting.HandleSaveException(e);
				NumberFountainUniqueIndexFailureHandlingTest.DeleteAddedRecords(loadList2Factory);
				loadList2Factory.Save();
			}

			AssertEquals("Should be T11112222/L2", "T11112222/L2", loadList2.JK_UniqueConsignRef);
			var container2InNewFactory = loadList2Factory.Load<CFSContainer>(container2.PK);
			Assert(container2InNewFactory.JC_IsCFSRegistered);
		}

		public void TestBillingModuleIsNotReadOnly()
		{
			var loadList1 = (CFSLoadListConsol)GetNewConsol();
			Assert(!((IJobInvoicingPlugIn)loadList1).InvoicingSupporter.IsPlugInReadOnly);
		}

		public void TestMainTransport()
		{
			var loadList = (CFSLoadListConsol)GetNewConsol();
			AssertNotNull(loadList.Transports.MostInterestingTransport);
			AssertEquals(loadList.Transports.MostInterestingTransport, loadList.MainTransport);
		}

		public void TestDefaultValues()
		{
			var loadList = (CFSLoadListConsol)GetNewConsol();
			AssertEquals("nothing by default", ZGuid.Empty, loadList.SendingForwarderPK);
			AssertEquals("nothing by default", ZGuid.Empty, loadList.ReceivingForwarderPK);
			AssertEquals("JK_IsCFS", true, loadList.JK_IsCFS);
			AssertEquals("JK_IsForwarding", false, loadList.JK_IsForwarding);
		}

		public void TestForwarderFieldUpdatesFromPackToUnPack()
		{
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUBNE";
			var forwarder = Factory.NewWithValidTestData<OrgHeader>();
			forwarder.OH_IsForwarder = true;

			var loadList = (CFSLoadListConsol)GetNewConsol();
			loadList.JK_OH_Forwarder = forwarder.PK;

			loadList.JK_RL_NKLoadPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			loadList.JK_RL_NKDischargePort = "AUPER";
			loadList.RunPreSaveValidation();
			Factory.Save();

			loadList.JK_RL_NKLoadPort = "AUSYD";
			AssertEquals(forwarder.PK, loadList.JK_OH_Forwarder);
		}

		public void TestForwardRegistredFlagOnChildShipments()
		{
			var loadList = (CFSLoadListConsol)GetNewConsol();
			loadList.JK_TransportMode = Constants.TransportModes.Sea;
			loadList.Transports[0].JW_JX = ImportSailing1.PK;
			var forwarder = Factory.New<TestLocalSendingForwarder>();
			forwarder.OH_IsForwarder = true;
			loadList.JK_OA_ReceivingForwarderAddress = forwarder.MainAddress.PK;
			var shipment = loadList.Shipments.AddNew();
			shipment.ConsigneePK = CreateTestLocalConsignee().PK;
			Factory.Save();
			Assert("Shipment number should start with H", shipment.JS_UniqueConsignRef.StartsWith("H"));
		}

		protected override CommonConsol GetNewConsol()
		{
			return Factory.New<CFSLoadListConsol>();
		}

		#region Proxied Properties Tests

		public void TestJK_OH_Forwarder()
		{
			SailingsForTestClasses helper = new SailingsForTestClasses(Factory);

			var branch1 = Factory.Load<GlbBranch>(GlbCompany.CurrentCompany.Branches[0].PK);
			var org1 = Factory.New<OrgHeader>();
			org1.OH_FullName = "Test Org 1";
			org1.MainAddress.OA_Address1 = "Test Address 1";
			org1.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			branch1.GB_OH_OrgProxy = org1.PK;

			var org2 = Factory.New<OrgHeader>();
			org2.OH_FullName = "Test Org 2";
			org2.MainAddress.OA_Address1 = "Test Address 2";
			org2.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			var consol = Factory.New<CFSLoadListConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.Transports[0].JW_JX = helper.SydLaxSailing.PK;
			var shipment = consol.Shipments.AddNew();

			Assert("Not expecting Load List to be Forward Registered.", !consol.JK_IsForwarding);
			Assert("Not expecting Shipment to be Forward Registered.", !shipment.JS_IsForwardRegistered);

			consol.JK_OH_Forwarder = org2.PK;

			Assert("Not expecting Load List to be Forward Registered.", !consol.JK_IsForwarding);
			Assert("Not expecting Shipment to be Forward Registered.", !shipment.JS_IsForwardRegistered);

			consol.JK_OH_Forwarder = org1.PK;

			Assert("Expecting Load List to be Forward Registered - client is a proxy org.", consol.JK_IsForwarding);
			Assert("Expecting Shipment to be Forward Registered.", shipment.JS_IsForwardRegistered);
		}

		CFSLoadListConsol temporaryLoadList;

		public void TestJK_OH_ForwarderSetViaTrigger_ShouldNotRaiseException()
		{
			ErrorReporter.Clear();
			var client = Factory.New<OrgHeader>();
			client.OH_FullName = "Test Org 1";
			client.MainAddress.OA_Address1 = "Test Address 1";
			client.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			temporaryLoadList = Factory.NewWithValidTestData<CFSLoadListConsol>();
			temporaryLoadList.Shipments.AddNew().OuterPackLines.AddNew().Containers.Add(temporaryLoadList.Containers.AddNew());

			var trigger = temporaryLoadList.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Trigger";
			trigger.TriggerConditions.TriggerEventCode = Events.EditedARecord.Code;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			action.PQ_FieldName = "<JK_OH_Forwarder>";
			action.PQ_FieldValue = "<JK_OA_ShippingLineAddress_ZAddress.OrgPK>";
			Factory.Save();

			temporaryLoadList.WarningMessageInfo.ValueChanged += WarningMessageInfo_ValueChanged;
			temporaryLoadList.JK_OA_ShippingLineAddress = client.MainAddress.PK;
			Factory.Save();

			AssertEquals(0, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
		}

		void WarningMessageInfo_ValueChanged(object sender, EventArgs e)
		{
			ErrorReporter.ReportOnce("ZFormCreatingStrategy.NotifyOpeningFormDuringDbTransactions", "Opening a form during a transaction");
			temporaryLoadList.ContinueWithChanging = false;
		}

		public void TestIsForwardingWhenInDatabase()
		{
			FreightDataRegistry.Instance.DefaultShipmentDestinationFromConsolDischarge.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			FreightDataRegistry.Instance.DefaultShipmentOriginFromConsolLoad.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var nonProxyOrg = Factory.New<OrgHeader>();
			nonProxyOrg.OH_FullName = "Test Org 2";
			nonProxyOrg.MainAddress.OA_Address1 = "Test Address 2";
			nonProxyOrg.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			var otherForwarder = Factory.New<OrgHeader>();
			otherForwarder.OH_FullName = "Other Forwarder";
			otherForwarder.MainAddress.OA_Address1 = "Other Forwarder Address";
			otherForwarder.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			var consol = (CFSLoadListConsol)GetImportConsol(typeof(CFSLoadListConsol));
			Factory.Save();
			AssertEquals("Load list should not be forward registered", false, consol.JK_IsForwarding);
			consol.JK_OH_Forwarder = nonProxyOrg.PK;
			var shipment = consol.Shipments.AddNew();
			var shipment2 = consol.Shipments.AddNew();
			shipment.JS_OH_HandledOnBehalfOfForwarder = otherForwarder.PK;
			Factory.Save();
			consol.JK_OH_Forwarder = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			AssertEquals("Setting the client on the Consol should not override the Changed client on the shripment",
				otherForwarder.PK, shipment.JS_OH_HandledOnBehalfOfForwarder);

			AssertEquals("Not expecting Shipment to be Forward Registered.", false, shipment.JS_IsForwardRegistered);
			AssertEquals("Shipment 2 should be forward registered.", true, shipment2.JS_IsForwardRegistered);
			shipment.JS_OH_HandledOnBehalfOfForwarder = ZGuid.Empty;
			AssertNotEquals("Clearing the client on the shipment should not default to the consols client", consol.JK_OH_Forwarder, shipment.JS_OH_HandledOnBehalfOfForwarder);
			AssertEquals("Not expecting Shipment to be Forward Registered.", false, shipment.JS_IsForwardRegistered);
			AssertEquals("Shipment 2 should be forward registered.", true, shipment2.JS_IsForwardRegistered);
			shipment.JS_OH_HandledOnBehalfOfForwarder = otherForwarder.PK;
			AssertEquals("Not expecting Shipment to be Forward Registered.", false, shipment.JS_IsForwardRegistered);
			AssertEquals("Shipment 2 should be forward registered.", true, shipment2.JS_IsForwardRegistered);
			Factory.Save();
			AssertEquals("Not expecting Shipment to be Forward Registered.", false, shipment.JS_IsForwardRegistered);
			AssertEquals("Shipment 2 should be forward registered.", true, shipment2.JS_IsForwardRegistered);
			shipment.JS_OH_HandledOnBehalfOfForwarder = consol.JK_OH_Forwarder;
			Factory.Save();
			AssertEquals("Shipment 1 should be Forward Registered.", true, shipment.JS_IsForwardRegistered);
			AssertEquals("Shipment 2 should be forward registered.", true, shipment2.JS_IsForwardRegistered);
		}

		public void TestJK_OA_CartageCoAddress()
		{
			var consol = Factory.New<CFSLoadListConsol>();
			consol.Transports[0].JW_JX = ExportSailing1.PK;
			consol.CartageCoPK = LocalLocalTransportCo.PK;

			AssertEquals("Expecting JK_OH_DeparturePackCFSTransport to be set.", LocalLocalTransportCo.PK, consol.DeparturePackCFSTransportPK);
			Assert("Expecting JK_OH_ArrivalUnpackCFSTransport to be empty.", consol.ArrivalUnpackCFSTransportPK.IsEmpty);

			consol.CartageCoPK = ZGuid.Empty;

			consol.Transports[0].JW_JX = ImportSailing1.PK;
			consol.CartageCoPK = LocalLocalTransportCo.PK;

			AssertEquals("Expecting JK_OH_ArrivalUnpackCFSTransport to be set.", LocalLocalTransportCo.PK, consol.ArrivalUnpackCFSTransportPK);
			Assert("Expecting JK_OH_DeparturePackCFSTransport to be empty.", consol.DeparturePackCFSTransportPK.IsEmpty);

			consol.CartageCoPK = ZGuid.Empty;

			consol.Transports[0].JW_RL_NKLoadPort = HomePort;
			consol.Transports[0].JW_RL_NKDiscPort = AlternateHomePort;
			consol.CartageCoPK = LocalLocalTransportCo.PK;

			AssertEquals("Expecting JK_OH_DeparturePackCFSTransport to be set.", LocalLocalTransportCo.PK, consol.DeparturePackCFSTransportPK);
			Assert("Expecting JK_OH_ArrivalUnpackCFSTransport to be empty.", consol.ArrivalUnpackCFSTransportPK.IsEmpty);

			consol.JK_OA_CartageCoAddress = ZGuid.Empty;

			consol.Transports[0].JW_RL_NKLoadPort = AlternateHomePort;
			consol.Transports[0].JW_RL_NKDiscPort = HomePort;
			consol.CartageCoPK = LocalLocalTransportCo.PK;

			AssertEquals("Expecting JK_OH_ArrivalUnpackCFSTransport to be set.", LocalLocalTransportCo.PK, consol.ArrivalUnpackCFSTransportPK);
			Assert("Expecting JK_OH_DeparturePackCFSTransport to be empty.", consol.DeparturePackCFSTransportPK.IsEmpty);
		}

		public void TestDepotPK()
		{
			var consol = Factory.New<CFSLoadListConsol>();
			var ownOrg = Factory.Load<OrgHeader>(GlbBranch.CurrentBranch.GB_OH_OrgProxy);

			AssertEquals("Should be read only", true, consol.DepotPKInfo.ReadOnly);
			AssertEquals("Should be read only", true, consol.JK_OA_DepotAddressInfo.ReadOnly);
			AssertEquals("Pack Depot Address should be empty", ZGuid.Empty, consol.JK_OA_PackDepotAddress);
			AssertEquals("Unpack Depot Address should be empty", ZGuid.Empty, consol.JK_OA_UnpackDepotAddress);

			consol.JK_OH_Forwarder = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			consol.JK_RL_NKLoadPort = "AUBNE";
			consol.JK_RL_NKDischargePort = "SGSIN";

			AssertEquals("Should be writeable", false, consol.DepotPKInfo.ReadOnly);
			AssertEquals("Should be writeable", false, consol.JK_OA_DepotAddressInfo.ReadOnly);
			AssertEquals("Pack depot defaults to current company", ownOrg.Addresses[0].PK, consol.JK_OA_PackDepotAddress);
			AssertEquals("Unpack depot address is empty", ZGuid.Empty, consol.JK_OA_UnpackDepotAddress);

			var depot = Factory.NewWithValidTestData<OrgHeader>();
			var pickupAddress = depot.Addresses.AddNew();
			pickupAddress.OA_Address1 = "1 Pickup St";
			pickupAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Pickup);
			var deliveryAddress = depot.Addresses.AddNew();
			deliveryAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Delivery);
			deliveryAddress.OA_Address1 = "1 Delivery St";

			Factory.Save();

			consol.DepotPK = depot.PK;

			AssertEquals("Export Consol Depot saved to Pack Depot Delivery address", deliveryAddress.PK, consol.JK_OA_PackDepotAddress);
			AssertEquals("Export Consol Depot Unpack Depot address empty", ZGuid.Empty, consol.JK_OA_UnpackDepotAddress);

			consol.JK_RL_NKLoadPort = "SGSIN";
			consol.JK_RL_NKDischargePort = "AUBNE";

			AssertEquals("Unpack depot defaults to current company", ownOrg.Addresses[0].PK, consol.JK_OA_UnpackDepotAddress);
			AssertEquals("Pack depot address is empty", ZGuid.Empty, consol.JK_OA_PackDepotAddress);

			consol.DepotPK = depot.PK;

			AssertEquals("Import Consol Depot saved to Unpack Depot Pickup address", pickupAddress.PK, consol.JK_OA_UnpackDepotAddress);
			AssertEquals("Import Consol Depot Pack Depot address empty", ZGuid.Empty, consol.JK_OA_PackDepotAddress);
		}

		#endregion

		#region IJobInvoicingPlugin Tests

		public void TestPluginProperties()
		{
			var loadList = Factory.New<CFSLoadListConsol>();
			var jobInvoicingPlugin = (IJobInvoicingPlugIn)loadList;

			AssertEquals("IJobInvoicingPlugIn.CreateAccountingJobOnSavingOfOperationsJob",
				true, jobInvoicingPlugin.InvoicingSupporter.CreateAccountingJobOnSavingOfOperationsJob);

			AssertNull("IJobInvoicingPlugIn.OperationsBranch", jobInvoicingPlugin.InvoicingSupporter.OperationsBranch);
			AssertEquals("AuditSecurity", Env.Security.CFSLoadListAuditBilling, jobInvoicingPlugin.InvoicingSupporter.AuditSecurity);
		}

		public void TestConsignorAndConsigneeAreTheSameAsClient()
		{
			var loadList = (CFSLoadListConsol)GetNewConsol();
			AssertNull("Consignor", loadList.InvoicingSupporter.Consignor);
			AssertNull("Consignee", loadList.InvoicingSupporter.Consignee);
			loadList.JK_OH_Forwarder = OrgHeader.New(Factory).PK;
			AssertEquals("Consignor", loadList.JK_OH_Forwarder, loadList.InvoicingSupporter.Consignor.PK);
			AssertEquals("Consignee", loadList.JK_OH_Forwarder, loadList.InvoicingSupporter.Consignee.PK);
		}

		public void TestEditSecurity()
		{
			IJobInvoicingPlugIn testJob = (IJobInvoicingPlugIn)GetNewConsol();
			AssertEquals("EditSecurityCheckPoint should be Empty", Env.Security.None, testJob.InvoicingSupporter.EditSecurityCheckpoint);
			AssertEquals("EditSecuritytMessage should be Empty", ZString.Empty, testJob.InvoicingSupporter.EditSecurityMessage);
			AssertEquals("EditSecurityLock should be False", ZBool.False, testJob.InvoicingSupporter.EditSecurityLock);
		}

		public void TestIJobInvoicingPlugInDefaultChargeGroup()
		{
			IJobInvoicingPlugIn testJob = (IJobInvoicingPlugIn)GetNewConsol();
			AssertEquals("DefaultChargeGroup should be Empty", ZString.Empty, testJob.InvoicingSupporter.DefaultChargeGroup);
		}

		#endregion

		#region Canada Specific Tests

		public void TestCanadaNumbers()
		{
			var loadList = (CFSLoadListConsol)GetNewConsol();

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Canada);
			Factory.Save();

			AssertEquals(ZString.Empty, loadList.CanadaCCNNumber);

			CusEntryNumber num1 = loadList.Numbers.AddNew();
			num1.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			num1.CE_EntryNum = "1";

			CusEntryNumber num2 = loadList.Numbers.AddNew();
			num2.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.PCN;
			num2.CE_EntryNum = "2";

			Factory.Save();

			AssertEquals("CanadaCCNNumber", "1", loadList.CanadaCCNNumber);
			AssertEquals("CanadaPCNNumber", "2", loadList.CanadaPCNNumber);

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.China);

			AssertEquals("CanadaCCNNumber should be empty", ZString.Empty, loadList.CanadaCCNNumber);
			AssertEquals("CanadaCCNNumber should be empty", ZString.Empty, loadList.CanadaPCNNumber);
		}

		#endregion

		#region IJobHeaderParent Members

		public void TestIJobHeaderParent_AllowInvoiceDeletion()
		{
			IJobHeaderParent cfsLoadListConsol = Factory.New<CFSLoadListConsol>();
			Assert(cfsLoadListConsol.AllowInvoiceDeletion);
		}

		#endregion

		#region IRatingSupporter

		public void TestGetAdaptersProviderReturnsCFSLoadListConsolRatingAdaptersProvider()
		{
			var loadList = Factory.New<CFSLoadListConsol>();
			AssertType(typeof(CFSLoadListConsolRatingAdaptersProvider<CFSLoadListConsol>), ((IRatingSupporter)loadList).AdaptersProvider);
		}

		public void TestAdapterTypeAndId()
		{
			var loadList = (CFSLoadListConsol)GetNewConsol();
			AssertEquals(AdapterType.LoadList, loadList.RatingAdapter.AdapterType);
			AssertEquals(loadList.JK_UniqueConsignRef + " Route 1", loadList.RatingAdapter.OperationalJobCode);
			AssertEquals(loadList.JK_UniqueConsignRef, loadList.RatingAdapter.JobID);
		}

		#endregion

		[ExpectNoExceptions]
		public void TestGetParentCartage_OnDeletedJob()
		{
			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			cartage.JJ_JX_Sailing = this.SydLaxSector.PK;
			cartage.LocalClientAddressPK = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			var container1 = cartage.ContainerBookedMoves.AddNew().Container;
			var container2 = cartage.ContainerBookedMoves.AddNew().Container;
			container1.JC_ContainerNum = "CONT1";
			container2.JC_ContainerNum = "CONT2";

			var cartageJobHeader = new JobHeader.Loader(cartage).TryLoadOrCreateWithMutex();
			cartage.JJ_ConsignmentID = "T11112222";

			Factory.Save();

			var loadList1Factory = new BusinessObjectFactory();
			var loadList2Factory = new BusinessObjectFactory();
			var loadList1 = loadList1Factory.New<CFSLoadListConsol>();
			var loadList2 = loadList2Factory.New<CFSLoadListConsol>();
			loadList1.PopulateFromCartage(cartage, new CommonContainer[] { container1 });
			loadList2.PopulateFromCartage(cartage, new CommonContainer[] { container2 });

			var uniqueIndexQuery = new ZQuery(JobConsolSchema.JK_UniqueConsignRef, SQLComparisonOperator.StartsWith, "T11112222/L");
			CFSLoadListConsol[] loadLists = loadList2Factory.Load<CFSLoadListConsol>(uniqueIndexQuery);

			loadList1.Job.Delete();
			loadList2.Job.Delete();

			using (GetFactoryIsolater(loadList1Factory))
			using (GetFactoryIsolater(loadList2Factory))
			{
				loadList1Factory.Save();
			}
		}

		public void TestEnableJobHeaderNumberChangeContext()
		{
			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			cartage.JJ_JX_Sailing = this.SydLaxSector.PK;
			cartage.LocalClientAddressPK = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			var container = cartage.ContainerBookedMoves.AddNew().Container;
			container.JC_ContainerNum = "CONT1";

			var cartageJobHeader = new JobHeader.Loader(cartage).TryLoadOrCreateWithMutex();
			cartage.JJ_ConsignmentID = "T11112222";
			Factory.Save();

			var loadList = Factory.New<CFSLoadListConsol>();
			loadList.PopulateFromCartage(cartage, new CommonContainer[] { container });
			Factory.Save();

			loadList.PopulateJK_UniqueConsignRefIfNeeded();
			AssertEquals("Factory has EnableJobHeaderNumberChange context", true, Factory.HasContext(BusinessContext.EnableJobHeaderNumberChange));

			Factory.Save();
			AssertEquals("Factory has no EnableJobHeaderNumberChange context", false, Factory.HasContext(BusinessContext.EnableJobHeaderNumberChange));
		}
	}
}
