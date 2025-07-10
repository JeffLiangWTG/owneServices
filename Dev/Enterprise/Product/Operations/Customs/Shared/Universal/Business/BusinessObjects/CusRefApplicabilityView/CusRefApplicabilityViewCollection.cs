using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public class CusRefApplicabilityViewCollection : ActiveBusinessObjectCollection<CusRefApplicabilityView>
	{
		public CusRefApplicabilityViewCollection(RateView master)
			: base(master.Factory, master, new ZQuery(), CusRefApplicabilityViewSchema.ZZT_ZZ2_Rate)
		{
		}

		public CusRefApplicabilityViewCollection(RefCusCondition master)
			: base(master.Factory, master, new ZQuery(), CusRefApplicabilityViewSchema.ZZT_ZX1_Conditions)
		{
		}

		public CusRefApplicabilityViewCollection(TariffAdditionalCodeView master)
			: base(master.Factory, master, new ZQuery(), CusRefApplicabilityViewSchema.ZZT_ZY2_AdditionalCode)
		{
		}
	}
}
