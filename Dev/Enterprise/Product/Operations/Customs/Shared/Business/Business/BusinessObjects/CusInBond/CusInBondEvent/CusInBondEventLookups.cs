using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class CusInBondEventLookups : AutoCusInBondEventLookups
	{
		public CusInBondEventLookups(AutoCusInBondEvent parent)
			: base(parent)
		{
		}

		public new CusInBondEvent Parent => (CusInBondEvent)base.Parent;

		public virtual CodeDescriptionPairList IncidentCodeList => Factory.GetCachedValue<CusInBondEventIncidentCodeList>();
	}
}
