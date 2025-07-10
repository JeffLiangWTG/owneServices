using System;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Business;
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
	[TestedType(typeof(CFSShipment))]
	public class CFSShipmentBusinessObjectTest : CFSBusinessObjectTestCase
	{
		#region Metadata

		protected override Type ExpectedMetadataType
		{
			get
			{
				return typeof(Metadata.Business.CommonShipment);
			}
		}

		#endregion

		[ExpectNoExceptions]
		public override void TestSaveAndDeleteBusinessObject()
		{
		}

		[ExpectException(typeof(CannotDeleteException))]
		public void TestSavedShipmentCannotBeDeleted()
		{
			BusinessObject shipment = GetNewBusinessObject();
			Factory.Save();
			shipment.Delete();
		}

		[ExpectNoExceptions]
		public void TestUnSavedShipmentCanBeDeleted()
		{
			BusinessObject shipment = GetNewBusinessObject();
			shipment.Delete();
		}

		#region TestPopulateBillAndShipmentNumberIfNeeded

		#region TestPopulateBillAndShipmentNumberIfNeeded

		public void TestPopulateBillAndShipmentNumberIfNeeded()
		{
			CommonShipment plainShipment = CommonShipment.New(Factory);
			plainShipment.JS_IsForwardRegistered = true;
			plainShipment.JS_IsCFSRegistered = false;
			Factory.Save();
			Assert("should start with forwarding number", plainShipment.JS_UniqueConsignRef.StartsWith(NumberFountains.JobShipmentFountainPrefix));

			plainShipment.JS_IsCFSRegistered = true;
			Factory.Save();
			Assert("should still start with forwarding number", plainShipment.JS_UniqueConsignRef.StartsWith(NumberFountains.JobShipmentFountainPrefix));

			CFSShipment cFSOnlyShipment = Factory.New<CFSShipment>();
			AssertEquals("should be CFS only", false, cFSOnlyShipment.JS_IsForwardRegistered);
			AssertEquals("should be CFS only", true, cFSOnlyShipment.JS_IsCFSRegistered);
			Factory.Save();
			Assert("should start with CFS number", cFSOnlyShipment.JS_UniqueConsignRef.StartsWith(NumberFountains.JobShipmentFountainCFSPrefix));

			CFSShipment bothShipment = Factory.New<CFSShipment>();
			bothShipment.JS_IsForwardRegistered = true;
			AssertEquals("should be both forwarding and CFS", true, bothShipment.JS_IsForwardRegistered);
			AssertEquals("should be both forwarding and CFS", true, bothShipment.JS_IsCFSRegistered);
			Factory.Save();
			Assert("should start with forwarding number", bothShipment.JS_UniqueConsignRef.StartsWith(NumberFountains.JobShipmentFountainPrefix));

			CFSShipment shipmentThatChanges = Factory.New<CFSShipment>();
			AssertEquals("should be CFS only", false, shipmentThatChanges.JS_IsForwardRegistered);
			AssertEquals("should be CFS only", true, shipmentThatChanges.JS_IsCFSRegistered);
			Factory.Save();
			string oldID = shipmentThatChanges.JS_UniqueConsignRef;
			Assert("should start with CFS number", shipmentThatChanges.JS_UniqueConsignRef.StartsWith(NumberFountains.JobShipmentFountainCFSPrefix));
			AssertEquals("precondition for following event creation assert", 0,
				shipmentThatChanges.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, "Changed job number from")).Length);
			shipmentThatChanges.JS_IsForwardRegistered = true;
			AssertEquals("should be both", true, shipmentThatChanges.JS_IsForwardRegistered);
			AssertEquals("should be both", true, shipmentThatChanges.JS_IsCFSRegistered);
			Factory.Save();
			string newID = shipmentThatChanges.JS_UniqueConsignRef;
			Assert("should start with forwarding number", shipmentThatChanges.JS_UniqueConsignRef.StartsWith(NumberFountains.JobShipmentFountainPrefix));
			AssertEquals("expecting event to have been created", 1,
				shipmentThatChanges.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.Equal, "Changed job number from " + oldID + " to " + newID)).Length);

			CFSShipment shipmentThatCannotChange = Factory.New<CFSShipment>();
			AssertEquals("should be CFS only", false, shipmentThatCannotChange.JS_IsForwardRegistered);
			AssertEquals("should be CFS only", true, shipmentThatCannotChange.JS_IsCFSRegistered);
			JobHeader attachedJobHeader = Factory.NewJobForTesting<JobHeader>();
			attachedJobHeader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			attachedJobHeader.JH_ParentID = shipmentThatCannotChange.PK;
			attachedJobHeader.JH_GB = GlbBranch.CurrentBranch.PK;
			attachedJobHeader.JH_GE = GlbDepartment.CurrentDepartment.PK;
			attachedJobHeader.JH_JobNum = "Job1";
			Factory.Save();
			Assert("should start with CFS number", shipmentThatCannotChange.JS_UniqueConsignRef.StartsWith(NumberFountains.JobShipmentFountainCFSPrefix));
			shipmentThatChanges.JS_IsForwardRegistered = true;
			AssertEquals("should be both", false, shipmentThatCannotChange.JS_IsForwardRegistered);
			AssertEquals("should be both", true, shipmentThatCannotChange.JS_IsCFSRegistered);
			Factory.Save();
			Assert("should start with CFS number", shipmentThatCannotChange.JS_UniqueConsignRef.StartsWith(NumberFountains.JobShipmentFountainCFSPrefix));
		}

		#endregion

		#region TestPopulateBillAndShipmentNumberIfNeeded_IsResetIfSavingTransactionFails

		public void TestPopulateBillAndShipmentNumberIfNeeded_IsResetIfSavingTransactionFails()
		{
			AssertNotEquals("Pre-condition: cannot be the same", NumberFountains.JobShipmentFountainPrefix, NumberFountains.JobShipmentFountainCFSPrefix);

			var cfsToForwardingShipment = Factory.New<CFSShipment>();
			Factory.Save();

			Assert("Should use CFS prefix", cfsToForwardingShipment.JS_UniqueConsignRef.StartsWith(NumberFountains.JobShipmentFountainCFSPrefix));

			var intialReference = cfsToForwardingShipment.JS_UniqueConsignRef;
			var changedNumberQuery = new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, "Changed job number from");
			var numberChangeLog = cfsToForwardingShipment.Logs.DatabaseHasLogs(changedNumberQuery);

			Assert("Pre-condition: should not have changed job number log", !numberChangeLog);

			try
			{
				cfsToForwardingShipment.JS_IsForwardRegistered = true;
				var org = Factory.New<OrgHeader>();
				org.OH_Code = "abc";
				org.OH_ScreeningStatus = "abc";

				Factory.Save();

				Fail("Should not reach this point because org headers has invalid column. That org has no relevance to this test other than to ensure saving will fail.");
			}
			catch (ZSaveException) { }

			AssertEquals("Saving was not successful JS_UniqueConsignRef number should remain the same", intialReference, cfsToForwardingShipment.JS_UniqueConsignRef);

			numberChangeLog = cfsToForwardingShipment.Logs.DatabaseHasLogs(changedNumberQuery);
			Assert("Should still not have changed number log as the changes were rolled back we don't expect these changes to have been saved", !numberChangeLog);
		}

		#endregion

		#endregion

		#region BusinessObjectTestCase Overrides

		public void TestBusinessContextOverride()
		{
			AssertEquals(BusinessContext.CFSShipmentReceival, Shipment.DocumentSupporter.BusinessContext);
		}

		public void TestSupportedDataContext()
		{
			CFSShipment shipment = Factory.New<CFSShipment>();
			AssertEquals("Core.Constants.DataContext.ShipmentReceival is Supported", true, shipment.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.ShipmentReceival)));
			AssertEquals("Core.Constants.DataContext.ContainerRego is Supported", true, shipment.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.ContainerRego)));
			AssertEquals("Core.Constants.DataContext.PackUnpackContainerRego is Supported", true, shipment.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.PackUnpackContainerRego)));
			AssertEquals("Core.Constants.DataContext.CFSContainerLeg is Supported", true, shipment.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.CFSContainerLeg)));
			AssertEquals("Core.Constants.DataContext.CartageAdvice is Supported", true, shipment.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.CartageAdvice)));
			AssertEquals("Core.Constants.DataContext.CFSAndForwardingShipment is Supported", true, shipment.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.CFSAndForwardingShipment)));
			AssertEquals("Core.Constants.DataContext.RequestForService is Supported", true, shipment.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.RequestForService)));
			AssertEquals("Core.Constants.DataContext.Service is Supported", true, shipment.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.Service)));
		}

		public void TestGetDocBusinessObjects()
		{
			DocumentWrapper[] wrappers = Shipment.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.ContainerRego, null);
			AssertEquals(1, wrappers.Length);
			AssertNotNull(wrappers[0]);
			AssertNotNull(Shipment.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.ShipmentReceival, null)[0]);
			AssertNotNull(Shipment.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.PackUnpackContainerRego, null)[0]);
			AssertNotNull(Shipment.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.CartageAdvice, null)[0]);
			AssertNotNull(Shipment.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.CFSAndForwardingShipment, null)[0]);
		}

		public void TestGetDocBusinessObjectForService()
		{
			CFSShipment shipment = Factory.New<CFSShipment>();
			DocumentWrapper[] wrapper = shipment.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.Service, null);
			AssertEquals("Wrapper should be created", 1, wrapper.Length);
			AssertEquals("Wrapper should be of type ShipmentReceival", "DocShipmentReceival", wrapper[0].GetType().Name);
		}

		#region TestGetDocBusinessObjectForRequestForService

		public void TestGetDocBusinessObjectsForRequestForService()
		{
			var shipment = (CFSShipment)GetDocumentSupportBusinessObject();

			var servicesSelectionProvider = new Mock<IServicesSelectionProvider>();

			Factory.SetValue(() => servicesSelectionProvider.Object);

			servicesSelectionProvider
				.Setup(m => m.GetServicesToPrint(It.Is<IHaveServices>(p =>
					(CFSDocsAndCartage)p == shipment.DocsAndCartage)))
				.Returns((JobService[])null);

			DocumentWrapper[] wrapper = shipment.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.RequestForService, null);
			AssertEquals("Wrapper should be null", null, wrapper);

			servicesSelectionProvider
				.Verify(m => m.GetServicesToPrint(It.Is<IHaveServices>(p =>
					(CFSDocsAndCartage)p == shipment.DocsAndCartage)), Times.Once);

			JobService service1 = shipment.DocsAndCartage.Services.AddNew();
			JobService service2 = shipment.DocsAndCartage.Services.AddNew();

			servicesSelectionProvider
				.Setup(m => m.GetServicesToPrint(It.Is<IHaveServices>(p => (CFSDocsAndCartage)p == shipment.DocsAndCartage)))
				.Returns(new[] { service1, service2 });

			wrapper = shipment.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.RequestForService, null);
			AssertEquals("Service Wrappers should be created", 2, wrapper.Length);
			AssertEquals("Wrapper type should be DocService", "DocService", wrapper[0].GetType().Name);
			AssertEquals("Wrapper type should be DocService", "DocService", wrapper[1].GetType().Name);

			servicesSelectionProvider
				.Verify(
					m => m.GetServicesToPrint(
						It.Is<IHaveServices>(p => (CFSDocsAndCartage)p == shipment.DocsAndCartage)), Times.Exactly(2));
		}

		#endregion

		#endregion

		#region Property Tests

		public void TestJS_OH_HandledOnBehalfOfForwarder()
		{
			AssertNotNull("GlbBranch.CurrentBranch.Organisation must not be null", GlbBranch.CurrentBranch.OrgProxy);

			GlbBranch.CurrentBranch.OrgProxy.OH_IsForwarder = true;
			GlbBranch.CurrentBranch.OrgProxy.OH_IsPackDepot = true;
			GlbBranch.CurrentBranch.OrgProxy.OH_IsUnpackDepot = true;

			OrgHeader cFSClient = Factory.New<OrgHeader>();
			cFSClient.OH_IsForwarder = true;
			cFSClient.OH_FullName = "Non Proxy Org Client";
			cFSClient.MainAddress.OA_Address1 = "Somewhere Locl";
			cFSClient.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.OrgProxy.OH_RL_NKClosestPort;

			CFSShipment shipment = Factory.New<CFSShipment>();
			shipment.JS_OH_HandledOnBehalfOfForwarder = Factory.New<OrgHeader>().PK;
			Assert("Not expecting is forwarding to be true", !shipment.JS_IsForwardRegistered);

			shipment.JS_OH_HandledOnBehalfOfForwarder = cFSClient.PK;
			Assert("Not expecting is forwarding to be true", !shipment.JS_IsForwardRegistered);

			shipment.JS_OH_HandledOnBehalfOfForwarder = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			Assert("Expecting is forwarding to be true", shipment.JS_IsForwardRegistered);

			shipment.JS_OH_HandledOnBehalfOfForwarder = Factory.New<OrgHeader>().PK;
			Assert("Not expecting is forwarding to be true", !shipment.JS_IsForwardRegistered);
		}

		public void TestSetShipment_JS_OH_HandledOnBehalfOfForwarder_ShipmentAddedToConsol()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "aaa";
			org1.OH_FullName = "bbb";
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "ccc";
			org2.OH_FullName = "ddd";
			OrgHeader org3 = Factory.NewWithValidTestData<OrgHeader>();
			org3.OH_Code = "eee";
			org3.OH_FullName = "fff";

			var consol = Factory.NewWithValidTestData<CFSLoadListConsol>();
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_OH_Forwarder = org1.PK;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKDestination = "NZAKL";
			shipment.JS_RL_NKOrigin = "AUSYD";

			var existingShipment = Factory.NewWithValidTestData<CFSShipment>();
			existingShipment.JS_RL_NKDestination = "NZAKL";
			existingShipment.JS_RL_NKOrigin = "AUSYD";
			existingShipment.JS_OH_HandledOnBehalfOfForwarder = org2.PK;

			Factory.Save();
			AssertEquals("JS_OH_HandledOnBehalfOfForwarder is set from CFS load list", shipment.JS_OH_HandledOnBehalfOfForwarder, consol.JK_OH_Forwarder);

			shipment.JS_OH_HandledOnBehalfOfForwarder = org3.PK;
			Factory.Save();

			AssertNotEquals("shipment HandledOnBehalfOfForwarder and consol forwarder are not same", shipment.HandledOnBehalfOfForwarder.MainAddress.PK, consol.JK_OA_SendingForwarderAddress);
			AssertEquals("JS_OH_HandledOnBehalfOfForwarder", shipment.JS_OH_HandledOnBehalfOfForwarder, org3.PK);

			consol.Shipments.Add(existingShipment);
			AssertNotEquals("shipment HandledOnBehalfOfForwarder and consol forwarder are not same", existingShipment.HandledOnBehalfOfForwarder.MainAddress.PK, consol.JK_OA_SendingForwarderAddress);
			AssertEquals("JS_OH_HandledOnBehalfOfForwarder", existingShipment.JS_OH_HandledOnBehalfOfForwarder, org2.PK);
		}

		public void TestSetShipment_JS_OH_HandledOnBehalfOfForwarder_ConsolAddedToShipment()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "aaa";
			org1.OH_FullName = "bbb";

			var shipment = Factory.NewWithValidTestData<CFSShipment>();
			shipment.JS_RL_NKDestination = "NZAKL";
			shipment.JS_RL_NKOrigin = "AUSYD";

			Factory.Save();
			AssertEquals("JS_OH_HandledOnBehalfOfForwarder is empty", ZGuid.Empty, shipment.JS_OH_HandledOnBehalfOfForwarder);

			var consol = shipment.Consols.AddNew();
			consol.JK_OH_Forwarder = org1.PK;

			Factory.Save();

			var query = new ZQuery();
			query.AddToFilter(JobShipmentSchema.JS_OH_HandledOnBehalfOfForwarder, org1.PK);

			shipment = Factory.LoadTop1<CFSShipment>(query);

			AssertEquals("JS_OH_HandledOnBehalfOfForwarder is set from CFS load list", shipment.JS_OH_HandledOnBehalfOfForwarder, consol.JK_OH_Forwarder);
			AssertEquals("JS_OH_HandledOnBehalfOfForwarder", shipment.JS_OH_HandledOnBehalfOfForwarder, org1.PK);
		}

		public void TestSetWarnNotErrorOnLocationTotalsOnPackLines()
		{
			PackLine pack = Shipment.OuterPackLines.AddNew();
			AssertEquals("precondition", false, pack.WarnNotErrorOnLocationTotals);
			Shipment.SetWarnNotErrorOnLocationTotalsOnPackLines(true);
			AssertEquals("should have changed", true, pack.WarnNotErrorOnLocationTotals);
			Shipment.SetWarnNotErrorOnLocationTotalsOnPackLines(false);
			AssertEquals("should have changed", false, pack.WarnNotErrorOnLocationTotals);
		}

		#endregion

		#region TestGatePassRequired

		public void TestGatePassRequired()
		{
			Shipment.JS_RL_NKOrigin = "";
			Shipment.JS_RL_NKDestination = GlbBranch.CurrentBranch.HomePort.Code;
			AssertEquals("GatePassRequired should be checked by default.", true, Shipment.JS_TranshipToOtherCFS);

			Shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.HomePort.Code;
			AssertEquals("GatePassRequired should not be checked by default.", false, Shipment.JS_TranshipToOtherCFS);

			Shipment.JS_RL_NKOrigin = "";
			AssertEquals("GatePassRequired should be checked by default.", true, Shipment.JS_TranshipToOtherCFS);

			Shipment.JS_RL_NKDestination = "";
			AssertEquals("GatePassRequired should not be checked by default.", false, Shipment.JS_TranshipToOtherCFS);
		}

		#endregion

		public void TestPackContainerPivotDataRefresh()
		{
			CFSLoadListConsol loadList = /*(CFSLoadListConsol)*/ Shipment.Consols.AddNew();
			CFSPackLine pack = Shipment.OuterPackLines.AddNew();
			CFSContainer container = loadList.Containers.AddNew();
			container.PackLines.Add(pack);

			//			Enterprise.Freight.Business.Container Container = Pack.Containers.AddNew();
			//			Container.JC_JK = LoadList.PK;
			Factory.Save();

			BusinessObjectFactory otherFactory = new BusinessObjectFactory();
			CFSShipment shipmentInOtherFactory = otherFactory.Load<CFSShipment>(Shipment.PK);
			AssertEquals("expecting 1 load list", 1, shipmentInOtherFactory.Consols.Count);
			AssertEquals("expecting 1 pack line", 1, shipmentInOtherFactory.OuterPackLines.Count);
			AssertEquals("expecting 1 container", 1, shipmentInOtherFactory.OuterPackLines[0].Containers.Count);
			AssertEquals("expecting read only pack", true, shipmentInOtherFactory.OuterPackLines[0].JL_PackageCountInfo.ReadOnly);

			pack.Containers.Remove(container);
			Factory.Save();
			AssertEquals("expecting 0 containers", 0, shipmentInOtherFactory.OuterPackLines[0].Containers.Count);
			AssertEquals("expecting editable pack", false, shipmentInOtherFactory.OuterPackLines[0].ReadOnly);
		}

		public void TestSetPackedPackLinesReadOnly()
		{
			PackLine packNotPacked = Shipment.OuterPackLines.AddNew();
			PackLine packToBePacked = Shipment.OuterPackLines.AddNew();

			AssertEquals("precondition - should not be packed yet", 0, packNotPacked.Containers.Count);
			AssertEquals("precondition - should not be packed yet", 0, packToBePacked.Containers.Count);
			AssertEquals("precondition", false, packNotPacked.ReadOnly);
			AssertEquals("precondition", false, packToBePacked.ReadOnly);

			packToBePacked.Containers.AddNew();
			Shipment.SetPackedPackLinesReadOnly();
			AssertEquals("should not be packed", 0, packNotPacked.Containers.Count);
			AssertEquals("should be packed", 1, packToBePacked.Containers.Count);
			AssertEquals("should not have been set read only", false, packNotPacked.ReadOnly);
			AssertEquals("should be read only", true, packToBePacked.JL_PackageCountInfo.ReadOnly);

			packToBePacked.Containers.RemoveAll();
			Shipment.SetPackedPackLinesReadOnly();
			AssertEquals("should not be packed", 0, packNotPacked.Containers.Count);
			AssertEquals("should not be packed", 0, packToBePacked.Containers.Count);
			AssertEquals("should not have been set read only", false, packNotPacked.ReadOnly);
			AssertEquals("should have ReadOnly set to false", false, packToBePacked.ReadOnly);
		}

		public void TestNoteTypesCanBeAccessed()
		{
			AssertNotNull(Shipment.NoteTypes);
		}

		public void TestWarningsInDifferentFactories()
		{
			PackLine pack = Shipment.OuterPackLines.AddNew();
			pack.JL_PackageCount = pack.JL_Outturn = Shipment.JS_OuterPacks = 10;
			pack.JL_F3_NKPackType = "PKG";
			pack.JL_ActualVolume = Shipment.JS_ActualVolume = 10;
			pack.JL_ActualWeight = Shipment.JS_ActualWeight = 10;
			Factory.Save();

			ZGuid loadListPK = Shipment.Consols.AddNew().PK;
			Factory.Save();

			BusinessObjectFactory otherFactory = new BusinessObjectFactory();
			CFSLoadListConsol loadListInOtherFactory = otherFactory.Load<CFSLoadListConsol>(loadListPK);
			CFSLoadListConsol loadListInSameFactory = otherFactory.Load<CFSLoadListConsol>(loadListPK);

			PackLine newPack = Shipment.OuterPackLines.AddNew();
			newPack.JL_F3_NKPackType = "DOZ";
			newPack.JL_Outturn = newPack.JL_PackageCount = 1;
			newPack.JL_ActualVolume = newPack.JL_ActualWeight = 1;
			Shipment.JS_ActualWeight++;
			Shipment.JS_ActualVolume++;
			Shipment.JS_OuterPacks++;
			Factory.Save();

			AssertEquals("should have been updated and validated together", false, loadListInOtherFactory.Shipments[0].JS_ActualWeightInfo.HasNotifications());
			AssertEquals("should have been updated and validated together", false, loadListInSameFactory.Shipments[0].JS_ActualWeightInfo.HasNotifications());

			AssertEquals("should have been updated and validated together", false, loadListInOtherFactory.Shipments[0].JS_ActualVolumeInfo.HasNotifications());
			AssertEquals("should have been updated and validated together", false, loadListInSameFactory.Shipments[0].JS_ActualVolumeInfo.HasNotifications());

			AssertEquals("should have been updated and validated together", false, loadListInOtherFactory.Shipments[0].JS_OuterPacksInfo.HasNotifications());
			AssertEquals("should have been updated and validated together", false, loadListInSameFactory.Shipments[0].JS_OuterPacksInfo.HasNotifications());

			loadListInOtherFactory.JK_MasterBillNum = "x";
			otherFactory.Save();

			AssertEquals("should have been updated and validated together", false, loadListInOtherFactory.Shipments[0].JS_ActualWeightInfo.HasNotifications());
			AssertEquals("should have been updated and validated together", false, loadListInSameFactory.Shipments[0].JS_ActualWeightInfo.HasNotifications());

			AssertEquals("should have been updated and validated together", false, loadListInOtherFactory.Shipments[0].JS_ActualVolumeInfo.HasNotifications());
			AssertEquals("should have been updated and validated together", false, loadListInSameFactory.Shipments[0].JS_ActualVolumeInfo.HasNotifications());

			AssertEquals("should have been updated and validated together", false, loadListInOtherFactory.Shipments[0].JS_OuterPacksInfo.HasNotifications());
			AssertEquals("should have been updated and validated together", false, loadListInSameFactory.Shipments[0].JS_OuterPacksInfo.HasNotifications());
		}

		public void TestOnJobCreating()
		{
			CFSShipment cfsShipment = Factory.New<CFSShipment>();
			var cartage = Factory.New<CommonCartage>();
			Factory.Save();

			cartage.JJ_ParentID = cfsShipment.PK;
			cartage.JJ_ParentTableCode = cfsShipment.TablePrefix;

			new JobHeader.Loader(cartage).TryLoadOrCreateWithoutMutexForTestOnly();
			AssertNotNull(cartage.Job);
			Assert(cartage.Job.JH_JH_ParentJob.IsEmpty);
			Assert(cartage.Job.JH_OA_LocalChargesAddr.IsEmpty);
			Factory.Save();

			new JobHeader.Loader(cfsShipment).TryLoadOrCreateWithoutMutexForTestOnly();
			AssertEquals(cfsShipment.Job.PK, cartage.Job.JH_JH_ParentJob);
		}

		public void TestWorkflowIsDeferredWhenIsForwardRegistered()
		{
			var cfsOnlyShipment = Factory.New<CFSShipment>();
			var dualCfsForwardingShipment = Factory.New<CFSShipment>();
			dualCfsForwardingShipment.JS_IsForwardRegistered = true;

			Factory.Save();

			Assert("should be CFS only", !cfsOnlyShipment.JS_IsForwardRegistered);
			Assert("should be CFS only", cfsOnlyShipment.JS_IsCFSRegistered);

			Assert("should be CFS and Forwarding", dualCfsForwardingShipment.JS_IsForwardRegistered);
			Assert("should be CFS and Forwarding", dualCfsForwardingShipment.JS_IsCFSRegistered);

			Assert("should not be deferred", cfsOnlyShipment.Logs.GetAllLogs().All(x => !((StmALog)x).SL_FireWorkflow));
			Assert("should be deferred", dualCfsForwardingShipment.Logs.GetAllLogs().All(x => ((StmALog)x).SL_FireWorkflow));
		}

		#region Implementation

		CFSShipment Shipment;

		protected override void SetUp()
		{
			OriginalBranchOrg = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			base.SetUp();
			new ConstantsAndReusables(Factory).EnsureCurrentCompanyMatchesCurrentBranch();
			Shipment = Factory.New<CFSShipment>();
		}

		protected override void TearDown()
		{
			base.TearDown();
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = OriginalBranchOrg;
		}
		ZGuid OriginalBranchOrg;

		protected BusinessObject GetDocumentSupportBusinessObject()
		{
			return Factory.New<CFSShipment>();
		}

		#endregion
	}
}
