using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.InBond.Business.Universal
{
	public class CusInBondMoveDetailDataObjectWriter : DataTransfer.Universal.CusInBondMoveDetailDataObjectWriter
	{
		public CusInBondMoveDetailDataObjectWriter(IDataWritingManager writeManager, InBondDataObjectWriterHelper helper, Shipment headerData, bool includeWarehouseData = false)
			: base(writeManager, helper, headerData)
		{
			this.includeWarehouseData = includeWarehouseData;
		}
		readonly bool includeWarehouseData;

		protected new InBondDataObjectWriterHelper Helper
		{
			get { return (InBondDataObjectWriterHelper)base.Helper; }
		}

		protected override void PopulateInBondSpecificData(US.Business.CusInBondMoveDetail moveDetailBO, InBondMoveDetail moveDetailData)
		{
			base.PopulateInBondSpecificData(moveDetailBO, moveDetailData);
			moveDetailData.SequenceNumber = moveDetailBO.B9_SeqNo;
			var inBondMoveDetailBO = (CusInBondMoveDetail)moveDetailBO;
			var isAir = inBondMoveDetailBO.Header?.IsAir ?? false;
			if (!isAir)
			{
				moveDetailData.PreviousInBondTransitDate = inBondMoveDetailBO.B9_PreviousITDate;
				moveDetailData.PreviousInBondTransitType = ListHelper.GetWithDescription<CodeDescriptionPair2Char>(inBondMoveDetailBO.B9_PreviousITType, inBondMoveDetailBO.Lookups.EntryTypeList);
				moveDetailData.PreviousInBondTransitPortScheduleD = ListHelper.GetWithDescription<CodeDescriptionPair4Char>(inBondMoveDetailBO.B9_PreviousITPortDCode, inBondMoveDetailBO.Lookups.RegionDistrictPorts);
				moveDetailData.InBondQuantity = inBondMoveDetailBO.B9_InBoundQty;
				PopulateContainers(inBondMoveDetailBO, moveDetailData);
				PopulateSecondaryNotifyParties(inBondMoveDetailBO, moveDetailData);
			}
			Helper.PopulateDispositions(inBondMoveDetailBO, moveDetailData);
			PopulateCBP7512Lines(inBondMoveDetailBO.CBP7512Lines, moveDetailData);
			if (moveDetailBO.MoveHeader?.Header?.BH_FTZMove ?? false)
			{
				PopulateWarehouseDetail(inBondMoveDetailBO.WarehouseDetails, moveDetailData);
			}
		}

		void PopulateSecondaryNotifyParties(CusInBondMoveDetail moveDetailBO, InBondMoveDetail moveDetailData)
		{
			moveDetailData.CustomsReferenceCollection = new List<CustomsReference>();
			var query = new ZQuery(CusCodeDataSchema.CY_ParentID, moveDetailBO.PK);
			query.AddToFilter(CusCodeDataSchema.CY_Code, new[] {
				SecondaryNotifyPartyCodeList.Codes.First,
				SecondaryNotifyPartyCodeList.Codes.Second,
				SecondaryNotifyPartyCodeList.Codes.Third,
				SecondaryNotifyPartyCodeList.Codes.Fourth
			});
			query.FetchOnlyFromLocalCache = !moveDetailBO.IsInDatabase;
			query.OrderBy = CusCodeDataSchema.CY_Code.Name;
			var snpBOs = Helper.Load<SecondaryNotifyParty>(query);
			if (snpBOs.Length > 0)
			{
				AddSecondaryNotifyParty(snpBOs.FirstOrDefault(x => x.CY_Code == SecondaryNotifyPartyCodeList.Codes.First), moveDetailData.CustomsReferenceCollection);
				AddSecondaryNotifyParty(snpBOs.FirstOrDefault(x => x.CY_Code == SecondaryNotifyPartyCodeList.Codes.Second), moveDetailData.CustomsReferenceCollection);
				AddSecondaryNotifyParty(snpBOs.FirstOrDefault(x => x.CY_Code == SecondaryNotifyPartyCodeList.Codes.Third), moveDetailData.CustomsReferenceCollection);
				AddSecondaryNotifyParty(snpBOs.FirstOrDefault(x => x.CY_Code == SecondaryNotifyPartyCodeList.Codes.Fourth), moveDetailData.CustomsReferenceCollection);
			}
		}

		void AddSecondaryNotifyParty(SecondaryNotifyParty snpBO, List<CustomsReference> collection)
		{
			if (snpBO != null)
			{
				collection.Add(new CustomsReference()
				{
					Type = new CodeDescriptionPair() { Code = Constants.SecondaryNotifyParty.Type, Description = Constants.SecondaryNotifyParty.TypeDescription },
					SubType = ListHelper.GetWithDescription<CodeDescriptionPair35Char>(snpBO.CY_Code, snpBO.Lookups.CY_CodeList),
					Reference = snpBO.CY_Data
				});
			}
		}

		void PopulateCBP7512Lines(CusInBondMoveLineItemCollection lines, InBondMoveDetail moveDetailData)
		{
			moveDetailData.InBondMoveLineItemCollection = new List<InBondMoveLineItem>();
			foreach (var lineItemBO in lines)
			{
				moveDetailData.InBondMoveLineItemCollection.Add(new InBondMoveLineItem()
				{
					PrintingSequenceNo = lineItemBO.BI_PrintingSequenceNo,
					MarksAndNumbers = lineItemBO.BI_MarksAndNumbers,
					DescriptionAndQuantityOfMerchandise = lineItemBO.BI_Description,
					Weight = lineItemBO.BI_Weight,
					WeightUnit = ListHelper.GetWithDescription<CodeDescriptionPair>(lineItemBO.BI_WeightUnit, lineItemBO.Lookups.WeightUnitList),
					MonetaryValue = lineItemBO.BI_MonetaryValue,
					RateComment = lineItemBO.BI_RateComment,
					DutyComment = lineItemBO.BI_DutyComment,
					IsMonetaryValueEstimated = lineItemBO.BI_IsMonetaryValueEstimated
				});
			}
		}

		void PopulateWarehouseDetail(WarehouseDetailCollection lines, InBondMoveDetail moveDetailData)
		{
			moveDetailData.WarehouseDetailCollection = new List<InBondWarehouseDetail>();
			foreach (WarehouseDetail warehouseDetailBO in lines)
			{
				moveDetailData.WarehouseDetailCollection.Add(new InBondWarehouseDetail()
				{
					EntryNumber = warehouseDetailBO.US_WarehouseNumber,
					BondedQuantity = warehouseDetailBO.US_WarehouseBondedQuantity,
					WithdrawQuantity = warehouseDetailBO.US_WarehouseWithdrawQuantity
				});
			}
		}

		protected override DataTransfer.Universal.CusInBondContainerDataObjectWriter InBondContainerDataObjectWriter(IDataWritingManager writeManager, DataTransfer.Universal.InBondDataObjectWriterHelper helper)
		{
			return new CusInBondContainerDataObjectWriter(writeManager, Helper);
		}

		protected override DataTransfer.Universal.CusInBondCargoDescDataObjectWriter InBondCargoDescDataObjectWriter(IDataWritingManager writeManager, DataTransfer.Universal.InBondDataObjectWriterHelper helper)
		{
			return new CusInBondCargoDescDataObjectWriter(writeManager, HeaderData, Helper, includeWarehouseData);
		}

		protected override IEnumerable<Customs.Business.CusInBondCargoDesc> GetRelatedCommotities(Customs.Business.CusInBondContainer containerBO, DataTransfer.Universal.InBondDataObjectWriterHelper helper)
		{
			return Helper.Load<CusInBondCargoDesc>(((CusInBondContainer)containerBO).Commodities.CompleteFilter).OrderBy(x => CusInBondCargoDescComparer.GetOrderKey(x));
		}
	}
}
