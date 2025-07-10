using System.Collections;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.Manifest.Business
{
	public class AsycudaBillLookups : ASYCUDA.Business.AsycudaBillLookups
	{
		public AsycudaBillLookups(AsycudaBill parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList PaymentTypeList => Factory.GetCachedValue("TRAsycudaBillLookups.PaymentTypeList", () => new PaymentTypeList());

		public CodeDescriptionPairList TransshipmentTypeList => Factory.GetCachedValue("TRAsycudaBillLookups.TransshipmentTypeList", () => new TransshipmentTypeList());

		public override ICollection Locations => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TurkeyWarehouseCodes, ZDateTime.Today);

		public CodeDescriptionPairList YesNoList => Factory.GetCachedValue<Universal.CodeDescriptionPairLists.YesNoList>();

		public CodeDescriptionPairList GoodsLocationList
		{
			get
			{
				return Factory.GetCachedValue("TRAsycudaBillLookups.GoodsLocationList", () =>
				{
					var result = new CodeDescriptionPairList();
					var goodsLocations = Locations as ZZRefCusCodeListCombinedCollection;
					goodsLocations.Load();
					foreach (ZZRefCusCodeListCombined codeList in goodsLocations)
					{
						result.AddPairIfNotExist(codeList.ZZD_Code, codeList.ZZD_Description);
					}
					return result;
				});
			}
		}
	}
}
