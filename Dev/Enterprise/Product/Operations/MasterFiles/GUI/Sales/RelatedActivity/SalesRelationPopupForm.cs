using System;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class SalesRelationPopupForm : ZChildForm
	{
		public SalesRelationPopupForm(ISalesRelationModel model)
			: base(model)
		{
			InitializeComponent();

			if (!DesignModeFinder.IsDesigning)
			{
				salesRelationControlPanel.BackColor = SystemDataRegistry.Instance.ColorTheme.TabBackgroundColor;
				SalesRelationControl.ShowCommunicationsCheckBox.BackColor = SystemDataRegistry.Instance.ColorTheme.TabBackgroundColor;
			}
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
	}
}
