using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public partial class GovernmentAgencyProgramCodeList
	{
		public const string NMFS = "NMFS";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public static bool IsPGAAllowed(ZBool isEntrySummary, ZBool isCargoRelease, ZBool isCertified, ZBool isExpeditedRelease, ZBool isWeeklyEstimateFiling, ZString entryType, ZString pgaCode)
		{
			var result = true;

			if ((isEntrySummary || isCargoRelease) && !entryType.IsEmpty && !isExpeditedRelease)
			{
				var isAECertifiedOrSE = (isEntrySummary && isCertified) || isCargoRelease;
				switch (entryType)
				{
					case EntryTypeList.Codes.ConsumptionFreeDutiable:
					case EntryTypeList.Codes.ConsumptionADDCVD:
						result = isAECertifiedOrSE;
						break;
					case EntryTypeList.Codes.ConsumptionQuotaVisa:
					case EntryTypeList.Codes.ConsumptionADDCVDQuotaVisa:
						result = isAECertifiedOrSE;
						break;
					case EntryTypeList.Codes.ConsumptionFTZ:
						if (isWeeklyEstimateFiling)
						{
							result = false;

							if (isCargoRelease)
							{
								result = pgaCode != Codes.Lacey && pgaCode != Codes.APHIS && pgaCode != Codes.VNE && pgaCode != Codes.NHTSA
									&& !IsNMFSAgencyProgramCode(pgaCode) && pgaCode != Codes.OMC && pgaCode != Codes.AMS;
							}

							if (isEntrySummary && !isExpeditedRelease)
							{
								result |= pgaCode == Codes.Lacey || pgaCode == Codes.VNE || pgaCode == Codes.NHTSA || IsNMFSAgencyProgramCode(pgaCode) || pgaCode == Codes.OMC
								|| pgaCode == Codes.AMS;
							}
						}
						else
						{
							if (isAECertifiedOrSE)
							{
								result = pgaCode != Codes.APHIS;
							}
							else if (isEntrySummary && !isExpeditedRelease)
							{
								result = pgaCode == Codes.VNE;
							}
						}
						break;
					case EntryTypeList.Codes.InformalFreeDutiable:
						result = isAECertifiedOrSE && pgaCode != Codes.Lacey;
						break;
					case EntryTypeList.Codes.InformalQuotaVisa:
						result = isAECertifiedOrSE && pgaCode != Codes.Lacey;
						break;
					case EntryTypeList.Codes.Warehouse:
						result = isAECertifiedOrSE && pgaCode != Codes.Lacey && pgaCode != Codes.DEA
							&& !IsNMFSAgencyProgramCode(pgaCode) && pgaCode != Codes.OMC && pgaCode != Codes.TTB;
						break;
					case EntryTypeList.Codes.ReWarehouse:
						result = false;
						break;
					case EntryTypeList.Codes.TemporaryImportationBond:
						result = isAECertifiedOrSE && pgaCode != Codes.Lacey && pgaCode != Codes.TTB;
						break;
					case EntryTypeList.Codes.GovernmentDutiable:
						result = isAECertifiedOrSE && !IsNMFSAgencyProgramCode(pgaCode);
						break;
					case EntryTypeList.Codes.LowValue:
						result = isCargoRelease && pgaCode != Codes.Lacey;
						break;
					case EntryTypeList.Codes.WarehouseWithdrawalConsumption:
					case EntryTypeList.Codes.WarehouseWithdrawalQuota:
					case EntryTypeList.Codes.WarehouseWithdrawalADDCVD:
					case EntryTypeList.Codes.WarehouseWithdrawalADDCVDQuotaVisa:
						result = isEntrySummary &&
							(pgaCode == Codes.Lacey || pgaCode == Codes.DEA || IsNMFSAgencyProgramCode(pgaCode) || pgaCode == Codes.OMC || pgaCode == Codes.TTB);
						break;
				}
			}

			return result;
		}

		public static bool CanDisclaim(string code)
		{
			return code != Codes.ATF && code != Codes.DDTC && code != Codes.SIMP && code != Codes.COA;
		}

		static bool IsNMFSAgencyProgramCode(ZString code)
		{
			return code == NMFS || code == Codes._370 || code == Codes.HMS || code == Codes.AMR || code == Codes.SIMP || code == Codes.COA;
		}
	}
}
