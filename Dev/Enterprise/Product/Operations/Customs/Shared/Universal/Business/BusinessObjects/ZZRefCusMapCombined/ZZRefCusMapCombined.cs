using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public sealed class ZZRefCusMapCombined : AutoZZRefCusMapCombined, ICodeDescription, IStmALogParent, ITemplateCopyable
	{
		public ZZRefCusMapCombined(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoZZRefCusMapCombined.Schema
		{
			public const string ZZM_ZZP_NKMapTypeDesc = "ZZM_ZZP_NKMapTypeDesc";
		}

		#region Static Methods

		public static ZString MapCW1CodeToCustomsCode(BusinessObjectFactory factory, ZString country, ZString codeType, ZString input, ZDateTime dateForMap)
		{
			var result = ZString.Empty;
			if (factory != null)
			{
				var dictionary = GetCachedDictionary(factory, country, codeType, MapType.CW1ToCustoms, dateForMap);
				dictionary.TryGetValue(input, out result);
			}
			return result;
		}

		public static Dictionary<ZString, ZString> GetCW1CodeToCustomsCodeMapping(BusinessObjectFactory factory, ZString country, ZString codeType, ZDateTime dateForMap)
		{
			return GetCachedDictionary(factory, country, codeType, MapType.CW1ToCustoms, dateForMap);
		}

		public static ZString MapCustomsCodeToCW1Code(BusinessObjectFactory factory, ZString country, ZString codeType, ZString input, ZDateTime dateForMap)
		{
			var result = ZString.Empty;
			if (factory != null)
			{
				var dictionary = GetCachedDictionary(factory, country, codeType, MapType.CustomsToCW1, dateForMap);
				dictionary.TryGetValue(input, out result);
			}
			return result;
		}

		public static Dictionary<ZString, ZString> GetCustomsCodeToCW1CodeMapping(BusinessObjectFactory factory, ZString country, ZString codeType, ZDateTime dateForMap)
		{
			return GetCachedDictionary(factory, country, codeType, MapType.CustomsToCW1, dateForMap);
		}

		static Dictionary<ZString, ZString> GetCachedDictionary(BusinessObjectFactory factory, ZString country, ZString codeType, MapType type, ZDateTime dateForMap)
		{
			Dictionary<ZString, ZString> result = null;
			if (factory == null)
			{
				result = new Dictionary<ZString, ZString>();
			}
			else
			{
				var cachedKey = string.Format(CultureInfo.InvariantCulture, "{0}_{1}_{2}_{3}_RefCusMap_Dictionary", country, codeType, type, dateForMap.ToString("yyMMdd", CultureInfo.InvariantCulture));
				result = factory.GetCachedValue(cachedKey, () =>
				{
					var innerResult = new Dictionary<ZString, ZString>();

					var typeQuery = new ZDBOnlySubQuery(typeof(RefCusMapType), RefCusMapTypeSchema.ZZP_MapType);
					typeQuery.AddToFilter(RefCusMapTypeSchema.ZZP_Direction, MapDirectionList.Codes.BTH);
					typeQuery.AddToFilter(JoinCondition.Or, RefCusMapTypeSchema.ZZP_Direction, type == MapType.CW1ToCustoms ? MapDirectionList.Codes.OUT : MapDirectionList.Codes.INW);

					var query = new ZDBOnlyQuery(typeof(ZZRefCusMapCombined));
					query.AddToFilter(ZZRefCusMapCombinedSchema.ZZM_ZZZ_NKDataGrouping, country);
					query.AddToFilter(ZZRefCusMapCombinedSchema.ZZM_ZZP_NKMapType, codeType);
					if (dateForMap.IsValid)
					{
						query.AddToFilter(ZZRefCusMapCombinedSchema.ZZM_StartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, dateForMap);
						query.AddToFilter(ZZRefCusMapCombinedSchema.ZZM_EndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, dateForMap);
					}
					query.AddSubQuery(ZZRefCusMapCombinedSchema.ZZM_ZZP_NKMapType, RefCusMapTypeSchema.ZZP_MapType, typeQuery, JoinCondition.And);

					foreach (var cusMap in factory.Load<ZZRefCusMapCombined>(query))
					{
						ZString key;
						ZString value;

						switch (type)
						{
							case MapType.CW1ToCustoms:
								key = cusMap.ZZM_CW1orCommercialValue;
								value = cusMap.ZZM_CustomsValue;
								break;
							case MapType.CustomsToCW1:
							default:
								key = cusMap.ZZM_CustomsValue;
								value = cusMap.ZZM_CW1orCommercialValue;
								break;
						}

						if (!innerResult.ContainsKey(key))
						{
							innerResult.Add(key, value);
						}
					}
					return innerResult;
				});
			}
			return result;
		}

		#endregion

		#region Properties

		[List("Lookups.MapTypesList")]
		[RelatedBusinessObject("CusMapType")]
		public override ZString ZZM_ZZP_NKMapType
		{
			get { return base.ZZM_ZZP_NKMapType; }
			set { base.ZZM_ZZP_NKMapType = value; }
		}

		[ReadOnly(true)]
		public override ZBool ZZM_IsSystem
		{
			get { return base.ZZM_IsSystem; }
			set { base.ZZM_IsSystem = value; }
		}

		[List("Lookups.CountryOrGroupingList")]
		public override ZString ZZM_ZZZ_NKDataGrouping
		{
			get { return base.ZZM_ZZZ_NKDataGrouping; }
			set { base.ZZM_ZZZ_NKDataGrouping = value; }
		}

		public override bool SupportsNotes
		{
			get { return false; }
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		public override bool ReadOnly
		{
			get { return ZZM_IsSystem || base.ReadOnly || (CusMapType != null && CusMapType.ZZP_IsReadonly); }
			set { base.ReadOnly = value; }
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ZZM_StartDate = ZDateTime.Today;
			ZZM_EndDate = ZDateTime.MaxSmallDateTimeValue;
		}

		#region New Properties

		[ResourceStringData("Enterprise.Customs.Universal.ZZRefCusMapCombined|ZZM_ZZP_NKMapTypeDesc", Caption = "Mapping Type Description", ShortCaption = "Mapping Desc.")]
		public ZString ZZM_ZZP_NKMapTypeDesc
		{
			get { return Lookups.MapTypesList.GetDescriptionFromCode(ZZM_ZZP_NKMapType); }
		}

		public ZPropertyInfo ZZM_ZZP_NKMapTypeDescInfo
		{
			get { return GetZPropertyInfo(Schema.ZZM_ZZP_NKMapTypeDesc); }
		}

		#endregion

		public enum MapType
		{
			CustomsToCW1,
			CW1ToCustoms
		}

		#endregion

		#region Related BusinessObjects

		public RefCusMapType CusMapType
		{
			get
			{
				if (cusMapType == null || (cusMapType.ZZP_MapType != ZZM_ZZP_NKMapType))
				{
					cusMapType = Factory.LoadFromNaturalKey<RefCusMapType>(RefCusMapTypeSchema.ZZP_MapType, ZZM_ZZP_NKMapType);
				}
				return cusMapType;
			}
		}
		RefCusMapType cusMapType;

		#endregion

		#region ICanDelete Members

		public override bool CanDelete
		{
			get { return !ZZM_IsSystem; }
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get { return ZZM_IsSystem ? CannotDeleteSystemGenerated : (NoResString)string.Empty; }
		}

		internal static MultilingualString CannotDeleteSystemGenerated
		{
			get { return ResString.GetMultilingualString("8B3E7165-430E-4A3A-80BB-C19621723F3A", "You cannot delete a system-generated record."); }
		}

		#endregion

		#region IStmALogParent Members

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		string IStmALogParent.LogsParentTableName
		{
			get { return ZZRefCusMapCombined.Schema.TableName; }
		}

		#endregion

		#region ITemplateCopyable Members

		public IBusiness TemplateCopy()
		{
			var result = (ZZRefCusMapCombined)Clone();
			result.ZZM_IsSystem = ZBool.False;
			return result;
		}

		#endregion

#if DEBUG

		protected override BusinessObjectTestDataHelper NewBusinessObjectTestDataHelper()
		{
			return new UniversalReferenceBOTestDataHelper();
		}

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			ZZM_ZZP_NKMapType = RefCusMapTypeList.Codes.ZADOC;
			ZZM_CW1orCommercialValue = "MCD";
			ZZM_CustomsValue = "OTH";
			ZZM_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.SouthAfrica;
			ZZM_StartDate = ZDateTime.MinSmallDateTimeValue;
			ZZM_EndDate = ZDateTime.MaxSmallDateTimeValue;
		}

#endif
	}
}
