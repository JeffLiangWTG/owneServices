using System;
using CargoWise.IO;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	[TestedType(typeof(ContainerDataContextManager))]
	sealed class ContainerDataContextManagerTest : ShipmentDataContextManagerTestCase<ContainerDataContextManager, CommonContainer>
	{
		public void TestDataContextType()
		{
			AssertEquals(DataContextType.ForwardingContainer, GetNewDataContextManager().DataContextType);
		}

		public void TestDataContextKey()
		{
			var consol = Factory.New<CommonConsol>();
			var container = consol.Containers.AddNew();
			container.JC_ContainerJobID = "D00001000";

			var manager = GetNewDataContextManager();
			((IDataContextManager)manager).Init(container);

			AssertEquals("D00001000", manager.DataContextKey);
		}

		public void TestManagesShipmentsManagesEvents()
		{
			var manager = GetNewDataContextManager();
			AssertEquals(true, manager.ManagesShipments);
			AssertEquals(true, manager.ManagesEvents);
		}

		public void TestDefaultOutputDirectory()
		{
			const string testDirectory = @"c:\Test";
			SystemDataRegistry.Instance.ShipmentExportDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testDirectory);

			AssertEquals(testDirectory, GetNewDataContextManager().DefaultOutputDirectory);
		}

		#region FRPort

		public void TestFRPort_ProcessATH()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_UniqueConsignRef = "C00001321";
			consol.JK_MasterBillNum = "BLTCT784512";

			var ctoAddress = Factory.NewWithValidTestData<OrgHeader>();
			ctoAddress.OH_FullName = "CTO";
			ctoAddress.OH_RL_NKClosestPort = "FRLEH";
			ctoAddress.MainAddress.OA_RN_NKCountryCode = "FR";
			consol.JK_OA_ArrivalCTOAddress = ctoAddress.MainAddress.PK;

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "WTGU2211290";

			Factory.SaveForTesting();

			AssertNullOrEmpty("Pre-condition: JC_ContainerImportDORelease has no value.", container.JC_ContainerImportDORelease);

			const string universalEvent = @"
<UniversalEvent>
	<Event>
		<EventTime>2022-11-29T08:05:13</EventTime>
		<EventType>ATH</EventType>
		<EventParameters>
			<Department>Terminal</Department>
			<EquipmentReferenceNumber>WTGU2211290</EquipmentReferenceNumber>
			<Type>Container Release</Type>
			<MessageType>Token Code for Trucker</MessageType>
			<ReferenceNumber>TCT985312</ReferenceNumber>
			<Facility>CTO</Facility>
			<Status>Accepted</Status>
			<Location>FRLEH</Location>
		</EventParameters>
		<ContextCollection>
			<Context>
				<Type>CarrierCode</Type>
				<Value>AMSC</Value>
			</Context>
			<Context>
				<Type>LloydsNumber</Type>
				<Value>9484455</Value>
			</Context>
			<Context>
				<Type>VesselName</Type>
				<Value>MSC RAPALLO</Value>
			</Context>
			<Context>
				<Type>MBOLNumber</Type>
				<Value>BLTCT784512</Value>
			</Context>
			<Context>
				<Type>ContainerNumber</Type>
				<Value>WTGU2211290</Value>
			</Context>
			<Context>
				<Type>ContainerISOCode</Type>
				<Value>45G1</Value>
			</Context>
			<Context>
				<Type>EventSource</Type>
				<Value>S-ONE</Value>
			</Context>
			<Context>
				<Type>AMQReference</Type>
				<Value>CNI0000923200</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";

			var message = GetQueuedUniversalEventMessage(universalEvent);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertEquals("TCT985312", container.JC_ContainerImportDORelease);
		}

		public void TestFRPort_ProcessATW()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_UniqueConsignRef = "C00001321";
			consol.JK_MasterBillNum = "BLTCT784512";

			var ctoAddress = Factory.NewWithValidTestData<OrgHeader>();
			ctoAddress.OH_FullName = "CTO";
			ctoAddress.OH_RL_NKClosestPort = "FRLEH";
			ctoAddress.MainAddress.OA_RN_NKCountryCode = "FR";
			consol.JK_OA_ArrivalCTOAddress = ctoAddress.MainAddress.PK;

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "WTGU2211290";
			container.JC_ContainerImportDORelease = "TCT985312";

			Factory.SaveForTesting();

			AssertEquals("Pre-condition: JC_ContainerImportDORelease has value.", "TCT985312", container.JC_ContainerImportDORelease);

			const string universalEvent = @"
