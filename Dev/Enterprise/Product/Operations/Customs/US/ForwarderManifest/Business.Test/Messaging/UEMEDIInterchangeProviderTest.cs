using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.Messaging.InterchangeProviders.Testing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.ForwarderManifest.Business.Test
{
	public class UEMEDIInterchangeProviderTest : InterchangeProviderTestCase
	{
		class UEMEDIInterchangeProviderFailureStatus : UEMEDIInterchangeProvider
		{
			public UEMEDIInterchangeProviderFailureStatus(NonDependentEDIMessageCollection messages) : base(messages)
			{
			}

			protected override ZString ProcessedMessageStatusCode(EDIInterchange interchange)
			{
				return EDIMessage.Status.Failed;
			}
		}

		public void TestWhenSendMessageFailed()
		{
			AssertEquals("Count of EDIInterchange before running function.", 0, Factory.GetDatabaseCount(typeof(EDIInterchange)));

			var header = Factory.New<USExportAsycudaManifestHeader>();
			header.AMA_TransportMode = TransportTypeList.Codes.Air;

			var message = Factory.New<UEMEDIMessage>();
			message.EM_GB = Env.CurrentBranchPK;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_MessageText = "TEST MESSAGE";
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_LinkedObject = header.Bills.AddNew();

			var orgCusCode = message.Branch.Company.OrgProxy.CustomsCodes.AddNew();
			orgCusCode.OK_CustomsRegNo = "CW1";
			orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
			orgCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;

			var messages = new NonDependentEDIMessageCollection(Factory);
			messages.Add(message);
			var interchangeProvider = new UEMEDIInterchangeProviderFailureStatus(messages);
			interchangeProvider.PackCollatedMessagesIntoInterchanges();
			Factory.Save();

			AssertEquals("Count of EDIInterchange after running function.", 1, Factory.GetDatabaseCount(typeof(EDIInterchange)));
			AssertEquals("Manifest's message status is empty.", ZString.Empty, header.AMA_MessageStatus);
		}

		public override void TestMessagesPopulateNewInterchange()
		{
			AssertEquals("Count of EDIInterchange before running function.", 0, Factory.GetDatabaseCount(typeof(EDIInterchange)));

			var header = Factory.New<USExportAsycudaManifestHeader>();
			header.AMA_TransportMode = TransportTypeList.Codes.Air;

			var message = Factory.New<UEMEDIMessage>();
			message.EM_GB = Env.CurrentBranchPK;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_MessageText = "TEST MESSAGE";
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_LinkedObject = header.Bills.AddNew();
			message.EM_MessageNum = "EDIEDIDAT_1";

			var messages = new NonDependentEDIMessageCollection(Factory);
			messages.Add(message);
			var interchangeProvider = GetInterchangeProvider(messages);
			interchangeProvider.PackCollatedMessagesIntoInterchanges();
			Factory.Save();

			AssertEquals("Count of EDIInterchange after running function.", 0, Factory.GetDatabaseCount(typeof(EDIInterchange)));
			AssertEquals("message status is DCD.", EDIMessage.Status.Discarded, message.EM_Status);
			AssertEquals("Manifest's message status is ERR.", MessageStatusCodeList.Codes.Error, header.AMA_MessageStatus);
			AssertContains(@"Message EDIEDIDAT_1 will be discarded for the following reason:
There is no Customs Interchange Sender ID set up.
Please add a carrier code in company 'EDI' organization proxy > Config > Registration Numbers / Codes, (using type 'CCP').", message.Notes.FindByDescription("Processing Log").Single().ST_NoteText);

			message.Notes.RemoveAndDeleteAll();
			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			header.AMA_MessageStatus = ZString.Empty;
			message.EM_LinkedObject = null;
			AssertEquals("Reset message notes.", 0, message.Notes.ToList<StmNote>().Count);
			AssertEquals("Reset message status.", EDIMessageStatusList.Codes.Queued, message.EM_Status);
			AssertEquals("Reset message linked object.", null, message.EM_LinkedObject);
			AssertEquals("Reset header status.", ZString.Empty, header.AMA_MessageStatus);

			interchangeProvider = GetInterchangeProvider(messages);
			interchangeProvider.PackCollatedMessagesIntoInterchanges();
			Factory.Save();

			AssertEquals("Count of EDIInterchange after running function.", 0, Factory.GetDatabaseCount(typeof(EDIInterchange)));
			AssertEquals("message status is DCD.", EDIMessage.Status.Discarded, message.EM_Status);
			AssertEquals("Manifest's message status is not changed.", ZString.Empty, header.AMA_MessageStatus);
			AssertContains(@"Message EDIEDIDAT_1 will be discarded for the following reason:
There is no header being related to this message.", message.Notes.FindByDescription("Processing Log").Single().ST_NoteText);

			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			header.AMA_MessageStatus = ZString.Empty;
			message.EM_LinkedObject = header.Bills[0];
			AssertEquals("Reset message status.", EDIMessageStatusList.Codes.Queued, message.EM_Status);
			AssertEquals("Reset header status.", ZString.Empty, header.AMA_MessageStatus);

			var orgCusCode = message.Branch.Company.OrgProxy.CustomsCodes.AddNew();
			orgCusCode.OK_CustomsRegNo = "CW1";
			orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
			orgCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;

			messages = new NonDependentEDIMessageCollection(Factory);
			messages.Add(message);
			interchangeProvider = GetInterchangeProvider(messages);
			interchangeProvider.PackCollatedMessagesIntoInterchanges();
			Factory.Save();

			AssertEquals("Count of EDIInterchange after running function.", 1, Factory.GetDatabaseCount(typeof(EDIInterchange)));

			var interchange = Factory.Load<EDIInterchange>(message.EM_EI);
			CombineAssertions("Created UEMEDIInterchange properties.", () =>
			{
				AssertEquals("Interchange Type", typeof(UEMEDIInterchange), interchange.GetType());
				AssertEquals("EI_ApplicationCode", ApplicationCodeList.Codes.USExportManifest, interchange.EI_ApplicationCode);
				AssertEquals("EI_InterchangeType", EDIMessageTypeList.Codes.XDC, interchange.EI_InterchangeType);
				AssertEquals("EI_ReceiveTransmit", EDIInterchange.Direction.Transmit, interchange.EI_ReceiveTransmit);
				AssertEquals("EI_Status", EDIInterchange.Status.eHubQueued, interchange.EI_Status);
				AssertEquals("EI_GB", Env.CurrentBranchPK, interchange.EI_GB);
				AssertEquals("EI_From", "CW1", interchange.EI_From);
				AssertEquals("EI_To", "USC", interchange.EI_To);
				AssertEquals("EI_BodyText", message.EM_MessageText, interchange.EI_BodyText);
				AssertEquals("EI_FooterText", string.Empty, interchange.EI_FooterText);
				AssertEquals("Number of ContainedMessages", 1, interchange.ContainedMessages.Count);

				var containedMessage = interchange.ContainedMessages[0];
				AssertEquals("Expected EDIMessage", message.PK, containedMessage.PK);
				AssertEquals("EM_Status", EDIMessage.Status.Sent, containedMessage.EM_Status);
			});

			AssertEquals("Manifest's message status is SNT.", MessageStatusCodeList.Codes.Sent, header.AMA_MessageStatus);
		}

		public void TestGetCollationKey()
		{
			AssertEquals("Count of EDIInterchange before running function.", 0, Factory.GetDatabaseCount(typeof(EDIInterchange)));

			var header = Factory.New<USExportAsycudaManifestHeader>();
			header.AMA_TransportMode = TransportTypeList.Codes.Air;
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();

			var message1 = Factory.New<UEMEDIMessage>();
			message1.EM_GB = Env.CurrentBranchPK;
			message1.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message1.EM_MessageText = "TEST MESSAGE";
			message1.EM_Status = EDIMessage.Status.Queued;
			message1.EM_MessageOwner = "JOEY";
			message1.EM_LinkedObject = bill1;

			var message2 = Factory.New<UEMEDIMessage>();
			message2.EM_GB = Env.CurrentBranchPK;
			message2.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message2.EM_MessageText = "TEST MESSAGE";
			message2.EM_Status = EDIMessage.Status.Queued;
			message2.EM_MessageOwner = "JOEY";
			message2.EM_LinkedObject = bill2;

			var orgCusCode = message1.Branch.Company.OrgProxy.CustomsCodes.AddNew();
			orgCusCode.OK_CustomsRegNo = "A123";
			orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
			orgCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;

			var messages = new NonDependentEDIMessageCollection(Factory);
			messages.Add(message1);
			messages.Add(message2);
			var interchangeProvider = GetInterchangeProvider(messages);
			interchangeProvider.PackCollatedMessagesIntoInterchanges();
			Factory.Save();

			AssertEquals("Count of EDIInterchange after running function.", 2, Factory.GetDatabaseCount(typeof(EDIInterchange)));
		}

		protected override InterchangeProviderBase GetInterchangeProvider(NonDependentEDIMessageCollection collection)
		{
			return new UEMEDIInterchangeProvider(collection);
		}
	}
}
