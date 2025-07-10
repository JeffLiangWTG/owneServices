using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	class TransportContractDocumentWrapper : ITransportContractDocument
	{
		public TransportContractDocumentWrapper(ZString id, ZString typeCode)
		{
			this.id = id;
			this.typeCode = typeCode;
		}

		readonly ZString id;
		readonly ZString typeCode;

		ZString ITransportContractDocument.ID => id;

		ZString ITransportContractDocument.TypeCode => typeCode;

		#region NotApplicable

		IPartyDetails ITransportContractDocument.Deconsolidator => null;

		#endregion
	}
}
