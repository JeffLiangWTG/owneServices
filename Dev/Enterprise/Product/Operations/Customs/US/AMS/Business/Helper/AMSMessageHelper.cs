namespace Enterprise.Customs.US.AMS.Business
{
	public class AMSMessageHelper
	{
		public AMSMessageHelper()
		{
		}

		public virtual int BatchSize
		{
			get
			{
				return 1000;
			}
		}

		public int Delay
		{
			get
			{
				return 10;
			}
		}
	}
}
