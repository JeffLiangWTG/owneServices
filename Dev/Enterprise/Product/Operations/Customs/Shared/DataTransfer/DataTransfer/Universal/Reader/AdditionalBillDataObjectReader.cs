using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal.AddInfoExtensions;
using Enterprise.Customs.DataTransfer.Universal.DataReaderExtensions;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public class AdditionalBillDataObjectReader<TBill> : DataObjectReader<AdditionalBill, TBill>
		where TBill : Bill
	{
		public AdditionalBillDataObjectReader(AdditionalBill additionalBillDataObject, IXmlImportLogger logger, UniversalDataObjectReaderHelper helper, AdditionalBillDataProvider<TBill> additionalBillDataProvider, BillDetail primaryMasterBillDetail, BillDetail primaryHouseBillDetail)
			: base(additionalBillDataObject, logger, helper.Factory)
		{
			this.additionalBillDataProvider = Argument.NotNull(additionalBillDataProvider, nameof(additionalBillDataProvider));
			this.helper = helper;
			this.primaryMasterBillDetail = primaryMasterBillDetail;
			this.primaryHouseBillDetail = primaryHouseBillDetail;
		}

		protected readonly AdditionalBillDataProvider<TBill> additionalBillDataProvider;
		protected readonly UniversalDataObjectReaderHelper helper;
		protected readonly BillDetail primaryMasterBillDetail;
		protected readonly BillDetail primaryHouseBillDetail;

		protected sealed override TBill GetExistingBusinessObject()
		{
			TBill result = null;
			if (dataObject.BillType is WayBillType wayBillType)
			{
				var billNumber = dataObject.BillNumber.GetValueOrDefault();
				var billType = GetCustomsBillType(wayBillType);
				result = additionalBillDataProvider.GetAndRemoveExistingBill(billNumber, billType, GetAdditionalBillMatch);
			}
			return result;
		}

		bool GetAdditionalBillMatch(Bill bill)
		{
			var billRow = GetColumnIndexer(bill);
			var result = IsMatchingBill(billRow, GetBillDetail());
			if (result)
			{
				var billType = billRow.GetValue(CusDecHouseBillSchema.CU_BillType);
				if (billType != BillTypeList.Codes.MasterBill)
				{
					result = IsMatchingParentBill(billRow, GetParentBillDetail());
					if (result && billType == BillTypeList.Codes.SubHouseBill)
					{
						var parentBillRow = GetColumnIndexer(helper.Factory.Load<Bill>(billRow.GetValue(CusDecHouseBillSchema.CU_CU_ParentBill)));
						result = IsMatchingOnParentMasterBillDetail(parentBillRow);
					}
				}
			}
			return result;
		}

		protected sealed override TBill GetNewBusinessObject() => additionalBillDataProvider.CreateNewBill();

		protected override ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(TBill targetBO)
		{
			var result = ZString.Empty;
			if (dataObject.BillType.GetCodeAsUpperCase().IsEmpty)
			{
				result = Res.GetString("81BFD7CC-9201-4BE3-A301-F77153DD9F37", "{0} must be specified and not empty.", "BillType.Code");
			}
			return result;
		}

		ZString GetCustomsBillType(WayBillType wayBillType)
		{
			return helper.GetCustomsBillType(wayBillType);
		}

		protected sealed override void PopulateBusinessObject(TBill bill)
		{
			((ISupportDataImporting)bill).IsImportingData = true;

			var billRow = GetColumnIndexer(bill);
			var addInfoManager = bill as IAddInfoManager;
			var isDefaultingEnabled = IsDefaultingEnabled;
			var isAddInfoSerialisationEnabled = addInfoManager != null && !isDefaultingEnabled;
			ZBool? gUIPresentationRecord = null;
			Func<Bill, bool> additionalPrimaryBillMatching = null;
			try
			{
				if (isAddInfoSerialisationEnabled)
				{
					addInfoManager.SetUpdateFromAddInfoSerialisationFlag(false);
				}
				var billPK = billRow.GetValue(CusDecHouseBillSchema.PK);
				var billIsInDatabase = bill.IsInDatabase;
				SetValue(billRow, CusDecHouseBillSchema.CU_JE, additionalBillDataProvider.DeclarationPK);
				SetValue(billRow, CusDecHouseBillSchema.CU_ClusterKey, additionalBillDataProvider.ClusterKey);
				var delaySetters = isDefaultingEnabled ? new Dictionary<string, ValueSetter>() : null;
				SetValue(billRow, CusDecHouseBillSchema.CU_BillNum, dataObject.BillNumber, delaySetters);
				var billType = ZString.Empty;
				if (dataObject.BillType is WayBillType wayBillType && wayBillType.Code.HasValue)
				{
					billType = GetCustomsBillType(wayBillType);
					SetValue(billRow, CusDecHouseBillSchema.CU_BillType, billType, delaySetters);
				}

				SetValue(billRow, CusDecHouseBillSchema.CU_IssueDate, dataObject.IssueDate, delaySetters);
				SetValue(billRow, CusDecHouseBillSchema.CU_NoOfPacks, dataObject.NoOfPacks, delaySetters);
				SetValue(billRow, CusDecHouseBillSchema.CU_PackType, dataObject.PackType, delaySetters);

				if (billType == BillTypeList.Codes.MasterBill)
				{
					if (primaryMasterBillDetail != null && primaryMasterBillDetail.BillNumber.HasValue)
					{
						gUIPresentationRecord = IsMatchingPrimaryMasterBill();
						if (gUIPresentationRecord.GetValueOrDefault())
						{
							additionalPrimaryBillMatching = AdditionalPrimaryMasterBillMatching;
						}
					}
				}
				else
				{
					IColumnIndexer parentBillRow = null;
					if (dataObject.ParentBillNumber.HasValue)
					{
						var parentBillNumber = dataObject.ParentBillNumber.Value;
						var parentBillType = billType == BillTypeList.Codes.HouseBill ? BillTypeList.Codes.MasterBill : BillTypeList.Codes.HouseBill;
						parentBillRow = FindParentBillByBillNumberAndType(parentBillNumber, parentBillType);
						if (parentBillRow == null)
						{
							logger.Log(Integration.LogType.Warning, Enterprise.Customs.DataTransfer.Res.GetString("F19E3136-D147-4885-AF11-01E4F4D3341D", "Cannot find Parent Bill (Type:'{0}', Number:'{1}') for Bill (Type:'{2}', Number:'{3}').", parentBillType, parentBillNumber, bill.CU_BillType, bill.CU_BillNum));
							SetValue(billRow, CusDecHouseBillSchema.CU_CU_ParentBill, ZGuid.Empty, delaySetters);
						}
						else
						{
							SetValue(billRow, CusDecHouseBillSchema.CU_CU_ParentBill, parentBillRow.GetValue(CusDecHouseBillSchema.PK), delaySetters);
						}
						gUIPresentationRecord = billType == BillTypeList.Codes.HouseBill && primaryHouseBillDetail != null && primaryHouseBillDetail.BillNumber.HasValue && IsMatchingPrimaryHouseBill(parentBillRow);
					}
					else if (primaryMasterBillDetail == null)
					{
						gUIPresentationRecord = billType == BillTypeList.Codes.HouseBill && primaryHouseBillDetail != null && primaryHouseBillDetail.BillNumber.HasValue && IsMatchingPrimaryHouseBill(null);
					}

					if (gUIPresentationRecord.GetValueOrDefault())
					{
						additionalPrimaryBillMatching = AdditionalPrimaryHouseBillMatching;
					}
				}
				SetValue(billRow, CusDecHouseBillSchema.CU_GUIPresentationRecord, gUIPresentationRecord, delaySetters);

				if (helper.IsSourceAndTargetCountrySame)
				{
					if (addInfoManager != null)
					{
						GetNewAddInfoDataObjectReader(bill).ReadIntoRow(addInfoManager, billRow, dataObject, delaySetters);
					}
				}
				delaySetters.SetValueInSpecificOrder(GetSettingOrder(bill));
				if (isDefaultingEnabled)
				{
					addInfoManager.UpdateRelatedPropertyInfo();
				}
				if (helper.IsSourceAndTargetCountrySame)
				{
					new AddInfoGroupCollectionDataObjectReader(logger, helper).ReadIntoDataRows(billPK, CusDecHouseBillSchema.Constants.Prefix, billIsInDatabase, dataObject);
					new CustomsReferenceCollectionDataObjectReader(logger, helper).ReadIntoDataRows(billPK, CusDecHouseBillSchema.Constants.Prefix, billIsInDatabase, dataObject);
				}
			}
			finally
			{
				if (isAddInfoSerialisationEnabled)
				{
					addInfoManager.UpdateAddInfoFromString(billRow.GetValue(CusDecHouseBillSchema.CU_AddInfo));
					addInfoManager.SetUpdateFromAddInfoSerialisationFlag(true);
				}
				if (!isDefaultingEnabled && gUIPresentationRecord.GetValueOrDefault())
				{
					additionalBillDataProvider.SyncPrimaryBill(bill, bill.CU_BillNum, additionalPrimaryBillMatching);
				}
			}
		}

		protected virtual bool IsMatchingPrimaryHouseBill(IColumnIndexer parentBillRow)
		{
			return dataObject.BillNumber.GetValueOrDefault() == primaryHouseBillDetail.BillNumber.Value
				&& (primaryMasterBillDetail == null
				|| !primaryMasterBillDetail.BillNumber.HasValue
				|| !dataObject.ParentBillNumber.HasValue
				|| dataObject.ParentBillNumber.GetValueOrDefault() == primaryMasterBillDetail.BillNumber.Value);
		}

		protected virtual bool AdditionalPrimaryHouseBillMatching(Bill bill) => primaryMasterBillDetail == null
				|| !primaryMasterBillDetail.BillNumber.HasValue
				|| bill.CU_MasterBill.EqualsIgnoringCase(primaryMasterBillDetail.BillNumber.Value);

		protected virtual bool IsMatchingPrimaryMasterBill()
		{
			return dataObject.BillNumber.GetValueOrDefault() == primaryMasterBillDetail.BillNumber.Value;
		}

		protected virtual bool AdditionalPrimaryMasterBillMatching(Bill bill) => true;

		protected virtual IEnumerable<ZString> GetSettingOrder(TBill bill)
		{
			return Enumerable.Empty<ZString>();
		}

		protected virtual AddInfoDataObjectReader GetNewAddInfoDataObjectReader(TBill bill)
		{
			var addInfoSchema = (bill as IAddInfoManagerWithSchema)?.AddInfoSchema;
			if (addInfoSchema == null)
			{
				return new AdditionalBillAddInfoDataObjectReader(logger, helper);
			}
			else
			{
				return new BusinessObjectAddInfoDataObjectReader(bill.GetType(), logger, helper, CusDecHouseBillSchema.CU_AddInfo, addInfoSchema);
			}
		}

		IColumnIndexer FindParentBillByBillNumberAndType(ZString billNumber, ZString billType)
		{
			return GetColumnIndexerFromRow(factory.RowFactory.FindFirstCusDecHouseBillByBillNumberAndType(additionalBillDataProvider.DeclarationPK, billNumber, billType, (x) => GetAdditionalParentBillMatch(GetColumnIndexerFromRow(x))));
		}

		bool GetAdditionalParentBillMatch(IColumnIndexer billRow)
		{
			var result = IsMatchingBill(billRow, GetParentBillDetail());
			if (result && billRow.GetValue(CusDecHouseBillSchema.CU_BillType) == BillTypeList.Codes.HouseBill)
			{
				result = IsMatchingOnParentMasterBillDetail(billRow);
			}
			return result;
		}

		bool IsMatchingOnParentMasterBillDetail(IColumnIndexer billRow)
		{
			return IsMatchingParentBill(billRow, GetParentMasterBillDetail());
		}

		bool IsMatchingParentBill(IColumnIndexer billRow, BillDetail parentBillDetail)
		{
			var result = true;
			if (parentBillDetail != null && parentBillDetail.BillNumber.HasValue)
			{
				var parentBillRow = GetColumnIndexer(helper.Load<Bill>(billRow, CusDecHouseBillSchema.CU_CU_ParentBill));
				result = parentBillRow != null && IsMatchingBill(parentBillRow, parentBillDetail);
			}
			return result;
		}

		protected virtual bool IsMatchingBill(IColumnIndexer billRow, BillDetail billDetail)
		{
			var result = true;
			if (billDetail != null && billDetail.BillNumber.HasValue)
			{
				result = billRow != null && billRow.GetValue(CusDecHouseBillSchema.CU_BillNum) == billDetail.BillNumber.Value;
			}
			return result;
		}

		protected virtual BillDetail GetBillDetail()
		{
			return new BillDetail() { BillNumber = dataObject.BillNumber };
		}

		protected virtual BillDetail GetParentBillDetail()
		{
			return new BillDetail() { BillNumber = dataObject.ParentBillNumber };
		}

		protected virtual BillDetail GetParentMasterBillDetail()
		{
			return dataObject.AddInfoCollection == null ? null : new BillDetail() { BillNumber = dataObject.AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.AdditionalBill.ParentMasterBillNumber) };
		}
	}
}
