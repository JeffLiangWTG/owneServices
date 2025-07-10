using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using RefCusCodeList = Enterprise.Core.Constants.Customs.Universal.RefCusCodeList;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;
using RefDataGrouping = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping;

namespace Enterprise.Customs.ZA.Business
{
	public class AsycudaPackLookups : ManifestBase.AsycudaPackLookups
	{
		public AsycudaPackLookups(AsycudaPack parent)
			: base(parent)
		{
		}

		protected new AsycudaPack Parent => (AsycudaPack)base.Parent;

		public override CodeDescriptionPairList PackUQList
		{
			get
			{
				var header = Parent.Bill?.Header;
				return header != null && (header.IsBBK || header.IsBLK)
					? Universal.RefCusCodeListTypes.GetCachedListMatchAnyAttributes(Factory, RefDataGrouping.Codes.UnitedNationsRecommendations, RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, ZDateTime.Today
						, new[]
						{
							new KeyValuePair<ZString, ZString>(RefCusCodeList.Attributes.Bulk, RefCusCodeList.AttributeValues.Bulk),
							new KeyValuePair<ZString, ZString>(RefCusCodeList.Attributes.BreakBulk, RefCusCodeList.AttributeValues.BreakBulk),
						})
					: Universal.AsycudaUniversalReference.RefCusCodeListTypes.GetCachedList(Factory, RefDataGrouping.Codes.UnitedNationsRecommendations, RefCusCodeListTypes.Codes.UnitedNationsPackageTypes);
			}
		}
	}
}
