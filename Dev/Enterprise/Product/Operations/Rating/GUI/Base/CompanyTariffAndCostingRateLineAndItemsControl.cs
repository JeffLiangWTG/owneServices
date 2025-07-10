using System;
using System.ComponentModel;
using Enterprise.Environment;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;

namespace Enterprise.Rating.GUI
{
	public partial class CompanyTariffAndCostingRateLineAndItemsControl : CostingRateLineAndItemsControl
	{
		public CompanyTariffAndCostingRateLineAndItemsControl()
		{
			InitializeComponent();
		}

		#region Related Rates Results Checkbox

		void CheckBox_ReadOnlyChanged(object sender, EventArgs e)
		{
			if (Header == null)
			{
				((ZCheckBox)sender).ReadOnly = false;
			}
			else if (sender == ClientRatesCheckBox)
			{
				ClientRatesCheckBox.ReadOnly = Header.ShowClientRates_ReadOnly;
			}
			else if (sender == CostingCheckBox)
			{
				CostingCheckBox.ReadOnly = Header.ShowCosting_ReadOnly;
			}
			else if (sender == CompanyTariffCheckBox)
			{
				CompanyTariffCheckBox.ReadOnly = Header.ShowCompanyTariff_ReadOnly;
			}
		}

		void CheckBox_CheckStateChanged(object sender, EventArgs e)
		{
			if (RelatedRateLines != null)
			{
				RateEntry entry = RelatedRateLines.Master;
				entry.InvalidateRelatedRateLinesIfValuesChanged();
			}
		}

		#endregion

		#region Binding

		protected override void OnCurrentChanged(object sender, EventArgs e)
		{
			base.OnCurrentChanged(sender, e);
			if (Header != null)
			{
				IHintExtension hint = CompanyTariffCheckBox.Extensions.Get<IHintExtension>();

				if (!Env.Security.CompanyTariffRates.IsAllowed)
				{
					hint.Description = Res.GetString("74f46322-82c5-4b0c-bb92-e7ea14840b1e", "You do not have security rights to see company tariff information");
				}

				if (!Env.Security.CostingRatesView.IsAllowed)
				{
					hint.Description = Res.GetString("39da50c3-6474-444a-b86a-87fd019dc51e", "You do not have security rights to see costing information");
				}
			}
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion
	}

	public partial class CompanyTariffAndCostingRateLineAndItemsLazyControl : ZUserControl
	{
		public CompanyTariffAndCostingRateLineAndItemsLazyControl()
		{
			InitializeComponent();
		}

		#region Wrapped properties

		[Browsable(true)]
		public virtual string BindTo
		{
			get { return InnerControl.BindTo; }
			set { InnerControl.BindTo = value; }
		}

		public RemoveAction RemoveAction
		{
			get { return InnerControl.RemoveAction; }
			set { InnerControl.RemoveAction = value; }
		}

		#endregion

		#region Design Code

		void showControlLabel_Click(object sender, EventArgs e)
		{
			ShowControlLabel.Visible = false;
			InnerControl.Visible = true;

			var masterEntry = InnerControl.RelatedRateLines?.Master
				?? InnerControl.RateLinesCollection?.Master;

			if (masterEntry != null && masterEntry.SelectedLineChargeCode.IsValid)
			{
				masterEntry.SelectedLineChargeCode = masterEntry.SelectedLineChargeCode;
			}
		}

		#endregion
	}
}

