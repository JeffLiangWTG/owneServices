using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.HRM.Common
{
	public class HrlUnpaidExclude : AutoHrlUnpaidExclude
	{
		public HrlUnpaidExclude(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			LPX_LPB_Benefit = Factory.NewWithValidTestData<HrlBenefit>().PK;
		}
#endif
	}
}
