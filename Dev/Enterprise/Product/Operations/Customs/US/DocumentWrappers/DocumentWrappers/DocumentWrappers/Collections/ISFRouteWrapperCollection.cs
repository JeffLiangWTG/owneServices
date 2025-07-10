using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.DocumentWrappers.GenericWrappers;

namespace Enterprise.Customs.US.DocumentWrappers
{
	public class ISFRouteWrapperCollection : RouteWrapperCollection
	{
		public ISFRouteWrapperCollection(CusISFHeader headerBO, BusinessObjectFactory factory)
			: base(factory)
		{
			if (headerBO != null)
			{
				LoadRoutings(headerBO, (l) => l.JW_TransportType == Constants.TransportPlanningType.MainVessel);
			}
		}
	}
}
