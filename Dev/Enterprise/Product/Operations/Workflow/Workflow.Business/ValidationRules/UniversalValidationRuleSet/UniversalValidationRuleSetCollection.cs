using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Workflow.Business
{
	[ModuleID(ModuleId.UniversalValidationRule)]
	public class UniversalValidationRuleSetCollection : ActiveBusinessObjectCollection<UniversalValidationRuleSet>
	{
		public UniversalValidationRuleSetCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
