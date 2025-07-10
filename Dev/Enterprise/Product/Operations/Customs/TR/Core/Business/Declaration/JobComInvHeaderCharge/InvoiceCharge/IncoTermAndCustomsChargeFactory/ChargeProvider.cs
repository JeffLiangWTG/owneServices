using System;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public static class ChargeProvider
	{
		public static CustomsChargeCode InternationalFreight => internationalFreight ?? (internationalFreight = new CustomsChargeCode(TRIncotermChargeCodeList.Codes.OFT, TRIncotermChargeCodeList.Descriptions.OFT)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = false,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			DistributeBy = ChargeDistributeByList.Codes.Value,
			DistributeByDeemedForThisCharge = true
		});
		[ThreadStatic]
		static CustomsChargeCode internationalFreight;

		public static CustomsChargeCode InternationalInsurance => internationalInsurance ?? (internationalInsurance = new CustomsChargeCode(TRIncotermChargeCodeList.Codes.ONS, TRIncotermChargeCodeList.Descriptions.ONS)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = false,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			DistributeBy = ChargeDistributeByList.Codes.Value,
			DistributeByDeemedForThisCharge = true
		});
		[ThreadStatic]
		static CustomsChargeCode internationalInsurance;

		public static CustomsChargeCode Commission => commission ?? (commission = new CustomsChargeCode(TRIncotermChargeCodeList.Codes.COM, TRIncotermChargeCodeList.Descriptions.COM)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = false,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			DistributeBy = ChargeDistributeByList.Codes.Value,
			DistributeByDeemedForThisCharge = true
		});
		[ThreadStatic]
		static CustomsChargeCode commission;

		public static CustomsChargeCode Demurrage => demurrage ?? (demurrage = new CustomsChargeCode(TRIncotermChargeCodeList.Codes.DEM, TRIncotermChargeCodeList.Descriptions.DEM)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = false,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			DistributeBy = ChargeDistributeByList.Codes.Value,
			DistributeByDeemedForThisCharge = true
		});
		[ThreadStatic]
		static CustomsChargeCode demurrage;

		public static CustomsChargeCode Royalty => royalty ?? (royalty = new CustomsChargeCode(TRIncotermChargeCodeList.Codes.ROY, TRIncotermChargeCodeList.Descriptions.ROY)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = false,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			DistributeBy = ChargeDistributeByList.Codes.Value,
			DistributeByDeemedForThisCharge = true
		});
		[ThreadStatic]
		static CustomsChargeCode royalty;

		public static CustomsChargeCode Interest => interest ?? (interest = new CustomsChargeCode(TRIncotermChargeCodeList.Codes.INT, TRIncotermChargeCodeList.Descriptions.INT)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = false,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			DistributeBy = ChargeDistributeByList.Codes.Value,
			DistributeByDeemedForThisCharge = true
		});
		[ThreadStatic]
		static CustomsChargeCode interest;

		public static CustomsChargeCode Other => other ?? (other = new CustomsChargeCode(TRIncotermChargeCodeList.Codes.OTH, TRIncotermChargeCodeList.Descriptions.OTH)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = false,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			DistributeBy = ChargeDistributeByList.Codes.Value,
			DistributeByDeemedForThisCharge = true
		});
		[ThreadStatic]
		static CustomsChargeCode other;

		public static CustomsChargeCode Observation => observation ?? (observation = new CustomsChargeCode(TRIncotermChargeCodeList.Codes.Observation, TRIncotermChargeCodeList.Descriptions.Observation)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = false,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			DistributeBy = ChargeDistributeByList.Codes.Value,
			DistributeByDeemedForThisCharge = true
		});
		[ThreadStatic]
		static CustomsChargeCode observation;

		public static CustomsChargeCode Surveillance => surveillance ?? (surveillance = new CustomsChargeCode(TRIncotermChargeCodeList.Codes.Surveillance, TRIncotermChargeCodeList.Descriptions.Surveillance)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = false,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			DistributeBy = ChargeDistributeByList.Codes.Value,
			DistributeByDeemedForThisCharge = true
		});
		[ThreadStatic]
		static CustomsChargeCode surveillance;

		public static CustomsChargeCode TotalForeignCharges => totalForeignCharges ?? (totalForeignCharges = new CustomsChargeCode(TRIncotermChargeCodeList.Codes.TotalForeignCharges, TRIncotermChargeCodeList.Descriptions.TotalForeignCharges)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = false,
			IsVATible = false,
			IsVATibleDeemedForThisCharge = false,
			DistributeBy = ChargeDistributeByList.Codes.Value,
			DistributeByDeemedForThisCharge = true
		});
		[ThreadStatic]
		static CustomsChargeCode totalForeignCharges;

		public static CustomsChargeCode LocalBankCharge => localBankCharge ?? (localBankCharge = new CustomsChargeCode(TRIncotermChargeCodeList.Codes.LBC, TRIncotermChargeCodeList.Descriptions.LBC)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = false,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			DistributeBy = ChargeDistributeByList.Codes.Value,
			DistributeByDeemedForThisCharge = true
		});
		[ThreadStatic]
		static CustomsChargeCode localBankCharge;

		public static CustomsChargeCode LocalStorageCharge => localStorageCharge ?? (localStorageCharge = new CustomsChargeCode(TRIncotermChargeCodeList.Codes.LSC, TRIncotermChargeCodeList.Descriptions.LSC)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = false,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			DistributeBy = ChargeDistributeByList.Codes.Value,
			DistributeByDeemedForThisCharge = true
		});
		[ThreadStatic]
		static CustomsChargeCode localStorageCharge;

		public static CustomsChargeCode LocalDischargeCharge => localDischargeCharge ?? (localDischargeCharge = new CustomsChargeCode(TRIncotermChargeCodeList.Codes.LDC, TRIncotermChargeCodeList.Descriptions.LDC)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = false,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			DistributeBy = ChargeDistributeByList.Codes.Value,
			DistributeByDeemedForThisCharge = true
		});
		[ThreadStatic]
		static CustomsChargeCode localDischargeCharge;

		public static CustomsChargeCode LocalPortCharge => localPortCharge ?? (localPortCharge = new CustomsChargeCode(TRIncotermChargeCodeList.Codes.LPC, TRIncotermChargeCodeList.Descriptions.LPC)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = false,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			DistributeBy = ChargeDistributeByList.Codes.Value,
			DistributeByDeemedForThisCharge = true
		});
		[ThreadStatic]
		static CustomsChargeCode localPortCharge;

		public static CustomsChargeCode LocalCultureCharge => localCultureCharge ?? (localCultureCharge = new CustomsChargeCode(TRIncotermChargeCodeList.Codes.LocalCultureCharge, TRIncotermChargeCodeList.Descriptions.LocalCultureCharge)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = false,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			DistributeBy = ChargeDistributeByList.Codes.Value,
			DistributeByDeemedForThisCharge = true
		});
		[ThreadStatic]
		static CustomsChargeCode localCultureCharge;

		public static CustomsChargeCode LocalResourceUtilizationSupportFund => localResourceUtilizationSupportFund ?? (localResourceUtilizationSupportFund = new CustomsChargeCode(TRIncotermChargeCodeList.Codes.LocalResourceUtilizationSupportFundCharge, TRIncotermChargeCodeList.Descriptions.LocalResourceUtilizationSupportFundCharge)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = false,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			DistributeBy = ChargeDistributeByList.Codes.Value,
			DistributeByDeemedForThisCharge = true
		});
		[ThreadStatic]
		static CustomsChargeCode localResourceUtilizationSupportFund;

		public static CustomsChargeCode LocalEnvironmentCharge => localEnvironmentCharge ?? (localEnvironmentCharge = new CustomsChargeCode(TRIncotermChargeCodeList.Codes.LocalEnvironmentCharge, TRIncotermChargeCodeList.Descriptions.LocalEnvironmentCharge)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = false,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			DistributeBy = ChargeDistributeByList.Codes.Value,
			DistributeByDeemedForThisCharge = true
		});
		[ThreadStatic]
		static CustomsChargeCode localEnvironmentCharge;

		public static CustomsChargeCode LocalOther => localOther ?? (localOther = new CustomsChargeCode(TRIncotermChargeCodeList.Codes.LOT, TRIncotermChargeCodeList.Descriptions.LOT)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = false,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			DistributeBy = ChargeDistributeByList.Codes.Value,
			DistributeByDeemedForThisCharge = true
		});
		[ThreadStatic]
		static CustomsChargeCode localOther;

		public static CustomsChargeCode LocalTotalCharge => localTotalCharge ?? (localTotalCharge = new CustomsChargeCode(TRIncotermChargeCodeList.Codes.LocalTotalCharges, TRIncotermChargeCodeList.Descriptions.LocalTotalCharges)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = false,
			IsVATible = false,
			IsVATibleDeemedForThisCharge = false,
			IsStatisticalValueApplicable = false,
			IsStatisticalValueApplicableDeemed = true,
			IsIncludedInITOTDeemedForThisCharge = true,
			IsIncludedInITOTIfDeemed = true,
			DistributeBy = ChargeDistributeByList.Codes.Value,
			DistributeByDeemedForThisCharge = true
		});
		[ThreadStatic]
		static CustomsChargeCode localTotalCharge;
	}
}
