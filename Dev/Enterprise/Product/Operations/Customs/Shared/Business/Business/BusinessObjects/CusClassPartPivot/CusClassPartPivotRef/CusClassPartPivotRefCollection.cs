using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public class CusClassPartPivotRefCollection : DependentBusinessObjectCollection<CusClassPartPivotRef, BaseCusClassPartPivot>
	{
		public CusClassPartPivotRefCollection(BaseCusClassPartPivot parent)
				: base(parent)
		{
		}

		public CusClassPartPivotRef AddNew(ZString type, ZString number)
		{
			var result = AddNew();
			result.CIR_ReferenceType = type;
			result.CIR_ReferenceNumber = number;
			return result;
		}

		protected override bool AllowNewCore
		{
			get
			{
				var master = Master;
				return master.SupportCusClassPartPivotRef;
			}
		}
	}
}
