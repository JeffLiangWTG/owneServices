using System.Collections;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business
{
	public class AsycudaBillLookups : ASYCUDA.Business.AsycudaBillLookups
	{
		public AsycudaBillLookups(AsycudaBill parent)
			: base(parent)
		{
		}

		AsycudaBill Bill => (AsycudaBill)Parent;

		public override ICollection Locations
		{
			get
			{
				var header = Parent?.Header;
				var effectiveDateForDutyRate = header?.ApplicationBusinessProvider.GetEffectiveDateForDutyRate(header) ?? ZDateTime.Today;
				var result = ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, effectiveDateForDutyRate);
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.AttributeName, "Property", new ZString(RefCusCodeListAttributeTypes.Codes.CustomsOffice)));
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.AttributeValue, "Property", Bill.Header.AMA_CustomsOffice));
				return result;
			}
		}

		public override ICodeDescriptionPairList Procedures => Factory.GetCachedValue<ProcedureList>();

		public override CodeDescriptionPairList IncotermList => Factory.GetCachedValue<BriefCustomsDeclarationIncotermList>();

		public CodeDescriptionPairList ShipperBondedIDTypeList => Factory.GetCachedValue("TWShipperBondedIDTypeList", () =>
		{
			var result = new CodeDescriptionPairList();
			OrgHeaderHelper.BCDExporterBondedIDCodeTypes.ForEach(x => result.AddPair(x));
			return result;
		});

		protected override CodeDescriptionPairList PackageTypeListCore => CachedCusPackListProvider.GetCIPCustomsPackList(Factory, string.Empty);

		IRefCusPackListProvider CachedCusPackListProvider => Factory.GetCachedValue<RefCusPackListProvider>();
	}
}
