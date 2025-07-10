using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.US.eManifest.Business
{
	public class InBondLookups : CusInBondMoveHeaderLookups
	{
		public InBondLookups(InBond parent)
			: base(parent)
		{
		}

		new InBond Parent
		{
			get { return (InBond)base.Parent; }
		}

		public ICodeDescriptionPairList InBondTypes
		{
			get { return Factory.GetCachedValue<InbondTypes>(); }
		}

		public ZZRefCusCodeListCombinedCollection ScheduleDPortCodes
		{
			get { return Parent.Trip.Lookups.ScheduleDPortCodes; }
		}

		public ZZRefCusCodeListCombinedCollection ScheduleKPortCodes
		{
			get { return Parent.Trip.Lookups.ScheduleKPortCodes; }
		}

		public IBusinessObjectCollection SCACCarrierCodes
		{
			get { return Parent.Trip.Lookups.SCACCarrierCodes; }
		}
	}
}
