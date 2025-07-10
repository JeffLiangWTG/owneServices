using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	// Tested functionally through relevant Customs country subclass
	public abstract class OrgSupplierPartDataLoad : MasterFiles.Business.OrgSupplierPartDataLoad
	{
		protected OrgSupplierPartDataLoad()
		{
		}

		protected IEnumerable<string> FieldNamesMatchingColumn => fieldNamesMatchingColumn ?? (fieldNamesMatchingColumn = FieldNamesCore.Where(f => HasColumn(f)).ToArray());
		IEnumerable<string> fieldNamesMatchingColumn;

		IEnumerable<string> FieldNamesCore => fieldNames ?? (fieldNames = GetFieldNames().ToArray());
		IEnumerable<string> fieldNames;

		protected abstract IEnumerable<string> GetFieldNames();

		protected override sealed void PopulatePartsDataToLoad(OCsvLine line, PartsDataToLoad partsDataToLoad)
		{
			foreach (var fieldName in FieldNamesMatchingColumn)
			{
				PopulatePartsDataForFieldAndProperty(line, partsDataToLoad, fieldName);
			}
			base.PopulatePartsDataToLoad(line, partsDataToLoad);
		}

		protected virtual (FieldInfo, PropertyInfo) PopulatePartsDataForFieldAndProperty(OCsvLine line, PartsDataToLoad partsDataToLoad, string fieldName)
		{
			PropertyInfo propertyInfo = null;
			var fieldInfo = partsDataToLoad.GetType().GetField(fieldName);
			if (fieldInfo != null)
			{
				var value = GetValue(line, fieldName, fieldInfo.FieldType);
				fieldInfo.SetValue(partsDataToLoad, value);
			}
			else
			{
				propertyInfo = partsDataToLoad.GetType().GetProperty(fieldName);
				if (propertyInfo != null)
				{
					var value = GetValue(line, fieldName, propertyInfo.PropertyType);
					propertyInfo.SetValue(partsDataToLoad, value);
				}
			}
			return (fieldInfo, propertyInfo);
		}

		protected override sealed IEnumerable<string> CSVTemplateHeaders => base.CSVTemplateHeaders.Union(FieldNamesCore).AsEnumerable();

		protected override void LoadCountrySpecificDataForTariffNum(MasterFiles.Business.OrgSupplierPart enterprisePart, ZString tariffNum, ZString importOrExport, PartsDataToLoad dataToLoad)
		{
			var pivot = AddCusClassPartPivotIfRequired(enterprisePart, importOrExport);
			SetValue(pivot.CI_TariffNumInfo, tariffNum.KeepNumericCharacters());
			pivot.CI_CC = ZGuid.Empty;
			pivot.CI_UsageComment = dataToLoad.UsageComment;
			pivot.CI_Description = dataToLoad.ClassificationDescription;
		}

		BaseCusClassPartPivot AddCusClassPartPivotIfRequired(MasterFiles.Business.OrgSupplierPart enterprisePart, string importOrExport)
		{
			var part = (OrgSupplierPart)enterprisePart;
			var pivotType = importOrExport == "IMP" ? "HTI" : "HTE";
			BaseCusClassPartPivot cusClassPartPivot = GetPivots(part, pivotType).FirstOrDefault();
			if (cusClassPartPivot == null)
			{
				cusClassPartPivot = part.PivotsForBinding.AddNew();
				cusClassPartPivot.CI_ChildType = pivotType;
			}
			SetupPivotSetterSuspenderIfNeeded(cusClassPartPivot);
			return cusClassPartPivot;
		}

		protected void SetupPivotSetterSuspenderIfNeeded(BaseCusClassPartPivot pivot)
		{
			var properties = PivotPropertiesToSuspendSetting;
			if (properties.Length > 0)
			{
				AddToDisposableList(pivot.SetterSuspender.SuspendSetting(properties));
			}
		}

		ZString[] PivotPropertiesToSuspendSetting => pivotPropertiesToSuspendSetting ?? (pivotPropertiesToSuspendSetting = GetPivotPropertiesToSuspendSetting().ToArray());
		ZString[] pivotPropertiesToSuspendSetting;

		protected virtual IEnumerable<ZString> GetPivotPropertiesToSuspendSetting()
		{
			return Enumerable.Empty<ZString>();
		}

		IEnumerable<BaseCusClassPartPivot> GetPivots(OrgSupplierPart enterprisePart, string pivotType)
		{
			return enterprisePart.PivotsForBinding.GetNonDeletedPivots().Where(x => x.CI_ChildType == pivotType);
		}
	}
}
