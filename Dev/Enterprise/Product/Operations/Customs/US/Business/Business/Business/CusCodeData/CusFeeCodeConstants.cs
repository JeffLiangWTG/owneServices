using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.Customs.US.Business
{
	public static class CusFeeCodeConstants
	{
		public static CodeDescriptionPairList GetAccountingClassFeeCodeList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("AccountingClassFeeCode", () =>
			{
				var list = new CodeDescriptionPairList();
				var codes = ZZRefCusCodeListCombined.Loader.Load(factory, Core.Constants.CountryCodes.UnitedStates, RefCusCodeListTypes.Codes.AccountingClassFeeCode, ZDateTime.Now);
				foreach (var codeListCombined in codes)
				{
					list.AddPairIfNotExist(codeListCombined.ZZD_Code, codeListCombined.ZZD_Description);
				}
				list.Sort();
				return list;
			});
		}

		public static CodeDescriptionPairList GetReconEntryHeaderFeeChargeCodeList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("ReconEntryHeaderFeeChargeCodeList", () =>
			{
				var list = new CodeDescriptionPairList();
				var codes = ZZRefCusCodeListCombined.Loader.Load(factory, Core.Constants.CountryCodes.UnitedStates, RefCusCodeListTypes.Codes.AccountingClassFeeCode, ZDateTime.Now);
				list.AddRange(codes);
				list.AddPairIfNotExist(Core.Constants.USCustoms.FeeCodes.Duty, "Duty");
				list.AddPairIfNotExist(Core.Constants.USCustoms.FeeCodes.ReconciliationInterest, "Interest Amount For Reconciliation Summary");
				list.AddPairIfNotExist(Core.Constants.USCustoms.FeeCodes.MPC, "MPF As Calculated And Unadjusted");
				list.Sort();
				return list;
			});
		}

		public static string[] GetTaxCodes()
		{
			return new string[]
			{
				Core.Constants.USCustoms.FeeCodes.DistilledSpirits,
				Core.Constants.USCustoms.FeeCodes.Tobacco,
				Core.Constants.USCustoms.FeeCodes.Wines,
				Core.Constants.USCustoms.FeeCodes.OtherExcise
			};
		}

		public static bool IsExciseTax(string code)
		{
			return code == Core.Constants.USCustoms.FeeCodes.OtherExcise
				|| code == Core.Constants.USCustoms.FeeCodes.DistilledSpirits
				|| code == Core.Constants.USCustoms.FeeCodes.Tobacco
				|| code == Core.Constants.USCustoms.FeeCodes.Wines;
		}

		public static bool IsHeaderLevelFee(string feeCode)
		{
			return HeaderLevelFeeCodes.Any(code => code == feeCode);
		}

		public static IEnumerable<ZString> HeaderLevelFeeCodes
		{
			get
			{
				yield return Core.Constants.USCustoms.FeeCodes.DutiableMail;
				yield return Core.Constants.USCustoms.FeeCodes.MerchandiseSurcharge;
				yield return Core.Constants.USCustoms.FeeCodes.MerchandiseInformal;
			}
		}

		public static bool IsRelatedToTariffNumber(string feeCode)
		{
			return feeCode != Core.Constants.USCustoms.FeeCodes.HMF
				&& feeCode != Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing
				&& feeCode != Core.Constants.USCustoms.FeeCodes.OtherAgencies
				&& !IsHeaderLevelFee(feeCode);
		}

		public static bool IsLineLevel62Record(string feeCode)
		{
			return feeCode == Core.Constants.USCustoms.FeeCodes.Beef ||
				feeCode == Core.Constants.USCustoms.FeeCodes.Pork ||
				feeCode == Core.Constants.USCustoms.FeeCodes.Honey ||
				feeCode == Core.Constants.USCustoms.FeeCodes.Cotton ||
				feeCode == Core.Constants.USCustoms.FeeCodes.Raspberry ||
				feeCode == Core.Constants.USCustoms.FeeCodes.Sugar ||
				feeCode == Core.Constants.USCustoms.FeeCodes.Potato ||
				feeCode == Core.Constants.USCustoms.FeeCodes.FreshLimes ||
				feeCode == Core.Constants.USCustoms.FeeCodes.Mushroom ||
				feeCode == Core.Constants.USCustoms.FeeCodes.Watermelon ||
				feeCode == Core.Constants.USCustoms.FeeCodes.SoftwoodLumber ||
				feeCode == Core.Constants.USCustoms.FeeCodes.Blueberry ||
				feeCode == Core.Constants.USCustoms.FeeCodes.Avocado ||
				feeCode == Core.Constants.USCustoms.FeeCodes.Mango ||
				feeCode == Core.Constants.USCustoms.FeeCodes.Sorghum ||
				feeCode == Core.Constants.USCustoms.FeeCodes.DairyFee ||
				feeCode == Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing ||
				feeCode == Core.Constants.USCustoms.FeeCodes.HMF ||
				feeCode == Core.Constants.USCustoms.FeeCodes.Pecan ||
				feeCode == Core.Constants.USCustoms.FeeCodes.ChristmasTree;
		}

		public static ZBool OtherFeeCodesToExcludeForACEDrawback(ZString feeCode)
		{
			return feeCode == Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing || feeCode == Core.Constants.USCustoms.FeeCodes.HMF || IsExciseTax(feeCode);
		}
	}
}
