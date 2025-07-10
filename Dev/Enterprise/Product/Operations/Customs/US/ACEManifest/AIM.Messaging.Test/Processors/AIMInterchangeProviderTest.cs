using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.Messaging.InterchangeProviders.Testing;
using Enterprise.ZArchitecture.Core.Testing;
using static Enterprise.Customs.US.AIM.Messaging.Constants;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.AIM.Messaging.Testing
{
	public class AIMInterchangeProviderTest : InterchangeProviderTestCase
	{
		public override void TestMessagesPopulateNewInterchange()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "Z2";

			var orgAddress = orgHeader.Addresses.AddNew();
			orgAddress.OA_Address1 = "line 2";
			orgAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.AirAMSOriginatorCode, "USAMSOC", Core.Constants.CountryCodes.UnitedStates);

			var header = Factory.New<ACEManifest.Business.AsycudaManifestHeader>();
			header.AMA_OA_DeconsolidateAddress = orgAddress.PK;
			var header1 = Factory.New<ACEManifest.Business.AsycudaManifestHeader>();
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader2.OH_Code = "Test2";
			var orgAddress2 = orgHeader2.Addresses.AddNew();
			orgAddress2.OA_Address1 = "Test Address 2";
			orgAddress2.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.AirAMSOriginatorCode, "TEST123", Core.Constants.CountryCodes.UnitedStates);
			GlbCompany.CurrentCompany.GC_OH_OrgProxy = orgHeader2.PK;
			Factory.Save();

			var msg1Text = @"FRI
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

			var msg2Text = @"FRI
JFKXYZ
999-87654321-M
/US/12345/123-456-7890";

			LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Test);

			var collection = new NonDependentEDIMessageCollection(Factory);
			var outgoingMessage1 = CreateQueuedAIMEDIMessage("TEST0001", msg1Text);
			outgoingMessage1.EM_LinkedObject = header;
			collection.Add(outgoingMessage1);
			var outgoingMessage2 = CreateQueuedAIMEDIMessage("TEST0002", msg2Text);
			outgoingMessage2.EM_LinkedObject = header;
			collection.Add(outgoingMessage2);
			var outgoingMessage3 = CreateQueuedAIMEDIMessage("TEST0005", msg2Text);
			outgoingMessage3.EM_LinkedObject = header1;
			collection.Add(outgoingMessage3);

			var interchanges = new AIMInterchangeProvider(collection).Interchanges;
			AssertEquals(3, interchanges.Length);
			var outgoingInterchange1 = (AIMEDIInterchange)interchanges[0];

			AssertEquals("EI_ApplicationCode", EDIInterchange.ApplicationCodes.USAMA, outgoingInterchange1.EI_ApplicationCode);
			AssertEquals("EI_InterchangeType", EDIMessageTypeList.Codes.FHL, outgoingInterchange1.EI_InterchangeType);
			AssertEquals("EI_Status", EDIInterchange.Status.eHubQueued, outgoingInterchange1.EI_Status);
			AssertEquals("EI_TransportType", EDIInterchange.TransportType.eHub, outgoingInterchange1.EI_TransportType);
			AssertEquals("EI_From", "USAMSOC", outgoingInterchange1.EI_From);
			AssertEquals("EI_To", "USC", outgoingInterchange1.EI_To);
			AssertEquals("EI_HeaderText", "WASUCCR\x0D\x0A.USAMSOC", outgoingInterchange1.EI_HeaderText);
			AssertEquals("EI_BodyText", outgoingMessage1.EM_MessageText, outgoingInterchange1.EI_BodyText);
			AssertEquals("EI_FooterText", ZString.Empty, outgoingInterchange1.EI_FooterText);

			AssertEquals("EM_Status", EDIMessage.Status.Sent, outgoingMessage1.EM_Status);
			AssertEquals(outgoingInterchange1.PK, outgoingMessage1.Interchange.PK);

			var outgoingInterchange2 = (AIMEDIInterchange)interchanges[1];
			AssertEquals("EI_HeaderText", "WASUCCR\x0D\x0A.USAMSOC", outgoingInterchange2.EI_HeaderText);
			AssertEquals("EI_BodyText", outgoingMessage2.EM_MessageText, outgoingInterchange2.EI_BodyText);
			AssertEquals("EI_FooterText", ZString.Empty, outgoingInterchange2.EI_FooterText);

			AssertEquals("EM_Status", EDIMessage.Status.Sent, outgoingMessage2.EM_Status);
			AssertEquals(outgoingInterchange2.PK, outgoingMessage2.Interchange.PK);

			var outgoingInterchange3 = (AIMEDIInterchange)interchanges[2];
			AssertEquals("EI_HeaderText", "WASUCCR\x0D\x0A.TEST123", outgoingInterchange3.EI_HeaderText);
			AssertEquals("EI_BodyText", outgoingMessage3.EM_MessageText, outgoingInterchange3.EI_BodyText);
			AssertEquals("EI_FooterText", ZString.Empty, outgoingInterchange3.EI_FooterText);

			AssertEquals("EM_Status", EDIMessage.Status.Sent, outgoingMessage3.EM_Status);
			AssertEquals(outgoingInterchange3.PK, outgoingMessage3.Interchange.PK);

			var outgoingMessage4 = CreateQueuedAIMEDIMessage("TEST0003", msg1Text);
			outgoingMessage4.EM_LinkedObject = header;
			AssertHeaderTextInProduction(outgoingMessage4);

			AssertOriginatorCodeNotSet(CreateQueuedAIMEDIMessage("TEST0004", msg1Text));
		}

		public void TestMessagesPopulateOnBillLevel()
		{
			AssertAMOCodeFromCurrentBranch(true);
		}

		public void TestMessagesPopulateNewInterchangeWhenAMOCodeFromCurrentBranch()
		{
			AssertAMOCodeFromCurrentBranch(false);
		}

		void AssertAMOCodeFromCurrentBranch(bool isBillLevel)
		{
			var header = Factory.New<ACEManifest.Business.AsycudaManifestHeader>();

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "Test1";
			var orgAddress1 = orgHeader.Addresses.AddNew();
			orgAddress1.OA_Address1 = "Test Address 1";
			orgAddress1.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.AirAMSOriginatorCode, "TEST123", Core.Constants.CountryCodes.UnitedStates);
			var orgAddress2 = orgHeader.Addresses.AddNew();
			orgAddress2.OA_Address1 = "Test Address 2";
			orgAddress2.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.AirAMSOriginatorCode, "TEST456", Core.Constants.CountryCodes.UnitedStates);
			orgHeader.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.AirAMSOriginatorCode, "TEST002", Core.Constants.CountryCodes.UnitedStates);
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = orgHeader.PK;
			var bill = header.Bills.AddNew();
			Factory.Save();

			var msgText = @"FRI
