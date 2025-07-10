using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.Business
{
	public interface ICusMAWBTypeDecider
	{
		Type DefaultType { get; }
	}

	public class CusMAWBTypeDecider : TypeDecider, ICusMAWBTypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			Type result;
			string applicationCode = row[CusMAWB.Schema.CM_ApplicationCode].ToString();

			if (applicationCodeCountryMap.Value.TryGetValue(applicationCode, out var country))
			{
				if (countrySpecificBaseTypes.Value.TryGetValue(country, out var baseType))
				{
					var countryTypeDecider = GetTypeDeciderFromType(baseType);
					if (baseType.IsAbstract && countryTypeDecider == null)
					{
						throw new ArgumentException(FormattableString.Invariant($"CusMAWBTypeDecider - Country specific type decider cannot be found for country {country}."));
					}

					result = countryTypeDecider != null ? countryTypeDecider.GetTypeForLoad(row, factory) : baseType;
				}
				else
				{
					throw new ArgumentException(FormattableString.Invariant($"CusMAWBTypeDecider - No base type has been specified for country {country}."));
				}
			}
			else
			{
				result = DefaultType;
			}

			return result;
		}

		public override Type GetTypeForBinding()
		{
			return null;
		}

		public override Type GetTypeForNew()
		{
			return null;
		}

		public static Type GetTypeForForwarding()
		{
			switch (GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
			{
				case Core.Constants.CountryCodes.Australia:
					return ObjectFactory.GetType<Integration.Customs.AU.ICusMAWB>();
				default:
					return null;
			}
		}

		public static string[] GetApplicationCodesForForwarding()
		{
			switch (GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
			{
				case Core.Constants.CountryCodes.Australia:
					return new[] { Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages };
				default:
					return Array.Empty<string>();
			}
		}

		public Type GetTypeForCountry(string country)
		{
			Type result;
			if (countrySpecificBaseTypes.Value.TryGetValue(country, out var baseType))
			{
				var countryTypeDecider = GetTypeDeciderFromType(baseType) as ICusMAWBTypeDecider;
				if (baseType.IsAbstract && countryTypeDecider == null)
				{
					throw new ArgumentException(FormattableString.Invariant($"CusMAWBTypeDecider - Country specific type decider cannot be found for country {country}."));
				}

				result = countryTypeDecider != null ? countryTypeDecider.DefaultType : baseType;
			}
			else
			{
				result = DefaultType;
			}

			return result;
		}

		readonly Lazy<ImmutableDictionary<string, Type>> countrySpecificBaseTypes = new Lazy<ImmutableDictionary<string, Type>>(() => new Dictionary<string, Type>
		{
			{ Core.Constants.CountryCodes.Australia, ObjectFactory.GetType<Integration.Customs.AU.ICusMAWBBase>() },
			{ Core.Constants.CountryCodes.NewZealand, ObjectFactory.GetType<Integration.Customs.NZ.ICusMAWB>() },
			{ Core.Constants.CountryCodes.UnitedKingdom, ObjectFactory.GetType<Integration.Customs.GB.CCSUK.ICusMAWB>() }
		}.ToImmutableDictionary());

		readonly Lazy<ImmutableDictionary<string, string>> applicationCodeCountryMap = new Lazy<ImmutableDictionary<string, string>>(() => new Dictionary<string, string>
		{
			{ Core.Constants.Customs.ExpressApplicationCodes.NZ.ECIWriteOff, Core.Constants.CountryCodes.NewZealand },
			{ Core.Constants.Customs.ExpressApplicationCodes.NZ.TSWWriteOff, Core.Constants.CountryCodes.NewZealand },
			{ ApplicationCodeList.Codes.GbCcsuk, Core.Constants.CountryCodes.UnitedKingdom },
			{ Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages, Core.Constants.CountryCodes.Australia },
		}.ToImmutableDictionary());

		public virtual Type DefaultType => typeof(CusMAWB);
	}
}
