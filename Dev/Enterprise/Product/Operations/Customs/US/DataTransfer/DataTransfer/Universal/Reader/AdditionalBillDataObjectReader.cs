using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.DataTransfer.Universal.AddInfoExtensions;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.DataTransfer.Universal
{
	public class AdditionalBillDataObjectReader : AdditionalBillDataObjectReader<Bill>
	{
		public AdditionalBillDataObjectReader(AdditionalBill additionalBillDataObject, IXmlImportLogger logger, UniversalDataObjectReaderHelper helper, AdditionalBillDataProvider<Bill> additionalBillDataProvider, BillDetail primaryMasterBillDetail, BillDetail primaryHouseBillDetail)
			: base(additionalBillDataObject, logger, helper, additionalBillDataProvider, primaryMasterBillDetail, primaryHouseBillDetail)
		{
		}

		protected override AddInfoDataObjectReader GetNewAddInfoDataObjectReader(Bill bill)
		{
			return new AddInfoDataObjectReader<Bill>(logger, helper, CusDecHouseBillSchema.CU_AddInfo, USAddInfoSchema.Instance);
		}

		protected override IEnumerable<ZString> GetSettingOrder(Bill bill)
		{
			yield return ColumnValueSetter.GetKey(bill.PK, CusDecHouseBillSchema.CU_BillType);
			yield return ColumnValueSetter.GetKey(bill.PK, CusDecHouseBillSchema.CU_BillNum);
			yield return ColumnValueSetter.GetKey(bill.PK, CusDecHouseBillSchema.CU_CU_ParentBill);
			yield return ColumnValueSetter.GetKey(bill.PK, USAddInfoSchema.US_UI_NKBillIssuerSCAC);
			yield return ColumnValueSetter.GetKey(bill.PK, CusDecHouseBillSchema.CU_NoOfPacks);
			yield return ColumnValueSetter.GetKey(bill.PK, CusDecHouseBillSchema.CU_PackType);
			yield return ColumnValueSetter.GetKey(bill.PK, USAddInfoSchema.US_SchDLoading);
			yield return ColumnValueSetter.GetKey(bill.PK, USAddInfoSchema.US_UC_NKCountryOfExport);
			yield return ColumnValueSetter.GetKey(bill.PK, USAddInfoSchema.US_US_NKLocationOfGoods);
			yield return ColumnValueSetter.GetKey(bill.PK, USAddInfoSchema.US_F_OH_PTTCarrier);
			yield return ColumnValueSetter.GetKey(bill.PK, USAddInfoSchema.US_F_Remarks);
		}

		string BillIssuerSCACKey
		{
			get { return Constants.AddInfoKeys.Declaration.WayBillIssuerSCAC; }
		}

		BillDetail AddBillIssuerSCACBillDetailIfExists(BillDetail billDetail, ZString? billIssuerSCAC)
		{
			if (billIssuerSCAC.HasValue)
			{
				billDetail.AddInfoCollection = billDetail.AddInfoCollection.AddSafe(UniversalDataBuss.DataObjects.Universal.AddInfo.New(BillIssuerSCACKey, billIssuerSCAC.Value));
			}
			return billDetail;
		}

		protected override BillDetail GetBillDetail()
		{
			return AddBillIssuerSCACBillDetailIfExists(base.GetBillDetail(), dataObject.AddInfoCollection == null ? null : dataObject.AddInfoCollection.GetZStringValue(BillIssuerSCACKey));
		}

		protected override BillDetail GetParentBillDetail()
		{
			return AddBillIssuerSCACBillDetailIfExists(base.GetParentBillDetail(), dataObject.AddInfoCollection == null ? null : dataObject.AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.AdditionalBill.ParentBillIssuerSCAC));
		}

		protected override BillDetail GetParentMasterBillDetail()
		{
			return AddBillIssuerSCACBillDetailIfExists(base.GetParentMasterBillDetail(), dataObject.AddInfoCollection == null ? null : dataObject.AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.AdditionalBill.ParentMasterBillIssuerSCAC));
		}

		protected override bool IsMatchingBill(IColumnIndexer billRow, BillDetail billDetail)
		{
			var result = true;
			if (billDetail != null && billDetail.BillNumber.HasValue)
			{
				result = billRow != null && billRow.GetValue(CusDecHouseBillSchema.CU_BillNum) == billDetail.BillNumber.Value;
				if (result)
				{
					var billIssuerSCAC = billDetail.AddInfoCollection == null ? null : billDetail.AddInfoCollection.GetZStringValue(BillIssuerSCACKey);
					if (billIssuerSCAC.HasValue)
					{
						var addInfo = billRow.GetAddInfos(CusDecHouseBillSchema.CU_AddInfo);
						result = addInfo.GetValue(USAddInfoSchema.US_UI_NKBillIssuerSCAC) == billIssuerSCAC.Value;
					}
				}
			}
			return result;
		}

		protected override bool IsMatchingPrimaryHouseBill(IColumnIndexer parentBillRow)
		{
			var result = false;
			if (IsMatchingOnBillDetail(primaryHouseBillDetail))
			{
				result = true;
				if (primaryMasterBillDetail != null && primaryMasterBillDetail.BillNumber.HasValue)
				{
					result = parentBillRow != null && IsMatchingBill(parentBillRow, primaryMasterBillDetail);
				}
			}
			return result;
		}

		bool IsMatchingOnBillDetail(BillDetail billDetail)
		{
			var result = false;
			if (dataObject.BillNumber.GetValueOrDefault() == billDetail.BillNumber.Value)
			{
				var billIssuerSCAC = GetWayBillIssuerSCAC(billDetail);
				result = !billIssuerSCAC.HasValue || (dataObject.AddInfoCollection != null && dataObject.AddInfoCollection.GetZStringValue(Bill.Schema.US_UI_NKBillIssuerSCAC.Substring(3)).GetValueOrDefault() == billIssuerSCAC.Value);
			}
			return result;
		}

		ZString? GetWayBillIssuerSCAC(BillDetail billDetail) => billDetail == null || billDetail.AddInfoCollection == null ? null : billDetail.AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.Declaration.WayBillIssuerSCAC);

		bool IsMatchingWayBillIssuerSCAC(Customs.Business.Bill bill, BillDetail billDetail)
		{
			var billIssuerSCAC = GetWayBillIssuerSCAC(billDetail);
			return !billIssuerSCAC.HasValue || ((Bill)bill).US_UI_NKBillIssuerSCAC.EqualsIgnoringCase(billIssuerSCAC.Value);
		}

		protected override bool IsMatchingPrimaryMasterBill()
		{
			return IsMatchingOnBillDetail(primaryMasterBillDetail);
		}

		protected override bool AdditionalPrimaryMasterBillMatching(Customs.Business.Bill bill) => IsMatchingWayBillIssuerSCAC(bill, primaryMasterBillDetail);

		protected override bool AdditionalPrimaryHouseBillMatching(Customs.Business.Bill bill)
		{
			var result = IsMatchingWayBillIssuerSCAC(bill, primaryHouseBillDetail);
			if (result && primaryMasterBillDetail != null && primaryMasterBillDetail.BillNumber.HasValue)
			{
				var parentBill = (Bill)bill.ParentBill;
				result = parentBill != null
					&& parentBill.CU_BillNum.EqualsIgnoringCase(primaryMasterBillDetail.BillNumber.Value)
					&& IsMatchingWayBillIssuerSCAC(parentBill, primaryMasterBillDetail);
			}
			return result;
		}
	}
}
