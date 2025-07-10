using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public partial class JobDeclarationLookups
	{
		public RefCountryCollection CountryList => Factory.GetCachedValue("TRAddInfoLookups.RefCountryCollection", delegate
		{
			return new RefCountryCollection(Factory);
		});

		public ZZRefCusCodeListCombinedCollection BankCodeList
			=> ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TRDeclarationBankCode, Declaration.DateOfValuation);

		public CodeDescriptionPairList TradeTypeList => Factory.GetCachedValue<TradeTypeList>();
	}
}