JFKXYZ
999-87654321-M
/US/12345/123-456-7890";

			LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Test);

			var collection = new NonDependentEDIMessageCollection(Factory);
			var outgoingMessage = CreateQueuedAIMEDIMessage("TEST0001", msgText);
			outgoingMessage.EM_LinkedObject = isBillLevel ? bill : header;
			collection.Add(outgoingMessage);

			var interchanges = new AIMInterchangeProvider(collection).Interchanges;
			AssertEquals(1, interchanges.Length);

			var outgoingInterchange = (AIMEDIInterchange)interchanges[0];
			AssertEquals("EI_HeaderText", "WASUCCR\x0D\x0A.TEST002", outgoingInterchange.EI_HeaderText);
			AssertEquals("EI_BodyText", outgoingMessage.EM_MessageText, outgoingInterchange.EI_BodyText);
			AssertEquals("EI_FooterText", ZString.Empty, outgoingInterchange.EI_FooterText);

			AssertEquals("EM_Status", EDIMessage.Status.Sent, outgoingMessage.EM_Status);
			AssertEquals(outgoingInterchange.PK, outgoingMessage.Interchange.PK);
		}

		void AssertOriginatorCodeNotSet(AIMEDIMessage outgoingMessage)
		{
			var collection = new NonDependentEDIMessageCollection(Factory);
			collection.Add(outgoingMessage);

			var interchanges = new AIMInterchangeProvider(collection).Interchanges;

			AssertEquals("EM_Status OriginatorCodeNotSet", EDIMessage.Status.Discarded, outgoingMessage.EM_Status);
			AssertEquals(0, interchanges.Length);
			var note = outgoingMessage.Notes.FindByDescription(AIMInterchangeProvider.ProcessingLogDescription)[0];
			AssertContains("There is no Customs Originator Code set up", note.ST_NoteText);
			AssertContains($"Originator code can be added under CFS Address > Details > Config > Registration Numbers / Codes, (using type 'AMO') or added under Current Branch ({GlbBranch.CurrentBranch.GB_BranchName}) or Current Company ({GlbCompany.CurrentCompany.CompanyName}) Organization Proxy > Details > Config > Registration Numbers / Codes, (using type 'AMO')", note.ST_NoteText);
		}

		void AssertHeaderTextInProduction(AIMEDIMessage outgoingMessage)
		{
			var collection = new NonDependentEDIMessageCollection(Factory);
			collection.Add(outgoingMessage);

			try
			{
				LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Production);

				var outgoingInterchange = new AIMInterchangeProvider(collection).Interchanges[0];
				AssertEquals("Production HeaderText", "WASUSCR\x0D\x0A.USAMSOC", outgoingInterchange.EI_HeaderText);
			}
			finally
			{
				LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Test);
			}
		}

		AIMEDIMessage CreateQueuedAIMEDIMessage(ZString messageNum, ZString messageText)
		{
			var message = Factory.New<AIMEDIMessage>();
			message.EM_MessageType = EDIMessageTypeList.Codes.FHL;
			message.EM_MessageSubType = AIMMessageSubTypes.FSN;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageNum = messageNum;
			message.EM_MessageText = messageText;
			return message;
		}

		protected override InterchangeProviderBase GetInterchangeProvider(NonDependentEDIMessageCollection collection)
		{
			return new AIMInterchangeProvider(collection);
		}
	}
}
