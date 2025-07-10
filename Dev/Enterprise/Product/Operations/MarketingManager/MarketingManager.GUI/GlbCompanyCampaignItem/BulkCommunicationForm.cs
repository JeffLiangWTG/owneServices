using System;
using System.Linq;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	public partial class BulkCommunicationForm : ZChildForm
	{
		public BulkCommunicationForm(BulkCommunication bulkCommBizObj)
			: base(bulkCommBizObj)
		{
			InitializeComponent();

			if (!DesignModeFinder.IsDesigning)
			{
				bulkCommunicationControlPanel.BackColor = SystemDataRegistry.Instance.ColorTheme.TabBackgroundColor;
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			DisableNewAction();
			if (!DesignModeFinder.IsDesigning)
			{
				ZFormPostingButtonsStrategy.SetupPosting(this, postingButtonsUserControl);
			}

			bulkCommunicationEntryUserControl1.Focus();
			CommunicationGrid.SelectAllElements();
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		protected override bool AllowNew
		{
			get { return false; }
		}

		public new BulkCommunication BusinessEntity
		{
			get { return (BulkCommunication)base.BusinessEntity; }
		}

		#region Form Caption

		public override string FormCaption
		{
			get { return Res.GetString("ed2923a1-f3ea-47cf-b3aa-417e963cb9c3", "Bulk Communication"); }
		}

		#endregion

		void UpdateSelectedButton_Click(object sender, EventArgs e)
		{
			BusinessEntity.UpdateFromBulkCommunication(BusinessEntity, CommunicationGrid.SelectedElements.Cast<OrgSalesCall>());
			BusinessEntity.CommunicationCollection.RefreshBindingIncludingChildren();
		}

		protected override CargoWise.EntityFramework.ContinueWithSave ValidateAndSave()
		{
			var result = base.ValidateAndSave();
			if (result == CargoWise.EntityFramework.ContinueWithSave.Yes && BusinessEntity.CommunicationCollection.Any())
			{
				BusinessEntity.SendCalenderReminder();
			}

			return result;
		}
	}
}
