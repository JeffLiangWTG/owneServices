using System.Collections.Generic;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	public class NX101Consignment : INX101Consignment
	{
		readonly CusTWControllingMessageHeader header;

		public NX101Consignment(CusTWControllingMessageHeader header)
		{
			this.header = header;
		}

		public INX101GovernmentAgencyGoodsItem GovernmentAgencyGoodsItem => new NX101GovernmentAgencyGoodsItem(header);

		public IEnumerable<IAdditionalDocument> AdditionalDocument => header.CertificateOfOrigins.Select(x => new AdditionalDocumentWrapper(x.CSI_ReferenceNumber));

		public ILocation UnloadingLocation => new LocationWrapper(id: header.TW1_RL_NKPortOfUnloading, name: header.TW1_PortOfUnloadingName);
	}
}
