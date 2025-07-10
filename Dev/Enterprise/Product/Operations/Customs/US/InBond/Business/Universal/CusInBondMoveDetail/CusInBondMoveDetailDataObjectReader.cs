using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.InBond.Business.Universal
{
	public class CusInBondMoveDetailDataObjectReader : DataTransfer.Universal.CusInBondMoveDetailDataObjectReader<CusInBondMoveDetail, CusInBondContainer, CusInBondCargoDesc>
	{
		public CusInBondMoveDetailDataObjectReader(InBondMoveDetail dataObject, IXmlImportLogger logger, InBondDataObjectReaderHelper helper, ZGuid moveHeaderPK, ZGuid billPK, ZString inBondNumber)
			: base(dataObject, logger, helper, moveHeaderPK, billPK, inBondNumber)
		{
		}

		protected new InBondDataObjectReaderHelper Helper
		{
			get { return (InBondDataObjectReaderHelper)base.Helper; }
		}

		protected override void FillInBondSpecificData(IColumnIndexer moveDetailRow)
		{
			base.FillInBondSpecificData(moveDetailRow);
			SetValue(moveDetailRow, CusInBondMoveDetailSchema.B9_SeqNo, dataObject.SequenceNumber);
			SetValue(moveDetailRow, CusInBondMoveDetailSchema.B9_PreviousITDate, dataObject.PreviousInBondTransitDate);
			SetValue(moveDetailRow, CusInBondMoveDetailSchema.B9_PreviousITType, dataObject.PreviousInBondTransitType);
			SetValue(moveDetailRow, CusInBondMoveDetailSchema.B9_PreviousITPortDCode, dataObject.PreviousInBondTransitPortScheduleD);
			var moveDetailPK = moveDetailRow.GetValue(CusInBondMoveDetailSchema.PK);
			FillSecondaryNotifyParties(moveDetailPK);
			FillInBondMoveLines(moveDetailPK, dataObject.InBondMoveLineItemCollection);
			FillInBondWarehouseDetail(moveDetailPK, dataObject.WarehouseDetailCollection);
		}

		void FillInBondMoveLines(ZGuid moveDetailPK, List<InBondMoveLineItem> inBondMoveLineItemCollection)
		{
			if (inBondMoveLineItemCollection != null)
			{
				var query = new ZQuery(CusInBondMoveLineItemSchema.BI_B9, moveDetailPK);
				query.OrderBy = CusInBondMoveLineItem.Schema.BI_PrintingSequenceNo + " ASC";
				query.FetchOnlyFromLocalCache = true;
				var lineItemBOs = factory.Load<CusInBondMoveLineItem>(query);

				foreach (var lineItemBO in lineItemBOs)
				{
					lineItemBO.Delete();
				}

				foreach (var lineItem in inBondMoveLineItemCollection)
				{
					var newLineItemRow = GetColumnIndexer(factory.New<CusInBondMoveLineItem>());
					SetValue(newLineItemRow, CusInBondMoveLineItemSchema.BI_B9, moveDetailPK);
					SetValue(newLineItemRow, CusInBondMoveLineItemSchema.BI_PrintingSequenceNo, lineItem.PrintingSequenceNo);
					SetValue(newLineItemRow, CusInBondMoveLineItemSchema.BI_MarksAndNumbers, lineItem.MarksAndNumbers);
					SetValue(newLineItemRow, CusInBondMoveLineItemSchema.BI_Description, lineItem.DescriptionAndQuantityOfMerchandise);
					SetValue(newLineItemRow, CusInBondMoveLineItemSchema.BI_Weight, lineItem.Weight);
					SetValue(newLineItemRow, CusInBondMoveLineItemSchema.BI_WeightUnit, lineItem.WeightUnit);
					SetValue(newLineItemRow, CusInBondMoveLineItemSchema.BI_MonetaryValue, lineItem.MonetaryValue);
					SetValue(newLineItemRow, CusInBondMoveLineItemSchema.BI_RateComment, lineItem.RateComment);
					SetValue(newLineItemRow, CusInBondMoveLineItemSchema.BI_DutyComment, lineItem.DutyComment);
					SetValue(newLineItemRow, CusInBondMoveLineItemSchema.BI_IsMonetaryValueEstimated, lineItem.IsMonetaryValueEstimated);
				}
			}
		}

		void FillInBondWarehouseDetail(ZGuid moveDetailPK, List<InBondWarehouseDetail> inBondWarehouseDetailCollection)
		{
			if (inBondWarehouseDetailCollection != null)
			{
				var query = new ZQuery(CusAddInfoSchema.B7_ParentID, moveDetailPK);
				query.FetchOnlyFromLocalCache = true;
				var warehouseDetailBOs = factory.Load<WarehouseDetail>(query);

				foreach (var warehouseDetailBO in warehouseDetailBOs)
				{
					warehouseDetailBO.Delete();
				}

				foreach (var wareHouseDetail in inBondWarehouseDetailCollection)
				{
					var newWarehouseDetailRow = GetColumnIndexer(factory.New<WarehouseDetail>());
					SetValue(newWarehouseDetailRow, CusAddInfoSchema.B7_ParentID, moveDetailPK);
					SetValue(newWarehouseDetailRow, CusAddInfoSchema.B7_ParentTableCode, (ZString)CusInBondMoveDetailSchema.Constants.Prefix);
					SetValue(newWarehouseDetailRow, CusAddInfoSchema.B7_Type, (ZString)CusAddInfoTypeAttribute.Codes.USWarehouseDetail);
					if (IsDefaultingEnabled)
					{
						SetValue(newWarehouseDetailRow, USWarehouseDetailAddInfoSchema.US_WarehouseNumber, wareHouseDetail.EntryNumber);
						SetValue(newWarehouseDetailRow, USWarehouseDetailAddInfoSchema.US_WarehouseBondedQuantity, wareHouseDetail.BondedQuantity);
						SetValue(newWarehouseDetailRow, USWarehouseDetailAddInfoSchema.US_WarehouseWithdrawQuantity, wareHouseDetail.WithdrawQuantity);
					}
					else
					{
						var addInfos = USWarehouseDetailAddInfo.Schema.US_WarehouseNumber.Substring(3) + "=" + wareHouseDetail.EntryNumber
							+ "*" + USWarehouseDetailAddInfo.Schema.US_WarehouseBondedQuantity.Substring(3) + "=" + wareHouseDetail.BondedQuantity
							+ "*" + USWarehouseDetailAddInfo.Schema.US_WarehouseWithdrawQuantity.Substring(3) + "=" + wareHouseDetail.WithdrawQuantity;
						SetValue(newWarehouseDetailRow, CusAddInfoSchema.B7_AddInfoData, addInfos);
					}
				}
			}
		}

		void FillSecondaryNotifyParties(ZGuid moveDetailPK)
		{
			if (dataObject.CustomsReferenceCollection != null)
			{
				foreach (var customsReferenceData in dataObject.CustomsReferenceCollection.Where(x => x.Type.GetCodeAsUpperCase() == Constants.SecondaryNotifyParty.Type))
				{
					var code = customsReferenceData.SubType.GetCodeAsUpperCase();
					if (code.IsEmpty || !factory.GetCachedValue<SecondaryNotifyPartyCodeList>().ContainsCode(code))
					{
						logger.Log(Integration.LogType.Error, ZString.Format("SubType '{0}' is an invalid Secondary Notify Party code.", code));
					}
					else
					{
						ZQuery query = new ZQuery(CusCodeDataSchema.CY_ParentID, moveDetailPK);
						query.AddToFilter(CusCodeDataSchema.CY_Code, code);
						query.FetchOnlyFromLocalCache = true;
						var snpBORow = GetColumnIndexer(factory.LoadTop1<SecondaryNotifyParty>(query) ?? factory.New<SecondaryNotifyParty>());
						SetValue(snpBORow, CusCodeDataSchema.CY_ParentTableCode, CusInBondMoveDetailSchema.Constants.Prefix);
						SetValue(snpBORow, CusCodeDataSchema.CY_ParentID, moveDetailPK);
						SetValue(snpBORow, CusCodeDataSchema.CY_Type, CusCodeDataTypeList.Codes.SecondaryNotifyParty);
						SetValue(snpBORow, CusCodeDataSchema.CY_Code, code);
						SetValue(snpBORow, CusCodeDataSchema.CY_Data, customsReferenceData.Reference);
					}
				}
			}
		}

		protected override DataTransfer.Universal.CusInBondContainerDataObjectReader<CusInBondContainer, CusInBondCargoDesc> InBondContainerDataObjectReader(Container containerData, IXmlImportLogger logger, DataTransfer.Universal.InBondDataObjectReaderHelper helper, ZGuid moveDetailPK)
		{
			return new CusInBondContainerDataObjectReader(containerData, logger, Helper, moveDetailPK);
		}
	}
}
