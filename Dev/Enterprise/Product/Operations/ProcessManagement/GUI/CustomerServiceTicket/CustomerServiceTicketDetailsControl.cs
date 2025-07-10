using System;
using CargoWise.Windows.UI;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ProcessManagement.GUI
{
	public partial class CustomerServiceTicketDetailsControl : ZUserControl
	{
		public CustomerServiceTicketDetailsControl(WorkRequest request)
		{
			InitializeComponent();
			CustomiseControls();
			SetConversationBinding(request);
			SetControlVisibilityConfiguration();
		}

		void CustomiseControls()
		{
			DescriptionEConvoSplitContainer.Panel2MinSize = ControlDpiScalingHelper.ScaleToCurrentDpiX(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(eConversationControl.Width) + 3);
			CustomFieldsControl.NothingSetupMessageLabelText = Res.GetString("e6f69df4-498b-4779-b8f7-fa8d2a3fffaa", "To make use of this tab, please setup Customer Service Ticket custom fields in Workflow Manager.");
		}

		void SetControlVisibilityConfiguration()
		{
			VisibilityConfigurationProvider.SetIsVisibilityConfigured(LeftTopPanel, true);
			VisibilityConfigurationProvider.SetIsVisibilityConfigured(MiddleTopPanel, true);
		}

		internal void SetConversationBinding(WorkRequest request)
		{
			var canBindToConversation = request != null && request.IsInDatabase;
			BindingSource.SetBindingMember(eConversationControl, canBindToConversation ? nameof(request.Conversation) : null);

			if (!canBindToConversation)
			{
				eConversationControl.SetMessageTextboxText(Res.GetString("d44cc071-26a6-4b8c-8459-7368f9b959a6", "Please save the form before using eConversation."));
				eConversationControl.DisableInputs();
			}
			else if (eConversationControl.AreInputsReadOnly)
			{
				eConversationControl.EnableInputs();
				eConversationControl.SetMessageTextboxText(string.Empty);
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			SelectionCriterion1DropEdit.CaptionResourceString = WorkRequest.SelectionCriterion1CaptionResourceString;
			SelectionCriterion2DropEdit.CaptionResourceString = WorkRequest.SelectionCriterion2CaptionResourceString;
			SelectionCriterion3DropEdit.CaptionResourceString = WorkRequest.SelectionCriterion3CaptionResourceString;
			SelectionCriterion4DropEdit.CaptionResourceString = WorkRequest.SelectionCriterion4CaptionResourceString;
			SelectionCriterion5DropEdit.CaptionResourceString = WorkRequest.SelectionCriterion5CaptionResourceString;

			base.OnLoad(e);
		}

#if DEBUG
		public ControlVisibilityConfigurationProvider GetVisibilityConfigurationProviderForTest()
		{
			return VisibilityConfigurationProvider;
		}
#endif
	}
}
