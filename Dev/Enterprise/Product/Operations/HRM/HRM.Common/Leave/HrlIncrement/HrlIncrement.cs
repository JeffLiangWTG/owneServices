using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.HRM.Common
{
	public class HrlIncrement : AutoHrlIncrement
	{
		public HrlIncrement(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			LPI_LeaveYearLength = 1;
			LPI_LeaveYearLengthTimeUnit = "D";
		}
#endif
	}
}
