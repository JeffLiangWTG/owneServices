using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.NCTS.Business
{
	public class SPTSDepartureMovementHeaderLookups : CusInBondMoveHeaderLookups
	{
		public SPTSDepartureMovementHeaderLookups(SPTSDepartureMovementHeader parent) : base(parent)
		{
		}

		public CodeDescriptionPairList TransitStatusList => Factory.GetCachedValue<SPTSTransitStatusList>();

		public CodeDescriptionPairList TransportModeList => Factory.GetCachedValue<SPTSTransportModeList>();

		public ShippingProviderCollection ShippingProviders => new ShippingProviderCollection(Factory);

		public CodeDescriptionPairList CustomsOfficeList => AsycudaUniversalReference.RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice);
	}
}
