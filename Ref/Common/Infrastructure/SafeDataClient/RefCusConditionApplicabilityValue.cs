using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.TypeProvider;

namespace CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New
{
	[NonPersistentObject]
	public class RefCusConditionApplicabilityValue : INonPersistentBusinessObject
	{
		public RefCusConditionApplicabilityValue(INonPersistentBusinessObjectFlatten topLevelNonPersistentObject)
		{
			S08_PK = Guid.NewGuid();
			TopLevelNonPersistentObjects.Add(topLevelNonPersistentObject);
		}

		public RefCusConditionApplicabilityValue(RefCusConditionValue val, INonPersistentBusinessObjectFlatten topLevelNonPersistentObject) : this(topLevelNonPersistentObject)
		{
			S08_Value = val.ZX3_Value;
			S08_ZX4_ValueType = val.ZX3_ZX4_ValueType;
			S08_LogicalORWithinGroup = val.ZX3_LogicalORWithinGroup;
			RefCusConditionValue = val;
			val.RefCusConditionApplicabilityValues.Add(this);
		}

		public void Link(RefCusConditionValue val)
		{
			RefCusConditionValue = val;
			if (!val.RefCusConditionApplicabilityValues.Contains(this))
			{
				val.RefCusConditionApplicabilityValues.Add(this);
			}
		}

		public void Update()
		{
			var val = RefCusConditionValue;
			val.ZX3_Value = S08_Value;
			val.ZX3_ZX4_ValueType = S08_ZX4_ValueType;
			val.ZX3_LogicalORWithinGroup = S08_LogicalORWithinGroup;
		}

		public IEnumerable<object> Unlink()
		{
			var val = RefCusConditionValue;
			val?.RefCusConditionApplicabilityValues.Remove(this);
			RefCusConditionValue = null;
			return new[] { val };
		}

		public ICollection<INonPersistentBusinessObjectFlatten> TopLevelNonPersistentObjects { get; } = new HashSet<INonPersistentBusinessObjectFlatten>();

		public Guid S08_PK { get; set; }
		public Guid? S08_ZX4_ValueType { get; set; }
		public Guid S08_S07_ConditionApplicability { get; set; }
		public string S08_Value { get; set; }
		public byte S08_LogicalORWithinGroup { get; set; }
		public virtual RefCusConditionValue RefCusConditionValue { get; set; }
	}
}
