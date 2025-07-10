using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI.Protest
{
	public partial class MessagesUserControl : MessageUserControl
	{
		public MessagesUserControl()
		{
			InitializeComponent();
			AddSentWithErrorsColumn();

			StatusesErrorsLabel.AllowOverlap(messagesStatusErrorsUserControl);
		}

		void AddSentWithErrorsColumn()
		{
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Messaging.Business.CBPEDIMessage)(((System.Collections.IList)(((Customs.Business.BaseJobDeclaration)(null)).Messages)).SyncRoot)).EM_SendWithMessageErrorsFormatted);
			ZArchitecture.ZTextBoxColumnStyleInfo sentWithErrorsBox = new ZArchitecture.ZTextBoxColumnStyleInfo();
			sentWithErrorsBox.Caption = "Sent With Errors";
			sentWithErrorsBox.ColumnName = "EM_SendWithMessageErrorsFormatted";
			this.MessagesGrid.ColumnStyles.Add(sentWithErrorsBox);
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			if (StatusesErrorsLabel.DataBindings[StatusLableVisibilityForBinding] == null && dataSource != null)
			{
				StatusesErrorsLabel.DataBindings.Add(new KBinding(StatusLableVisibilityForBinding, dataSource, "Messages.StatusesAndErrorsVisible", true, DataSourceUpdateMode.Never));
			}
		}
		const string StatusLableVisibilityForBinding = "IsVisibleForBinding";

		void StatusesErrorsLabel_VisibleChanged(object sender, EventArgs e)
		{
			if (this.StatusesErrorsLabel.Visible || (this.MessagesGrid.ListManager != null && this.MessagesGrid.ListManager.Count <= 0))
			{
				this.StatusesErrorsLabel.BringToFront();
			}
		}
	}
}
