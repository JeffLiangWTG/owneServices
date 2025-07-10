using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Integration;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(CommunicationForm))]
	sealed class CommunicationFormTest : ZFormBasherTest
	{
		public void TestSetButtonsToReadOnlyIfDisplayModeReadOnly()
		{
			var communication = Factory.NewWithValidTestData<OrgSalesCall>();

			using (var form = new CommunicationForm(communication))
			{
				form.DisplayMode = ODisplayMode.ReadOnly;
				form.Show();

				AssertEquals(false, form.TradeLanesCheckedListBox.Enabled);
				AssertEquals(false, form.DetailsUserControl.SendInvitationButton.Enabled);
			}
		}

		public void TestSetClientVisibleNoteVisibility()
		{
			var communication = Factory.NewWithValidTestData<OrgSalesCall>();
			communication.OQ_IsReminderClientFacing = true;

			using (var form = new CommunicationFormForTesting(communication))
			{
				form.Show();
				AssertEquals(false, form.NotesSplitContainer_Exposed.Panel2Collapsed);

				communication.OQ_IsReminderClientFacing = false;
				AssertEquals(true, form.NotesSplitContainer_Exposed.Panel2Collapsed);
			}

			using (var form = new CommunicationFormForTesting(communication))
			{
				form.Show();
				AssertEquals(true, form.NotesSplitContainer_Exposed.Panel2Collapsed);

				communication.OQ_IsReminderClientFacing = true;
				AssertEquals(false, form.NotesSplitContainer_Exposed.Panel2Collapsed);
			}

			using (var form = new CommunicationFormForTesting(communication))
			{
				form.Show();
				form.NotesSplitContainer_Exposed.Panel1Collapsed = false;
				form.BusinessEntity_Exposed.OQ_IsReminderClientFacing = new ZBool(false);
				form.BusinessEntity_Exposed.Delete();
				form.SetClientVisibleNoteVisibility_Exposed();
				AssertEquals("NotesSplitContainer_Exposed.Panel1Collapsed should be still false.", false, form.NotesSplitContainer_Exposed.Panel1Collapsed);
			}
		}

		public void TestSetBottomSplitContainerSplitterDistance()
		{
			using (var form = new CommunicationFormForTesting(Factory.NewWithValidTestData<OrgSalesCall>()))
			{
				form.WindowState = FormWindowState.Maximized;
				form.Show();
				Application.DoEvents();

				Assert("BottomSplitContainerSplitterDistance should be set on OnShown event.", form.BottomSplitContainerSplitterDistanceWasSetOnShownEvent);
			}
		}

		public void TestPromptConfirmationIfNeeded()
		{
			var communication = Factory.NewWithValidTestData<OrgSalesCall>();
			var attendee = communication.AdditionalAttendeesContact.AddNew();
			attendee.O6_ReceiverReminder = true;
			communication.OQ_IsReminderClientFacing = true;

			using (var form = new CommunicationFormForTesting(communication))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				communication.OQ_IsReminderClientFacing = false;
				AssertNotNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(false, communication.HasRiskOfSendingInternalNotesToAttendees);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				communication.OQ_IsReminderClientFacing = false;
				AssertNotNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, communication.HasRiskOfSendingInternalNotesToAttendees);
			}
		}

		#region IHasCustomNewRelatedCommunicationHandling

		public void TestTheParentsOfNewRelatedCommunicationsAreTheCurrentlySelectedGridRelatedActivities()
		{
			var communication = Factory.NewWithValidTestData<OrgSalesCall>();
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			Factory.Save();

			communication.RelatedParentActivityPivotCollection.AddActivity(opportunity);
			communication.RelatedParentActivityPivotCollection.AddActivity(inquiry);

			using (var form = new CommunicationFormForTesting(communication))
			{
				form.Show();

				AssertEquals("Precondition", 2, form.RelatedActivitiesGrid_Exposed.InnerGrid.ListManager.Count);
				form.RelatedActivitiesGrid_Exposed.InnerGrid.SelectAllElements();

				var communicationController = ZControllerFactory.Create(ControllerIDs.Communication);
				((ICommunicationController)communicationController).CreateNewWithParentFormBizObjDefaults = true;
				((ICommunicationController)communicationController).ActiveFormOverrideForTesting = form;
				using (var newCommunicationForm = (ZForm)communicationController.ShowNewForm())
				{
					var newCommunication = (OrgSalesCall)newCommunicationForm.BusinessEntity;
					AssertContainsExactElementsInAnyOrder("Should have made the currently selected grid activies the parent of new communication",
						BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer,
						new BusinessObject[] { opportunity, inquiry },
						newCommunication.RelatedParentActivityPivotCollection.Activities.Cast<BusinessObject>());
				}
			}
		}

		public void TestNewRelatedCommunicationsOnLinkedCommunicationsAreLinkedToSameInquiry()
		{
			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			var communication = Factory.NewWithValidTestData<OrgSalesCall>();
			Factory.Save();

			using (var form = new CommunicationFormForTesting(communication))
			{
				form.Show();

				var communicationController = ZControllerFactory.Create(ControllerIDs.Communication);
				((ICommunicationController)communicationController).CreateNewWithParentFormBizObjDefaults = true;
				((ICommunicationController)communicationController).ActiveFormOverrideForTesting = form;

				communication.LinkedInquiry = null;
				using (var newCommunicationForm = (ZForm)communicationController.ShowNewForm())
				{
					var newCommunication = (OrgSalesCall)newCommunicationForm.BusinessEntity;
					AssertNull("Should not link communication to any inquiry when current communication is not linked to any", newCommunication.LinkedInquiry);
				}

				communication.LinkedInquiry = inquiry;
				using (var newCommunicationForm = (ZForm)communicationController.ShowNewForm())
				{
					var newCommunication = (OrgSalesCall)newCommunicationForm.BusinessEntity;
					AssertNotNull("Should link new communication to the same inquiry as current communication's linked inquiry", newCommunication.LinkedInquiry);
					AssertEquals("Should link new communication to the same inquiry as current communication's linked inquiry", inquiry.PK, newCommunication.LinkedInquiry.PK);
				}
			}
		}

		#endregion

		public void TestDragDropMsgFileReturnCodeInvalidEmailFormat()
		{
			var communication = Factory.NewWithValidTestData<OrgSalesCall>();

			using (var form = new CommunicationFormForTesting(communication))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.Show();

				using (var tempFile = TempFile.NewWithExtension("msg"))
				{
					File.WriteAllText(tempFile.Filename, "test");
					var args = new DragEventArgs(new DataObject(DataFormats.FileDrop, new string[] { tempFile.Filename }), 0, 0, 0, DragDropEffects.Copy, DragDropEffects.Copy);
					form.OnDragDrop_Exposed(args);
					AssertEquals("The file can not be parsed,please check it.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestDragDropEmlFileReturnCodeInvalidEmailFormat()
		{
			var communication = Factory.NewWithValidTestData<OrgSalesCall>();

			using (var form = new CommunicationFormForTesting(communication))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.Show();

				using (var tempFile = TempFile.NewWithExtension("eml"))
				{
					File.WriteAllText(tempFile.Filename, "test");
					var args = new DragEventArgs(new DataObject(DataFormats.FileDrop, new string[] { tempFile.Filename }), 0, 0, 0, DragDropEffects.Copy, DragDropEffects.Copy);
					form.OnDragDrop_Exposed(args);
					AssertEquals("The file can not be parsed,please check it.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestDragDropReturnCodeNoMatchingContacts()
		{
			var communication = Factory.NewWithValidTestData<OrgSalesCall>();

			using (var form = new CommunicationFormForTesting(communication))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.Show();

				using (var tempFile = TempFile.NewWithExtension("eml"))
				{
					File.WriteAllText(tempFile.Filename, "From: test@test.com\r\nSubject: test\r\nMessage-Id: <OZNTPWJBVJU4.PUCG9WKJ1HLM1@test>\r\n");
					var args = new DragEventArgs(new DataObject(DataFormats.FileDrop, new string[] { tempFile.Filename }), 0, 0, 0, DragDropEffects.Copy, DragDropEffects.Copy);
					form.OnDragDrop_Exposed(args);
					AssertEquals("No matching contacts were found within the dropped email. Please enter desired Client and Primary Contact details manually.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		[ExpectNoExceptions]
		public void TestDragDropNoException()
		{
			using (var form = new CommunicationFormForTesting(Factory.NewWithValidTestData<OrgSalesCall>()))
			{
				form.Show();
				form.OnDragDrop_Exposed(new DragEventArgs(new DataObject(DataFormats.FileDrop, new string[] { "file.txt" }), 0, 0, 0, DragDropEffects.Copy, DragDropEffects.Copy));
			}
		}

		public void TestDragDropParsedEmailDocType()
		{
			var communication = Factory.NewWithValidTestData<OrgSalesCall>();

			using (var form = new CommunicationFormForTesting(communication))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.Show();

				using (var tempFile = TempFile.NewWithExtension("eml"))
				{
					File.WriteAllText(tempFile.Filename, "test");
					var args = new DragEventArgs(new DataObject(DataFormats.FileDrop, new string[] { tempFile.Filename }), 0, 0, 0, DragDropEffects.Copy, DragDropEffects.Copy);
					form.OnDragDrop_Exposed(args);
					var documents = communication.DocManagerInfo().AllEDocs;
					AssertEquals("Email should have default DocType for Parsed Emails", Core.Constants.RefDocTypes.MiscellaneousDocument, documents[0].DocType);
				}
			}

			var newDocType = Factory.LoadTop1<IRefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, Core.Constants.RefDocTypes.Letter));
			OrganisationsDataRegistry.Instance.CommunicationDocumentTypeParsedEmails.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newDocType.PK.ToGuid());

			var communication2 = Factory.NewWithValidTestData<OrgSalesCall>();

			using (var form = new CommunicationFormForTesting(communication2))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.Show();

				using (var tempFile = TempFile.NewWithExtension("eml"))
				{
					File.WriteAllText(tempFile.Filename, "test");
					var args = new DragEventArgs(new DataObject(DataFormats.FileDrop, new string[] { tempFile.Filename }), 0, 0, 0, DragDropEffects.Copy, DragDropEffects.Copy);
					form.OnDragDrop_Exposed(args);
					var documents = communication2.DocManagerInfo().AllEDocs;
					AssertEquals("Email should have Letter as DocType", Core.Constants.RefDocTypes.Letter, documents[0].DocType);
				}
			}
		}

		public void TestDocDataPlugIn()
		{
			using (var form = new CommunicationFormForTesting(Factory.NewWithValidTestData<OrgSalesCall>()))
			{
				var plugIn = form.PlugIns.GetPlugIn(ControllerIDs.DocDataPlugIn);
				AssertNotNull(plugIn);
			}
		}

		public void TestCommunicationFormLoad_DbHits()
		{
			var communication = Factory.NewWithValidTestData<OrgSalesCall>();
			var opportunity1 = Factory.NewWithValidTestData<OrgOpportunity>();
			var opportunity2 = Factory.NewWithValidTestData<OrgOpportunity>();
			var opportunity3 = Factory.NewWithValidTestData<OrgOpportunity>();
			var opportunity4 = Factory.NewWithValidTestData<OrgOpportunity>();
			var inquiry1 = Factory.NewWithValidTestData<SalesEnquiry>();
			var inquiry2 = Factory.NewWithValidTestData<SalesEnquiry>();
			var childCommunication = Factory.NewWithValidTestData<OrgSalesCall>();
			var arInvoice1 = Factory.NewWithValidTestData(ObjectFactory.GetType<IARInvoice>());
			var arInvoice2 = Factory.NewWithValidTestData(ObjectFactory.GetType<IARInvoice>());
			var arInvoice3 = Factory.NewWithValidTestData(ObjectFactory.GetType<IARInvoice>());

			var quotedBooking1 = ObjectFactory.Get<IQuotedBookingBuilder>().CreateNew(QuoteBookingType.QuickBooking, Factory);
			var quotedBooking2 = ObjectFactory.Get<IQuotedBookingBuilder>().CreateNew(QuoteBookingType.QuickBooking, Factory);

			Factory.Save();

			communication.RelatedParentActivityPivotCollection.AddActivity(opportunity1);
			communication.RelatedParentActivityPivotCollection.AddActivity(opportunity2);
			communication.RelatedParentActivityPivotCollection.AddActivity(opportunity3);
			communication.RelatedParentActivityPivotCollection.AddActivity(opportunity4);
			communication.RelatedParentActivityPivotCollection.AddActivity(inquiry1);
			communication.RelatedParentActivityPivotCollection.AddActivity(inquiry2);
			communication.RelatedParentActivityPivotCollection.AddActivity(childCommunication);
			communication.RelatedParentActivityPivotCollection.AddActivity(arInvoice1 as IRelatableActivity);
			communication.RelatedParentActivityPivotCollection.AddActivity(arInvoice2 as IRelatableActivity);
			communication.RelatedParentActivityPivotCollection.AddActivity(arInvoice3 as IRelatableActivity);
			communication.RelatedParentActivityPivotCollection.AddActivity(quotedBooking1 as IRelatableActivity);
			communication.RelatedParentActivityPivotCollection.AddActivity(quotedBooking2 as IRelatableActivity);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var communicationReloaded = newFactory.Load<OrgSalesCall>(communication.PK);

			using (var form = new CommunicationFormForTesting(communicationReloaded))
			{
				newFactory.ResetDatabaseLoadCount();

				using (AssertDbHitsWithUsefulQueryInformation(new Dictionary<string, int>
				{
					{ OrgOpportunitySchema.Constants.TableName, 1 },
					{ OrgColdCallRegisterSchema.Constants.TableName, 1 },
					{ OrgSalesCallSchema.Constants.TableName, 1 },
					{ AccTransactionHeaderSchema.Constants.TableName, 1 },
					{ ViewQuotedBookingSchema.Constants.TableName, 2 },
					{ ViewRelatedActivityPivotSchema.Constants.TableName, 3 },
					{ JobDocAddressSchema.Constants.TableName, 6 },
				}, newFactory, ignoredNotSpecifiedUnlessGreaterThan5Hits: true))
				{
					form.Show();
					Application.DoEvents();
				}
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var communication = Factory.New<OrgSalesCall>();
			var form = new CommunicationForm(communication);
			form.ControllerID = ControllerIDs.Communication;

			return form;
		}

		class CommunicationFormForTesting : CommunicationForm
		{
			public CommunicationFormForTesting(OrgSalesCall communication)
				: base(communication)
			{
			}

			public RelatedActivityButtonGrid RelatedActivitiesGrid_Exposed
			{
				get { return RelatedActivitiesGrid; }
			}

			public KSplitContainer NotesSplitContainer_Exposed
			{
				get { return NotesSplitContainer; }
			}

			public void OnDragDrop_Exposed(DragEventArgs drgevent)
			{
				OnDragDrop(drgevent);
			}

			public OrgSalesCall BusinessEntity_Exposed
			{
				get { return (OrgSalesCall)base.BusinessEntity; }
			}

			public void SetClientVisibleNoteVisibility_Exposed()
			{
				SetClientVisibleNoteVisibility(null, null);
			}

			protected override void SetBottomSplitContainerSplitterDistanceOnShown()
			{
				base.SetBottomSplitContainerSplitterDistanceOnShown();
				BottomSplitContainerSplitterDistanceWasSetOnShownEvent = true;
			}

			internal bool BottomSplitContainerSplitterDistanceWasSetOnShownEvent;
		}

		#endregion
	}
}
