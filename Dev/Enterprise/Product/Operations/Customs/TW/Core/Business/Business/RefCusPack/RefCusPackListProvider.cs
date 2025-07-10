using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Integration.Customs.TW;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public sealed class RefCusPackListProvider : MasterFiles.Business.RefCusPackListProvider, IRefCusPackListProvider
	{
		public override CodeDescriptionPairList GetCustomsPackList(BusinessObjectFactory factory, ZString type, ZString country) => type == RPTypeList.Codes.PermitQuantityUnits
				? TWRefCusCodeListTypes.GetCommercialPackUnitsList(factory)
				: base.GetCustomsPackList(factory, type, country);

		public override CodeDescriptionPairList GetCIPCustomsPackList(BusinessObjectFactory factory, ZString country) => TWRefCusCodeListTypes.GetCustomsPackUnitsList(factory);

		public override CodeDescriptionPairList GetCommercialPackList(BusinessObjectFactory factory, ZString type) => type == RPTypeList.Codes.PermitQuantityUnits
				? RefCusCodeListTypes.GetCachedList(factory, Core.Constants.CountryCodes.China, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, ZDateTime.Today, languageCode: Core.Constants.Customs.Universal.RefLanguageType.Codes.ChineseTraditional, onlyThisLanguage: true)
				: TWRefCusCodeListTypes.GetCommercialPackUnitsList(factory);

		public static MasterFiles.Integration.IRefCusPackListProvider GetCachedCusPackListProvider(BusinessObjectFactory factory) => Loader.GetRefCusPackListProvider(factory, Core.Constants.CountryCodes.Taiwan);

		public override CodeDescriptionPairList GetDeclarationPackTypeList(BusinessObjectFactory factory) => factory.GetNull<JobDeclaration>().Lookups.PackingUnitTypesList;

		public override CodeDescriptionPairList GetPackConversionTypeList(BusinessObjectFactory factory) => factory.GetCachedValue<RPTypeList>();
	}
}
