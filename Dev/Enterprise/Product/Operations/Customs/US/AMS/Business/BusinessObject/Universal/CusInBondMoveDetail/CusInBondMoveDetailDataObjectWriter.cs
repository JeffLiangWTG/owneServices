using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.US.AMS.Business.Universal
{
	public class CusInBondMoveDetailDataObjectWriter : DataTransfer.Universal.CusInBondMoveDetailDataObjectWriter
	{
		public CusInBondMoveDetailDataObjectWriter(IDataWritingManager writeManager, InBondDataObjectWriterHelper helper, Shipment headerData)
			: base(writeManager, helper, headerData)
		{
		}

		protected new InBondDataObjectWriterHelper Helper
		{
			get { return (InBondDataObjectWriterHelper)base.Helper; }
		}

		protected override void PopulateInBondSpecificData(US.Business.CusInBondMoveDetail moveDetailBO, InBondMoveDetail moveDetailData)
		{
			base.PopulateInBondSpecificData(moveDetailBO, moveDetailData);
			var amsMoveDetailBO = (CusInBondMoveDetail)moveDetailBO;
			moveDetailData.InBondQuantity = amsMoveDetailBO.B9_InBoundQty;
			moveDetailData.MonetaryValue = amsMoveDetailBO.B9_MonetaryValue;
			moveDetailData.ForeignDestPortScheduleK = amsMoveDetailBO.B9_ForeignDestPortKCode;
			moveDetailData.ExportDate = amsMoveDetailBO.B9_ExportDate;
			moveDetailData.ExportVesselName = amsMoveDetailBO.B9_ExportLadenOn;
			moveDetailData.MessageStatus = ListHelper.GetWithDescription<CodeDescriptionPair>(amsMoveDetailBO.B9_MessageStatus, amsMoveDetailBO.Lookups.MessageStatusList);
			moveDetailData.DepartureStatus = ListHelper.GetWithDescription<CodeDescriptionPair>(amsMoveDetailBO.InBondDepartureStatus, amsMoveDetailBO.Lookups.InBondStatusList);
			moveDetailData.ArrivalStatus = ListHelper.GetWithDescription<CodeDescriptionPair>(amsMoveDetailBO.InBondArrivalStatus, amsMoveDetailBO.Lookups.InBondStatusList);
			moveDetailData.ExportationStatus = ListHelper.GetWithDescription<CodeDescriptionPair>(amsMoveDetailBO.InBondExportationStatus, amsMoveDetailBO.Lookups.InBondStatusList);
			moveDetailData.TransferOfLiabilityStatus = ListHelper.GetWithDescription<CodeDescriptionPair>(amsMoveDetailBO.InBondTransferOfLiabilityStatus, amsMoveDetailBO.Lookups.InBondStatusList);
			PopulateContainers(amsMoveDetailBO, moveDetailData);
		}

		protected override DataTransfer.Universal.CusInBondContainerDataObjectWriter InBondContainerDataObjectWriter(IDataWritingManager writeManager, DataTransfer.Universal.InBondDataObjectWriterHelper helper)
		{
			return new CusInBondContainerDataObjectWriter(writeManager, Helper);
		}

		protected override DataTransfer.Universal.CusInBondCargoDescDataObjectWriter InBondCargoDescDataObjectWriter(IDataWritingManager writeManager, DataTransfer.Universal.InBondDataObjectWriterHelper helper)
		{
			return new CusInBondCargoDescDataObjectWriter(writeManager, Helper);
		}

		protected override IEnumerable<Customs.Business.CusInBondCargoDesc> GetRelatedCommotities(Customs.Business.CusInBondContainer containerBO, DataTransfer.Universal.InBondDataObjectWriterHelper helper)
		{
			return Helper.Load<CusInBondCargoDesc>(((CusInBondContainer)containerBO).Commodities.CompleteFilter);
		}
	}
}
