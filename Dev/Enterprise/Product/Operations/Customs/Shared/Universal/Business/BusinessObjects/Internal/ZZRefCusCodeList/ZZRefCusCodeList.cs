using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.Universal.Internal
{
	[CodeAlive("Under Development")]
	public class ZZRefCusCodeList : AutoZZRefCusCodeList
	{
		public ZZRefCusCodeList(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

#if DEBUG
		protected override BusinessObjectTestDataHelper NewBusinessObjectTestDataHelper()
		{
			return new UniversalReferenceBOTestDataHelper();
		}

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			if (ZZD_StartDate >= ZZD_EndDate)
			{
				var swapHolder = ZZD_StartDate;
				ZZD_StartDate = ZZD_EndDate;
				ZZD_EndDate = swapHolder;
			}
		}
#endif
	}
}
