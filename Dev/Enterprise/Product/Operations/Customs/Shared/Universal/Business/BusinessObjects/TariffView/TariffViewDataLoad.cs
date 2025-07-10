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
	public struct TariffDataToLoad
	{
		public ZString Version;
		public ZString TariffType;
		public ZString TariffCode;
		public ZString Description;
		public ZDateTime StartDate;
		public ZDateTime? EndDate;
		public ZString TaxOrFeeCode;
		public ZString CountryCode;
		public ZString UOM1;
		public ZString UOM2;
		public ZString UOM3;
	}

	public class TariffViewDataLoad : DataLoadWithFlexibleColumns
	{
		internal static class FieldNames
		{
			public const string Version = nameof(Version);
			public const string TariffType = nameof(TariffType);
			public const string TariffCode = nameof(TariffCode);
			public const string Description = nameof(Description);
			public const string StartDate = nameof(StartDate);
			public const string EndDate = nameof(EndDate);
			public const string TaxOrFeeCode = nameof(TaxOrFeeCode);
			public const string CountryCode = nameof(CountryCode);
			public const string UOM1 = nameof(UOM1);
			public const string UOM2 = nameof(UOM2);
			public const string UOM3 = nameof(UOM3);

			public static readonly ImmutableArray<string> MandatoryFields = new[] { Version, TariffType, TariffCode, CountryCode }.ToImmutableArray();
			public static readonly ImmutableArray<string> MandatoryFieldsForInsert = new[] { Description, UOM1 }.ToImmutableArray();
		}

		internal static class Constant
		{
			public const string VAT = nameof(VAT);
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "date description")]
			public const string StartDate = "start date";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "date description")]
			public const string VersionEffectiveDate = "the tariff version's effective date";
			public static readonly int CR1_CRT_NKTariffVersionMaxLength = CusRefTariffSchema.CR1_CRT_NKTariffVersion.MaxLength;
			public static readonly int CR1_DescriptionMaxLength = CusRefTariffSchema.CR1_Description.MaxLength;
			public static readonly int CR1_RN_NKCountryCodeMaxLength = CusRefTariffSchema.CR1_RN_NKCountryCode.MaxLength;
			public static readonly int CR1_TariffCodeMaxLength = CusRefTariffSchema.CR1_TariffCode.MaxLength;
			public static readonly int CR1_ZZF_NKTaxOrFeeCodeMaxLength = CusRefTariffSchema.CR1_ZZF_NKTaxOrFeeCode.MaxLength;
		}

		internal static class MessageGenerator
		{
			public static ZString GetEmptyVersionError() => Res.GetString("9898C220-AA46-4D7F-B1DE-3F03F9ED4A86", "Version must have value.");

			public static ZString GetInvalidTariffTypeError(ZString tariffType)
				=> Res.GetString("4B25EC4A-B096-46D5-8DF9-5038DCF6F7F6", "Tariff Type '{0}' should be '{1}'.", tariffType, Constants.TariffTypes.HarmonizedSystem);

			public static ZString GetInvalidTariffCodeError(ZString tariffCode)
				=> Res.GetString("6EE957A0-6C4B-4447-8CCA-8B9B50A57F25", "Tariff Code '{0}' must have value and be numeric.", tariffCode);

			public static ZString GetInvalidCountryCodeError(ZString countryCode)
				=> Res.GetString("AF8B91AF-9429-4603-BC55-6B684234825E", "Country/Region Code '{0}' must have value and be 2 characters.", countryCode);

			public static ZString GetNoMandatoryColumnsError(HashSet<string> missingFields)
				=> Res.GetString("8A025321-B115-46F9-BC5F-846F574E8331", "Does not contain mandatory column heading(s): {0}.", string.Join(",", missingFields));

			public static ZString GetNotExistedError(ZString fieldName, ZString fieldValue)
				=> Res.GetString("B11ED643-339F-4C5C-86CA-EB6A11F7126C", "{0} '{1}' does not exist.", fieldName, fieldValue);

			public static ZString GetEmptyValueForInsertError(HashSet<ZString> emptyFields)
				=> Res.GetString("1AEC6C64-DFD5-4AFF-BA35-61320FDCBCE2", "The columns should not be empty to insert new tariff: {0}.", string.Join(",", emptyFields));

			public static ZString GetDuplicateTariffError(ZDateTime startDate, ZDateTime endDate, ZString tariffType, ZString tariffCode, ZString dataGrouping, ZString version)
				=> Res.GetString("CC2E1432-E28F-4DF4-94D8-4FAE555D45DF", "The update of this tariff (start date '{0}' and end date '{1}') would result in a duplicate for another Tariff with tariff type '{2}' and code '{3}' and country '{4}' and version '{5}.",
					startDate.ToShortDateString(), endDate.ToShortDateString(), tariffType, tariffCode, dataGrouping, version);

			public static ZString GetDateOverlapError(ZDateTime startDate, ZDateTime endDate, ZString tariffType, ZString tariffCode, ZString dataGrouping, ZString version)
				=> Res.GetString("7D76CECD-5656-464F-976A-3EB21DB79FDA", "The date range of this tariff (start date '{0}' and end date '{1}') overlaps with another tariff with tariff type '{2}' and code '{3}' and country '{4}' and version '{5}'.",
					startDate.ToShortDateString(), endDate.ToShortDateString(), tariffType, tariffCode, dataGrouping, version);

			public static ZString GetEndDateError(ZDateTime endDate, ZString dateDescription, ZDateTime date)
				=> Res.GetString("0E3BA3A5-60C1-4E9B-A875-328C498425D8", "End Date {0} is earlier than {1} {2}.", endDate.ToShortDateString(), dateDescription, date.ToShortDateString());

			public static ZString GetInvalidVATRateCodeError(ZString code, ZString dataGrouping)
				=> Res.GetString("94F0DB77-BDD7-4D2C-AB8E-C11740CDDB2B", "Tax or Fee Code {0} is not valid VAT rate code for {1}.", code, dataGrouping);

			public static ZString GetNoUOM1Error() => Res.GetString("D4056BC1-058E-4EC0-A6BE-9EADD02D5E28", "There has to be a UOM1 value when the tariff has UOM2 value.");

			public static ZString GetNoUOM2Error() => Res.GetString("0C88BAE6-0A99-4206-ADBD-7ABD07930B24", "There has to be a UOM2 value when the tariff has UOM3 value.");

			public static ZString GetEmptyUOMsError() => Res.GetString("9A1FD534-A49D-4BCA-8969-10E049B24016", "Tariff UOM1/UOM2/UOM3 cannot be all empty and it will delete the existing tariff UOMs.");

			public static ZString GetDuplicateUOMsError() => Res.GetString("02F54C38-CACB-4E16-80C5-3473C930FA73", "Tariff UOM1/UOM2/UOM3 value must be unique.");

			public static ZString GetStarDateUpdateMessage(ZDateTime versionEffectiveDate)
				=> Res.GetString("0775D2A1-CEC7-45A1-843A-111ED7D2DCB8", "Start Date field is empty or not provided then default as version effective date {0}.", versionEffectiveDate.ToShortDateString());

			public static ZString GetCreateTariffMessage(ZString dataGrouping, ZString tariffType, ZString tariffCode, ZString version, ZDateTime tariffStartDate)
				=> Res.GetString("0AF72E72-B613-4D2A-A9DE-C0E95407B731", "No matched tariff found and a new tariff (Country/Region={0}, Tariff Type={1}, Tariff Code={2}, Version={3}, Start Date={4}) is created.", dataGrouping, tariffType, tariffCode, version, tariffStartDate);

			public static ZString GetUpdateTariffMessage(ZString dataGrouping, ZString tariffType, ZString tariffCode, ZString version, ZDateTime tariffStartDate)
				=> Res.GetString("0AF719B4-4FA8-4633-A873-8CDFE4B54F9C", "Matched tariff (Country/Region={0}, Tariff Type={1}, Tariff Code={2}, Version={3}, Start Date={4}) found and updated.", dataGrouping, tariffType, tariffCode, version, tariffStartDate);
		}

		public void ImportTariffData(string dataLocation)
		{
			ImportData(dataLocation, (NoResString)"Tariff");
		}

		#region implement abstarct class

		public override string CSVTemplateHeading => string.Join(",", CSVTemplateHeaders);

		string[] CSVTemplateHeaders => Factory.GetCachedValue("CSVTemplateHeading_Tariff", () =>
		{
			return new[]
					{
						FieldNames.Version,
						FieldNames.TariffType,
						FieldNames.TariffCode,
						FieldNames.Description,
						FieldNames.StartDate,
						FieldNames.EndDate,
						FieldNames.TaxOrFeeCode,
						FieldNames.CountryCode,
						FieldNames.UOM1,
						FieldNames.UOM2,
						FieldNames.UOM3,
					};
		});

		protected override void ProcessDataForThisLine(OCsvLine line)
		{
			try
			{
				logs = new ZStringBuilder();
				var data = PopulateTariffDataToLoad(line);
				ProcessTariffData(data);
			}
			finally
			{
				logs = null;
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
				DisplayLogMessage(MessageGenerator.GetNoMandatoryColumnsError(missingFields));
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

		TariffDataToLoad PopulateTariffDataToLoad(OCsvLine line)
		{
			var record = new TariffDataToLoad();
			try
			{
				CheckElementCount(line);

				record.Version = TryGetStringValue(line, FieldNames.Version, Constant.CR1_CRT_NKTariffVersionMaxLength);
				if (record.Version.IsEmpty)
				{
					logs.Append(MessageGenerator.GetEmptyVersionError());
				}

				record.TariffType = TryGetStringValue(line, FieldNames.TariffType);
				if (record.TariffType.IsEmpty)
				{
					record.TariffType = Constants.TariffTypes.HarmonizedSystem;
				}
				else if (record.TariffType != Constants.TariffTypes.HarmonizedSystem)
				{
					logs.Append(MessageGenerator.GetInvalidTariffTypeError(record.TariffType));
				}

				record.TariffCode = TryGetStringValue(line, FieldNames.TariffCode, Constant.CR1_TariffCodeMaxLength);
				if (record.TariffCode.IsEmpty || !record.TariffCode.IsNumbersOnlyOrEmpty)
				{
					logs.Append(MessageGenerator.GetInvalidTariffCodeError(record.TariffCode));
				}

				record.CountryCode = TryGetStringValue(line, FieldNames.CountryCode, Constant.CR1_RN_NKCountryCodeMaxLength);
				if (record.CountryCode.Length != 2)
				{
					logs.Append(MessageGenerator.GetInvalidCountryCodeError(record.CountryCode));
				}

				record.Description = TryGetStringValue(line, FieldNames.Description, Constant.CR1_DescriptionMaxLength);

				record.StartDate = TryGetDateValue(line, FieldNames.StartDate) ?? ZDateTime.Empty;

				record.EndDate = TryGetDateValue(line, FieldNames.EndDate);
				ValidateEndDate(record.EndDate, record.StartDate, Constant.StartDate);

				record.TaxOrFeeCode = TryGetStringValue(line, FieldNames.TaxOrFeeCode, Constant.CR1_ZZF_NKTaxOrFeeCodeMaxLength);
				record.UOM1 = TryGetStringValue(line, FieldNames.UOM1, AutoTariffUOMView.Schema.ZZ8_UOMMaxLength);
				record.UOM2 = TryGetStringValue(line, FieldNames.UOM2, AutoTariffUOMView.Schema.ZZ8_UOMMaxLength);
				record.UOM3 = TryGetStringValue(line, FieldNames.UOM3, AutoTariffUOMView.Schema.ZZ8_UOMMaxLength);
				var uoms = new[] { record.UOM1, record.UOM2, record.UOM3 };
				var nonEmptyUOMs = uoms.Where(x => !x.IsEmpty);
				if (nonEmptyUOMs.Any() && nonEmptyUOMs.Distinct().Count() != nonEmptyUOMs.Count())
				{
					logs.Append(MessageGenerator.GetDuplicateUOMsError());
				}

				return record;
			}
			catch (ArgumentException ex)
			{
				logs.Append(ex.Message);
				return record;
			}
		}

		void ProcessTariffData(TariffDataToLoad dataToLoad)
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
						if (dataToLoad.StartDate.IsEmpty)
						{
							dataToLoad.StartDate = matchedVersion.CRT_EffectiveDate;
							logs.Append(MessageGenerator.GetStarDateUpdateMessage(matchedVersion.CRT_EffectiveDate));
						}

						var matchedTariff = GetExistingTariff(dataToLoad.CountryCode, dataToLoad.TariffType, dataToLoad.TariffCode, dataToLoad.Version, dataToLoad.StartDate);
						if (matchedTariff == null)
						{
							var missingFields = GetMissingMandatoryFields(ColumnNames.ToArray(), FieldNames.MandatoryFieldsForInsert.ToArray());
							if (missingFields.Any())
							{
								logs.Append(MessageGenerator.GetNoMandatoryColumnsError(missingFields));
							}
							else if (HasValidValueToInsert(dataToLoad, matchedVersion) && ValidateUOM2(dataToLoad.UOM2, dataToLoad.UOM3))
							{
								UpdateTariffDetails(null, dataToLoad);
								RunCounters.RecsCreated++;
								hasUpdated = true;
								isCreated = true;
								logs.Append(MessageGenerator.GetCreateTariffMessage(dataToLoad.CountryCode, dataToLoad.TariffType, dataToLoad.TariffCode, dataToLoad.Version, dataToLoad.StartDate));
							}
						}
						else
						{
							if (HasValidValueToUpdate(dataToLoad, matchedVersion, matchedTariff) && ValidateUOMs(matchedTariff, dataToLoad))
							{
								UpdateTariffDetails(matchedTariff, dataToLoad);
								RunCounters.RecsUpdated++;
								hasUpdated = true;
								logs.Append(MessageGenerator.GetUpdateTariffMessage(dataToLoad.CountryCode, dataToLoad.TariffType, dataToLoad.TariffCode, dataToLoad.Version, dataToLoad.StartDate));
							}
						}
					}
					else
					{
						logs.Append(MessageGenerator.GetNotExistedError(FieldNames.Version, dataToLoad.Version));
					}

					if (!hasUpdated)
					{
						RunCounters.RecsExcluded++;
						DisplayLogMessage(Res.GetString("2D7BAC82-5DF7-4BCC-9A65-A8D423F7BDB6", "Row {0} excluded: {1}", RunCounters.CurrentRow.ToString(), logs.ToString()));
					}
					else
					{
						RunCounters.RecsToUpdate++;
						if (isCreated)
						{
							DisplayLogMessage(Res.GetString("7A1EC885-4141-466F-9539-A83B7B108003", "Row {0} created: {1}", RunCounters.CurrentRow.ToString(), logs.ToString()));
						}
						else
						{
							DisplayLogMessage(Res.GetString("3EF2A849-E18C-4E41-83D7-6079C40ADC07", "Row {0} updated: {1}", RunCounters.CurrentRow.ToString(), logs.ToString()));
						}
					}
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
				DisplayLogMessage(Res.GetString("330C1EEC-1E6C-4B71-9AAB-DDB721E53DFD", "Row {0} excluded: {1}", RunCounters.CurrentRow.ToString(), logs.ToString()));
			}
		}

		void UpdateTariffDetails(TariffView tariff, TariffDataToLoad dataToLoad)
		{
			var isNewTariff = tariff == null;
			if (isNewTariff)
			{
				tariff = CreateTariff(dataToLoad);
			}

			if (dataToLoad.EndDate.HasValue && !dataToLoad.EndDate.Value.IsEmpty)
			{
				tariff.ZZ1_EndDate = dataToLoad.EndDate.Value;
			}

			if (!dataToLoad.TaxOrFeeCode.IsEmpty)
			{
				tariff.ZZ1_ZZF_NKTaxOrFeeCode = dataToLoad.TaxOrFeeCode;
			}

			if (!dataToLoad.Description.IsEmpty)
			{
				tariff.ZZ1_Description = dataToLoad.Description;
			}

			var uomLoader = new TariffUOMView.Loader(Factory);
			UpdateTariffUOM(uomLoader, FieldNames.UOM1, tariff.PK, UOMTypeList.Codes.CU1, dataToLoad.UOM1);
			UpdateTariffUOM(uomLoader, FieldNames.UOM2, tariff.PK, UOMTypeList.Codes.CU2, dataToLoad.UOM2);
			UpdateTariffUOM(uomLoader, FieldNames.UOM3, tariff.PK, UOMTypeList.Codes.CU3, dataToLoad.UOM3);
		}

		TariffView GetExistingTariff(ZString dataGrouping, ZString tariffType, ZString tariffCode, ZString version, ZDateTime starEDate)
		{
			var tariffLoader = new TariffView.Loader(Factory);
			var matchedTariff = tariffLoader.LoadTariffByVersion(dataGrouping, tariffType, tariffCode, version, starEDate);

			return matchedTariff;
		}

		TariffView CreateTariff(TariffDataToLoad dataToLoad)
		{
			var newTariff = Factory.New<TariffView>();
			newTariff.ZZ1_ZZI_NKTariffType = dataToLoad.TariffType;
			newTariff.ZZ1_TariffCode = dataToLoad.TariffCode;
			newTariff.ZZ1_ZZZ_NKDataGrouping = dataToLoad.CountryCode;
			newTariff.ZZ1_CRT_NKTariffVersion = dataToLoad.Version;
			newTariff.ZZ1_StartDate = dataToLoad.StartDate;
			newTariff.ZZ1_EndDate = new ZDateTime(9999, 12, 31);
			return newTariff;
		}

		bool HasValidValueToInsert(TariffDataToLoad dataToLoad, ICusRefTariffVersion version)
		{
			bool hasValidValueToInsert;
			var emptyFields = new HashSet<ZString>();

			if (dataToLoad.Description.IsEmpty)
			{
				emptyFields.Add(FieldNames.Description);
			}

			if (dataToLoad.UOM1.IsEmpty)
			{
				emptyFields.Add(FieldNames.UOM1);
			}

			if (emptyFields.Any())
			{
				logs.Append(MessageGenerator.GetEmptyValueForInsertError(emptyFields));
				hasValidValueToInsert = false;
			}
			else
			{
				hasValidValueToInsert = HasValidValueToUpdate(dataToLoad, version, null);
			}

			return hasValidValueToInsert;
		}

		bool HasValidValueToUpdate(TariffDataToLoad dataToLoad, ICusRefTariffVersion version, TariffView tariff)
		{
			var isValidTaxOrFeeCode = ValidateTaxOrFeeCode(dataToLoad.TaxOrFeeCode, dataToLoad.CountryCode);
			var isValidEndDate = ValidateEndDate(dataToLoad.EndDate, version.CRT_EffectiveDate, Constant.VersionEffectiveDate);

			var hasValidValueToUpdate = false;
			if (isValidTaxOrFeeCode && isValidEndDate)
			{
				var startDate = dataToLoad.StartDate;
				var endDate = GetEndDateToUpdate(dataToLoad.EndDate, tariff);
				if (ValidateUniqueTariff(tariff, dataToLoad.TariffType, dataToLoad.TariffCode, dataToLoad.CountryCode, dataToLoad.Version, startDate, endDate))
				{
					hasValidValueToUpdate = ValidateNoTariffDateOverlap(tariff, dataToLoad.TariffType, dataToLoad.TariffCode, dataToLoad.CountryCode, dataToLoad.Version, startDate, endDate);
				}
			}
			return hasValidValueToUpdate;
		}

		ZDateTime? GetEndDateToUpdate(ZDateTime? endDate, TariffView tariff)
		{
			ZDateTime? endDateToUpdate;
			if (endDate.HasValue && !endDate.Value.IsEmpty)
			{
				endDateToUpdate = endDate;
			}
			else if (tariff == null)
			{
				endDateToUpdate = new ZDateTime(9999, 12, 31);
			}
			else
			{
				endDateToUpdate = tariff.ZZ1_EndDate;
			}
			return endDateToUpdate;
		}

		bool ValidateUniqueTariff(TariffView tariff, ZString type, ZString code, ZString dataGrouping, ZString version, ZDateTime startDate, ZDateTime? endDate)
		{
			var isValid = true;
			if (!type.IsEmpty && !dataGrouping.IsEmpty && !code.IsEmpty && !version.IsEmpty && startDate.IsValid && endDate.HasValue && !endDate.Value.IsEmpty
				&& Factory.Load<TariffView>(TariffView.Loader.GetDuplicateManualTariffFilter(tariff, type, dataGrouping, code, version, startDate, endDate.Value)).Length > 0)
			{
				logs.Append(MessageGenerator.GetDuplicateTariffError(startDate, endDate.Value, type, code, dataGrouping, version));
				isValid = false;
			}

			return isValid;
		}

		bool ValidateNoTariffDateOverlap(TariffView tariff, ZString type, ZString code, ZString dataGrouping, ZString version, ZDateTime startDate, ZDateTime? endDate)
		{
			var isValid = true;
			if (!type.IsEmpty && !dataGrouping.IsEmpty && !code.IsEmpty && !version.IsEmpty && startDate.IsValid && endDate.HasValue && !endDate.Value.IsEmpty
				&& Factory.Load<TariffView>(TariffView.Loader.GetDateOverlapManualTariffFilter(tariff, type, dataGrouping, code, version, startDate, endDate.Value)).Length > 0)
			{
				logs.Append(MessageGenerator.GetDateOverlapError(startDate, endDate.Value, type, code, dataGrouping, version));
				isValid = false;
			}

			return isValid;
		}

		bool ValidateEndDate(ZDateTime? endDate, ZDateTime date, ZString dateDescription)
		{
			var isValid = true;
			if (endDate.HasValue && endDate.Value < date)
			{
				logs.Append(MessageGenerator.GetEndDateError(endDate.Value, dateDescription, date));
				isValid = false;
			}
			return isValid;
		}

		bool ValidateTaxOrFeeCode(ZString code, ZString dataGrouping)
		{
			var isValid = true;
			if (!code.IsEmpty)
			{
				var loader = new RefCusTaxOrFee.Loader(Factory);
				isValid = loader.IsExistedTaxOrFee(dataGrouping, code, Constant.VAT);
				if (!isValid)
				{
					logs.Append(MessageGenerator.GetInvalidVATRateCodeError(code, dataGrouping));
				}
			}
			return isValid;
		}

		bool ValidateUOM2(ZString uom2, ZString uom3)
		{
			var result = true;
			if ((!HasColumn(FieldNames.UOM2) || uom2.IsEmpty) && (HasColumn(FieldNames.UOM3) && !uom3.IsEmpty))
			{
				logs.Append(MessageGenerator.GetNoUOM2Error());
				result = false;
			}
			return result;
		}

		bool ValidateUOMs(TariffView tariff, TariffDataToLoad dataToLoad)
		{
			var result = true;
			var uoms = tariff.UnitsOfMeasure.ToDictionary(uom => uom.ZZ8_Type, uom => uom.ZZ8_UOM);

			if (uoms.Any() && HasColumn(FieldNames.UOM1) && dataToLoad.UOM1.IsEmpty && HasColumn(FieldNames.UOM2) && dataToLoad.UOM2.IsEmpty && HasColumn(FieldNames.UOM3) && dataToLoad.UOM3.IsEmpty)
			{
				result = false;
				logs.Append(MessageGenerator.GetEmptyUOMsError());
			}
			else if (HasColumn(FieldNames.UOM1) || HasColumn(FieldNames.UOM2) || HasColumn(FieldNames.UOM3))
			{
				UpdateUOMDictionary(uoms, FieldNames.UOM1, UOMTypeList.Codes.CU1, dataToLoad.UOM1);
				UpdateUOMDictionary(uoms, FieldNames.UOM2, UOMTypeList.Codes.CU2, dataToLoad.UOM2);
				UpdateUOMDictionary(uoms, FieldNames.UOM3, UOMTypeList.Codes.CU3, dataToLoad.UOM3);

				if (uoms.Any())
				{
					if (uoms.ContainsKey(UOMTypeList.Codes.CU2) && !uoms.ContainsKey(UOMTypeList.Codes.CU1))
					{
						result = false;
						logs.Append(MessageGenerator.GetNoUOM1Error());
					}

					if (uoms.ContainsKey(UOMTypeList.Codes.CU3) && !uoms.ContainsKey(UOMTypeList.Codes.CU2))
					{
						result = false;
						logs.Append(MessageGenerator.GetNoUOM2Error());
					}

					if (uoms.Values.Distinct().Count() != uoms.Count)
					{
						result = false;
						logs.Append(MessageGenerator.GetDuplicateUOMsError());
					}
				}
			}

			return result;
		}

		void UpdateUOMDictionary(Dictionary<ZString, ZString> uoms, ZString columnUOM, ZString uomType, ZString uomValue)
		{
			if (HasColumn(columnUOM))
			{
				if (!uomValue.IsEmpty)
				{
					if (uoms.ContainsKey(uomType))
					{
						uoms[uomType] = uomValue;
					}
					else
					{
						uoms.Add(uomType, uomValue);
					}
				}
				else
				{
					if (uoms.ContainsKey(uomType))
					{
						uoms.Remove(uomType);
					}
				}
			}
		}

		void UpdateTariffUOM(TariffUOMView.Loader loader, ZString columnUOM, ZGuid tariffPK, ZString type, ZString value)
		{
			if (HasColumn(columnUOM))
			{
				var uom = loader.LoadTariffUomView(tariffPK, type, false);
				if (!value.IsEmpty)
				{
					uom = uom ?? CreateCusRefTariffUOM(tariffPK, type);
					uom.ZZ8_UOM = value;
				}
				else
				{
					uom?.Delete();
				}
			}
		}

		TariffUOMView CreateCusRefTariffUOM(ZGuid tariffPK, ZString type)
		{
			var uom = Factory.New<TariffUOMView>();
			uom.ZZ8_ZZ1_ParentTariffOrNationalCode = tariffPK;
			uom.ZZ8_Type = type;
			return uom;
		}

		#endregion
	}
}
