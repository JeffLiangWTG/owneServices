using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs.Shared;

namespace Enterprise.Customs.Universal
{
	[RestrictedFilteredItem]
	[EnableRefererenceXMLExporter(nameof(ZZRefCusCodeListCombined), Schema.PK)]
	public sealed class ZZRefCusCodeListCombined : AutoZZRefCusCodeListCombined, ICodeDescription, IStmALogParent, ITemplateCopyable, ITransportModeListSupporter, ITranslatableZZBusinessObject
	{
		public ZZRefCusCodeListCombined(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoZZRefCusCodeListCombined.Schema
		{
			public const string ZZD_CodeTypeDesc = "ZZD_CodeTypeDesc";
			public const string ZZD_TransportModes = "ZZD_TransportModes";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Loader : AutoZZRefCusCodeListCombined.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public static ZQuery GetFilter(BusinessObjectFactory factory, ZString dataGroupingCode, ZString codeType, ZDateTime date, IEnumerable<RefCusCodeListAttributeFilter> attributeFilters, bool includeParentDataGroupings)
				=> GetFilter(factory, dataGroupingCode, new[] { codeType }, date, attributeFilters, includeParentDataGroupings);

			public static ZQuery GetFilter(BusinessObjectFactory factory, ZString dataGroupingCode, ZString[] codeTypes, ZDateTime date, IEnumerable<RefCusCodeListAttributeFilter> attributeFilters, bool includeParentDataGroupings)
				=> GetFilter(factory, new[] { dataGroupingCode }, codeTypes, date, attributeFilters, includeParentDataGroupings);

			public static ZQuery GetFilter(BusinessObjectFactory factory, ZString[] dataGroupingCodes, ZString[] codeTypes, ZDateTime date, IEnumerable<RefCusCodeListAttributeFilter> attributeFilters, bool includeParentDataGroupings)
				=> GetFilter(factory, dataGroupingCodes, codeTypes, date, date, attributeFilters, null, includeParentDataGroupings);

			public static ZQuery GetFilter(BusinessObjectFactory factory, ZString dataGroupingCode, ZString codeType, ZDateTime date, ZQuery filter, bool includeParentDataGrouping)
				=> GetFilter(factory, dataGroupingCode, codeType, date, date, filter, includeParentDataGrouping);

			public static ZQuery GetFilter(BusinessObjectFactory factory, ZString dataGroupingCode, ZString codeType, ZDateTime startDate, ZDateTime endDate, ZQuery filter, bool includeParentDataGrouping)
				=> GetFilter(factory, dataGroupingCode, new[] { codeType }, startDate, endDate, null, filter, includeParentDataGrouping);

			static ZQuery GetFilter(BusinessObjectFactory factory, ZString dataGroupingCode, ZString[] codeTypes, ZDateTime startDate, ZDateTime endDate, IEnumerable<RefCusCodeListAttributeFilter> attributeFilters, ZQuery filter, bool includeParentDataGrouping)
				=> GetFilter(factory, new[] { dataGroupingCode }, codeTypes, startDate, endDate, attributeFilters, filter, includeParentDataGrouping);

			static ZQuery GetFilter(BusinessObjectFactory factory, ZString[] dataGroupingCodes, ZString[] codeTypes, ZDateTime startDate, ZDateTime endDate, IEnumerable<RefCusCodeListAttributeFilter> attributeFilters, ZQuery filter, bool includeParentDataGrouping)
			{
				ZQuery result;
				Array.Sort(dataGroupingCodes);
				if (dataGroupingCodes.IsNullOrEmpty() || dataGroupingCodes.Any(dataGroupingCode => dataGroupingCode.IsEmpty) || !startDate.IsValid || !endDate.IsValid || codeTypes.IsNullOrEmpty() || codeTypes.Any(codeType => codeType.IsEmpty))
				{
					result = new ZQuery { IsNoResultQuery = true };
				}
				else
				{
					if (includeParentDataGrouping)
					{
						var dataGroupingList = new List<ZString>();
						foreach (var dataGroupingCode in dataGroupingCodes)
						{
							dataGroupingList.AddRange(RefDataGrouping.GetDataGroupingIncludingParent(factory, dataGroupingCode));
						}
						var sortedDataGroupings = dataGroupingList.Distinct().OrderBy(x => x).ToArray();
						if (sortedDataGroupings.SequenceEqual(dataGroupingCodes))
						{
							result = new ZQuery(ZZRefCusCodeListCombinedSchema.ZZD_CountryOrGrouping, dataGroupingCodes);
						}
						else
						{
							result = new ZDBOnlyQuery(typeof(ZZRefCusCodeListCombined));
							result.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_CountryOrGrouping, sortedDataGroupings);
						}
					}
					else
					{
						result = new ZQuery(ZZRefCusCodeListCombinedSchema.ZZD_CountryOrGrouping, dataGroupingCodes);
					}

					AddDefaultValuesToFilter(result, codeTypes, startDate, endDate, filter, attributeFilters);
				}
				return result;
			}

			public static ZQuery GetFilter(IEnumerable<ZString> dataGroupingCodes, ZString codeType, ZDateTime date, IEnumerable<RefCusCodeListAttributeFilter> attributeFilters)
			{
				ZQuery result;
				if (dataGroupingCodes == null || !dataGroupingCodes.Any() || codeType.IsEmpty || !date.IsValid)
				{
					result = new ZQuery() { IsNoResultQuery = true };
				}
				else
				{
					result = new ZQuery(ZZRefCusCodeListCombinedSchema.ZZD_CountryOrGrouping, dataGroupingCodes);
					AddDefaultValuesToFilter(result, new[] { codeType }, date, date, null, attributeFilters);
				}
				return result;
			}

			static void AddDefaultValuesToFilter(ZQuery query, ZString[] codeTypes, ZDateTime startDate, ZDateTime endDate, ZQuery filter, IEnumerable<RefCusCodeListAttributeFilter> attributeFilters)
			{
				query.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_CodeType, codeTypes);
				if (startDate < ZDateTime.MaxSmallDateTime)
				{
					query.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_StartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, startDate);
				}
				query.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_EndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, endDate);

