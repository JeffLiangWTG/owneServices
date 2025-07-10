namespace Enterprise.Services.ServiceHost.NetCore
{
	public interface IDummyDependancy2
	{
		public int GetDummyNumber();
		public string GetDummyString();
	}

	public class DummyDependancy2 : IDummyDependancy2
	{
		public int GetDummyNumber()
		{
			return 2;
		}

		public string GetDummyString()
		{
			return nameof(DummyDependancy2);
		}
	}
}
