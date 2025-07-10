using Enterprise.Customs.Common;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	class ForwardingConsolAdditionalReferenceCollectionReader : CusEntryAdditionalReferenceCollectionReader<ForwardingConsol>
	{
		public ForwardingConsolAdditionalReferenceCollectionReader(DataObjectList<AdditionalReference> additionalReferenceDataObjects, IXmlImportLogger logger, UniversalObjectFactory factory, ForwardingConsol consol)
			: base(additionalReferenceDataObjects, logger, factory, consol)
		{
		}

		protected override CusEntryNumAdditionalReferenceCollection Numbers
		{
			get { return Parent.Numbers; }
		}
	}
}