				if (filter != null)
				{
					query.AddToFilter(filter);
				}

				if (attributeFilters != null && attributeFilters.Any())
				{
					var attributeFilterQuery = new ZQuery();
					foreach (var attributeFilter in attributeFilters)
					{
						var attributeFilterSubQuery = attributeFilter.Filter;
						if (attributeFilterSubQuery != null)
						{
							attributeFilterQuery.AddToFilter(attributeFilterSubQuery, attributeFilter.JoinCondition);
						}
					}
					query.AddToFilter(attributeFilterQuery);
				}
			}

			public static ZQuery GetFilterForStartDateBeforeAndAfterToday(BusinessObjectFactory factory, ZString dataGroupingCode, ZString codeType, ZDateTime dateBefore, ZQuery filter, bool includeParentDataGrouping = true)
				=> GetFilter(factory, dataGroupingCode, new[] { codeType }, dateBefore, ZDateTime.Today.AddDays(1), null, filter, includeParentDataGrouping);

			public static ZZRefCusCodeListCombined[] Load(BusinessObjectFactory factory, ZString dataGroupingCode, ZString codeType, ZDateTime date, IEnumerable<RefCusCodeListAttributeFilter> attributeFilters)
				=> factory.Load<ZZRefCusCodeListCombined>(GetFilter(factory, dataGroupingCode, codeType, date, attributeFilters, false)).ToArray();

			public static ZZRefCusCodeListCombined[] Load(BusinessObjectFactory factory, ZString dataGroupingCode, ZString codeType, ZDateTime date, ZQuery additionalFilter = null, bool addAttributeFetchHints = false, string transportMode = "", bool includeParentDataGrouping = true)
				=> Load(factory, dataGroupingCode, codeType, date, date, additionalFilter, addAttributeFetchHints, transportMode, includeParentDataGrouping);

			public static ZZRefCusCodeListCombined[] Load(BusinessObjectFactory factory, ZString dataGroupingCode, ZString codeType, ZDateTime startDate, ZDateTime endDate, ZQuery additionalFilter = null, bool addAttributeFetchHints = false, string transportMode = "", bool includeParentDataGrouping = true)
			{
				var filter = GetFilter(factory, dataGroupingCode, codeType, startDate, endDate, additionalFilter, includeParentDataGrouping);
				var result = factory.Load<ZZRefCusCodeListCombined>(filter).Where(x => x.HasNoTransportMode || x.MatchTransportMode(transportMode));

				if (includeParentDataGrouping)
				{
					result = GetUniqueCodeListFromDataGrouping(result, dataGroupingCode);
				}

				if (addAttributeFetchHints)
				{
					foreach (var pk in result.Select(x => x.PK))
					{
						factory.AddFetchHint(ZZRefCusCodeListAttributeCombinedSchema.ZZE_ZZD_CodeList, pk);
					}
				}
				return result.ToArray();
			}

			public static ZZRefCusCodeListCombined[] LoadAndFallbackToParentDataGroupingIfNotFound(BusinessObjectFactory factory, ZString dataGroupingCode, ZString codeType, ZDateTime date, ZQuery additionalFilter = null)
			{
				Argument.NotNull(factory, nameof(factory));
				Argument.NotNullOrEmpty(dataGroupingCode, nameof(dataGroupingCode));
				Argument.NotNullOrEmpty(codeType, nameof(codeType));

				var filter = GetFilter(factory, dataGroupingCode, codeType, date, additionalFilter, false);
				var result = factory.Load<ZZRefCusCodeListCombined>(filter);
				if (!result.Any())
				{
					var parentDataGrouping = RefDataGrouping.GetParentDataGroupingCode(factory, dataGroupingCode);
					if (!parentDataGrouping.IsEmpty)
					{
						filter = GetFilter(factory, parentDataGrouping, codeType, date, additionalFilter, false);
						result = factory.Load<ZZRefCusCodeListCombined>(filter);
					}
				}
				return result;
			}

			static ZZRefCusCodeListCombined[] GetUniqueCodeListFromDataGrouping(IEnumerable<ZZRefCusCodeListCombined> source, string dataGroupingCode)
			{
				var result = new List<ZZRefCusCodeListCombined>();
				var groups = source.GroupBy(c => c.ZZD_Code);

				foreach (var group in groups)
				{
					var codeList = group.FirstOrDefault(c => c.ZZD_CountryOrGrouping == dataGroupingCode) ?? group.FirstOrDefault();
					result.Add(codeList);
				}

				return result.ToArray();
			}

			public static ZZRefCusCodeListCombined[] LoadForStartDateBeforeAndAfterToday(BusinessObjectFactory factory, ZString dataGroupingCode, ZString codeType, ZDateTime dateBefore, ZQuery additionalFilter = null, bool addAttributeFetchHints = false, string transportMode = "", bool includeParentDataGrouping = true)
			{
				var filter = GetFilterForStartDateBeforeAndAfterToday(factory, dataGroupingCode, codeType, dateBefore, additionalFilter, includeParentDataGrouping);
				var result = factory.Load<ZZRefCusCodeListCombined>(filter).Where(x => x.HasNoTransportMode || x.MatchTransportMode(transportMode));

				if (addAttributeFetchHints)
				{
					foreach (var pk in result.Select(x => x.PK))
					{
						factory.AddFetchHint(ZZRefCusCodeListAttributeCombinedSchema.ZZE_ZZD_CodeList, pk);
					}
				}
				return result.ToArray();
			}

			public static ZZRefCusCodeListCombined LoadTop1ByParentDataGrouping(BusinessObjectFactory factory, ZString code, ZString parentDataGroupingCode, ZString codeType, ZDateTime date)
			{
				parentDataGroupingCode = Argument.NotNullOrEmpty(parentDataGroupingCode, "parentDataGroupingCode");
				ZZRefCusCodeListCombined result = null;
				if (!codeType.IsEmpty && !code.IsEmpty)
				{
					var dataGroupings = RefDataGrouping.GetChildDataGroupings(factory, parentDataGroupingCode);
					if (dataGroupings.Length > 0)
					{
						var query = new ZQuery(ZZRefCusCodeListCombinedSchema.ZZD_Code, code);
						query.AddToFilter(GetFilter(factory, dataGroupings.Select(x => x.ZZZ_DataGrouping).OrderBy(x => x).ToArray(), new[] { codeType }, date, null, false));
						result = factory.Load<ZZRefCusCodeListCombined>(query).OrderBy(x => x.ZZD_StartDate).FirstOrDefault();
					}
				}
				return result;
			}

			public static ZZRefCusCodeListCombined LoadTop1ByCountry(BusinessObjectFactory factory, ZString code, ZString dataGroupingCode,
				ZString codeType, ZDateTime date, ZQuery filter = null, ZString[] attributeNames = null, bool includeParentDataGrouping = true)
			{
				dataGroupingCode = Argument.NotNullOrEmpty(dataGroupingCode, "dataGroupingCode");
				ZZRefCusCodeListCombined result = null;
				if (!code.IsEmpty && !codeType.IsEmpty)
				{
					var query = new ZQuery(ZZRefCusCodeListCombinedSchema.ZZD_Code, code);
					query.AddToFilter(GetFilter(factory, dataGroupingCode, codeType, date, filter, includeParentDataGrouping));
					IEnumerable<ZZRefCusCodeListCombined> codes = factory.Load<ZZRefCusCodeListCombined>(query);
					if (attributeNames != null)
					{
						codes.ForEach(x => factory.AddFetchHint(ZZRefCusCodeListAttributeCombinedSchema.ZZE_ZZD_CodeList, x.PK));
						codes = codes.Where(x => x.HasMatchingAttributes(attributeNames));
					}
					result = codes.OrderBy(x => x.ZZD_StartDate).FirstOrDefault(x => x.ZZD_CountryOrGrouping == dataGroupingCode);
					if (result == null)
					{
						result = codes.OrderBy(x => x.ZZD_StartDate).FirstOrDefault();
					}
				}
				return result;
			}

			public static ZZRefCusCodeListCombined LoadTop1ByCountryAndAttributes(BusinessObjectFactory factory, ZString code, ZString dataGroupingCode, ZString codeType, ZDateTime date, ZQuery filter = null, IEnumerable<RefCusCodeListAttributeFilter> attributeFilters = null, bool includeParentDataGrouping = true)
			{
				dataGroupingCode = Argument.NotNullOrEmpty(dataGroupingCode, "dataGroupingCode");
				ZZRefCusCodeListCombined result = null;
				if (!code.IsEmpty && !codeType.IsEmpty)
				{
					var query = new ZQuery(ZZRefCusCodeListCombinedSchema.ZZD_Code, code);
					query.AddToFilter(GetFilter(factory, dataGroupingCode, new[] { codeType }, date, date, attributeFilters, filter, includeParentDataGrouping));
					IEnumerable<ZZRefCusCodeListCombined> codes = factory.Load<ZZRefCusCodeListCombined>(query);
					result = codes.OrderBy(x => x.ZZD_StartDate).FirstOrDefault();
				}
				return result;
			}

			public static ZZRefCusCodeListCombined LoadTop1ByCountryAndCodeType(BusinessObjectFactory factory, ZString dataGroupingCode, ZString codeType, ZDateTime date, bool includeParentDataGrouping = true)
			{
				dataGroupingCode = Argument.NotNullOrEmpty(dataGroupingCode, "dataGroupingCode");
				ZZRefCusCodeListCombined result = null;
				if (!codeType.IsEmpty)
				{
					var query = new ZQuery();
					query.AddToFilter(GetFilter(factory, dataGroupingCode, new[] { codeType }, date, date, null, null, includeParentDataGrouping));
					var codes = factory.Load<ZZRefCusCodeListCombined>(query);
					result = codes.OrderBy(x => x.ZZD_StartDate).FirstOrDefault();
				}
				return result;
			}

			public static ZZRefCusCodeListCombined[] LoadAllCountries(BusinessObjectFactory factory, ZString codeType, ZDateTime date, ZQuery filter = null, ZString[] attributeNames = null)
			{
				var query = new ZQuery();
				if (!date.IsValid || codeType.IsEmpty)
				{
					query.IsNoResultQuery = true;
				}
				else
				{
					AddDefaultValuesToFilter(query, new[] { codeType }, date, date, filter, null);
					query.OrderBy = ZZRefCusCodeListCombinedSchema.Constants.ZZD_CodeType + ", " + ZZRefCusCodeListCombinedSchema.Constants.ZZD_Code;
				}
				var codes = factory.Load<ZZRefCusCodeListCombined>(query);
				if (attributeNames != null)
				{
					foreach (var pk in codes.Select(x => x.PK))
					{
						factory.AddFetchHint(ZZRefCusCodeListAttributeCombinedSchema.ZZE_ZZD_CodeList, pk);
					}
					codes = codes.Where(x => x.HasMatchingAttributes(attributeNames)).ToArray();
				}
				return codes;
			}

			public static ZZRefCusCodeListCombined[] LoadForCodes(BusinessObjectFactory factory, ZString dataGroupingCode, ZString codeType, ZString[] codes, ZDateTime date, IEnumerable<RefCusCodeListAttributeFilter> attributeFilters)
			{
				var query = new ZQuery(ZZRefCusCodeListCombinedSchema.ZZD_Code, codes);
				query.AddToFilter(GetFilter(factory, dataGroupingCode, codeType, date, attributeFilters, false));
				return factory.Load<ZZRefCusCodeListCombined>(query);
			}

			public static ZZRefCusCodeListCombined[] LoadByCode(BusinessObjectFactory factory, ZString dataGroupingCode, ZString[] codeTypes, ZString code, ZDateTime date)
			{
				ZQuery query;
				if (code.IsEmpty)
				{
					query = new ZQuery() { IsNoResultQuery = true };
				}
				else
				{
					query = GetFilter(factory, dataGroupingCode, codeTypes, date, date, null, new ZQuery(ZZRefCusCodeListCombinedSchema.ZZD_Code, code), false);
				}
				return factory.Load<ZZRefCusCodeListCombined>(query);
			}

			public static bool ExistsWithinEffectiveDate(BusinessObjectFactory factory, ZString dataGroupingCode, ZString codeType, ZString code, ZDateTime effectiveDate, bool includeParentDataGrouping)
			{
				var filter = GetFilter(factory, dataGroupingCode, codeType, effectiveDate, new ZQuery(ZZRefCusCodeListCombinedSchema.ZZD_Code, code), includeParentDataGrouping);
				return factory.Exists(typeof(ZZRefCusCodeListCombined), filter);
			}

			protected override Type GetTypeOfBusinessObjectToLoad() => typeof(ZZRefCusCodeListCombined);
		}

		public static ZString[] GetUniqueCodes(BusinessObjectFactory factory, ZString dataGroupingCode, ZString codeType, ZDateTime date, bool includeParentDataGroupings = true)
		{
			return factory.GetCachedValue(string.Join("|", "ZZRefCusCodeListCombined|UniqueCodes", dataGroupingCode, codeType, date.ToShortDateString(), includeParentDataGroupings.ToString()),
				() =>
				{
					var filter = ZZRefCusCodeListCombined.Loader.GetFilter(factory, dataGroupingCode, codeType, date, attributeFilters: null, includeParentDataGroupings: includeParentDataGroupings);
					var sqlText = string.Format(CultureInfo.InvariantCulture, @"
			SELECT DISTINCT ZZD_Code
			FROM ZZRefCusCodeListCombined
				{0}
			OPTION (RECOMPILE)"
						, filter.GetAsWhereClause(false)); // SuppressCod)eSmell Reason = A part of SQL expression.

					var collection = new DynamicBusinessObjectCollection(factory);
					collection.Load(sqlText, new ZSqlParameterCollection(filter.Params));
					return collection.Select(x => new ZString(x[ZZRefCusCodeListCombined.Schema.ZZD_Code])).ToArray();
				});
		}

		public bool MatchTransportMode(ZString tranportMode)
		{
			var result = false;
			switch (tranportMode)
			{
				case (RefTransportModeList.Codes.AIR):
					result = ZZD_IsAir;
					break;
				case (RefTransportModeList.Codes.FIX):
					result = ZZD_IsFix;
					break;
				case (RefTransportModeList.Codes.INW):
					result = ZZD_IsInw;
					break;
				case (RefTransportModeList.Codes.MAI):
					result = ZZD_IsMai;
					break;
				case (RefTransportModeList.Codes.RAI):
					result = ZZD_IsRai;
					break;
				case (RefTransportModeList.Codes.ROA):
					result = ZZD_IsRoa;
					break;
				case (RefTransportModeList.Codes.SEA):
					result = ZZD_IsSea;
					break;
				default:
					result = true;
					break;
			}
			return result;
		}

		public bool HasNoTransportMode
		{
			get
			{
				return !ZZD_IsAir && !ZZD_IsFix && !ZZD_IsInw && !ZZD_IsMai && !ZZD_IsRai && !ZZD_IsRoa && !ZZD_IsSea;
			}
		}

		/// <summary>
		/// This method returns true when all attribute names are matched or when there is no attribute and no attribute name is specified.
		/// </summary>
		public bool HasMatchingAttributes(ZString[] attributeNames)
		{
			var result = false;
			if (attributeNames != null)
			{
				if (attributeNames.Length == 0)
				{
					result = Attributes.Count == 0;
				}
				else
				{
					var attributesToCheck = Attributes.Cast<ZZRefCusCodeListAttributeCombined>();
					result = attributeNames.All(attributeName => attributesToCheck.Any(y => y.ZZE_ZXE_NKName.EqualsIgnoringCase(attributeName)));
				}
			}
			return result;
		}

		public bool HasAttribute(ZString name, ZString? attrValue = null, ZDateTime? date = null)
		{
			var result = false;
			var effectiveDate = date ?? ZDateTime.Today;

			if (attrValue.HasValue)
			{
				result = Attributes.Cast<ZZRefCusCodeListAttributeCombined>().Any(x => x.ZZE_ZXE_NKName.EqualsIgnoringCase(name) && x.ZZE_Value.EqualsIgnoringCase(attrValue.Value) && DatePredicate(x, effectiveDate));
			}
			else
			{
				result = Attributes.Cast<ZZRefCusCodeListAttributeCombined>().Any(x => x.ZZE_ZXE_NKName.EqualsIgnoringCase(name) && DatePredicate(x, effectiveDate));
			}

			return result;
		}

		ZZRefCusCodeListAttributeCombined FindAttributeByName(string attributeName, ZDateTime? date = null)
		{
			var effectiveDate = date ?? ZDateTime.Today;

			return Attributes.OfType<ZZRefCusCodeListAttributeCombined>().FirstOrDefault(x => x.ZZE_ZXE_NKName.EqualsIgnoringCase(attributeName) && DatePredicate(x, effectiveDate));
		}

		bool DatePredicate(ZZRefCusCodeListAttributeCombined item, ZDateTime effectiveDate) => (item.ZZE_StartDate.IsEmpty || item.ZZE_StartDate <= effectiveDate)
																	  && (item.ZZE_EndDate.IsEmpty || item.ZZE_EndDate >= effectiveDate);

		public ZString GetAttribute(ZString attributeName, ZDateTime? date = null)
		{
			return FindAttributeByName(attributeName, date)?.ZZE_Value ?? ZString.Empty;
		}

		public object GetAttribute(RefCusCodeListAttributeName attributeName, ZDateTime? date = null)
		{
			var resultStr = GetAttribute(attributeName.ZXE_Name, date);
			object result = null;
			if (!resultStr.IsEmpty)
			{
				switch (attributeName.ZXE_ValueDataType.ToUpper())
				{
					case "":
					case Constants.RefCusCodeListAttributeName.ValueDataTypes.String:
						result = resultStr;
						break;
					case Constants.RefCusCodeListAttributeName.ValueDataTypes.Boolean:
						result = ZBool.ParseSafe(resultStr, ZBool.False);
						break;
					case Constants.RefCusCodeListAttributeName.ValueDataTypes.Integer:
						result = ZInt.ParseSafe(resultStr, ZInt.Zero);
						break;
					case Constants.RefCusCodeListAttributeName.ValueDataTypes.Decimal:
						result = ZDecimal.ParseSafe(resultStr, ZDecimal.Zero);
						break;
				}
			}
			return result;
		}

		public bool SetAttribute(RefCusCodeListAttributeName attributeName, object value)
		{
			var result = false;
			var attrToSet = FindAttributeByName(attributeName.ZXE_Name);
			if (attrToSet != null)
			{
				attrToSet.ZZE_Value = value.ToString();
				result = true;
			}
			return result;
		}

		/// <summary>
		/// If the ZZD has attributes A="1", A="2", A="3" and B="4", then GetAttributesValues(A) will return {"1", "2", "3"}
		/// </summary>
		public IEnumerable<ZString> GetAttributesValues(ZString attributeName, ZDateTime? date = null)
		{
			var effectiveDate = date ?? ZDateTime.Today;

			return Attributes.OfType<ZZRefCusCodeListAttributeCombined>().Where(x => x.ZZE_ZXE_NKName.EqualsIgnoringCase(attributeName) && DatePredicate(x, effectiveDate)).Select(x => x.ZZE_Value);
		}

		#region New Properties

		[ResourceStringData("Enterprise.Customs.Universal.ZZRefCusCodeListCombined|ZZD_CodeTypeDesc", Caption = "List Description", ShortCaption = "List Desc.")]
		public ZString ZZD_CodeTypeDesc
		{
			get { return Lookups.CodeTypeList.GetDescriptionFromCode(ZZD_CodeType); }
		}

		public ZPropertyInfo ZZD_CodeTypeDescInfo
		{
			get { return GetZPropertyInfo(Schema.ZZD_CodeTypeDesc); }
		}

		public RefCusCodeListAttributeName[] GetCodeListAttributeNames()
		{
			return Factory.GetCachedValue(string.Format(CultureInfo.InvariantCulture, "GetCodeListAttributeNames_{0}_{1}", ZZD_CodeType, ZZD_CountryOrGrouping), () =>
			{
				var query = new ZQuery(RefCusCodeListAttributeNameSchema.ZXE_ZZK_NKCodeType, ZZD_CodeType);
				query.AddToFilter(RefCusCodeListAttributeNameSchema.ZXE_ZZZ_NKDataGrouping, ZZD_CountryOrGrouping);
				return Factory.Load<RefCusCodeListAttributeName>(query);
			});
		}

		#region Transport Modes

		[ResourceStringData("Enterprise.Customs.Universal.ZZRefCusCodeListCombined|ZZD_TransportModes", Caption = "Transport Modes", ShortCaption = "Trans. Modes")]
		public ZString ZZD_TransportModes => ZString.Join(",", TransportModes.ToArray());

		public ZPropertyInfo ZZD_TransportModesInfo => GetZPropertyInfo(Schema.ZZD_TransportModes);

		public ZBoolDescriptionPairList TransportModePairList => fTransportModePairList ?? (fTransportModePairList = this.CreateNewTransportModePairList());
		ZBoolDescriptionPairList fTransportModePairList;

		public IEnumerable<ZString> TransportModes => TransportModePairList.Where(x => x.Value).Select(x => x.Description);

		public static string GetTransportModePropertyName(ZString transportMode)
		{
			switch (transportMode)
			{
				case RefTransportModeList.Codes.AIR:
					return AutoZZRefCusCodeListCombined.Schema.ZZD_IsAir;
				case RefTransportModeList.Codes.SEA:
					return AutoZZRefCusCodeListCombined.Schema.ZZD_IsSea;
				case RefTransportModeList.Codes.FIX:
					return AutoZZRefCusCodeListCombined.Schema.ZZD_IsFix;
				case RefTransportModeList.Codes.RAI:
					return AutoZZRefCusCodeListCombined.Schema.ZZD_IsRai;
				case RefTransportModeList.Codes.ROA:
					return AutoZZRefCusCodeListCombined.Schema.ZZD_IsRoa;
				case RefTransportModeList.Codes.MAI:
					return AutoZZRefCusCodeListCombined.Schema.ZZD_IsMai;
				case RefTransportModeList.Codes.INW:
					return AutoZZRefCusCodeListCombined.Schema.ZZD_IsInw;
				default:
					throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "Transport Mode {0} is not supported for ZZRefCusCodeListCombined.", transportMode));
			}
		}

		string ITransportModeListSupporter.GetTransportModePropertyName(ZString transportMode)
		{
			return GetTransportModePropertyName(transportMode);
		}

		ZPropertyInfo ITransportModeListSupporter.TransportModesPropertyInfo => ZZD_TransportModesInfo;

		#endregion

		#endregion

		#region Override Properties

		[ReadOnly(true)]
		public override ZBool ZZD_IsSystem
		{
			get { return base.ZZD_IsSystem; }
			set
			{
				var oldValue = ZZD_IsSystem;
				base.ZZD_IsSystem = value;
				if (!IsCopying && oldValue != ZZD_IsSystem)
				{
					Attributes.MarkAsNeedingValidation();
				}
			}
		}

		[List("Lookups.CodeTypeList")]
		[RelatedBusinessObject("CusCodeType")]
		public override ZString ZZD_CodeType
		{
			get { return base.ZZD_CodeType; }
			set
			{
				var oldValue = ZZD_CodeType;
				base.ZZD_CodeType = value;
				if (!IsCopying && oldValue != ZZD_CodeType)
				{
					Attributes.MarkAsNeedingValidation();
				}
			}
		}

		public RefCusCodeType CusCodeType
		{
			get
			{
				var query = new ZQuery(RefCusCodeTypeSchema.ZZK_CodeType, ZZD_CodeType);
				query.AddToFilter(RefCusCodeTypeSchema.ZZK_ZZZ_NKDataGrouping, new ZString[] { ZZD_CountryOrGrouping, ZString.Empty });
				return Factory.Load<RefCusCodeType>(query).OrderByDescending(x => x.ZZK_ZZZ_NKDataGrouping).FirstOrDefault();
			}
		}

		[List("Lookups.CountryOrGroupingList")]
		public override ZString ZZD_CountryOrGrouping
		{
			get { return base.ZZD_CountryOrGrouping; }
			set { base.ZZD_CountryOrGrouping = value; }
		}

		public override bool SupportsNotes
		{
			get { return false; }
		}

		public override bool ReadOnly
		{
			get { return ZZD_IsSystem || base.ReadOnly; }
			set { base.ReadOnly = value; }
		}

		public override ZString ZZD_Description
		{
			get => TranslationHelper.GetTranslatedValue(this, base.ZZD_Description, RefCusCodeListLanguageSchema.ZXA_Description);
			set => base.ZZD_Description = value;
		}

		#endregion

		#region Collection

		[ChildEditable]
		public ZZRefCusCodeListAttributeCombinedCollection Attributes
		{
			get
			{
				if (attributes == null)
				{
					attributes = new ZZRefCusCodeListAttributeCombinedCollection(this);
					attributes.Load();
					RegisterEditableChildObject(attributes);
				}
				return attributes;
			}
		}
		ZZRefCusCodeListAttributeCombinedCollection attributes;

		[ChildEditable]
		public ZZRefCusCodeListLanguageCombinedCollection Languages
		{
			get
			{
				if (languages == null)
				{
					languages = new ZZRefCusCodeListLanguageCombinedCollection(this);
					languages.Load();
					RegisterEditableChildObject(languages);
				}
				return languages;
			}
		}
		ZZRefCusCodeListLanguageCombinedCollection languages;

		#endregion

		#region Override Method

		public override void Delete()
		{
			Attributes.RemoveAndDeleteAll();
			base.Delete();
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new Strategy(this);
		}

		class Strategy : TranslatableZZBusinessObjectFetchStrategy<ZZRefCusCodeListCombined>
		{
			public Strategy(ZZRefCusCodeListCombined cusCodeList)
				: base(cusCodeList)
			{
			}

			protected new ZZRefCusCodeListCombined BusinessObject
			{
				get { return (ZZRefCusCodeListCombined)base.BusinessObject; }
			}

			protected override void FetchForLoadChildEditableObjectsCore()
			{
				base.FetchForLoadChildEditableObjectsCore();
				Factory.AddFetchHint(ZZRefCusCodeListAttributeCombinedSchema.ZZE_ZZD_CodeList, BusinessObject.PK);
			}

			protected override void FetchForViewCore(TableColumn[] columns)
			{
				base.FetchForViewCore(columns);

				foreach (var column in columns)
				{
					var columnName = column.ColumnName;
					if (columnName == ZZRefCusCodeListCombined.Schema.ZZD_CodeTypeDesc)
					{
						Factory.AddFetchHint(RefCusCodeTypeSchema.ZZK_CodeType, BusinessObject.ZZD_CodeType);
					}
				}
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ZZD_StartDate = ZDateTime.Today;
			ZZD_EndDate = ZDateTime.MaxSmallDateTimeValue;
			ZZD_CountryOrGrouping = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var result = (ZZRefCusCodeListCombined)base.CloneInternal(args);
			using (result.GetValidationSuspender())
			{
				foreach (ZZRefCusCodeListAttributeCombined attribute in Attributes)
				{
					result.Attributes.Add((ZZRefCusCodeListAttributeCombined)attribute.Clone());
				}
			}
			return result;
		}

		#endregion

		#region ICanDelete Members

		public override bool CanDelete
		{
			get { return !ZZD_IsSystem; }
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get { return ZZD_IsSystem ? CannotDeleteSystemGenerated : (NoResString)string.Empty; }
		}

		internal static MultilingualString CannotDeleteSystemGenerated
		{
			get { return ResString.GetMultilingualString("{2B97FEC8-F924-455E-A528-3C73B81B6C06}", "You cannot delete a system-generated record."); }
		}

		#endregion

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		string IStmALogParent.LogsParentTableName
		{
			get { return ZZRefCusCodeListCombined.Schema.TableName; }
		}

		#region ITemplateCopyable Members

		public IBusiness TemplateCopy()
		{
			var result = (ZZRefCusCodeListCombined)Clone();
			result.ZZD_IsSystem = ZBool.False;
			return result;
		}

		#endregion

		#region ITranslatableZZBusinessObject

		public ITableSchema LanguageTableSchema => RefCusCodeListLanguageSchema.Instance;
		public Type LanguageTableType => typeof(RefCusCodeListLanguage);

		#endregion

#if DEBUG
		protected override BusinessObjectTestDataHelper NewBusinessObjectTestDataHelper()
		{
			return new UniversalReferenceBOTestDataHelper();
		}

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			ZZD_CodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice;
			ZZD_Code = "BOB";
			ZZD_Description = "BOB THE BUILDER";
			ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Eritrea;
			if (ZZD_StartDate >= ZZD_EndDate)
			{
				var swapHolder = ZZD_StartDate;
				ZZD_StartDate = ZZD_EndDate;
				ZZD_EndDate = swapHolder;
			}
		}
#endif
	}
}
