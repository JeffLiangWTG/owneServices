using System;

namespace Enterprise.MasterData.Business
{
	public class MergeProgressEventArgs : EventArgs
	{
		public MergeProgressEventArgs(int progressCount, int progressTotal)
		{
			ProgressCount = progressCount;
			ProgressTotal = progressTotal;
		}

		public int ProgressCount;

		public int ProgressTotal;
	}
}
