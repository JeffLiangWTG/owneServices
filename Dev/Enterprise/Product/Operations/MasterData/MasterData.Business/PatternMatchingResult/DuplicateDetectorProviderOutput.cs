using System;

namespace Enterprise.MasterData.Business
{
	public class DuplicateDetectorProviderOutput
	{
		public DuplicateDetectorProviderOutput(string masterSourceTable, Guid masterPK, string targetSourceTable, Guid targetPK, double score)
		{
			MasterSourceTable = masterSourceTable;
			MasterPK = masterPK;
			TargetSourceTable = targetSourceTable;
			TargetPK = targetPK;
			Score = score;
		}

		public Guid MasterPK { get; }

		public string MasterSourceTable { get; }

		public Guid TargetPK { get; }

		public string TargetSourceTable { get; }

		public double Score { get; }
	}
}
