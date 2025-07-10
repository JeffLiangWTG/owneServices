using System;
using System.Data;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.CustomValues
{
	[CodeProperty(GenCustomAddOnRuleSchema.Constants.XR_Code), DescriptionProperty(GenCustomAddOnRuleSchema.Constants.XR_Code)]
	public class GenCustomAddOnRule : AutoGenCustomAddOnRule
	{
		public GenCustomAddOnRule(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{ }

		[ChildEditable(true)]
		public AvailableRuleCollection AllRules
		{
			get
			{
				if (allRules == null)
				{
					allRules = new AvailableRuleCollection(Factory);
					ICustomAddOnRule[] rules = GetRules();
					foreach (ICustomAddOnRule rule in rules)
					{
						AvailableRule el = allRules.AddNew();
						el.Rule = rule;
					}
					foreach (string ruleCode in CargoWise.Workflow.CustomAddOnRuleTypes.All)
					{
						if (!Array.Exists(rules, rule => rule.Code.Equals(ruleCode)))
						{
							AvailableRule el = allRules.AddNew();
							el.Rule = new RulesFactory().New(ruleCode);
						}
					}
					allRules.Sort(AvailableRule.Schema.Name);
					RegisterEditableChildObject(allRules);
				}

				return allRules;
			}
		}
		AvailableRuleCollection allRules;

		public ICustomAddOnRule[] GetRules()
		{
			XElement xml;
			try
			{
				xml = XElement.Parse(XR_SourceCode);
			}
			catch (XmlException)
			{
				return Array.Empty<ICustomAddOnRule>();
			}

			return new RulesFactory().FromXml(xml).ToArray();
		}

		public void SetRules(params ICustomAddOnRule[] rules)
		{
			XElement xml = new RulesFactory().ToXml(rules);
			XR_SourceCode = xml.ToString(SaveOptions.DisableFormatting);
		}

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();

			if (AllRules.HasChanges)
			{
				SetRules(AllRules.Cast<AvailableRule>().Select(r => r.Rule).ToArray());
			}
		}
	}
}