<UniversalEvent>
	<Event>
		<EventTime>2022-11-29T08:15:13</EventTime>
		<EventType>ATW</EventType>
		<EventParameters>
			<Department>Terminal</Department>
			<EquipmentReferenceNumber>WTGU2211290</EquipmentReferenceNumber>
			<Type>Container Release</Type>
			<MessageType>Token Code for Trucker</MessageType>
			<Facility>CTO</Facility>
			<Status>Accepted</Status>
			<Location>FRLEH</Location>
		</EventParameters>
		<ContextCollection>
			<Context>
				<Type>CarrierCode</Type>
				<Value>AMSC</Value>
			</Context>
			<Context>
				<Type>LloydsNumber</Type>
				<Value>9484455</Value>
			</Context>
			<Context>
				<Type>VesselName</Type>
				<Value>MSC RAPALLO</Value>
			</Context>
			<Context>
				<Type>MBOLNumber</Type>
				<Value>BLTCT784512</Value>
			</Context>
			<Context>
				<Type>ContainerNumber</Type>
				<Value>WTGU2211290</Value>
			</Context>
			<Context>
				<Type>ContainerISOCode</Type>
				<Value>45G1</Value>
			</Context>
			<Context>
				<Type>EventSource</Type>
				<Value>S-ONE</Value>
			</Context>
			<Context>
				<Type>AMQReference</Type>
				<Value>CNI0000923200</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";

			var message = GetQueuedUniversalEventMessage(universalEvent);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertNullOrEmpty(container.JC_ContainerImportDORelease);
		}

		public void TestFRPort_LPD_ProcessSTU()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_UniqueConsignRef = "C00001321";
			consol.JK_MasterBillNum = "COSU6349027420";

			var ctoAddress = Factory.New<OrgHeader>();
			ctoAddress.OH_FullName = "CTO";
			ctoAddress.OH_RL_NKClosestPort = "FRMAR";
			ctoAddress.MainAddress.Address1 = "House 16777214";
			ctoAddress.MainAddress.Address2 = "Coelosis inermis";
			ctoAddress.MainAddress.City = "The Big City";
			ctoAddress.MainAddress.Postcode = "1234";
			ctoAddress.MainAddress.OA_RN_NKCountryCode = "FR";
			consol.JK_OA_ArrivalCTOAddress = ctoAddress.MainAddress.PK;

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "BEAU4701325";

			Factory.SaveForTesting();

			AssertNullOrEmpty("Pre-condition: JC_ImportDepotCustomsReference has no value.", container.JC_ImportDepotCustomsReference);

			const string universalEvent = @"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Key>C00001321</Key>
					<Type>ForwardingConsol</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2023-01-17T11:11:00</EventTime>
		<EventType>STU</EventType>
		<EventParameters>
			<Department>Terminal</Department>
			<Type>LPD Notification</Type>
			<Status>VAL</Status>
			<EquipmentReferenceNumber>BEAU4701325</EquipmentReferenceNumber>
			<CustomsReferenceNumber>CNI03743974</CustomsReferenceNumber>
		</EventParameters>
		<ContextCollection>
			<Context>
				<Type>MBOLNumber</Type>
				<Value>COSU6349027420</Value>
			</Context>
			<Context>
				<Type>ContainerNumber</Type>
				<Value>BEAU4701325</Value>
			</Context>
			<Context>
				<Type>ContainerISOCode</Type>
				<Value>45G1</Value>
			</Context>
			<Context>
				<Type>DOCReference</Type>
				<Value>DAI0001827919</Value>
			</Context>
			<Context>
				<Type>OTCReference</Type>
				<Value>VAG0000163733</Value>
			</Context>
			<Context>
				<Type>ATPReference</Type>
				<Value>VOS0000170298</Value>
			</Context>
			<Context>
				<Type>LPDReference</Type>
				<Value>FDC0002585387</Value>
			</Context>
			<Context>
				<Type>VoyageNumber</Type>
				<Value>AEU2Q4X030W</Value>
			</Context>
			<Context>
				<Type>VesselName</Type>
				<Value>CMA CGM ZHENG HE</Value>
			</Context>
			<Context>
				<Type>PortArea</Type>
				<Value>LEH</Value>
			</Context>
			<Context>
				<Type>PortLocation</Type>
				<Value>MBFA</Value>
			</Context>
			<Context>
				<Type>TerminalCode</Type>
				<Value>MBFA</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";

			var message = GetQueuedUniversalEventMessage(universalEvent);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertEquals("CNI03743974", container.JC_ImportDepotCustomsReference);
		}

		public void TestFRPort_LDE_ProcessSTU()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_UniqueConsignRef = "C00001321";
			consol.JK_MasterBillNum = "COSU6349027420";

			var ctoAddress = Factory.New<OrgHeader>();
			ctoAddress.OH_FullName = "CTO";
			ctoAddress.OH_RL_NKClosestPort = "FRMAR";
			ctoAddress.MainAddress.Address1 = "House 16777214";
			ctoAddress.MainAddress.Address2 = "Coelosis inermis";
			ctoAddress.MainAddress.City = "The Big City";
			ctoAddress.MainAddress.Postcode = "1234";
			ctoAddress.MainAddress.OA_RN_NKCountryCode = "FR";
			consol.JK_OA_ArrivalCTOAddress = ctoAddress.MainAddress.PK;

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "BEAU4701325";

			Factory.SaveForTesting();

			AssertNullOrEmpty("Pre-condition: JC_ExportDepotCustomsReference has no value.", container.JC_ExportDepotCustomsReference);

			const string universalEvent = @"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Key>C00001321</Key>
					<Type>ForwardingConsol</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2023-01-17T11:11:00</EventTime>
		<EventType>STU</EventType>
		<EventParameters>
			<Department>Terminal</Department>
			<Type>LDE Notification</Type>
			<Status>VAL</Status>
			<EquipmentReferenceNumber>BEAU4701325</EquipmentReferenceNumber>
			<CustomsReferenceNumber>CNI03743974</CustomsReferenceNumber>
		</EventParameters>
		<ContextCollection>
			<Context>
				<Type>MBOLNumber</Type>
				<Value>COSU6349027420</Value>
			</Context>
			<Context>
				<Type>ContainerNumber</Type>
				<Value>BEAU4701325</Value>
			</Context>
			<Context>
				<Type>ContainerISOCode</Type>
				<Value>45G1</Value>
			</Context>
			<Context>
				<Type>DOCReference</Type>
				<Value>DAI0001827919</Value>
			</Context>
			<Context>
				<Type>OTCReference</Type>
				<Value>VAG0000163733</Value>
			</Context>
			<Context>
				<Type>ATPReference</Type>
				<Value>VOS0000170298</Value>
			</Context>
			<Context>
				<Type>LPDReference</Type>
				<Value>FDC0002585387</Value>
			</Context>
			<Context>
				<Type>VoyageNumber</Type>
				<Value>AEU2Q4X030W</Value>
			</Context>
			<Context>
				<Type>VesselName</Type>
				<Value>CMA CGM ZHENG HE</Value>
			</Context>
			<Context>
				<Type>PortArea</Type>
				<Value>LEH</Value>
			</Context>
			<Context>
				<Type>PortLocation</Type>
				<Value>MBFA</Value>
			</Context>
			<Context>
				<Type>TerminalCode</Type>
				<Value>MBFA</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";

			var message = GetQueuedUniversalEventMessage(universalEvent);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertEquals("CNI03743974", container.JC_ExportDepotCustomsReference);
		}

		#endregion

		#region BEPortCPU

		public void TestBEPortCPu_ProcessATH()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_UniqueConsignRef = "C00001321";

			var ctoAddress = Factory.New<OrgHeader>();
			ctoAddress.OH_FullName = "CTO";
			ctoAddress.OH_RL_NKClosestPort = "BEANR";
			ctoAddress.MainAddress.Address1 = "House 16777214";
			ctoAddress.MainAddress.Address2 = "Coelosis inermis";
			ctoAddress.MainAddress.City = "The Big City";
			ctoAddress.MainAddress.Postcode = "1234";
			ctoAddress.MainAddress.OA_RN_NKCountryCode = "BE";
			consol.JK_OA_ArrivalCTOAddress = ctoAddress.MainAddress.PK;

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "1111";

			Factory.SaveForTesting();

			AssertNullOrEmpty("Pre-condition: JC_ContainerImportDORelease has no value.", container.JC_ContainerImportDORelease);

			const string universalEvent = @"
