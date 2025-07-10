using System;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public partial class AccTaxOverrideGroupFormBase : ZForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public AccTaxOverrideGroupFormBase()
		{
		}

		public AccTaxOverrideGroupFormBase(AccTaxOverrideGroup taxOverrideGroup) : base(taxOverrideGroup)
		{
		}

		protected new AccTaxOverrideGroup BusinessEntity
		{
			get { return (AccTaxOverrideGroup)base.BusinessEntity; }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();

			InitializeComponent();

			if (!DesignModeFinder.IsDesigning)
			{
				ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
				PlugIns.Add(ControllerIDs.eDocsPlugIn);
				PlugIns.Add(ControllerIDs.Audit);

				if (!GlbCompany.CurrentCompany.Country.IsPartOfEuropeanUnion)
				{
					foreach (ZGridColumnInfo column in TaxOverridesGrid.ColumnStyles)
					{
						if (column.ColumnName == AccChargeTaxOverride.Schema.AO_CustomsStatus)
						{
							TaxOverridesGrid.ColumnStyles.Remove(column);
							break;
						}
					}
				}

				if (GlbCompany.CurrentCompany.Country.Code != Core.Constants.CountryCodes.Italy)
				{
					foreach (ZGridColumnInfo column in TaxOverridesGrid.ColumnStyles)
					{
						if (column.ColumnName == AccChargeTaxOverride.Schema.AO_SplitPaymentVATOrganisation)
						{
							TaxOverridesGrid.ColumnStyles.Remove(column);
							break;
						}
					}
				}

				if (!AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.Value && !DesignModeFinder.IsDesigning)
				{
					foreach (ZGridColumnInfo column in TaxOverridesGrid.ColumnStyles)
					{
						if (column.ColumnName == AccChargeTaxOverride.Schema.AO_GB)
						{
							TaxOverridesGrid.ColumnStyles.Remove(column);
							break;
						}
					}
				}

				if (!AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.Value)
				{
					foreach (ZGridColumnInfo column in TaxOverridesGrid.ColumnStyles)
					{
						if (column.ColumnName == AccChargeTaxOverride.Schema.AO_SupplyType)
						{
							TaxOverridesGrid.ColumnStyles.Remove(column);
							break;
						}
					}
				}

				TaxOverridesGroupBox.Text = Res.GetString("Accounting|AccTaxOverrideGroupForm|TaxOverridesTabPageCaptionSuffix", "Tax Overrides");
			}
		}
	}
}
