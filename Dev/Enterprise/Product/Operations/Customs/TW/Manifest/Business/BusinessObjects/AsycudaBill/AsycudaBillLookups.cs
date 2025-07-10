using System.Collections;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Manifest.Business
{
	public class AsycudaBillLookups : ASYCUDA.Business.AsycudaBillLookups
	{
		public AsycudaBillLookups(AsycudaBill parent)
			: base(parent)
		{
		}

		AsycudaBill Bill => (AsycudaBill)Parent;

		public override RefUNLOCOCollection FinalDestinations
		{
			get
			{
				var result = new RefUNLOCOCollection(Factory);
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("CountryState", "Property1", new ZString(Enterprise.Core.Constants.CountryCodes.Taiwan)));
				return result;
			}
		}

		public override ICollection Locations
		{
			get
			{
				var result = ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, ZDateTime.Today);
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.AttributeName, "Property", new ZString(RefCusCodeListAttributeTypes.Codes.CustomsOffice)));
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.AttributeValue, "Property", Bill.Header.AMA_CustomsOffice));
				return result;
			}
		}

		public CodeDescriptionPairList BagNumberList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				Bill.Header?.Bills.Cast<AsycudaBill>().Select(x => x.BagNumber).Where(x => !x.IsEmpty).Distinct().ForEach(x => result.AddPair(x));
				return result;
			}
		}

		public CodeDescriptionPairList TWShipmentTypes => Factory.GetCachedValue<TWManifestShipmentTypes>();

		public override CodeDescriptionPairList MessageStatusList => Factory.GetCachedValue<TWMessageStatusCodeList>();

		IRefCusPackListProvider CachedCusPackListProvider => Factory.GetCachedValue<TW.Business.RefCusPackListProvider>();

		protected override CodeDescriptionPairList PackageTypeListCore => CachedCusPackListProvider.GetCommercialPackList(Factory, ZString.Empty);

		public ChildTariffViewCollection TariffCollection => ChildTariffViewCollection.GetNewCollection(Factory, Core.Constants.CountryCodes.Taiwan, Universal.Constants.TariffTypes.HarmonizedSystem, ZDateTime.Today, null);

		public UNDGSubstanceCollection UNDGSubstances => new UNDGSubstanceCollection(Factory);
	}
}
