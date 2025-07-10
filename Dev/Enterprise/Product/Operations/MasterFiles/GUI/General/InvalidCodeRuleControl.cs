using System.Linq;

using CargoWise.EntityFramework;
using CargoWise.Workflow;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.General
{
	public partial class InvalidCodeRuleControl : ZUserControl
	{
		public InvalidCodeRuleControl()
		{
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			GenCustomAddOnRule addOnRule = dataSource as GenCustomAddOnRule;
			if (addOnRule != null)
			{
				AvailableRule availableRule = addOnRule.AllRules.Cast<AvailableRule>().First(r => r.Rule.Code.Equals(CustomAddOnRuleTypes.InvalidCode));
				InvalidCodeRule rule = (InvalidCodeRule)availableRule.Rule;
				BusinessObject objForBinding = rule.GetObjectForBinding(availableRule.Factory);
				availableRule.RegisterEditableChildObject(objForBinding);

				codeDescriptionGrid.SetDataBinding(objForBinding, "Collection");
				base.SetDataBinding(objForBinding, "");
			}
			else
			{
				base.SetDataBinding(dataSource, dataMember);
			}
		}
	}
}
