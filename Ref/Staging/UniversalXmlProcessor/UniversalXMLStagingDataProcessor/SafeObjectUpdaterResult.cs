using System;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor
{
	public class SafeObjectUpdaterResult
	{
		public Guid ParentPK { get; set; }
		public string ParentCode { get; set; }
		public Guid? ExpirableAncestorPK { get; set; }
		public ResultAction Action { get; set; }
		public Guid? NewRecordForCloneActionPK { get; set; }
		public Guid? DatasetPK { get; set; }
	}

	public enum ResultAction
	{
		Update,
		Insert,
		Expire
	}
}
