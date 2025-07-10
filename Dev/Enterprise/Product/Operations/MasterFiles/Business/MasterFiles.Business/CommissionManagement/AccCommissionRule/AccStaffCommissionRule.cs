using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class AccStaffCommissionRule : AccCommissionRule
	{
		public AccStaffCommissionRule(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Test
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			if (ACM_GS_NKStaff.IsEmpty)
			{
				ACM_GS_NKStaff = Factory.NewWithValidTestData<GlbStaff>().GS_Code;
			}

			base.FillWithValidTestDataCore(kind, propertyPath);
		}

#endif
		#endregion
	}
}
