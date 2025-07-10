namespace Enterprise.Freight.Forwarding.Routing.S8.Business.Test
{
	class S8ClientForMutexTest : S8Client
	{
		public S8ClientForMutexTest() : base()
		{
		}

		protected override void InitialiseAndLogin()
		{
			throw new S8ClientException(LoginMutexExceptionMessage);
		}
	}
}
