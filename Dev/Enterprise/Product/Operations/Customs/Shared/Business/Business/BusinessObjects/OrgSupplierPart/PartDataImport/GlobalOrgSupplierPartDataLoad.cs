using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class GlobalPartsDataToLoad : PartsDataToLoad
	{
		public ZDateTime StartDate;
		public ZDateTime EndDate;
		public ZString ExportCountry;
		public ZString TaxType;
		public ZString OriginState;
		public ZString PrimaryPreference;
		public ZString SecondaryPreference;
		public ZString ValuationCode;
		public ZDecimal ValuationMarkup;

		Dictionary<string, AddInfoValueObject> AddInfos => addInfos ?? (addInfos = new Dictionary<string, AddInfoValueObject>());
		Dictionary<string, AddInfoValueObject> addInfos;

		public bool HasAddInfo(string fieldName)
		{
			return AddInfos.ContainsKey(fieldName);
		}

		public void AddAddInfo(string fieldName, ZPropertyInfo propertyInfo)
		{
			AddInfos.Add(fieldName, new AddInfoValueObject()
			{
				Key = propertyInfo.Name,
				Type = propertyInfo.PropertyType
			});
		}

		public AddInfoValueObject GetAddInfoValueObject(string fieldName)
		{
			return AddInfos.GetValueSafe(fieldName);
		}

		public IEnumerable<AddInfoValueObject> GetAddInfoValues()
		{
			return AddInfos.Values;
		}

		public class AddInfoValueObject
		{
			public string Key;
			public Type Type;
			public object Value;
		}
	}

	public class GlobalOrgSupplierPartDataLoad : OrgSupplierPartDataLoad, Integration.Customs.IGlobalOrgSupplierPartDataLoad
	{
		protected override IEnumerable<string> GetFieldNames()
		{
			var fieldNames = new List<string>();

			fieldNames.Add("StartDate");
			fieldNames.Add("EndDate");
			fieldNames.Add("ExportCountry");
			fieldNames.Add("TaxType");
			fieldNames.Add("OriginState");
			fieldNames.Add("PrimaryPreference");
			fieldNames.Add("SecondaryPreference");
			fieldNames.Add("ValuationCode");
			fieldNames.Add("ValuationMarkup");

			fieldNames.AddRange(GetAddInfoFieldNames().Where(x => !fieldNames.Contains(x)));

			return fieldNames;
		}

		IEnumerable<string> GetAddInfoFieldNames()
		{
			var fieldNames = new List<string>();

			var pivot = new BusinessObjectFactory().New<BaseCusClassPartPivot>();
			if (pivot is IAddInfoManager addInfoManager)
			{
				var properties = Enumerable.Empty<ZPropertyInfo>();
				if (pivot is IAddInfoSchemaProvider addInfoSchemaProvider)
				{
					properties = addInfoSchemaProvider.AddInfoTableSchema.All
						.Where(addInfoProperty => !addInfoProperty.IsPKColumn)
						.Select(addInfoProperty => pivot.ZPropertyInfoHash[addInfoProperty.Name])
						.Where(x => x != null);
				}
				else if (addInfoManager.AddInfo is BaseAddInfo addInfo)
				{
					properties = pivot.ZPropertyInfoHash.GetPropertyInfos(PropertyInfoTypes.Wrapping).
						OfType<ZWrappedPropertyInfo>().Select(x => addInfo.ZPropertyInfoHash.GetPropertySafe(x.Name)).Where(x => x != null);
				}

				foreach (var property in properties)
				{
					if (!AddInfoExclusionList.Contains(property.Name))
					{
						var name = addInfoManager.AddInfo.GetKey(property.Name);
						var data = (GlobalPartsDataToLoad)GetPartsDataToLoad();
						data.AddAddInfo(name, property);
						fieldNames.Add(name);
					}
				}
			}

			return fieldNames;
		}

		protected virtual ICollection<string> AddInfoExclusionList => addInfoExclusionList ?? (addInfoExclusionList = new List<string>());
		ICollection<string> addInfoExclusionList;

		protected sealed override (FieldInfo, PropertyInfo) PopulatePartsDataForFieldAndProperty(OCsvLine line, PartsDataToLoad partsData, string fieldName)
		{
			var (fieldInfo, propertyInfo) = base.PopulatePartsDataForFieldAndProperty(line, partsData, fieldName);
			if (fieldInfo == null && propertyInfo == null)
			{
				var data = (GlobalPartsDataToLoad)partsData;
				if (data.HasAddInfo(fieldName))
				{
					var addInfoValueObject = data.GetAddInfoValueObject(fieldName);
					if (addInfoValueObject != null)
					{
						var value = GetValue(line, fieldName, addInfoValueObject.Type);
						addInfoValueObject.Value = value;
					}
				}
			}
			return (fieldInfo, propertyInfo);
		}

		GlobalPartsDataToLoad PartsDataToLoad => partsDataToLoad ?? (partsDataToLoad = new GlobalPartsDataToLoad());
		GlobalPartsDataToLoad partsDataToLoad;

		protected override PartsDataToLoad GetPartsDataToLoad()
		{
			return PartsDataToLoad;
		}

		#region Create/Update Pivot

		protected override void SetTariffAndClassificationDetails(MasterFiles.Business.OrgSupplierPart enterprisePart, PartsDataToLoad dataToLoad)
		{
			if (UseOldClassificationFields)
			{
				base.SetTariffAndClassificationDetails(enterprisePart, dataToLoad);
			}
			else
			{
				if (!dataToLoad.Tariff.IsEmpty) // Tariff must be done first - classifications are being deprecated / Pivot has both tariff number and classification lookup, but lookup will be removed if a tariff number is loaded.
				{
					LoadCountrySpecificDataForTariffNum(enterprisePart, dataToLoad.Tariff, GetClassificationType(dataToLoad), dataToLoad);
				}
				else if (!dataToLoad.ClassificationLookup.IsEmpty)
				{
					LoadCountrySpecificDataForLookup(enterprisePart, dataToLoad.ClassificationLookup, GetClassificationType(dataToLoad), dataToLoad);
				}

				if (!dataToLoad.PartFullDesc.IsEmpty)
				{
					AddDescriptionNote(enterprisePart, dataToLoad.PartFullDesc, (NoResString)"Full Product Description");
				}
			}
		}

		protected override void LoadCountrySpecificDataForLookup(MasterFiles.Business.OrgSupplierPart enterprisePart, ZString lookupCode, ZString classificationType, PartsDataToLoad dataToLoad)
		{
			var classification = GetClassification(lookupCode, classificationType);
			if (classification != null)
			{
				var pivot = AddCusClassPartPivot(enterprisePart, classificationType, IsScheduleB(classification.CC_TariffNum), dataToLoad);
				pivot.CI_CC = classification.PK;
				SetPivotSpecificFields(pivot, dataToLoad);
			}
		}

		protected override void LoadCountrySpecificDataForTariffNum(MasterFiles.Business.OrgSupplierPart enterprisePart, ZString tariffNum, ZString classificationType, PartsDataToLoad dataToLoad)
		{
			var pivot = AddCusClassPartPivot(enterprisePart, classificationType, IsScheduleB(tariffNum), dataToLoad);
			SetValue(pivot.CI_TariffNumInfo, tariffNum);
			pivot.CI_CC = ZGuid.Empty;
			SetPivotSpecificFields(pivot, dataToLoad);
		}

		protected virtual void SetPivotSpecificFields(BaseCusClassPartPivot pivot, PartsDataToLoad dataToLoad)
		{ }

		protected override bool UseOldClassificationFields => false;

		protected virtual bool UseScheduleB => false;

		protected virtual bool IsScheduleB(ZString tariff) => false;

		protected virtual string ScheduleBClassificationType
		{
			get { throw new NotSupportedException("If ScheduleB is used by the overriding country, then this property should be overridden."); }
		}

		protected BaseCusClassPartPivot AddCusClassPartPivot(MasterFiles.Business.OrgSupplierPart enterprisePart, string classificationType, bool isScheduleB, PartsDataToLoad partsData)
		{
			// the parameter classificationType and isScheduleB are currently only used in CA and US, should be refactored and extracted to the corresponding solution
			var part = (OrgSupplierPart)enterprisePart;
			BaseCusClassPartPivot cusClassPartPivot = null;
			var tariffNum = ZString.Empty;
			if (classificationType == BaseCusClassification.ClassificationType.Both)
			{
				tariffNum = partsData.Tariff;
			}
			else
			{
				tariffNum = classificationType == BaseCusClassification.ClassificationType.IMP ? partsData.ImportTariff : partsData.ExportTariff;
			}

			if (UseOldClassificationFields)
			{
				// this logic is currently only used in CA and US, should be refactored and extracted to the corresponding solution
				cusClassPartPivot = GetPivotUseOldClassificationFields(part, classificationType, isScheduleB);
				if (cusClassPartPivot == null)
				{
					cusClassPartPivot = part.PivotsForBinding.AddNew();
					cusClassPartPivot.CI_ChildType = classificationType == BaseCusClassification.ClassificationType.IMP ? ClassificationTypeList.Codes.HTI
						: isScheduleB ? ScheduleBClassificationType
						: ClassificationTypeList.Codes.HTE;
					SetupPivotSetterSuspenderIfNeeded(cusClassPartPivot);
					AddDataToPivotUseOldClassificationFields(cusClassPartPivot, partsData, classificationType);
				}
				else
				{
					var originalAddInfo = cusClassPartPivot.CI_AddInfo;
					SetupPivotSetterSuspenderIfNeeded(cusClassPartPivot);
					AddDataToPivotUseOldClassificationFields(cusClassPartPivot, partsData, classificationType);
					SaveAddInfo(cusClassPartPivot, originalAddInfo);
				}
			}
			else
			{
				var originalAddInfo = ZString.Empty;
				var dataToLoad = (GlobalPartsDataToLoad)partsData;
				var pivotType = dataToLoad.ClassificationType;

				var existingPivot = FindExistingPivot(part, pivotType);
				if (existingPivot != null)
				{
					originalAddInfo = existingPivot.CI_AddInfo;

					if (existingPivot.CI_TariffNum == tariffNum)
					{
						cusClassPartPivot = existingPivot;
					}
					else
					{
						existingPivot.Delete();
					}
				}

				if (cusClassPartPivot == null)
				{
					cusClassPartPivot = CreatePivot(part, pivotType);
				}

				SetupPivotSetterSuspenderIfNeeded(cusClassPartPivot);
				AddDataToPivot(cusClassPartPivot, partsData);
				SaveAddInfo(cusClassPartPivot, originalAddInfo);
			}

			return cusClassPartPivot;
		}

		protected virtual BaseCusClassPartPivot FindExistingPivot(OrgSupplierPart part, ZString pivotType)
		{
			return GetPivots(part, pivotType).FirstOrDefault();
		}

		protected virtual BaseCusClassPartPivot CreatePivot(OrgSupplierPart part, ZString pivotType)
		{
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = pivotType;
			return pivot;
		}

		void SaveAddInfo(BaseCusClassPartPivot cusClassPartPivot, ZString originalAddInfo)
		{
			var newAddInfo = ZString.Empty;
			if (!originalAddInfo.IsEmpty)
			{
				var addInfoManager = cusClassPartPivot as IAddInfoManager;
				if (addInfoManager != null)
				{
					var addInfo = addInfoManager.AddInfo as BaseAddInfo;
					if (addInfo != null)
					{
						newAddInfo = addInfo.ToString();
					}
				}

				if (originalAddInfo != newAddInfo)
				{
					string previousAddInfoValue = Res.GetString("39C4870B-0760-4DBA-AB30-3DBDB9EAA9B5", "Previous Add Info: {0}", originalAddInfo);
					cusClassPartPivot.Part.Notes.AddNew(true, Res.GetString("85702E4D-D3E3-4183-8370-6FBD777F647B", "Data Import Update: Previous {0} Add Info Data", cusClassPartPivot.CI_ChildType), previousAddInfoValue);
				}
			}
		}

		protected virtual void AddDataToPivotUseOldClassificationFields(BaseCusClassPartPivot pivot, PartsDataToLoad partsData, string classificationType)
		{
			AddDataToPivot(pivot, partsData);
		}

		protected virtual void AddDataToPivot(BaseCusClassPartPivot pivot, PartsDataToLoad partsData)
		{
			var data = (GlobalPartsDataToLoad)partsData;
			var setterSuspender = pivot.SetterSuspender;
			SetValue((ZPropertyInfoDateTime)pivot.CI_DateStartInfo, data.StartDate);
			SetValue((ZPropertyInfoDateTime)pivot.CI_DateEndInfo, data.EndDate);
			SetValue(pivot.CI_RN_NKCountryOfOriginInfo, data.PartOrigin, propertyIdentifier: FieldNames.Origin, setterSuspender: setterSuspender, resumeSuspender: true);
			SetValue(pivot.CI_RN_NKCountryOfExportInfo, data.ExportCountry);
			SetValue(pivot.CI_ZZF_NKTaxTypeInfo, data.TaxType);
			SetValue(pivot.CI_RW_NKOriginStateInfo, data.OriginState);
			SetValue(pivot.CI_PrimaryPreferenceInfo, data.PrimaryPreference);
			SetValue(pivot.CI_SecondaryPreferenceInfo, data.SecondaryPreference);
			SetValue(pivot.CI_ValuationCodeInfo, data.ValuationCode);
			SetValue(pivot.CI_UsageCommentInfo, data.UsageComment);
			SetValue(pivot.CI_DescriptionInfo, data.ClassificationDescription);
			SetValue((ZPropertyInfoDecimal)pivot.CI_ValuationMarkupInfo, data.ValuationMarkup);

			var pivotClassType = pivot.GetType();
			foreach (var valueObject in data.GetAddInfoValues())
			{
				var propertyInfo = pivotClassType.GetProperty(valueObject.Key);
				if (propertyInfo != null)
				{
					propertyInfo.SetValue(pivot, valueObject.Value);
				}
			}
		}

		protected override IEnumerable<ZString> GetPivotPropertiesToSuspendSetting()
		{
			foreach (var property in base.GetPivotPropertiesToSuspendSetting())
			{
				yield return property;
			}
			if (HasColumn(FieldNames.Origin))
			{
				yield return BaseCusClassPartPivot.Schema.CI_RN_NKCountryOfOrigin;
			}
		}

		#endregion

		#region Classification Change

		protected override void SaveAndClearPreviousClassificationLookupDetailsIfPresentAndDifferent(MasterFiles.Business.OrgSupplierPart enterprisePart, ZString importLookup, ZString exportLookup, PartsDataToLoad dataToLoad)
		{
			var part = (OrgSupplierPart)enterprisePart;
			if (UseOldClassificationFields)
			{
				var classificationTypes = new string[] { ClassificationTypeList.Codes.HTI, ClassificationTypeList.Codes.HTE };
				if (UseScheduleB)
				{
					classificationTypes = classificationTypes.Add(ScheduleBClassificationType, StringComparer.OrdinalIgnoreCase);
				}
				foreach (var classType in classificationTypes)
				{
					var pivots = part.PivotsForBinding.GetNonDeletedPivots().Where(x => x.CI_ChildType == classType).ToArray();
					if (pivots.Length > 1)
					{
						throw new ArgumentException(Res.GetString("6797FF22-7FE7-40FC-85B4-AF1B1F0369D7", "contains multiple {0} records and cannot be update by the import.", classType));
					}
				}

				SaveDetails(GetPivotUseOldClassificationFields(part, BaseCusClassification.ClassificationType.IMP, false), importLookup, BaseCusClassification.ClassificationType.IMP, dataToLoad.ImportTariff);

				var exportTariff = ZString.Empty;
				if (!exportLookup.IsEmpty)
				{
					exportTariff = GetClassification(exportLookup, BaseCusClassification.ClassificationType.EXP)?.CC_TariffNum ?? ZString.Empty;
				}
				else if (!dataToLoad.ExportTariff.IsEmpty)
				{
					exportTariff = dataToLoad.ExportTariff;
				}

				var isScheduleB = IsScheduleB(exportTariff);
				SaveDetails(GetPivotUseOldClassificationFields(part, BaseCusClassification.ClassificationType.EXP, isScheduleB), exportLookup, BaseCusClassification.ClassificationType.EXP, dataToLoad.ExportTariff);
			}
			else
			{
				var partsData = (GlobalPartsDataToLoad)dataToLoad;
				var classificationType = partsData.ClassificationType;
				var pivots = GetPivots(part, classificationType);

				if (pivots.Take(2).Count() > 1)
				{
					throw new ArgumentException(Res.GetString("6797FF22-7FE7-40FC-85B4-AF1B1F0369D7", "contains multiple {0} records and cannot be update by the import.", classificationType));
				}
				else
				{
					var pivot = pivots.FirstOrDefault();
					if (pivot != null)
					{
						SaveDetails(pivot, partsData.ClassificationLookup, GetClassificationType(partsData), partsData.Tariff);
					}
				}
			}
		}

		void SaveDetails(BaseCusClassPartPivot pivot, ZString newLookup, ZString classificationType, ZString newTariff)
		{
			if (pivot != null && (!newTariff.IsEmpty || GetClassificationPK(newLookup, classificationType).IsValid))
			{
				var existingClassification = GetClassificationFromPivot(pivot);
				if (existingClassification != null)
				{
					var lookupCode = existingClassification.CC_LookupCode;
					if (!lookupCode.IsEmpty && lookupCode != newLookup)
					{
						AddNote(pivot, lookupCode, true);
					}
				}
				else
				{
					var tariff = pivot.CI_TariffNum;
					if (!tariff.IsEmpty && tariff != newTariff)
					{
						AddNote(pivot, tariff, false);
					}
				}
			}
		}

		void AddNote(BaseCusClassPartPivot pivot, ZString code, bool isLookup)
		{
			string lookup = Res.GetString("EDD4194E-0B96-4E49-B6BC-A36895D0B1CB", "Previous {1}: {0}", code, isLookup
				? Res.GetString("674c1c08-04c2-4256-adfc-267d7927d505", "Lookup")
				: Res.GetString("8e1f53a2-a8e9-4440-9b18-bd233761ef0a", "Tariff"));
			pivot.Part.Notes.AddNew(true, Res.GetString("9912F8F6-0107-4988-A4D6-A006D0BE89B6", "Data Import Update: Previous {0} Details", pivot.CI_ChildType), lookup);
		}

		BaseCusClassification GetClassificationFromPivot(BaseCusClassPartPivot pivot)
		{
			return pivot != null ? pivot.Classification : null;
		}

		BaseCusClassPartPivot GetPivotUseOldClassificationFields(OrgSupplierPart enterprisePart, string classificationType, bool isExportClass)
		{
			var country = enterprisePart.CurrentCompanyCustomsCountryCode;
			if (classificationType == BaseCusClassification.ClassificationType.IMP)
			{
				return enterprisePart.GetPivots<BaseCusClassPartPivot>(country).GetImportMatch(country, ZGuid.Empty, ZGuid.Empty, UseOldClassificationFields);
			}
			else
			{
				return enterprisePart.GetPivots<BaseCusClassPartPivot>(country).GetExportMatch(country, isExportClass, ZGuid.Empty, ZGuid.Empty, UseOldClassificationFields);
			}
		}

		IEnumerable<BaseCusClassPartPivot> GetPivots(OrgSupplierPart enterprisePart, string pivotType)
		{
			return enterprisePart.GetPivots<BaseCusClassPartPivot>(enterprisePart.CurrentCompanyCustomsCountryCode).GetNonDeletedPivots().Where(x => x.CI_ChildType == pivotType);
		}

		#endregion

		public ZString CurrentCompanyCustomsCountryCode
		{
			get
			{
				if (!currentCompanyCustomsCountryCode.HasValue)
				{
					currentCompanyCustomsCountryCode = Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				}
				return currentCompanyCustomsCountryCode.Value;
			}
		}
		ZString? currentCompanyCustomsCountryCode;

		#region Classification

		protected override ZGuid GetClassificationPK(string partClassCode, string classType)
		{
			var classification = GetClassification(partClassCode, classType);
			return classification != null ? classification.PK : ZGuid.Empty;
		}

		protected virtual BaseCusClassification GetClassification(string partClassCode, string classType)
		{
			return BaseCusClassification.LoadFromLookupCode(Factory, partClassCode, classType, CurrentCompanyCustomsCountryCode);
		}

		protected virtual ZString GetClassificationType(PartsDataToLoad dataToLoad)
		{
			ZString classType = ZString.Empty;
			var partsData = dataToLoad as GlobalPartsDataToLoad;

			switch (partsData.ClassificationType)
			{
				case ClassificationTypeList.Codes.HTI:
					classType = BaseCusClassification.ClassificationType.IMP;
					break;
				case ClassificationTypeList.Codes.HTE:
					classType = BaseCusClassification.ClassificationType.EXP;
					break;
				case ClassificationTypeList.Codes.HTB:
					classType = BaseCusClassification.ClassificationType.Both;
					break;
			}

			return classType;
		}

		#endregion

		#region Set UQ

		protected override string GetUQFromTariff(ZString tariffCode, ZString classType)
		{
			var result = ZString.Empty;
			var tariff = new Universal.TariffView.Loader(Factory).LoadMostRecentCachedTariff(CurrentCompanyCustomsCountryCode, tariffCode, ZDateTime.Now);
			if (tariff != null)
			{
				result = tariff.UnitsOfMeasure.OfType<Universal.TariffUOMView>().FirstOrDefault(x => x.ZZ8_Type == Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType)?.ZZ8_UOM ?? ZString.Empty;
			}

			return result;
		}

		ZString GetUQFromClassification(ZString partClassification, ZString classificationType)
		{
			ZString uq = ZString.Empty;

			var classification = GetClassification(partClassification, classificationType);
			if (classification != null)
			{
				uq = GetUQFromTariff(classification.CC_TariffNum, classificationType);
			}

			return uq;
		}

		protected override void SetRecordUQFromTariff(PartsDataToLoad record)
		{
			ZString uq = ZString.Empty;

			if (UseOldClassificationFields)
			{
				if (!record.PartClassification.IsEmpty)
				{
					uq = GetUQFromClassification(record.PartClassification, BaseCusClassification.ClassificationType.IMP);
				}

				if (uq.IsEmpty && !record.PartExportClassification.IsEmpty)
				{
					uq = GetUQFromClassification(record.PartExportClassification, BaseCusClassification.ClassificationType.EXP);
				}

				if (uq.IsEmpty && !record.ImportTariff.IsEmpty)
				{
					uq = GetUQFromTariff(record.ImportTariff, BaseCusClassification.ClassificationType.IMP);
				}

				if (uq.IsEmpty && !record.ExportTariff.IsEmpty)
				{
					uq = GetUQFromTariff(record.ExportTariff, BaseCusClassification.ClassificationType.EXP);
				}
			}
			else
			{
				var classificationType = GetClassificationType(record);

				if (!record.ClassificationLookup.IsEmpty)
				{
					uq = GetUQFromClassification(record.ClassificationLookup, classificationType);
				}

				if (uq.IsEmpty && !record.Tariff.IsEmpty)
				{
					uq = GetUQFromTariff(record.Tariff, classificationType);
				}
			}

			var unitList = RefPackTypeCollection.GetAsCodeDescriptionPairWithStandardUnits(Factory);
			record.PartUQ = unitList.ContainsCode(uq) ? uq : ZString.Empty;
		}

		#endregion
	}
}
