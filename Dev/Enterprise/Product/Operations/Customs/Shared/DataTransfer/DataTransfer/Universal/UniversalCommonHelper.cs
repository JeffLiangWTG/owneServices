using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal.AddInfoExtensions;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public abstract class UniversalCommonHelper
	{
		protected UniversalCommonHelper(BusinessObjectFactory factory)
		{
			this.factory = Argument.NotNull(factory, "BusinessObjectFactory factory");
		}
		protected readonly BusinessObjectFactory factory;

		public BusinessObject[] Load(Type bizObjType, ZQuery query)
		{
			return factory.Load(bizObjType, query);
		}

		public T Load<T>(IColumnIndexer row, SchemaGuidColumn column)
			where T : BusinessObject
		{
			return factory.Load<T>(row.GetValue(column));
		}

		public T LoadFromNaturalKey<T>(IColumnIndexer row, SchemaStringColumn rowColumn, SchemaColumn naturalKeyColumn)
			where T : BusinessObject
		{
			return factory.LoadFromNaturalKey<T>(naturalKeyColumn, row.GetValue(rowColumn));
		}

		public T LoadFromNaturalKey<T>(SchemaStringColumn column, ZString naturalKeyValue)
			where T : BusinessObject
		{
			return factory.LoadFromNaturalKey<T>(column, naturalKeyValue);
		}

		public T LoadTop1<T>(ZQuery query)
			where T : class
		{
			return factory.LoadTop1<T>(query);
		}

		public T Load<T>(ZGuid pk)
			where T : class
		{
			return factory.Load<T>(pk);
		}

		public T[] Load<T>(ZQuery query)
			where T : class
		{
			return factory.Load<T>(query);
		}

		protected ICustomLabelsProvider GetJobComInvoiceLineCustomLabelsProviderFromProvider(ICustomsCustomLabelsConfigOrgProvider provider)
		{
			var pk = provider == null ? ZGuid.Empty : provider.PK;
			var key = "JobComInvoiceLineCustomLabelsProvider" + pk.ToStringKey();
			return factory.GetCachedValue(key, () =>
			{
				ICustomLabelsProvider result = null;
				if (provider != null)
				{
					result = new BaseJobComInvoiceLine.CustomLabelsProvider(provider);
				}
				return result;
			});
		}

		protected ICustomLabelsProvider GetCusContainerCustomLabelsProviderFromDeclaration(BaseJobDeclaration declaration)
		{
			var pk = declaration == null ? ZGuid.Empty : declaration.PK;
			var key = "CusContainerCustomLabelsProvider" + pk.ToStringKey();
			return factory.GetCachedValue(key, () =>
			{
				ICustomLabelsProvider result = null;
				if (declaration != null)
				{
					result = new BaseCusContainer.CustomLabelsProvider(declaration);
				}
				return result;
			});
		}

		protected virtual IUniversalCustomsDataObjectProvider GetUniversalCustomsDataObjectProvider(ZString countryCode)
		{
			return factory.GetUniversalCustomsDataObjectProvider(countryCode);
		}

		public static string GetCusSupportingInfoCSI_TypeListCacheKey(ZString countryCode, ZString tablePrefix, string dataContext) => string.Format(CultureInfo.InvariantCulture, "{0}-{1}-{2}-CusSupportingInfoCSI_TypeList", countryCode, tablePrefix, dataContext);
		protected ICodeDescriptionPairList GetCusSupportingInfoCSI_TypeList(ZString countryCode, ZString tablePrefix, string dataContext)
		{
			return GetListFor(GetCusSupportingInfoCSI_TypeListCacheKey(countryCode, tablePrefix, dataContext), GetUniversalCustomsDataObjectProvider(countryCode), (x) => x.TableSpecificCusSupportingInfoTypeList(tablePrefix, dataContext));
		}

		protected ICodeDescriptionPairList GetCusAddInfoB7_TypeList(ZString countryCode, ZString tablePrefix, string dataContext)
		{
			return GetListFor(string.Format(CultureInfo.InvariantCulture, "{0}-{1}-{2}-CusAddInfoB7_TypeList", countryCode, tablePrefix, dataContext), GetUniversalCustomsDataObjectProvider(countryCode), (x) => x.TableSpecificCusAddInfoTypeList(tablePrefix, dataContext));
		}

		protected ICodeDescriptionPairList GetAddInfoGroupTypesNeedInsertedToOtherTableList(ZString countryCode, ZString tablePrefix, string dataContext)
		{
			return GetListFor(string.Format(CultureInfo.InvariantCulture, "{0}-{1}-{2}-AddInfoGroupTypesNeedInsertedToOtherTableList", countryCode, tablePrefix, dataContext), GetUniversalCustomsDataObjectProvider(countryCode), (x) => x.TableSpecificAddInfoGroupTypesNeedInsertedToOtherTableList(tablePrefix, dataContext));
		}

		protected ICodeDescriptionPairList GetCusCodeDataCY_TypeList(ZString countryCode, ZString tablePrefix, string dataContext)
		{
			return GetListFor(string.Format(CultureInfo.InvariantCulture, "{0}-{1}-{2}-CusCodeDataCY_TypeList", countryCode, tablePrefix, dataContext), GetUniversalCustomsDataObjectProvider(countryCode), (x) => x.TableSpecificCusCodeDataTypeList(tablePrefix, dataContext));
		}

		protected ICodeDescriptionPairList GetCusReferenceCFR_TypeList(ZString countryCode, ZString tablePrefix, string dataContext)
		{
			return GetListFor(string.Format(CultureInfo.InvariantCulture, "{0}-{1}-{2}-CusReferenceCFR_TypeList", countryCode, tablePrefix, dataContext), GetUniversalCustomsDataObjectProvider(countryCode), (x) => x.TableSpecificCusReferenceTypeList(tablePrefix, dataContext));
		}

		protected ICodeDescriptionPairList GetCusCodeDataCY_CodeList(ZString countryCode, ZString tablePrefix, string dataContext)
		{
			return GetListFor(string.Format(CultureInfo.InvariantCulture, "{0}-{1}-{2}-CusCodeDataCY_CodeList", countryCode, tablePrefix, dataContext), GetUniversalCustomsDataObjectProvider(countryCode), (x) => x.TableSpecificCusCodeDataCodeList(tablePrefix, dataContext));
		}

		protected ICodeDescriptionPairList GetListFor(string key, IUniversalCustomsDataObjectProvider universalCustomsDataObjectProvider, Func<IUniversalCustomsDataObjectProvider, ICodeDescriptionPairList> getList)
		{
			return factory.GetCachedValue(key, () =>
			{
				var provider = universalCustomsDataObjectProvider;
				return provider != null ? getList(provider) : null;
			});
		}

		public static string GetSupportedCusSupportingInfoCSI_TypesForCacheKey(ZString countryCode, ZString tablePrefix, string dataContext) => string.Format(CultureInfo.InvariantCulture, "{0}-{1}-{2}-SupportedCusSupportingInfoCSI_Types", countryCode, tablePrefix, dataContext);
		protected ZString[] GetSupportedCusSupportingInfoCSI_TypesFor(ZString countryCode, ZString tablePrefix, string dataContext)
		{
			return GetSupportedTypes(GetSupportedCusSupportingInfoCSI_TypesForCacheKey(countryCode, tablePrefix, dataContext), () => GetCusSupportingInfoCSI_TypeList(countryCode, tablePrefix, dataContext));
		}

		protected ZString[] GetSupportedCusAddInfoB7_TypesFor(ZString countryCode, ZString tablePrefix, string dataContext)
		{
			return GetSupportedTypes(string.Format(CultureInfo.InvariantCulture, "{0}-{1}-{2}-SupportedCusAddInfoB7_Types", countryCode, tablePrefix, dataContext), () => GetCusAddInfoB7_TypeList(countryCode, tablePrefix, dataContext));
		}

		protected ZString[] GetAddInfoGroupTypesNeedInsertedToOtherTableFor(ZString countryCode, ZString tablePrefix, string dataContext)
		{
			return GetSupportedTypes(string.Format(CultureInfo.InvariantCulture, "{0}-{1}-{2}-AddInfoGroupTypesNeedInsertedToOtherTable", countryCode, tablePrefix, dataContext), () => GetAddInfoGroupTypesNeedInsertedToOtherTableList(countryCode, tablePrefix, dataContext));
		}

		protected ZString[] GetSupportedCusCodeDataCY_TypesFor(ZString countryCode, ZString tablePrefix, string dataContext)
		{
			return GetSupportedTypes(string.Format(CultureInfo.InvariantCulture, "{0}-{1}-{2}-SupportedCusCodeDataCY_Types", countryCode, tablePrefix, dataContext), () => GetCusCodeDataCY_TypeList(countryCode, tablePrefix, dataContext));
		}

		protected ZString[] GetSupportedCusReferenceCFR_TypesFor(ZString countryCode, ZString tablePrefix, string dataContext)
		{
			return GetSupportedTypes(string.Format(CultureInfo.InvariantCulture, "{0}-{1}-{2}-SupportedCusReferenceCFR_Types", countryCode, tablePrefix, dataContext), () => GetCusReferenceCFR_TypeList(countryCode, tablePrefix, dataContext));
		}

		protected ZString[] GetSupportedTypes(string key, Func<ICodeDescriptionPairList> getList)
		{
			return factory.GetCachedValue(key, () =>
			{
				ZString[] result = null;
				var list = getList();
				if (list != null)
				{
					var supportedTypes = new List<ZString>();
					foreach (ICodeDescription pair in list)
					{
						supportedTypes.Add(pair.Code);
					}
					result = supportedTypes.ToArray();
				}
				return result;
			});
		}

		public static bool IsOrganizationAddressTypeMatched(OrganizationAddress x, OrganizationAddress y)
		{
			return x != null && y != null && x.AddressType.GetValueOrDefault() == y.AddressType.GetValueOrDefault();
		}

		public static bool IsBillMatched(AdditionalBill bill1, AdditionalBill bill2)
		{
			return
				bill1.BillType != null && bill2.BillType != null &&
				bill1.BillType.GetCodeAsUpperCase() == bill2.BillType.GetCodeAsUpperCase() &&
				bill1.BillNumber.HasValue && bill2.BillNumber.HasValue &&
				bill1.BillNumber.GetValueOrDefault() == bill2.BillNumber.GetValueOrDefault() &&
				bill1.ParentBillNumber.HasValue && bill2.ParentBillNumber.HasValue &&
				bill1.ParentBillNumber.GetValueOrDefault() == bill2.ParentBillNumber.GetValueOrDefault();
		}

		#region Add Info Methods

		public T Load<T, B>(SchemaGuidColumn addInfoGuidColumn, Dictionary<ZString, ZString> addInfos)
			where T : BusinessObject
			where B : BusinessObject
		{
			T result = null;
			var value = GetValue<B>(addInfoGuidColumn, addInfos);
			if (value != null && value is ZGuid)
			{
				result = factory.Load<T>((ZGuid)value);
			}
			return result;
		}

		public ZString? GetValue<B>(SchemaStringColumn addInfoColumn, Dictionary<ZString, ZString> addInfos)
			where B : BusinessObject
		{
			ZString? result = null;
			var value = GetValueCore<B>(addInfoColumn, addInfos);
			if (value != null)
			{
				result = (ZString)value;
			}
			return result;
		}

		public ZBool? GetValue<B>(SchemaBoolColumn addInfoColumn, Dictionary<ZString, ZString> addInfos)
			where B : BusinessObject
		{
			ZBool? result = null;
			var value = GetValueCore<B>(addInfoColumn, addInfos);
			if (value != null)
			{
				result = (ZBool)value;
			}
			return result;
		}

		public ZDateTime? GetValue<B>(SchemaDateTimeColumn addInfoColumn, Dictionary<ZString, ZString> addInfos)
			where B : BusinessObject
		{
			ZDateTime? result = null;
			var value = GetValueCore<B>(addInfoColumn, addInfos);
			if (value != null)
			{
				result = (ZDateTime)value;
			}
			return result;
		}

		public ZDecimal? GetValue<B>(SchemaDecimalColumn addInfoColumn, Dictionary<ZString, ZString> addInfos)
			where B : BusinessObject
		{
			ZDecimal? result = null;
			var value = GetValueCore<B>(addInfoColumn, addInfos);
			if (value != null)
			{
				result = (ZDecimal)value;
			}
			return result;
		}

		public ZInt? GetValue<B>(SchemaIntColumn addInfoColumn, Dictionary<ZString, ZString> addInfos)
			where B : BusinessObject
		{
			ZInt? result = null;
			var value = GetValueCore<B>(addInfoColumn, addInfos);
			if (value != null)
			{
				result = (ZInt)value;
			}
			return result;
		}

		public ZShort? GetValue<B>(SchemaShortColumn addInfoColumn, Dictionary<ZString, ZString> addInfos)
			where B : BusinessObject
		{
			ZShort? result = null;
			var value = GetValueCore<B>(addInfoColumn, addInfos);
			if (value != null)
			{
				result = (ZShort)value;
			}
			return result;
		}

		public ZGuid? GetValue<B>(SchemaGuidColumn addInfoColumn, Dictionary<ZString, ZString> addInfos)
			where B : BusinessObject
		{
			ZGuid? result = null;
			var value = GetValueCore<B>(addInfoColumn, addInfos);
			if (value != null)
			{
				result = (ZGuid)value;
			}
			return result;
		}

		IZType GetValueCore<B>(SchemaColumn addInfoColumn, Dictionary<ZString, ZString> addInfos)
			where B : BusinessObject
		{
			IZType result = null;
			var name = addInfoColumn.Name.Substring(3);
			ZString value;
			if (addInfos.TryGetValue(name, out value))
			{
				var supportedAddInfos = factory.GetAddInfoSchemaDictionary<B>(addInfoColumn.TableSchema);
				SchemaColumn dataType;
				if (supportedAddInfos.TryGetValue(name, out dataType))
				{
					result = BaseAddInfo.ConvertToZType(dataType.GetEquivalentZType(), value);
				}
			}
			return result;
		}

		public void Update(List<AddInfo> addInfoCollection, ZString key, IZType value)
		{
			if (value.IsEmpty)
			{
				foreach (var addInfoToRemove in addInfoCollection.Where(x => x.Key.GetValueOrDefault() == key).ToArray())
				{
					addInfoCollection.Remove(addInfoToRemove);
				}
			}
			else
			{
				var addInfo = addInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == key);
				var stringValue = BaseAddInfo.GetStringRepresentation(value);
				if (addInfo == null)
				{
					addInfoCollection.Add(AddInfo.New(key, stringValue));
				}
				else
				{
					addInfo.Value = stringValue;
				}
			}
		}
		#endregion
	}
}
