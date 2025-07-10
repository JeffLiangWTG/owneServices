using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class AccGroupCommissionRule : AccCommissionRule
	{
		public AccGroupCommissionRule(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override AccCommissionRuleValidation GetNewValidation()
		{
			return new AccGroupCommissionRuleValidation(this);
		}

		#region Test
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			if (ACM_GG.IsEmpty)
			{
				ACM_GG = Factory.NewWithValidTestData<GlbGroup>().PK;
			}

			base.FillWithValidTestDataCore(kind, propertyPath);
		}

#endif
		#endregion
	}
}
