using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Business
{
	class TransportFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public TransportFetchStrategy(Transport transport)
			: base(transport)
		{
			this.transport = transport;
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			if (transport.JW_IsLinked)
			{
				Factory.AddFetchHint(typeof(JobSailing), transport.JW_JX);
			}
		}

		protected readonly Transport transport;
	}
}
