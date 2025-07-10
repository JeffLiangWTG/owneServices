using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public class RefCusConditionValueCollection : ActiveBusinessObjectCollection<RefCusConditionValue>
	{
		public RefCusConditionValueCollection(RefCusCondition parent)
			: base(parent.Factory, parent, new ZQuery(), RefCusConditionValueSchema.ZX3_ZX1_Condition)
		{
		}
	}
}
