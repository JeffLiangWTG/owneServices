using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business
{
	public class AsycudaPackedItemLookups : ASYCUDA.Business.AsycudaPackedItemLookups
	{
		public AsycudaPackedItemLookups(ASYCUDA.Business.AsycudaPackedItem parent) : base(parent)
		{
		}

		public AsycudaPackedItem PackedItem => (AsycudaPackedItem)Parent;

		public new CodeDescriptionPairList CustomsUQList => TWRefCusCodeListTypes.GetCommercialPackUnitsList(Factory);

		public CodeDescriptionPairList PreferenceList => UniversalReferenceDataHelper.GetPreferenceList(Factory,
																									true,
																									PackedItem.API_Tariff,
																									PackedItem.API_RN_NKGoodsOrigin,
																									PackedItem.UniversalTariff,
																									new RateSelectionCriteria(PackedItem, ZString.Empty, ZString.Empty),
																									Core.Constants.CountryCodes.Taiwan);
	}
}
