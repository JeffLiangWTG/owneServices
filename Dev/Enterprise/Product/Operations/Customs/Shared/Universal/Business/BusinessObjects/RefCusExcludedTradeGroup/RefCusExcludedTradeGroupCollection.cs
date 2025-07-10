using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public class RefCusExcludedTradeGroupCollection : ActiveBusinessObjectCollection<RefCusExcludedTradeGroup>
	{
		public RefCusExcludedTradeGroupCollection(CusRefApplicabilityView master)
			: base(master.Factory, master, new ZQuery(), RefCusExcludedTradeGroupSchema.ZZC_ZZT_Applicability)
		{ }
	}
}
