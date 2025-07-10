using System;
using System.Windows.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class CargoManifestStatusQueryActionForm : ZChildForm
	{
		public CargoManifestStatusQueryActionForm(CargoManifestStatusQueryHeaderObject headerSendingObject)
			: base(headerSendingObject)
		{
			this.headerSendingObject = headerSendingObject;
			InitializeColumns();
		}
		internal readonly CargoManifestStatusQueryHeaderObject headerSendingObject;

		public override string FormVerb
		{
			get { return ""; }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		void InitializeColumns()
		{
			var inBondHeader = headerSendingObject.Header as Customs.Business.CusInBondHeader;
			if (inBondHeader != null)
			{
				this.LevelsGrid.RemoveFromAvailableColumns("UpdateEntryWithResults");
			}
		}

		internal void SendButton_Click(object sender, EventArgs e)
		{
			headerSendingObject.RunPreSaveValidation();
			headerSendingObject.ShouldSendMessage = false;

			if (headerSendingObject.HasErrors)
			{
				Globals.Message.ShowError("Please fix the errors first");
			}
			else if (!headerSendingObject.HasObjectsMarkedForSending)
			{
				Globals.Message.ShowError("You have not selected any related records to send messages for.");
			}
			else
			{
				if (!headerSendingObject.HasNotifications() || Globals.Message.Show("There are notifications. Are you sure you wish to continue?", "Send messages", MessageBoxButtons.OKCancel, DialogResult.Cancel) == DialogResult.OK)
				{
					headerSendingObject.ShouldSendMessage = true;
					Close();
				}
			}
		}

		void CancelButton_Click(object sender, EventArgs e)
		{
			headerSendingObject.ShouldSendMessage = false;
			Close();
		}
	}
}
