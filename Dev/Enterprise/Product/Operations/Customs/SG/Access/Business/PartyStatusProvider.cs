using CargoWise.EntityFramework;
using CargoWise.Integration;

namespace Enterprise.Customs.SG.Access.Business
{
	public class PartyStatusProvider : Integration.Customs.ASYCUDA.SGAccess.IAsycudaPartyStatusProvider
	{
		public ICodeDescriptionPairList GetPartyStatusCodeList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue<SGPartyStatusList>();
		}
	}
}



