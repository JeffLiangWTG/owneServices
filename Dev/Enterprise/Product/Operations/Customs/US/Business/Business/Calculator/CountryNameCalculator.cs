using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	/// <summary>
	/// Country calculator added for obtaining Country name from USCForeignPort 
	/// and USCRegionDistrictPort reference files using SchD Port Code
	/// USCForeignPort UH_Name field contains Country name
	/// UH_Name field has different Custom formats
	/// </summary>
	public static class CountryNameCalculator
	{
		public static ZString CalculateCountryNameFrom(ZString unloco, ZString scheduleDCode, BusinessObjectFactory factory)
		{
			ZString result;

			ZString countryCode = ZString.Empty;

			if (unloco.IsEmpty)
			{
				ZQuery query = new ZQuery(RefLocoMapSchema.RY_LocalPortCode, scheduleDCode);
				query.AddToFilter(RefLocoMapSchema.RY_RN, Core.Constants.CountryGuids.UnitedStates);

				RefLocoMap unlocoPortMapping = factory.LoadTop1<RefLocoMap>(query);
				if (unlocoPortMapping != null)
				{
					countryCode = unlocoPortMapping.RY_RL_NKLocoPort.Left(2);
				}
			}
			else
			{
				countryCode = unloco.Left(2);
			}

			if (!countryCode.IsEmpty)
			{
				RefCountry country = factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, countryCode);
				result = country == null ? ZString.Empty : country.RN_DescMultilingual;
			}
			else
			{
				ZString countryName = ZString.Empty;

				IForeignRegionalDistrictPort regionalOrForeignPort = GetForeignOrRegionalPort(scheduleDCode, factory);
				if (regionalOrForeignPort != null && regionalOrForeignPort.PortName.Contains(","))
				{
					countryName = regionalOrForeignPort.PortName.Substring(regionalOrForeignPort.PortName.IndexOf(",") + 1).Trim();
				}

				result = countryName;
			}

			return result;
		}

		public static IForeignRegionalDistrictPort GetForeignOrRegionalPort(ZString scheduleCode, BusinessObjectFactory factory)
		{
			IForeignRegionalDistrictPort result = null;
			if (scheduleCode.Length == 4)
			{
				result = new USCForeignPortWrapper(ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(factory, scheduleCode, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today));
			}
			else if (scheduleCode.Length == 5)
			{
				result = GetForeignPort(scheduleCode, USCForeignPortWrapper.Type.Common, factory);
			}
			return result;
		}

		public static IForeignRegionalDistrictPort GetForeignPort(ZString scheduleCode, USCForeignPortWrapper.Type type, BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("CountryNameCalculator|" + scheduleCode + "|" + type, () =>
			{
				var foreignPortTypes = new List<ZString>() { ForeignPortTypeList.Codes.Common };
				if (type != USCForeignPortWrapper.Type.Common)
				{
					foreignPortTypes.Add(ForeignPortTypeList.GetCodeFromType(type));
				}
				var attributeFilters = new List<RefCusCodeListAttributeFilter>() { new RefCusCodeListAttributeFilter(RefCusCodeListAttributeTypes.Codes.PortValidType, SQLComparisonOperator.Equal, foreignPortTypes.ToArray()) };
				var port = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountryAndAttributes(factory, scheduleCode, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, ZDateTime.Today, attributeFilters: attributeFilters);
				return new USCForeignPortWrapper(port);
			});
		}
	}
}
