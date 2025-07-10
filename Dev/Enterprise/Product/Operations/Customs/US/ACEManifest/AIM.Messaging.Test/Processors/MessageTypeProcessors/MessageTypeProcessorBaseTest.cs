using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business.Customs;
using Enterprise.Registry.Business.Customs.Manifest;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Customs.US.AIM.Messaging.Constants;

namespace Enterprise.Customs.US.AIM.Messaging.Testing
{
	public abstract class MessageTypeProcessorBaseTest : TestCaseWithFactory
	{
		protected abstract IAIMMessageTypeProcessor GetNewMessageTypeProcessor();
		protected abstract AIMInboundMessage GetNewInboundMessage();
		protected abstract ZString GetResponseMessageForGroupNotificationTesting();
		protected abstract ZString ExpectedEmailMessageType { get; }

		#region Base Class Testcase

		public void TestProcessMessage_GroupNotification_NonHVLV()
		{
			using (ManifestCustomsDataRegistry.Instance.USAirAMSGroupNotification.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new ManifestGroupNotification(Core.Constants.EmailTo.StaffMemberAndNominatedGroup, nonHVLVGroup.PK, false)))
			{
				ManifestHeader.AMA_GB = GlbBranch.CurrentBranch.PK;
				var responseMessage = GetResponseMessageForGroupNotificationTesting();

				ProcessMessage(responseMessage, ManifestHeader);

				var email = Env.OutgoingMailManager.EmailsCreated[0];
				AssertContainsExactElementsInAnyOrder("Email recipients with notification group", new[] { newStaff.GS_EmailAddress, nonHVLVStaff.GS_EmailAddress }, email.Recipients.ToStringCollection());
			}
		}

		public void TestProcessMessage_GroupNotification_HVLV()
		{
			using (ManifestCustomsDataRegistry.Instance.USHVLVAirAMSGroupNotification.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new ManifestGroupNotification(Core.Constants.EmailTo.StaffMemberAndNominatedGroup, hvlvGroup.PK, false)))
			{
				ManifestHeader.AMA_GB = GlbBranch.CurrentBranch.PK;
				ManifestHeader.Logs.AddNew(Events.Transferred, "|TYP=HVL");
				var responseMessage = GetResponseMessageForGroupNotificationTesting();

				ProcessMessage(responseMessage, ManifestHeader);

				var email = Env.OutgoingMailManager.EmailsCreated[0];
				AssertContainsExactElementsInAnyOrder("Email recipients with HVLV notification group", new[] { newStaff.GS_EmailAddress, hvlvStaff.GS_EmailAddress }, email.Recipients.ToStringCollection());
			}
		}

		#endregion

		#region Implementation

		protected void ProcessMessage(ZString responseMessageText, BusinessObject outgoingMessageParent)
		{
			var outgoingMessage = CreateSentMessage(outgoingMessageParent);
			var inboundMessage = CreateQueuedMessage(responseMessageText);
			Factory.Save();

			ProcessMessage(inboundMessage, outgoingMessage, outgoingMessageParent);
		}

		protected void ProcessMessage(AIMInboundMessage inboundMessage, EDIMessage outgoingMessage, BusinessObject expectedParent)
		{
			var processor = GetNewMessageTypeProcessor();
			processor.Process(inboundMessage);
			Factory.Save();

			inboundMessage.Reload();
			AssertEquals("inboundMessage.EM_LinkTable", expectedParent.TableName, inboundMessage.EM_LinkTable);
			AssertEquals("inboundMessage.EM_LinkUniqueID", expectedParent.PK, inboundMessage.EM_LinkUniqueID);
			AssertEquals("inboundMessage.EM_GB == outgoingMessage.EM_GB", outgoingMessage?.EM_GB ?? GlbBranch.CurrentBranch.PK, inboundMessage.EM_GB);

			var expectedReference = ZString.Empty;
			if (expectedParent is AsycudaBill bill)
			{
				expectedReference = bill.ABL_BillNumber;
			}
			else if (expectedParent is AsycudaManifestHeader manifest)
			{
				expectedReference = manifest.AMA_MasterBill;
			}
			else if (expectedParent is AsycudaArrivalHeader arrivalHeader)
			{
				// Assuming the arrival message is targeting the first bill.
				expectedReference = arrivalHeader.ManifestHeader.Bills[0].ABL_BillNumber;
			}

			AssertEquals("inboundMessage.EM_ApplicationReference", expectedReference, inboundMessage.EM_ApplicationReference);
		}

		protected void AssertEmailCreated(string expectedEmailSubjectLine, string expectedPropertyTableHTML, string expectedExtraHTML = "")
		{
			var expectedEmailBody = $@"<strong>
Job Number : {ManifestHeader.AMA_MasterBill}
<br />
</strong>A {ExpectedEmailMessageType} message has been received from CBP.<br />
<br />
{expectedPropertyTableHTML}
<br />
{expectedExtraHTML}
<br />
<!--DynamicHtml3-->
<hr />
<br />
<strong><large>
<!--EndSection Details--></strong></large>
Regards,<br />
<br />
CargoWise One Administrative Messages Sender<br />
<br />";

			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals(expectedEmailSubjectLine, email.Subject);
			AssertContains("Email Recipients", newStaff.GS_EmailAddress, email.Recipients.RecipientsAsDelimitedString(), ignoreCase: true);
			AssertContains("Email body", expectedEmailBody, email.Body);
		}

		protected AIMInboundMessage CreateQueuedMessage(ZString messageText)
		{
			var message = GetNewInboundMessage();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageText = messageText;
			message.EM_MessageNum = "TEST998877";
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			return message;
		}

		protected EDIMessage CreateSentMessage(BusinessObject linkedObject, string messageText = OutgoingMessageText, string messageSubType = AIMMessageSubTypes.FRI)
		{
			var outgoingMessage = Factory.New<AIMEDIMessage>();
			outgoingMessage.EM_MessageType = EDIMessageTypeList.Codes.FHL;
			outgoingMessage.EM_MessageSubType = messageSubType;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_Status = EDIMessage.Status.Sent;
			outgoingMessage.EM_MessageText = messageText;
			outgoingMessage.EM_MessageNum = "TEST11223344";
			outgoingMessage.EM_LinkedObject = linkedObject;
			outgoingMessage.EM_ApplicationReference = "123456";
			outgoingMessage.EM_SystemCreateUser = newStaff.GS_Code;
			return outgoingMessage;
		}

		const string OutgoingMessageText = @"FRI
