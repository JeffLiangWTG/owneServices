using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Interop.DataObjects;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.GUI
{
	public partial class CommunicationForm : ZTemplateForm, IHasCustomNewRelatedCommunicationHandling, IEDocsUnattendedConfigProvider, OrgSalesCallEmailParser.IGUIProvider
	{
		public CommunicationForm()
		{
			InitializeComponent();
		}

		public CommunicationForm(OrgSalesCall communication)
			: base(communication)
		{
			InitializeComponent();
			WorkflowTabPage.Initialize(communication);
			PlugIns.Add(ControllerIDs.DocDataPlugIn);
			CommunicationCustomFieldsControl.NothingSetupMessageLabelText = Res.GetString("32B2194A-4C21-4F1A-8F76-2C89A885C16E", "To make use of this tab, please setup communication manager custom fields in Workflow Manager.");
		}

		new OrgSalesCall BusinessEntity
		{
			get { return (OrgSalesCall)base.BusinessEntity; }
		}

		public override string FormCaption
		{
			get
			{
				var caption = Res.GetString("9bd94813-d5de-4cd5-9e52-70a22240a9f9", "Communication");
				if (BusinessEntity != null)
				{
					return caption + " " + BusinessEntity.OQ_CommunicationID;
				}
				else
				{
					return caption;
				}
			}
		}

		#region Load

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!DesignModeFinder.IsDesigning)
			{
				CallNotesTabPage.SetupSecurity(Env.Security.CommunicationManagerViewNotes);
				FollowupNotesTabPage.SetupSecurity(Env.Security.CommunicationManagerViewFollowUpNotes);

				SetButtonsToReadOnlyIfDisplayModeReadOnly();
			}
		}

		void SetButtonsToReadOnlyIfDisplayModeReadOnly()
		{
			if (DisplayMode == ODisplayMode.ReadOnly)
			{
				TradeLanesCheckedListBox.Enabled = false;
				DetailsUserControl.SetButtonsReadOnly();
			}
		}

		#endregion

		#region SetDataBinding

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (BusinessEntity != null)
			{
				BusinessEntity.OQ_IsReminderClientFacingInfo.ValueChanged -= new EventHandler(SetClientVisibleNoteVisibility);
				BusinessEntity.OQ_IsReminderClientFacingInfo.ValueChanged -= new EventHandler(PromptConfirmationIfNeeded);
			}
			base.SetDataBinding(dataSource, dataMember);
			if (BusinessEntity != null)
			{
				BusinessEntity.OQ_IsReminderClientFacingInfo.ValueChanged += new EventHandler(SetClientVisibleNoteVisibility);
				BusinessEntity.OQ_IsReminderClientFacingInfo.ValueChanged += new EventHandler(PromptConfirmationIfNeeded);
			}
		}

#if DEBUG
		protected
