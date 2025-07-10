using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	class TariffValidator
	{
		internal static void Validate(JobComInvoiceLine invoiceLine, USCTariff importTariff, ZPropertyInfo tariffInfo)
		{
			if (invoiceLine.EffectiveDateForDutyRate.IsValid)
			{
				if (importTariff == null)
				{
					if (!tariffInfo.Value.IsEmpty)
					{
						ValidateWhenImportTariffIsNull(invoiceLine, tariffInfo);
					}
				}
				else
				{
					ValidateEntryDateRestriction(invoiceLine, importTariff, tariffInfo);

					EnsuresDerivedLineHasChildLines(invoiceLine, importTariff, tariffInfo);
				}
			}
		}

		internal static bool CorrectTariffLength(ZString tariff)
		{
			bool chapter98Or99Tariff = tariff.StartsWith("98", System.StringComparison.OrdinalIgnoreCase) || tariff.StartsWith("99", System.StringComparison.OrdinalIgnoreCase);
			return chapter98Or99Tariff ? tariff.Length == 8 || tariff.Length == 10 : tariff.Length == 10;
		}

		internal static bool NeedsFullValueToCalculateProvDutyFor232(ZString tariff)
		{
			return tariff.StartsWith("98020060", System.StringComparison.OrdinalIgnoreCase);
		}

		internal static bool IsProvOrProgDutyNoNeedForSection301(IEntryLineOrInvoiceLineDutyData line)
		{
			var allTariffs = new List<ZString>();
			if (!line.Tariff.IsEmpty)
			{
				allTariffs.Add(line.Tariff);
			}

			if (line.SupTariffs.Count > 0)
			{
				allTariffs.AddRange(line.SupTariffs);
			}

			var date = line.DateForDutyCalculation;
			var factory = line.Factory;

			return allTariffs.Any(tariff => USRefTariffDataLoader.TariffViewHasRuleWithAttribute(factory, tariff, date, UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs) != null);
		}

		static bool HasTariffAttributeType301CN(BusinessObjectFactory factory, ZString tariff, ZDateTime effectiveDateForDutyRate)
		{
			return USRefTariffDataLoader.TariffViewHasRuleWithAttribute(factory, tariff, effectiveDateForDutyRate, UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values._301CN) != null;
		}

		internal static bool IsSection232Relevant(BusinessObjectFactory factory, ZString tariff, ZDateTime effectiveDateForDutyRate)
		{
			return USRefTariffDataLoader.TariffViewHasRuleWithAttribute(factory, tariff, effectiveDateForDutyRate, UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values._232) != null;
		}

		static void ValidateWhenImportTariffIsNull(JobComInvoiceLine invoiceLine, ZPropertyInfo tariffInfo)
		{
			var tariffNumber = (ZString)tariffInfo.Value;
			if (tariffNumber != TariffViewAsCodeDescription.NotApplicableCode)
			{
				if (!CorrectTariffLength(tariffNumber))
				{
					tariffInfo.AddMessageError(InvalidLength);
				}
				else
				{
					USCTariff tariff = invoiceLine.Factory.LoadTop1<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, tariffInfo.Value));
					if (tariff == null)
					{
						tariffInfo.AddMessageError("Tariff unable to be found. " + SendTariffRequestMessagesAdviceForImport);
					}
					else
					{
						var effectiveDate = invoiceLine.EffectiveDateForDutyRate;
						tariffInfo.AddMessageError(GetTariffFoundButNotValid(tariffInfo.HumanReadableName, effectiveDate.ToShortDateString()) + SendTariffRequestMessagesAdviceForImport);
					}
				}
			}
		}

		internal static void ValidateWhenTariffViewIsNull(JobComInvoiceLine invoiceLine, ZPropertyInfo tariffInfo, ZString tariffType, ZDateTime validDate)
		{
			var tariffNumber = (ZString)tariffInfo.Value;
			if (tariffNumber == TariffViewAsCodeDescription.NotApplicableCode)
			{
				return;
			}

			var tariffNotFoundOrNotValid = true;
			if (tariffType != Universal.Constants.TariffTypes.ScheduleB && !CorrectTariffLength(tariffNumber))
			{
				tariffInfo.AddMessageError(InvalidLength);
			}
			else
			{
				var tariffExists = new TariffView.Loader(invoiceLine.Factory).Exists(Core.Constants.CountryCodes.UnitedStates, tariffType, (ZString)tariffInfo.Value);
				if (tariffExists)
				{
					if (tariffType == Constants.TariffTypes.ScheduleB || tariffType == Constants.TariffTypes.Export)
					{
						if (invoiceLine.TariffExpirationDateWinin30Days != null)
						{
							tariffNotFoundOrNotValid = false;
							tariffInfo.AddWarning(TariffExpiredButCanBeUsedWithin30Days);
						}
						else
						{
							tariffInfo.AddMessageError(GetTariffFoundButNotValid(tariffInfo.HumanReadableName, validDate.ToShortDateString()));
						}
					}
					else
					{
						tariffInfo.AddMessageError(GetTariffFoundButNotValid(tariffInfo.HumanReadableName, validDate.ToShortDateString()));
					}
				}
			}

			if (tariffNotFoundOrNotValid)
			{
				tariffInfo.AddMessageError(ListValidation.InvalidCodeMessageError.ToString());
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		internal const string SendTariffRequestMessagesAdviceForImport = "When the Declaration is saved, CargoWise One will automatically submit a request for the latest information relating to the Tariff that has been entered. A response should be available in a few minutes. Duties and fees can then be recalculated by selecting from the Menu, 'Brokerage > Generate Entries (Merge)'. Note that Tariff Requests can also be sent manually, at any time, by selecting from the Menu, 'Brokerage > Request Tariffs'. If the duty calculation date has changed due to a Release Date Update or any other date changes, you can reset the date used to validate tariffs by selecting from the menu, Brokerage > Reset Duty Calculation Date.";
		internal const string InvalidLength = "Tariff is invalid. The tariff should be 10 digits or 8 digits (in the case of chapter 98 and 99 tariffs)";
		internal const string TariffExpiredButCanBeUsedWithin30Days = "This tariff has expired but can still be used as you are within the 30 days grace period for tariff expirations.";

		internal static string GetTariffFoundButNotValid(string type, string date) => System.FormattableString.Invariant($"{type} was found but is not valid for {date}.");

		internal static void ValidateBasedOnAdditionalTariffNumberIndicator(JobComInvoiceLine invoiceLine, USCTariff tariff, ZPropertyInfo tariffInfo, bool hasSecondaryLine)
		{
			if (tariff != null && !tariffInfo.Value.IsEmpty && !invoiceLine.IsCombinedLine())
			{
				if (!tariff.UE_AdditionalTariffNumberIndicator && (tariff.UE_Tariff.StartsWith("98") || tariff.UE_Tariff.StartsWith("99")))
				{
					if (hasSecondaryLine)
					{
						tariffInfo.AddMessageError(NoSecondaryTariffNumberAllowed);
					}
				}
				else if (HasTariffAttributeType301CN(invoiceLine.Factory, tariff.UE_Tariff, invoiceLine.EffectiveDateForDutyRate) && !hasSecondaryLine)
				{
					if (invoiceLine.US_UC_NKCountryOfOrigin == Core.Constants.CountryCodes.China)
					{
						tariffInfo.AddWarning(WatchOrClockShouldReportSeparatelyWarning);
					}
					else
					{
						tariffInfo.AddMessageError(WatchOrClockShouldReportSeparatelyError);
					}
				}
				else if (ShouldHaveSecondaryLine(invoiceLine, tariff) && !hasSecondaryLine)
				{
					tariffInfo.AddMessageError(TariffRequiresAdditionalTariff);
				}
			}
		}

		internal const string NoSecondaryTariffNumberAllowed = "This tariff may not have secondary tariff numbers associated.";
		internal const string TariffRequiresAdditionalTariff = "This tariff requires additional tariff lines.";
		internal const string WatchOrClockShouldReportSeparatelyWarning = "Watch or Clock may be reported as separate entry lines only if it contains Chinese and non-Chinese components from various countries of origin and Section 301 Duties apply to some of these components. Please confirm that all components are reported correctly.";
		internal const string WatchOrClockShouldReportSeparatelyError = "Watch or Clock may be reported as separate entry lines only if it contains Chinese and non-Chinese components from various countries of origin and Section 301 Duties apply to some of these components. Please confirm that all components are reported correctly. This cannot be validated systematically, authorized users should review and send with message errors.";

		static bool ShouldHaveSecondaryLine(JobComInvoiceLine invoiceLine, USCTariff tariff)
		{
			bool result = false;

			if (!invoiceLine.IsSetXLine && !invoiceLine.IsSecondaryTariffLine && invoiceLine.HasEmptySupTariff)
			{
				if (tariff != null && tariff.UE_AdditionalTariffNumberIndicator)
				{
					if (tariff.UE_Tariff.StartsWith("99"))
					{
						result = true;
					}
					else
					{
						result = tariff.GetTariffRuleIfApplies(TariffRuleList.Codes.EligibleForSecondaryTariffNumbers, invoiceLine.EffectiveDateForDutyRate) != null;
					}
				}
			}

			return result;
		}

		static void EnsuresDerivedLineHasChildLines(JobComInvoiceLine invoiceLine, USCTariff tariff, ZPropertyInfo tariffInfo)
		{
			if (tariff != null && tariff.UE_DutyComputationCode == ComputationCodeList.Codes.Derived && invoiceLine.ChildLines.IsNullOrEmpty())
			{
				tariffInfo.AddMessageError(TariffRequiresSecondaryTariffForDutyCalculation);
			}
		}
		internal const string TariffRequiresSecondaryTariffForDutyCalculation = "The tariff you have selected requires a secondary tariff for duty calculation.";

		static void ValidateEntryDateRestriction(JobComInvoiceLine invoiceLine, USCTariff tariff, ZPropertyInfo tariffInfo)
		{
			ZDate dutyDate = invoiceLine.EffectiveDateForDutyRate;
			if (!tariff.ConformToDateRestriction(dutyDate))
			{
				tariffInfo.AddMessageError(EntryDateRestrictionMessage + dutyDate.ToShortDateString() + ". It is allowed only on the following date range(s):" + tariff.TariffDateRestrictions.GetAllRestrictionDateDescriptions(dutyDate.Year));
			}
		}
		internal const string EntryDateRestrictionMessage = "This tariff is not allowed to be imported on ";

		internal static void CheckSTNRule(JobComInvoiceLine invoiceLine, USCTariff tariff, ZPropertyInfo tariffInfo, ZBool isSupplementaryTariff)
		{
			if (tariff != null)
			{
				var effectiveDateForDutyRate = invoiceLine.EffectiveDateForDutyRate;
				var lineTariff = invoiceLine.JI_Tariff;
				var stnTariffRule = tariff.GetTariffRuleIfApplies(TariffRuleList.Codes.EligibleForSecondaryTariffNumbers, effectiveDateForDutyRate);

				if (stnTariffRule != null)
				{
					if (!invoiceLine.IsParentLine && !lineTariff.IsEmpty && (invoiceLine.IsSecondaryTariffLine || isSupplementaryTariff))
					{
						var secondaryTariffRule = stnTariffRule.SecondaryTariffs.GetMatchingSecondaryTariffRule(lineTariff, effectiveDateForDutyRate);
						if (secondaryTariffRule == null && !IsWatchStrapRelatedTariff(invoiceLine, tariff.UE_Tariff))
						{
							if (!HasTariffAttributeType301CN(invoiceLine.Factory, tariff.UE_Tariff, invoiceLine.EffectiveDateForDutyRate))
							{
								tariffInfo.AddMessageError(ValidationConstants.InvoiceLine.ImportTariff.ChildLineTariffNotFoundInRuleTariffs);
							}
						}
					}
					else if (invoiceLine.IsParentLine)
					{
						var secondaryTariffRule = stnTariffRule.SecondaryTariffs[0];
						var secondaryTariffs = new List<ZString>(secondaryTariffRule.GetASetOfAssociatedTariffNumbers());

						var secondaryTariffLinesCount = invoiceLine.SecondaryTariffLines.Count();
						var requiredSecondaryTariffsCount = secondaryTariffs.Count;
						requiredSecondaryTariffsCount += invoiceLine.SecondaryTariffLines.Count(x => IsWatchStrapRelatedTariff(x, invoiceLine.JI_Tariff));

						if (requiredSecondaryTariffsCount != secondaryTariffLinesCount &&
							//if this is a complete tariff
							secondaryTariffs.TrueForAll(x => (x.Length == 10 || x.Length == 12) && new USCTariff.Loader(invoiceLine.Factory).LoadBestMatch(x, invoiceLine.EffectiveDateForDutyRate) != null))
						{
							tariffInfo.AddMessageError(string.Format(ValidationConstants.InvoiceLine.ImportTariff.EnteredSecondaryTariffsDoNotMatchRule, requiredSecondaryTariffsCount, secondaryTariffLinesCount));
						}
					}
				}
			}
		}

		static bool IsWatchStrapRelatedTariff(JobComInvoiceLine invoiceLine, ZString tariffCode)
		{
			var result = false;
			var tariffView = USRefTariffDataLoader.TariffViewHasRuleWithAttribute(invoiceLine.Factory, invoiceLine.JI_Tariff, invoiceLine.EffectiveDateForDutyRate, UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, UniversalReferenceConstants.TariffAttributeTypes.Values.WatchStrap);
			if (tariffView != null)
			{
				result = tariffView.RelatedTariffs.Any(x => x.ZZH_ZZI_TariffTypeCode == Universal.Constants.TariffTypes.HarmonizedSystem && tariffCode.StartsWith(x.ZZH_TariffCode));
			}
			return result;
		}

		internal static void ValidateAgainstFormalEntryRequirement(JobDeclaration declaration, USCTariff tariff, ZDateTime effectiveDateForDutyRate, ZPropertyInfo tariffInfo, ZGuid lineParentID)
		{
			if (tariff != null)
			{
				var invoiceLine = tariffInfo.BizObj as JobComInvoiceLine;
				if (invoiceLine != null && !invoiceLine.IsCombinedLine())
				{
					if (!declaration.IsSampleShipment && lineParentID.IsEmpty &&
							tariff.Applies(TariffRuleList.Codes.RequiresFormalEntryRegardlessOfValue, effectiveDateForDutyRate))
					{
						tariffInfo.AddMessageError(FormalEntryRequired + TariffRuleList.Codes.RequiresFormalEntryRegardlessOfValue);
					}
				}
			}
		}
		internal const string FormalEntryRequired = "This tariff requires a formal entry according to a rule, ";

		internal static void Ensure98_99IsNotEntered(ZPropertyInfo tariffInfo, USCTariff tariff)
		{
			var value = (ZString)tariffInfo.Value;

			if (value.StartsWith("99"))
			{
				if (tariff == null || tariff.UE_AdditionalTariffNumberIndicator)
				{
					tariffInfo.AddMessageError(TariffNumber99ShouldNotBeEnteredHere);
				}
			}
		}
		internal const string TariffNumber99ShouldNotBeEnteredHere = "This tariff number should not be entered here. Please enter it in the Prov/Prog. Tariff field.";

		internal static void Ensure98_99IsEntered(ZPropertyInfo tariffInfo)
		{
			ZString value = (ZString)tariffInfo.Value;

			if (!value.IsEmpty && value != TariffViewAsCodeDescription.NotApplicableCode && !value.StartsWith("98") && !value.StartsWith("99"))
			{
				tariffInfo.AddMessageError(TariffNumber98_99ShouldBeEnteredHere);
			}
		}
		internal const string TariffNumber98_99ShouldBeEnteredHere = "The tariff numbers entered here should be one in chapter 98 or 99.";

		internal static void ValidateFishNotFromRussia(JobComInvoiceLine invoiceLine, ZPropertyInfo tariffInfo)
		{
			if (invoiceLine.IsFishNotFromRussia)
			{
				tariffInfo.AddWarning(TariffRequiresNonRussianCertificationDocument);
			}
		}
		internal const string TariffRequiresNonRussianCertificationDocument = "Per Executive Order issued Dec 22, 2023, Tariff Requires Non-Russian Certification document sent via DIS.";

		internal static void ValidateSupplementaryTariffIfEnteredOnChildLineForDerivedSets(JobComInvoiceLine invoiceLine, ZPropertyInfo tariffPropertyInfo)
		{
			if (!tariffPropertyInfo.Value.IsEmpty)
			{
				if (invoiceLine.ParentTariffLine is JobComInvoiceLine parentInvoiceLine && parentInvoiceLine.ImportTariff is USCTariff importTariff && importTariff.UE_DutyComputationCode == ComputationCodeList.Codes.Derived)
				{
					tariffPropertyInfo.AddMessageError(SupplementaryTariffShouldBeEnteredOnParentLineForDerivedSets);
				}
			}
		}
		internal const string SupplementaryTariffShouldBeEnteredOnParentLineForDerivedSets = "Please enter the supplementary tariff on parent invoice line.";
	}
}
