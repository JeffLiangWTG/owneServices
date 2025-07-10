using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business.Testing
{
	class RatingHeaderWorkflowTriggerNotificationTest : TestCaseWithFactory
	{
		public void TestQuoteSubstituteClient()
		{
			var quote = Factory.New<Quote>();
			quote.TH_QuoteNumber = "TH3222";
			quote.TH_OH = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			var trigger = quote.WorkflowItems.Triggers.AddNew();
			trigger.P9_ParentID = quote.PK;

			var notification = trigger.ProcessTaskNotifications.AddNew();
			notification.PQ_EmailText = "This notes should end up in an email. Organisation:" + "(*Organization*)";
			trigger.P9_Description = "UNDESCRIBABLEBAL";

			var expectedString = string.Format("This notes should end up in an email. Organisation:{0}", quote.Header.OH_FullName);
			AssertProcess(expectedString, notification);
		}

		public void TestClientRateSubstituteClient()
		{
			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			var trigger = clientRate.WorkflowItems.Triggers.AddNew();
			trigger.P9_ParentID = clientRate.PK;

			var notification = trigger.ProcessTaskNotifications.AddNew();
			notification.PQ_EmailText = "This notes should end up in an email. Organisation:" + "(*Organization*)";
			trigger.P9_Description = "UNDESCRIBABLEBAL";

			var expectedString = string.Format("This notes should end up in an email. Organisation:{0}", clientRate.Header.OH_FullName);
			AssertProcess(expectedString, notification);
		}

		EDICommunicationsMode CommunicationsMode
		{
			get
			{
				if (communicationsMode == null)
				{
					communicationsMode = Factory.NewWithValidTestData<EDICommunicationsMode>();
					communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
					communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.NotificationEmail;
					communicationsMode.EK_Destination = "test@test.com";
					communicationsMode.EK_Filename = "test_filename";
					communicationsMode.EK_ServerAddressSubject = "subbjectt";
				}
				return communicationsMode;
			}
		}
		EDICommunicationsMode communicationsMode;

		void AssertProcess(string notesExpected, ProcessTaskNotification action)
		{
			var notification = new WorkflowTriggerNotification(Lazy.Create(() => new MessageProcessorCommunicationModesResult(new EDICommunicationsMode[] { CommunicationsMode }, null)), action, action.Parent.GetJob(), null)
			{
				ExtraDataSubstitution = RatingHeaderWorkflowTriggerNotification.Substitute
			};
			((IProcessor)notification).Process(null);
			Factory.Save();
			AssertEquals("Should create an email", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertContains("Wrong body", notesExpected, Env.OutgoingMailManager.EmailsCreated[0].Body);
			AssertEquals("Should send it to correct recepient", true, Env.OutgoingMailManager.EmailsCreated[0].Recipients.Contains("test@test.com"));
		}
	}
}
