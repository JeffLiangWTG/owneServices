using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CargoWise.Tools.DuplicateDetector;
using Enterprise.MasterData.Common;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class OrgHeaderDuplicationFinder_ThatTimesOutForTest : ISupportDuplicationFinder
	{
		public Type TargetType => throw new NotImplementedException();

		public bool ShouldStopProcessing { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		public List<ScoringResult> ScoringResults => throw new NotImplementedException();

		protected int RegistryTimeout => 0;

		public DuplicationStatus LastRunStatus { get; set; }

		public Task FindDuplicates()
		{
			throw new NotImplementedException();
		}

		public void RequestToCancel()
		{
			throw new NotImplementedException();
		}
	}
}
