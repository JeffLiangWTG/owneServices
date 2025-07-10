using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.CustomValues
{
	[ModuleID(ModuleId.GenCustomAddOnRule)]
	public class GenCustomAddOnRuleCollection : ActiveBusinessObjectCollection<GenCustomAddOnRule>
	{
		public GenCustomAddOnRuleCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			return new ZQuery(GenCustomAddOnRuleSchema.XR_RuleType, string.Empty);  // CW1 rules have this as empty whilst GLOW rules do not.
		}
	}
}
