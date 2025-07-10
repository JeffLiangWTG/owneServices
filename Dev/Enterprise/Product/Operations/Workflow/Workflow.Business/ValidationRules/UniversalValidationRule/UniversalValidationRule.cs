using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Workflow.Business
{
	[DependentBusinessObject(typeof(UniversalValidationRuleSet), nameof(UniversalValidationRuleSet.Rules))]
	public class UniversalValidationRule : AutoUniversalValidationRule
	{
		public UniversalValidationRule(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[List(nameof(Lookups) + "." + nameof(UniversalValidationRuleLookups.StatusList))]
		public override ZString VR_Status { get => base.VR_Status; set => base.VR_Status = value; }

		public ZBool IsError => VR_Status == UniversalValidationRuleLookups.StatusCodes.Error;
	}
}
