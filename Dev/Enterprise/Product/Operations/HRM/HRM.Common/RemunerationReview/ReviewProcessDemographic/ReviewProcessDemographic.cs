using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.HRM.Common
{
	public class ReviewProcessDemographic : AutoReviewProcessDemographic
	{
		public ReviewProcessDemographic(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			RPD_Priority = 1;
		}
#endif
	}
}