#endif
		void SetClientVisibleNoteVisibility(object sender, EventArgs e)
		{
			SetClientVisibleNoteVisibility();
		}

		void SetClientVisibleNoteVisibility()
		{
			if (BusinessEntity != null && !BusinessEntity.IsDeleted && NotesSplitContainer != null)
			{
				NotesSplitContainer.Panel2Collapsed = !BusinessEntity.OQ_IsReminderClientFacing;
			}
		}

		void PromptConfirmationIfNeeded(object sender, EventArgs e)
		{
			var communication = CurrentDataItem as OrgSalesCall;
			if (communication != null && communication.HasRiskOfSendingInternalNotesToAttendees)
			{
				string caption = Res.GetString("732FAD82-03AD-4DF7-BE09-F5CC0C0C536C", "Un-ticking Client Visible Invitation");
				string message = Res.GetString("C20BF4B6-FAD8-453D-9BE3-70BA940C6827", @"Note: You have unchecked the Calendar invitation version flag whilst attendees are flagged to receive reminders. 
To avoid the undesired effect of delivering an internal calendar version to your contact attendee(s), you must ensure that all contacts reminders are unticked and saved first.  You can then proceed to untick the client visible invitation. 

If you proceed to save, your additional contacts may receive an internal calendar version cancellation.  Select No to cancel this action and follow the required steps.");
				var result = Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
				if (result == DialogResult.No)
				{
					communication.FixRiskOfSendingInternalNotesToAttendees();
				}
			}
		}

		#endregion

		#region Layout

		protected override void OnLayout(LayoutEventArgs levent)
		{
			base.OnLayout(levent);
			SetBottomSplitContainerSplitterDistance();
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			SetBottomSplitContainerSplitterDistanceOnShown();
		}

		protected virtual void SetBottomSplitContainerSplitterDistanceOnShown()
		{
			SetBottomSplitContainerSplitterDistance();
		}

		void SetBottomSplitContainerSplitterDistance()
		{
			if (Visible && DetailsUserControl != null && bottomSplitContainer != null)
			{
				var scaleX = ControlDpiScalingHelper.UnscaleFromCurrentDpiX(DetailsUserControl.AdditionalAttendeeGridTableLayoutPanel.GetColumnWidths()[0]);
				var anchorPoint = DetailsUserControl.AdditionalAttendeeGridTableLayoutPanel.PointToScreen(ControlDpiScalingHelper.NewScaledPoint(scaleX, 0));
				var splitterDistanceLowerBound = bottomSplitContainer.Panel1MinSize;
				var splitterDistanceUpperBound = bottomSplitContainer.Width - bottomSplitContainer.Panel2MinSize;

				// Must check that the splitter distance can be within the two bounds, otherwise will throw exception when we try to set it
				if (splitterDistanceUpperBound > splitterDistanceLowerBound + 2)
				{
					bottomSplitContainer.SplitterDistance = Math.Max(splitterDistanceLowerBound + 1, Math.Min(splitterDistanceUpperBound - 1, bottomSplitContainer.PointToClient(anchorPoint).X - 2));
				}

				if (NotesSplitContainer != null)
				{
					SetClientVisibleNoteVisibility();
				}
			}
		}

		#endregion

		#region IHasCustomNewRelatedCommunicationHandling

		void IHasCustomNewRelatedCommunicationHandling.OnShowingNewFormForRelated(NewRelatedCommunicationArgs newRelatedCommunicationArgs)
		{
			newRelatedCommunicationArgs.ActivitiesToMakeParent.Clear();
			foreach (var selectedGridActivity in RelatedActivitiesGrid.CurrentlySelectedGridActivitiesWithFallback)
			{
				newRelatedCommunicationArgs.ActivitiesToMakeParent.Add(selectedGridActivity);
			}

			var communication = BusinessEntity;
			var newCommunication = newRelatedCommunicationArgs.NewRelatedCommunication as OrgSalesCall;
			if (communication != null && newCommunication != null)
			{
				newCommunication.LinkedInquiry = communication.LinkedInquiry;
			}
		}

		#endregion

		#region DragDrop

		protected override void OnDragDrop(DragEventArgs drgevent)
		{
			var shouldSuspendEDocPopup = false;

			if (drgevent?.Data != null && BusinessEntity != null && !BusinessEntity.IsInDatabase)
			{
				using (var insertableData = ZDataObject.FromData(drgevent.Data))
				{
					ZString fileName = (insertableData?.GetData("FileDrop") as string[])?.FirstOrDefault();
					var result = new OrgSalesCallEmailParser(BusinessEntity, this).PopulateFromEmailFile(fileName);

					shouldSuspendEDocPopup = !(result == OrgSalesCallEmailParser.Result.NotEmailFormat);

					if (result == OrgSalesCallEmailParser.Result.InvalidEmailFormat)
					{
						var caption = Res.GetString("60C62326-DAE8-493C-BF30-77AD3EE6E9B6", "Can not be parsed");
						var message = Res.GetString("0E744406-04B1-492A-B0C4-E40143205CEF", "The file can not be parsed,please check it.");
						Globals.Message.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
					}
					else if (result == OrgSalesCallEmailParser.Result.NoMatchingContacts)
					{
						var caption = Res.GetString("B87596FD-BED4-44A0-9EB8-9682A315A4F6", "No matching contacts");
						var message = Res.GetString("8B2CE5CA-487B-4BC2-AA10-048C836FA047", "No matching contacts were found within the dropped email. Please enter desired Client and Primary Contact details manually.");
						Globals.Message.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
					}
				}
			}

			using (var suspender = shouldSuspendEDocPopup ? EDocPopupSuspender.GetSuspender() : null)
			{
				base.OnDragDrop(drgevent);
			}
		}

		readonly FunctionalitySuspender EDocPopupSuspender = new FunctionalitySuspender();

		bool IEDocsUnattendedConfigProvider.ApplyDocumentConfig(IeDoc eDoc)
		{
			if (EDocPopupSuspender.IsSuspended)
			{
				var factory = BusinessEntity.Factory.GetCachedReadOnlyFactory();
				var docType = factory.Load<RefDocType>(OrganisationsDataRegistry.Instance.CommunicationDocumentTypeParsedEmails.Value);
				if (docType != null)
				{
					eDoc.DocType = docType.RT_DocType;
					eDoc.Description = docType.RT_DescMultilingual;
				}
				else
				{
					eDoc.DocType = RefDocTypes.MiscellaneousDocument;
					eDoc.Description = RefDocTypeDescriptions.MiscellaneousDocument;
				}
				return true;
			}
			else
			{
				return false;
			}
		}

		OrgContact OrgSalesCallEmailParser.IGUIProvider.SelectOrgContact(FilteredContactsCollectionWrapper contacts)
		{
			using (var form = new OrgContactSelectionForm(contacts))
			{
				ZFormModaliser.ShowDialogWithoutDispose(form, this);
				return form.SelectedOrgContact;
			}
		}

		#endregion
	}
}
