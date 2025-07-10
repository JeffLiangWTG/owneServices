using System;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	public static class IGlbBranchExtension
	{
		public static IDisposable SetAsTemporaryContext(this IGlbBranch branch)
		{
			return new TemporaryUserContext() { BranchPK = branch.PK.ToGuid() }.Set();
		}
	}
}
