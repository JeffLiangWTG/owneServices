using System;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.MasterFiles.Business.AccCashAdvanceDefaultingConfigurationCollection;

namespace Enterprise.MasterFiles.GUI
{
	public partial class CashAdvanceJobConfig : ZUserControl
	{
		public CashAdvanceJobConfig()
		{
			InitializeComponent();
			jobConfigGrid.CurrentCellChanged += JobConfigGrid_CurrentCellChanged;
		}

		#region Overrides

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			if (DataSource != null)
			{
				if (dataSource is GlbCompany company)
				{
					company.CashAdvanceConfigurations.OnChildDefaultingOptionChanged += CashAdvanceConfigurations_OnChildDefaultingOptionChanged;
				}
				else if (dataSource is OrgHeader orgHeader)
				{
					orgHeader.CompanyData.AccARCashAdvanceConfigurations.OnChildDefaultingOptionChanged += CashAdvanceConfigurations_OnChildDefaultingOptionChanged;
				}
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			BindConfigChargeCodesToMultiSelectUserControl();
			SetChargeGroupAndChargeCodeGridVisibility();
		}

		void BindConfigChargeCodesToMultiSelectUserControl()
		{
			if (jobConfigGrid.GetCurrent() is AccCashAdvanceDefaultingConfiguration config)
			{
				chargeCodeMultiSelectUserControl1.Bind(config);
				chargeCodeMultiSelectUserControl1.Enabled = !config.ReadOnly;
			}
		}

		#endregion

		#region Implementation

		void SetChargeGroupAndChargeCodeGridVisibility()
		{
			if (jobConfigGrid.GetCurrent() is AccCashAdvanceDefaultingConfiguration config)
			{
				if (config.CAC_DefaultingOption != CashAdvanceDefaultingOption.SelectedChargeCodesAndGroups)
				{
					ChargeCodesGroupBox.Visible = false;
					ChargeGroupsGroupBox.Visible = false;
				}
				else
				{
					ChargeCodesGroupBox.Visible = true;
					ChargeGroupsGroupBox.Visible = true;
				}
			}
		}

		#region Event Handlers

		void JobConfigGrid_CurrentCellChanged(object sender, EventArgs e)
		{
			BindConfigChargeCodesToMultiSelectUserControl();
			SetChargeGroupAndChargeCodeGridVisibility();
		}

		void CashAdvanceConfigurations_OnChildDefaultingOptionChanged(object sender, DefaultingOptionChangedEventArgs e)
		{
			if (jobConfigGrid.GetCurrent() is AccCashAdvanceDefaultingConfiguration config &&
				config.PK == e.ConfigPk && e.ShouldHide)
			{
				ChargeCodesGroupBox.Visible = false;
				ChargeGroupsGroupBox.Visible = false;
			}
			else
			{
				ChargeCodesGroupBox.Visible = true;
				ChargeGroupsGroupBox.Visible = true;
			}
		}

		#endregion

		#endregion
	}
}
