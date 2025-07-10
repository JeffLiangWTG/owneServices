using System;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.GUI
{
	public partial class BondDetailsUserControl : ZUserControl
	{
		public BondDetailsUserControl()
		{
			InitializeComponent();
			AddNumericEvents(CEI_WHSMonthTextBox);
		}

		JobDeclaration Declaration => (JobDeclaration)base.CurrentDataItem;

		ZBool IsImport => Declaration?.IsImport ?? ZBool.False;

		public const string IsVisibleForBindingString = "IsVisibleForBinding";

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			CEI_ReasonForDutyDropEdit.DataBindings.RemoveBinding(IsVisibleForBindingString);
			CEI_BillOfMaterialsCheckBox.DataBindings.RemoveBinding(IsVisibleForBindingString);
			CEI_DutyRefundCheckBox.DataBindings.RemoveBinding(IsVisibleForBindingString);
			CEI_BillOfMaterialsPageNoCalcEdit.DataBindings.RemoveBinding(IsVisibleForBindingString);

			if (dataSource is JobDeclaration declaration)
			{
				CEI_ReasonForDutyDropEdit.DataBindings.Add(new KBinding(IsVisibleForBindingString, BindingSource.DataSource, "IsImport", false, DataSourceUpdateMode.Never));
				CEI_BillOfMaterialsCheckBox.DataBindings.Add(new KBinding(IsVisibleForBindingString, BindingSource.DataSource, "IsExport", false, DataSourceUpdateMode.Never));
				CEI_DutyRefundCheckBox.DataBindings.Add(new KBinding(IsVisibleForBindingString, BindingSource.DataSource, "IsExport", false, DataSourceUpdateMode.Never));
				CEI_BillOfMaterialsPageNoCalcEdit.DataBindings.Add(new KBinding(IsVisibleForBindingString, BindingSource.DataSource, "CustomsEntryInstructions.IsBillOfMaterialsEnable", false, DataSourceUpdateMode.Never));

				declaration.JE_MessageTypeInfo.ValueChanged -= JE_MessageTypeInfo_ValueChanged;
				declaration.JE_MessageTypeInfo.ValueChanged += JE_MessageTypeInfo_ValueChanged;
				JE_MessageTypeInfo_ValueChanged(null, null);
			}
		}

		void JE_MessageTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			if (IsImport)
			{
				TopRightGroupBox.GetExtension<LabelCaptionRenderer>().Caption = Res.GetString("TopRightGroupBox|29ecdd9d-513d-4aeb-92ed-9db8ac6a1b2c", "Domestic Sales");
				RelatedBondedPartiesGroupBox.GetExtension<LabelCaptionRenderer>().Caption = Res.GetString("BondedFactoryGroupBox|e1339969-9868-48e1-8fae-51ceecd41d1b", "Previous Bonded Parties");
			}
			else
			{
				TopRightGroupBox.GetExtension<LabelCaptionRenderer>().Caption = Res.GetString("TopRightGroupBox|a203a7c1-41d9-4e37-a9fd-ad5edb6bb301", "Materials");
				RelatedBondedPartiesGroupBox.GetExtension<LabelCaptionRenderer>().Caption = Res.GetString("BondedFactoryGroupBox|a46b7de2-8d12-4815-9809-13eb3f6dd50c", "Related Bonded Parties");
			}
		}

		void AddNumericEvents(ZArchitecture.ZTextBox textBox)
		{
			textBox.KeyPress -= Numeric_KeyPress;
			textBox.KeyPress += Numeric_KeyPress;

			textBox.TextChanged -= Numeric_TextChanged;
			textBox.TextChanged += Numeric_TextChanged;
		}

		void Numeric_TextChanged(object sender, EventArgs e)
		{
			var textBox = sender as ZArchitecture.ZTextBox;
			if (textBox != null)
			{
				var number = new ZString(textBox.Text);
				if (!number.IsEmpty && !number.IsNumbersOnlyOrEmpty)
				{
					textBox.Text = ZString.Empty;
				}
			}
		}

		void Numeric_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (!char.IsNumber(e.KeyChar) && !char.IsPunctuation(e.KeyChar) && !char.IsControl(e.KeyChar))
			{
				e.Handled = true;
			}
		}
	}
}
