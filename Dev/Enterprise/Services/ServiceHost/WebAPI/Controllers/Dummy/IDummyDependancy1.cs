namespace Enterprise.Services.ServiceHost.NetCore
{
	public interface IDummyDependancy1
	{
		public int GetDummyNumber();
		public string GetDummyString();
	}

	public class DummyDependancy1 : IDummyDependancy1
	{
		public int GetDummyNumber()
		{
			return 1;
		}

		public string GetDummyString()
		{
			return nameof(DummyDependancy1);
		}
	}
}
