using System;
using System.ComponentModel;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	sealed partial class UnsubscribeContactsForm : ZChildForm
	{
		public UnsubscribeContactsForm(UnsubscribeContactsBusinessObject unsubscribeContacts)
			: base(unsubscribeContacts)
		{
			InitializeComponent();

			if (!DesignModeFinder.IsDesigning)
			{
				BackColor = SystemDataRegistry.Instance.ColorTheme.TabBackgroundColor;
			}

			if (UnsubscribeContactsBusinessEntity.IsHRCampaign)
			{
				SubscriptionsGrid.RemoveFromAvailableColumns("IsOrgLevel");
				ContactsGrid.RemoveFromAvailableColumns("ClientName");
			}

			ActiveControl = postingButtonsUserControl;
		}

		public override string FormVerb => string.Empty;

		protected override bool AllowNew => false;

		UnsubscribeContactsBusinessObject UnsubscribeContactsBusinessEntity => (UnsubscribeContactsBusinessObject)BusinessEntity;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			DisableNewAction();
			if (!DesignModeFinder.IsDesigning)
			{
				ZFormPostingButtonsStrategy.SetupPosting(this, postingButtonsUserControl);
			}
		}

		protected override void ZForm_Closing(object sender, CancelEventArgs e)
		{
			//	Empty to prevent a second popup from appearing
		}

		protected override void SaveInternal()
		{
			base.SaveInternal();
			UnsubscribeContactsBusinessEntity.UnsubscribeContacts();
		}
	}
}
