using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	class NX5105CMApplicationWine : IApplicationWine
	{
		public NX5105CMApplicationWine(CusTWControllingMessageHeader header)
		{
			this.header = header;
		}

		public IEnumerable<IAdditionalDocument> AdditionalDocuments => header.EthanolPermitNumbers.Cast<EthanolPermitNumberCusSupporting>().Select(x => new AdditionalDocumentWrapper(x.CSI_ReferenceNumber));

		public ZString GovernmentProcedurePreviousCode => header.TW1_PreWineInspectionStatus;

		public IPreviousDocument PreviousDocument => new PreviousDocumentWrapper(header.TW1_PrePermitNumber);

		CusTWControllingMessageHeader header { get; }
	}
}
