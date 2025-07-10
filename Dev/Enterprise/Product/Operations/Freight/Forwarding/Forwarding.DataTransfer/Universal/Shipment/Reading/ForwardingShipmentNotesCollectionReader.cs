using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	class ForwardingShipmentNotesCollectionReader : NotesCollectionReader
	{
		public ForwardingShipmentNotesCollectionReader(DataObjectList<Note> notes, IXmlImportLogger logger, UniversalObjectFactory factory, IUniversalXMLNoteParent noteParent, IEnumerable<ZString> skippedNoteDescriptions = null)
			: base(notes, logger, factory, noteParent, skippedNoteDescriptions)
		{ }

		protected override StmNote ReadIntoBusinessObject(Note dataObject, StmNote note)
		{
			var reader = new ForwardingShipmentNoteDataObjectReader(dataObject, logger, factory, noteParent, (dataObj) => note);
			return reader.ReadIntoBusinessObject();
		}
	}
}
