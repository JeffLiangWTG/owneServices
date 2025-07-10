using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.NCTS.Business
{
	public class NctsWarehouseToOpenLookups : CusSupportingInfoLookups
	{
		public NctsWarehouseToOpenLookups(AutoCusSupportingInfo parent) : base(parent)
		{
		}

		protected new NctsWarehouseToOpen Parent => (NctsWarehouseToOpen)base.Parent;

		public CodeDescriptionPairList IncotermList => Factory.GetCachedValue<IncotermCodeList>();

		public override CodeDescriptionPairList SubTypeList => Factory.GetCachedValue<PaymentTypeList>();

		public CodeDescriptionPairList PackagesTypeList => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, ZDateTime.Today);

		public CodeDescriptionPairList NatureOfBusinessList => AsycudaUniversalReference.RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Turkey, NatureOfBusinessType);

		const string NatureOfBusinessType = "TRNOB";
	}
}
