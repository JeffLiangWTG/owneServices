using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.Application;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Module
{
	public class JobDeclarationFilterBusinessObjectFactory
	{
		public JobDeclarationFilterBusinessObject GetJobDeclarationFilterBusinessObjectForCountry()
		{
			return (JobDeclarationFilterBusinessObject)System.Activator.CreateInstance(GetCountrySpecificType(GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
		}

		protected Type GetCountrySpecificType(string countryCode)
		{
			Type result;
			if (CountrySpecificType.ContainsKey(countryCode))
			{
				result = CountrySpecificType[countryCode];
			}
			else
			{
				var customsCountryCode = Enterprise.Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(countryCode);
				result = customsCountryCode != countryCode ? GetCountrySpecificType(customsCountryCode) : typeof(JobDeclarationFilterBusinessObject);
			}
			return result;
		}

		Dictionary<string, Type> CountrySpecificType
		{
			get
			{
				if (fCountrySpecificTypes == null)
				{
					fCountrySpecificTypes = new Dictionary<string, Type>();
					fCountrySpecificTypes.Add(Core.Constants.CountryCodes.Australia, ObjectFactory.GetType<Integration.Customs.AU.IJobDeclarationFilterBusinessObject>());
					fCountrySpecificTypes.Add(Core.Constants.CountryCodes.Canada, ObjectFactory.GetType<Integration.Customs.CA.IJobDeclarationFilterBusinessObject>());
					fCountrySpecificTypes.Add(Core.Constants.CountryCodes.Malaysia, ObjectFactory.GetType<Integration.Customs.MY.IJobDeclarationFilterBusinessObject>());
					fCountrySpecificTypes.Add(Core.Constants.CountryCodes.NewZealand, ObjectFactory.GetType<Integration.Customs.NZ.IJobDeclarationFilterBusinessObject>());
					fCountrySpecificTypes.Add(Core.Constants.CountryCodes.Singapore, ObjectFactory.GetType<Integration.Customs.SG.IJobDeclarationFilterBusinessObject>());
					fCountrySpecificTypes.Add(Core.Constants.CountryCodes.SouthAfrica, ObjectFactory.GetType<Integration.Customs.ZA.IJobDeclarationFilterBusinessObject>());
					fCountrySpecificTypes.Add(Core.Constants.CountryCodes.UnitedArabEmirates, ObjectFactory.GetType<Integration.Customs.AE.IJobDeclarationFilterBusinessObject>());
					fCountrySpecificTypes.Add(Core.Constants.CountryCodes.UnitedStates, ObjectFactory.GetType<Integration.Customs.US.IJobDeclarationFilterBusinessObject>());
					foreach (var country in ObjectFactory.Get<Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>().GetEuropeanUnionForCustomsMembersAndCustomsUnionAdditionalMembers())
					{
						fCountrySpecificTypes.Add(country, fEuCountrySpecificTypes.GetValueOrDefault(country) ?? ObjectFactory.GetType<Integration.Customs.EU.IJobDeclarationFilterBusinessObject>());
					}
				}
				return fCountrySpecificTypes;
			}
		}
		Dictionary<string, Type> fCountrySpecificTypes;
		readonly ImmutableDictionary<string, Type> fEuCountrySpecificTypes = new Dictionary<string, Type>()
		{
			{ Core.Constants.CountryCodes.France, ObjectFactory.GetType<Integration.Customs.FR.IJobDeclarationFilterBusinessObject>() }
		}.ToImmutableDictionary();
	}
}
