using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.US.AMS.Business.Universal
{
	public class CusInBondMoveHeaderDataObjectWriter : DataTransfer.Universal.CusInBondMoveHeaderDataObjectWriter
	{
		public CusInBondMoveHeaderDataObjectWriter(IDataWritingManager writeManager, InBondDataObjectWriterHelper helper, Shipment headerData)
			: base(writeManager, helper, headerData)
		{
		}

		protected new InBondDataObjectWriterHelper Helper
		{
			get { return (InBondDataObjectWriterHelper)base.Helper; }
		}

		protected override void PopulateInBondSpecificData(US.Business.CusInBondMoveHeader moveHeaderBO, InBondMoveHeader moveHeaderData)
		{
			base.PopulateInBondSpecificData(moveHeaderBO, moveHeaderData);
			var amsMoveHeaderBO = (CusInBondMoveHeader)moveHeaderBO;
			moveHeaderData.MessagingApplicationCode = ListHelper.GetWithDescription<CodeDescriptionPair>(moveHeaderBO.BM_SubApplicationCode, amsMoveHeaderBO.Lookups.SubApplicationCodeList);
			moveHeaderData.SequenceNumber = amsMoveHeaderBO.BM_ManifestSequenceNumber;
		}

		protected override DataTransfer.Universal.CusInBondMoveDetailDataObjectWriter InBondMoveDetailDataObjectWriter(IDataWritingManager writeManager, DataTransfer.Universal.InBondDataObjectWriterHelper helper, Shipment headerShipment)
		{
			return new CusInBondMoveDetailDataObjectWriter(writeManager, Helper, headerShipment);
		}
	}
}
