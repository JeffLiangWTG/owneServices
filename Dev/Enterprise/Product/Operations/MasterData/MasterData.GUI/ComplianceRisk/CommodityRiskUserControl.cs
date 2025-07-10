using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ComplianceRisk.Business;
using Enterprise.ComplianceRisk.GUI;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Customs.Universal.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.MasterData.GUI
{
	public partial class CommodityRiskUserControl : ZUserControl, ICommodityRiskUserControl
	{
		public CommodityRiskUserControl(ComplianceRiskPlugInBusinessObject plugInBizO)
		{
			HostBusinessEntity = plugInBizO.HostBusinessEntity;
			InitializeComponent();

			if (HostBusinessEntity is IForwardingConsol)
			{
				CommodityRiskGrid.ColumnStyles.Remove(CommodityRiskGrid.GetColumnStyle("LegalBookLink"));
			}
			else
			{
				(CommodityRiskGrid.GetColumnStyle((NoResString)"Source")).IsVisible = false;
			}

			(CommodityRiskGrid.GetColumnStyle((NoResString)"NomenclatureCondition")).IsVisible = false;
			(CommodityRiskGrid.GetColumnStyle((NoResString)"SpecificCondition")).IsVisible = false;

			(new CommoditiesDetailsGridActionMenuItem(plugInBizO, CommodityRiskGrid)).AddMenuItems();

			if (ComplianceRiskHelper.IsCustomsEnabledManageRiskStatusOnCommercialInvoice
				&& OrganisationsDataRegistry.Instance.CustomsShowImportAlertsOnExportDeclarations.Value
				&& HostBusinessEntity is IComplianceCommodityRiskStatusProvider provider
				&& provider.RiskCalculateFactor == CommodityRiskCalculateFactor.Export)
			{
				CommodityRiskGrid.ColumnStyles.Insert(1, zImportAlertsForExportJobDescription);
			}
		}

		public void AddOrRemoveComplianceAlertsForExportDeclaration(bool needImportAlerts)
		{
			if (needImportAlerts)
			{
				if (!CommodityRiskGrid.ColumnStyles.Contains(zImportAlertsForExportJobDescription))
				{
					BindingSource.SetBindingMember(CommodityRiskGrid, "");
					CommodityRiskGrid.ColumnStyles.Insert(1, zImportAlertsForExportJobDescription);
					BindingSource.SetBindingMember(CommodityRiskGrid, "CommodityDetailCollectionView");
				}
			}
			else
			{
				if (CommodityRiskGrid.ColumnStyles.Contains(zImportAlertsForExportJobDescription))
				{
					BindingSource.SetBindingMember(CommodityRiskGrid, "");
					CommodityRiskGrid.ColumnStyles.Remove(zImportAlertsForExportJobDescription);
					BindingSource.SetBindingMember(CommodityRiskGrid, "CommodityDetailCollectionView");
				}
			}
		}

		IBusiness HostBusinessEntity { get; }

		public Func<bool> ValidateSecurityEditHarmonizedCode { get; set; }

		public Func<bool> ValidateSecurityEditComplianceAssessment { get; set; }

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			HookValidateSecurityEditHarmonizedCode();
			HookValidateSecurityEditComplianceAssessment();
		}

		void HookValidateSecurityEditHarmonizedCode()
		{
			var tariffColumnStyle = CommodityRiskGrid?.TableStyles.Count > 0 ? CommodityRiskGrid?.TableStyles[0]?.GridColumnStyles["CCD_HarmonizedCode"] : null;
			if (tariffColumnStyle is TariffColumnStyle tariffColStyle && tariffColStyle.EditControl is TariffGridFindBox findBox)
			{
				findBox.GetDataGrouping = () => "WCO";
				findBox.CodeBox.Enter += ((sender, args) =>
				{
					if (!(ValidateSecurityEditHarmonizedCode?.Invoke() ?? false) && sender is Control control && control.Parent.Enabled)
					{
						control.Parent.Enabled = false;
					}
				});
			}
		}

		void HookValidateSecurityEditComplianceAssessment()
		{
			var riskStatusColumnStyle = CommodityRiskGrid?.TableStyles.Count > 0 ? CommodityRiskGrid?.TableStyles[0]?.GridColumnStyles["CCD_RiskStatusDescription"] : null;
			if (riskStatusColumnStyle is CommodityRiskStatusColumnStyle riskStatusColSytle)
			{
				riskStatusColSytle.EditControl.Enter += ((sender, args) =>
				{
					if (!(ValidateSecurityEditComplianceAssessment?.Invoke() ?? false) && sender is Control control && control.Enabled)
					{
						control.Enabled = false;
					}
				});
			}
		}

		async void CommodityRiskGrid_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Clicks == 1)
			{
				var hitTest = CommodityRiskGrid.HitTest(e.X, e.Y);
				var row = hitTest.Row;
				var col = hitTest.Column;
				if (row >= 0 && col >= 0)
				{
					const string columnName = "LegalBookLink";
					var column = CommodityRiskGrid.Columns[col];
					if (column.ColumnName == columnName)
					{
						var listManager = CommodityRiskGrid.ListManager.List;
						if (row < listManager.Count)
						{
							var commodityDetail = listManager[row] as ComplianceCommodityDetail;
							if (commodityDetail != null && !string.IsNullOrEmpty(commodityDetail.CCD_HarmonizedCode) && commodityDetail.CommodityType != CommodityType.RelatedJobLink && commodityDetail.ComplianceRiskStatus != null)
							{
								await commodityDetail.ComplianceRiskStatus.ViewBorderWisePortalIfAvailable(commodityDetail);
							}
						}
					}
				}
			}
		}
	}
}
