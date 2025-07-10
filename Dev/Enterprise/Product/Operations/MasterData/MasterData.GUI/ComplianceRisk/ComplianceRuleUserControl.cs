using System;
using Enterprise.ComplianceRisk.Business;
using Enterprise.ComplianceRisk.GUI;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Customs.Universal.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterData.GUI
{
	public partial class ComplianceRuleUserControl : ZUserControl, IComplianceRuleUserControl
	{
		public ComplianceRuleUserControl()
		{
			InitializeComponent();
			var item = new ComplianceRuleBulkUpdateMenuItem(ComplianceRuleGrid);
			item.AddMenuItem();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			var gridColumnStyle = ComplianceRuleGrid.TableStyles[0].GridColumnStyles["CRU_HarmonizedCode"];
			if (gridColumnStyle is TariffColumnStyle tariffColStyle && tariffColStyle.EditControl is TariffGridFindBox findBox)
			{
				findBox.GetDataGrouping = () => "WCO";
			}
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (dataSource is RefCountry country)
			{
				dataSource = new ComplianceRuleBindingObject(country.ComplianceRules as ComplianceRuleCollection);
				base.SetDataBinding(dataSource, nameof(ComplianceRuleBindingObject.ComplianceRules));
			}
			else
			{
				base.SetDataBinding(dataSource, dataMember);
			}
		}
	}
}
