using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Customs.Universal.GUI
{
	public partial class RateFormulaForm : ZChildForm, IFindBoxPopup
	{
		public RateFormulaForm(RateFormulaEditHelper helper, int formulaMaxLength) : base(helper)
		{
			InitializeComponent();
			FormulaTextBox.MaxLength = formulaMaxLength;
			PercentageOfCustomsValueCalcEdit.DataBindings.Add(new KBinding(IsVisibleForBindingString, BindingSource.DataSource, "PercentageOfCustomsValueAvailable"));
			UnitDropEdit.DataBindings.Add(new KBinding(IsVisibleForBindingString, BindingSource.DataSource, "RatePerUnitAvailable"));
			RatePerUnitCalcEdit.DataBindings.Add(new KBinding(IsVisibleForBindingString, BindingSource.DataSource, "RatePerUnitAvailable"));
		}

		public override string FormCaption => Res.GetString("59834726-535A-4CA1-93A8-493704413CC5", "Rate Formula");

		public const string IsVisibleForBindingString = "IsVisibleForBinding";

		public new RateFormulaEditHelper BusinessEntity => (RateFormulaEditHelper)base.BusinessEntity;

		void OkButton_Click(object sender, System.EventArgs e)
		{
			BusinessEntity.ValidateAll();
			if (BusinessEntity.HasErrors())
			{
				ShowErrorsDialog();
			}
			else
			{
				if (findBox != null)
				{
					findBox.Code = BusinessEntity.Formula;
				}
				Close();
			}
		}

		void CloseButton_Click(object sender, System.EventArgs e)
		{
			Close();
		}

		#region IFindBoxPopup Members

		public void ShowModal(IFindBox findBox, Form parentForm)
		{
			this.findBox = findBox;

			BusinessEntity.Formula = this.findBox.Code;
			BusinessEntity.IsFreeFormat = true;

			ZFormModaliser.Show(this, parentForm);
		}

		IFindBox findBox;

		public SilentSelectResult SelectFromPopupWithoutDisplaying(IFindBox findBox, EmbeddedModulePopup popup)
		{
			return SilentSelectResult.None;
		}

		public void SelectRowByPK(ZGuid pK)
		{
		}

		#endregion
	}
}
