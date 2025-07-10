using CargoWise.EntityFramework;
using Enterprise.Customs.Universal.Internal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	internal class RefCusRateUOMCollection : ActiveBusinessObjectCollection<RefCusRateUOM>
	{
		public RefCusRateUOMCollection(RateView master)
			: base(master.Factory, master, new ZQuery(), RefCusRateUOMSchema.ZXG_ZZ2_Rate)
		{
		}

		public RefCusRateUOMCollection(RefCusRate master)
			: base(master.Factory, master, new ZQuery(), RefCusRateUOMSchema.ZXG_ZZ2_Rate)
		{
		}
	}
}
