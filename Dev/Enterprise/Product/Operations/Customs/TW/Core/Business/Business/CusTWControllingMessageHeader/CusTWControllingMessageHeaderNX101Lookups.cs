using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.Customs.TW.Business
{
	public class CusTWControllingMessageHeaderNX101Lookups : CusTWControllingMessageHeaderLookups
	{
		public CusTWControllingMessageHeaderNX101Lookups(AutoCusTWControllingMessageHeader parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList ProcessingUnitList => TWRefCusCodeListTypes.GetProcessingUnitList(Factory, Parent.TW1_CertificateType, Parent.EntryInstruction?.DateOfValuation ?? ZDateTime.Today, true);

		public override CodeDescriptionPairList CertificateTypeList => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Taiwan, Codes.TaiwanCertificateOfOriginType, ZDateTime.Today);
	}
}
