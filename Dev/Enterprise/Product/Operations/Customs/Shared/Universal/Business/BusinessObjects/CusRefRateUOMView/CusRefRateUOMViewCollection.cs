using CargoWise.EntityFramework;
using Enterprise.Customs.Universal.Internal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public class CusRefRateUOMViewCollection : ActiveBusinessObjectCollection<CusRefRateUOMView>
	{
		public CusRefRateUOMViewCollection(RateView master)
			: base(master.Factory, master, new ZQuery(), CusRefRateUOMViewSchema.ZXG_ZZ2_Rate)
		{
		}

		public CusRefRateUOMViewCollection(RefCusRate master)
			: base(master.Factory, master, new ZQuery(), CusRefRateUOMViewSchema.ZXG_ZZ2_Rate)
		{
		}
	}
}
