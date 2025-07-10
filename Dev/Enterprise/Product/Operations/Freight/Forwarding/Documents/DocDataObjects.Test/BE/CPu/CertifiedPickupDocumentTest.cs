using System.Collections.Generic;
using System.Xml.Linq;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.BE;
using Enterprise.Freight.Forwarding.Documents.Testing.BE;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Integration;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	sealed class CertifiedPickupDocumentTest : DocumentVisualizer.Testing.StandardDocumentContentTest
	{
		public override void TestDocumentContent()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			PopulateConsol(consol);

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONT1111111";
			container1.JC_ContainerImportDORelease = "CONT1DORelease";
			CertifiedPickupContainerEventTestHelper.AddLogForContainer(container1, CertifiedPickupConstants.Status.DeclinedByNextPartyForTransferRevoke);

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CONT2222222";
			container2.JC_ContainerImportDORelease = "CONT2DORelease";
			var container2Log = CertifiedPickupContainerEventTestHelper.AddLogForContainer(container2, CertifiedPickupConstants.Status.Assigned);
			AddEDIMessageForATHEvent(container2Log, container2.JC_ContainerNum, "cw1EntityId", "WISETECH", "cw1202304288");

			var containers = new List<ForwardingContainer>() { container1, container2 };
			var acceptDeclineDocDataObjectParameters = new DocDataObjectParameters(BelgianPortsConstants.DocumentNames.CPuReleaseRightAcceptDecline, DataContext.BECertifiedPickup, containers);
			AssertContents(consol, BelgianPortsConstants.DocumentNames.CPuReleaseRightAcceptDecline, CreateContentAcceptDecline(), acceptDeclineDocDataObjectParameters);

			var revokeDocDataObjectParameters = new DocDataObjectParameters(BelgianPortsConstants.DocumentNames.CPuReleaseRightRevoke, DataContext.BECertifiedPickup, containers);
			AssertContents(consol, BelgianPortsConstants.DocumentNames.CPuReleaseRightRevoke, CreateContentRevoke(), revokeDocDataObjectParameters);

			var transferDocDataObjectParameters = new DocDataObjectParameters(BelgianPortsConstants.DocumentNames.CPuReleaseRightTransfer, DataContext.BECertifiedPickup, containers);
			AssertContents(consol, BelgianPortsConstants.DocumentNames.CPuReleaseRightTransfer, CreateContentTransfer(), transferDocDataObjectParameters);
		}

		string CreateContentAcceptDecline() => $@"[2,4] Sending Party
[2,16] Operational Port
[2,27] 
Notification Import
[2,28] Certified PickUp
Release Right
[3,4] EDI CUSTOMS BROKERS
[3,16] BRSAO - Sao Paulo
[4,4] 10 HUTCHESON STREET
[4,16] Terminal
[5,4] ALBION  QLD
[6,16] BOL Number(s)
[7,4] AUSTRALIA
[7,12] 4010
[7,16] 21598757411
[8,4] Sending Party ID
[8,16] Carrier ID
[9,4] BTW
[9,16] BTW
[11,4] Equipment Details
[11,16] Status:
[11,22] Certified Pickup - Transfer Message Rejected, CONT1111111
[12,4] Container
[12,16] Release Identification
[12,28] Accept / Decline
[13,4] CONT1111111
[13,16] CONT1DORelease
[13,28] ☐ Accept
[13,40] ☐ Decline
[14,4] Received From
[15,4] Reason
[16,4] Equipment Details
[16,16] Status:
[17,4] Container
[17,16] Release Identification
[17,28] Accept / Decline
[18,4] CONT2222222
[18,16] CONT2DORelease
[18,28] ☑ Accept
[18,40] ☐ Decline
[19,4] Received From
[19,16] WISETECH
[19,28] cw1EntityId
[19,32] cw1202304288
[20,4] Reason
[24,42] Created By
";

		string CreateContentRevoke() => $@"[2,4] Operational Port
