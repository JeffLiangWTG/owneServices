using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.InBond.Business
{
	public class MovementHeaderWrapperLookups : ZLookups
	{
		public MovementHeaderWrapperLookups(BusinessObject parent) : base(parent)
		{
		}

		public InbondCommonTypeList EntryTypeList => Factory.GetCachedValue<InbondCommonTypeList>();

		public ShippingProviderCollection Carriers
		{
			get
			{
				return new ShippingProviderCollection(Factory);
			}
		}

		public ZZRefCusCodeListCombinedCollection RegionDistrictPorts
		{
			get { return ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today); }
		}
	}
}
