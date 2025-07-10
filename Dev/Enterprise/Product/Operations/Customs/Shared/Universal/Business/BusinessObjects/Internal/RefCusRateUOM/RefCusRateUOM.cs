using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Universal
{
	internal class RefCusRateUOM : AutoRefCusRateUOM
	{
		public RefCusRateUOM(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[RelatedBusinessObject("CusRate")]
		public override ZGuid ZXG_ZZ2_Rate
		{
			get => base.ZXG_ZZ2_Rate;
			set => base.ZXG_ZZ2_Rate = value;
		}

		public RateView CusRate => Factory.Load<RateView>(ZXG_ZZ2_Rate);
	}
}
