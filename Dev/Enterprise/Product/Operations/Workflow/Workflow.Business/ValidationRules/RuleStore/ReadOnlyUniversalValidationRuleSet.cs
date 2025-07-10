using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Workflow.Business
{
	class ReadOnlyUniversalValidationRuleSet : IReadOnlyUniversalValidationRuleSet
	{
		/// <summary>
		/// Construct from business objects.
		/// </summary>
		/// <param name="bizRuleSet">Not null</param>
		/// <param name="activeRules">Not null. Caller is responsible for ensuring rules are active</param>
		public ReadOnlyUniversalValidationRuleSet(UniversalValidationRuleSet bizRuleSet, IEnumerable<UniversalValidationRule> activeRules)
		{
			Argument.NotNull(bizRuleSet, nameof(bizRuleSet));
			Argument.NotNull(activeRules, nameof(activeRules));

			AlwaysApply = bizRuleSet.VRS_AlwaysApply;
			Code = bizRuleSet.VRS_Code;
			Criteria = bizRuleSet.VRS_Criteria;
			DataContext = bizRuleSet.VRS_DataContext;
			Description = bizRuleSet.VRS_Description;
			IsActive = bizRuleSet.VRS_IsActive;
			Name = bizRuleSet.VRS_Name;

			ActiveRules = activeRules
				.Select(bizRule => new ReadOnlyUniversalValidationRule(bizRule))
				.ToList();
		}

		public ZBool AlwaysApply { get; }
		public ZString Code { get; }
		public ZString Criteria { get; }
		public ZString DataContext { get; }
		public ZString Description { get; }
		public ZBool IsActive { get; }
		public ZString Name { get; }
		public IEnumerable<IReadOnlyUniversalValidationRule> ActiveRules { get; }
	}
}
