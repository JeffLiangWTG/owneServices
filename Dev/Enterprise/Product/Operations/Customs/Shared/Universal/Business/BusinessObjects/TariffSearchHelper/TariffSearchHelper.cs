using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;

namespace Enterprise.Customs.Universal
{
	[SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue")]
	public enum SelectionStyle
	{
		Heading = 4,
		Subheading = 6,
		EightCharNomenclature = 8,
		Tariff
	}

	public class TariffSearchHelper : NonPersistentBusinessObject, IObsoleteValidation, IResultCountHandler
	{
		public TariffSearchHelper(ZString dataGroupingCode, ZString tariffType, ZQuery nomenclatureGroupAdditionalFilter, ZQuery tariffAdditionalFilter, bool needLoadParentDataGroup = true, bool needLoadNomenclatureWhenTariffNotFound = false)
			: base(new BusinessObjectFactory())
		{
			this.dataGroupingCode = Argument.NotNullOrEmpty(dataGroupingCode, nameof(dataGroupingCode));
			fTariffType = tariffType;

			this.nomenclatureGroupAdditionalFilter = nomenclatureGroupAdditionalFilter;
			NeedLoadParentDataGroup = needLoadParentDataGroup;
			NeedLoadNomenclatureWhenTariffNotFound = needLoadNomenclatureWhenTariffNotFound;

			this.tariffAdditionalFilter = tariffAdditionalFilter;

			EffectiveDate = ZDateTime.Today;

			currentDateFilter = new ZQuery();
			currentDateFilter.AddToFilter(RefCusNomenclatureGroupSchema.ZZ5_StartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, EffectiveDate);
			currentDateFilter.AddToFilter(RefCusNomenclatureGroupSchema.ZZ5_EndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, EffectiveDate);

			SelectNomenclatureModes = new List<SelectionStyle> { SelectionStyle.Tariff };
			LoadLanguageFromSettingsStorage();

			PartialDescriptionMinLength = GetPartialDescriptionMinLength(3);
		}

		public TariffSearchHelper(ZString dataGroupingCode, ZString tariffType, ZQuery nomenclatureGroupAdditionalFilter, ZQuery tariffAdditionalFilter, List<SelectionStyle> selectNomenclatureModes, bool needLoadParentDataGroup, bool needLoadNomenclatureWhenTariffNotFound)
			: this(dataGroupingCode, tariffType, nomenclatureGroupAdditionalFilter, tariffAdditionalFilter, needLoadParentDataGroup, needLoadNomenclatureWhenTariffNotFound)
		{
			SelectNomenclatureModes = selectNomenclatureModes ?? new List<SelectionStyle> { SelectionStyle.Tariff };
		}

		public static class Schema
		{
			public const string PartialDescription = "PartialDescription";
			public const string ChapterHeadingTariff = "ChapterHeadingTariff";
			public const string EffectiveDate = "EffectiveDate";
			public const string TariffType = "TariffType";
			public const string Language = "Language";
			public const int LanguageMaxLength = RefCusNomenclatureLanguage.Schema.ZX8_ZX6_NKLanguageMaxLength;
		}

		public List<SelectionStyle> SelectNomenclatureModes { get; set; }

		public ZString DataGroupingCode => dataGroupingCode;

		public ZBool NeedLoadNomenclatureWhenTariffNotFound { get; }

		public ZBool LoadTargetDataFromTariff { get; set; } = false;

		TariffPreferredLanguageManager TariffPreferredLanguageManager => tariffPreferredLanguageManager ?? (tariffPreferredLanguageManager = new TariffPreferredLanguageManager());
		TariffPreferredLanguageManager tariffPreferredLanguageManager;

		[MaxLength(Schema.LanguageMaxLength)]
		[ResourceStringData("Enterprise.Customs.Universal.TariffSearchHelper|Language", Caption = "Language")]
		[List(nameof(Languages))]
		public ZString Language
		{
			get => language;
			set
			{
				var isValueSet = SetNonPersistentPropertyValue(LanguageInfo, ref language, value);
				if (isValueSet)
				{
					TariffPreferredLanguageManager.Language = language;
					LanguageCode = language != DefaultPreferredLanguageCode ? language : ZString.Empty;
				}
				LanguageInfo.RefreshBinding();
			}
		}
		ZString language;

		void LoadLanguageFromSettingsStorage()
		{
			var savedLanguage = TariffPreferredLanguageManager.Language;

			if (savedLanguage == DefaultPreferredLanguageCode)
			{
				var userProfileLanguage = TranslationHelper.GetLanguageCode(Env.CurrentUser.Language);
				savedLanguage = Languages.ContainsCode(userProfileLanguage) ? userProfileLanguage : GetDefaultPreferredLanguageCode();
			}
			else if (!Languages.ContainsCode(savedLanguage))
			{
				string languageCode = TranslationHelper.GetLanguageCode(savedLanguage);
				savedLanguage = Languages.ContainsCode(languageCode) ? languageCode : DefaultPreferredLanguageCode;
			}
			Language = savedLanguage;
		}

		ZString GetDefaultPreferredLanguageCode()
		{
			var countryCode = GlbCompany.GetCurrentCompany(Factory).GC_RN_NKCountryCode;
			return countryCode == Core.Constants.CountryCodes.Germany ? TranslationHelper.GetLanguageCodeForCountry(countryCode) : (ZString)DefaultPreferredLanguageCode;
		}

		public ZPropertyInfo LanguageInfo
		{
			get { return GetZPropertyInfo(Schema.Language); }
		}

		public ZString LanguageCode { get; private set; }

		const string DefaultPreferredLanguageCode = "DEF";
		string DefaultPreferredLanguageDescription => Res.GetString("Universal|TariffSearchHelper|DefaultPreferredLanguageDescription", "Default");

		public bool NeedLoadParentDataGroup { get; }

		public CodeDescriptionPairList Languages
		{
			get
			{
				return Factory.GetCachedValue("TariffSearchHelper.Languages", () =>
				{
					var list = LoadPossibleRefCusNomenclatureLanguages();
					var availableLanguages = new CodeDescriptionPairList(OLookUpEditType.Language);
					var result = new CodeDescriptionPairList();
					result.Add(new CodeDescriptionPair(DefaultPreferredLanguageCode, DefaultPreferredLanguageDescription));
					foreach (CodeDescriptionPair item in availableLanguages)
					{
						var languageCode = TranslationHelper.GetLanguageCode(item.Code);
						if (list.Contains(languageCode))
						{
							result.AddPairIfNotExist(languageCode, item.Description);
						}
					}
					return result;
				});
			}
		}

		public ZString PartialDescription
		{
			get { return partialDescription; }
			set
			{
				SetNonPersistentPropertyValue(PartialDescriptionInfo, ref partialDescription, value);
			}
		}

		ZString partialDescription;

		public ZPropertyInfo PartialDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.PartialDescription); }
		}

		public ZString ChapterHeadingTariff
		{
			get { return chapterHeadingTariff; }
			set
			{
				var newTariff = TariffFormatter?.Format(value) ?? value;
				SetNonPersistentPropertyValue(ChapterHeadingTariffInfo, ref chapterHeadingTariff, newTariff);
			}
		}

		ZString chapterHeadingTariff;

		public ZPropertyInfo ChapterHeadingTariffInfo
		{
			get { return GetZPropertyInfo(Schema.ChapterHeadingTariff); }
		}

		public static bool HasValidDataGroupingAndTariffType(BusinessObjectFactory factory, ZString dataGroupingCode, ZString tariffTypeCode)
		{
			var result = false;
			if (!dataGroupingCode.IsEmpty && !tariffTypeCode.IsEmpty)
			{
				var dataGroupings = RefDataGrouping.GetDataGroupingIncludingParent(factory, dataGroupingCode);

				var query = new ZDBOnlyQuery(typeof(RefCusTariffType));
				query.AddToFilter(RefCusTariffTypeSchema.ZZI_ZZZ_NKDataGrouping, dataGroupings);
				query.AddToFilter(RefCusTariffTypeSchema.ZZI_TariffType, tariffTypeCode);

				result = factory.Exists(typeof(RefCusTariffType), query);
			}
			return result;
		}

		#region Country Data Set

		public static bool GetHasCountryDataSet(BusinessObjectFactory factory, ZString dataGroupingCode)
		{
			var dataGroupingQuery = new ZDBOnlyQuery(typeof(RefCusNomenclatureGroup));
			dataGroupingQuery.AddToFilter(RefCusNomenclatureGroupSchema.ZZ5_ZZZ_NKDataGrouping, dataGroupingCode);
			return factory.Exists(typeof(RefCusNomenclatureGroup), dataGroupingQuery);
		}

		public static bool GetHasCountryGroupingDataSet(BusinessObjectFactory factory, ZString dataGroupingCode)
		{
			var result = false;
			var grouping = RefDataGrouping.GetParentDataGroupingCode(factory, dataGroupingCode);
			if (grouping.IsEmpty)
			{
				var dataGroupingQuery = new ZDBOnlyQuery(typeof(RefCusNomenclatureGroup));
				dataGroupingQuery.AddToFilter(RefCusNomenclatureGroupSchema.ZZ5_ZZZ_NKDataGrouping, grouping);
				result = factory.Exists(typeof(RefCusNomenclatureGroup), dataGroupingQuery);
			}

			return result;
		}

		#endregion

		#region EffectiveDate

		public ZDateTime EffectiveDate
		{
			get { return effectiveDate; }
			set
			{
				SetNonPersistentPropertyValue(EffectiveDateInfo, ref effectiveDate, value);
			}
		}

		ZDateTime effectiveDate;

		public ZPropertyInfo EffectiveDateInfo
		{
			get { return GetZPropertyInfo(Schema.EffectiveDate); }
		}

		#endregion

		#region TariffType

		public ZString TariffType
		{
			get { return fTariffType; }
			set
			{
				SetNonPersistentPropertyValue(TariffTypeInfo, ref fTariffType, value);
				if (!IsValidationSuspended)
				{
					TariffTypeInfo.ClearAllNotifications();
					MandatoryValidation.CheckEntered(TariffTypeInfo);
				}
			}
		}
		ZString fTariffType;

		public ZPropertyInfo TariffTypeInfo => GetZPropertyInfo(Schema.TariffType);

		#endregion

		public bool HasNomenclatureGroup
		{
			get
			{
				if (!hasNomenclatureGroup.HasValue)
				{
					var cusTariffType = GetCusTariffType(dataGroupingCode);
					if (cusTariffType == null)
					{
						var dataGrouping = Factory.LoadFromNaturalKey<RefDataGrouping>(RefDataGroupingSchema.ZZZ_DataGrouping, dataGroupingCode);
						var parentDataGrouping = dataGrouping?.Parent?.ZZZ_DataGrouping ?? ZString.Empty;
						if (!parentDataGrouping.IsEmpty)
						{
							cusTariffType = GetCusTariffType(parentDataGrouping);
						}
					}
					hasNomenclatureGroup = !(cusTariffType?.ZZI_ZZ9_NKNomenclatureGroupType ?? ZString.Empty).IsEmpty;
				}
				return hasNomenclatureGroup.Value;
			}
		}
		bool? hasNomenclatureGroup;

		RefCusTariffType GetCusTariffType(ZString dataGrouping)
		{
			var tariffTypeQuery = new ZQuery(RefCusTariffTypeSchema.ZZI_TariffType, TariffType);
			tariffTypeQuery.AddToFilter(RefCusTariffTypeSchema.ZZI_ZZZ_NKDataGrouping, dataGrouping);
			return Factory.LoadTop1<RefCusTariffType>(tariffTypeQuery);
		}

		public TariffDataObjectCollection TariffDataObjects
			=> tariffDataObjectCollection ?? (tariffDataObjectCollection = new TariffDataObjectCollection());

		TariffDataObjectCollection tariffDataObjectCollection;

		public Common.ITariffFormatter TariffFormatter { get; set; }

		public RefCusNomenclatureGroup[] LoadRefCusNomenclatureGroup(ZQuery query)
		{
			query.AddToFilter(GetRefCusNomenclatureGroupMainFilter(), JoinCondition.And);
			return Factory.Load<RefCusNomenclatureGroup>(query);
		}

		public TariffView[] LoadRefCusTariff(ZQuery query)
		{
			query.AddToFilter(GetRefCusTariffMainFilter(), JoinCondition.And);
			return Factory.Load<TariffView>(query);
		}

		#region IResultCountHandler

		public const int MaxRowsToLoad = 500;
		public const int MaxRecommendedRowsToLoad = 250;

		protected ResultCountMessage ResultCountMessage
		{
			get { return resultCountMessage ?? (resultCountMessage = new ResultCountMessage(this, MaxRowsToLoad, MaxRecommendedRowsToLoad)); }
		}
		ResultCountMessage resultCountMessage;

		void IResultCountHandler.UpdateNumberLoadedMessage(ZString message, int numberOfRecordsFound, bool shouldShowNumberLoadedMessageBox)
		{
			ShouldShowNumberLoadedMessageBox = shouldShowNumberLoadedMessageBox;
			NumberRecordsLoadedLabelText = message;
		}
		public ZString NumberRecordsLoadedLabelText;
		public bool ShouldShowNumberLoadedMessageBox;

		#endregion

		public TariffDataObject TariffMatched { get; private set; }

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public TariffDataObject[] Search(ZQuery filter, ZQuery dateOnlyFilter, ZQuery descriptionOnlyFilter, ZQuery tariffOnlyFilter)
		{
			TariffDataObject[] searchResult = null;

			if (!HasErrors)
			{
				TariffMatched = null;
				ClearCachedData();

				currentDateFilter = dateOnlyFilter;
				this.tariffOnlyFilter = tariffOnlyFilter;

				var tariffCode = ChapterHeadingTariff;

				var compositeKeys = new List<ZString>();
				filter.MaximumRows = MaxRowsToLoad + 1;

				var tariffs = LoadRefCusTariff(filter).ToDictionary((x) => x.PK);
				var loadedNomenclatureGroups = new List<RefCusNomenclatureGroup>();

				if (tariffs.Count <= MaxRowsToLoad)
				{
					var tariffDataObjs = new List<TariffDataObject>(tariffs.Values.Select(tariff => GetOrCreateTariffDataObject(tariff)));
					compositeKeys.AddRange(tariffDataObjs.Select(x => x.GetNomenclatureGroupCompositeKeyPart(x.CompositeKey)).Where(x => !x.IsEmpty).Distinct());
					loadedNomenclatureGroups.AddRange(LoadRefCusNomenclatureGroupToFactoryUpward(compositeKeys));

					if (descriptionOnlyFilter != null && !descriptionOnlyFilter.IsEmpty)
					{
						var nomenclatureGroups = LoadRefCusNomenclatureGroup(ConvertToRefCusNomenclatureGroupFilter(descriptionOnlyFilter));
						if (nomenclatureGroups.Length > 0)
						{
							loadedNomenclatureGroups.AddRange(nomenclatureGroups);

							var nomenclatureGroupCompositeKeys = nomenclatureGroups.Select(x => x.ZZ5_CompositeKey).ToArray();
							loadedNomenclatureGroups.AddRange(LoadRefCusNomenclatureGroupToFactoryUpward(nomenclatureGroupCompositeKeys));
							loadedNomenclatureGroups.AddRange(LoadRefCusNomenclatureGroupToFactoryDownward(nomenclatureGroupCompositeKeys));
						}

						if (tariffOnlyFilter == null || tariffOnlyFilter.IsEmpty)
						{
							var newCompositeKeys = new List<ZString>();

							foreach (var nomenclatureGroup in nomenclatureGroups)
							{
								tariffDataObjs.Add(GetOrCreateTariffDataObject(nomenclatureGroup, true));

								var compositeKey = nomenclatureGroup.ZZ5_CompositeKey;
								if (!compositeKeys.Contains(compositeKey))
								{
									compositeKeys.Add(compositeKey);
									newCompositeKeys.Add(compositeKey);
								}
							}

							foreach (var tariff in LoadRefCusTariffToFactory(newCompositeKeys).Where(x => !tariffs.ContainsKey(x.PK)))
							{
								tariffs.Add(tariff.PK, tariff);
							}
						}
						else if (nomenclatureGroups.Length > 0)
						{
							foreach (var nomenclatureGroupTariff in LoadRefCusTariffToFactory(GetHeaderCompositeKeysOnly(nomenclatureGroups.Select(x => x.ZZ5_CompositeKey).Distinct()), tariffOnlyFilter).Where(x => !tariffs.ContainsKey(x.PK)))
							{
								tariffs.Add(nomenclatureGroupTariff.PK, nomenclatureGroupTariff);

								var nomenclatureGroupTariffData = GetOrCreateTariffDataObject(nomenclatureGroupTariff);
								tariffDataObjs.Add(nomenclatureGroupTariffData);

								var compositeKey = nomenclatureGroupTariffData.GetNomenclatureGroupCompositeKeyPart(nomenclatureGroupTariffData.CompositeKey);
								if (compositeKeys.Contains(compositeKey))
								{
									compositeKeys.Add(compositeKey);
								}
							}
						}
					}

					if (NeedLoadNomenclatureWhenTariffNotFound && tariffOnlyFilter != null && !tariffOnlyFilter.IsEmpty)
					{
						var nomenclatureGroups = LoadRefCusNomenclatureGroup(ConvertToRefCusNomenclatureGroupFilter(tariffOnlyFilter));
						if (nomenclatureGroups.Length > 0)
						{
							loadedNomenclatureGroups.AddRange(nomenclatureGroups);

							var nomenclatureGroupCompositeKeys = nomenclatureGroups.Select(x => x.ZZ5_CompositeKey).ToArray();
							loadedNomenclatureGroups.AddRange(LoadRefCusNomenclatureGroupToFactoryUpward(nomenclatureGroupCompositeKeys));
							loadedNomenclatureGroups.AddRange(LoadRefCusNomenclatureGroupToFactoryDownward(nomenclatureGroupCompositeKeys));
						}

						var newCompositeKeys = new List<ZString>();

						foreach (var nomenclatureGroup in nomenclatureGroups)
						{
							tariffDataObjs.Add(GetOrCreateTariffDataObject(nomenclatureGroup, true));

							var compositeKey = nomenclatureGroup.ZZ5_CompositeKey;
							newCompositeKeys.Add(compositeKey);
							compositeKeys.Add(compositeKey);
						}

						foreach (var tariff in LoadRefCusTariffToFactory(newCompositeKeys).Where(x => !tariffs.ContainsKey(x.PK)))
						{
							tariffs.Add(tariff.PK, tariff);
						}
					}

					if (tariffs.Count <= MaxRowsToLoad)
					{
						if (!tariffCode.IsEmpty)
						{
							TariffMatched = tariffDataObjs.FirstOrDefault(x => x.TariffCode == tariffCode);
						}

						compositeKeys = GetHeaderCompositeKeysOnly(compositeKeys);
						LoadRefCusNomenclatureGroupToFactoryUpward(GetHeaderCompositeKeys(tariffDataObjs));

						var result = new List<TariffDataObject>();
						ProcessTariffData(result, tariffDataObjs, GetHeaderApplicableTariff);

						searchResult = tariffs.Count <= MaxRowsToLoad ? result.OrderBy(x => x.TariffCode).Where(x => x.HasSelectableTariff).ToArray() : null;
					}
				}

				if (loadedNomenclatureGroups.Count > 0)
				{
					LoadRefCusNomenclatureLanguage(loadedNomenclatureGroups.Distinct().Select(x => x.PK));
				}

				if (tariffs.Count > 0)
				{
					LoadCusRefTariffLanguageView(tariffs.Keys);
				}

				if (tariffs.Count <= MaxRowsToLoad)
				{
					ResultCountMessage.UpdateResultCountMessage(searchResult.Length);
				}
				else
				{
					ResultCountMessage.UpdateResultCountMessage(tariffs.Count);
				}
			}

			return searchResult ?? Array.Empty<TariffDataObject>();
		}

		void LoadRefCusNomenclatureLanguage(IEnumerable<ZGuid> pks)
		{
			if (!LanguageCode.IsEmpty)
			{
				var query = new ZQuery(RefCusNomenclatureLanguageSchema.ZX8_ZZ5_NomenclatureGroup, pks);
				query.AddToFilter(RefCusNomenclatureLanguageSchema.ZX8_ZX6_NKLanguage, LanguageCode);
				_ = Factory.Load<RefCusNomenclatureLanguage>(query);
			}
		}

		void LoadCusRefTariffLanguageView(IEnumerable<ZGuid> pks)
		{
			if (!LanguageCode.IsEmpty)
			{
				var query = new ZQuery(CusRefTariffLanguageViewSchema.ZX7_ZZ1_Tariff, pks);
				query.AddToFilter(CusRefTariffLanguageViewSchema.ZX7_ZX6_NKLanguage, LanguageCode);
				_ = Factory.Load<CusRefTariffLanguageView>(query);
			}
		}

		List<ZString> LoadPossibleRefCusNomenclatureLanguages()
		{
			var result = new List<ZString>();
			ZStringBuilder queryStringBuilder = new ZStringBuilder(
				ZString.Format(@"
					SELECT DISTINCT {0} 
					FROM {1} 
					INNER JOIN {2} ON {3} = {4} 
					WHERE {5} IN ({6}) ",
				RefCusNomenclatureLanguage.Schema.ZX8_ZX6_NKLanguage,
				RefCusNomenclatureLanguage.Schema.TableName,
				RefCusNomenclatureGroup.Schema.TableName,
				RefCusNomenclatureLanguage.Schema.ZX8_ZZ5_NomenclatureGroup,
				RefCusNomenclatureGroup.Schema.PK,
				RefCusNomenclatureGroup.Schema.ZZ5_ZZZ_NKDataGrouping,
				string.Join(", ", DataGroupingCodes().Select(x => "'" + x + "'"))
			));

			var collection = new DynamicBusinessObjectCollection(Factory);
			collection.Load(queryStringBuilder.ToString(), new ZSqlParameterCollection());
			foreach (DynamicBusinessObject obj in collection)
			{
				ZString lang = obj[RefCusNomenclatureLanguageSchema.ZX8_ZX6_NKLanguage].ToString();
				result.Add(lang);
			}
			return result;
		}

		static List<ZString> GetHeaderCompositeKeysOnly(IEnumerable<ZString> compositeKeys)
		{
			var result = new List<ZString>();
			foreach (var compositeKey in compositeKeys.OrderBy(x => x))
			{
				var existingMatchedKeys = result.Where(x => x.StartsWith(compositeKey, StringComparison.OrdinalIgnoreCase)).ToList();

				existingMatchedKeys.ForEach(x => result.Remove(x));
				if (!result.Any(x => compositeKey.StartsWith(x, StringComparison.OrdinalIgnoreCase)))
				{
					result.Add(compositeKey);
				}
			}
			return result;
		}

		IEnumerable<ZString> GetHeaderCompositeKeys(IEnumerable<TariffDataObject> tariffDataObjects)
		{
			return tariffDataObjects.Select(x => GetHeaderCompositeKey(x).GetValueOrDefault()).Where(x => !x.IsEmpty).Distinct();
		}

		TariffView[] LoadRefCusTariffToFactory(IEnumerable<ZString> compositeKeys, ZQuery additionalFilter = null)
		{
			var result = new List<TariffView>();
			var compositeKeyQuery = new ZQuery { DefaultJoinCondition = JoinCondition.Or };
			int count = 0;
			foreach (var compositeKey in compositeKeys)
			{
				compositeKeyQuery.AddToFilter(TariffViewSchema.ZZ1_CompositeKeyOnZZ5, SQLComparisonOperator.StartsWith, compositeKey);
				if (++count > 100)
				{
					count = 0;
					result.AddRange(LoadRefCusTariff(GetCombineFilter(additionalFilter, compositeKeyQuery)));
					compositeKeyQuery = new ZQuery { DefaultJoinCondition = JoinCondition.Or };
				}
			}
			if (!compositeKeyQuery.IsEmpty)
			{
				result.AddRange(LoadRefCusTariff(GetCombineFilter(additionalFilter, compositeKeyQuery)));
			}
			return result.ToArray();
		}

		static ZQuery GetCombineFilter(ZQuery filter1, ZQuery filter2)
		{
			var query = filter1 == null ? new ZQuery() : filter1.DeepClone();
			if (filter2 != null)
			{
				query.AddToFilter(filter2);
			}
			return query.IsEmpty ? ZQuery.NoResultQuery : query;
		}

		IEnumerable<RefCusNomenclatureGroup> LoadRefCusNomenclatureGroupToFactoryDownward(IEnumerable<ZString> compositeKeys)
		{
			var result = new List<RefCusNomenclatureGroup>();
			var parentCompositeKeys = new List<ZString>();
			foreach (var compositeKey in compositeKeys.OrderBy(x => x))
			{
				if (parentCompositeKeys.Any(x => compositeKey.StartsWith(x, StringComparison.OrdinalIgnoreCase)))
				{
					continue;
				}
				parentCompositeKeys.Add(compositeKey);
			}
			if (parentCompositeKeys.Count > 0)
			{
				var compositeKeyQuery = new ZQuery { DefaultJoinCondition = JoinCondition.Or };
				int count = 0;
				foreach (var compositeKey in parentCompositeKeys)
				{
					compositeKeyQuery.AddToFilter(RefCusNomenclatureGroupSchema.ZZ5_CompositeKey, SQLComparisonOperator.StartsWith, compositeKey);
					if (++count > 100)
					{
						count = 0;
						result.AddRange(LoadRefCusNomenclatureGroup(compositeKeyQuery));
						compositeKeyQuery = new ZQuery { DefaultJoinCondition = JoinCondition.Or };
					}
				}
				if (!compositeKeyQuery.IsEmpty)
				{
					result.AddRange(LoadRefCusNomenclatureGroup(compositeKeyQuery));
				}
			}
			return result.Distinct();
		}

		IEnumerable<RefCusNomenclatureGroup> LoadRefCusNomenclatureGroupToFactoryUpward(IEnumerable<ZString> compositeKeys)
		{
			var result = new List<RefCusNomenclatureGroup>();
			ZString[] unprocessedCompositeKeys = GetUnprocessedCompositeKeys(compositeKeys);
			if (unprocessedCompositeKeys.Length > 0)
			{
				var compositeKeyQuery = new ZQuery { DefaultJoinCondition = JoinCondition.Or };
				int count = 0;
				foreach (var compositeKey in unprocessedCompositeKeys)
				{
					compositeKeyQuery.AddToFilter(RefCusNomenclatureGroupSchema.ZZ5_CompositeKey, compositeKey);
					ProcessedCompositeKeys.Add(compositeKey);
					if (++count > 100)
					{
						count = 0;
						result.AddRange(LoadRefCusNomenclatureGroup(compositeKeyQuery));
						compositeKeyQuery = new ZQuery { DefaultJoinCondition = JoinCondition.Or };
					}
				}
				if (!compositeKeyQuery.IsEmpty)
				{
					result.AddRange(LoadRefCusNomenclatureGroup(compositeKeyQuery));
				}
			}
			return result.Distinct();
		}

		ZString[] GetUnprocessedCompositeKeys(IEnumerable<ZString> compositeKeys)
		{
			var result = new HashSet<ZString>();
			foreach (var compositeKey in compositeKeys.Where(x => !x.IsEmpty && !ProcessedCompositeKeys.Contains(x) && !result.Contains(x)))
			{
				result.Add(compositeKey);
				var components = compositeKey.Split('.');
				var length = components.Length;
				if (length > 1)
				{
					for (int level = length - 1; level > 0; level--)
					{
						var compositeKeyToCheck = GenerateCompositeKey(components, level);
						if (!ProcessedCompositeKeys.Contains(compositeKeyToCheck) && !result.Contains(compositeKeyToCheck))
						{
							result.Add(compositeKeyToCheck);
						}
						else
						{
							break;
						}
					}
				}
			}
			return result.ToArray();
		}

		TariffDataObject GetHeaderApplicableTariff(List<TariffDataObject> dataCollection, TariffDataObject tariffData)
		{
			TariffDataObject result = null;
			var chapter = GetChapter(tariffData);
			if (chapter == null)
			{
				var otherTariffData = dataCollection.FirstOrDefault(x => x.IsOther);
				if (otherTariffData == null)
				{
					otherTariffData = GetOther();
					dataCollection.Add(otherTariffData);
				}
				otherTariffData.AddRelatedData(AddLowerHeaderIfPossible(tariffData));
			}
			else
			{
				chapter.AddCompositeKeyToCheck(tariffData.CompositeKey, tariffData.IsNomenclatureGroup);
				result = chapter;
			}
			return result;
		}

		TariffDataObject GetOther()
		{
			return other ?? (other = Universal.TariffDataObject.GetOther(this));
		}

		TariffDataObject other;

		TariffDataObject AddLowerHeaderIfPossible(TariffDataObject tariffData)
		{
			var result = tariffData;
			var compositeKeyComponents = tariffData.GetCompositeKeyComponents();
			var level = HeaderLevel;
			while (compositeKeyComponents.Length > ++level)
			{
				var chapter = GetChapter(GenerateCompositeKey(compositeKeyComponents, level));
				if (chapter != null)
				{
					result = chapter;
					break;
				}
			}
			return result;
		}

		void ClearCachedData()
		{
			if (tariffDataObjectDictionary != null)
			{
				foreach (var tariffData in tariffDataObjectDictionary.Values)
				{
					tariffData.ClearCachedData();
				}
			}

			if (processedCompositeKeys != null)
			{
				processedCompositeKeys.Clear();
			}

			other?.ClearCachedData();
		}

		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "There is no benefit to creating a new type for this.")]
		public void ProcessTariffData(List<TariffDataObject> searchResultDictionary
			, IEnumerable<TariffDataObject> tariffDataObjects
			, Func<List<TariffDataObject>, TariffDataObject, TariffDataObject> getApplicableTariff)
		{
			foreach (var tariffDataObject in tariffDataObjects)
			{
				ProcessTariffData(searchResultDictionary, tariffDataObject, getApplicableTariff);
			}
		}

		static void ProcessTariffData(List<TariffDataObject> dataCollection
			, TariffDataObject tariffData
			, Func<List<TariffDataObject>, TariffDataObject, TariffDataObject> getApplicableTariff)
		{
			if (tariffData != null)
			{
				var resultTariff = getApplicableTariff(dataCollection, tariffData);
				if (resultTariff != null)
				{
					var key = resultTariff.CompositeKey;
					if (dataCollection.All(x => x.CompositeKey != key))
					{
						dataCollection.Add(resultTariff);
					}
				}
			}
		}

		int HeaderLevel
		{
			get
			{
				if (levelCached == null)
				{
					levelCached = (bool)ObjectFactory.Get<Integration.Customs.Shared.ICustomsDataRegistry>().ShowHeaderTariffData.Value ? 1 : 2;
				}
				return levelCached.Value;
			}
		}

		int? levelCached;

		ZString? GetHeaderCompositeKey(TariffDataObject tariffData)
		{
			ZString? result = null;
			var compositeKeyComponents = tariffData.GetCompositeKeyComponents();
			if (compositeKeyComponents.Length >= HeaderLevel)
			{
				result = GenerateCompositeKey(compositeKeyComponents, HeaderLevel);
			}
			return result;
		}

		TariffDataObject GetChapter(TariffDataObject tariffData)
		{
			TariffDataObject result = null;
			var headerCompositeKey = GetHeaderCompositeKey(tariffData);
			if (headerCompositeKey.HasValue)
			{
				result = GetChapter(headerCompositeKey.Value);
			}
			return result;
		}

		TariffDataObject GetChapter(ZString headerCompositeKey)
		{
			TariffDataObject result = null;
			var filter = GetRefCusNomenclatureGroupMainFilter();
			filter.AddToFilter(RefCusNomenclatureGroupSchema.ZZ5_CompositeKey, headerCompositeKey);
			filter.FetchOnlyFromLocalCache = true;
			var nomenclatureGroup = Factory.LoadTop1<RefCusNomenclatureGroup>(filter);
			if (nomenclatureGroup != null)
			{
				result = GetOrCreateTariffDataObject(nomenclatureGroup);
			}
			return result;
		}

		public ZString GenerateCompositeKey(ZString[] compositeKeyComponents, int elements)
		{
			return new ZStringBuilder(compositeKeyComponents.Take(elements)).ToStringWithDelimiterBetweenAppends(".");
		}

		public ZQuery GetRefCusNomenclatureGroupMainFilter()
		{
			var query = new ZQuery(RefCusNomenclatureGroupSchema.ZZ5_ZZZ_NKDataGrouping, DataGroupingCodes());
			query.AddToFilter(ConvertToRefCusNomenclatureGroupFilter(currentDateFilter));

			if (!TariffType.IsEmpty)
			{
				// Should not use DBOnlyQuery as it causes performance issue when we only want to fetch from local cache
				query.AddToFilter(RefCusNomenclatureGroupSchema.ZZ5_ZZ9_NKNomenclatureGroupType, GetNomenclatureGroupTypeForTariffType(TariffType));
			}
			if (nomenclatureGroupAdditionalFilter != null)
			{
				query.AddToFilter(nomenclatureGroupAdditionalFilter);
			}
			return query;
		}

		ZString[] GetNomenclatureGroupTypeForTariffType(ZString tariffType)
		{
			return Factory.GetCachedValue(System.FormattableString.Invariant($"NomenclatureGroupTypeForTariffType_{dataGroupingCode}_{tariffType}"), () =>
			{
				var query = new ZQuery(RefCusTariffTypeSchema.ZZI_ZZZ_NKDataGrouping, DataGroupingCodes());
				query.AddToFilter(RefCusTariffTypeSchema.ZZI_TariffType, tariffType);
				return Factory.Load<RefCusTariffType>(query).Select(x => x.ZZI_ZZ9_NKNomenclatureGroupType).Distinct().ToArray();
			});
		}

		#region ConvertToRefCusNomenclatureGroupFilter

		static ZQuery ConvertToRefCusNomenclatureGroupFilter(ZQuery filter)
		{
			var sql = filter.FilterString;
			var query = sql.Contains(CusRefTariffLanguageViewSchema.Constants.TableName) ? new ZDBOnlyQuery(typeof(RefCusNomenclatureGroup)) : new ZQuery();
			sql = sql.Replace(TariffViewSchema.Constants.ZZ1_Description, RefCusNomenclatureGroupSchema.Constants.ZZ5_Description)
				.Replace(TariffViewSchema.Constants.ZZ1_TariffCode, RefCusNomenclatureGroupSchema.Constants.ZZ5_Value)
				.Replace(TariffViewSchema.Constants.ZZ1_StartDate, RefCusNomenclatureGroupSchema.Constants.ZZ5_StartDate)
				.Replace(TariffViewSchema.Constants.ZZ1_EndDate, RefCusNomenclatureGroupSchema.Constants.ZZ5_EndDate)
				.Replace(ParameterNameFactory.ParameterPrefix, ParameterPrefix)
				.Replace(RefCusTariffSchema.Constants.PK, RefCusNomenclatureGroupSchema.Constants.PK)
				.Replace(CusRefTariffLanguageViewSchema.Constants.ZX7_ZZ1_Tariff, RefCusNomenclatureLanguageSchema.Constants.ZX8_ZZ5_NomenclatureGroup)
				.Replace(CusRefTariffLanguageViewSchema.Constants.ZX7_Description, RefCusNomenclatureLanguageSchema.Constants.ZX8_Description)
				.Replace(CusRefTariffLanguageViewSchema.Constants.ZX7_ZX6_NKLanguage, RefCusNomenclatureLanguageSchema.Constants.ZX8_ZX6_NKLanguage)
				.Replace(CusRefTariffLanguageViewSchema.Constants.TableName, RefCusNomenclatureLanguageSchema.Constants.TableName);

			var parms = new ZSqlParameterCollection();
			foreach (var parm in filter.Params)
			{
				var newName = parm.ParameterName.Replace(ParameterNameFactory.ParameterPrefix, ParameterPrefix);
				var newParm = ZSqlParameter.New(newName, parm.Value, GetMappedSchemaColumn(parm), parm.ComparisonOperator, parm.ComparisonOptions);
				parms.Add(newParm);
			}
			query.AddFilterAndZSQLParameterCollection(sql, parms);

			return query;
		}

		const string ParameterPrefix = "@TSH";

		static SchemaColumn GetMappedSchemaColumn(ZSqlParameter columnName)
		{
			switch (columnName.SchemaColumn.Name)
			{
				case (TariffViewSchema.Constants.PK):
					return RefCusNomenclatureGroupSchema.PK;
				case (TariffViewSchema.Constants.ZZ1_Description):
					return RefCusNomenclatureGroupSchema.ZZ5_Description;
				case (TariffViewSchema.Constants.ZZ1_TariffCode):
					return RefCusNomenclatureGroupSchema.ZZ5_Value;
				case (TariffViewSchema.Constants.ZZ1_StartDate):
					return RefCusNomenclatureGroupSchema.ZZ5_StartDate;
				case (TariffViewSchema.Constants.ZZ1_EndDate):
					return RefCusNomenclatureGroupSchema.ZZ5_EndDate;
				case (CusRefTariffLanguageViewSchema.Constants.ZX7_ZZ1_Tariff):
					return RefCusNomenclatureLanguageSchema.ZX8_ZZ5_NomenclatureGroup;
				case (CusRefTariffLanguageViewSchema.Constants.ZX7_Description):
					return RefCusNomenclatureLanguageSchema.ZX8_Description;
				case (CusRefTariffLanguageViewSchema.Constants.ZX7_ZX6_NKLanguage):
					return RefCusNomenclatureLanguageSchema.ZX8_ZX6_NKLanguage;
			}
			return columnName.SchemaColumn;
		}

		#endregion

		public ZQuery GetRefCusTariffMainFilter()
		{
			var query = new ZQuery(TariffViewSchema.ZZ1_ZZZ_NKDataGrouping, DataGroupingCodes());
			query.AddToFilter(TariffViewSchema.ZZ1_TableType, TariffView.GetEffectiveTableType(Factory, DataGroupingCode, TariffType, EffectiveDate.IsValid ? EffectiveDate.Date : ZDate.Today));
			if (!TariffType.IsEmpty)
			{
				query.AddToFilter(TariffViewSchema.ZZ1_ZZI_NKTariffType, TariffType);
			}
			if (tariffAdditionalFilter != null)
			{
				query.AddToFilter(tariffAdditionalFilter);
			}
			return query;
		}

		public TariffDataObject GetOrCreateTariffDataObject(ITariffData tariffData, bool isOriginNomenclatureSQLResult = false)
		{
			if (!TariffDataObjectDictionary.TryGetValue(tariffData, out TariffDataObject result))
			{
				result = new TariffDataObject(this, tariffData, isOriginNomenclatureSQLResult);
				TariffDataObjectDictionary.Add(tariffData, result);
			}
			else if (isOriginNomenclatureSQLResult)
			{
				result.AllowLoadTariff = true;
			}
			return result;
		}

		Dictionary<ITariffData, TariffDataObject> TariffDataObjectDictionary
			=> tariffDataObjectDictionary ?? (tariffDataObjectDictionary = new Dictionary<ITariffData, TariffDataObject>());

		Dictionary<ITariffData, TariffDataObject> tariffDataObjectDictionary;
		HashSet<ZString> ProcessedCompositeKeys => processedCompositeKeys ?? (processedCompositeKeys = new HashSet<ZString>());
		HashSet<ZString> processedCompositeKeys;

		ZQuery currentDateFilter;
		readonly ZString dataGroupingCode;
		readonly ZQuery nomenclatureGroupAdditionalFilter;
		readonly ZQuery tariffAdditionalFilter;

		public ZInt PartialDescriptionMinLength;

		public ZQuery TariffOnlyFilter => tariffOnlyFilter ?? new ZQuery();

		ZQuery tariffOnlyFilter;

		public int GetPartialDescriptionMinLength(int defaultMinLength)
		{
			var result = defaultMinLength;
			if (LanguageCode == Core.Constants.Customs.Universal.RefLanguageType.Codes.ChineseSimplified || LanguageCode == Core.Constants.Customs.Universal.RefLanguageType.Codes.ChineseTraditional)
			{
				result = 1;
			}
			return result;
		}

		public ZString[] DataGroupingCodes() => dataGroupingCodes ?? (dataGroupingCodes = GetDataGroupingCodes().Distinct().ToArray());
		ZString[] dataGroupingCodes;

		IEnumerable<ZString> GetDataGroupingCodes()
		{
			yield return dataGroupingCode;

			if (NeedLoadParentDataGroup)
			{
				var parentDataGroupingCode = RefDataGrouping.GetParentDataGroupingCode(Factory, dataGroupingCode);
				if (!parentDataGroupingCode.IsEmpty)
				{
					yield return parentDataGroupingCode;
				}
			}
		}

		protected override ZString HumanReadableNameCore
			=> Res.GetString("{FBF913A6-3A9E-4605-A7F6-2DA8CCDFFE8C}", "Tariff Search");

		internal void ReportRelatedDataIsInvalid(TariffDataObject parent, TariffDataObject relatedData)
		{
			ErrorReporter.ReportOnce(Invariant($@"Tariff data of '{dataGroupingCode}' is invalid as it either has tariffView with multiple related parent or is not Nomenclature Group:
Related Data Parent (Tariff: {relatedData.Parent?.TariffCode}, CompositeKey: {relatedData.Parent?.CompositeKey}, IsNomenclatureGroup: {relatedData.Parent?.IsNomenclatureGroup})
Related Data (Tariff: {relatedData.TariffCode}, CompositeKey: {relatedData.CompositeKey}, IsNomenclatureGroup: {relatedData.IsNomenclatureGroup})
New Parent (Tariff: {parent.TariffCode}, CompositeKey: {parent.CompositeKey}, IsNomenclatureGroup: {parent.IsNomenclatureGroup})"));
		}
	}

	[WTG.StaticAnalysis.Annotation.CodeAlive("Interface stub to be used to validate new customs reference data from other WTG modules")]
	public class TariffValidator : Integration.Customs.ITariffValidationDataProvider
	{
		public Integration.Customs.ITariffValidationData Validate(ZString country, ZString tariffOrNomenclature, bool findNearest)
		{
			return new TariffValidation(country, tariffOrNomenclature, findNearest);
		}

		class TariffValidation : Integration.Customs.ITariffValidationData
		{
			public TariffValidation(ZString country, ZString tariffOrNomenclature, bool findNearest)
			{
				if (country.IsEmpty || tariffOrNomenclature.IsEmpty)
				{
					throw new ArgumentException("country and tariffOrNomenclature parameters must contain a value.");
				}

				if (!tariffOrNomenclature.IsNumbersOnlyOrEmpty)
				{
					throw new ArgumentException("tariffOrNomenclature parameter value must be digits only.");
				}

				this.country = country;
				this.tariffOrNomenclature = tariffOrNomenclature;
				this.findNearest = findNearest;
				tariffView = null;
				nomenclature = null;
				factory = new BusinessObjectFactory();
			}

			readonly ZString country;
			readonly ZString tariffOrNomenclature;
			readonly bool findNearest;
			TariffView tariffView;
			RefCusNomenclatureGroup nomenclature;
			readonly BusinessObjectFactory factory;

			ZString[] DataGroupingCodes => dataGroupingCodes ?? (dataGroupingCodes = GetDataGroupingCodes().ToArray());
			ZString[] dataGroupingCodes;

			IEnumerable<ZString> GetDataGroupingCodes()
			{
				yield return country;

				var parentDataGroupingCode = CountryGrouping?.ZZZ_DataGrouping ?? ZString.Empty;
				if (!parentDataGroupingCode.IsEmpty)
				{
					yield return parentDataGroupingCode;
				}
			}

			#region ITariffValidationData Implementation

			ZString Integration.Customs.ITariffValidationData.DataGroupingName
			{
				get
				{
					var datagrouping = tariffView?.ZZ1_ZZZ_NKDataGrouping ?? nomenclature?.ZZ5_ZZZ_NKDataGrouping ?? ZString.Empty;
					return !datagrouping.IsEmpty ? GetCountryGroupingDescription(datagrouping) : ZString.Empty;
				}
			}

			ZString Integration.Customs.ITariffValidationData.Description => tariffView?.ZZ1_Description ?? nomenclature?.ZZ5_Description ?? ZString.Empty;

			ZDateTime Integration.Customs.ITariffValidationData.EndDate => tariffView?.ZZ1_EndDate ?? nomenclature?.ZZ5_EndDate ?? ZDateTime.Empty;

			bool Integration.Customs.ITariffValidationData.IsValid => findNearest ? FindNearestNomenclature() : FindExactMatch();

			ZString Integration.Customs.ITariffValidationData.NearestNomenclature => tariffView?.ZZ1_TariffCode ?? nomenclature?.ZZ5_Value ?? ZString.Empty;

			#endregion

			#region Implementation

			bool FindExactMatch()
			{
				var query = new ZDBOnlyQuery(typeof(TariffView));
				query.AddToFilter(TariffViewSchema.ZZ1_ZZZ_NKDataGrouping, DataGroupingCodes);
				query.AddToFilter(TariffViewSchema.ZZ1_TariffCode, tariffOrNomenclature);
				var isValid = factory.Exists(typeof(TariffView), query);

				tariffView = null;
				if (isValid)
				{
					var tariffToLoad = new ZQuery(TariffViewSchema.ZZ1_TariffCode, tariffOrNomenclature);
					tariffToLoad.AddToFilter(TariffViewSchema.ZZ1_ZZZ_NKDataGrouping, DataGroupingCodes);
					tariffToLoad.AddToFilter(TariffViewSchema.ZZ1_StartDate, SQLComparisonOperator.LessThanOrEqualTo,
						ZDateTime.UtcNow);
					tariffToLoad.OrderBy = TariffViewSchema.ZZ1_StartDate.Name + OrderByClause.Descending;
					tariffView = factory.LoadTop1<TariffView>(tariffToLoad);
				}

				return isValid;
			}

			bool FindNearestNomenclature()
			{
				var isValid = FindExactMatch();
				if (!isValid)
				{
					if (HasCountryDataSet || HasCountryGroupingDataSet)
					{
						var dataGrouping = HasCountryDataSet ? country : CountryGroupingCode;
						var query = new ZDBOnlyQuery(typeof(RefCusNomenclatureGroup));
						query.AddToFilter(RefCusNomenclatureGroupSchema.ZZ5_ZZZ_NKDataGrouping, dataGrouping);
						query.AddToFilter(RefCusNomenclatureGroupSchema.ZZ5_Value, tariffOrNomenclature.SubstringSafe(0, 6));
						isValid = factory.Exists(typeof(RefCusNomenclatureGroup), query);

						nomenclature = null;
						if (isValid)
						{
							var nomenclatureToLoad = new ZQuery(RefCusNomenclatureGroupSchema.ZZ5_Value, tariffOrNomenclature.SubstringSafe(0, 6));
							nomenclatureToLoad.AddToFilter(RefCusNomenclatureGroupSchema.ZZ5_ZZZ_NKDataGrouping, dataGrouping);
							nomenclatureToLoad.AddToFilter(RefCusNomenclatureGroupSchema.ZZ5_StartDate, SQLComparisonOperator.LessThanOrEqualTo, ZDateTime.UtcNow);
							nomenclatureToLoad.OrderBy = RefCusNomenclatureGroupSchema.ZZ5_StartDate.Name + OrderByClause.Descending;
							nomenclature = factory.LoadTop1<RefCusNomenclatureGroup>(nomenclatureToLoad);
						}
					}
				}

				return isValid;
			}

			bool HasCountryDataSet
			{
				get
				{
					if (fHasCountryDataSet == null)
					{
						fHasCountryDataSet = TariffSearchHelper.GetHasCountryDataSet(factory, country);
					}

					return fHasCountryDataSet.Value;
				}
			}

			bool? fHasCountryDataSet;

			bool HasCountryGroupingDataSet
			{
				get
				{
					if (fHasCountryGroupingDataSet == null)
					{
						fHasCountryGroupingDataSet = TariffSearchHelper.GetHasCountryDataSet(factory, country);
					}

					return fHasCountryGroupingDataSet.Value;
				}
			}
			bool? fHasCountryGroupingDataSet;

			RefDataGrouping CountryGrouping => countryGrouping ?? (countryGrouping = RefDataGrouping.GetParentDataGrouping(factory, country));
			RefDataGrouping countryGrouping;

			ZString CountryGroupingCode => CountryGrouping?.ZZZ_DataGrouping ?? ZString.Empty;

			ZString GetCountryGroupingDescription(string dataGroupingCode)
			{
				return RefDataGrouping.GetParentDataGrouping(factory, dataGroupingCode)?.ZZZ_Description ?? factory.LoadFromNaturalKey<RefDataGrouping>(RefDataGroupingSchema.ZZZ_DataGrouping, dataGroupingCode)?.ZZZ_Description ?? ZString.Empty;
			}

			#endregion
		}
	}
}
