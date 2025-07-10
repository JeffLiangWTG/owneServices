using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Manifest.Business
{
	public class N5101HTransportContractDocument : ITransportContractDocument
	{
		public N5101HTransportContractDocument(AsycudaBill bill)
		{
			this.bill = bill;
		}

		readonly AsycudaBill bill;

		ZString ITransportContractDocument.ID => bill.ABL_BillNumber;

		ZString ITransportContractDocument.TypeCode => ZString.Empty;

		IPartyDetails ITransportContractDocument.Deconsolidator => null;
	}
}