<UniversalEvent>
	<Event>
		<DataContext>
			<DocumentaryOverride>
				<DocumentName>Certified Pickup - ReleaseRight</DocumentName>
			</DocumentaryOverride>
			<DataTargetCollection>
				<DataTarget>
					<Key>C00001321</Key>
					<Type>ForwardingConsol</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2021-04-26T07:16:51</EventTime>
		<EventType>ATH</EventType>
		<EventParameters>
			<Department>Terminal</Department>
			<EquipmentReferenceNumber>1111</EquipmentReferenceNumber>
			<Type>Container Release</Type>
			<ReferenceNumber>REL210426_2</ReferenceNumber>
			<Facility>CTO</Facility>
			<Location>BEANR</Location>
			<Status>Transferred</Status>
		</EventParameters>
		<ContextCollection>
			<Context>
				<Type>ReleaseFromParty</Type>
				<Value>WISETECH GLOBAL LIMITED</Value>
			</Context>
			<Context>
				<Type>ReleaseFromPartyId</Type>
				<Value>nxtEntityId</Value>
			</Context>
			<Context>
				<Type>ReleaseFromPartyCode</Type>
				<Value>NXT20000051292</Value>
			</Context>
			<Context>
				<Type>CarrierCode</Type>
				<Value>NXT21000060365</Value>
			</Context>
			<Context>
				<Type>ContainerNumber</Type>
				<Value>1111</Value>
			</Context>
			<Context>
				<Type>MBOLNumber</Type>
				<Value>BL2104261</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";

			var message = GetQueuedUniversalEventMessage(universalEvent);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertEquals("REL210426_2", container.JC_ContainerImportDORelease);
		}

		public void TestBEPortCPu_ProcessATW()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_UniqueConsignRef = "C00001321";

			var ctoAddress = Factory.New<OrgHeader>();
			ctoAddress.OH_FullName = "CTO";
			ctoAddress.OH_RL_NKClosestPort = "BEANR";
			ctoAddress.MainAddress.Address1 = "House 16777214";
			ctoAddress.MainAddress.Address2 = "Coelosis inermis";
			ctoAddress.MainAddress.City = "The Big City";
			ctoAddress.MainAddress.Postcode = "1234";
			ctoAddress.MainAddress.OA_RN_NKCountryCode = "BE";
			consol.JK_OA_ArrivalCTOAddress = ctoAddress.MainAddress.PK;

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "1111";
			container.JC_ContainerImportDORelease = "REL210426_2";

			Factory.SaveForTesting();

			AssertEquals("Pre-condition: JC_ContainerImportDORelease has value.", "REL210426_2", container.JC_ContainerImportDORelease);

			const string universalEvent = @"
