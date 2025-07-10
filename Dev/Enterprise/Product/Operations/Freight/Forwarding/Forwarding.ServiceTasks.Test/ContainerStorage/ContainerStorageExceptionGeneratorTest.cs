using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.ServiceTasks.Test
{
	internal class ContainerStorageExceptionGeneratorTest : TestCaseWithFactory
	{
		#region Consol Exceptions Test

		[TestDate(2000, 1, 2, 0, 0, 0)]
		public void TestConsolMilestoneException()
		{
			ForwardingConsol consol1 = NewConsol();
			AddContainer(consol1, ZBool.False, ZDateTime.Empty, ZDateTime.Now.AddDays(+1));
			AddLinkedTransport(consol1, "vessel1", "voyage1", "port0", "port1", ZDateTime.Now.AddDays(-1));
			ForwardingConsol consol2 = NewConsol();
			AddContainer(consol2, ZBool.False, ZDateTime.Empty, ZDateTime.Now.AddDays(-1));
			AddLinkedTransport(consol2, "vessel2", "voyage2", "port0", "port1", ZDateTime.Now.AddDays(+1));
			ForwardingConsol consol3 = NewConsol();
			AddContainer(consol3, ZBool.True, ZDateTime.Empty, ZDateTime.Now.AddDays(+1));
			AddLinkedTransport(consol3, "vessel3", "voyage3", "port0", "port1", ZDateTime.Now.AddDays(-1));
			ForwardingConsol consol4 = NewConsol();
			AddContainer(consol4, ZBool.True, ZDateTime.Empty, ZDateTime.Now.AddDays(-1));
			AddLinkedTransport(consol4, "vessel4", "voyage4", "port0", "port1", ZDateTime.Now.AddDays(+1));
			ForwardingConsol consol5 = NewConsol();
			AddContainer(consol5, ZBool.True, ZDateTime.Now, ZDateTime.Now.AddDays(-1));
			AddLinkedTransport(consol5, "vessel5", "voyage5", "port0", "port1", ZDateTime.Now.AddDays(-1));
			Factory.Save();
			GenerateExceptions();
			GenerateExceptions();
			AssertExceptionCreated(consol1, "consol1", 1);
			AssertExceptionCreated(consol2, "consol2", 0);
			AssertExceptionCreated(consol3, "consol3", 0);
			AssertExceptionCreated(consol4, "consol4", 1);
			AssertExceptionCreated(consol5, "consol5", 0);
		}

		[TestDate(2000, 1, 2, 0, 0, 0)]
		public void TestConsolMilestoneException_OptimiseSQL()
		{
			ForwardingConsol consol1 = NewConsol();
			AddContainer(consol1, ZBool.False, ZDateTime.Empty, ZDateTime.Now.AddDays(+1));
			AddLinkedTransport(consol1, "vessel1", "voyage1", "port0", "port1", ZDateTime.Now.AddDays(-30));
			Factory.Save();
			GenerateExceptions();
			GenerateExceptions();
			AssertExceptionCreated(consol1, "consol1", 0);
		}

		ForwardingConsol NewConsol()
		{
			return Factory.New<ForwardingConsol>();
		}

		void AddContainer(ForwardingConsol consol, ZBool overridden, ZDateTime wharfGateOut, ZDateTime storageDate)
		{
			CommonContainer container = consol.Containers.AddNew();
			container.JC_OverrideFCLAvailableStorage = overridden;
			if (overridden)
			{
				container.JC_ArrivalCTOStorageStartDate = storageDate;
			}

			container.JC_FCLWharfGateOut = wharfGateOut;
		}

		void AddLinkedTransport(ForwardingConsol consol, ZString vessel, ZString voyageNo, ZString loadPort, ZString dischPort, ZDateTime storageDate)
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel;
			voyage.JV_VoyageFlight = voyageNo;
			voyage.JV_FlightDate = ZDateTime.Empty;
			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = dischPort;
			destination.JB_StorageDate = storageDate;
			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = loadPort;
			voyage.GenerateSailings();
			JobSailing sailing = voyage.Sailings[0];
			sailing.JX_JB = destination.PK;
			sailing.JX_JA = origin.PK;
			Transport transport = consol.Transports.AddNew();
			transport.JW_IsLinked = true;
			transport.JW_JX = sailing.PK;
		}

		#endregion Consol Exceptions Test

		#region Declaration Exceptions Test

		[TestDate(2000, 1, 2, 0, 0, 0)]
		public void TestSeaDeclarationMilestoneException()
		{
			TestVesselLinkedDeclarationMilestoneException(Constants.TransportModes.Sea);
		}

		[TestDate(2000, 1, 2, 0, 0, 0)]
		public void TestRailDeclarationMilestoneException()
		{
			TestVesselLinkedDeclarationMilestoneException(Constants.TransportModes.Rail);
		}

		[TestDate(2000, 1, 2, 0, 0, 0)]
		public void TestRoadDeclarationMilestoneException()
		{
			TestVesselLinkedDeclarationMilestoneException(Constants.TransportModes.Road);
		}

		void TestVesselLinkedDeclarationMilestoneException(ZString transportMode)
		{
			Enterprise.Integration.Customs.IBaseJobDeclaration declaration1 = NewDeclaration(transportMode);
			AddCusContainer(declaration1, ZBool.False, ZDateTime.Empty, ZDateTime.Now.AddDays(+1));
			AddSailingLinkedToDeclaraton(declaration1, "vessel1", "voyage1", "port0", "port1", ZDateTime.Now.AddDays(-1), false);
			Enterprise.Integration.Customs.IBaseJobDeclaration declaration2 = NewDeclaration(transportMode);
			AddCusContainer(declaration2, ZBool.False, ZDateTime.Empty, ZDateTime.Now.AddDays(-1));
			AddSailingLinkedToDeclaraton(declaration2, "vessel2", "voyage2", "port0", "port1", ZDateTime.Now.AddDays(+1), false);
			Enterprise.Integration.Customs.IBaseJobDeclaration declaration3 = NewDeclaration(transportMode);
			AddCusContainer(declaration3, ZBool.True, ZDateTime.Empty, ZDateTime.Now.AddDays(+1));
			AddSailingLinkedToDeclaraton(declaration3, "vessel3", "voyage3", "port0", "port1", ZDateTime.Now.AddDays(-1), false);
			Enterprise.Integration.Customs.IBaseJobDeclaration declaration4 = NewDeclaration(transportMode);
			AddCusContainer(declaration4, ZBool.True, ZDateTime.Empty, ZDateTime.Now.AddDays(-1));
			AddSailingLinkedToDeclaraton(declaration4, "vessel4", "voyage4", "port0", "port1", ZDateTime.Now.AddDays(+1), false);
			Enterprise.Integration.Customs.IBaseJobDeclaration declaration5 = NewDeclaration(transportMode);
			AddCusContainer(declaration5, ZBool.True, ZDateTime.Now, ZDateTime.Now.AddDays(-1));
			AddSailingLinkedToDeclaraton(declaration5, "vessel5", "voyage5", "port0", "port1", ZDateTime.Now.AddDays(-1), false);
			Enterprise.Integration.Customs.IBaseJobDeclaration declaration6 = NewDeclaration(transportMode);
			AddCusContainer(declaration6, ZBool.False, ZDateTime.Empty, ZDateTime.Now.AddDays(+1));
			AddSailingLinkedToDeclaraton(declaration6, "vessel6", "voyage6", "port0", "port1", ZDateTime.Now.AddDays(-1), true);
			Factory.Save();
			GenerateExceptions();
			GenerateExceptions();
			AssertExceptionCreated((IWorkflowProvider)declaration1, "declaration1", 1);
			AssertExceptionCreated((IWorkflowProvider)declaration2, "declaration2", 0);
			AssertExceptionCreated((IWorkflowProvider)declaration3, "declaration3", 0);
			AssertExceptionCreated((IWorkflowProvider)declaration4, "declaration4", 1);
			AssertExceptionCreated((IWorkflowProvider)declaration5, "declaration5", 0);
			AssertExceptionCreated((IWorkflowProvider)declaration6, "chartered", 0);
		}

		public void TestDeclarationWithShipmentDoesntDuplicateExceptions()
		{
			var declaration = NewDeclaration(Constants.TransportModes.Sea);
			var shipment = NewShipment(declaration);
			AddCusContainer(declaration, ZBool.True, ZDate.Empty, ZDateTime.Now.AddDays(-1));
			Factory.Save();
			GenerateExceptions();
			GenerateExceptions();
			AssertExceptionCreated((IWorkflowProvider)declaration, "declaration", 1);
		}

		Enterprise.Integration.Customs.IBaseJobDeclaration NewDeclaration(ZString transportMode)
		{
			Enterprise.Integration.Customs.IBaseJobDeclaration declaration = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			((BusinessObject)declaration)[JobDeclarationSchema.JE_TransportMode] = transportMode;
			if (transportMode != Constants.TransportModes.Sea)
			{
				((BusinessObject)declaration)[JobDeclarationSchema.JE_ContainerMode] = Constants.ContainerModes.Containerised;
			}

			return declaration;
		}

		Enterprise.Integration.Forwarding.IForwardingShipment NewShipment(Enterprise.Integration.Customs.IBaseJobDeclaration declaration)
		{
			var shipment = Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			((BusinessObject)declaration)[JobDeclarationSchema.JE_JS] = shipment.PK;
			return shipment;
		}

		void AddCusContainer(Enterprise.Integration.Customs.IBaseJobDeclaration declaration, ZBool overridden, ZDateTime wharfOut, ZDateTime storageDate)
		{
			CommonContainer container = Factory.New<CommonContainer>();
			container.JC_OverrideFCLAvailableStorage = overridden;
			if (overridden)
			{
				container.JC_ArrivalCTOStorageStartDate = storageDate;
			}

			container.JC_FCLWharfGateOut = wharfOut;
			BusinessObject cusContainer = ((IBusinessObjectCollection)((BusinessObject)declaration)["CusContainers"]).AddNew();
			cusContainer[CusContainerSchema.CO_JC] = container.PK;
		}

		JobVoyage AddSailingLinkedToDeclaraton(Enterprise.Integration.Customs.IBaseJobDeclaration declaration, ZString vessel, ZString voyageNo, ZString loadPort, ZString dischPort, ZDateTime storageDate, ZBool isCharter)
		{
			return AddSailingLinkedToDeclaraton(declaration, vessel, voyageNo, ZDateTime.Empty, loadPort, dischPort, storageDate, isCharter);
		}

		JobVoyage AddSailingLinkedToDeclaraton(Enterprise.Integration.Customs.IBaseJobDeclaration declaration, ZString vessel, ZString voyageNo, ZDateTime flightDate, ZString loadPort, ZString dischPort, ZDateTime storageDate, ZBool isCharter)
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = (ZString)((BusinessObject)declaration)[JobDeclarationSchema.JE_TransportMode];
			voyage.JV_FlightDate = flightDate;
			voyage.JV_RV_NKVessel = vessel;
			voyage.JV_VoyageFlight = voyageNo;
			voyage.JV_IsChartered = isCharter;
			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = loadPort;
			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = dischPort;
			destination.JB_StorageDate = storageDate;
			voyage.GenerateSailings();
			JobSailing sailing = voyage.Sailings[0];
			sailing.JX_JB = destination.PK;
			sailing.JX_JA = origin.PK;
			var transport = ((IRoutingSupport)declaration).Transports.AddNew();
			transport.JW_IsLinked = true;
			transport.JW_JX = sailing.PK;
			return voyage;
		}

		#endregion Declaration Exceptions Test

		#region Generator Overrides

		public void TestContainerExceptionEvent()
		{
			AssertEquals("Event", ProcessWorkflowExceptionType.ExceptionContainerStorage, new ContainerStorageExceptionGeneratorForTesting().ContainerExceptionEvent);
		}

		public void TestHighWaterMarkRegistryItem()
		{
			ForwardingConfigurationRegistry.Instance.ContainerStorageExceptionGeneratorHighWaterMark.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2011, 05, 05));
			AssertEquals("HighWaterMarkRegistryItem", new DateTime(2011, 05, 05), new ContainerStorageExceptionGeneratorForTesting().HighWaterMarkRegistryItem.Value);
		}

		#endregion Generator Overrides

		#region Implementation

		void GenerateExceptions()
		{
			ContainerStorageExceptionGenerator generator = new ContainerStorageExceptionGenerator();
			generator.Process(new NotificationBuffer());
		}

		void AssertExceptionCreated(IWorkflowProvider bizO, ZString name, ZInt excCount)
		{
			bizO.WorkflowItems.Load();
			ZInt count = bizO.WorkflowItems.Exceptions.Count;
			AssertEquals("For " + name + " " + excCount + " exception(s) expected", excCount, count);
			if (count > 0)
			{
				AssertEquals("Exception for " + name + " must have storage date exceed reason code", ProcessWorkflowExceptionType.ExceptionContainerStorage, bizO.WorkflowItems.Exceptions[0].P9_SE_NKExceptionEvent);
			}
		}

		class ContainerStorageExceptionGeneratorForTesting : ContainerStorageExceptionGenerator
		{
			public new string ContainerExceptionEvent
			{
				get
				{
					return base.ContainerExceptionEvent;
				}
			}

			public new DateTimeRegistryItem HighWaterMarkRegistryItem
			{
				get
				{
					return base.HighWaterMarkRegistryItem;
				}
			}
		}

		#endregion Implementation
	}
}
