using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Universal
{
	[ModuleID(ModuleId.ZZRefCusCodeList)]
	public class ZZRefCusCodeListCombinedCollection : BusinessObjectCollection<ZZRefCusCodeListCombined>
	{
		public ZZRefCusCodeListCombinedCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ZZRefCusCodeListCombinedCollection(BusinessObjectFactory factory, ZString dataGroupingCode, ZString codeType, ZDateTime date)
			: base(factory)
		{
			DataGroupingCodes = ImmutableHashSet.Create(dataGroupingCode);
			CodeTypes = ImmutableHashSet.Create<ZString>(Argument.NotNullOrEmpty(codeType, "codeType"));
			relationshipFilter = ZZRefCusCodeListCombined.Loader.GetFilter(factory, dataGroupingCode, codeType, date, (ZQuery)null, true);
		}

		public ZZRefCusCodeListCombinedCollection(BusinessObjectFactory factory, ZString parentDataGroupingCode, ZString codeType, ZDateTime date, IEnumerable<RefCusCodeListAttributeFilter> attributeFilters, ZString[] otherDataGroupingCodes = null)
			: base(factory)
		{
			CodeTypes = ImmutableHashSet.Create<ZString>(Argument.NotNullOrEmpty(codeType, "codeType"));
			var dataGroupingCodes = RefDataGrouping.GetChildDataGroupings(factory, parentDataGroupingCode)
				.Select(x => x.ZZZ_DataGrouping)
				.Union(otherDataGroupingCodes ?? Array.Empty<ZString>())
				.Distinct()
				.ToArray();
			DataGroupingCodes = ImmutableHashSet.Create(dataGroupingCodes);
			ShouldAddDefaultDataGroupFilter = true;
			relationshipFilter = ZZRefCusCodeListCombined.Loader.GetFilter(factory, dataGroupingCodes, new[] { codeType }, date, attributeFilters, false);
		}

		public ZZRefCusCodeListCombinedCollection(BusinessObjectFactory factory, IEnumerable<ZString> dataGroupingCodes, ZString codeType, ZDateTime date, IEnumerable<RefCusCodeListAttributeFilter> attributeFilters)
			: base(factory)
		{
			CodeTypes = ImmutableHashSet.Create<ZString>(Argument.NotNullOrEmpty(codeType, "codeType"));
			var groupingCodes = dataGroupingCodes.ToArray();
			DataGroupingCodes = ImmutableHashSet.Create(groupingCodes);
			ShouldAddDefaultDataGroupFilter = true;
			relationshipFilter = ZZRefCusCodeListCombined.Loader.GetFilter(groupingCodes, codeType, date, attributeFilters);
		}

		public ZZRefCusCodeListCombinedCollection(BusinessObjectFactory factory, ZString dataGroupingCode, ZString[] codeTypes, ZDateTime date, IEnumerable<RefCusCodeListAttributeFilter> attributeFilters, bool includeParentDataGroupings = true)
			: this(factory, null, dataGroupingCode, codeTypes, date, attributeFilters, includeParentDataGroupings)
		{
		}

		public ZZRefCusCodeListCombinedCollection(BusinessObjectFactory factory, ZString[] dataGroupingCodes, ZString[] codeTypes, ZDateTime date, IEnumerable<RefCusCodeListAttributeFilter> attributeFilters, bool includeParentDataGroupings = true)
			: this(factory, null, dataGroupingCodes, codeTypes, date, attributeFilters, includeParentDataGroupings)
		{
		}

		public ZZRefCusCodeListCombinedCollection(BusinessObjectFactory factory, ZQuery additionalFilter, ZString dataGroupingCode, ZString[] codeTypes, ZDateTime date, IEnumerable<RefCusCodeListAttributeFilter> attributeFilters, bool includeParentDataGroupings = true)
			: this(factory, additionalFilter, new[] { dataGroupingCode }, codeTypes, date, attributeFilters, includeParentDataGroupings)
		{
		}

		public ZZRefCusCodeListCombinedCollection(BusinessObjectFactory factory, ZQuery additionalFilter, ZString[] dataGroupingCodes, ZString[] codeTypes, ZDateTime date, IEnumerable<RefCusCodeListAttributeFilter> attributeFilters, bool includeParentDataGroupings = true)
			: base(factory, additionalFilter)
		{
			Argument.NotNull(dataGroupingCodes, "dataGroupingCodes");
			dataGroupingCodes.ForEach(dataGroupingCode => Argument.NotNullOrEmpty(dataGroupingCode, "dataGroupingCodes"));
			Array.Sort(dataGroupingCodes);
			DataGroupingCodes = ImmutableHashSet.Create(dataGroupingCodes);
			Argument.NotNull(codeTypes, "codeTypes");
			codeTypes.ForEach(codeType => Argument.NotNullOrEmpty(codeType, "codeTypes"));
			Array.Sort(codeTypes);
			CodeTypes = ImmutableHashSet.Create(codeTypes);
			relationshipFilter = ZZRefCusCodeListCombined.Loader.GetFilter(factory, dataGroupingCodes, codeTypes, date, attributeFilters, includeParentDataGroupings);
			IncludeParentDataGroupings = includeParentDataGroupings;
		}

		public static ZZRefCusCodeListCombinedCollection GetCachedCollection(BusinessObjectFactory factory, ZString dataGroupingCode, ZString codeType, ZDateTime date)
			=> GetCachedCollection(factory, dataGroupingCode, new[] { codeType }, date, null, includeParentDataGroupings: true);

		public static ZZRefCusCodeListCombinedCollection GetCachedCollection(BusinessObjectFactory factory, ZString dataGroupingCode, ZString[] codeTypes, ZDateTime date, IEnumerable<RefCusCodeListAttributeFilter> attributeFilters, bool includeParentDataGroupings = true)
			=> GetCachedCollection(factory, new[] { dataGroupingCode }, codeTypes, date, attributeFilters, includeParentDataGroupings);

		public static ZZRefCusCodeListCombinedCollection GetCachedCollection(BusinessObjectFactory factory, ZString[] dataGroupingCodes, ZString[] codeTypes, ZDateTime date, IEnumerable<RefCusCodeListAttributeFilter> attributeFilters, bool includeParentDataGroupings = true)
			=> GetCachedCollection(factory, null, dataGroupingCodes, codeTypes, date, attributeFilters, includeParentDataGroupings);

		public static ZZRefCusCodeListCombinedCollection GetCachedCollection(BusinessObjectFactory factory, ZString dataGroupingCode, ZString[] codeTypes,
			ZDateTime date, IEnumerable<RefCusCodeListAttributeFilter> attributeFilters, RefCusCodeListTypes.IncludeParentDataGroupingOptions includeParentDataGroupings)
		{
			switch (includeParentDataGroupings)
			{
				case RefCusCodeListTypes.IncludeParentDataGroupingOptions.ChildOnly:
					return GetCachedCollection(factory, null, dataGroupingCode, codeTypes, date, attributeFilters, includeParentDataGroupings: false);
				case RefCusCodeListTypes.IncludeParentDataGroupingOptions.Union:
					return GetCachedCollection(factory, null, dataGroupingCode, codeTypes, date, attributeFilters, includeParentDataGroupings: true);
				case RefCusCodeListTypes.IncludeParentDataGroupingOptions.ChildFirstThenParent:
					var collection = GetCachedCollection(factory, null, dataGroupingCode, codeTypes, date, attributeFilters, includeParentDataGroupings: false);
					if (collection.GetEstimatedLoadCount(collection.CompleteFilter) == 0)
					{
						collection = GetCachedCollection(factory, null, dataGroupingCode, codeTypes, date, attributeFilters, includeParentDataGroupings: true);
					}
					return collection;
				default:
					return new ZZRefCusCodeListCombinedCollection(factory);
			}
		}

		public static ZZRefCusCodeListCombinedCollection GetCachedCollection(BusinessObjectFactory factory, ZQuery additionalFilter, ZString dataGroupingCode, ZString[] codeTypes, ZDateTime date, IEnumerable<RefCusCodeListAttributeFilter> attributeFilters, bool includeParentDataGroupings = true)
			=> GetCachedCollection(factory, additionalFilter, new[] { dataGroupingCode }, codeTypes, date, attributeFilters, includeParentDataGroupings);

		public static ZZRefCusCodeListCombinedCollection GetCachedCollection(BusinessObjectFactory factory, ZQuery additionalFilter, ZString[] dataGroupingCodes, ZString[] codeTypes, ZDateTime date, IEnumerable<RefCusCodeListAttributeFilter> attributeFilters, bool includeParentDataGroupings = true)
		{
			var sortedDataGroupingCodes = dataGroupingCodes.Distinct().OrderBy(x => x).ToArray();
			var sortedCodeTypes = codeTypes.Distinct().OrderBy(x => x).ToArray();
			var attrFilterKeyPart = attributeFilters == null ? string.Empty : string.Join(";", attributeFilters.Select(x => x.Key).OrderBy(x => x));
			var additionalFilterString = additionalFilter?.FilterPartsHashKey ?? string.Empty;
			var key = string.Join("_", "ZZRefCusCodeListCombinedCollection", string.Join(",", sortedDataGroupingCodes), string.Join(",", sortedCodeTypes), date.Date, attrFilterKeyPart, includeParentDataGroupings, additionalFilterString);

			return factory.GetCachedValue(key,
				() =>
				{
					var collection = new ZZRefCusCodeListCombinedCollection(factory, additionalFilter, sortedDataGroupingCodes, sortedCodeTypes, date, attributeFilters, includeParentDataGroupings);
					if (sortedCodeTypes.Length == 1)
					{
						var codeType = sortedCodeTypes[0];
						if (!string.IsNullOrEmpty(codeType))
						{
							collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Constants.ZZRefCusCodeListFilters.ListType, "Property", codeType));
						}
					}
					else
					{
						for (var defaultIndex = 1; defaultIndex <= sortedCodeTypes.Length; defaultIndex++)
						{
							collection.FilterBusinessObjectDefaults.Add(
								new FilterBusinessObjectDefault(Constants.ZZRefCusCodeListFilters.ListType, "Property", sortedCodeTypes[defaultIndex - 1], ZArchitecture.Business.FilterOrCategory.Red, defaultIndex)
							);
						}
					}

					if (date.IsValid)
					{
						collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Constants.ZZRefCusCodeListFilters.EffectiveDate, "Property1", date));
					}

					return collection;
				});
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var newElement = (ZZRefCusCodeListCombined)child;
			newElement.ZZD_CodeType = (CodeTypes != null && CodeTypes.Count == 1) ? CodeTypes.First() : ZString.Empty;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var result = base.CreateRelationshipFilter();
			if (relationshipFilter != null)
			{
				result.AddToFilter(relationshipFilter);
			}

			return result;
		}

		public ImmutableHashSet<ZString> CodeTypes { get; }
		public ImmutableHashSet<ZString> DataGroupingCodes { get; }
		public bool IncludeParentDataGroupings { get; }
		public bool ShouldAddDefaultDataGroupFilter { get; }

		RefCusCodeListAttributeNameCollection fMandatoryAttributeNames;
		public RefCusCodeListAttributeNameCollection MandatoryAttributeNames
		{
			get
			{
				if (fMandatoryAttributeNames == null)
				{
					fMandatoryAttributeNames = new RefCusCodeListAttributeNameCollection(this);
					fMandatoryAttributeNames.SetReadOnlyIncludingChildren(true);
				}
				return fMandatoryAttributeNames;
			}
		}

		protected readonly ZQuery relationshipFilter;
	}
}
