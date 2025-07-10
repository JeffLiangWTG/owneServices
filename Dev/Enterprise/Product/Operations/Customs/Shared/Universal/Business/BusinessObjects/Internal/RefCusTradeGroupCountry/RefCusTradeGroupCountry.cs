using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Universal
{
	public sealed class RefCusTradeGroupCountry : AutoRefCusTradeGroupCountry
	{
		public RefCusTradeGroupCountry(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[RelatedBusinessObject("CusTradeGroup")]
		public override ZGuid ZZB_ZZA_TradeGroup
		{
			get { return base.ZZB_ZZA_TradeGroup; }
			set { base.ZZB_ZZA_TradeGroup = value; }
		}

		public RefCusTradeGroup CusTradeGroup
		{
			get { return Factory.Load<RefCusTradeGroup>(ZZB_ZZA_TradeGroup); }
		}

#if DEBUG
		protected override BusinessObjectTestDataHelper NewBusinessObjectTestDataHelper()
		{
			return new UniversalReferenceBOTestDataHelper();
		}

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			if (ZZB_StartDate >= ZZB_EndDate)
			{
				var swapHolder = ZZB_StartDate;
				ZZB_StartDate = ZZB_EndDate;
				ZZB_EndDate = swapHolder;
			}
		}
#endif
	}
}
