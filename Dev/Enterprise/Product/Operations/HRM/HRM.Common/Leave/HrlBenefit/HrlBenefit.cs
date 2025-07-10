using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.HRM.Common
{
	public class HrlBenefit : AutoHrlBenefit
	{
		public HrlBenefit(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			LPB_LPE_Entitlement = Factory.NewWithValidTestData<HrlEntitlement>().PK;
			LPB_LPI_Increment = Factory.NewWithValidTestData<HrlIncrement>().PK;
		}
#endif
	}
}