<UniversalEvent>
	<Event>
		<DataContext>
			<DocumentaryOverride>
				<DocumentName>Certified Pickup - ReleaseRight</DocumentName>
			</DocumentaryOverride>
			<DataTargetCollection>
				<DataTarget>
					<Key>C00001321</Key>
					<Type>ForwardingConsol</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2021-04-26T07:16:51</EventTime>
		<EventType>ATW</EventType>
		<EventParameters>
			<Department>Terminal</Department>
			<EquipmentReferenceNumber>1111</EquipmentReferenceNumber>
			<Type>Container Release</Type>
			<Facility>CTO</Facility>
			<Location>BEANR</Location>
			<Status>Transferred</Status>
		</EventParameters>
		<ContextCollection>
			<Context>
				<Type>ReleaseFromParty</Type>
				<Value>WISETECH GLOBAL LIMITED</Value>
			</Context>
			<Context>
				<Type>ReleaseFromPartyId</Type>
				<Value>nxtEntityId</Value>
			</Context>
			<Context>
				<Type>ReleaseFromPartyCode</Type>
				<Value>NXT20000051292</Value>
			</Context>
			<Context>
				<Type>CarrierCode</Type>
				<Value>NXT21000060365</Value>
			</Context>
			<Context>
				<Type>ContainerNumber</Type>
				<Value>1111</Value>
			</Context>
			<Context>
				<Type>MBOLNumber</Type>
				<Value>BL2104261</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";

			var message = GetQueuedUniversalEventMessage(universalEvent);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertNullOrEmpty(container.JC_ContainerImportDORelease);
		}

		public void TestBEPortCPu_WrongDocumentName()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_UniqueConsignRef = "C00001321";
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "1111";

			Factory.SaveForTesting();

			AssertNullOrEmpty("Pre-condition: JC_ContainerImportDORelease has no value.", container.JC_ContainerImportDORelease);

			const string universalEvent = @"
