using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.DataTransfer.Universal
{
	internal class ContainerAdditionalReferenceCollectionReader<T> : CusEntryAdditionalReferenceCollectionReader<T> where T : CommonContainer
	{
		public ContainerAdditionalReferenceCollectionReader(DataObjectList<AdditionalReference> additionalReferenceCollection, IXmlImportLogger logger, UniversalObjectFactory factory, T container)
			: base(additionalReferenceCollection, logger, factory, container)
		{
		}

		protected override CusEntryNumAdditionalReferenceCollection Numbers => Parent.AdditionalReferenceNumbers;
	}
}
