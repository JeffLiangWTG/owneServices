using System;
using System.Windows.Forms;
using Enterprise.EConversation.Business;
using Enterprise.Recruiter.Business;
using Enterprise.Recruitment.Module;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Tools;

namespace Enterprise.EConversation.GUI
{
	public partial class CandidateEConversationControl : ZUserControl, IConversationView
	{
		public new GroupedEConversation CurrentDataItem => (GroupedEConversation)base.CurrentDataItem;

		readonly IConversationViewController controller;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1046", Justification = "The control is not a button")]
		public CandidateEConversationControl()
		{
			InitializeComponent();
			this.controller = new EConversationViewController();
			controller.Initialize(this);
			econversationMessageTextBox.Hotkeys.RegisterHotKey(Keys.Control | Keys.Enter, SendMessageFromTextbox, Res.GetString("803b6a71-b68c-4138-82db-a3ec67795e85", "Add Note"));
			Enabled = false;

			if (!DesignModeFinder.IsDesigning)
			{
				SpellChecker.InitialiseSpellcheck(econversationMessageTextBox, nameof(econversationMessageTextBox));
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			var oldConversation = CurrentDataItem?.RootConversation;
			if (oldConversation != null)
			{
				oldConversation.Messages.CountChanged -= Messages_CountChanged;
			}

			base.OnCurrentDataItemChanged(e);

			econversationMessageTextBox.Clear();

			var newConversation = CurrentDataItem?.RootConversation;

			Enabled = newConversation != null;
			if (Enabled)
			{
				newConversation.Messages.CountChanged += Messages_CountChanged;
			}

			econversationMessageTextBox.SetDataBinding(newConversation, nameof(newConversation.NextMessage));
		}

		void Messages_CountChanged(object sender, EventArgs e)
		{
			chatboxControl.RefreshMessages();
		}

		void SendMessageFromTextbox()
		{
			controller.OnSendInternalMessage();
			chatboxControl.RefreshMessages();
		}

		#region IConversationView

		JobConversation IConversationView.Conversation => CurrentDataItem?.RootConversation;
		TextBoxBase IConversationView.MessageTextBox => econversationMessageTextBox;
		ZButton IConversationView.SendButton => sendButton;
		ZButton IConversationView.AddInternalCommentButton => AddInternalCommentButton;
		ZButton IConversationView.BroadcastButton => null;

		#endregion
	}
}
