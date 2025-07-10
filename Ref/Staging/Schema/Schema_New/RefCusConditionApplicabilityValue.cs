using System;
using System.ComponentModel.DataAnnotations.Schema;
using CargoWise.RefDbRepo.Common.TypeProvider;

namespace CargoWise.RefDbRepo.Staging.Schema_New
{
	[NotMapped]
	[NonPersistentObject]
	public class RefCusConditionApplicabilityValue
	{
		public RefCusConditionApplicabilityValue()
		{
			S08_PK = Guid.NewGuid();
		}

		public RefCusConditionApplicabilityValue(RefCusConditionValue val) : this()
		{
			S08_ZX1_Condition = val.ZX3_ZX1_Condition;
			S08_Value = val.ZX3_Value;
			S08_ZX4_NKValueType = val.ZX3_ZX4_NKValueType;
			S08_LogicalORWithinGroup = val.ZX3_LogicalORWithinGroup;
			S08_ZX4_ZZZ_NKDataGrouping = val.ZX3_ZX4_ZZZ_NKDataGrouping;
		}

		public Guid S08_PK { get; set; }
		public Guid S08_S07_ConditionApplicability { get; set; }
		public Guid S08_ZX1_Condition { get; set; }
		public string S08_Value { get; set; }
		public byte S08_LogicalORWithinGroup { get; set; }
		public string S08_ZX4_NKValueType { get; set; }
		public string S08_ZX4_ZZZ_NKDataGrouping { get; set; }

		public RefCusCondition RefCusCondition { get; set; }
	}
}
