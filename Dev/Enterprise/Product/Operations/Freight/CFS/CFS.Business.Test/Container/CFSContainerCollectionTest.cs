using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.CFS.Business.Testing
{
	internal sealed class CFSContainerCollectionTest : BaseFreightTest
	{
		public void TestSetDefaultsForNewChild()
		{
			CFSLoadListConsol loadList = Factory.New<CFSLoadListConsol>();
			loadList.JK_OH_Forwarder = ZGuid.NewZGuid();
			loadList.JK_TransportMode = Constants.TransportModes.Air;
			loadList.Transports[0].JW_JX = SydLaxFlightLeg.PK;

			CFSContainer container = loadList.Containers.AddNew();
			AssertEquals("JC_TransportMode", loadList.JK_TransportMode, container.JC_TransportMode);
			AssertEquals("JC_TrainWagonNumberInfo.ReadOnly", true, container.JC_TrainWagonNumberInfo.ReadOnly);
			AssertEquals("JC_ContainerMode", Constants.ContainerModes.ULD, container.JC_ContainerMode);
			AssertEquals("JC_OH_CFSClient", loadList.JK_OH_Forwarder, container.JC_OH_CFSClient);

			loadList.JK_TransportMode = Constants.TransportModes.Rail;
			CFSContainer container2 = loadList.Containers.AddNew();
			AssertEquals("JC_TrainWagonNumberInfo.ReadOnly", false, container2.JC_TrainWagonNumberInfo.ReadOnly);
		}

		public void TestAdd_MayNotAddAStorageContainer()
		{
			CFSLoadListConsol loadList = Factory.New<CFSLoadListConsol>();
			CFSContainer cFSContainer = Factory.New<CFSContainer>();
			cFSContainer.JC_Purpose = ContainerPurposeTypeCodeDescriptionPairList.Codes.Storage;

			loadList.Containers.Add(cFSContainer);
			AssertEquals(0, loadList.Containers.Count);

			cFSContainer.JC_Purpose = ContainerPurposeTypeCodeDescriptionPairList.Codes.CFS;
			loadList.Containers.Add(cFSContainer);
			AssertEquals(1, loadList.Containers.Count);
		}

		public void TestJC_OH_CFSClient()
		{
			var receivingClient = Factory.NewWithValidTestData<OrgHeader>();
			var loadList = Factory.NewWithValidTestData<CFSLoadListConsol>();
			loadList.JK_OH_Forwarder = receivingClient.PK;

			var container = loadList.Containers.AddNew();
			var shipment = loadList.Shipments.AddNew();
			var packLine = shipment.OuterPackLines.AddNew();
			Factory.Save();

			AssertEquals("Client should be loaded from consol on the CFS Container", receivingClient.PK, container.JC_OH_CFSClient);

			var newClient = Factory.NewWithValidTestData<OrgHeader>();
			loadList.JK_OH_Forwarder = newClient.PK;
			Factory.Save();

			AssertEquals("Client should be updated from consol on the CFS Container", newClient.PK, container.JC_OH_CFSClient);
		}

		[ExpectNoExceptions]
		public void TestAddNewCopysFieldsAccrossThatWouldOtherwiseCauseWarnings()
		{
			OrgHeader client = Factory.NewWithValidTestData<OrgHeader>();

			JobVoyage voyage = Factory.New<JobVoyage>();

			VoyageDestination dest = voyage.Destinations.AddNew();
			dest.JB_RL_NKPortOfDischarge = "MYBAG";

			VoyageOrigin orig = voyage.Origins.AddNew();
			orig.JA_RL_NKPortOfLoading = "AUPER";

			JobSailing sailing = voyage.Sailings.AddNew();
			sailing.JX_JA = orig.PK;
			sailing.JX_JB = dest.PK;

			CFSLoadListConsol loadList = Factory.New<CFSLoadListConsol>();
			loadList.JK_OH_Forwarder = client.PK;

			Transport transport = loadList.Transports[0];
			transport.JW_JX = sailing.PK;

			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			CFSLoadListConsol loadList2 = factory2.Load<CFSLoadListConsol>(loadList.PK);

			EventHandler handler = new EventHandler(FailOnWarningEventHandler);
			try
			{
				loadList.WarningMessageInfo.ValueChanged += handler;
				loadList2.WarningMessageInfo.ValueChanged += handler;

				CFSContainer container = loadList2.Containers.AddNew();
				container.JC_RC = factory2.LoadFromNaturalKey(typeof(RefContainer), RefContainerSchema.RC_Code, "40GP").PK;

				factory2.Save();
			}
			finally
			{
				loadList.WarningMessageInfo.ValueChanged -= handler;
				loadList2.WarningMessageInfo.ValueChanged -= handler;
			}
		}

		public void TestUpdatePackLineContainerOnAdd()
		{
			var loadList = Factory.New<CFSLoadListConsol>();

			loadList.AutomaticallyUpdatePackLineContainers = true;
			AssertEquals("This test is only valid when containers are automatically packed", true, loadList.AutomaticallyUpdatePackLineContainers);

			var shipment = loadList.Shipments.AddNew();
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 1;
			var container = loadList.Containers.AddNew();

			AssertEquals("Container.PackLines.Count", 1, container.PackLines.Count);
			AssertNotNull("PackLine should be assigned to Container", packLine.GetContainer(loadList));

			var loadList2 = Factory.New<CFSLoadListConsol>();
			loadList2.AutomaticallyUpdatePackLineContainers = true;
			loadList2.JK_IsForwarding = true;
			var shipment2 = loadList2.Shipments.AddNew();
			var packLine2 = shipment2.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 1;
			var container2 = loadList2.Containers.AddNew();

			AssertEquals("Container2.PackLines.Count", 1, container2.PackLines.Count);
			AssertEquals("Container2.PackLines[0]", packLine2, container2.PackLines[0]);
			AssertEquals("PackLine2 should be assigned to Container2", container2.PK, packLine2.GetContainer(loadList2).PK);
		}

		public void TestRemoveContainerWithPackLinesEvent()
		{
			var eventHandlerCalled = 0;
			var loadList = Factory.New<CFSLoadListConsol>();
			loadList.Shipments.AddNew().OuterPackLines.AddNew().Containers.Add(loadList.Containers.AddNew());

			loadList.OnRemovingContainerWithPackLines += (s, e) => eventHandlerCalled++;
			Factory.Saving += (factory) => loadList.Containers.RemoveAll();

			loadList.Containers.RemoveAll();
			AssertEquals("Event raised to confirm removal of container", 1, eventHandlerCalled);

			Factory.Save();
			AssertEquals("No event raised while database is in transaction", 1, eventHandlerCalled);
			AssertEquals("Container removed without user confirmation as part of a seperate transaction", 0, loadList.Containers.Count);
		}

		public void TestDataRefreshBusDoesNotTriggerDetachWarning()
		{
			var loadListConsol = Factory.New<CFSLoadListConsol>();
			var container = loadListConsol.Containers.AddNew();
			var shipment = loadListConsol.Shipments.AddNew();
			var warningTriggered = false;

			Factory.RefreshEnabled = true;

			var packline = shipment.OuterPackLines.AddNew();
			packline.JL_PackageCount = 5;
			packline.JL_Description = "Angered Anchovies";
			container.AddPackLines(new BusinessObject[] { packline });
			loadListConsol.OnRemovingContainerWithPackLines += (s, e) => warningTriggered = true;

			Factory.Save();

			var factory1 = new BusinessObjectFactory();
			var cfsContainer = factory1.Load<CFSContainer>(container.PK);
			cfsContainer.JC_RH_NKContainerCommodityCode = "GEN";

			factory1.Save();

			Assert("Detach warning should not have been triggered", !warningTriggered);
		}

		public void TestContainerSailingEvents()
		{
			EventWatcher watcher = new EventWatcher();

			CFSContainer container = Factory.New<CFSContainer>();

			container.JC_JA_NKPortOfLoadingInfo.ValueChanged += watcher.Handler;
			container.JC_JB_NKPortOfDischargeInfo.ValueChanged += watcher.Handler;
			container.JC_JV_NKVesselInfo.ValueChanged += watcher.Handler;
			container.JC_JV_VoyageFlightInfo.ValueChanged += watcher.Handler;
			container.JC_JA_E_DEPInfo.ValueChanged += watcher.Handler;
			container.JC_JB_E_ARVInfo.ValueChanged += watcher.Handler;

			AssertEquals("Just Attacted", 0, watcher.Count);

			container.JC_JX = ZGuid.NewZGuid();
			AssertEquals("Set JC_JX", 6, watcher.Count);

			watcher.Clear();
			CFSLoadListConsol loadList = Factory.New<CFSLoadListConsol>();
			Transport transport = loadList.Transports[0];
			AssertEquals("New LoadList", 0, watcher.Count);

			transport.JW_JX = ZGuid.NewZGuid();
			AssertEquals("Set JK_JX", 0, watcher.Count);

			container.JC_JK = loadList.PK;
			loadList.Containers.Load();
			AssertEquals("Attached a LoadList", 6, watcher.Count);
			watcher.Clear();

			transport.JW_JX = ZGuid.NewZGuid();
			AssertEquals("Set JK_JX", 6, watcher.Count);
			watcher.Clear();

			loadList.Containers.Remove(container);
			AssertEquals("Detached the LoadList", 6, watcher.Count);
			watcher.Clear();

			transport.JW_JX = ZGuid.NewZGuid();
			AssertEquals("Set JK_JX after detaching", 0, watcher.Count);

			container.JC_JA_NKPortOfLoadingInfo.ValueChanged -= watcher.Handler;
			container.JC_JB_NKPortOfDischargeInfo.ValueChanged -= watcher.Handler;
			container.JC_JV_NKVesselInfo.ValueChanged -= watcher.Handler;
			container.JC_JV_VoyageFlightInfo.ValueChanged -= watcher.Handler;
			container.JC_JA_E_DEPInfo.ValueChanged -= watcher.Handler;
			container.JC_JB_E_ARVInfo.ValueChanged -= watcher.Handler;
		}

		#region Implementation

		void FailOnWarningEventHandler(object sender, EventArgs e)
		{
			AssertEquals(ZString.Empty, (sender as CFSLoadListConsol).WarningMessage);
		}

		public class EventWatcher
		{
			int fCount;
			public readonly EventHandler Handler;

			public EventWatcher()
			{
				Handler = new EventHandler(HandlerMethod);
			}

			public int Count
			{
				get { return fCount; }
			}

			public void Clear()
			{
				fCount = 0;
			}

			void HandlerMethod(object sender, EventArgs e)
			{
				fCount++;
			}
		}

		#endregion
	}
}
