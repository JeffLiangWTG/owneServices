using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.AMS.Business
{
	public class CusInBondContainerLookups : Customs.Business.CusInBondContainerLookups
	{
		public CusInBondContainerLookups(CusInBondContainer parent)
			: base(parent)
		{
		}

		public RefContainerCollection ContainerTypes
		{
			get { return new RefContainerCollection(Factory); }
		}

		public ServiceTypeList ServiceTypes
		{
			get { return Factory.GetCachedValue<ServiceTypeList>(); }
		}

		public ZZRefCusCodeListCombinedCollection ScheduleKList
		{
			get { return ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, ZDateTime.Today); }
		}
	}
}
