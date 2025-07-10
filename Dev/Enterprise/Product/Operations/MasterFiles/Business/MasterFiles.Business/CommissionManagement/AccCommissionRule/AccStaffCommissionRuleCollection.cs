using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccStaffCommissionRuleCollection : ActiveBusinessObjectCollection<AccStaffCommissionRule>
	{
		readonly GlbStaff staff;
		public AccStaffCommissionRuleCollection(GlbStaff staff)
			: base(staff.Factory, staff, new ZQuery(), AccCommissionRuleSchema.ACM_GS_NKStaff)
		{
			this.staff = staff;
		}

		protected override void SetDefaultsForNewElementCore(AccStaffCommissionRule newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.ACM_GS_NKStaff = staff.GS_Code;
		}
	}
}
