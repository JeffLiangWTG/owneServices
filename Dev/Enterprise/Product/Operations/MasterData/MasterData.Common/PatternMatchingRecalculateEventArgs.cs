using System;

namespace Enterprise.MasterData.Common
{
	public class RecalculatingEventArgs : EventArgs
	{
		public int Current { get; set; }
		public int Total { get; set; }
		public int Progress
		{
			get
			{
				if (Total == 0)
				{
					Total = 1;
				}

				return (Current * 100) / Total;
			}
		}
		public string ProgressText { get; set; }
	}

	public class RecalculatedEventArgs : EventArgs
	{
		public int EffectiveCount { get; set; }
		public string ErrorMessage { get; set; }
	}
}

