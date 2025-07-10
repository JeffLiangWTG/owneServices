using CargoWise.Integration;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.eManifest.Business
{
	public class CrewMemberLookups : CusInBondPersonLookups
	{
		public CrewMemberLookups(CrewMember parent)
			: base(parent)
		{
		}

		public ICodeDescriptionPairList CrewTypes
		{
			get { return Factory.GetCachedValue<CrewTypes>(); }
		}
	}
}
