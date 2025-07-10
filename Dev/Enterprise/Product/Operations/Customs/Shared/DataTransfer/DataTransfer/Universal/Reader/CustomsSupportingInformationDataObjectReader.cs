using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public class CustomsSupportingInformationDataObjectReader : DataObjectReader<UniversalCustoms.CustomsSupportingInformation>
	{
		public CustomsSupportingInformationDataObjectReader(UniversalCustoms.CustomsSupportingInformation supportingInfoDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ZGuid parentPK, ZString parentTableCode, GetMatchingDataPredicate getMatchingData = null)
			: base(supportingInfoDataObject, logger, factory)
		{
			this.parentPK = parentPK;
			this.parentTableCode = parentTableCode;
			this.getMatchingData = getMatchingData;
		}

		public IColumnIndexer ReadIntoDataRow()
		{
			IColumnIndexer row = null;
			if (ShouldReadDataObject())
			{
				var category = dataObject.Category.GetCodeAsUpperCase();
				var type = new CusSupportingInfoTypeDecider().GetTypeForLoad(category, ZString.Empty, dataObject.Type?.Code ?? null, parentTableCode, parentPK, factory.BOFactory);
				if (type != null)
				{
					row = GetOrCreateNew(CusSupportingInfoSchema.PK, type);
					if (row != null)
					{
						var cusSupportingInfo = row as CusSupportingInfo;
						var isSetterSuspendingEnabled = cusSupportingInfo != null;
						var setterSuspender = isSetterSuspendingEnabled ? cusSupportingInfo.SetterSuspender : new SetterSuspender();
						using (isSetterSuspendingEnabled ? SuspendSetters(setterSuspender) : null)
						{
							SetValue(row, CusSupportingInfoSchema.CSI_ParentID, parentPK);
							SetValue(row, CusSupportingInfoSchema.CSI_ParentTableCode, parentTableCode);
							SetValue(row, CusSupportingInfoSchema.CSI_Type, category);
							SetValue(row, CusSupportingInfoSchema.CSI_Code, dataObject.Type);
							SetValue(row, CusSupportingInfoSchema.CSI_RN_NKCountryCode, dataObject.Country);
							SetValue(row, CusSupportingInfoSchema.CSI_CustomsOffice, dataObject.CustomsOffice);
							SetValue(row, CusSupportingInfoSchema.CSI_DateOfIssue, dataObject.DateOfIssue);
							using (setterSuspender.ResumeSetting(CusSupportingInfo.Schema.CSI_Description))
							{
								SetValue(row, CusSupportingInfoSchema.CSI_Description, dataObject.Description);
							}
							SetValue(row, CusSupportingInfoSchema.CSI_LineNo, dataObject.LineNo);
							SetValue(row, CusSupportingInfoSchema.CSI_Procedure, dataObject.Procedure);
							SetValue(row, CusSupportingInfoSchema.CSI_Quantity, dataObject.Quantity);
							SetValue(row, CusSupportingInfoSchema.CSI_Quantity2, dataObject.Quantity2);
							SetValue(row, CusSupportingInfoSchema.CSI_Quantity3, dataObject.Quantity3);
							SetValue(row, CusSupportingInfoSchema.CSI_ReferenceNumber, dataObject.ReferenceNumber);
							SetValue(row, CusSupportingInfoSchema.CSI_ReferenceNumber2, GetReferenceNumber(dataObject, Constants.ReferenceNumberTypes.Codes.LocalReferenceNumber));
							SetValue(row, CusSupportingInfoSchema.CSI_ItemNumber, dataObject.ItemNumber);
							SetValue(row, CusSupportingInfoSchema.CSI_Status, dataObject.Status);
							SetValue(row, CusSupportingInfoSchema.CSI_SubType, dataObject.SubType);
							SetValue(row, CusSupportingInfoSchema.CSI_Tariff, dataObject.Tariff);
							SetValue(row, CusSupportingInfoSchema.CSI_UnitOfQuantity, dataObject.UnitOfQuantity);
							SetValue(row, CusSupportingInfoSchema.CSI_UnitOfQuantity2, dataObject.UnitOfQuantity2);
							SetValue(row, CusSupportingInfoSchema.CSI_UnitOfQuantity3, dataObject.UnitOfQuantity3);
							SetValue(row, CusSupportingInfoSchema.CSI_DateOfExpiry, dataObject.DateOfExpiry);
							SetValue(row, CusSupportingInfoSchema.CSI_RX_NKCurrency, dataObject.ValueCurrency);
							SetValue(row, CusSupportingInfoSchema.CSI_Value, dataObject.Value);
							SetValue(row, CusSupportingInfoSchema.CSI_AdditionalDescription, dataObject.AdditionalDescription);
							SetValue(row, CusSupportingInfoSchema.CSI_IssuerType, dataObject.IssuerType);
							SetValue(row, CusSupportingInfoSchema.CSI_PackQty, dataObject.PackQuantity);
							SetValue(row, CusSupportingInfoSchema.CSI_PackType, dataObject.PackUnitOfQuantity);
						}
					}
				}
			}
			return row;
		}

		IColumnIndexer GetOrCreateNew(SchemaPKColumn pK, Type type)
		{
			IColumnIndexer result = getMatchingData?.Invoke(dataObject);
			return result ?? CreateNewColumnIndexer(pK, type);
		}

		bool ShouldReadDataObject()
		{
			return dataObject.Category != null &&
				(dataObject.Type != null
					|| dataObject.Country != null || dataObject.CustomsOffice != null || dataObject.DateOfIssue.HasValue
					|| dataObject.Description.HasValue || dataObject.LineNo.HasValue || dataObject.Procedure != null
					|| dataObject.Quantity.HasValue || dataObject.Quantity2.HasValue || dataObject.Quantity3.HasValue
					|| dataObject.ReferenceNumber.HasValue || (dataObject.ReferenceNumberCollection?.Any() ?? false)
					|| dataObject.Status != null || dataObject.SubType != null || dataObject.Tariff.HasValue
					|| dataObject.UnitOfQuantity != null || dataObject.UnitOfQuantity2 != null || dataObject.UnitOfQuantity3 != null
					|| dataObject.DateOfExpiry.HasValue || dataObject.ValueCurrency != null || dataObject.Value.HasValue || dataObject.AdditionalDescription.HasValue
					|| dataObject.IssuerType != null || dataObject.ItemNumber.HasValue || dataObject.PackQuantity.HasValue || dataObject.PackUnitOfQuantity != null
				);
		}

		ZString GetReferenceNumber(UniversalCustoms.CustomsSupportingInformation dataObject, ZString numberType)
		{
			var refNumber = dataObject.ReferenceNumberCollection?.FirstOrDefault(
				number => (number.Type?.Code.HasValue ?? false) && number.Type.Code.Value == numberType
			);
			return refNumber?.ReferenceNumber ?? ZString.Empty;
		}

		IDisposable SuspendSetters(SetterSuspender setterSuspender) => setterSuspender.SuspendSetting(GetCusSupportingInfoPropertiesToSuspendSetting().ToArray());

		protected virtual IEnumerable<ZString> GetCusSupportingInfoPropertiesToSuspendSetting()
		{
			if (dataObject.Description.HasValue)
			{
				yield return CusSupportingInfo.Schema.CSI_Description;
			}
		}

		public delegate IColumnIndexer GetMatchingDataPredicate(UniversalCustoms.CustomsSupportingInformation customsSupportingInformation);

		readonly GetMatchingDataPredicate getMatchingData;
		readonly ZGuid parentPK;
		readonly ZString parentTableCode;
	}
}
