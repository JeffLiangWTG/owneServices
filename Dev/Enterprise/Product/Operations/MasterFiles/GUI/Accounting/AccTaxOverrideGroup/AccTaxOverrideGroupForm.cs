using System;
using System.Windows.Forms;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class AccTaxOverrideGroupForm : AccTaxOverrideGroupFormBase, IAllowTabBackwardBetweenSomeOfMyChildren
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public AccTaxOverrideGroupForm()
		{
		}

		public AccTaxOverrideGroupForm(AccTaxOverrideGroup taxOverrideGroup) : base(taxOverrideGroup)
		{
			DeleteDuplicateOverridesButton.AllowOverlap(ChargeCodesModuleButtonGrid);
		}

		public override string FormCaption
		{
			get
			{
				return Res.GetString("551706A3-17FF-4244-8536-7E6F7672BADF", "{0} Tax Override Group", GlbCompany.CurrentCompany.Country.ConsumptionTaxDescription);
			}
		}

		bool IAllowTabBackwardBetweenSomeOfMyChildren.AllowTabBackward(Control control, Control previousControl)
		{
			return (control.Name == "ChargeCodesModuleButtonGrid" && previousControl.Name == "DeleteDuplicateOverridesButton")
				|| (control.Name == "DeleteDuplicateOverridesButton" && previousControl.Name == "ChargeCodesModuleButtonGrid");
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();

			InitializeComponent();

			if (!DesignModeFinder.IsDesigning)
			{
				if (!GlbCompany.CurrentCompany.Country.IsPartOfEuropeanUnion)
				{
					foreach (ZGridColumnInfo column in ChargeCodeTaxOverridesGrid.ColumnStyles)
					{
						if (column.ColumnName == AccChargeTaxOverride.Schema.AO_CustomsStatus)
						{
							ChargeCodeTaxOverridesGrid.ColumnStyles.Remove(column);
							break;
						}
					}
				}

				if (GlbCompany.CurrentCompany.Country.Code != Core.Constants.CountryCodes.Italy)
				{
					foreach (ZGridColumnInfo column in ChargeCodeTaxOverridesGrid.ColumnStyles)
					{
						if (column.ColumnName == AccChargeTaxOverride.Schema.AO_SplitPaymentVATOrganisation)
						{
							ChargeCodeTaxOverridesGrid.ColumnStyles.Remove(column);
							break;
						}
					}
				}

				if (!AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.Value)
				{
					foreach (ZGridColumnInfo column in ChargeCodeTaxOverridesGrid.ColumnStyles)
					{
						if (column.ColumnName == AccChargeTaxOverride.Schema.AO_SupplyType)
						{
							ChargeCodeTaxOverridesGrid.ColumnStyles.Remove(column);
							break;
						}
					}
				}

				foreach (ZGridColumnInfo column in TaxOverridesGrid.ColumnStyles)
				{
					if (column.ColumnName == AccChargeTaxOverride.Schema.AO_CreateTaxRecord)
					{
						TaxOverridesGrid.ColumnStyles.Remove(column);
						break;
					}
				}

				TaxOverridesGroupBox.Text = GlbCompany.CurrentCompany.Country.ConsumptionTaxDescription + " " + TaxOverridesGroupBox.Text;
				ChargeCodeTaxOverridesGroupBox.Text = Res.GetString("2822309C-B5B6-4331-B6D3-F8484B676DD7", "Charge Code") + " " + TaxOverridesGroupBox.Text;
			}
		}

		void DeleteDuplicateOverridesButton_Click(object sender, EventArgs e)
		{
			BusinessEntity.DeleteDuplicateTaxOverridesInChargeCodes();
		}
	}
}