[2,16] Terminal
[2,27] 
Notification Import
[2,28] Certified PickUp
Release Right Revoke
[3,4] BRSAO - Sao Paulo
[4,4] Carrier ID
[4,16] BOL Number(s)
[5,4] BTW
[5,16] 21598757411
[7,4] Additional Parties
[8,4] Sending Party
[8,16] Forwarder
[8,28] Transport Company
[9,4] EDI CUSTOMS BROKERS
[9,16] RECEIVINGFORWARDER
[9,28] ARRIVALTRANSPORTORG
[10,4] 10 HUTCHESON STREET
[10,16] AV PAULISTA 291
[10,28] CN SHANGHAI
[11,4] ALBION  QLD
[11,16] CONSOLACAO
[11,28] PARK
[12,16] SAO PAULO
[12,24] SP
[12,28] SHANGHAI
[13,4] AUSTRALIA
[13,12] 4010
[13,16] CHINA
[13,24] 11157802
[13,28] CHINA
[13,36] 22222
[14,4] Sending Party ID
[14,16] Forwarder ID
[14,28] Transport Company ID
[15,4] BTW
[15,16] BTW
[15,28] BTW
[17,4] Equipment Details
[17,16] Status:
[17,22] Certified Pickup - Transfer Message Rejected, CONT1111111
[18,4] Container
[18,16] Release Identification
[18,28] Revoke From
[19,4] CONT1111111
[19,16] CONT1DORelease
[19,28] ☐ Forwarder
[19,40] ☐ Transport Company
[20,4] Received From
[21,4] Reason
[22,4] Equipment Details
[22,16] Status:
[23,4] Container
[23,16] Release Identification
[23,28] Revoke From
[24,4] CONT2222222
[24,16] CONT2DORelease
[24,28] ☐ Forwarder
[24,40] ☐ Transport Company
[25,4] Received From
[25,16] WISETECH
[25,28] cw1EntityId
[25,32] cw1202304288
[26,4] Reason
[29,42] Created By
";

		string CreateContentTransfer() => $@"[2,4] Operational Port
