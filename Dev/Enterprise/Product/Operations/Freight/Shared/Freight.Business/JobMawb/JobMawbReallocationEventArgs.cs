using System.ComponentModel;

namespace Enterprise.Freight.Business
{
	public class JobMawbReallocationEventArgs : CancelEventArgs
	{
		public JobMawbReallocationEventArgs(JobMawb mawb)
		{
			Mawb = mawb;
		}

		public JobMawb Mawb { get; private set; }
	}
}
