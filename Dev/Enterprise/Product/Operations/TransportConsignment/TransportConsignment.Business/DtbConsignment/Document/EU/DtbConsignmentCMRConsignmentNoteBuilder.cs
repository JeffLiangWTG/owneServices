using CargoWise.Common;
using Enterprise.Freight.Business;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbConsignmentCMRConsignmentNoteBuilder : ICMRConsignmentNoteBuilder
	{
		readonly DtbConsignment consignment;

		public DtbConsignmentCMRConsignmentNoteBuilder(DtbConsignment consignment)
		{
			this.consignment = Argument.NotNull(consignment, nameof(consignment));
		}

		public CMRConsignmentNoteDocDataObjectCollection Build()
		{
			var consignmentCmrNote = new DtbConsignmentCMRConsignmentNote(consignment);
			return new CMRConsignmentNoteDocDataObjectCollection([new CMRConsignmentNoteDocDataObject(consignmentCmrNote)]);
		}
	}
}
