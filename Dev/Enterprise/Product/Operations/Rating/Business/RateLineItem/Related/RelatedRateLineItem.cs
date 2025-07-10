using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Rating.Business
{
	public class RelatedRateLineItem : RateLineItem
	{
		public RelatedRateLineItem(BusinessObjectFactory factory, System.Data.DataRow row)
			: base(factory, row)
		{
		}

		public override bool IsSavedByFactory
		{
			get { return false; }
		}

		public override RateLine Parent
		{
			get { return Factory.Load<RelatedRateLine>(TM_TL); }
		}

		#region Properties

		#region TM_Value

		public override ZDecimal TM_Value
		{
			get
			{
				if (Parent != null && Parent.IsTariffLineInherited)
				{
					return ((ZDecimal)TM_ValueInfo.OriginalValue) * (1 - Parent.CompanyTariffDiscount / 100);
				}
				else
				{
					return (ZDecimal)TM_ValueInfo.OriginalValue;
				}
			}
			set { base.TM_Value = value; }
		}

		#endregion

		#endregion
	}
}

