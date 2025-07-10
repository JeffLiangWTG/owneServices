using System;
using CargoWise.Windows.UI;
using Enterprise.EConversation.GUI;

namespace Enterprise.Recruitment.Module
{
	public class CandidateEConversationMessageListUserControl : EConversationMessageListUserControl
	{
		#region Constructors

		public CandidateEConversationMessageListUserControl() : base()
		{ }

		public CandidateEConversationMessageListUserControl(bool isClientSystem, bool hideViewModeOptions)
			: base(isClientSystem, hideViewModeOptions)
		{ }

		public CandidateEConversationMessageListUserControl(bool isClientSystem)
			: base(isClientSystem)
		{ }

		#endregion

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			messagesLayoutPanel.SuspendLayout();

			if (EConversation != null)
			{
				BuildMessageControls();
			}
			else
			{
				messagesLayoutPanel.Controls.RemoveAndDisposeAll();
			}

			messagesLayoutPanel.ResumeLayout();
		}
	}
}
