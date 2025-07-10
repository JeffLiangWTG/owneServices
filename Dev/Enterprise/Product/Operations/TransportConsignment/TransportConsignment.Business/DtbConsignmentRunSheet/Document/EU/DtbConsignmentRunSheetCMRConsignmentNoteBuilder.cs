using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.Freight.Business;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbConsignmentRunSheetCMRConsignmentNoteBuilder : ICMRConsignmentNoteBuilder
	{
		readonly DtbConsignmentRunSheet runSheet;

		public DtbConsignmentRunSheetCMRConsignmentNoteBuilder(DtbConsignmentRunSheet runSheet)
		{
			this.runSheet = Argument.NotNull(runSheet, nameof(runSheet));
		}

		public CMRConsignmentNoteDocDataObjectCollection Build()
		{
			var consignmentsFromRunSheet = ConsignmentRunSheetHelper.GetConsignmentsFromRunSheet(runSheet);
			var cmrConsignmentNoteDataObjects = new List<CMRConsignmentNoteDocDataObject>();

			foreach (var consignment in consignmentsFromRunSheet)
			{
				var consignmentCmrNote = new DtbConsignmentCMRConsignmentNote(consignment);
				cmrConsignmentNoteDataObjects.Add(new CMRConsignmentNoteDocDataObject(consignmentCmrNote));
			}
			
			return new CMRConsignmentNoteDocDataObjectCollection(cmrConsignmentNoteDataObjects);
		}
	}
}
