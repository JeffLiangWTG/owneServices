namespace Enterprise.Rating.Rateable
{
	public class ClientProviderValues : IClientProviderValues
	{
		public ClientProviderValues(decimal actual, decimal forClient, decimal forProvider)
		{
			Actual = actual;
			ForClient = forClient;
			ForProvider = forProvider;
		}

		public ClientProviderValues(decimal actual)
			: this(actual, actual, actual)
		{
		}

		public decimal Actual { get; }
		public decimal ForClient { get; }
		public decimal ForProvider { get; }

		public override string ToString()
			=> string.Format("({0}, {1}, {2})", Actual, ForClient, ForProvider);

		public static ClientProviderValues Sum(IClientProviderValues first, IClientProviderValues second)
		{
			return new ClientProviderValues(first.Actual + second.Actual, first.ForClient + second.ForClient, first.ForProvider + second.ForProvider);
		}
	}
}
