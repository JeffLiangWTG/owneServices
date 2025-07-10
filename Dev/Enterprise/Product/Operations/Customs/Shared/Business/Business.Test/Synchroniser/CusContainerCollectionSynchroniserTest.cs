using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusContainerCollectionSynchroniserTest : SynchroniserTestCase
	{
		public void TestSynchroniseContainers()
		{
			var consol1 = Factory.New<ForwardingConsol>();
			consol1.JK_RL_NKLoadPort = "ERABC";
			consol1.JK_TransportMode = Core.Constants.TransportModes.Sea;
			var shipment = consol1.Shipments.AddNew();
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.ShipmentSynchroniser.SetEnabled(false, false);

			var jobContainer1 = consol1.Containers.AddNew();
			jobContainer1.JC_SealNum = "1";
			jobContainer1.JC_AdditionalSealNum = "11";
			var jobContainer2 = consol1.Containers.AddNew();
			jobContainer2.JC_SealNum = "2";
			jobContainer2.JC_AdditionalSealNum = "22";
			var jobContainer3 = consol1.Containers.AddNew();
			jobContainer3.JC_SealNum = "3";
			jobContainer3.JC_AdditionalSealNum = "33";
			var jobContainer4 = consol1.Containers.AddNew();
			jobContainer4.JC_SealNum = "4";
			jobContainer4.JC_AdditionalSealNum = "44";
			var jobContainer5 = consol1.Containers.AddNew();
			jobContainer5.JC_SealNum = "5";
			jobContainer5.JC_AdditionalSealNum = "55";

			var cusContainer1 = declaration.CusContainers.AddNew();
			var cusContainer2 = declaration.CusContainers.AddNew();
			var cusContainer3 = declaration.CusContainers.AddNew();
			var cusContainer4 = declaration.CusContainers.AddNew();

			cusContainer1.CO_JC = jobContainer5.PK;
			cusContainer2.CO_JC = ZGuid.Empty;
			cusContainer3.CO_JC = jobContainer1.PK;
			cusContainer4.CO_JC = ZGuid.Invalid;

			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_JC = jobContainer1.PK;
			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_JC = jobContainer2.PK;
			var packLine3 = shipment.OuterPackLines.AddNew();
			packLine3.JL_JC = jobContainer3.PK;
			var packLine4 = shipment.OuterPackLines.AddNew();
			packLine4.JL_JC = jobContainer4.PK;
			var packLine5 = shipment.OuterPackLines.AddNew();
			packLine5.JL_JC = jobContainer5.PK;

			declaration.ShipmentSynchroniser.SetEnabled(true, false);
			declaration.ShipmentSynchroniser.Synchronise(true);
			AssertEquals(5, declaration.CusContainers.Count);
			AssertEquals(jobContainer1.PK, cusContainer3.CO_JC);
			AssertEquals("1", cusContainer3.CO_Seal);
			AssertEquals("11", cusContainer3.CO_SecondSeal);
			AssertEquals(jobContainer2.PK, cusContainer2.CO_JC);
			AssertEquals("2", cusContainer2.CO_Seal);
			AssertEquals("22", cusContainer2.CO_SecondSeal);
			AssertEquals(jobContainer3.PK, cusContainer4.CO_JC);
			AssertEquals("3", cusContainer4.CO_Seal);
			AssertEquals("33", cusContainer4.CO_SecondSeal);
			AssertEquals(jobContainer5.PK, cusContainer1.CO_JC);
			AssertEquals("5", cusContainer1.CO_Seal);
			AssertEquals("55", cusContainer1.CO_SecondSeal);
			AssertEquals(jobContainer4.PK, declaration.CusContainers[4].CO_JC);
			AssertEquals("4", declaration.CusContainers[4].CO_Seal);
			AssertEquals("44", declaration.CusContainers[4].CO_SecondSeal);
		}

		public void TestSynchroniseContainersIfContainerNumberEmpty()
		{
			var consol1 = Factory.New<ForwardingConsol>();
			consol1.JK_RL_NKLoadPort = "ERABC";
			consol1.JK_TransportMode = Core.Constants.TransportModes.Sea;
			var container1 = consol1.Containers.AddNew();
			container1.JC_ContainerNum = "";
			var container2 = consol1.Containers.AddNew();
			container2.JC_ContainerNum = "";
			var shipment = consol1.Shipments.AddNew();
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_JC = container1.PK;
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.ShipmentSynchroniser.SetEnabled(true, false);
			declaration.ShipmentSynchroniser.Synchronise(true);
			AssertEquals(1, declaration.CusContainers.Count);
			var cusContainer = declaration.CusContainers[0];
			AssertEquals(container1.PK, cusContainer.CO_JC);
			AssertEquals(2, packLine.AllContainersOnShipment_List.Count);
			Factory.Save();

			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_JC = ZGuid.Empty;
			packLine2.JL_JC = container2.PK;
			AssertEquals(2, declaration.CusContainers.Count);
			cusContainer = declaration.CusContainers[1];
			AssertEquals(container2.PK, cusContainer.CO_JC);
			AssertEquals(2, packLine2.CurrentConsol.Containers.Count);
			AssertEquals(container1, packLine2.CurrentConsol.Containers[0]);
			AssertEquals(container2, packLine2.CurrentConsol.Containers[1]);
		}

		public void TestContainerIsReSynchroniseCorrectlyWhenAttachingToDifferentConsol()
		{
			var consol1 = Factory.New<ForwardingConsol>();
			consol1.JK_RL_NKLoadPort = "ERABC";
			consol1.JK_TransportMode = Core.Constants.TransportModes.Sea;
			var container1 = consol1.Containers.AddNew();
			container1.JC_ContainerNum = "CONT323423";
			var consol2 = Factory.New<ForwardingConsol>();
			consol2.JK_RL_NKLoadPort = "ERABC";
			consol2.JK_TransportMode = Core.Constants.TransportModes.Sea;
			var container2 = consol2.Containers.AddNew();
			container2.JC_ContainerNum = "CONT323423";
			var shipment = consol1.Shipments.AddNew();
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 1;
			packLine.SetContainer(container1.PK);
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.ShipmentSynchroniser.SetEnabled(true, false);
			declaration.ShipmentSynchroniser.Synchronise(true);
			AssertEquals(1, declaration.CusContainers.Count);
			var cusContainer = declaration.CusContainers[0];
			AssertEquals(container1.PK, cusContainer.CO_JC);
			Factory.Save();
			consol1.Shipments.Remove(shipment);
			AssertEquals(false, cusContainer.IsDeleted);
			AssertEquals(ZGuid.Empty, cusContainer.CO_JC);
			consol2.Shipments.Add(shipment);
			AssertEquals(true, cusContainer.IsDeleted);
			Factory.Save();
			var factory = new BusinessObjectFactory();
			declaration = factory.Load<BaseJobDeclaration>(declaration.PK);
			((Integration.Customs.IJobDeclarationWithShipmentSynchonisation)declaration).SynchroniseWithShipmentIfNeeded(); // Mimick plugin to shipment
			AssertEquals(1, declaration.CusContainers.Count);
			cusContainer = declaration.CusContainers[0];
			AssertEquals(container2.PK, cusContainer.CO_JC);
		}

		public void TestSynchroniseContainersForRepackedShipmentToAnotherContainerForImport()
		{
			var departurePort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
			var repackingPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, new[] { GlbCompany.CurrentCompany.GC_RN_NKCountryCode, departurePort.RL_RN_NKCountryCode }));
			var localPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));

			SetUp(departurePort.Code, repackingPort.Code, localPort.Code);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.ShipmentSynchroniser.Synchronise(true);
			AssertEquals("Container count on import", 1, declaration.CusContainers.Count);
			AssertEquals("For import declaration should synchronise with arrival container", ContainerNumber2, declaration.CusContainers[0].CO_ContainerNumber);
		}

		public void TestSynchroniseContainersForRepackedShipmentToAnotherContainerForExport()
		{
			var departurePort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
			var repackingPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, new[] { GlbCompany.CurrentCompany.GC_RN_NKCountryCode, departurePort.RL_RN_NKCountryCode }));
			var localPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));

			SetUp(localPort.Code, repackingPort.Code, departurePort.Code);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.ShipmentSynchroniser.Synchronise(true);
			AssertEquals("Container count on export", 1, declaration.CusContainers.Count);
			AssertEquals("For export declaration should synchronise with departure container", ContainerNumber1, declaration.CusContainers[0].CO_ContainerNumber);
		}

		public void TestSynchroniseContainersForRepackedTranshipmentToAnotherContainer()
		{
			var port1 = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
			var port2 = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, new[] { GlbCompany.CurrentCompany.GC_RN_NKCountryCode, port1.RL_RN_NKCountryCode }));
			var localPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));

			SetUp(port1.Code, localPort.Code, port2.Code);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.ShipmentSynchroniser.Synchronise(true);
			AssertEquals("Container count on export", 1, declaration.CusContainers.Count);
			AssertEquals("For export declaration should synchronise with departure container", ContainerNumber2, declaration.CusContainers[0].CO_ContainerNumber);

			var container3 = consol2.Containers.AddNew();
			container3.JC_RC = containerType.PK;
			container3.JC_ContainerNum = ContainerNumber3;

			var packLine3 = shipment.OuterPackLines.AddNew();
			packLine3.SetContainer(consol2, container3);

			AssertEquals("Container Count", 2, declaration.CusContainers.Count);
			declaration.CusContainers.Sort(BaseCusContainer.Schema.CO_ContainerNumber);
			const string message = "For export shipment should synchronise with departure containers";
			AssertEquals(message, ContainerNumber3, declaration.CusContainers[0].CO_ContainerNumber);
			AssertEquals(message, ContainerNumber2, declaration.CusContainers[1].CO_ContainerNumber);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Container count on import", 1, declaration.CusContainers.Count);
			AssertEquals("For import declaration should synchronise with arrival container", ContainerNumber1, declaration.CusContainers[0].CO_ContainerNumber);

			declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertEquals("If declaration type is not import nor export and more than one consol should not synchronise", 0, declaration.CusContainers.Count);
		}

		public void TestGetSourceContainersToSynchroniseForBuyersConsol()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
				consol.JK_MasterBillNum = "masterbill";
				consol.JK_RL_NKLoadPort = "GBLON";
				consol.JK_RL_NKDischargePort = "AUSYD";
				consol.JK_ConsolMode = "BCN";
				var leadShipment = consol.Shipments.AddNew();
				leadShipment.JS_RL_NKDestination = "AUSYD";
				leadShipment.JS_HouseBill = "H1";
				leadShipment.JS_PackingMode = "BCN";
				leadShipment.JS_ShipmentType = "BCN";
				var childShipment = consol.Shipments.AddNew();
				childShipment.JS_PackingMode = "BCN";
				childShipment.JS_ShipmentType = "STD";
				childShipment.JS_RL_NKDestination = "AUSYD";
				childShipment.JS_JS_ColoadMasterShipment = leadShipment.PK;
				childShipment.JS_HouseBill = "H2";

				var c1 = consol.Containers.AddNew();
				c1.JC_ContainerNum = "C1";
				var c2 = consol.Containers.AddNew();
				c2.JC_ContainerNum = "C2";

				var p1 = leadShipment.OuterPackLines.AddNew();
				p1.JL_JC = c1.PK;
				var p2 = childShipment.OuterPackLines.AddNew();
				p2.JL_JC = c2.PK;

				declaration = BaseJobDeclaration.New(Factory);
				declaration.JE_JS = leadShipment.PK;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.ShipmentSynchroniser.Synchronise(true);

				AssertEquals(2, declaration.Packages.Count);
				AssertEquals(true,
						("C1" == declaration.Packages[0].CW_ContainerNoOrEquipmentNo && "C2" == declaration.Packages[1].CW_ContainerNoOrEquipmentNo)
						||
						("C2" == declaration.Packages[0].CW_ContainerNoOrEquipmentNo && "C1" == declaration.Packages[1].CW_ContainerNoOrEquipmentNo)
						);
			}
		}

		public void TestGetSourceContainersToSynchroniseForBuyersConsolDoesNotDuplicate()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_AgentType = "OTH";
				consol.JK_ConsolMode = "OTH";
				consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
				consol.JK_MasterBillNum = "masterbill";
				consol.JK_RL_NKLoadPort = "GBLON";
				consol.JK_RL_NKDischargePort = "USLAX";
				var leadShipment = consol.Shipments.AddNew();
				leadShipment.JS_RL_NKDestination = "USLAX";
				leadShipment.JS_HouseBill = "H1";
				leadShipment.JS_PackingMode = "BCN";
				leadShipment.JS_ShipmentType = "BCN";
				leadShipment.JS_OuterPacks = 10;
				var childShipment = consol.Shipments.AddNew();
				childShipment.JS_PackingMode = "FCL";
				childShipment.JS_ShipmentType = "STD";
				childShipment.JS_RL_NKDestination = "USLAX";
				childShipment.JS_JS_ColoadMasterShipment = leadShipment.PK;
				childShipment.JS_HouseBill = "H2";
				childShipment.JS_OuterPacks = 10;

				var c1 = consol.Containers.AddNew();
				c1.JC_ContainerNum = "C1";

				var p1 = leadShipment.OuterPackLines.AddNew();
				p1.JL_JC = c1.PK;
				var p2 = childShipment.OuterPackLines.AddNew();
				p2.JL_JC = c1.PK;

				declaration = BaseJobDeclaration.New(Factory);
				declaration.JE_JS = leadShipment.PK;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.ShipmentSynchroniser.Synchronise(true);

				AssertEquals("Should see one container on declaration, should not see two by mistake (doubled up)", 1, declaration.CusContainers.Count);
			}
		}

		void SetUp(ZString departurePort, ZString repackingPort, ZString destinationPort)
		{
			containerType = Factory.LoadFromNaturalKey(typeof(RefContainer), RefContainerSchema.RC_Code, "20NOR");

			shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_RL_NKOrigin = departurePort;
			shipment.JS_RL_NKDestination = destinationPort;

			var consol1 = shipment.Consols.AddNew();
			consol1.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol1.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol1.JK_ConsolMode = Core.Constants.ContainerModes.LCL;
			consol1.JK_RL_NKLoadPort = departurePort;
			consol1.JK_RL_NKDischargePort = repackingPort;

			var container1 = consol1.Containers.AddNew();
			container1.JC_RC = containerType.PK;
			container1.JC_ContainerNum = ContainerNumber1;

			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.SetContainer(consol1, container1);

			consol2 = shipment.Consols.AddNew();
			consol2.JK_AgentType = Core.Constants.AgentType.Agent;
			consol2.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol2.JK_ConsolMode = Core.Constants.ContainerModes.Groupage;

			consol2.JK_RL_NKLoadPort = repackingPort;
			consol2.JK_RL_NKDischargePort = destinationPort;

			var container2 = consol2.Containers.AddNew();
			container2.JC_RC = containerType.PK;
			container2.JC_ContainerNum = ContainerNumber2;

			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.SetContainer(consol2, container2);

			declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_JS = shipment.PK;

			Factory.Save(); // To stop the JobContainer being deleted when not attached to a consol
		}

		const string ContainerNumber1 = "CNHK1234567";
		const string ContainerNumber2 = "CRXU1234569";
		const string ContainerNumber3 = "CNHK1234569";
		BaseJobDeclaration declaration;
		BusinessObject containerType;
		ForwardingConsol consol2;
		ForwardingShipment shipment;
	}
}
