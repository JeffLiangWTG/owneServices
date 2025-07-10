using System.Collections.Generic;
using CargoWise.Common;

namespace Enterprise.Freight.Business
{
	public sealed class CMRConsignmentNoteDocDataObjectCollection
	{
		public CMRConsignmentNoteDocDataObjectCollection(List<CMRConsignmentNoteDocDataObject> cmrConsignmentNoteDocuments)
		{
			this.cmrConsignmentNoteDocuments = Argument.NotNull(cmrConsignmentNoteDocuments, nameof(cmrConsignmentNoteDocuments));
		}

		public List<CMRConsignmentNoteDocDataObject> CMRConsignmentNoteDocuments
		{
			get => cmrConsignmentNoteDocuments;
		}

		readonly List<CMRConsignmentNoteDocDataObject> cmrConsignmentNoteDocuments;
	}
}
