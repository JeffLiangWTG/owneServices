using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.Universal
{
	public struct RateDataToLoad
	{
		public ZString Version;
		public ZString TariffType;
		public ZString TariffCode;
		public ZDateTime TariffStartDate;
		public ZString RateCode;
		public ZGuid RateCodePK;
		public ZDateTime StartDate;
		public ZDateTime? EndDate;
		public ZString Preference;
		public ZGuid PreferencePK;
		public ZString TradeGroup;
		public ZGuid TradeGroupPK;
		public ZString OrderNumber;
		public ZString RateFormula;
		public ZDecimal ADValoremRate;
		public ZDecimal SpecificRate;
		public ZString SpecificRateUOM;
	}

	public class RateViewDataLoad : DataLoadWithFlexibleColumns
	{
		internal static class FieldNames
		{
			public const string Version = nameof(Version);
			public const string TariffType = nameof(TariffType);
			public const string TariffCode = nameof(TariffCode);
			public const string TariffStartDate = nameof(TariffStartDate);
			public const string RateCode = nameof(RateCode);
			public const string StartDate = nameof(StartDate);
			public const string EndDate = nameof(EndDate);
			public const string Preference = nameof(Preference);
			public const string RateFormula = nameof(RateFormula);
			public const string ADValoremRate = nameof(ADValoremRate);
			public const string SpecificRate = nameof(SpecificRate);
			public const string SpecificRateUOM = nameof(SpecificRateUOM);
			public const string TradeGroup = nameof(TradeGroup);
			public const string OrderNumber = nameof(OrderNumber);

			public static readonly ImmutableArray<string> MandatoryFields = new[] { Version, TariffType, TariffCode, TariffStartDate, RateCode, Preference, TradeGroup }.ToImmutableArray();
		}

		static class Constant
		{
			public const string VFD = nameof(VFD);
			public static readonly int CR1_CRT_NKTariffVersionMaxLength = CusRefTariffSchema.CR1_CRT_NKTariffVersion.MaxLength;
			public static readonly int CR1_TariffTypeMaxLength = CusRefTariffSchema.CR1_ZZI_NKTariffType.MaxLength;
			public static readonly int CR1_TariffCodeMaxLength = CusRefTariffSchema.CR1_TariffCode.MaxLength;
			public static readonly int CR7_RateCodeMaxLength = CusRefRateCodeSchema.CR7_RateCode.MaxLength;
			public static readonly int CR8_PreferenceMaxLength = CusRefPreferenceSchema.CR8_Preference.MaxLength;
			public static readonly int CR9_TradeGroupMaxLength = CusRefTradeGroupSchema.CR9_TradeGroup.MaxLength;
			public static readonly int CR4_OrderNumberMaxLength = CusRefApplicabilitySchema.CR4_OrderNumber.MaxLength;
			public static readonly int CR2_RateFormulaMaxLength = CusRefRateSchema.CR2_RateFormula.MaxLength;
			public static readonly int CR5_UnitOfMeasure = CusRefRateUomSchema.CR5_UnitOfMeasure.MaxLength;
		}

		internal static class MessageGenerator
		{
			public static ZString GetEmptyValueError(ZString fieldName) => Res.GetString("9C016968-1B52-4630-90DB-72350E370BA0", "{0} must have value.", fieldName);
			public static ZString GetMustBeNumericError(ZString fieldName, ZString fieldValue) => Res.GetString("B9FA1F0A-8895-4AB8-AD59-6B5CD3068658", "{0} '{1}' must be numeric.", fieldName, fieldValue);
			public static ZString GetNotExistedError(ZString fieldName, ZString fieldValue) => Res.GetString("7DDDBDC3-4B48-482B-A890-163A83964F63", "{0} '{1}' does not exist.", fieldName, fieldValue);
			public static ZString GetNoTariffError(ZString dataGrouping, ZString tariffType, ZString tariffCode, ZString version) => Res.GetString("A79C4520-75A1-49BE-8B7F-EACAEECE38A6", "Tariff (Country/Region={0}, Tariff Type={1}, Tariff Code={2}, Version={3}) does not exist.", dataGrouping, tariffType, tariffCode, version);
			public static ZString GetNoMandatoryFieldsForInsertError() => Res.GetString("478346CD-7366-4705-90C0-0119B3C0A6D6", "One of column Rate Formula or Ad-valorem Rate or Specific Rate and Specific Rate UOM should be existed and have value to insert new rate.");
			public static ZString GetEndDateEarlierThanStartDateError(ZDateTime startDate, ZDateTime endDate) => Res.GetString("F344AE6C-7C7C-4F29-BE11-AEF6570D226D", "End Date {0} is earlier than Start Date {1}.", endDate.ToShortDateString(), startDate.ToShortDateString());
			public static ZString GetStartDateNotEarlierThanFromDateOfTariffError(ZDateTime startDate, ZDateTime startDateOfTariff) => Res.GetString("C959ABBC-489B-46F7-9C30-B791B280A39E", "Start Date {0} cannot be earlier than Tariff's Effective From date {1}.", startDate.ToShortDateString(), startDateOfTariff.ToShortDateString());
			public static ZString GetEndDateNotLaterThanEndDateOfTariffError(ZDateTime endDate, ZDateTime endDateOfTariff) => Res.GetString("E50B184C-3B68-4DA8-84E5-F94A0BA978E4", "End Date {0} cannot be later than Tariff's Effective To date {1}.", endDate.ToShortDateString(), endDateOfTariff.ToShortDateString());
			public static ZString GetNoRateAndApplicabilityByStartDateError(ZDateTime startDate) => Res.GetString("0FC562AA-3527-4784-B202-9410DC7BF772", "There's no matched rate with applicability's start date = {0}.", startDate);
			public static ZString GetRateOverlapsError() => Res.GetString("1706055C-BC4D-4017-A8B9-DE24169B05C3", "The date range of this Rate overlaps with another Rate with same Preference and Rate Code.");
			public static ZString GetRateApplicabilityOverlapsError() => Res.GetString("98992BB7-CD7F-4C18-9FD8-B139654A76ED", "The date range of this Applicability overlaps with another Applicability with same Trade Group and Order.");
			public static ZString GetInvalidUOMError() => Res.GetString("215F0BD3-A71E-44BB-BDA1-6C1E7991F6EF", "The Specific rate UOM should be valid UOM of the rate.");
			public static ZString GetInvalidRateFormulaError() => Res.GetString("8D56E25B-0C14-4C89-9C09-754657931D25", "The rate formula is not valid to be used in rate calculations.");
			public static ZString GetCreateRateMessage(ZString rateCode, ZString preference, ZString tradeGroup, ZString orderNumber, ZString dataGrouping, ZString tariffType, ZString tariffCode, ZString version, ZDateTime tariffStartDate)
				=> Res.GetString("8323AA55-475C-44D4-B2A9-210A38FB0886", "No matched rate found and a new rate (Rate Code={0}, Preference={1}, Trade Group={2}, Order Number={3}) of tariff (Country/Region={4}, Tariff Type={5}, Tariff Code={6}, Version={7}, Start Date={8}) is created.", rateCode, preference, tradeGroup, orderNumber, dataGrouping, tariffType, tariffCode, version, tariffStartDate);
			public static ZString GetUpdateRateMessage(ZString rateCode, ZString preference, ZString tradeGroup, ZString orderNumber, ZString dataGrouping, ZString tariffType, ZString tariffCode, ZString version, ZDateTime tariffStartDate)
				=> Res.GetString("10833D8F-BDF8-4B2D-8C8F-71FFAB43F116", "Matched rate (Rate Code={0}, Preference={1}, Trade Group={2}, Order Number={3}) of tariff (Country/Region={4}, Tariff Type={5}, Tariff Code={6}, Version={7}, Start Date={8}) found and updated.", rateCode, preference, tradeGroup, orderNumber, dataGrouping, tariffType, tariffCode, version, tariffStartDate);
		}

		public void ImportRateData(string dataLocation)
		{
			ImportData(dataLocation, (NoResString)"Rate");
		}

		#region implement abstarct class

		public override string CSVTemplateHeading => string.Join(",", CSVTemplateHeaders);

		string[] CSVTemplateHeaders => Factory.GetCachedValue("CSVTemplateHeading_Rate", () =>
		{
			return new[]
					{
						FieldNames.Version,
						FieldNames.TariffType,
						FieldNames.TariffCode,
						FieldNames.TariffStartDate,
						FieldNames.RateCode,
						FieldNames.StartDate,
						FieldNames.EndDate,
						FieldNames.Preference,
						FieldNames.TradeGroup,
						FieldNames.OrderNumber,
						FieldNames.RateFormula,
						FieldNames.ADValoremRate,
						FieldNames.SpecificRate,
						FieldNames.SpecificRateUOM,
					};
		});

		protected override void ProcessDataForThisLine(OCsvLine line)
		{
			try
			{
				logs = new ZStringBuilder();
				var data = PopulateRateDataToLoad(line);
				ProcessRateData(data);
			}
			finally
			{
				logs = null;
				rateFormulaToUpdate = ZString.Empty;
			}
		}

		ZStringBuilder logs;

		#endregion

		#region override

		protected override bool CheckMandatoryFields(Dictionary<string, bool> headings, string[] headerLineValues)
		{
			var result = true;
			var missingFields = GetMissingMandatoryFields(headerLineValues, FieldNames.MandatoryFields.ToArray());
			if (missingFields.Any())
			{
				DisplayLogMessage(Res.GetString("86AB5586-09F8-4B5B-8B4C-94B3426CB005", "Does not contain mandatory column heading(s): {0}.", string.Join(",", missingFields)));
				result = false;
			}
			return result;
		}

		static HashSet<string> GetMissingMandatoryFields(string[] headings, string[] mandatoryFields)
		{
			var missingFields = new HashSet<string>();
			foreach (var mandatoryField in mandatoryFields)
			{
				if (!headings.Contains(mandatoryField))
				{
					missingFields.Add(mandatoryField);
				}
			}

			return missingFields;
		}

		#endregion

		#region ProcessImportData

		RateDataToLoad PopulateRateDataToLoad(OCsvLine line)
		{
			var record = new RateDataToLoad();
			try
			{
				CheckElementCount(line);

				record.Version = TryGetStringValue(line, FieldNames.Version, Constant.CR1_CRT_NKTariffVersionMaxLength);
				ValidateNoEmptyValue(FieldNames.Version, record.Version);

				record.TariffType = TryGetStringValue(line, FieldNames.TariffType, Constant.CR1_TariffTypeMaxLength);
				ValidateNoEmptyValue(FieldNames.TariffType, record.TariffType);

				record.TariffCode = TryGetStringValue(line, FieldNames.TariffCode, Constant.CR1_TariffCodeMaxLength);
				if (ValidateNoEmptyValue(FieldNames.TariffCode, record.TariffCode) && !record.TariffCode.IsNumbersOnlyOrEmpty)
				{
					logs.Append(MessageGenerator.GetMustBeNumericError(FieldNames.TariffCode, record.TariffCode));
				}

				record.TariffStartDate = TryGetDateValue(line, FieldNames.TariffStartDate) ?? ZDate.Empty;
				ValidateNoEmptyValue(FieldNames.TariffStartDate, record.TariffStartDate);

				record.RateCode = TryGetStringValue(line, FieldNames.RateCode, Constant.CR7_RateCodeMaxLength);
				if (ValidateNoEmptyValue(FieldNames.RateCode, record.RateCode))
				{
					var rateCode = CusRefRateCodeView.Loader.LoadByRateCode(Factory, CurrentCountry, record.RateCode, Core.Constants.Customs.Universal.DataSetTypes.OWNData).FirstOrDefault();
					if (rateCode != null)
					{
						record.RateCodePK = rateCode.PK;
					}
					else
					{
						logs.Append(MessageGenerator.GetNotExistedError(FieldNames.RateCode, record.RateCode));
					}
				}

				record.Preference = TryGetStringValue(line, FieldNames.Preference, Constant.CR8_PreferenceMaxLength);
				if (ValidateNoEmptyValue(FieldNames.Preference, record.Preference))
				{
					var preference = CusRefPreferenceView.Loader.LoadByPreference(Factory, CurrentCountry, record.Preference, Core.Constants.Customs.Universal.DataSetTypes.OWNData);
					if (preference != null)
					{
						record.PreferencePK = preference.PK;
					}
					else
					{
						logs.Append(MessageGenerator.GetNotExistedError(FieldNames.Preference, record.Preference));
					}
				}

				record.TradeGroup = TryGetStringValue(line, FieldNames.TradeGroup, Constant.CR9_TradeGroupMaxLength);
				if (ValidateNoEmptyValue(FieldNames.TradeGroup, record.TradeGroup))
				{
					var tradeGroup = (new CusRefTradeGroupView.Loader(Factory)).Load(CurrentCountry, record.TradeGroup, ZDateTime.Today, Core.Constants.Customs.Universal.DataSetTypes.OWNData);
					if (tradeGroup != null)
					{
						record.TradeGroupPK = tradeGroup.PK;
					}
					else
					{
						logs.Append(MessageGenerator.GetNotExistedError(FieldNames.TradeGroup, record.TradeGroup));
					}
				}

				record.OrderNumber = TryGetStringValue(line, FieldNames.OrderNumber, Constant.CR4_OrderNumberMaxLength);
				record.StartDate = TryGetDateValue(line, FieldNames.StartDate) ?? ZDate.Empty;
				record.EndDate = TryGetDateValue(line, FieldNames.EndDate);
				ValidateStartDateAndEndDate(record.StartDate, record.EndDate);

				record.RateFormula = TryGetStringValue(line, FieldNames.RateFormula, Constant.CR2_RateFormulaMaxLength);
				record.ADValoremRate = TryGetDecimalValue(line, FieldNames.ADValoremRate, 9, 2);
				record.SpecificRate = TryGetDecimalValue(line, FieldNames.SpecificRate, 9, 2);
				record.SpecificRateUOM = TryGetStringValue(line, FieldNames.SpecificRateUOM, Constant.CR5_UnitOfMeasure);

				return record;
			}
			catch (ArgumentException ex)
			{
				logs.Append(ex.Message);
				return record;
			}
		}

		void ProcessRateData(RateDataToLoad dataToLoad)
		{
			if (logs.IsEmpty)
			{
				try
				{
					var hasUpdated = false;
					var isCreated = false;
					var matchedVersion = Factory.LoadTop1<ICusRefTariffVersion>(new ZQuery(CusRefTariffVersionSchema.CRT_Version, dataToLoad.Version));
					if (matchedVersion != null)
					{
						var matchedTariff = GetTariffIfExists(CurrentCountry, dataToLoad.TariffType, dataToLoad.TariffCode, dataToLoad.Version, dataToLoad.TariffStartDate);
						if (matchedTariff != null)
						{
							var startDate = GetStartDateToUpdate(matchedTariff, dataToLoad);
							var endDate = GetEndDateToUpdate(matchedTariff, dataToLoad);
							if (ValidateRateDate(matchedTariff, startDate, endDate))
							{
								var matchedRates = GetRatesIfExists(matchedTariff, dataToLoad);
								if (!matchedRates.Any())
								{
									if (HasValidValueToInsert(dataToLoad, matchedTariff))
									{
										UpdateRateDetails(matchedTariff, null, null, dataToLoad);
										RunCounters.RecsCreated++;
										isCreated = true;
										logs.Append(MessageGenerator.GetCreateRateMessage(dataToLoad.RateCode, dataToLoad.Preference, dataToLoad.TradeGroup, dataToLoad.OrderNumber, CurrentCountry, dataToLoad.TariffType, dataToLoad.TariffCode, dataToLoad.Version, dataToLoad.TariffStartDate));
									}
								}
								else
								{
									var (matchedRate, applicability) = GetRateAndApplicabilityByStartDate(matchedRates, startDate);
									if (matchedRate == null)
									{
										logs.Append(MessageGenerator.GetNoRateAndApplicabilityByStartDateError(startDate));
									}
									else if (HasValidValueToUpdate(dataToLoad, matchedTariff, matchedRate, applicability))
									{
										UpdateRateDetails(matchedTariff, matchedRate, applicability, dataToLoad);
										RunCounters.RecsUpdated++;
										hasUpdated = true;
										logs.Append(MessageGenerator.GetUpdateRateMessage(dataToLoad.RateCode, dataToLoad.Preference, dataToLoad.TradeGroup, dataToLoad.OrderNumber, CurrentCountry, dataToLoad.TariffType, dataToLoad.TariffCode, dataToLoad.Version, dataToLoad.TariffStartDate));
									}
								}
							}
						}
						else
						{
							logs.Append(MessageGenerator.GetNoTariffError(CurrentCountry, dataToLoad.TariffType, dataToLoad.TariffCode, dataToLoad.Version));
						}
					}
					else
					{
						logs.Append(MessageGenerator.GetNotExistedError(FieldNames.Version, dataToLoad.Version));
					}

					UpdateRunCounters(hasUpdated, isCreated);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					RunCounters.RecsExcluded++;
					DisplayLogMessage($"Row {RunCounters.CurrentRow} excluded: there is an exception {ex.Message}");
				}
			}
			else
			{
				RunCounters.RecsExcluded++;
				DisplayLogMessage(Res.GetString("AD8D67A3-A27A-48FF-8F02-15F0B6021795", "Row {0} excluded: {1}", RunCounters.CurrentRow.ToString(), logs.ToString()));
			}
		}

		static ZDateTime GetStartDateToUpdate(TariffView tariff, RateDataToLoad dataToLoad) => dataToLoad.StartDate.IsEmpty && tariff != null ? tariff.ZZ1_StartDate : dataToLoad.StartDate;

		static ZDateTime? GetEndDateToUpdate(TariffView tariff, RateDataToLoad dataToLoad) => dataToLoad.EndDate.HasValue && dataToLoad.EndDate.Value.IsEmpty && tariff != null ? tariff.ZZ1_EndDate : dataToLoad.EndDate;

		void UpdateRateDetails(TariffView tariff, RateView rate, CusRefApplicabilityView rateApplicability, RateDataToLoad dataToLoad)
		{
			var isNewRate = rate == null;
			if (isNewRate)
			{
				rate = CreateRate(tariff, dataToLoad);
			}

			if (!rateFormulaToUpdate.IsEmpty)
			{
				rate.ZZ2_RateFormula = rateFormulaToUpdate;
			}

			if (dataToLoad.EndDate.HasValue && !dataToLoad.EndDate.Value.IsEmpty)
			{
				if (rateApplicability != null)
				{
					rateApplicability.ZZT_EndDate = dataToLoad.EndDate.Value;
				}
				if (!rate.RateApplicabilities.Any(x => !x.ZZT_IsSystem && x.ZZT_EndDate < dataToLoad.EndDate.Value))
				{
					rate.ZZ2_EndDate = dataToLoad.EndDate.Value;
				}
			}
		}

		void UpdateRunCounters(bool hasUpdated, bool isCreated)
		{
			if (hasUpdated || isCreated)
			{
				RunCounters.RecsToUpdate++;
				if (isCreated)
				{
					DisplayLogMessage(Res.GetString("020B055B-E52B-484E-9540-32EA09ECE0C1", "Row {0} created: {1}", RunCounters.CurrentRow.ToString(), logs.ToString()));
				}
				else
				{
					DisplayLogMessage(Res.GetString("56EBC20E-29AC-460B-AA08-D67F6C564CB4", "Row {0} updated: {1}", RunCounters.CurrentRow.ToString(), logs.ToString()));
				}
			}
			else
			{
				RunCounters.RecsExcluded++;
				DisplayLogMessage(Res.GetString("1E8B83BC-254A-4B56-8CE3-D760C1BA2FBE", "Row {0} excluded: {1}", RunCounters.CurrentRow.ToString(), logs.ToString()));
			}
		}

		RateView CreateRate(TariffView tariff, RateDataToLoad dataToLoad)
		{
			var newRate = Factory.New<RateView>();
			newRate.ZZ2_ZZ1_ParentTariffOrNationalCode = tariff.PK;
			newRate.ZZ2_ZY1_RateCode = dataToLoad.RateCodePK;
			newRate.ZZ2_ZZS_Preference = dataToLoad.PreferencePK;

			newRate.ZZ2_StartDate = GetStartDateToUpdate(tariff, dataToLoad);
			newRate.ZZ2_EndDate = GetEndDateToUpdate(tariff, dataToLoad) ?? ZDate.Empty;

			CreateCusRefApplicability(newRate, dataToLoad);
			return newRate;
		}

		void CreateCusRefApplicability(RateView rate, RateDataToLoad dataToLoad)
		{
			var newRateApplicability = Factory.New<CusRefApplicabilityView>();
			newRateApplicability.ZZT_ZZ2_Rate = rate.PK;
			newRateApplicability.ZZT_ZZA_TradeGroup = dataToLoad.TradeGroupPK;
			newRateApplicability.ZZT_StartDate = rate.ZZ2_StartDate;
			newRateApplicability.ZZT_EndDate = rate.ZZ2_EndDate;
			newRateApplicability.ZZT_OrderNumber = dataToLoad.OrderNumber;
		}

		TariffView GetTariffIfExists(ZString countryCode, ZString tariffType, ZString tariffCode, ZString version, ZDateTime startDate)
		{
			var tariffLoader = new TariffView.Loader(Factory);
			var matchedTariff = tariffLoader.LoadTariffByVersion(countryCode, tariffType, tariffCode, version, startDate);

			return matchedTariff;
		}

		static IEnumerable<RateView> GetRatesIfExists(TariffView tariff, RateDataToLoad dataToLoad)
		{
			var rates = tariff.Rates.Where(x => !x.ZZ2_IsSystem && x.RateCode == dataToLoad.RateCode && IsEffectiveDateRange(x.ZZ2_StartDate, x.ZZ2_EndDate)
											&& x.RateApplicabilities.Any(a => !a.ZZT_IsSystem && a.ZZT_OrderNumber == dataToLoad.OrderNumber && a.ZZT_ZZA_TradeGroup == dataToLoad.TradeGroupPK && IsEffectiveDateRange(a.ZZT_StartDate, a.ZZT_EndDate)));
			return rates;
		}

		static bool IsEffectiveDateRange(ZDateTime startDate, ZDateTime endDate) => startDate <= ZDateTime.Today && (endDate.IsEmpty || endDate >= ZDateTime.Today);

		static (RateView, CusRefApplicabilityView) GetRateAndApplicabilityByStartDate(IEnumerable<RateView> rates, ZDateTime startDate)
		{
			foreach (var rate in rates)
			{
				var applicability = rate.RateApplicabilities.FirstOrDefault(a => a.ZZT_StartDate == startDate && !a.ZZT_IsSystem);
				if (applicability != null)
				{
					return (rate, applicability);
				}
			}

			return (null, null);
		}

		#region Validation

		bool HasValidValueToInsert(RateDataToLoad dataToLoad, TariffView tariff)
		{
			var hasValidValueToInsert = CheckMandatoryFieldsForInsert(dataToLoad);
			if (hasValidValueToInsert)
			{
				hasValidValueToInsert = HasValidValueToUpdate(dataToLoad, tariff, null, null);
			}

			return hasValidValueToInsert;
		}

		bool HasValidValueToUpdate(RateDataToLoad dataToLoad, TariffView tariff, RateView rate, CusRefApplicabilityView applicability)
		{
			var startDate = GetStartDateToUpdate(tariff, dataToLoad);
			return ValidRateCode(tariff, rate, dataToLoad.PreferencePK, dataToLoad.RateCodePK, startDate, dataToLoad.EndDate)
				&& ValidRateApplicability(tariff, rate, applicability, dataToLoad.TradeGroupPK, dataToLoad.OrderNumber, startDate, dataToLoad.EndDate)
				&& GetRateFormulaToUpdate(rate, dataToLoad, out rateFormulaToUpdate);
		}
		ZString rateFormulaToUpdate;

		bool CheckMandatoryFieldsForInsert(RateDataToLoad dataToLoad)
		{
			var result = false;

			if (!dataToLoad.RateFormula.IsEmpty || !dataToLoad.ADValoremRate.IsEmpty || !dataToLoad.SpecificRate.IsEmpty && !dataToLoad.SpecificRateUOM.IsEmpty)
			{
				result = true;
			}
			else
			{
				logs.Append(MessageGenerator.GetNoMandatoryFieldsForInsertError());
			}

			return result;
		}

		bool ValidateNoEmptyValue<T>(ZString fieldName, T fieldValue) where T : IZType
		{
			if (fieldValue.IsEmpty)
			{
				logs.Append(MessageGenerator.GetEmptyValueError(fieldName));
			}

			return !fieldValue.IsEmpty;
		}

		bool ValidateRateDate(TariffView tariff, ZDateTime startDate, ZDateTime? endDate)
		{
			var isValidStartDate = ValidateStartDateNotEarlierThanFromDateOfTariff(startDate, tariff.ZZ1_StartDate);
			var isValidEndDate = ValidateEndDateNotLaterThanEndDateOfTariff(endDate, tariff.ZZ1_EndDate);
			var isValidRateDate = ValidateStartDateAndEndDate(startDate, endDate);

			return isValidStartDate && isValidEndDate && isValidRateDate;
		}

		bool ValidateStartDateAndEndDate(ZDateTime startDate, ZDateTime? endDate)
		{
			var isValid = true;
			if (endDate.HasValue && endDate.Value < startDate)
			{
				logs.Append(MessageGenerator.GetEndDateEarlierThanStartDateError(startDate, endDate.Value));
				isValid = false;
			}
			return isValid;
		}

		bool ValidateStartDateNotEarlierThanFromDateOfTariff(ZDateTime startDate, ZDateTime startDateOfTariff)
		{
			var isValid = true;
			if (!startDate.IsEmpty && startDate < startDateOfTariff)
			{
				logs.Append(MessageGenerator.GetStartDateNotEarlierThanFromDateOfTariffError(startDate, startDateOfTariff));
				isValid = false;
			}
			return isValid;
		}

		bool ValidateEndDateNotLaterThanEndDateOfTariff(ZDateTime? endDate, ZDateTime endDateOfTariff)
		{
			var isValid = true;
			if (endDate.HasValue && endDate.Value > endDateOfTariff)
			{
				logs.Append(MessageGenerator.GetEndDateNotLaterThanEndDateOfTariffError(endDate.Value, endDateOfTariff));
				isValid = false;
			}
			return isValid;
		}

		bool ValidRateCode(TariffView tariff, RateView rate, ZGuid preferencePK, ZGuid rateCodePK, ZDateTime startDate, ZDateTime? endDate)
		{
			bool isValid = true;
			if (tariff != null && rate != null && rateCodePK.IsValid && preferencePK.IsValid && startDate.IsValid)
			{
				var ratesWithSamePreferenceAndRateCode = tariff.Rates.Where(x => x.ZZ2_ZZS_Preference == preferencePK && x.ZZ2_ZY1_RateCode == rateCodePK && x.PK != rate.PK).ToArray();
				if (ratesWithSamePreferenceAndRateCode.Any(x => (startDate < x.ZZ2_StartDate && (!endDate.HasValue || endDate.Value.IsEmpty || endDate.Value >= x.ZZ2_StartDate))
																	|| (startDate <= x.ZZ2_EndDate && (!endDate.HasValue || endDate.Value.IsEmpty || endDate.Value >= x.ZZ2_EndDate))))
				{
					logs.Append(MessageGenerator.GetRateOverlapsError());
					isValid = false;
				}
			}

			return isValid;
		}

		bool ValidRateApplicability(TariffView tariff, RateView rate, CusRefApplicabilityView applicability, ZGuid tradeGroupPK, ZString orderNumber, ZDateTime startDate, ZDateTime? endDate)
		{
			bool isValid = true;
			if (tariff != null && rate != null && tradeGroupPK.IsValid && startDate.IsValid)
			{
				var applicabilitiesWithSameTradeGroupAndOrderNumber = rate.RateApplicabilities.Where(x => x.ZZT_ZZA_TradeGroup == tradeGroupPK && x.ZZT_OrderNumber == orderNumber && (applicability == null || x.PK != applicability.PK)).ToArray();
				if (applicabilitiesWithSameTradeGroupAndOrderNumber.Any(x => (startDate < x.ZZT_StartDate && (!endDate.HasValue || endDate.Value.IsEmpty || endDate.Value >= x.ZZT_StartDate))
																				|| (startDate <= x.ZZT_EndDate && (!endDate.HasValue || endDate.Value.IsEmpty || endDate.Value >= x.ZZT_EndDate))))
				{
					logs.Append(MessageGenerator.GetRateApplicabilityOverlapsError());
					isValid = false;
				}
			}

			return isValid;
		}

		bool GetRateFormulaToUpdate(RateView rate, RateDataToLoad dataToLoad, out ZString rateFormula)
		{
			var isValidRateFormula = true;
			rateFormula = dataToLoad.RateFormula;
			if (rateFormula.IsEmpty)
			{
				if (!dataToLoad.ADValoremRate.IsEmpty)
				{
					rateFormula = dataToLoad.ADValoremRate + "*" + Constant.VFD;
				}
				else if (!dataToLoad.SpecificRate.IsEmpty && !dataToLoad.SpecificRateUOM.IsEmpty)
				{
					isValidRateFormula = ValidateUOM(rate, dataToLoad.SpecificRateUOM);
					if (isValidRateFormula)
					{
						rateFormula = dataToLoad.SpecificRate + "*[" + dataToLoad.SpecificRateUOM + "]";
					}
				}
			}
			else
			{
				var errors = UniversalRateCalculator.GetRateFormulaParseError(rateFormula, out _);
				if (!errors.IsEmpty)
				{
					isValidRateFormula = false;
					logs.Append(MessageGenerator.GetInvalidRateFormulaError());
				}
			}

			return isValidRateFormula;
		}

		bool ValidateUOM(RateView rate, ZString uom)
		{
			var result = rate != null && rate.UnitsOfMeasure.Any(x => x.ZXG_UOM == uom);
			if (!result)
			{
				logs.Append(MessageGenerator.GetInvalidUOMError());
			}

			return result;
		}

		#endregion

		static ZString CurrentCountry => GlbCompany.CurrentCompany.Country.Code;

		#endregion
	}
}
