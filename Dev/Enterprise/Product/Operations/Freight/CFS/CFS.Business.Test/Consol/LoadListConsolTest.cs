using System;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.CFS.Business.Testing
{
	public class LoadListConsolTest : BaseFreightTest
	{
		#region Related Business Objects Tests

		public void TestShipments()
		{
			CFSLoadListConsol consol = Factory.New<CFSLoadListConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;

			Transport transport = consol.Transports[0];
			transport.JW_JX = new ConstantsAndReusables(Factory).CreateNewSailing(false).PK;
			consol.JK_RL_NKLoadPort = transport.JW_RL_NKLoadPort;
			consol.JK_RL_NKDischargePort = transport.JW_RL_NKDiscPort;

			CFSShipment s1 = consol.Shipments.AddNew();
			s1.JS_InterimReceipt = "7";
			Factory.Save();

			CFSShipment s2 = consol.Shipments.AddNew();
			s2.JS_InterimReceipt = "23";
			Factory.Save();

			CFSShipment s3 = consol.Shipments.AddNew();
			s3.JS_InterimReceipt = "55";
			Factory.Save();

			CFSShipment s4 = consol.Shipments.AddNew();
			s4.JS_InterimReceipt = "1234";
			Factory.Save();

			BusinessObjectFactory factory3 = new BusinessObjectFactory();
			CFSLoadListConsol loadedConsol = factory3.Load<CFSLoadListConsol>(consol.PK);

			AssertEquals("Precondition: Expecting Consol to have 4 shipments.", 4, loadedConsol.Shipments.Count);

			AssertEquals("Consol2 should be Export Consol", true, loadedConsol.IsExport());

			// Sort by InterimReceipt for Exports
			AssertEquals("Expecting shipment 4 to be first in the list.", s4.PK, loadedConsol.Shipments[0].PK);
			AssertEquals("Expecting shipment 2 to be second in the list.", s2.PK, loadedConsol.Shipments[1].PK);
			AssertEquals("Expecting shipment 3 to be third in the list.", s3.PK, loadedConsol.Shipments[2].PK);
			AssertEquals("Expecting shipment 1 to be fourth in the list.", s1.PK, loadedConsol.Shipments[3].PK);

			transport.JW_JX = new ConstantsAndReusables(Factory).CreateNewSailing(true).PK;
			consol.JK_RL_NKLoadPort = transport.JW_RL_NKLoadPort;
			consol.JK_RL_NKDischargePort = transport.JW_RL_NKDiscPort;

			Factory.Save();

			BusinessObjectFactory factory4 = new BusinessObjectFactory();
			CFSLoadListConsol loadedConsol2 = factory4.Load<CFSLoadListConsol>(consol.PK);
			AssertEquals("Consol2 should be Import Consol", true, loadedConsol2.IsImport());

			// Sort by UniqueConsignRef for imports
			AssertEquals("Expecting shipment 1 to be first in the list.", s1.PK, loadedConsol2.Shipments[0].PK);
			AssertEquals("Expecting shipment 2 to be second in the list.", s2.PK, loadedConsol2.Shipments[1].PK);
			AssertEquals("Expecting shipment 3 to be third in the list.", s3.PK, loadedConsol2.Shipments[2].PK);
			AssertEquals("Expecting shipment 4 to be fourth in the list.", s4.PK, loadedConsol2.Shipments[3].PK);
		}

		#endregion

		public void TestCabotageDoesNotClearClient()
		{
			OrgHeader forwarder = Factory.New<OrgHeader>();
			forwarder.OH_IsForwarder = true;
			forwarder.OH_RL_NKClosestPort = HomePort;
			forwarder.OH_FullName = "AUSSIE FORWARDERS";

			CFSLoadListConsol consol = Factory.New<CFSLoadListConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_OH_Forwarder = forwarder.PK;
			AssertEquals("Loadlist client should be forwarder.", forwarder.PK, consol.JK_OH_Forwarder);

			AssertEquals("", consol.JK_RL_NKLoadPort);
			AssertEquals("", consol.JK_RL_NKDischargePort);
			AssertEquals(true, consol.IsUnknown());

			consol.JK_RL_NKLoadPort = "AUBNE";
			AssertEquals(true, consol.IsExport());
			AssertEquals("AUBNE", consol.JK_RL_NKLoadPort);
			AssertEquals("", consol.JK_RL_NKDischargePort);
			AssertEquals("Loadlist client should be forwarder.", forwarder.PK, consol.JK_OH_Forwarder);

			consol.JK_RL_NKDischargePort = "AUSYD";
			AssertEquals("Loadlist client should be forwarder.", forwarder.PK, consol.JK_OH_Forwarder);
		}

		public void TestFillEmptyContainerClient()
		{
			CFSContainer container = LoadList.Containers.AddNew();
			LoadList.JK_OH_Forwarder = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			container.JC_OH_CFSClient = ZGuid.Empty;
			Factory.Save();
			AssertEquals("should have been defaulted from load list", false, container.JC_OH_CFSClient.IsEmpty);
		}

		public void TestCTOAddressDefaultFromSailing()
		{
			AssertEquals("precondition", ZGuid.Empty, LoadList.JK_OA_CTOAddress);

			OrgHeader org1 = Factory.New<OrgHeader>();
			org1.OH_FullName = "Test Org 1";
			org1.MainAddress.OA_Address1 = "Test Address 1";
			org1.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			OrgAddress address1 = org1.Addresses.AddNew();
			address1.OA_Address1 = "Departure Address";

			OrgHeader org2 = Factory.New<OrgHeader>();
			org2.OH_FullName = "Test Org 2";
			org2.MainAddress.OA_Address1 = "Test Address 2";
			org2.OH_RL_NKClosestPort = "USLAX";
			OrgAddress address2 = org2.Addresses.AddNew();
			address2.OA_Address1 = "Arrival CTO Address";

			JobSailing sailing = Helper.SydLaxSailing;
			sailing.Origin.JA_OA_DepartureCTOAddress = address1.PK;
			sailing.Destination.JB_OA_ArrivalCTOAddress = address2.PK;

			LoadList.JK_RL_NKLoadPort = "AUSYD";
			LoadList.JK_RL_NKDischargePort = "USLAX";
			LoadList.JK_TransportMode = Constants.TransportModes.Sea;
			LoadList.Transports[0].JW_JX = Helper.SydLaxSailing.PK;
			if (ImportExportHelper.IsBranchCountry(LoadList.JK_RL_NKLoadPort))
			{
				AssertEquals("Expecting CTO addresses to be copied from Sailing.Origin", Helper.SydLaxSailing.Origin.JA_OA_DepartureCTOAddress, LoadList.JK_OA_CTOAddress);
			}
			else
			{
				AssertEquals("Expecting CTO addresses to be copied from Sailing.Destination", Helper.SydLaxSailing.Destination.JB_OA_ArrivalCTOAddress, LoadList.JK_OA_CTOAddress);
			}
		}

		public void TestJK_IsCFS()
		{
			var loadList = Factory.New<CFSLoadListConsol>();
			var shipmentA = loadList.Shipments.AddNew();
			var packA1 = shipmentA.OuterPackLines.AddNew();
			var packA2 = shipmentA.OuterPackLines.AddNew();
			var containerA = loadList.Containers.AddNew();

			packA1.SetContainer(containerA.PK);
			packA2.SetContainer(containerA.PK);

			var shipmentB = loadList.Shipments.AddNew();
			var packB1 = shipmentB.OuterPackLines.AddNew();
			var packB2 = shipmentB.OuterPackLines.AddNew();
			var containerB = loadList.Containers.AddNew();

			packB1.SetContainer(containerB.PK);
			packB2.SetContainer(containerB.PK);

			var shipmentC = loadList.Shipments.AddNew();
			var packC1 = shipmentC.OuterPackLines.AddNew();
			var packC2 = shipmentC.OuterPackLines.AddNew();

			packC1.SetContainer(containerA.PK);
			packC2.SetContainer(containerA.PK);

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var otherContainerA = otherFactory.Load<CFSContainer>(containerA.PK);
			var otherContainerB = otherFactory.Load<CFSContainer>(containerB.PK);
			var otherConsol = otherFactory.Load<CFSLoadListConsol>(loadList.PK);
			otherConsol.JK_ConsolMode = otherConsol.JK_ConsolMode == "FCL" ? "GRP" : "FCL";
			otherFactory.Save();
			AssertEquals("should be CFS", ZBool.True, otherConsol.JK_IsCFS);
		}

		[ExpectNoExceptions]
		public void TestCFSProcessTaskLoadedAsForwardingProcessTask()
		{
			FreightConfigurationRegistry.Instance.AutoDeliverDelayAlertDocuments.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DelayAlertDeliveryRuleCollection()
			{
				new DelayAlertDeliveryRule()
			});

			SetupSailing(LoadList, true);
			LoadList.JK_IsForwarding = true;

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var forwardingConsol = factory2.Load<Enterprise.Integration.Forwarding.IForwardingConsol>(LoadList.PK);
			var trigger = ((IWorkflowProvider)forwardingConsol).WorkflowItems.Triggers.AddNew();

			trigger.TriggerConditions.TriggerEventCode = Events.Departure.Code;
			trigger.TriggerConditions.TriggerFieldName = JobConsolTransportSchema.Constants.JW_ETA;

			factory2.Save();

			LoadList.MainTransport.JW_ETA = ZDateTime.Now.AddDays(2);
			LoadList.JK_CustomsReference = "Ref00001";

			Factory.Save();
		}

		public void TestEnsureSubColoadsForwarderIsNotResetWhenLoadListClientChanges()
		{
			OrgHeader org1 = Factory.New<OrgHeader>();
			OrgHeader org2 = Factory.New<OrgHeader>();
			OrgHeader org3 = Factory.New<OrgHeader>();

			CFSLoadListConsol consol = Factory.New<CFSLoadListConsol>();
			consol.JK_OH_Forwarder = org1.PK;

			CFSShipment coloadMaster = consol.Shipments.AddNew();
			coloadMaster.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			coloadMaster.ConsignorPK = org1.PK;

			CFSShipment coloadSub1 = consol.Shipments.AddNew();
			coloadSub1.JS_JS_ColoadMasterShipment = coloadMaster.PK;
			coloadSub1.JS_OH_HandledOnBehalfOfForwarder = org2.PK;

			consol.JK_OH_Forwarder = org3.PK;

			AssertEquals("Expecting Co-loadMaster's client to update to Org3", org3.PK, coloadMaster.JS_OH_HandledOnBehalfOfForwarder);
			AssertEquals("Not expecting Co-loadMaster's client to update to Org3 - it should stay as org2", org2.PK, coloadSub1.JS_OH_HandledOnBehalfOfForwarder);
		}

		public void TestSetPackUnpackDepotAddress()
		{
			BusinessObjectFactory factory2 = new BusinessObjectFactory();

			OrgHeader org1 = factory2.New<OrgHeader>();
			org1.OH_IsForwarder = ZBool.True;
			org1.OH_Code = new ZString("AAAAA");
			org1.OH_FullName = "DSFHGERTEWER";
			org1.MainAddress.OA_Address1 = new ZString("FOO");
			org1.OH_RL_NKClosestPort = "AUBNE";
			OrgAddress address1 = org1.Addresses.AddNew();
			address1.OA_Address1 = new ZString("ABC");
			address1.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Delivery);

			OrgHeader org2 = factory2.New<OrgHeader>();
			org2.OH_IsForwarder = ZBool.True;
			org2.OH_Code = new ZString("BBBBB");
			org1.OH_FullName = "jdheo3874o2i3h4";
			org2.MainAddress.OA_Address1 = new ZString("BBB");
			org2.OH_RL_NKClosestPort = "AUBNE";
			OrgAddress address2 = org2.Addresses.AddNew();
			address2.OA_Address1 = new ZString("DEF");
			address2.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Pickup);

			factory2.Save();

			GlbBranch branch1 = factory2.Load<GlbBranch>(GlbCompany.CurrentCompany.Branches[0].PK);
			branch1.GB_OH_OrgProxy = org1.PK;

			CFSLoadListConsol consol = factory2.New<CFSLoadListConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			JobSailing sailing = new ConstantsAndReusables(factory2).CreateNewSailing(false);

			Transport transport = consol.Transports[0];
			transport.JW_JX = sailing.PK;
			consol.JK_RL_NKLoadPort = transport.JW_RL_NKLoadPort;
			consol.JK_RL_NKDischargePort = transport.JW_RL_NKDiscPort;
			consol.JK_OH_Forwarder = org1.PK;

			AssertEquals("Consol Unpack Address should be empty", ZGuid.Empty, consol.JK_OA_UnpackDepotAddress);
			AssertEquals("Consol Pack Address should not be empty", address1.PK, consol.JK_OA_PackDepotAddress);

			consol.JK_OA_PackDepotAddress = ZGuid.Empty;
			consol.JK_OA_UnpackDepotAddress = ZGuid.Empty;
			transport.JW_JX = new ConstantsAndReusables(factory2).CreateNewSailing(true).PK;
			consol.JK_RL_NKLoadPort = transport.JW_RL_NKLoadPort;
			consol.JK_RL_NKDischargePort = transport.JW_RL_NKDiscPort;
			consol.JK_OH_Forwarder = org2.PK;

			AssertEquals("Consol Pack Address should be empty", ZGuid.Empty, consol.JK_OA_PackDepotAddress);
			AssertEquals("Consol Unpack Address should not be empty", false, consol.JK_OA_UnpackDepotAddress.IsEmpty);
		}

		public void TestOrgDebtor_List()
		{
			Assert("use masterfiles collection, not make one yourself", LoadList.OrgDebtor_List is DebtorCollection);
		}

		public void TestImportLoadListSeesContainersFromFreightSystem()
		{
			ZGuid consolPK = CreateTestConsolWithOneContainerThroughFreightSystem();
			var loadList = Factory.Load<CFSLoadListConsol>(consolPK);
			AssertEquals("Load List should have brought over one container from the freight system", 1, loadList.Containers.Count);
		}

		public void TestAttachShipment()
		{
			SetupSailing(LoadList, false);
			LoadList.AutomaticallyUpdatePackLineContainers = true;
			LoadList.Shipments.RemoveAll();
			AssertEquals("Shipments.Count", 0, LoadList.Shipments.Count);
			AssertEquals("UnAllocatedPackLines.Count", 0, LoadList.UnAllocatedPackLines.Count);

			CFSShipment shipment = LoadList.Shipments.AddNew();
			AssertEquals("Shipments.Count", 1, LoadList.Shipments.Count);
			AssertEquals("OuterPackLines.Count", 0, shipment.OuterPackLines.Count);
			AssertEquals("UnAllocatedPackLines.Count", 0, LoadList.UnAllocatedPackLines.Count);

			shipment.JS_A_RCV = ZDateTime.Now;
			PackLine line = shipment.OuterPackLines.AddNew();
			AssertEquals("OuterPackLines.Count", 1, shipment.OuterPackLines.Count);
			AssertEquals("UnAllocatedPackLines.Count", 1, LoadList.UnAllocatedPackLines.Count);
			AssertEquals("UnAllocatedPackLines[0].PK", line.PK, LoadList.UnAllocatedPackLines[0].PK);
		}

		void SetupSailing(CFSLoadListConsol loadList, bool isImport)
		{
			loadList.JK_TransportMode = Constants.TransportModes.Sea;

			Transport transport = loadList.Transports[0];
			transport.JW_IsLinked = true;
			transport.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			transport.JW_JX = new ConstantsAndReusables(Factory).CreateNewSailing(isImport).PK;

			loadList.JK_RL_NKLoadPort = transport.JW_RL_NKLoadPort;
			loadList.JK_RL_NKDischargePort = transport.JW_RL_NKDiscPort;
		}

		public void TestDetachShipment()
		{
			SetupSailing(LoadList, true);
			AssertEquals("Shipments.Count", 0, LoadList.Shipments.Count);
			AssertEquals("Containers.Count", 0, LoadList.Containers.Count);

			CFSShipment shipment = LoadList.Shipments.AddNew();
			CFSContainer container = LoadList.Containers.AddNew();
			AssertEquals("Shipments.Count", 1, LoadList.Shipments.Count);
			AssertEquals("Containers.Count", 1, LoadList.Containers.Count);
			AssertEquals("PackLines.Count", 0, container.PackLines.Count);

			CFSPackLine line = shipment.OuterPackLines.AddNew();
			container.AddPackLine(line);
			AssertEquals("PackLines.Count", 1, container.PackLines.Count);

			LoadList.Shipments.Remove(shipment);
			AssertEquals("Shipments.Count", 0, LoadList.Shipments.Count);
			AssertEquals("Containers.Count", 1, LoadList.Containers.Count);
			AssertEquals("PackLines.Count", 0, container.PackLines.Count);
		}

		public void TestAttachContainer()
		{
			SetupSailing(LoadList, true);
			AssertNotNull(LoadList.Schedule);
			LoadList.JK_OH_Forwarder = GetOrCreateForwarder(LoadList.Schedule.JX_JA_RL_NKPortOfLoading).PK;

			CFSLoadListConsol loadList2 = Factory.New<CFSLoadListConsol>();
			loadList2.JK_TransportMode = LoadList.JK_TransportMode;
			loadList2.Transports[0].JW_JX = LoadList.Schedule.PK;
			loadList2.JK_OH_Forwarder = LoadList.JK_OH_Forwarder;

			CFSContainer container = Factory.New<CFSContainer>();
			container.JC_ContainerNum = "GFCU0292036";
			LoadList.Containers.Add(container);
			AssertEquals("LoadList should contain the container.", true, LoadList.Containers.Contains(container));
			AssertEquals("Client should be defaulted.", LoadList.JK_OH_Forwarder, container.JC_OH_CFSClient);

			CFSContainer container2 = Factory.New<CFSContainer>();
			container2.JC_ContainerNum = "GFCU0292036";
			loadList2.Containers.Add(container2);
			AssertEquals("LoadList2 should not contain the container as this container (same container number) is on another load list on the same sailing.", false, loadList2.Containers.Contains(container2));
			AssertEquals("LoadList should contain the container.", true, LoadList.Containers.Contains(container));
			AssertEquals("LoadList2 should not contain the container.", false, loadList2.Containers.Contains(container));

			LoadList.Containers.Remove(container);
			loadList2.Containers.Add(container);
			AssertEquals("LoadList should not contain the container.", false, LoadList.Containers.Contains(container));
			AssertEquals("LoadList2 should contain the container.", true, loadList2.Containers.Contains(container));

			container.JC_OH_CFSClient = ZGuid.NewZGuid();
			LoadList.Containers.Add(container);
			AssertEquals("LoadList should contain the container.", true, LoadList.Containers.Contains(container));
			AssertEquals("LoadList2 should not contain the container.", false, loadList2.Containers.Contains(container));
			AssertEquals("Client should be defaulted.", LoadList.JK_OH_Forwarder, container.JC_OH_CFSClient);
			AssertEquals("Sailing should be the same as on the consol.", LoadList.JK_JX_Sailing, container.JC_JX);
		}

		public void TestDetachContainer()
		{
			SetupSailing(LoadList, true);
			LoadList.Containers.RemoveAll();
			LoadList.Shipments.RemoveAll();
			AssertEquals("Containers.Count", 0, LoadList.Containers.Count);
			AssertEquals("Shipments.Count", 0, LoadList.Shipments.Count);

			CFSContainer container = LoadList.Containers.AddNew();
			CFSShipment shipment = LoadList.Shipments.AddNew();
			AssertEquals("Containers.Count", 1, LoadList.Containers.Count);
			AssertEquals("Shipments.Count", 1, LoadList.Shipments.Count);
			AssertEquals("PackLines.Count", 0, container.PackLines.Count);

			CFSPackLine line = shipment.OuterPackLines.AddNew();
			container.PackLines.Add(line);
			AssertEquals("PackLines.Count", 1, container.PackLines.Count);

			LoadList.Containers.Remove(container);
			AssertEquals("Containers.Count", 0, LoadList.Containers.Count);
			AssertEquals("Shipments.Count", 1, LoadList.Shipments.Count);
			AssertEquals("PackLines.Count", 0, container.PackLines.Count);
			AssertEquals("Container.IsDeleted", false, container.IsDeleted);
		}

		public void TestContainers_List()
		{
			OrgHeader line = Factory.LoadTop1<OrgHeader>(new ZQuery());

			SetupSailing(LoadList, true);
			LoadList.JK_OH_Forwarder = line.PK;

			CFSContainer container = Factory.New<CFSContainer>();
			container.JC_ContainerMode = LoadList.JK_ConsolMode;
			container.JC_OH_CFSClient = LoadList.JK_OH_Forwarder;
			container.JC_JX = LoadList.Schedule.PK;

			CFSContainer container2 = Factory.New<CFSContainer>();

			ZQuery filter = new ZQuery(JobContainerSchema.JC_ContainerMode, LoadList.JK_ConsolMode);
			filter.AddToFilter(JobContainerSchema.JC_OH_CFSClient, LoadList.JK_OH_Forwarder);
			filter.AddToFilter(JobContainerSchema.JC_JX, LoadList.JK_JX_Sailing);

			CFSContainer container3 = Factory.New<CFSContainer>();
			container3.JC_Purpose = ContainerPurposeTypeCodeDescriptionPairList.Codes.Storage;

			CFSContainer container4 = Factory.New<CFSContainer>();
			container4.JC_Purpose = ContainerPurposeTypeCodeDescriptionPairList.Codes.CFS;

			LoadList.Containers_List.Load(filter);
			AssertEquals(true, LoadList.Containers_List.Contains(container));
			AssertEquals(false, LoadList.Containers_List.Contains(container2));
			AssertEquals(false, LoadList.Containers_List.Contains(container3));
			AssertEquals(false, LoadList.Containers_List.Contains(container4));
		}

		public void TestPackLines()
		{
			ZGuid consolPK = CreateTestConsolWithOneContainerThroughFreightSystem();
			var loadList = Factory.Load<CFSLoadListConsol>(consolPK);
			CFSShipment shipment = Factory.New<CFSShipment>();
			PackLine line = shipment.OuterPackLines.AddNew();
			loadList.Shipments.Add(shipment);

			IPackLineInfo[] packs = ((IHaveInternalCartage)loadList).GetPackLines();
			AssertEquals("GetPackLines", 1, packs.Length);
			AssertSame(line, packs[0]);
		}

		public void TestRefreshRelatedPackLines()
		{
			ZGuid consolPK = CreateTestConsolWithOneContainerThroughFreightSystem();
			var loadList = Factory.Load<CFSLoadListConsol>(consolPK);

			AssertEquals("Shipments.Count == 0", 0, loadList.Shipments.Count);
			AssertEquals("RelatedPackLines.Count == 0", 0, loadList.RelatedPackLines.Count);

			CFSShipment shipment = Factory.New<CFSShipment>();
			AssertEquals("OuterPackLines.Count == 0", 0, shipment.OuterPackLines.Count);

			PackLine line = shipment.OuterPackLines.AddNew();
			AssertEquals("OuterPackLines.Count == 1", 1, shipment.OuterPackLines.Count);
			AssertEquals("Shipments.Count == 0", 0, loadList.Shipments.Count);
			AssertEquals("RelatedPackLines.Count == 0", 0, loadList.RelatedPackLines.Count);

			loadList.Shipments.Add(shipment);
			AssertEquals("Shipments.Count == 1", 1, loadList.Shipments.Count);
			AssertEquals("RelatedPackLines.Count == 1", 1, loadList.RelatedPackLines.Count);

			loadList.Shipments.Remove(shipment);
			AssertEquals("Shipments.Count == 0", 0, loadList.Shipments.Count);
			AssertEquals("RelatedPackLines.Count == 0", 0, loadList.RelatedPackLines.Count);
		}

		#region Test JK_IsForwarding

		public void TestJK_IsForwarding()
		{
			var branch1 = Factory.Load<GlbBranch>(GlbCompany.CurrentCompany.Branches[0].PK);
			OrgHeader org1 = Factory.New<OrgHeader>();
			branch1.GB_OH_OrgProxy = org1.PK;

			var consol1 = Factory.New<CFSLoadListConsol>();
			consol1.JK_OH_Forwarder = org1.PK;
			AssertEquals("JK_IsForwarding should be set to true", true, consol1.JK_IsForwarding);
		}

		[ExpectNoExceptions]
		public void TestJK_IsForwardingWithoutReportOnce()
		{
			var errorReporterMock = new Mock<IErrorReporter>();
			using (Globals.TemporaryOverrideForIsTest(false))
			using (ErrorReporter.SetTemporaryInstanceForTest(errorReporterMock.Object))
			{
				var branch = Factory.Load<GlbBranch>(GlbCompany.CurrentCompany.Branches[0].PK);
				var org = Factory.NewWithValidTestData<OrgHeader>();
				branch.GB_OH_OrgProxy = org.PK;
				var shipment = Factory.NewWithValidTestData<CFSShipment>();
				shipment.JS_OH_HandledOnBehalfOfForwarder = org.PK;
				Factory.Save();

				var consol = Factory.NewWithValidTestData<CFSLoadListConsol>();
				consol.Shipments.Add(shipment);

				branch.GB_OH_OrgProxy = Guid.Empty;
				consol.JK_OH_Forwarder = org.PK;

				errorReporterMock.Verify(reporter => reporter.Report("CommonShipmentShouldNotResetJS_IsForwardRegistered", Moq.It.IsAny<string>(), Moq.It.IsAny<Exception>()), Moq.Times.Never);
			}
		}

		#endregion

		public void TestIsForwarderDefaultedFromPort()
		{
			ZGuid tempGuid = Factory.New<OrgHeader>().PK;
			LoadList.JK_TransportMode = Core.Constants.TransportModes.Sea;
			LoadList.JK_OH_Forwarder = tempGuid;

			LoadList.Transports[0].JW_JX = new ConstantsAndReusables(Factory).CreateNewSailing(true).PK;
			AssertEquals("JK_OH_Forwarder", tempGuid, LoadList.JK_OH_Forwarder);
		}

		public void TestJK_IsCFSDoesNotChangeWhenAttachShipment()
		{
			CFSLoadListConsol loadList = Factory.New<CFSLoadListConsol>();
			AssertEquals("JK_IsCFS", true, loadList.JK_IsCFS);

			SetupSailing(loadList, false);
			AssertNotNull(loadList.Schedule);
			loadList.JK_OH_Forwarder = GetOrCreateForwarder(loadList.Schedule.JX_JA_RL_NKPortOfLoading).PK;

			CFSShipment shipment = loadList.Shipments.AddNew();
			shipment.OuterPackLines.AddNew();
			Factory.Save();
			AssertEquals("JK_IsCFS", true, loadList.JK_IsCFS);
		}

		public void TestLineIsDefaultedFromVoyage()
		{
			OrgHeader line = Factory.LoadTop1<OrgHeader>(new ZQuery());

			JobSailing sailing = new ConstantsAndReusables(Factory).CreateNewSailing(false);
			sailing.Voyage.JV_OH_Line = line.PK;
			sailing.Vessel.RV_OH = ZGuid.Empty;

			CFSLoadListConsol loadList = Factory.New<CFSLoadListConsol>();
			AssertNull("Schedule should be null", loadList.Schedule);
			AssertEquals("ShippingLinePK", ZGuid.Empty, loadList.ShippingLinePK);

			loadList.JK_TransportMode = Constants.TransportModes.Sea;
			loadList.Transports[0].JW_JX = sailing.PK;

			AssertEquals("ShippingLinePK", sailing.Voyage.JV_OH_Line, loadList.ShippingLinePK);
		}

		public void TestAutomaticallyUpdatePackLineContainers()
		{
			var loadList = Factory.New<CFSLoadListConsol>();

			AssertEquals("\r\nCurrent default for automatically packing containers is false.  "
				+ "\r\nThere is a registry item that controls the default value for CFS objects."
				+ "\r\nChanging this test to get it to pass means you will need to fix unit tests outside of freight",
				false, loadList.AutomaticallyUpdatePackLineContainers);

			loadList.AutomaticallyUpdatePackLineContainers = true;

			AssertEquals("This test ensures functionality related to automatically packing containers "
				+ "\r\nRegardless of the default state of this value, it must be true for this test", true, loadList.AutomaticallyUpdatePackLineContainers);

			var container = loadList.Containers.AddNew();
			var shipment = Factory.New<CFSShipment>();
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 1;
			loadList.Shipments.Add(shipment);

			AssertEquals("Container.PackLines.Count", 1, container.PackLines.Count);
			AssertNotNull("PackLine should be assigned to Container", packLine.GetContainer(loadList));

			var loadList2 = Factory.New<CFSLoadListConsol>();
			loadList2.JK_IsForwarding = true;
			loadList2.AutomaticallyUpdatePackLineContainers = true;

			var container2 = loadList2.Containers.AddNew();
			var shipment2 = loadList2.Shipments.AddNew();
			var packLine2 = shipment2.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 1;
			loadList2.Shipments.Add(shipment2);

			AssertEquals("Container2.PackLines.Count", 1, container2.PackLines.Count);
			AssertEquals("Container2.PackLines[0]", packLine2, container2.PackLines[0]);
			AssertEquals("PackLine2 should be assigned to Container2", container2.PK, packLine2.GetContainer(loadList2).PK);
		}

		public void TestSetWarnNotErrorOnLocationTotalsOnPackLines()
		{
			CFSShipment shipment = LoadList.Shipments.AddNew();
			CFSPackLine pack = shipment.OuterPackLines.AddNew();
			AssertEquals("precondition", false, pack.WarnNotErrorOnLocationTotals);
			LoadList.SetWarnNotErrorOnLocationTotalsOnPackLines(true);
			AssertEquals("should have changed", true, pack.WarnNotErrorOnLocationTotals);
			LoadList.SetWarnNotErrorOnLocationTotalsOnPackLines(false);
			AssertEquals("should have changed", false, pack.WarnNotErrorOnLocationTotals);
		}

		public void TestWarnNotErrorOnLocationTotalsDefaults()
		{
			CFSShipment shipment = LoadList.Shipments.AddNew();
			CFSPackLine pack = shipment.OuterPackLines.AddNew();
			AssertEquals("should be false by default", false, pack.WarnNotErrorOnLocationTotals);
		}

		[ExpectNoExceptions]
		public void TestValidatePackLinesRelatingToConsol()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Constants.TransportModes.Sea;
			voyage.JV_VoyageFlight = new ZString("234");
			voyage.JV_RV_NKVessel = RefVessel.LookupVesselByName("ARAFURA", Factory).First().RV_FK;

			VoyageOrigin origin = Factory.New<VoyageOrigin>();
			origin.JA_JV = voyage.PK;
			origin.JA_RL_NKPortOfLoading = new ZString("AUSYD");
			voyage.Origins.Add(origin);

			VoyageDestination destination = Factory.New<VoyageDestination>();
			destination.JB_JV = voyage.PK;
			destination.JB_RL_NKPortOfDischarge = new ZString("USLAX");
			voyage.Destinations.Add(destination);

			voyage.GenerateSailings();
			JobSailing sailing = voyage.Sailings[0];
			CFSLoadListConsol consol1 = Factory.New<CFSLoadListConsol>();
			consol1.JK_TransportMode = Constants.TransportModes.Sea;
			consol1.Transports[0].JW_JX = sailing.PK;
			CFSShipment shipment1 = consol1.Shipments.AddNew();
			shipment1.JS_PackingMode = Constants.ContainerModes.LCL;
			shipment1.JS_OuterPacks = 8;
			shipment1.OuterPackLines[0].JL_PackageCount = 7;
			shipment1.JS_ActualWeight = 0;
			shipment1.JS_ActualVolume = 0;
			consol1.RunPreSaveValidation();
			AssertEquals("Shipment should have warnings on OuterPacks", true, shipment1.JS_OuterPacksInfo.HasWarnings());
			AssertEquals("Shipment should have warnings on Weight", true, shipment1.JS_ActualWeightInfo.HasWarnings());
			AssertEquals("Shipment should have warnings on ActualVolume", true, shipment1.JS_ActualVolumeInfo.HasWarnings());
			BusinessObject[] elements = consol1.UnAllocatedPackLines.ToArray();
			consol1.ValidatePackLinesRelatingToConsol(elements);
			AssertEquals("UnAllocated Packlines should have a warning on the row", true, elements[0].HasRowWarnings);
			consol1.ValidatePackLinesRelatingToConsol(elements);
		}

		public void TestEmptyContainerYardProxysCorrectValues()
		{
			var pickupPark = ZGuid.NewZGuid();
			var returnPark = ZGuid.NewZGuid();

			LoadList.JK_OA_ContainerYardEmptyPickupAddress = pickupPark;
			LoadList.JK_OA_ContainerYardEmptyReturnAddress = returnPark;
			LoadList.JK_TransportMode = Constants.TransportModes.Sea;

			Transport transport = LoadList.Transports[0];
			transport.JW_JX = ExportSailing1.PK;
			LoadList.JK_RL_NKLoadPort = transport.JW_RL_NKLoadPort;
			LoadList.JK_RL_NKDischargePort = transport.JW_RL_NKDiscPort;
			AssertEquals("Export", pickupPark, LoadList.JK_OA_EmptyContainerYard);

			LoadList.Transports[0].JW_JX = ImportSailing1.PK;
			LoadList.JK_RL_NKLoadPort = transport.JW_RL_NKLoadPort;
			LoadList.JK_RL_NKDischargePort = transport.JW_RL_NKDiscPort;
			AssertEquals("Import", returnPark, LoadList.JK_OA_EmptyContainerYard);
		}

		public void TestOnJobCreating()
		{
			CFSLoadListConsol loadList = Factory.New<CFSLoadListConsol>();
			var cartage = Factory.New<CommonCartage>();
			Factory.Save();     // to populate JJ_ConsignmentID

			cartage.JJ_ParentID = loadList.PK;
			cartage.JJ_ParentTableCode = loadList.TablePrefix;

			new JobHeader.Loader(cartage).TryLoadOrCreateWithoutMutexForTestOnly();
			AssertNotNull(cartage.Job);
			Assert(cartage.Job.JH_JH_ParentJob.IsEmpty);
			Assert(cartage.Job.JH_OA_LocalChargesAddr.IsEmpty);
			Factory.Save();

			new JobHeader.Loader(loadList).TryLoadOrCreateWithoutMutexForTestOnly();
			AssertEquals(loadList.Job.PK, ((LocalCartage.Integration.ICommonCartage)cartage).Job.JH_JH_ParentJob);
		}

		public void TestWorkflowIsDeferredWhenIsForwarding()
		{
			var cfsLoadList = Factory.New<CFSLoadListConsol>();
			SetupSailing(cfsLoadList, false);

			var shipment = cfsLoadList.Shipments.AddNew();
			shipment.OuterPackLines.AddNew();

			Factory.Save();

			AssertEquals("JK_IsCFS", true, cfsLoadList.JK_IsCFS);
			AssertEquals("JK_IsForwarding", false, cfsLoadList.JK_IsForwarding);

			var logs = cfsLoadList.Logs.GetAllLogs();
			Assert("SL_FireWorkflow is false", !logs[0].SL_FireWorkflow);

			var dualCfsForwardingConsol = Factory.New<CFSLoadListConsol>();
			SetupSailing(dualCfsForwardingConsol, false);

			dualCfsForwardingConsol.JK_OH_Forwarder = GetOrCreateForwarder(dualCfsForwardingConsol.Schedule.JX_JA_RL_NKPortOfLoading).PK;

			shipment = dualCfsForwardingConsol.Shipments.AddNew();
			shipment.OuterPackLines.AddNew();

			Factory.Save();

			AssertEquals("JK_IsCFS", true, dualCfsForwardingConsol.JK_IsCFS);
			AssertEquals("JK_IsForwarding", true, dualCfsForwardingConsol.JK_IsForwarding);

			logs = dualCfsForwardingConsol.Logs.GetAllLogs();
			Assert("SL_FireWorkflow is true", logs[0].SL_FireWorkflow);
		}

		#region IAutoRating

		public void TestJobDatesProvider()
		{
			var loadListConsol = Factory.New<CFSLoadListConsol>();
			AssertType<CFSLoadListConsolJobDatesProvider>(loadListConsol.RatingAdapter.JobDatesProvider);
		}

		public void TestAutoRatingJobServices()
		{
			CFSLoadListConsol loadList = Factory.New<CFSLoadListConsol>();
			IAutoRating autoRating = loadList.RatingAdapter;

			Assert(!autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.CFSLoadList, Constants.FreightServiceType.Codes.Fumigation));
			Assert(!autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.CFSLoadList, Constants.FreightServiceType.Codes.QuarantineInspection));
			Assert(!autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.CFSLoadList, Constants.FreightServiceType.Codes.CustomsHold));
			Assert(!autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.CFSLoadList, Constants.FreightServiceType.Codes.Washing));
			Assert(!autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.CFSLoadList, Constants.FreightServiceType.Codes.SteamCleaning));
			Assert(!autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.CFSLoadList, Constants.FreightServiceType.Codes.ExtraInspection));
			Assert(!autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.CFSLoadList, Constants.FreightServiceType.Codes.Cleaning));
			Assert(!autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.CFSLoadList, Constants.FreightServiceType.Codes.Tailgate));
			Assert(!autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.CFSLoadList, Constants.FreightServiceType.Codes.QuarantineUnpack));
			Assert(!autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.CFSLoadList, ChargeCodeSubGroupList.PackingCharges));
			Assert(!autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.CFSLoadList, ChargeCodeSubGroupList.UnpackingCharges));

			CFSContainer container = loadList.Containers.AddNew();

			loadList.JK_TransportMode = Constants.TransportModes.Sea;

			Transport transport = loadList.Transports[0];
			transport.JW_JX = ExportSailing1.PK;
			loadList.JK_RL_NKLoadPort = transport.JW_RL_NKLoadPort;
			loadList.JK_RL_NKDischargePort = transport.JW_RL_NKDiscPort;

			CFSShipment shipment = Factory.New<CFSShipment>();
			CFSPackLine packLine = shipment.OuterPackLines.AddNew();
			loadList.Shipments.Add(shipment);

			Assert(!autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.CFSLoadList, Constants.FreightServiceType.Codes.Fumigation));
			JobService fumigation = container.Services.AddNew();
			fumigation.ES_ServiceCode = Constants.FreightServiceType.Codes.Fumigation;
			fumigation.ES_Completed = ZDateTime.Now;
			Assert(autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.CFSLoadList, Constants.FreightServiceType.Codes.Fumigation));

			Assert(!autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.CFSLoadList, Constants.FreightServiceType.Codes.QuarantineInspection));
			JobService quarantine = container.Services.AddNew();
			quarantine.ES_ServiceCode = Constants.FreightServiceType.Codes.QuarantineInspection;
			quarantine.ES_Completed = ZDateTime.Now;
			Assert(autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.CFSLoadList, Constants.FreightServiceType.Codes.QuarantineInspection));

			Assert(!autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.CFSLoadList, Constants.FreightServiceType.Codes.CustomsHold));
			JobService customsHold = container.Services.AddNew();
			customsHold.ES_ServiceCode = Constants.FreightServiceType.Codes.CustomsHold;
			customsHold.ES_Completed = ZDateTime.Now;
			Assert(autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.CFSLoadList, Constants.FreightServiceType.Codes.CustomsHold));

			Assert(!autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.CFSLoadList, Constants.FreightServiceType.Codes.Washing));
			JobService washing = container.Services.AddNew();
			washing.ES_ServiceCode = Constants.FreightServiceType.Codes.Washing;
			washing.ES_Completed = ZDateTime.Now;
			Assert(autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.CFSLoadList, Constants.FreightServiceType.Codes.Washing));

			Assert(!autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.CFSLoadList, Constants.FreightServiceType.Codes.SteamCleaning));
			JobService steamCleaning = container.Services.AddNew();
			steamCleaning.ES_ServiceCode = Constants.FreightServiceType.Codes.SteamCleaning;
			steamCleaning.ES_Completed = ZDateTime.Now;
			Assert(autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.CFSLoadList, Constants.FreightServiceType.Codes.SteamCleaning));

			Assert(!autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.CFSLoadList, Constants.FreightServiceType.Codes.ExtraInspection));
			JobService extraInspection = container.Services.AddNew();
			extraInspection.ES_ServiceCode = Constants.FreightServiceType.Codes.ExtraInspection;
			extraInspection.ES_Completed = ZDateTime.Now;
			Assert(autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.CFSLoadList, Constants.FreightServiceType.Codes.ExtraInspection));

			Assert(!autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.CFSLoadList, Constants.FreightServiceType.Codes.Cleaning));
			JobService cleaning = container.Services.AddNew();
			cleaning.ES_ServiceCode = Constants.FreightServiceType.Codes.Cleaning;
			cleaning.ES_Completed = ZDateTime.Now;
			Assert(autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.CFSLoadList, Constants.FreightServiceType.Codes.Cleaning));

			Assert(!autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.CFSLoadList, Constants.FreightServiceType.Codes.Tailgate));
			JobService tailgate = container.Services.AddNew();
			tailgate.ES_ServiceCode = Constants.FreightServiceType.Codes.Tailgate;
			tailgate.ES_Completed = ZDateTime.Now;
			Assert(autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.CFSLoadList, Constants.FreightServiceType.Codes.Tailgate));

			Assert(!autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.CFSLoadList, Constants.FreightServiceType.Codes.QuarantineUnpack));
			JobService unpack = container.Services.AddNew();
			unpack.ES_ServiceCode = Constants.FreightServiceType.Codes.QuarantineUnpack;
			unpack.ES_Completed = ZDateTime.Now;
			Assert(autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.CFSLoadList, Constants.FreightServiceType.Codes.QuarantineUnpack));

			AssertNotNull("Sailing On Load List is Invalid", loadList.Schedule);
			Assert("Load List is export", loadList.IsExport());
			Assert(!autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.CFSLoadList, ChargeCodeSubGroupList.PackingCharges));
			container.JC_PackDate = ZDateTime.Today;
			Assert(autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.CFSLoadList, ChargeCodeSubGroupList.PackingCharges));

			transport.JW_JX = ImportSailing1.PK;
			loadList.JK_RL_NKLoadPort = transport.JW_RL_NKLoadPort;
			loadList.JK_RL_NKDischargePort = transport.JW_RL_NKDiscPort;

			Assert("Load List is Import", loadList.IsImport());
			Assert(!autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.CFSLoadList, ChargeCodeSubGroupList.UnpackingCharges));
			container.JC_LCLUnpack = ZDateTime.Today;
			Assert(autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.CFSLoadList, ChargeCodeSubGroupList.UnpackingCharges));
		}

		public void TestAutoRatingFreightMode()
		{
			CFSLoadListConsol loadList = Factory.New<CFSLoadListConsol>();
			IAutoRating autoRating = loadList.RatingAdapter;

			loadList.JK_TransportMode = Core.Constants.TransportModes.Sea;
			loadList.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			AssertEquals(FreightMode.FCL, autoRating.FreightMode);

			loadList.JK_ConsolMode = Core.Constants.ContainerModes.Groupage;
			AssertEquals(FreightMode.GRP, autoRating.FreightMode);

			loadList.JK_ConsolMode = Core.Constants.ContainerModes.LCL;
			AssertEquals(FreightMode.LCL, autoRating.FreightMode);

			loadList.JK_TransportMode = Core.Constants.TransportModes.Road;
			loadList.JK_ConsolMode = Core.Constants.ContainerModes.LTL;
			AssertEquals(FreightMode.LRO, autoRating.FreightMode);

			loadList.JK_ConsolMode = Core.Constants.ContainerModes.FTL;
			AssertEquals(FreightMode.FTL, autoRating.FreightMode);

			loadList.JK_TransportMode = Core.Constants.TransportModes.Rail;
			loadList.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			AssertEquals(FreightMode.FRA, autoRating.FreightMode);

			loadList.JK_TransportMode = Core.Constants.TransportModes.Air;
			loadList.JK_ConsolMode = Core.Constants.ContainerModes.Loose;
			AssertEquals(FreightMode.LSE, autoRating.FreightMode);

			loadList.JK_ConsolMode = Core.Constants.ContainerModes.ULD;
			AssertEquals(FreightMode.ULD, autoRating.FreightMode);
		}

		public void TestAutoRatingLocations()
		{
			var loadList = Factory.New<CFSLoadListConsol>();
			var ratingAdapter = loadList.RatingAdapter;

			loadList.JK_RL_NKLoadPort = "INBOM";
			loadList.JK_RL_NKDischargePort = "USLAX";
			AssertEquals("INBOM", ratingAdapter.Origin.Code);
			AssertEquals("USLAX", ratingAdapter.Destination.Code);

			loadList.Transports.MostInterestingTransport.JW_IsLinked = true;
			AssertEquals("INBOM", ratingAdapter.Origin.Code);
			AssertEquals("USLAX", ratingAdapter.Destination.Code);
		}

		#endregion

		#region Document Tests

		public void TestSupportedDataContext()
		{
			AssertEquals("Core.Constants.DataContext.LoadListConsol is Supported", true, LoadList.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.LoadListConsol)));
			AssertEquals("Core.Constants.DataContext.ERA is Supported", true, LoadList.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.ERA)));
			AssertEquals("Core.Constants.DataContext.CartageAdvice is Supported", true, LoadList.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.CartageAdvice)));
			AssertEquals("Core.Constants.DataContext.IMO is Supported", true, LoadList.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.IMO)));
			AssertEquals("Core.Constants.DataContext.LoadListDocument is Supported", true, LoadList.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.LoadListDocument)));
			AssertEquals("Core.Constants.DataContext.CommonConsol is Supported", true, LoadList.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.CommonConsol)));
			AssertEquals("Core.Constants.DataContext.PackUnpackContainerRego is Supported", true, LoadList.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.PackUnpackContainerRego)));
			AssertEquals("Core.Constants.DataContext.PickListDocument is Supported", true, LoadList.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.PickListDocument)));
		}

		public void TestGetDocBusinessObjectForDataContextCommonConsol()
		{
			CFSLoadListConsol consol = Factory.New<CFSLoadListConsol>();
			DocumentWrapper[] wrapper = consol.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.CommonConsol, null);
			AssertEquals("LoadListConsol wrapper should be created", 1, wrapper.Length);
			AssertEquals("Wrapper type should be DocLoadListConsol", "DocLoadListConsol", wrapper[0].GetType().Name);
		}

		public void TestGetContactOrganisation()
		{
			LoadList.JK_OA_CartageCoAddress = ZGuid.Empty;
			var contactOrg = LoadList.DocumentSupporter.GetContactOrganisation("", ContactType.NoContactType, DocumentDirection.ANY);
			AssertEquals("ContactOrg", null, contactOrg);

			contactOrg = LoadList.DocumentSupporter.GetContactOrganisation("", ContactType.LocalTransport, DocumentDirection.ANY);
			AssertEquals("ContactOrg", null, contactOrg.OrgHeader);

			var factoryToSave = new BusinessObjectFactory();
			var cartageCo = factoryToSave.NewWithValidTestData<OrgHeader>();
			factoryToSave.Save();

			LoadList.JK_OA_CartageCoAddress = cartageCo.MainAddress.PK;

			contactOrg = LoadList.DocumentSupporter.GetContactOrganisation("", ContactType.LocalTransport, DocumentDirection.ANY);
			AssertEquals("ContactOrg", LoadList.CartageCoPK, contactOrg.OrgHeader.PK);

			contactOrg = LoadList.DocumentSupporter.GetContactOrganisation("", ContactType.NoContactType, DocumentDirection.ANY);
			AssertEquals("ContactOrg", null, contactOrg);
		}

		public void TestGetContainerForDocumentsUsesSeparateFactory()
		{
			var consol = Factory.New<CFSLoadListConsol>();
			CFSContainer container = consol.Containers.AddNew();

			container.JC_ContainerNum = "CTRL0000011";

			Factory.Save();

			var queryProvider = new Mock<ICommonConsolDocumentSupporterQueryProvider>();

			Factory.SetValue(() => queryProvider.Object);

			queryProvider.Setup(m => m.GetContainersToPrint(It.IsAny<ContainerToSelectFromForPrintingCollection>(), It.IsAny<bool>()));
			DocumentWrapper[] wrappers = consol.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.CartageAdvice, StmMenuItem.GetForTesting(DocumentDirection.DEP));
			AssertNull("CartageAdvice DataContext is not supported so GetContainersToPrint is never executed hence no wrappers", wrappers);
			queryProvider.Verify(m => m.GetContainersToPrint(It.IsAny<ContainerToSelectFromForPrintingCollection>(), It.IsAny<bool>()), Times.Never());
		}

		#endregion

		#region DepotAddressDefaulting

		public void TestDepotAddressShouldBeDefaulted_WhenReceivingForwarderAddressOrDischargePortChanged()
		{
			var orgProxy = Factory.NewWithValidTestData<OrgHeader>();
			orgProxy.Addresses.DeleteAll();

			var address1 = orgProxy.Addresses.AddNew();
			address1.OA_Code = "Pickup Address";
			address1.OA_Address1 = "Address 1";
			address1.AddAddressType(OrgAddressType.Pickup);

			var company = Factory.New<GlbCompany>();
			company.GC_Code = "K#@";
			company.GC_Name = "TEST COMP";
			company.GC_RN_NKCountryCode = "AU";
			company.GC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			var branch = company.Branches.AddNew();
			branch.GB_Code = "B$#";
			branch.GB_BranchName = "BKD NAME";
			branch.GB_RN_NKCountryCode = "AU";
			branch.GB_OH_OrgProxy = orgProxy.PK;
			branch.GB_RL_NKHomePort = "AUSYD";

			var receivingForwarder = Factory.New<OrgHeader>();
			receivingForwarder.OH_Code = "TST01";
			receivingForwarder.OH_FullName = "Receiving Forwarder";
			receivingForwarder.MainAddress.Address1 = "Unit 1";

			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var loadList = Factory.New<CFSLoadListConsol>();
				((IBusinessObjectInternals)loadList).IsCopying = false;
				((ISupportDataImporting)loadList).IsImportingData = true;

				AssertEquals("Prerequisite", ZGuid.Empty, loadList.JK_OA_PackDepotAddress);
				AssertEquals("Prerequisite", ZGuid.Empty, loadList.JK_OA_UnpackDepotAddress);
				AssertEquals("Prerequisite", ZGuid.Empty, loadList.DepotPK);

				loadList.JK_RL_NKDischargePort = "AUSYD";
				AssertEquals(ZGuid.Empty, loadList.JK_OA_PackDepotAddress);
				AssertEquals(address1.PK, loadList.JK_OA_UnpackDepotAddress);
				AssertEquals(orgProxy.PK, loadList.DepotPK);

				var loadList2 = Factory.New<CFSLoadListConsol>();
				loadList2.Transports.DepartureTransport.JW_RL_NKLoadPort = "NAZKL";
				((IBusinessObjectInternals)loadList2).IsCopying = false;
				((ISupportDataImporting)loadList2).IsImportingData = true;

				AssertEquals("Prerequisite", ZGuid.Empty, loadList2.JK_OA_PackDepotAddress);
				AssertEquals("Prerequisite", ZGuid.Empty, loadList2.JK_OA_UnpackDepotAddress);
				AssertEquals("Prerequisite", ZGuid.Empty, loadList2.DepotPK);

				loadList2.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;
				AssertEquals(ZGuid.Empty, loadList2.JK_OA_PackDepotAddress);
				AssertEquals(address1.PK, loadList2.JK_OA_UnpackDepotAddress);
				AssertEquals(orgProxy.PK, loadList2.DepotPK);
			}
		}

		public void TestDepotAddressShouldBeDefaulted_WhenSendingForwarderAddressOrLoadPortChanged()
		{
			var orgProxy = Factory.NewWithValidTestData<OrgHeader>();
			orgProxy.Addresses.DeleteAll();

			var address1 = orgProxy.Addresses.AddNew();
			address1.OA_Code = "Delivery Address";
			address1.OA_Address1 = "Address 1";
			address1.AddAddressType(OrgAddressType.Delivery);

			var company = Factory.New<GlbCompany>();
			company.GC_Code = "K#@";
			company.GC_Name = "TEST COMP";
			company.GC_RN_NKCountryCode = "AU";
			company.GC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			var branch = company.Branches.AddNew();
			branch.GB_Code = "B$#";
			branch.GB_BranchName = "BKD NAME";
			branch.GB_RN_NKCountryCode = "AU";
			branch.GB_OH_OrgProxy = orgProxy.PK;
			branch.GB_RL_NKHomePort = "AUSYD";

			var sendingForwarder = Factory.New<OrgHeader>();
			sendingForwarder.OH_Code = "TST01";
			sendingForwarder.OH_FullName = "Sending Forwarder";
			sendingForwarder.MainAddress.Address1 = "Unit 1";

			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var loadList = Factory.New<CFSLoadListConsol>();
				((IBusinessObjectInternals)loadList).IsCopying = false;
				((ISupportDataImporting)loadList).IsImportingData = true;

				AssertEquals("Prerequisite", ZGuid.Empty, loadList.JK_OA_PackDepotAddress);
				AssertEquals("Prerequisite", ZGuid.Empty, loadList.JK_OA_UnpackDepotAddress);
				AssertEquals("Prerequisite", ZGuid.Empty, loadList.DepotPK);

				loadList.JK_RL_NKLoadPort = "AUSYD";
				AssertEquals(address1.PK, loadList.JK_OA_PackDepotAddress);
				AssertEquals(ZGuid.Empty, loadList.JK_OA_UnpackDepotAddress);
				AssertEquals(orgProxy.PK, loadList.DepotPK);

				var loadList2 = Factory.New<CFSLoadListConsol>();
				loadList2.Transports.DepartureTransport.JW_RL_NKLoadPort = "AUSYD";
				((IBusinessObjectInternals)loadList2).IsCopying = false;
				((ISupportDataImporting)loadList2).IsImportingData = true;

				AssertEquals("Prerequisite", ZGuid.Empty, loadList2.JK_OA_PackDepotAddress);
				AssertEquals("Prerequisite", ZGuid.Empty, loadList2.JK_OA_UnpackDepotAddress);
				AssertEquals("Prerequisite", ZGuid.Empty, loadList2.DepotPK);

				loadList2.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;
				AssertEquals(address1.PK, loadList2.JK_OA_PackDepotAddress);
				AssertEquals(ZGuid.Empty, loadList2.JK_OA_UnpackDepotAddress);
				AssertEquals(orgProxy.PK, loadList2.DepotPK);
			}
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			OriginalBranchOrg = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			base.SetUp();
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUBNE";
			new ConstantsAndReusables(Factory).EnsureCurrentCompanyMatchesCurrentBranch();
			Helper = new SailingsForTestClasses(Factory);
			LoadList = Factory.New<CFSLoadListForTest>();
		}

		protected override void TearDown()
		{
			base.TearDown();
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = OriginalBranchOrg;
		}
		ZGuid OriginalBranchOrg;

		protected CFSLoadListForTest LoadList;
		protected SailingsForTestClasses Helper;

		protected ZGuid CreateTestConsolWithOneContainerThroughFreightSystem()
		{
			JobSailing sailing = new ConstantsAndReusables(Factory).CreateNewSailing(true);
			CommonConsol newConsol = Factory.New<CommonConsol>();
			newConsol.JK_TransportMode = Constants.TransportModes.Sea;

			newConsol.Transports[0].JW_JX = sailing.PK;
			CommonContainer newContainer = newConsol.Containers.AddNew();
			newContainer.JC_ContainerNum = "GFCU0292036";
			OrgHeader forwarderClient = GetOrCreateForwarder(GlbBranch.CurrentBranch.GB_RL_NKHomePort);
			newConsol.JK_OA_ReceivingForwarderAddress = forwarderClient.MainAddress.PK;
			newContainer.JC_OH_CFSClient = forwarderClient.PK;

			return newConsol.PK;
		}

		protected OrgHeader GetOrCreateForwarder(ZString uNLOCO)
		{
			ZQuery forwarderFilter = new ZQuery(OrgHeaderSchema.OH_RL_NKClosestPort, uNLOCO);
			forwarderFilter.AddToFilter(OrgHeaderSchema.OH_IsForwarder, Enterprise.Core.Constants.BooleanTrueString);
			var result = Factory.LoadTop1<OrgHeader>(forwarderFilter);
			if (result == null)
			{
				result = Factory.New<OrgHeader>();
				result.OH_FullName = "Test Forwarder";
				result.MainAddress.OA_Address1 = "Forwarders Address";
				result.OH_RL_NKClosestPort = uNLOCO;
			}
			return result;
		}

		public class CFSLoadListForTest : CFSLoadListConsol
		{
			public CFSLoadListForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public bool ExposedIsDefaultedFromForwarder
			{
				get { return IsDefaultedFromForwarder; }
				set { IsDefaultedFromForwarder = value; }
			}
		}

		#endregion
	}
}
