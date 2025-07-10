using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transit.Business;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public class TransitConsignmentOrderReferenceDataObjectWriter : DataObjectWriter<WhsItemConsignmentOrderReference, OrderNumber>
	{
		public TransitConsignmentOrderReferenceDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected override OrderNumber PopulateDataObject(WhsItemConsignmentOrderReference consignmentOrderReference)
		{
			return new OrderNumber
			{
				OrderReference = consignmentOrderReference.WOR_OrderReference,
				Sequence = NextSequenceNo
			};
		}

		ZShort NextSequenceNo
		{
			get { return SequenceNo++; }
		}

		ZShort SequenceNo { get; set; }
	}
}
