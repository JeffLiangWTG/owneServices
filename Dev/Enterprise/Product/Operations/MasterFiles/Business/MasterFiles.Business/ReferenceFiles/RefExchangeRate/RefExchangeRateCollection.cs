using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business
{
	public class RefExchangeRateCollection : ActiveBusinessObjectCollection<RefExchangeRate>, IBusinessObjectLoader
	{
		public RefExchangeRateCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public RefExchangeRateCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		BusinessObject IBusinessObjectLoader.Load(BusinessObjectFactory factory, ZGuid pk)
		{
			return factory.LoadTop1<RefExchangeRate>(RefExchangeRate.Loader.GetFilterByPK(pk));
		}
	}
}
