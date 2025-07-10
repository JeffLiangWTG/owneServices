using System;
using CargoWise.ComponentModel;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class PackLineConfirmDiscrepanciesDialog : ZChildForm
	{
		internal PackLineConfirmDiscrepancyAction result = PackLineConfirmDiscrepancyAction.NoAction;

		public PackLineConfirmDiscrepanciesDialog(PackLine packLine)
		{
			InitializeComponent();
		}

		void CancelButton_Click(object sender, EventArgs e) => ReturnActionAndClose(PackLineConfirmDiscrepancyAction.NoAction);

		void UpdatePacklineButton_Click(object sender, EventArgs e) => ReturnActionAndClose(PackLineConfirmDiscrepancyAction.UpdatePacklineAndConfirm);

		void AcceptDiscrepancyButton_Click(object sender, EventArgs e) => ReturnActionAndClose(PackLineConfirmDiscrepancyAction.AcceptDiscrepancyAndConfirm);

		void ReturnActionAndClose(PackLineConfirmDiscrepancyAction action)
		{
			result = action;
			Close();
		}

		#region Disable status bar

		protected override bool ShowStatusBar => false;

		protected override void UpdateStatusBar(string notification, INotificationType notificationType)
		{
		}

		#endregion
	}
}
