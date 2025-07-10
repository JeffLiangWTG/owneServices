using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.CustomValues
{
	public class AvailableRuleValidation : AutoAvailableRuleValidation
	{
		public AvailableRuleValidation(AutoAvailableRule parent)
			: base(parent) { }

		protected override void ValidateAllCore()
		{
			base.ValidateAllCore();
			ValidateIsEnabled();
		}

		public void ValidateIsEnabled()
		{
			ValidateCalculatedProperty(Parent.IsEnabledInfo);
		}

		protected void CheckIsEnabled()
		{
			var otherActiveRules =
				from collection in ((IBusinessObjectInternals)Parent).ParentCollections
				where collection is AvailableRuleCollection
				from rule in collection.Cast<AvailableRule>()
				where rule.IsEnabled && rule.PK != Parent.PK
				select rule;
			var types = from pair in new AddOnColumnDataType().Cast<CodeDescriptionPair>() select AddOnColumnDataType.GetTypeFromCode(pair.Code);
			foreach (var otherRule in otherActiveRules)
			{
				bool intersect = false;
				foreach (var type in types)
				{
					if (Parent.Rule.CanBeApplied(type) && otherRule.Rule.CanBeApplied(type))
					{
						intersect = true;
						break;
					}
				}

				if (!intersect)
				{
					if (Parent.IsEnabled)
					{
						string[] ruleNames = { Parent.Name, otherRule.Name };
						Array.Sort(ruleNames);
						Parent.IsEnabledInfo.AddError(Res.GetString("8e3d2da5-db45-4da4-8a89-265dbb89867b", "'{0}' and '{1}' behaviors are mutually exclusive.", ruleNames[0], ruleNames[1]));
					}
					otherRule.Validation.ValidateIsEnabled();
					break;
				}
			}
		}

		#region Implementation

		public new AvailableRule Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (AvailableRule)base.Parent; }
		}

		#endregion
	}
}
