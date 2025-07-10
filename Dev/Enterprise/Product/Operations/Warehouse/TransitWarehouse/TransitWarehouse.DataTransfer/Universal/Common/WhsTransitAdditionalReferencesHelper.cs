using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public class WhsTransitAdditionalReferencesHelper : DataObjectReader
	{
		public WhsTransitAdditionalReferencesHelper(IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(logger)
		{
			Factory = Argument.NotNull(factory, nameof(factory));
		}
		readonly UniversalObjectFactory Factory;

		public static IEnumerable<ReferenceTypeMappingInfo> GetTransitReferenceMappingInfo(Guid companyPK, Guid warehouseBranchPK)
		{
			var mappings = WarehouseDataRegistry.Instance.TransitReferenceMapping.GetFallBackValueAtAllLevels(companyPK, warehouseBranchPK, Guid.Empty).TransitReferenceMappingCollection.Cast<TransitReferenceMapping>();
			return mappings.Select(m => new ReferenceTypeMappingInfo
			{
				FromReferenceCategory = m.SourceCategory,
				FromReferenceType = m.SourceType,
				ToReferenceCategory = m.TargetCategory,
				ToReferenceType = m.TargetType,
				Direction = m.Direction
			});
		}

		public void UpdateOrCreateAddOnValue(BusinessObject bo, string name, string type, ZString? value)
		{
			var addOnValue = Factory.Load<GenCustomAddOnValue>(new ZQuery(GenCustomAddOnValueSchema.XV_ParentID, bo.PK)).Where(g => g.XV_Name == name).FirstOrDefault();
			if (addOnValue != null && value.HasValue)
			{
				addOnValue.XV_Data = value.Value;
			}
			else
			{
				PopulateAddOnValue(bo.PK, bo.TablePrefix, name, type, value);
			}
		}

		public void PopulateAddOnValue(ZGuid parentPK, string tablePrefix, string name, string type, ZString? value)
		{
			if (!value.GetValueOrDefault().IsEmpty)
			{
				var row = Factory.RowFactory.NewRowWithPK(GenCustomAddOnValueSchema.Instance);
				SetValue(row, GenCustomAddOnValueSchema.XV_ParentID, parentPK);
				SetValue(row, GenCustomAddOnValueSchema.XV_ParentTableCode, tablePrefix);
				SetValue(row, GenCustomAddOnValueSchema.XV_Type, type);
				SetValue(row, GenCustomAddOnValueSchema.XV_Name, name);
				SetValue(row, GenCustomAddOnValueSchema.XV_Data, value);
			}
		}

		public void CollectTransitAdditionalReferenceInfo(List<TransitAdditionalReferenceInfo> additionalReferences, ZString? type, ZString? value, string sourceType = "", string category = CusEntryNumber.Categories.AdditionalReferenceNumber, string countryCode = "")
		{
			if (!value.GetValueOrDefault().IsEmpty)
			{
				TransitAdditionalReferenceInfo transitAdditionalReferenceInfo = new TransitAdditionalReferenceInfo
				{
					Type = type,
					Value = value,
					Category = category,
					CountryCode = countryCode,
					SourceType = sourceType
				};

				additionalReferences.Add(transitAdditionalReferenceInfo);
			}
		}

		public void CollectTransitAdditionalReferenceInfoIfNumberNotExist(List<TransitAdditionalReferenceInfo> additionalReferences, ZGuid parentPK, ZString type, ZString? value, string sourceType = "", string category = CusEntryNumber.Categories.AdditionalReferenceNumber, string countryCode = "")
		{
			if (!value.GetValueOrDefault().IsEmpty)
			{
				var rows = Factory.RowFactory.Load(CusEntryNumSchema.Constants.TableName, new ZQuery(CusEntryNumSchema.CE_ParentID, parentPK));
				var existingReferences = rows.Select(r => DataObjectReader.GetColumnIndexerFromRow(r));
				if (!existingReferences.Any(r => value.Equals(r[CusEntryNumSchema.Constants.CE_EntryNum])
					&& r[CusEntryNumSchema.Constants.CE_Category].Equals(category)
					&& r[CusEntryNumSchema.Constants.CE_EntryType].Equals(type.ToString())
					&& r[CusEntryNumSchema.Constants.CE_RN_NKCountryCode].Equals(countryCode))
					&& !additionalReferences.Any(r => type.Equals(r.Type?.ToString())
						&& value.Equals(r.Value?.ToString())
						&& category.Equals(r.Category?.ToString())
						&& countryCode.Equals(r.CountryCode?.ToString())))
				{
					TransitAdditionalReferenceInfo transitAdditionalReferenceInfo = new TransitAdditionalReferenceInfo
					{
						Type = type,
						Value = value,
						Category = category,
						CountryCode = countryCode,
						SourceType = sourceType
					};
					additionalReferences.Add(transitAdditionalReferenceInfo);
				}
			}
		}
	}
}
