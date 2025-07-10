using System;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	public partial class DeliveryDetailsPopupForm : ZChildForm
	{
		public DeliveryDetailsPopupForm()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!DesignModeFinder.IsDesigning)
			{
				var ownerForm = Owner as ZForm;
				if (ownerForm == null || ownerForm.DisplayMode == ODisplayMode.ReadOnly)
				{
					ZFormPostingButtonsStrategy.SetupPosting(this, postingButtonsUserControl);
				}
				else
				{
					postingButtonsUserControl.SaveAndCloseButton.Visible = false;
					postingButtonsUserControl.SaveButton.Visible = false;
					postingButtonsUserControl.CloseButton.Click += CloseButton_Click;
				}
			}
		}

		#region Form Caption

		public override string FormCaption
		{
			get { return Res.GetString("4d50894d-8007-476d-a590-7c16ad531d5f", "Delivery Details: {0}", BusinessEntity != null && BusinessEntity.CampaignItem != null ? BusinessEntity.CampaignItem.CampaignID : ZString.Empty); }
		}

		#endregion

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

		public new EmailDeliveryDetailsProvider BusinessEntity
		{
			get { return (EmailDeliveryDetailsProvider)base.BusinessEntity; }
		}
	}
}
