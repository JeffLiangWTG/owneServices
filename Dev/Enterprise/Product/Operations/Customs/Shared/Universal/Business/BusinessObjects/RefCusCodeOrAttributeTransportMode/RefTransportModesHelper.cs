using System.Globalization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public static class RefTransportModesHelper
	{
		public const int MaxLength = 3;

		public static ICodeDescriptionPairList GetList(BusinessObjectFactory factory)
		{
			Argument.NotNull(factory, "Factory");

			return factory.GetCachedValue<ICodeDescriptionPairList>("UniveralRefTransportModeList", () =>
			{
				return new RefTransportModeList();
			});
		}

		public static bool ExistsTransportModesForThatCodeTypeAndCountryOrGroupInZZDatabase(BusinessObjectFactory factory, ZString codeType, ZString countryOrGrouping)
		{
			Argument.NotNull(factory, "Factory");

			return factory.GetCachedValue(string.Format(CultureInfo.CurrentCulture, "TransportModesExists_{0}_{1}", codeType, countryOrGrouping), () =>
			{
				ZDBOnlyQuery transQuery = GetQueryForThatCodeTypeAndCountryOrGroup(codeType, countryOrGrouping);
				return factory.ExistsInDatabase(RefCusCodeOrAttributeTransportModeSchema.Constants.TableName, transQuery);
			});
		}

		public static bool ExistsSpecificTransportModeForThatCodeTypeAndCountryOrGroupInZZDatabase(BusinessObjectFactory factory, ZString codeType, ZString countryOrGrouping, ZString specificTransportMode)
		{
			Argument.NotNull(factory, "Factory");

			return factory.GetCachedValue(string.Format(CultureInfo.CurrentCulture, "TransportModesExists_{0}_{1}_{2}", codeType, countryOrGrouping, specificTransportMode), () =>
			{
				ZDBOnlyQuery transQuery = GetQueryForThatCodeTypeAndCountryOrGroup(codeType, countryOrGrouping);
				transQuery.AddToFilter(RefCusCodeOrAttributeTransportModeSchema.ZZU_TransportMode, specificTransportMode);
				return factory.ExistsInDatabase(RefCusCodeOrAttributeTransportModeSchema.Constants.TableName, transQuery);
			});
		}

		static ZDBOnlyQuery GetQueryForThatCodeTypeAndCountryOrGroup(ZString codeType, ZString countryOrGrouping)
		{
			var codeQuery = new ZDBOnlySubQuery(typeof(RefCusCodeList), RefCusCodeOrAttributeTransportModeSchema.ZZU_ZZD_CodeList);
			codeQuery.AddToFilter(RefCusCodeListSchema.ZZD_ZZK_NKCodeType, codeType);
			codeQuery.AddToFilter(RefCusCodeListSchema.ZZD_ZZZ_NKDataGrouping, countryOrGrouping);

			var transQuery = new ZDBOnlyQuery(typeof(RefCusCodeOrAttributeTransportMode));
			transQuery.AddSubQuery(codeQuery, JoinCondition.And);
			return transQuery;
		}

		public static bool ExistsTransportModesForThatAttributeNameInZZDatabase(BusinessObjectFactory factory, ZString codeType, ZString countryOrGrouping, ZString attributeName)
		{
			Argument.NotNull(factory, "Factory");

			return factory.GetCachedValue(string.Format(CultureInfo.CurrentCulture, "TransportModesExists_{0}_{1}_{2}", codeType, countryOrGrouping, attributeName), () =>
			{
				var codeQuery = new ZDBOnlySubQuery(typeof(RefCusCodeList), RefCusCodeListAttributeSchema.ZZE_ZZD_CodeList);
				codeQuery.AddToFilter(RefCusCodeListSchema.ZZD_ZZK_NKCodeType, codeType);
				codeQuery.AddToFilter(RefCusCodeListSchema.ZZD_ZZZ_NKDataGrouping, countryOrGrouping);

				var attrQuery = new ZDBOnlySubQuery(typeof(RefCusCodeListAttribute), RefCusCodeOrAttributeTransportModeSchema.ZZU_ZZE_Attribute);
				attrQuery.AddToFilter(RefCusCodeListAttributeSchema.ZZE_ZXE_NKName, attributeName);
				attrQuery.AddSubQuery(codeQuery, JoinCondition.And);

				var transQuery = new ZDBOnlyQuery(typeof(RefCusCodeOrAttributeTransportMode));
				transQuery.AddSubQuery(attrQuery, JoinCondition.And);

				return factory.ExistsInDatabase(RefCusCodeOrAttributeTransportModeSchema.Constants.TableName, transQuery);
			});
		}

		public static ZBoolDescriptionPairList CreateNewTransportModePairList(this ITransportModeListSupporter supporter)
		{
			Argument.NotNull(supporter, nameof(supporter));

			var factory = (supporter as BusinessObject)?.Factory ?? new BusinessObjectFactory();

			var result = new ZBoolDescriptionPairList();
			foreach (CodeDescriptionPair transportMode in GetList(factory))
			{
				result.AddNew(transportMode.Code, supporter.IsTransportModeApplied(transportMode.Code));
			}

			result.OnPairChanged += (e) =>
			{
				supporter.SetTransportMode(e.Pair.Description, e.Pair.Value);
				supporter.TransportModesPropertyInfo?.RefreshBinding();
			};

			foreach (var pair in result)
			{
				var propertyInfo = supporter.GetZPropertyInfoForTransportMode(pair.Description);
				if (propertyInfo != null)
				{
					propertyInfo.ValueChanged += delegate
					{ result[pair.Description].Value = (ZBool)propertyInfo.Value; };
				}
			}
			return result;
		}

		public static ZBool IsTransportModeApplied(this ITransportModeListSupporter supporter, ZString transportMode)
		{
			Argument.NotNull(supporter, nameof(supporter));

			var factory = (supporter as BusinessObject)?.Factory ?? new BusinessObjectFactory();

			var propertyInfo = GetList(factory).ContainsCode(transportMode) ? supporter.GetZPropertyInfoForTransportMode(transportMode) : null;
			return propertyInfo != null ? (ZBool)propertyInfo.Value : ZBool.False;
		}

		static void SetTransportMode(this ITransportModeListSupporter supporter, ZString transportMode, ZBool applied)
		{
			var propertyInfo = supporter.GetZPropertyInfoForTransportMode(transportMode);
			if (propertyInfo != null)
			{
				propertyInfo.Value = applied;
			}
		}

		internal static ZPropertyInfo GetZPropertyInfoForTransportMode(this ITransportModeListSupporter supporter, ZString transportMode)
		{
			Argument.NotNull(supporter, nameof(supporter));

			var propertyInfo = (supporter as BusinessObject)?.FindPropertyInfo(supporter.GetTransportModePropertyName(transportMode));
			return propertyInfo?.PropertyType == typeof(ZBool) ? propertyInfo : null;
		}
	}

	public interface ITransportModeListSupporter
	{
		string GetTransportModePropertyName(ZString transportMode);
		ZPropertyInfo TransportModesPropertyInfo { get; }
	}
}
