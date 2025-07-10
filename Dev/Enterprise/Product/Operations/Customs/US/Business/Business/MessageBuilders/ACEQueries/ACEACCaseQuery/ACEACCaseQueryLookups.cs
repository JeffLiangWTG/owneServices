using CargoWise.EntityFramework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class ACEACCaseQueryLookups : ZLookups
	{
		public ACEACCaseQueryLookups(ACEACCaseQuery parent)
			: base(parent) { }

		public CodeDescriptionPairList CompanyCaseStatusList
		{
			get
			{
				return Factory.GetCachedValue("CompanyCaseStatusList", delegate
					{
						var result = new CodeDescriptionPairList();
						result.AddPair(ACCaseStatusList.QueryMessageCodes.Active, ACCaseStatusList.QueryMessgeDescriptions.Active);
						result.AddPair(ACCaseStatusList.QueryMessageCodes.Inactive, ACCaseStatusList.QueryMessgeDescriptions.Inactive);
						result.AddPair(ACCaseStatusList.QueryMessageCodes.Both, ACCaseStatusList.QueryMessgeDescriptions.Both);
						return result;
					});
			}
		}

		public USCCountryCollection CountryList
		{
			get { return new USCCountryCollection(Factory); }
		}

		public USCTariffCollection TariffList
		{
			get { return new USCTariffCollection(Factory); }
		}
	}
}
