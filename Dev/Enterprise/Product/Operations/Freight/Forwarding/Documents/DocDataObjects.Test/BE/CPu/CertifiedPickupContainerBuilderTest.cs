using System.Xml.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.BE;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Integration;

namespace Enterprise.Freight.Forwarding.Documents.Testing.BE
{
	sealed class CertifiedPickupContainerBuilderTest : TestCaseWithFactory
	{
		public void TestHumanReadableStatus_Transfer_DeclinedByNextParty()
		{
			var container = CreateContainer();
			CertifiedPickupContainerEventTestHelper.AddLogForContainer(container, CertifiedPickupConstants.Status.DeclinedByNextPartyForTransferRevoke);

			var builder = new CertifiedPickupContainerBuilder();
			var data = builder.Build(container, CertifiedPickup.FormModeTransfer);

			AssertEquals("Certified Pickup - Transfer Message Rejected, CONT1111111", data.HumanReadableStatus);
		}

		public void TestHumanReadableStatus_Transfer_DeclinedByOtherReason()
		{
			var container = CreateContainer();
			CertifiedPickupContainerEventTestHelper.AddLogForContainer(container, CertifiedPickupConstants.Status.DeclinedByOtherReason);

			var builder = new CertifiedPickupContainerBuilder();
			var data = builder.Build(container, CertifiedPickup.FormModeTransfer);

			AssertEquals("Certified Pickup - Transfer Message Rejected, CONT1111111", data.HumanReadableStatus);
		}

		public void TestHumanReadableStatus_Revoke()
		{
			var container = CreateContainer();
			CertifiedPickupContainerEventTestHelper.AddLogForContainer(container, CertifiedPickupConstants.Status.TransferSent);

			var builder = new CertifiedPickupContainerBuilder();
			var data = builder.Build(container, CertifiedPickup.FormModeRevoke);

			AssertEquals("Certified Pickup - Transfer Message Pending Processing, CONT1111111", data.HumanReadableStatus);
		}

		public void TestReleaseFromFieldsIsFromATHEventRelatedEDIMessage()
		{
			var container = CreateContainer();
			var log = CertifiedPickupContainerEventTestHelper.AddLogForContainer(container, CertifiedPickupConstants.Status.Assigned);
			AddEDIMessageForATHEvent(log, container.JC_ContainerNum);

			var builder = new CertifiedPickupContainerBuilder();
			var data = builder.Build(container, CertifiedPickup.FormModeAcceptDecline);

			AssertEquals("ReceivedFromId", "nxtEntityId", data.ReleaseFromId);
			AssertEquals("ReceivedFromName", "INTRIS", data.ReleaseFromName);
			AssertEquals("ReceivedFromCode", "NXT2000004288", data.ReleaseFromCode);
		}

		public void TestCurrentStatus()
		{
			CombineAssertions("This test should ensure the value of containers' CurrentStatus are correct and all test cases are creating correct test data.", () =>
			{
				AssertCurrentStatus(CertifiedPickupConstants.Status.Assigned, CertifiedPickup.FormModeAcceptDecline);

				AssertCurrentStatus(CertifiedPickupConstants.Status.TransferSentAwaitingResponse, CertifiedPickup.FormModeTransfer);
				AssertCurrentStatus(CertifiedPickupConstants.Status.Accepted, CertifiedPickup.FormModeTransfer);
				AssertCurrentStatus(CertifiedPickupConstants.Status.Revoked, CertifiedPickup.FormModeTransfer);
				AssertCurrentStatus(CertifiedPickupConstants.Status.DeclinedByNextPartyForTransferRevoke, CertifiedPickup.FormModeTransfer);
				AssertCurrentStatus(CertifiedPickupConstants.Status.DeclinedByOtherReason, CertifiedPickup.FormModeTransfer);

				AssertCurrentStatus(CertifiedPickupConstants.Status.TransferSent, CertifiedPickup.FormModeRevoke);

				AssertCurrentStatus(CertifiedPickupConstants.Status.DeclinedByNextPartyForAcceptDecline, CertifiedPickup.FormModeNotApplicable);
			});

			void AssertCurrentStatus(ZString cpuStatus, ZString formMode)
			{
				var container = CreateContainer();
				CertifiedPickupContainerEventTestHelper.AddLogForContainer(container, cpuStatus);

				var builder = new CertifiedPickupContainerBuilder();
				var data = builder.Build(container, formMode);

				AssertEquals(formMode, data.FormMode);
				AssertEquals(cpuStatus, data.CurrentStatus);
			}
		}

		#region Implementation

		ForwardingContainer CreateContainer()
		{
			var container = Factory.New<ForwardingContainer>();
			container.JC_ContainerNum = "CONT1111111";
			container.JC_IsNonOperativeReefer = false;

			return container;
		}

		void AddEDIMessageForATHEvent(StmALog log, string containerNumber)
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
				<Value>INTRIS</Value>
			</Context>
			<Context>
				<Type>ReleaseFromPartyId</Type>
				<Value>nxtEntityId</Value>
			</Context>
			<Context>
				<Type>ReleaseFromPartyCode</Type>
				<Value>NXT2000004288</Value>
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
			pivot.XX_RelationType = Constants.GenPivotTypes.XmlEdiMessage;
		}

		#endregion
	}
}
