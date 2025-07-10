using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class AccCommissionRuleStaffDisable : AutoAccCommissionRuleStaffDisable
	{
		public AccCommissionRuleStaffDisable(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Test
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			ParentRule.ACM_GG = Factory.NewWithValidTestData<GlbGroup>().PK;
		}

#endif
		#endregion
	}
}
