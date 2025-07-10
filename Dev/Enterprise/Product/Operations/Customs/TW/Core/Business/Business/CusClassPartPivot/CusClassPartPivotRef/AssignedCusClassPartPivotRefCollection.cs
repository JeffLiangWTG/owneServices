using CargoWise.EntityFramework;
using Enterprise.Customs.Common.TW;

namespace Enterprise.Customs.TW.Business
{
	public class AssignedCusClassPartPivotRefCollection : CusClassPartPivotRefCollection<AssignedCusClassPartPivotRef>
	{
		public AssignedCusClassPartPivotRefCollection(BusinessObject parent)
				: base(parent, JobComInvLineRefsType.Codes.AssignedNumber)
		{
		}
	}
}
