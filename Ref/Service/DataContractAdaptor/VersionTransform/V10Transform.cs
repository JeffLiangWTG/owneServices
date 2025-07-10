using System;
using CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.Service.DataContractAdaptor.VersionTransform
{
	public class V10Transform : ITransformStrategy
	{
		static V10Transform v10Transform;

		V10Transform() { }

		public static V10Transform Instance()
		{
			if (v10Transform == null)
			{
				v10Transform = new V10Transform();
			}
			return v10Transform;
		}

		public bool RequireTransform(Type dataSetType, int version)
		{
			return version < 10 && (dataSetType == typeof(RefCarrierCode)
				|| dataSetType == typeof(RefCusCodeList)
				|| dataSetType == typeof(RefCusCodeType)
				|| dataSetType == typeof(RefCusMap)
				|| dataSetType == typeof(RefCusNomenclatureGroup)
				|| dataSetType == typeof(RefCusProcedure)
				|| dataSetType == typeof(RefCusRateType)
				|| dataSetType == typeof(RefCusTariff)
				|| dataSetType == typeof(RefCusTariffType)
				|| dataSetType == typeof(RefCusTaxOrFee)
				|| dataSetType == typeof(RefCusTradeAgreement));
		}

		public T Transform<T>(T data)
		{
			var result = data;
			if (result != null)
			{
				if (result is RefCarrierCode)
				{
					var item = result as RefCarrierCode;
					item.ZZ4_RN_CountryOrGrouping = item.ZZ4_ZZZ_NKDataGrouping;
				}
				else if (result is RefCusCodeList)
				{
					var item = result as RefCusCodeList;
					item.ZZD_RN_CountryOrGrouping = item.ZZD_ZZZ_NKDataGrouping;
					item.ZZD_ZZN_NKCodeType = item.ZZD_ZZK_NKCodeType;
				}
				else if (result is RefCusCodeType)
				{
					var item = result as RefCusCodeType;
					item.ZZN_CodeType = item.ZZK_CodeType;
					item.ZZN_Description = item.ZZK_Description;
				}
				else if (result is RefCusMap)
				{
					var item = result as RefCusMap;
					item.ZZM_RN_CountryOrGrouping = item.ZZM_ZZZ_NKDataGrouping;
				}
				else if (result is RefCusNomenclatureGroup)
				{
					var item = result as RefCusNomenclatureGroup;
					item.ZZ5_RN_CountryOrGrouping = item.ZZ5_ZZZ_NKDataGrouping;
				}
				else if (result is RefCusNomenclatureGroupNote)
				{
					var item = result as RefCusNomenclatureGroupNote;
					item.ZZL_RN_CountryOrGrouping = item.ZZL_ZZZ_NKDataGrouping;
				}
				else if (result is RefCusProcedure)
				{
					var item = result as RefCusProcedure;
					item.ZZ6_RN_CountryOrGrouping = item.ZZ6_ZZZ_NKDataGrouping;
				}
				else if (result is RefCusRateType)
				{
					var item = result as RefCusRateType;
					item.ZZR_RN_CountryOrGrouping = item.ZZR_ZZZ_NKDataGrouping;
				}
				else if (result is RefCusTariff)
				{
					var item = result as RefCusTariff;
					item.ZZ1_RN_CountryOrGrouping = item.ZZ1_ZZZ_NKDataGrouping;
				}
				else if (result is RefCusTariffRelationship)
				{
				}
				else if (result is RefCusTariffType)
				{
					var item = result as RefCusTariffType;
					item.ZZI_RN_CountryOrGrouping = item.ZZI_ZZZ_NKDataGrouping;
				}
				else if (result is RefCusTaxOrFee)
				{
					var item = result as RefCusTaxOrFee;
					item.ZZF_RN_CountryOrGrouping = item.ZZF_ZZZ_NKDataGrouping;
				}
				else if (result is RefCusTradeAgreement)
				{
					var item = result as RefCusTradeAgreement;
					item.ZZA_RN_CountryOrGrouping = item.ZZA_ZZZ_NKDataGrouping;
					item.ZZA_TradeAgreement = item.ZZA_TradeGroup;
				}
				else if (result is RefCusTradeAgreementCountry)
				{
					var item = result as RefCusTradeAgreementCountry;
					item.ZZB_RN_NKTradeAgreementCountryCode = item.ZZB_RN_NKTradeGroupCountryCode;
				}
			}
			return result;
		}

		readonly Type[] typesToBeTransformed = {
				typeof(RefCarrierCode),
				typeof(RefCusCodeList),
				typeof(RefCusCodeType),
				typeof(RefCusMap),
				typeof(RefCusNomenclatureGroup),
				typeof(RefCusNomenclatureGroupNote),
				typeof(RefCusProcedure),
				typeof(RefCusRateType),
				typeof(RefCusTariff),
				typeof(RefCusTariffType),
				typeof(RefCusTaxOrFee),
				typeof(RefCusTradeAgreement),
				typeof(RefCusTradeAgreementCountry)
			};

		public Type[] ToBeTransformedTypes
		{
			get { return typesToBeTransformed; }
		}
	}
}