<UniversalEvent>
	<Event>
		<DataContext>
			<DocumentaryOverride>
				<DocumentName>NOT Certified Pickup - ReleaseRight</DocumentName>
			</DocumentaryOverride>
			<DataTargetCollection>
				<DataTarget>
					<Key>C00001321</Key>
					<Type>ForwardingConsol</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2021-04-26T07:16:51</EventTime>
		<EventType>ATH</EventType>
		<EventParameters>
			<Department>Terminal</Department>
			<EquipmentReferenceNumber>1111</EquipmentReferenceNumber>
			<Type>Container Release</Type>
			<ReferenceNumber>REL210426_2</ReferenceNumber>
			<Facility>CTO</Facility>
			<Location>BEANR</Location>
			<Status>Transferred</Status>
		</EventParameters>
		<ContextCollection>
			<Context>
				<Type>ReleaseFromParty</Type>
				<Value>WISETECH GLOBAL LIMITED</Value>
			</Context>
			<Context>
				<Type>ReleaseFromPartyId</Type>
				<Value>nxtEntityId</Value>
			</Context>
			<Context>
				<Type>ReleaseFromPartyCode</Type>
				<Value>NXT20000051292</Value>
			</Context>
			<Context>
				<Type>CarrierCode</Type>
				<Value>NXT21000060365</Value>
			</Context>
			<Context>
				<Type>ContainerNumber</Type>
				<Value>1111</Value>
			</Context>
			<Context>
				<Type>MBOLNumber</Type>
				<Value>BL2104261</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";

			var message = GetQueuedUniversalEventMessage(universalEvent);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertNullOrEmpty(container.JC_ContainerImportDORelease);
		}

		#endregion

		#region Implementation

		protected override CommonContainer GetNewBusinessObjectForTesting()
		{
			return Factory.New<CommonContainer>();
		}

		ContainerDataContextManager GetNewDataContextManager()
		{
			return new ContainerDataContextManager();
		}

		protected override RecipientRoleType[] SupportedRecipientRoleTypes => Array.Empty<RecipientRoleType>();

		protected override string ValidPopulatedUniversalShipmentXML
		{
			get
			{
				using (var resourceRetriever = new EmbeddedResourceRetriever())
				{
					return resourceRetriever.GetString("Enterprise.Freight.DataTransfer.Test.Freight.Universal.TestFiles.FreightContainer_UniversalShipment.xml");
				}
			}
		}

		protected override bool ManagerChecksDataTargetToImport
		{
			get { return false; }
		}

		#endregion
	}
}
