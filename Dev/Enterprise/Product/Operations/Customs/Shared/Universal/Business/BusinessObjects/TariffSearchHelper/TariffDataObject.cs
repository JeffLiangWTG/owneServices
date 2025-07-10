using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	[DebuggerDisplay("{TariffCodeAndDescription}")]
	public class TariffDataObject : NonPersistentBusinessObject, IObsoleteValidation
	{
		public TariffDataObject(TariffSearchHelper helper, ITariffData tariffData, bool isOriginNomenclatureSQLResult = false)
		{
			this.helper = helper;
			this.tariffData = tariffData;
			this.AllowLoadTariff = isOriginNomenclatureSQLResult;
		}

		public static TariffDataObject GetOther(TariffSearchHelper helper)
		{
			return new TariffDataObject(helper, new TariffData { IsNomenclatureGroup = true }) { IsOther = true };
		}

		public static class Schema
		{
			public const string FullDescription = "FullDescription";
		}

		public TariffView CusTariff => tariffData as TariffView;

		public bool IsOther
		{
			get;
			private set;
		}

		public bool AllowLoadTariff { get; set; }

		public bool IsNomenclatureGroup => tariffData.IsNomenclatureGroup;

		public ZString TariffCodeAndDescription
			=> IsOther
				? Other
				: (helper?.TariffFormatter?.Format(tariffData.TariffCode) ?? tariffData.TariffCode) + " " + Description;

		protected static string Other => Res.GetString("134BDF51-FC3B-43C2-91BB-191997E93EA4", " Other");

		ZString Description
		{
			get
			{
				if (!description.HasValue)
				{
					description = tariffData.GetDescription(helper?.LanguageCode ?? TranslationHelper.GetCurrentLanguageCode());
				}
				return description.Value;
			}
		}
		ZString? description;

		public ZString TariffCode => tariffData.TariffCode;

		[ResourceStringData("Enterprise.Customs.Universal.TariffDataObject|FullDescription", Caption = "Full Description")]
		public ZString FullDescription => Description;

		public ZPropertyInfo FullDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.FullDescription); }
		}

		public bool HasSelectableTariff
		{
			get
			{
				var result = !IsNomenclatureGroup;

				if (!result)
				{
					result = RelatedDataCollection.Any(x => x.HasSelectableTariff);
				}

				return result;
			}
		}

		public ZString CompositeKey => tariffData.CompositeKey;

		public ZString[] GetCompositeKeyComponents()
		{
			return compositeKeyComponents ?? (compositeKeyComponents = GetCompositeKeyComponents(CompositeKey));
		}
		ZString[] compositeKeyComponents;

		static ZString[] GetCompositeKeyComponents(ZString compositeKey)
		{
			return compositeKey.IsEmpty ? Array.Empty<ZString>() : compositeKey.Split('.');
		}

		public void ClearCachedData()
		{
			compositeKeysToCheck = null;
			relatedDataCollection = null;
			Parent = null;
			description = null;
			AllowLoadTariff = false;
		}

		public void AddCompositeKeyToCheck(ZString compositeKeyToCheck, bool isFromNomenclatureGroup)
		{
			if (CanAddCompositeKeyToCheck(compositeKeyToCheck))
			{
				var key = isFromNomenclatureGroup ? compositeKeyToCheck : GetNomenclatureGroupCompositeKeyPart(compositeKeyToCheck);
				if (!isFromNomenclatureGroup && key.EndsWith("."))
				{
					// When the compositeKeyToCheck is "A.B..C" and key is "A.B.", use "A.B..C" to find the nomenclature group
					key = compositeKeyToCheck;
				}

				compositeKeysToCheck ??= new ();
				var existingMatchedKeys = compositeKeysToCheck.Where(x => x.StartsWith(key, StringComparison.OrdinalIgnoreCase) && x != key).ToList();
				existingMatchedKeys.ForEach(x => compositeKeysToCheck.Remove(x));
				if (!compositeKeysToCheck.Any(x => key.StartsWith(x, StringComparison.OrdinalIgnoreCase)))
				{
					compositeKeysToCheck.Add(key);
				}
			}
		}

		public ZString GetNomenclatureGroupCompositeKeyPart(ZString compositeKeyToCheck)
		{
			var result = compositeKeyToCheck;
			var components = GetCompositeKeyComponents(compositeKeyToCheck);
			if (components.Length > 1)
			{
				result = helper.GenerateCompositeKey(components, components.Length - 1);
			}
			return result;
		}

		List<ZString> compositeKeysToCheck;

		bool CanAddCompositeKeyToCheck(ZString compositeKeyToCheck)
		{
			return !compositeKeyToCheck.IsEmpty && IsNomenclatureGroup && compositeKeyToCheck.StartsWith(CompositeKey, StringComparison.OrdinalIgnoreCase);
		}

		static ZString GetCompareKey(TariffDataObject x)
		{
			var compositeKeyPrefix = Regex.IsMatch(x.CompositeKey, @"^\d\d\.\d\d\.\.\d\d?\.\d\d?$") ? (ZString)$"{x.CompositeKey}.0" : x.CompositeKey;
			return compositeKeyPrefix + "_" + x.TariffCode;
		}

		static TariffDataObject GetApplicableTariff(List<TariffDataObject> searchResultDictionary, TariffDataObject data)
		{
			return data;
		}

		IEnumerable<RefCusNomenclatureGroup> GetNomenclatureGroups()
		{
			var mainQuery = new ZQuery(RefCusNomenclatureGroupSchema.ZZ5_CompositeKey, SQLComparisonOperator.Like, CompositeKey + "%");
			mainQuery.AddToFilter(RefCusNomenclatureGroupSchema.ZZ5_CompositeKey, SQLComparisonOperator.NotEqual, CompositeKey);
			mainQuery.FetchOnlyFromLocalCache = true;

			var groups = new List<RefCusNomenclatureGroup>();

			if (compositeKeysToCheck == null)
			{
				groups.AddRange(GatherParentOnlyNomenclatureGroups(helper.LoadRefCusNomenclatureGroup(mainQuery)));
			}
			else
			{
				var currentLevel = GetCompositeKeyComponents(CompositeKey).Length;
				var fullGroups = new List<RefCusNomenclatureGroup>();

				foreach (var compositeKeyToCheck in compositeKeysToCheck)
				{
					var compositeKeyQuery = new ZQuery { DefaultJoinCondition = JoinCondition.Or };
					var components = GetCompositeKeyComponents(compositeKeyToCheck);

					for (var level = currentLevel + 1; level <= components.Length; level++)
					{
						compositeKeyQuery.AddToFilter(RefCusNomenclatureGroupSchema.ZZ5_CompositeKey, helper.GenerateCompositeKey(components, level));
					}

					var query = new ZQuery(mainQuery);
					query.AddToFilter(compositeKeyQuery);
					query.FetchOnlyFromLocalCache = true;

					fullGroups.AddRange(GatherParentOnlyNomenclatureGroups(helper.LoadRefCusNomenclatureGroup(query), (group) =>
					{
						helper.GetOrCreateTariffDataObject(group).AddCompositeKeyToCheck(compositeKeyToCheck, true);
					}));
				}

				groups.AddRange(fullGroups.OrderBy(o => GetRefCusNomenclatureGroupOrderBy(o))
					.Where(x => !groups.Any(y => x.ZZ5_CompositeKey.StartsWith(y.ZZ5_CompositeKey, StringComparison.OrdinalIgnoreCase))));
			}

			return groups;
		}

		static IEnumerable<RefCusNomenclatureGroup> GatherParentOnlyNomenclatureGroups(IEnumerable<RefCusNomenclatureGroup> groupsToProcess, Action<RefCusNomenclatureGroup> additionalAction = null)
		{
			additionalAction = additionalAction ?? (x => { });

			var orderedGroups = groupsToProcess.OrderBy(o => GetRefCusNomenclatureGroupOrderBy(o)).ToArray();
			var result = new List<RefCusNomenclatureGroup>();

			foreach (var group in orderedGroups.Where(x => !result.Any(y => x.ZZ5_CompositeKey.StartsWith(y.ZZ5_CompositeKey, StringComparison.OrdinalIgnoreCase))))
			{
				result.Add(group);
				additionalAction(group);
			}

			return result;
		}

		static string GetRefCusNomenclatureGroupOrderBy(RefCusNomenclatureGroup group) => group.ZZ5_CompositeKey + " " + group.ZZ5_StartDate.ToString("yyyyMMdd", Culture.Invariant);

		IEnumerable<TariffDataObject> LoadRelatedTariffs(IEnumerable<ZString> ignoredCompositeKeys)
		{
			var query = new ZQuery(TariffViewSchema.ZZ1_CompositeKeyOnZZ5, SQLComparisonOperator.Like, CompositeKey + "%");

			if (!IsNomenclatureGroup)
			{
				query.AddToFilter(TariffViewSchema.PK, SQLComparisonOperator.NotEqual, tariffData.PK);
			}

			foreach (var ignoredCompositeKey in ignoredCompositeKeys)
			{
				query.AddToFilter(TariffViewSchema.ZZ1_CompositeKeyOnZZ5, SQLComparisonOperator.DoesNotStartWith, ignoredCompositeKey);
			}

			query.OrderBy = TariffViewSchema.ZZ1_TariffCode.Name;

			if (compositeKeysToCheck != null)
			{
				var compositeKeysToCheckQuery = new ZQuery { DefaultJoinCondition = JoinCondition.Or };
				var currentLevel = GetCompositeKeyComponents(CompositeKey).Length;

				foreach (var compositeKey in compositeKeysToCheck)
				{
					compositeKeysToCheckQuery.AddToFilter(TariffViewSchema.ZZ1_CompositeKeyOnZZ5, SQLComparisonOperator.Like, compositeKey + ".%");
					if (compositeKey.StartsWith(CompositeKey + "..") && GetCompositeKeyComponents(compositeKey).Length == currentLevel + 2)
					{
						// When the compositeKeyToCheck is "A.B..C" and the CompositeKey is "A.B", retrieve related tariffs with composite key of A.B..C too
						compositeKeysToCheckQuery.AddToFilter(TariffViewSchema.ZZ1_CompositeKeyOnZZ5, compositeKey);
					}
				}

				query.AddToFilter(compositeKeysToCheckQuery);
			}

			query.FetchOnlyFromLocalCache = true;

			return helper.Factory.Load<TariffView>(query).Select(x => helper.GetOrCreateTariffDataObject(x)).ToList();
		}

		public void AddRelatedData(TariffDataObject tariffDataObject)
		{
			LoadRelatedDataCollection();
			AddRelatedDataCore(tariffDataObject);
		}

		void AddRelatedDataCore(TariffDataObject tariffDataObject)
		{
			if (IsNomenclatureGroup && tariffDataObject != this && (tariffDataObject.Parent == null || tariffDataObject.Parent == this))
			{
				tariffDataObject.Parent = this;
				relatedDataCollection.Add(tariffDataObject);
			}
			else
			{
				helper.ReportRelatedDataIsInvalid(this, tariffDataObject);
			}
		}

		internal TariffDataObject Parent
		{
			get;
			private set;
		}

		public IEnumerable<TariffDataObject> RelatedDataCollection
		{
			get
			{
				LoadRelatedDataCollection();
				return relatedDataCollection;
			}
		}
		List<TariffDataObject> relatedDataCollection;

		void LoadRelatedDataCollection()
		{
			if (relatedDataCollection == null)
			{
				relatedDataCollection = new List<TariffDataObject>();

				if (!IsOther && IsNomenclatureGroup)
				{
					var collection = new List<TariffDataObject>();
					helper.ProcessTariffData(collection, GetNomenclatureGroups().Select(x => helper.GetOrCreateTariffDataObject(x)), GetApplicableTariff);

					foreach (var relatedData in collection.Concat(LoadRelatedTariffs(collection.Select(x => x.CompositeKey))))
					{
						if (AllowLoadTariff || (relatedData.IsNomenclatureGroup && relatedData.HasSelectableTariff) || (relatedData.CusTariff?.MatchesFilter(helper.TariffOnlyFilter) ?? false))
						{
							if (AllowLoadTariff)
							{
								relatedData.AllowLoadTariff = true;
							}

							AddRelatedDataCore(relatedData);
						}
					}

					relatedDataCollection.Sort((x, y) => GetCompareKey(x).CompareTo(GetCompareKey(y)));
				}
			}
		}

		public int GetNumberOfLeafNodes()
		{
			if (RelatedDataCollection.Any())
			{
				return RelatedDataCollection.Sum(x => x.GetNumberOfLeafNodes());
			}
			else
			{
				return 1;
			}
		}

		readonly TariffSearchHelper helper;
		readonly ITariffData tariffData;
	}
}
