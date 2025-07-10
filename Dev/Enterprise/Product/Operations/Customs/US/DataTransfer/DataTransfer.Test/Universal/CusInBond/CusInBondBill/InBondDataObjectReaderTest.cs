using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.US.DataTransfer.Universal.Testing
{
	partial class InBondDataObjectReaderTest<THeader, TBill, TMoveHeader, TMoveDetail, TContainer, TCommodity>
	{
		protected AdditionalBill SetupInBondBill(ZString billNumber, ZString billType, ZString parentBillNumber, ZDecimal manifestQty, ZInt link)
		{
			var result = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Link = link,
				BillNumber = billNumber,
				BillType = new WayBillType()
				{ Code = billType },
				ParentBillNumber = parentBillNumber,
				NoOfPacks = manifestQty,
			};
			return result;
		}
	}
}
