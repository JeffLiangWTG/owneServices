using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	public class JobServiceTypeProvider : Integration.Customs.TW.IJobServiceTypeProvider
	{
		public ICodeDescriptionPairList GetJobServiceTypes()
		{
			return new ServiceTypes();
		}

		ZBool Integration.Customs.IJobServiceTypeProvider.ServiceTypeNeedsToBeUnique(ZString serviceType)
		{
			switch (serviceType)
			{
				case ServiceTypes.CommodityInspection:
					return false;
				default:
					return true;
			}
		}
	}
}
