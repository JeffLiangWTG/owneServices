using System;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	class ForwardingShipmentNoteDataObjectReader : NoteDataObjectReader
	{
		public ForwardingShipmentNoteDataObjectReader(Note noteDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, IUniversalXMLNoteParent parent, Func<Note, StmNote> noteBusinessObjectFinder = null)
			: base(noteDataObject, logger, factory, parent, noteBusinessObjectFinder)
		{ }

		protected override StmNote GetNewBusinessObject()
		{
			return factory.New<ForwardingShipmentStmNote>();
		}
	}
}
