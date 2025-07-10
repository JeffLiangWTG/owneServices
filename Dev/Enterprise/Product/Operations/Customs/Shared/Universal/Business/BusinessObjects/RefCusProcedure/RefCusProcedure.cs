using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
#if NETFRAMEWORK
using CargoWise.Common;
#endif
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.Universal
{
	[CodeProperty("FullCodeCurrentPlusPreviousPlusConcession"), DescriptionProperty(Schema.ZZ6_Description)]
	public sealed class RefCusProcedure : AutoRefCusProcedure, ITranslatableZZBusinessObject
	{
		public RefCusProcedure(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override ZString ZZ6_Description
		{
			get => TranslationHelper.GetTranslatedValue(this, base.ZZ6_Description, RefCusProcedureLanguageSchema.ZXV_Description);
			set => base.ZZ6_Description = value;
		}

		public new class Schema : AutoRefCusProcedure.Schema
		{
			public const string ZZ6_EndDate_ForDisplay = "ZZ6_EndDate_ForDisplay";
		}

		protected override ZString HumanReadableNameCore
		{
			get { return ResString.GetMultilingualString("1B6DCEB3-7364-45BB-BE24-828547C943E1", "Cus. Procedure"); }
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ZZ6_StartDate = ZDateTime.MinSmallDateTimeValue;
			ZZ6_EndDate = ZDateTime.MaxSmallDateTimeValue;
		}

		public static ZString GetProcedureDescription(BusinessObjectFactory factory, ZString dataGrouping, ZString procedureCode) =>
			GetDescription(factory, dataGrouping, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProcedureCode, procedureCode);

		public static ZString GetConcessionDescription(BusinessObjectFactory factory, ZString dataGrouping, ZString concessionCode) =>
			GetDescription(factory, dataGrouping, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdvancedProcedureCode, concessionCode);

		static ZString GetDescription(BusinessObjectFactory factory, ZString dataGrouping, ZString codeType, ZString code)
		{
			var date = ZDateTime.Today;
			var codeTypeDictionary = factory.GetCachedValue<IDictionary<ZString, IDictionary<ZString, ZString>>>(
				$"BaseRefCusProcedure.{dataGrouping}.{TranslationHelper.GetCurrentLanguageCode()}.{date.ToShortDateString()}.Dictionary",
				() => new Dictionary<ZString, IDictionary<ZString, ZString>>());
			var dictionary = CargoWise.Common.IDictionaryExtensions.GetOrAdd(codeTypeDictionary, codeType, () => GetDescriptionDictionary(factory, dataGrouping, codeType, date));
			return dictionary.TryGetValue(code, out var description)
				? description
				: code;
		}

		static IDictionary<ZString, ZString> GetDescriptionDictionary(BusinessObjectFactory factory, ZString dataGrouping, ZString codeType, ZDateTime date)
		{
			return ZZRefCusCodeListCombined.Loader.Load(factory, dataGrouping, codeType, date)
				.GroupBy(item => item.ZZD_Code)
				.Select(group =>
				{
					ZZRefCusCodeListCombined first = null;
					foreach (var item in group)
					{
						if (item.ZZD_CountryOrGrouping.EqualsIgnoringCase(dataGrouping))
						{
							return item;
						}
						else if (first == null)
						{
							first = item;
						}
					}
					return first;
				})
				.DistinctBy(item => item.ZZD_Code).ToDictionary(item => item.ZZD_Code, item => item.ZZD_Description);
		}

		public static CodeDescriptionPairList GetCachedList(BusinessObjectFactory factory, ZString dataGroupingCode, ZDateTime date, ZString declarationType, ZString shipmentType)
		{
			CodeDescriptionPairList result = null;
			if (factory == null)
			{
				result = new CodeDescriptionPairList();
			}
			else
			{
				var key = GetKey(dataGroupingCode, date.Date, declarationType, shipmentType);
				result = factory.GetCachedValue("RefCusProcedure" + key, () =>
				{
					var list = new CodeDescriptionPairList();
					var customsProcedures = new RefCusProcedureCollection(factory, dataGroupingCode, date, declarationType, shipmentType);
					customsProcedures.ApplySort(nameof(FullCodeCurrentPlusPreviousPlusConcession), System.ComponentModel.ListSortDirection.Ascending);
					foreach (var codeList in customsProcedures)
					{
						list.AddPair(codeList.ZZ6_ProcedureCode, codeList.ZZ6_Description);
					}
					return list;
				});
			}
			return result;
		}

		static ZString GetKey(ZString dataGroupingCode, ZDate date, ZString declarationType, ZString shipmentType)
		{
			var result = new ZStringBuilder(dataGroupingCode);
			result.Append(date.ToString("yyMMdd", CultureInfo.InvariantCulture));
			result.Append(declarationType);
			result.Append(shipmentType);

			return result.ToStringWithDelimiterBetweenAppends("_");
		}

		public ZString FullCodeCurrentPlusPreviousPlusConcession => ZZ6_ProcedureCode + ZZ6_PreviousProcedureCode + ZZ6_Concession;

		public ZBool HasAttribute(string code) => Attributes.Any(a => a.ZXB_Name == code);

		public ZString GetAttributeValue(ZString code, ZString defaultValue)
		{
			var result = defaultValue;
			if (HasAttribute(code))
			{
				result = GetAttributeValues(code).First();
			}

			return result;
		}

		public IEnumerable<ZString> GetAttributeValues(ZString code)
		{
			return Attributes.Where(x => x.ZXB_Name.EqualsIgnoringCase(code)).Select(x => x.ZXB_Value);
		}

		#region New Properties

		public IEnumerable<ZString> Concessions => ZZ6_Concession.Split(',').Select(x => x.Trim());

		public IEnumerable<ZString> ShipmentTypes => ZZ6_ShipmentType.Split(CommaAndSemiColon).Select(x => x.Trim());

		public IEnumerable<ZString> Groups => ZZ6_Group.Split(CommaAndSemiColon).Select(x => x.Trim());

		[ChildEditable]
		public RefCusProcedureAttributeCollection Attributes
		{
			get
			{
				if (attributes == null)
				{
					attributes = new RefCusProcedureAttributeCollection(this);
					RegisterEditableChildObject(attributes);
				}
				return attributes;
			}
		}
		RefCusProcedureAttributeCollection attributes;

		public ZDateTime ZZ6_EndDate_ForDisplay => ZZ6_EndDate == ZDateTime.MaxSmallDateTime ? ZDateTime.Empty : ZZ6_EndDate;

		public ZPropertyInfo ZZ6_EndDate_ForDisplayInfo => GetZPropertyInfo(Schema.ZZ6_EndDate_ForDisplay);

		#endregion

		#region Warehousing & Processing

		public bool IsIntoRegime() => Factory.GetValue(ref isIntoRegimeCached, () => IsIntoWarehouse() || IsIntoInwardProcessing() || IsIntoOutwardProcessing());
		CachedProperty<bool> isIntoRegimeCached;

		public bool IsOutOfRegime() => Factory.GetValue(ref isOutOfRegimeCached, () => IsOutOfWarehouse() || IsOutOfInwardProcessing() || IsOutOfOutwardProcessing());
		CachedProperty<bool> isOutOfRegimeCached;

		public ZBool IsIntoWarehouse() => IsIntoWarehouse(null);

		public ZBool IsOutOfWarehouse() => IsOutOfWarehouse(null);

		public ZBool IsGuaranteeConsumed() => ZZ6_IsGuaranteeConsumed == YesNoList.Codes.Yes;

		public ZBool IsGuaranteeReleased() => ZZ6_IsGuaranteeReleased == YesNoList.Codes.Yes;

		public ZBool IsTransit() => ZZ6_IsTransit == YesNoList.Codes.Yes;

		public ZBool IsIntoInwardProcessing() => IsIntoInwardProcessing(null);

		public ZBool IsOutOfInwardProcessing() => IsOutOfInwardProcessing(null);

		public ZBool IsIntoOutwardProcessing() => IsIntoOutwardProcessing(null);

		public ZBool IsOutOfOutwardProcessing() => IsOutOfOutwardProcessing(null);

		public ZBool IsIntoTemporaryImport() => IsIntoTemporaryImport(null);

		public ZBool IsOutOfTemporaryImport() => IsOutOfTemporaryImport(null);

		public ZBool IsIntoTemporaryExport() => IsIntoTemporaryExport(null);

		public ZBool IsOutOfTemporaryExport() => IsOutOfTemporaryExport(null);

		public ZBool IsIntoVATWarehouse() => IsIntoVATWarehouse(null);

		public ZBool IsIntoWarehouse(IRefCusProcedureContext refCusProcedureContext) => DeterminesBooleanValue(refCusProcedureContext, ZZ6_IntoWarehouse, x => x.IsIntoWarehouse);

		public ZBool IsIntoVATWarehouse(IRefCusProcedureContext refCusProcedureContext) => DeterminesBooleanValue(refCusProcedureContext, ZZ6_IntoVATWarehouse, x => x.IsIntoVATWarehouse);

		public ZBool IsOutOfWarehouse(IRefCusProcedureContext refCusProcedureContext) => DeterminesBooleanValue(refCusProcedureContext, ZZ6_OutOfWarehouse, x => x.IsOutOfWarehouse);

		public ZBool IsIntoInwardProcessing(IRefCusProcedureContext refCusProcedureContext) => DeterminesBooleanValue(refCusProcedureContext, ZZ6_IntoInwardProcessing, x => x.IsIntoInwardProcessing);

		public ZBool IsOutOfInwardProcessing(IRefCusProcedureContext refCusProcedureContext) => DeterminesBooleanValue(refCusProcedureContext, ZZ6_OutOfInwardProcessing, x => x.IsOutOfInwardProcessing);

		public ZBool IsIntoOutwardProcessing(IRefCusProcedureContext refCusProcedureContext) => DeterminesBooleanValue(refCusProcedureContext, ZZ6_IntoOutwardProcessing, x => x.IsIntoOutwardProcessing);

		public ZBool IsOutOfOutwardProcessing(IRefCusProcedureContext refCusProcedureContext) => DeterminesBooleanValue(refCusProcedureContext, ZZ6_OutofOutwardProcessing, x => x.IsOutOfOutwardProcessing);

		public ZBool IsIntoTemporaryImport(IRefCusProcedureContext refCusProcedureContext) => DeterminesBooleanValue(refCusProcedureContext, ZZ6_IntoTemporaryImport, x => x.IsIntoTemporaryImport);

		public ZBool IsOutOfTemporaryImport(IRefCusProcedureContext refCusProcedureContext) => DeterminesBooleanValue(refCusProcedureContext, ZZ6_OutOfTemporaryImport, x => x.IsOutOfTemporaryImport);

		public ZBool IsIntoTemporaryExport(IRefCusProcedureContext refCusProcedureContext) => DeterminesBooleanValue(refCusProcedureContext, ZZ6_IntoTemporaryExport, x => x.IsIntoTemporaryExport);

		public ZBool IsOutOfTemporaryExport(IRefCusProcedureContext refCusProcedureContext) => DeterminesBooleanValue(refCusProcedureContext, ZZ6_OutOfTemporaryExport, x => x.IsOutOfTemporaryExport);

		ZBool DeterminesBooleanValue(IRefCusProcedureContext refCusProcedureContext, ZString businessPropertyValue, Func<IRefCusProcedureContext, ZBool> getContextValue) => refCusProcedureContext != null && businessPropertyValue == WarehouseMoveStatus.Codes.Indeterminate ? getContextValue(refCusProcedureContext) : (ZBool)(businessPropertyValue == WarehouseMoveStatus.Codes.Yes);
		#endregion

		public override void Delete()
		{
			Attributes.DeleteAll();
			base.Delete();
		}

		[RelatedBusinessObject("DataGrouping")]
		public override ZString ZZ6_ZZZ_NKDataGrouping
		{
			get { return base.ZZ6_ZZZ_NKDataGrouping; }
			set { base.ZZ6_ZZZ_NKDataGrouping = value; }
		}

		public RefDataGrouping DataGrouping
		{
			get { return Factory.LoadFromNaturalKey<RefDataGrouping>(RefDataGroupingSchema.ZZZ_DataGrouping, ZZ6_ZZZ_NKDataGrouping); }
		}

		#region ITranslatableZZBusinessObject

		CargoWise.Schema.ITableSchema ITranslatableZZBusinessObject.LanguageTableSchema => RefCusProcedureLanguageSchema.Instance;

		Type ITranslatableZZBusinessObject.LanguageTableType => typeof(RefCusProcedureLanguage);

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Loader : AutoRefCusProcedure.Loader
		{
			public Loader(BusinessObjectFactory factory) : base(factory)
			{
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(RefCusProcedure);
			}

			public RefCusProcedure LoadTop1FromFullCodeCurrentPlusPreviousPlusConcession(ZString fullCodeCurrentPlusPreviousPlusConcession, ZString dataGrouping, ZDateTime date)
			{
				RefCusProcedure result = null;
				if (!fullCodeCurrentPlusPreviousPlusConcession.IsEmpty)
				{
					var procedureCode = fullCodeCurrentPlusPreviousPlusConcession.SubstringSafe(0, 2);
					var previousProcedureCode = fullCodeCurrentPlusPreviousPlusConcession.SubstringSafe(2, 2);
					var concession = fullCodeCurrentPlusPreviousPlusConcession.SubstringSafe(4, 3);
					var cpcQuery = GetFilter(dataGrouping, date);
					cpcQuery.AddToFilter(RefCusProcedureSchema.ZZ6_ProcedureCode, procedureCode);
					if (!previousProcedureCode.IsEmpty)
					{
						cpcQuery.AddToFilter(RefCusProcedureSchema.ZZ6_PreviousProcedureCode, previousProcedureCode);
						if (!concession.IsEmpty)
						{
							cpcQuery.AddToFilter(RefCusProcedureSchema.ZZ6_Concession, concession);
						}
					}

					result = Factory.LoadTop1<RefCusProcedure>(cpcQuery);
				}

				return result;
			}

			public RefCusProcedure LoadTop1FromCodeAndCountry(ZString procedureCode, ZString previousProcedureCode, ZString dataGroupingCode, ZDateTime date, ZString? group = null, ZString? concession = null)
			{
				RefCusProcedure result = null;
				if (!procedureCode.IsEmpty && !dataGroupingCode.IsEmpty)
				{
					var query = GetFilter(dataGroupingCode, date);
					query.AddToFilter(RefCusProcedureSchema.ZZ6_ProcedureCode, procedureCode);
					query.AddToFilter(RefCusProcedureSchema.ZZ6_PreviousProcedureCode, previousProcedureCode);
					if (group.HasValue)
					{
						query.AddToFilter(RefCusProcedureSchema.ZZ6_Group, group.Value);
					}
					if (concession.HasValue)
					{
						query.AddToFilter(RefCusProcedureSchema.ZZ6_Concession, concession.Value);
					}
					result = Factory.LoadTop1<RefCusProcedure>(query);
				}
				return result;
			}

			public RefCusProcedure LoadFromProcedureAndPreviousProcedureAndConcession(ZString procedureCode, ZString previousProcedureCode, ZString concession, ZString category, ZString dataGroupingCode, ZDateTime date)
			{
				RefCusProcedure result = null;
				if (!procedureCode.IsEmpty && !dataGroupingCode.IsEmpty)
				{
					var query = GetFilter(dataGroupingCode, date);
					query.AddToFilter(RefCusProcedureSchema.ZZ6_ProcedureCode, procedureCode);
					query.AddToFilter(RefCusProcedureSchema.ZZ6_PreviousProcedureCode, previousProcedureCode);
					query.AddToFilter(RefCusProcedureSchema.ZZ6_Concession, concession);
					query.AddToFilter(RefCusProcedureSchema.ZZ6_Category, category);
					result = Factory.LoadTop1<RefCusProcedure>(query);
				}
				return result;
			}

			public RefCusProcedure[] LoadForZzzDataGrouping(ZString dataGroupingCode)
			{
				var results = Array.Empty<RefCusProcedure>();
				if (!dataGroupingCode.IsEmpty)
				{
					var query = new ZQuery(RefCusProcedureSchema.ZZ6_ZZZ_NKDataGrouping, dataGroupingCode);
					results = Factory.Load<RefCusProcedure>(query);
				}
				return results;
			}

			public RefCusProcedure[] LoadForShipmentTypeAndZzzDataGrouping(ZString exactlyOneShipmentType, ZString dataGroupingCode)
			{
				var results = Array.Empty<RefCusProcedure>();
				if (!exactlyOneShipmentType.IsEmpty && !dataGroupingCode.IsEmpty)
				{
					var query = new ZQuery(RefCusProcedureSchema.ZZ6_ZZZ_NKDataGrouping, dataGroupingCode);
					query.AddToFilter(RefCusProcedureSchema.ZZ6_ShipmentType, SQLComparisonOperator.Contains, exactlyOneShipmentType);
					results = Factory.Load<RefCusProcedure>(query);
				}
				return results;
			}

			public RefCusProcedure[] LoadForShipmentTypeAndZzzDataGrouping(ZString exactlyOneShipmentType, ZString dataGroupingCode, ZDateTime date)
			{
				var results = Array.Empty<RefCusProcedure>();
				if (!exactlyOneShipmentType.IsEmpty && !dataGroupingCode.IsEmpty)
				{
					var query = GetFilter(dataGroupingCode, date);
					query.AddToFilter(RefCusProcedureSchema.ZZ6_ShipmentType, SQLComparisonOperator.Contains, exactlyOneShipmentType);
					results = Factory.Load<RefCusProcedure>(query);
				}
				return results;
			}

			public RefCusProcedure[] LoadForShipmentTypeAndZzzDataGroupingAndGroup(ZString exactlyOneShipmentType, ZString dataGroupingCode, ZString groupCode)
			{
				var results = LoadForShipmentTypeAndZzzDataGrouping(exactlyOneShipmentType, dataGroupingCode);
				return (from RefCusProcedure p in results where !p.ZZ6_Group.IsEmpty && p.Groups.Any(g => g.ToUpper() == groupCode) select p).ToArray();
			}

			public RefCusProcedure[] LoadForProcedureCodesAndGrouping(ZString exactlyOneShipmentType, ZString procedureCode, ZString previousProcedureCode, ZString dataGroupingCode)
			{
				var result = Array.Empty<RefCusProcedure>();

				if (!exactlyOneShipmentType.IsEmpty && !procedureCode.IsEmpty && !previousProcedureCode.IsEmpty && !dataGroupingCode.IsEmpty)
				{
					var query = new ZQuery(RefCusProcedureSchema.ZZ6_ZZZ_NKDataGrouping, dataGroupingCode);
					query.AddToFilter(RefCusProcedureSchema.ZZ6_ShipmentType, SQLComparisonOperator.Contains, exactlyOneShipmentType);
					query.AddToFilter(RefCusProcedureSchema.ZZ6_ProcedureCode, procedureCode);
					query.AddToFilter(RefCusProcedureSchema.ZZ6_PreviousProcedureCode, previousProcedureCode);
					result = Factory.Load<RefCusProcedure>(query);
				}

				return result;
			}

			public IEnumerable<ZString> LoadDistinctGroupCodesForDatagrouping(ZString dataGrouping)
			{
				var distinctZz6Group = LoadForZzzDataGrouping(dataGrouping);
				return GetDistinctAndSplitedGroupCodes(distinctZz6Group);
			}

			public IEnumerable<ZString> LoadDistinctGroupCodes(ZString exactlyOneShipmentType, ZString zzzDataGroupingAkaCountry)
			{
				// ZZ6_Group might have values "AAA", "BBB", "AAA,BBB;CCC" - note mixed delimiters - give back {"AAA","BBB","CCC"} 
				var distinctZz6Group = LoadForShipmentTypeAndZzzDataGrouping(exactlyOneShipmentType, zzzDataGroupingAkaCountry);
				return GetDistinctAndSplitedGroupCodes(distinctZz6Group);
			}

			public IEnumerable<ZString> LoadDistinctGroupCodes(ZString exactlyOneShipmentType, ZString zzzDataGroupingAkaCountry, ZDateTime date)
			{
				// ZZ6_Group might have values "AAA", "BBB", "AAA,BBB;CCC" - note mixed delimiters - give back {"AAA","BBB","CCC"} 
				var distinctZz6Group = LoadForShipmentTypeAndZzzDataGrouping(exactlyOneShipmentType, zzzDataGroupingAkaCountry, date);
				return GetDistinctAndSplitedGroupCodes(distinctZz6Group);
			}

			IEnumerable<ZString> GetDistinctAndSplitedGroupCodes(RefCusProcedure[] distinctZz6Group)
			{
				var result = new List<ZString>();
				if (distinctZz6Group != null)
				{
					var distinctCodesWithCommas = (from RefCusProcedure p in distinctZz6Group where !p.ZZ6_Group.IsEmpty select p.ZZ6_Group).Distinct();
					foreach (var x in distinctCodesWithCommas)
					{
						result.AddRange(x.Split(CommaAndSemiColon));
					}
				}
				return result.Where(x => !x.IsEmpty).Distinct().Select(x => x.Trim());
			}

			public static ZQuery GetFilter(ZString dataGroupingCode, ZDateTime date)
			{
				return GetFullFilter(dataGroupingCode, date, ZString.Empty, ZString.Empty);
			}

			public static ZQuery GetFullFilter(ZString dataGroupingCode, ZDateTime date, ZString declarationType, ZString shipmentType)
			{
				var query = new ZQuery(RefCusProcedureSchema.ZZ6_ZZZ_NKDataGrouping, dataGroupingCode);
				if (!date.IsEmpty)
				{
					query.AddToFilter(RefCusProcedureSchema.ZZ6_StartDate, SQLComparisonOperator.LessThanOrEqualTo, date);
					query.AddToFilter(RefCusProcedureSchema.ZZ6_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, date);
				}

				if (!declarationType.IsEmpty)
				{
					if (dataGroupingCode == Core.Constants.CountryCodes.SouthAfrica)
					{
						query.AddToFilter(RefCusProcedureSchema.ZZ6_ProcedureCode, declarationType);
					}
					else
					{
						query.AddToFilter(RefCusProcedureSchema.ZZ6_Group, SQLComparisonOperator.Contains, declarationType);
					}
				}

				if (!shipmentType.IsEmpty)
				{
					query.AddToFilter(RefCusProcedureSchema.ZZ6_ShipmentType, SQLComparisonOperator.Contains, shipmentType);
				}

				return query;
			}
		}

		[ThreadSafe]
		internal static readonly char[] CommaAndSemiColon = new char[] { ';', ',' };

		#region FillWithValidTestData
#if DEBUG

		protected override BusinessObjectTestDataHelper NewBusinessObjectTestDataHelper() => new RefCusProcedureBusinessObjectTestDataHelper();

		class RefCusProcedureBusinessObjectTestDataHelper : BusinessObjectTestDataHelper
		{
			protected override void PopulateUniqueString(ZPropertyInfo property, PropertyDescriptor[] propertyPath, int maxLength)
			{
				base.PopulateUniqueString(property, propertyPath, property.Name == RefCusProcedureSchema.ZZ6_Description.Name ? 50 : maxLength);
			}
		}

#endif
		#endregion

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new TranslatableZZBusinessObjectFetchStrategy<RefCusProcedure>(this);
		}
	}
}
