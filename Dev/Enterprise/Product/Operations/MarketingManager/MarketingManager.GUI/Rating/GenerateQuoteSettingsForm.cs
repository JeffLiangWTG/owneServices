using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	public partial class GenerateQuoteSettingsForm : ZChildForm
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1091:Do Not Set CausesValidation to false", Justification = "don't want to validate this whenever this radio button is changed. Only want to validate it manually (e.g. on save)")]
		public GenerateQuoteSettingsForm(GenerateQuoteSettings settings)
			: base(settings)
		{
			Argument.NotNull(settings, "settings");

			if (!DesignModeFinder.IsDesigning)
			{
				this.BackColor = SystemDataRegistry.Instance.ColorTheme.TabBackgroundColor; // must be before InitializeComponent() so that the backcolor of radio button's inherit it
			}

			InitializeComponent();

			if (!DesignModeFinder.IsDesigning)
			{
				mainSplitContainer.Panel1Collapsed = settings.TradeDetailSelectionItems.Count == 0;
				mainSplitContainer.Panel2Collapsed = settings.QuoteSelectionItems.Count == 0;
				createAmendmentRadioButton.CausesValidation = false;
			}
		}

		public new GenerateQuoteSettings BusinessEntity
		{
			get { return (GenerateQuoteSettings)base.BusinessEntity; }
		}

		#region Form Caption

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		#endregion

		#region Buttons

		#region Create Button

		public ZButton CreateButton
		{
			get { return createButton; }
		}

		void CreateButton_Click(object sender, EventArgs e)
		{
			Create();
		}

		void Create()
		{
			BusinessEntity.RunPreSaveValidation();
			if (!BusinessEntity.HasErrors())
			{
				DialogResult = DialogResult.OK;
				Close();
			}
			else
			{
				ShowErrorsDialog();
			}
		}

		#endregion

		#region Cancel Button

		void CancelButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		#endregion

		#endregion
	}
}
