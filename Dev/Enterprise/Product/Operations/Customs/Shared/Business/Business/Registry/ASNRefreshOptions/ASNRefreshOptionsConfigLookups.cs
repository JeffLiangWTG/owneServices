using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DataRegistry.Business
{
	public class ASNRefreshOptionsConfigLookups : ZLookups
	{
		public ASNRefreshOptionsConfigLookups(ASNRefreshOptionsConfig parent, BusinessObjectFactory factory)
			: base(parent)
		{
			this.factory = Argument.NotNull(factory, "Factory");
		}

		readonly BusinessObjectFactory factory;

		public CodeDescriptionPairList FieldTypeList
		{
			get
			{
				return factory.GetCachedValue("ASNRefreshOptionsConfigLookups+FieldTypeList", () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(Constants.Customs.ASNRefreshDefaultsOptions.Codes.Classification, Constants.Customs.ASNRefreshDefaultsOptions.Descriptions.Classification);
					result.AddPair(Constants.Customs.ASNRefreshDefaultsOptions.Codes.CountryOfOrigin, Constants.Customs.ASNRefreshDefaultsOptions.Descriptions.CountryOfOrigin);
					result.AddPair(Constants.Customs.ASNRefreshDefaultsOptions.Codes.Preference, Constants.Customs.ASNRefreshDefaultsOptions.Descriptions.Preference);
					result.AddPair(Constants.Customs.ASNRefreshDefaultsOptions.Codes.Tariff, Constants.Customs.ASNRefreshDefaultsOptions.Descriptions.Tariff);
					return result;
				});
			}
		}
	}
}
