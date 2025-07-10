using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class ACEAffirmationCodesCombinationValidation
	{
		public void ValidateAffirmationOfCodesForProcessingCodeAndIntendedUseCode(ACEFDA fda, ZPropertyInfo processingCodeInfo)
		{
			switch (fda.US_ProgramCode)
			{
				case FDAProgramCodeList.Codes.BIO:
				case FDAProgramCodeList.Codes.DRU:
					ValidateAffirmationOfCodesForBIOAndDRU(fda, processingCodeInfo);
					break;
				case FDAProgramCodeList.Codes.TOB:
					ValidateAffirmationOfCodesForTOB(fda, processingCodeInfo);
					break;
				case FDAProgramCodeList.Codes.VME:
					ValidateAffirmationOfCodesForVME(fda, processingCodeInfo);
					break;
			}
		}

		void ValidateAffirmationOfCodesForBIOAndDRU(ACEFDA fda, ZPropertyInfo processingCodeInfo)
		{
			var processingCode = (ZString)processingCodeInfo.Value;
			ValidateAffirmationOfCodesForCombinationOfProcessingCodeAndIntendedUseCode(fda, processingCodeInfo, delegate
			{
				switch (fda.US_ProgramCode)
				{
					case FDAProgramCodeList.Codes.BIO:
						return RequiredAffirmationCodesForBIO(fda.US_IntendedUseCode, processingCode);
					case FDAProgramCodeList.Codes.DRU:
						return RequiredAffirmationCodesForDRU(fda.US_IntendedUseCode, processingCode);
				}

				return new Dictionary<ZString, List<ZString>>();
			});
		}

		void ValidateAffirmationOfCodesForTOB(ACEFDA fda, ZPropertyInfo processingCodeInfo)
		{
			if (fda.AffirmationCodes.OfType<ACEAffirmationCode>().Any(x => x.CY_Code == ACE_AffirmationOfComplianceList.Codes.SE ||
																		x.CY_Code == ACE_AffirmationOfComplianceList.Codes.PMT ||
																		x.CY_Code == ACE_AffirmationOfComplianceList.Codes.EXE)
					&& !fda.AffirmationCodes.OfType<ACEAffirmationCode>().Any(x => x.CY_Code == ACE_AffirmationOfComplianceList.Codes.CMT)
					&& !fda.AffirmationCodes.OfType<ACEAffirmationCode>().Any(x => x.CY_Code == ACE_AffirmationOfComplianceList.Codes.TST))
			{
				processingCodeInfo.AddMessageError(TSTIsRequired);
			}
		}
		internal const string TSTIsRequired = "Affirmation Of Compliance code 'TST' is mandatory if Processing Code is 'CSU' and either affirmation code 'SE' or 'PMT' or 'EXE' is declared. If declaring 'CMT', then 'TST' is optional.";

		void ValidateAffirmationOfCodesForVME(ACEFDA fda, ZPropertyInfo processingCodeInfo)
		{
			var processingCode = (ZString)processingCodeInfo.Value;
			if (processingCode == FDAProcessingCodeList.Codes.VME_ADR)
			{
				ValidateAffirmationOfCodesForProcessingCode(fda, processingCodeInfo, delegate
				{
					return new List<ZString>()
						{
							ACE_AffirmationOfComplianceList.Codes.REG
						};
				});

				if (fda.US_IntendedUseCode == FDAIntendedUseCodesHelper.Codes._085003 ||
					fda.US_IntendedUseCode == FDAIntendedUseCodesHelper.Codes._150020)
				{
					if (!fda.AffirmationCodes.OfType<ACEAffirmationCode>().Any(x => x.CY_Code == ACE_AffirmationOfComplianceList.Codes.VNA ||
									x.CY_Code == ACE_AffirmationOfComplianceList.Codes.VAN))
					{
						processingCodeInfo.AddMessageError(EitherVNAOrVANIsRequired);
					}
				}
				else
				{
					ValidateAffirmationOfCodesForProcessingCode(fda, processingCodeInfo, delegate
					{
						if (fda.US_IntendedUseCode == FDAIntendedUseCodesHelper.Codes._180009)
						{
							return new List<ZString>()
							{
								ACE_AffirmationOfComplianceList.Codes.VIN
							};
						}
						return null;
					});
				}
			}

			ValidateAffirmationOfCodesForProcessingCode(fda, processingCodeInfo, delegate
			{
				if (fda.US_IntendedUseCode == FDAIntendedUseCodesHelper.Codes._085003 ||
				fda.US_IntendedUseCode == FDAIntendedUseCodesHelper.Codes._150020 ||
				fda.US_IntendedUseCode == FDAIntendedUseCodesHelper.Codes._150013 ||
				fda.US_IntendedUseCode == FDAIntendedUseCodesHelper.Codes._980000)
				{
					return new List<ZString>()
					{
						ACE_AffirmationOfComplianceList.Codes.NDC
					};
				}

				return null;
			});
		}
		internal const string EitherVNAOrVANIsRequired = "At least one of affirmation codes 'VNA' or 'VAN' should be declared if Processing Code is 'ADR' and Intended Use Code is 085.003 or 150.020.";

		Dictionary<ZString, List<ZString>> RequiredAffirmationCodesForBIO(ZString intendedUseCode, ZString processingCode)
		{
			if (intendedUseCode == FDAIntendedUseCodesHelper.Codes._180009 && FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_180009(processingCode))
			{
				return new Dictionary<ZString, List<ZString>>()
							{
								{ ACE_AffirmationOfComplianceList.Codes.IND, null }
							};
			}
			else if (intendedUseCode == FDAIntendedUseCodesHelper.Codes._080000)
			{
				if (FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_080000(processingCode))
				{
					return new Dictionary<ZString, List<ZString>>()
								{
									{ ACE_AffirmationOfComplianceList.Codes.BLN, null },
									{ ACE_AffirmationOfComplianceList.Codes.STN, null }
								};
				}
				else if (FDAProcessingCodeList.IsAOCRequiredForBIO_BBA_080000(processingCode))
				{
					return new Dictionary<ZString, List<ZString>>()
								{
									{ ACE_AffirmationOfComplianceList.Codes.DA, new List<ZString> { ACE_AffirmationOfComplianceList.Codes.NDA, ACE_AffirmationOfComplianceList.Codes.AND } },
									{ ACE_AffirmationOfComplianceList.Codes.REG, null }
								};
				}
			}
			else if (intendedUseCode == FDAIntendedUseCodesHelper.Codes._082000 && FDAProcessingCodeList.IsAOCRequiredForBIO_HCT_082000(processingCode))
			{
				return new Dictionary<ZString, List<ZString>>()
							{
								{ ACE_AffirmationOfComplianceList.Codes.HCT, null },
								{ ACE_AffirmationOfComplianceList.Codes.HRN, null }
							};
			}
			else if ((intendedUseCode == FDAIntendedUseCodesHelper.Codes._180016 && FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_180016(processingCode)
					|| (intendedUseCode == FDAIntendedUseCodesHelper.Codes._155000 && FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_155000(processingCode))
					|| (intendedUseCode == FDAIntendedUseCodesHelper.Codes._150007 && FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_150007(processingCode))))
			{
				return new Dictionary<ZString, List<ZString>>()
							{
								{ ACE_AffirmationOfComplianceList.Codes.BLN, null },
								{ ACE_AffirmationOfComplianceList.Codes.STN, null }
							};
			}
			else if (intendedUseCode == FDAIntendedUseCodesHelper.Codes._150007 && FDAProcessingCodeList.IsAOCRequiredForBIO_BBA_150007(processingCode))
			{
				return new Dictionary<ZString, List<ZString>>()
							{
								{ ACE_AffirmationOfComplianceList.Codes.DA, null }
							};
			}
			else if (intendedUseCode == FDAIntendedUseCodesHelper.Codes._970000 &&
					 FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_970000(processingCode))
			{
				return new Dictionary<ZString, List<ZString>>()
				{
					{ ACE_AffirmationOfComplianceList.Codes.IFE, null }
				};
			}

			return new Dictionary<ZString, List<ZString>>() { };
		}

		Dictionary<ZString, List<ZString>> RequiredAffirmationCodesForDRU(ZString intendedUseCode, ZString processingCode)
		{
			var result = new Dictionary<ZString, List<ZString>>();

			if (intendedUseCode == FDAIntendedUseCodesHelper.Codes._080012)
			{
				if (processingCode == FDAProcessingCodeList.Codes.DRU_PRE)
				{
					result.Add(ACE_AffirmationOfComplianceList.Codes.DA, null);
					result.Add(ACE_AffirmationOfComplianceList.Codes.REG, null);
				}
				else if (processingCode == FDAProcessingCodeList.Codes.DRU_804)
				{
					result.Add(ACE_AffirmationOfComplianceList.Codes.DA, null);
					result.Add(ACE_AffirmationOfComplianceList.Codes.DLS, null);
					result.Add(ACE_AffirmationOfComplianceList.Codes.FSR, null);
					result.Add(ACE_AffirmationOfComplianceList.Codes.PRN, null);
				}
			}
			else if (intendedUseCode == FDAIntendedUseCodesHelper.Codes._130000 ||
				intendedUseCode == FDAIntendedUseCodesHelper.Codes._150007 ||
				intendedUseCode == FDAIntendedUseCodesHelper.Codes._150013 ||
				intendedUseCode == FDAIntendedUseCodesHelper.Codes._150017 ||
				intendedUseCode == FDAIntendedUseCodesHelper.Codes._155009 ||
				intendedUseCode == FDAIntendedUseCodesHelper.Codes._980000
				)
			{
				result.Add(ACE_AffirmationOfComplianceList.Codes.REG, null);
				result.Add(ACE_AffirmationOfComplianceList.Codes.DLS, null);
			}
			else if (intendedUseCode == FDAIntendedUseCodesHelper.Codes._180009)
			{
				result.Add(ACE_AffirmationOfComplianceList.Codes.IND, null);
			}
			return result;
		}

		void ValidateAffirmationOfCodesForCombinationOfProcessingCodeAndIntendedUseCode(ACEFDA fda, ZPropertyInfo processingCodeInfo, Func<Dictionary<ZString, List<ZString>>> affirmationCodesRequired)
		{
			var processingCode = (ZString)processingCodeInfo.Value;
			var affirmationCodes = affirmationCodesRequired();
			if (fda != null && affirmationCodes != null && affirmationCodes.Count > 0)
			{
				foreach (var affirmationCode in affirmationCodes)
				{
					if (!fda.AffirmationCodes.OfType<ACEAffirmationCode>().Any(x => x.CY_Code == affirmationCode.Key))
					{
						if (affirmationCode.Value == null || affirmationCode.Value.Count == 0)
						{
							processingCodeInfo.AddMessageError(ZString.Format(AffirmationOfCodeForCombinationIsRequired, affirmationCode.Key, processingCode, fda.US_IntendedUseCode));
						}
						else
						{
							if (!fda.AffirmationCodes.OfType<ACEAffirmationCode>().Any(x => affirmationCode.Value.Any(code => code == x.CY_Code)))
							{
								processingCodeInfo.AddMessageError(ZString.Format(AffirmationOfCodeForCombinationIsRequiredIncludes, affirmationCode.Key, processingCode, fda.US_IntendedUseCode, string.Join("' and '", affirmationCode.Value)));
							}
						}
					}
				}
			}
		}
		internal const string AffirmationOfCodeForCombinationIsRequired = "Affirmation Of Compliance code '{0}' is required when Processing Code is '{1}' and Intended Use Code is '{2}'.";
		internal const string AffirmationOfCodeForCombinationIsRequiredIncludes = "Affirmation Of Compliance code '{0}' is required when Processing Code is '{1}' and Intended Use Code is '{2}'. '{0}' includes '{3}', either of them should be entered.";

		void ValidateAffirmationOfCodesForProcessingCode(ACEFDA fda, ZPropertyInfo processingCodeInfo, Func<List<ZString>> affirmationCodesRequired)
		{
			var processingCode = (ZString)processingCodeInfo.Value;
			var affirmationCodes = affirmationCodesRequired();
			if (fda != null && affirmationCodes != null && affirmationCodes.Count > 0)
			{
				foreach (var affirmationCode in affirmationCodes)
				{
					if (!fda.AffirmationCodes.OfType<ACEAffirmationCode>().Any(x => x.CY_Code == affirmationCode))
					{
						processingCodeInfo.AddMessageError(ZString.Format(AffirmationOfCodeForProcessingCodeIsRequired, affirmationCode, processingCode));
					}
				}
			}
		}
		internal const string AffirmationOfCodeForProcessingCodeIsRequired = "Affirmation Of Compliance code '{0}' is required when Processing Code is '{1}'.";
	}
}