JFKXYZ
999-12345675-M
WBL/FRA/T1/K10/TOYS
ARR/XYZ123/25OCT
SHP/TOTLERTOYS
/12 VIRGINIA COURT
/FRANKFURT
/DE
CNE/TOYSRWE
/8812 FUN STREET
/NEWYORK/NY
/US/12345/123-456-7890";

		protected AsycudaManifestHeader ManifestHeader
		{
			get
			{
				if (fManifestHeader == null)
				{
					fManifestHeader = Factory.NewWithValidTestData<ACEManifest.Business.AsycudaManifestHeader>();
					fManifestHeader.AMA_RN_NKCountry = "US";
					fManifestHeader.AMA_TransportMode = "AIR";
					fManifestHeader.AMA_MasterBill = "08111223344";
					fManifestHeader.AMA_SystemCreateTimeUtc = ZDateTime.UtcNow;

					PopulateManifestHeader(fManifestHeader);
				}
				return fManifestHeader;
			}
		}
		AsycudaManifestHeader fManifestHeader;

		protected virtual void PopulateManifestHeader(AsycudaManifestHeader manifestHeader)
		{
		}

		protected AsycudaBill HouseBill
		{
			get
			{
				if (fHouseBill == null)
				{
					fHouseBill = ManifestHeader.Bills.AddNew();
					fHouseBill.ABL_BillNumber = "HAWB123";
					PopulateHouseBill(fHouseBill);
				}
				return fHouseBill;
			}
		}
		AsycudaBill fHouseBill;

		protected virtual void PopulateHouseBill(AsycudaBill houseBill)
		{
		}

		protected AsycudaArrivalHeader ArrivalHeader
		{
			get
			{
				if (fArrivalHeader == null)
				{
					fArrivalHeader = ManifestHeader.ArrivalHeaders.AddNew();
					PopulateArrivalHeader(fArrivalHeader);
				}
				return fArrivalHeader;
			}
		}
		AsycudaArrivalHeader fArrivalHeader;

		protected virtual void PopulateArrivalHeader(AsycudaArrivalHeader arrivalHeader)
		{
		}

		protected AsycudaArrivalLine ArrivalLine
		{
			get
			{
				if (fArrivalLine == null)
				{
					fArrivalLine = ArrivalHeader.ArrivalDetails.AddNew();
					fArrivalLine.ATL_ABL_AsycudaBill = HouseBill.PK;
				}
				return fArrivalLine;
			}
		}
		AsycudaArrivalLine fArrivalLine;

		GlbStaff newStaff;

		GlbGroup hvlvGroup;
		GlbStaff hvlvStaff;

		GlbGroup nonHVLVGroup;
		GlbStaff nonHVLVStaff;

		protected override void SetUp()
		{
			base.SetUp();

			var newGroup = Factory.New<GlbGroup>();
			newGroup.GG_Code = "NG1";
			newStaff = newGroup.Staff.AddNew();
			newStaff.GS_Code = "NS1";
			newStaff.GS_LoginName = "NS1";
			newStaff.GS_EmailAddress = "ns1@cargowise.com";

			hvlvGroup = Factory.New<GlbGroup>();
			hvlvGroup.GG_Code = "NGH";
			hvlvStaff = hvlvGroup.Staff.AddNew();
			hvlvStaff.GS_Code = "NSH";
			hvlvStaff.GS_LoginName = "NSH";
			hvlvStaff.GS_EmailAddress = "nsh@cargowise.com";

			nonHVLVGroup = Factory.New<GlbGroup>();
			nonHVLVGroup.GG_Code = "NGN";
			nonHVLVStaff = nonHVLVGroup.Staff.AddNew();
			nonHVLVStaff.GS_Code = "NSN";
			nonHVLVStaff.GS_LoginName = "NSN";
			nonHVLVStaff.GS_EmailAddress = "nsn@cargowise.com";

			Factory.Save();
		}

		#endregion
	}
}
