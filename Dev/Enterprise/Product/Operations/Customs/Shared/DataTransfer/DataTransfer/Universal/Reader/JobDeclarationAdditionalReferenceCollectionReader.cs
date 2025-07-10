using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.DataTransfer.Universal
{
	class JobDeclarationAdditionalReferenceCollectionReader : CusEntryAdditionalReferenceCollectionReader<BaseJobDeclaration>
	{
		public JobDeclarationAdditionalReferenceCollectionReader(DataObjectList<AdditionalReference> additionalReferenceDataObjects, IXmlImportLogger logger, UniversalObjectFactory factory, BaseJobDeclaration declaration)
			: base(additionalReferenceDataObjects, logger, factory, declaration)
		{
		}

		protected override ZArchitecture.Business.Notes ParentNotes
		{
			get { return Parent.NotesOfDeclarationOrShipment; }
		}

		protected override CusEntryNumAdditionalReferenceCollection Numbers
		{
			get { return Parent.AdditionalReferenceNumbers; }
		}
	}
}
