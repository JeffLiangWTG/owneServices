using System;

namespace CargoWise.RefDbRepo.Common.Utils
{
	public class CloneProcessResult
	{
		public string ClonedRecordTypeName { get; set; }
		public Guid OriginalRecordPK { get; set; }
		public Guid ClonedRecordPK { get; set; }
		public Guid? ClonedRecordExpirableAncestorPK { get; set; }
		public Guid DataSetPK { get; set; }
		public object ClonedRecord { get; set; }
	}
}
