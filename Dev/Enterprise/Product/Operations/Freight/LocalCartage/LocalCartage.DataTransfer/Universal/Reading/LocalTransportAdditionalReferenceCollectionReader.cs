using Enterprise.Customs.Common;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.LocalCartage.DataTransfer.Universal
{
	class LocalTransportAdditionalReferenceCollectionReader : CusEntryAdditionalReferenceCollectionReader<CommonCartage>
	{
		public LocalTransportAdditionalReferenceCollectionReader(DataObjectList<AdditionalReference> additionalReferenceDataObjects, IXmlImportLogger logger, UniversalObjectFactory factory, CommonCartage cartage)
			: base(additionalReferenceDataObjects, logger, factory, cartage)
		{
		}

		protected override CusEntryNumAdditionalReferenceCollection Numbers
		{
			get { return Parent.AdditionalReferenceNumbers; }
		}
	}
}
