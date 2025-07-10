using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI.Permits
{
	public partial class CusGuaranteeRuleUserControl : ZUserControl, IAllowTabBackwardBetweenSomeOfMyChildren
	{
		public CusGuaranteeRuleUserControl()
		{
			InitializeComponent();
			PermitRuleValueFromText.AllowOverlap(PermitRuleValueFromTextDropEdit);
			PermitRuleValueFromText.AllowOverlap(PermitRuleValueFromTextCodeFindBox);
			PermitRuleValueFromTextDropEdit.AllowOverlap(PermitRuleValueFromTextCodeFindBox);
			PermitRuleValueToZTextBox.AllowOverlap(PermitRuleValueToTextDropEdit);
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			AddVisibleBindings();
		}

		void AddVisibleBindings()
		{
			if (DataSource != null)
			{
				const string isVisibleForBinding = "IsVisibleForBinding";
				PermitRuleValueFromText.DataBindings.RemoveBinding(isVisibleForBinding);
				PermitRuleValueFromText.DataBindings.Add(new KBinding(isVisibleForBinding, DataSource, PermitRuleGrid.BindTo + ".ValueFromFieldIsOthers"));

				PermitRuleValueFromTextDropEdit.DataBindings.RemoveBinding(isVisibleForBinding);
				PermitRuleValueFromTextDropEdit.DataBindings.Add(new KBinding(isVisibleForBinding, DataSource, PermitRuleGrid.BindTo + ".ValueFromFieldIsTextDropEdit"));

				PermitRuleValueFromTextCodeFindBox.DataBindings.RemoveBinding(isVisibleForBinding);
				PermitRuleValueFromTextCodeFindBox.DataBindings.Add(new KBinding(isVisibleForBinding, DataSource, PermitRuleGrid.BindTo + ".ValueFromFieldIsTextCodeFindBox"));

				PermitRuleValueToZTextBox.DataBindings.RemoveBinding(isVisibleForBinding);
				PermitRuleValueToZTextBox.DataBindings.Add(new KBinding(isVisibleForBinding, DataSource, PermitRuleGrid.BindTo + ".ValueToFieldIsOthers"));

				PermitRuleValueToTextDropEdit.DataBindings.RemoveBinding(isVisibleForBinding);
				PermitRuleValueToTextDropEdit.DataBindings.Add(new KBinding(isVisibleForBinding, DataSource, PermitRuleGrid.BindTo + ".ValueToFieldIsTextDropEdit"));
			}
		}

		bool IAllowTabBackwardBetweenSomeOfMyChildren.AllowTabBackward(Control control, Control previousControl)
		{
			return control is ZCodeFindBox && previousControl is ZTextBox;
		}
	}
}
