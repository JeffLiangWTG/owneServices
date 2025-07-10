using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	public class JobDeclarationUniversalMessagingHelperTest : TestCaseWithFactory
	{
		[GuiTest]
		public void TestValidateCanSubmit()
		{
			var declaration = Factory.New<Business.Testing.MessageManageableJobDeclaration>();
			var wrapper = new JobDeclarationMessageSendingObjectParent<JobDeclarationMessageSendingObject>(declaration);
			var helper = new JobDeclarationUniversalMessagingHelperForTest(wrapper);

			AssertEquals("Warning text", "A Recipient ID should be entered. It can be entered in the Registry: Registry>Customs>Italy>Recipient ID", helper.ValidateCanSubmit());

			helper.SetUniversalCustomsMessagingRecipientID("ITCUSTOMS");
			declaration.DPSFreightMovementRestricted = true;
			AssertEquals("Warning text", "Unable to submit message due to Denied Party Screening cancellation.", helper.ValidateCanSubmit());

			declaration.DPSFreightMovementRestricted = false;
			AssertEquals("Warning text", ZString.Empty, helper.ValidateCanSubmit());
		}

		public void TestInterchange()
		{
			var staffA = Factory.New<IGlbStaff>();
			staffA.GS_Code = "BOB";
			staffA.GS_LoginName = "BOB";
			staffA.GS_FullName = "Bob the Builder";
			Factory.Save();

			using (Factory.AddDisposableService())
			using (EnvProxy.Instance.SetTemporaryUserContext(new UserContext("BOB", Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_ApplicationCode = "BLT";
				declaration.JE_MessageType = "IMP";
				var entry = declaration.CustomsEntryHeaders.AddNew();
				entry.CH_JE = declaration.PK;

				Factory.Save();

				var wrapper = new JobDeclarationMessageSendingObjectParent<JobDeclarationMessageSendingObject>(declaration);
				var helper = new JobDeclarationUniversalMessagingHelperForTest(wrapper);
				helper.SetUniversalCustomsMessagingRecipientID("ITCUSTOMS");
				AssertEquals("No message sent", 0, helper.SendUniversalMessage());

				AssertEquals(0, Factory.GetDatabaseCount(typeof(EDIInterchange)));

				foreach (JobDeclarationMessageSendingObject sendingObject in wrapper.SendingObjectsCollection)
				{
					sendingObject.ShouldSend = true;
				}
				helper.GetSelectedShouldSendUniversalMessageHeadersFunc = () => new List<BusinessObject>();
				AssertEquals(0, helper.SelectedShouldSendUniversalMessageHeaders.Count());
				AssertEquals(0, helper.SendUniversalMessage());

				helper.GetSelectedShouldSendUniversalMessageHeadersFunc = null;
				AssertEquals(1, helper.SelectedShouldSendUniversalMessageHeaders.Count());
				AssertEquals("1 message sent", 1, helper.SendUniversalMessage());

				var createdMessage = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, entry.PK));
				AssertNotNull(createdMessage);
				AssertTextContainsDispiteBlanks(ExpectedDataSourceCollection.Replace("<<JOBREFERENCENUMBER>>", declaration.JE_DeclarationReference), createdMessage.EM_MessageText);
				AssertTextContainsDispiteBlanks(ExpectedEventReference.Replace("<<JOBREFERENCENUMBER>>", declaration.JE_DeclarationReference), createdMessage.EM_MessageText);
				AssertTextContainsDispiteBlanks(ExpectedEventUser, createdMessage.EM_MessageText);
				AssertTextContainsDispiteBlanks(ExpectedEventType, createdMessage.EM_MessageText);
				AssertEquals("Message Interpretation should be created", 1, createdMessage.Notes.FindByDescription(PredefinedNoteTypes.Instance.MessageInterpretation.Description).Length);

				var interchange = createdMessage.Interchange;
				CombineAssertions(() =>
				{
					AssertTextContainsDispiteBlanks(interchangeHeader, interchange.EI_BodyText);
					AssertTextContainsDispiteBlanks(ExpectedDataSourceCollection.Replace("<<JOBREFERENCENUMBER>>", declaration.JE_DeclarationReference), interchange.EI_BodyText);
					AssertTextContainsDispiteBlanks(ExpectedEventReference.Replace("<<JOBREFERENCENUMBER>>", declaration.JE_DeclarationReference), interchange.EI_BodyText);
					AssertTextContainsDispiteBlanks(ExpectedEventUser, interchange.EI_BodyText);
					AssertTextContainsDispiteBlanks(ExpectedEventType, interchange.EI_BodyText);
					AssertEquals("<EDIDelivery><FileName>007</FileName><EmailSubject></EmailSubject></EDIDelivery>", interchange.EI_HeaderText);
				});

				AssertEquals("CH_Status has been updated", MessageStatusList.Codes.AwaitingOriginal, entry.CH_Status);

				helper.UpdateSendingObjectHeaderStatusAfterCreateEDIEnterchageFunc = (businessObject, status) =>
				{
					if (businessObject is CusEntryHeader entryHeader)
					{
						entryHeader.CH_Status = "666";
					}
				};
				AssertEquals(1, helper.SendUniversalMessage());
				createdMessage = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, entry.PK));
				interchange = createdMessage.Interchange;
				AssertEquals("CH_Status has been updated", "666", entry.CH_Status);
				helper.UpdateSendingObjectHeaderStatusAfterCreateEDIEnterchageFunc = null;
			}
		}

		public void TestRollbackOnSaveFailed()
		{
			using (Factory.AddDisposableService())
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_ApplicationCode = "BLT";
				declaration.JE_MessageType = "IMP";
				var entry = declaration.CustomsEntryHeaders.AddNew();
				entry.CH_JE = declaration.PK;
				Factory.Save();

				var wrapper = new JobDeclarationMessageSendingObjectParent<JobDeclarationMessageSendingObject>(declaration);
				var helper = new JobDeclarationUniversalMessagingHelperForTest(wrapper);
				helper.SetUniversalCustomsMessagingRecipientID("ITCUSTOMS");

				foreach (JobDeclarationMessageSendingObject sendingObject in wrapper.SendingObjectsCollection)
				{
					sendingObject.ShouldSend = true;
				}

				CombineAssertions(() =>
				{
					declaration.JE_OH_Buyer = ZGuid.Invalid;
					AssertEquals("No message sent", 0, helper.SendUniversalMessage());
					AssertEquals("1 message created", 1, helper.CreatedMessages.Count);
					AssertEquals("1 message created but deleted", true, helper.CreatedMessages[0].IsDeleted);

					AssertEquals("Declaration logs added on sending should have been cleaned.", 0, declaration.Logs.LogsNotInDB.Length);
					AssertEquals("EntryHeader logs added on sending should have been cleaned.", 0, entry.Logs.LogsNotInDB.Length);
				});
			}
		}

		protected void AssertTextContainsDispiteBlanks(ZString expected, ZString actual)
		{
			var expectedNoBlanks = Regex.Replace(expected, @"\s", string.Empty);
			var actualNoBlanks = Regex.Replace(actual, @"\s", string.Empty);
			AssertContains(expectedNoBlanks, actualNoBlanks);
		}

		readonly string interchangeHeader = $@"
  <UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Header>
	<SenderID>{GlbCompany.CurrentCompany.LicenceKeyIdentifier}</SenderID>
	<RecipientID>ITCUSTOMS</RecipientID>
  </Header>";

		const string ExpectedDataSourceCollection = @"
	 <DataSourceCollection>
		<DataSource>
		  <Type>CustomsDeclaration</Type>
		  <Key><<JOBREFERENCENUMBER>></Key>
		</DataSource>
	</DataSourceCollection>";

		const string ExpectedEventReference = "<EventReference>MST=IM|RFN=<<JOBREFERENCENUMBER>></EventReference>";

		const string ExpectedEventUser = @"
	  <EventUser>
		<Code>BOB</Code>
		<Name>Bob the Builder</Name>
	  </EventUser>";

		const string ExpectedEventType = @"
	  <EventType>
		<Code>MRS</Code>";

		sealed class JobDeclarationUniversalMessagingHelperForTest : JobDeclarationUniversalMessagingHelper
		{
			public JobDeclarationUniversalMessagingHelperForTest(JobDeclarationMessageSendingObjectParent<JobDeclarationMessageSendingObject> messageSendingObject) : base(messageSendingObject)
			{
			}

			public void SetUniversalCustomsMessagingRecipientID(ZString value) => universalCustomsMessagingRecipientID = value;

			public Func<IEnumerable<BusinessObject>> GetSelectedShouldSendUniversalMessageHeadersFunc;

			public IEnumerable<BusinessObject> SelectedShouldSendUniversalMessageHeaders => GetSelectedMessageSendingObjects();

			public Action<BusinessObject, ZString> UpdateSendingObjectHeaderStatusAfterCreateEDIEnterchageFunc;

			protected override IEnumerable<BusinessObject> GetSelectedMessageSendingObjects() => GetSelectedShouldSendUniversalMessageHeadersFunc == null ? base.GetSelectedMessageSendingObjects() : GetSelectedShouldSendUniversalMessageHeadersFunc.Invoke();

			protected override void UpdateSendingObjectHeaderAfterCreatingEDIEnterchage(BusinessObject businessObject, ZString status)
			{
				if (UpdateSendingObjectHeaderStatusAfterCreateEDIEnterchageFunc == null)
				{
					base.UpdateSendingObjectHeaderAfterCreatingEDIEnterchage(businessObject, status);
				}
				else
				{
					UpdateSendingObjectHeaderStatusAfterCreateEDIEnterchageFunc.Invoke(businessObject, status);
				}
			}

			protected override NonPersistentEDICommunicationMode GetEDICommunicationMode(BusinessObject header, ZString destination)
			{
				var mode = base.GetEDICommunicationMode(header, destination);
				mode.EK_Filename = "007";
				return mode;
			}

			protected override ZString InstructionForRecipientIDSetup => "Registry>Customs>Italy>Recipient ID";

			protected override ZString UniversalCustomsMessagingRecipientID => universalCustomsMessagingRecipientID;

			protected override ZString GetMessageTypeForEventReference(BusinessObject entryHeader) => "IM";

			protected override ZString GetReferenceNumberForEventReferenceCore() => MessageSendingObjectParent.ParentDeclaration.JE_DeclarationReference;

			ZString universalCustomsMessagingRecipientID;

			public List<EDIMessage> CreatedMessages { get; } = new List<EDIMessage>();

			protected override void SetMessageInterpretion(EDIMessage message)
			{
				base.SetMessageInterpretion(message);
				CreatedMessages.Add(message);
			}
		}
	}
}