[2,16] Terminal
[2,27] 
Notification Import
[2,28] Certified PickUp
Release Right Transfer
[3,4] BRSAO - Sao Paulo
[4,4] Carrier ID
[4,16] BOL Number(s)
[5,4] BTW
[5,16] 21598757411
[7,4] Additional Parties
[8,4] Sending Party
[8,16] Forwarder
[8,28] Transport Company
[9,4] EDI CUSTOMS BROKERS
[9,16] RECEIVINGFORWARDER
[9,28] ARRIVALTRANSPORTORG
[10,4] 10 HUTCHESON STREET
[10,16] AV PAULISTA 291
[10,28] CN SHANGHAI
[11,4] ALBION  QLD
[11,16] CONSOLACAO
[11,28] PARK
[12,16] SAO PAULO
[12,24] SP
[12,28] SHANGHAI
[13,4] AUSTRALIA
[13,12] 4010
[13,16] CHINA
[13,24] 11157802
[13,28] CHINA
[13,36] 22222
[14,4] Sending Party ID
[14,16] Forwarder ID
[14,28] Transport Company ID
[15,4] BTW
[15,16] BTW
[15,28] BTW
[17,4] Equipment Details
[17,16] Status:
[17,22] Certified Pickup - Transfer Message Rejected, CONT1111111
[18,4] Container
[18,16] Release Identification
[18,28] Transfer To
[19,4] CONT1111111
[19,16] CONT1DORelease
[19,28] ☐ Forwarder
[19,40] ☐ Transport Company
[20,4] Received From
[21,4] Reason
[22,4] Equipment Details
[22,16] Status:
[23,4] Container
[23,16] Release Identification
[23,28] Transfer To
[24,4] CONT2222222
[24,16] CONT2DORelease
[24,28] ☐ Forwarder
[24,40] ☐ Transport Company
[25,4] Received From
[25,16] WISETECH
[25,28] cw1EntityId
[25,32] cw1202304288
[26,4] Reason
[29,42] Created By
";

		#region Implementation

		void PopulateConsol(ForwardingConsol consol)
		{
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "215-98757411";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "BRSAO";
			consol.JK_UniqueConsignRef = "C00001004";

			var carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = "Carrier";
			carrier.OH_RL_NKClosestPort = "AUMEL";
			carrier.MainAddress.Address1 = "Unit 000";
			carrier.MainAddress.Address2 = "Hypocrea astronidii";
			carrier.MainAddress.City = "Mel";
			carrier.MainAddress.Postcode = "2019";
			carrier.MainAddress.OA_RN_NKCountryCode = "AU";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var receivingForwarder = Factory.New<OrgHeader>();
			receivingForwarder.OH_FullName = "ReceivingForwarder";
			receivingForwarder.OH_RL_NKClosestPort = "BRSAO";
			receivingForwarder.MainAddress.Address1 = "Av Paulista 291";
			receivingForwarder.MainAddress.Address2 = "Consolacao";
			receivingForwarder.MainAddress.City = "Sao Paulo";
			receivingForwarder.MainAddress.Postcode = "11157802";
			receivingForwarder.MainAddress.OA_RN_NKCountryCode = "CN";
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;

			var sendingForwarder = Factory.New<OrgHeader>();
			sendingForwarder.OH_FullName = "SendingForwarder";
			sendingForwarder.OH_RL_NKClosestPort = "BRSAO";
			sendingForwarder.MainAddress.Address1 = "Av Paulista 291";
			sendingForwarder.MainAddress.Address2 = "Consolacao";
			sendingForwarder.MainAddress.City = "Salvador";
			sendingForwarder.MainAddress.Postcode = "11157802";
			sendingForwarder.MainAddress.OA_RN_NKCountryCode = "BR";
			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;

			var arrivalTransportOrg = Factory.New<OrgHeader>();
			arrivalTransportOrg.OH_FullName = "ArrivalTransportOrg";
			arrivalTransportOrg.OH_RL_NKClosestPort = "CNSHG";
			arrivalTransportOrg.MainAddress.Address1 = "CN Shanghai";
			arrivalTransportOrg.MainAddress.Address2 = "Park";
			arrivalTransportOrg.MainAddress.City = "Shanghai";
			arrivalTransportOrg.MainAddress.Postcode = "22222";
			arrivalTransportOrg.MainAddress.OA_RN_NKCountryCode = "CN";
			consol.JK_OA_ArrivalUnpackCFSTransportAddress = arrivalTransportOrg.MainAddress.PK;

			var transportLeg1 = consol.Transports[0];
			transportLeg1.JW_LegOrder = 1;
			transportLeg1.JW_TransportMode = Core.Constants.TransportModes.Air;
			transportLeg1.JW_RL_NKLoadPort = "AUSYD";
			transportLeg1.JW_RL_NKDiscPort = "NZAKL";

			var transportLeg2 = consol.Transports.AddNew();
			transportLeg2.JW_LegOrder = 2;
			transportLeg2.JW_TransportMode = Core.Constants.TransportModes.Air;
			transportLeg2.JW_RL_NKLoadPort = "NZAKL";
			transportLeg2.JW_RL_NKDiscPort = "CLSCL";

			var transportLeg3 = consol.Transports.AddNew();
			transportLeg3.JW_LegOrder = 3;
			transportLeg3.JW_TransportMode = Core.Constants.TransportModes.Air;
			transportLeg3.JW_RL_NKLoadPort = "CLSCL";
			transportLeg3.JW_RL_NKDiscPort = "BRSAO";
			transportLeg3.JW_ETD = ZDateTime.Today;
		}

		void AddEDIMessageForATHEvent(StmALog log, string containerNumber, string releaseFromPartyId, string releaseFromParty, string releaseFromPartyCode)
		{
			var message = Factory.New<IXmlEDIMessage>();
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
			message.Content = XElement.Parse($@"
<UniversalEvent>
	<Event>
		<DataContext>
			<DocumentaryOverride>
				<DocumentName>Certified Pickup - ReleaseRight</DocumentName>
			</DocumentaryOverride>
		</DataContext>
		<EventTime>2022-06-21T04:14:09</EventTime>
		<EventType>ATH</EventType>
		<EventParameters>
			<Department>Terminal</Department>
			<EquipmentReferenceNumber>{containerNumber}</EquipmentReferenceNumber>
			<Type>Container Release</Type>
			<ReferenceNumber>RELID22062102</ReferenceNumber>
			<Facility>CTO</Facility>
			<Status>Transferred</Status>
			<Location>BEANR</Location>
		</EventParameters>
		<ContextCollection>
			<Context>
				<Type>ReleaseFromParty</Type>
				<Value>{releaseFromParty}</Value>
			</Context>
			<Context>
				<Type>ReleaseFromPartyId</Type>
				<Value>{releaseFromPartyId}</Value>
			</Context>
			<Context>
				<Type>ReleaseFromPartyCode</Type>
				<Value>{releaseFromPartyCode}</Value>
			</Context>
			<Context>
				<Type>CarrierCode</Type>
				<Value>NXT2000004288</Value>
			</Context>
			<Context>
				<Type>ContainerNumber</Type>
				<Value>WTGU2206204</Value>
			</Context>
			<Context>
				<Type>MBOLNumber</Type>
				<Value>BLCPU2022062101</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>");

			var pivot = Factory.New<IGenPivot>();
			pivot.XX_Relation1ID = log.PK;
			pivot.XX_Relation1TableCode = "SL";
			pivot.XX_Relation2ID = message.PK;
			pivot.XX_Relation2TableCode = "EM";
			pivot.XX_RelationType = Core.Constants.GenPivotTypes.XmlEdiMessage;
		}

		#endregion
	}
}
