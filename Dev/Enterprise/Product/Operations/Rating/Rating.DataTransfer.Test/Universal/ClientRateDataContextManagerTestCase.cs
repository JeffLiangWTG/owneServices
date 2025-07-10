using Enterprise.Rating.Business;
using NUnit.Framework;

namespace Enterprise.Rating.DataTransfer.Testing
{
	[TestedType(typeof(ClientRateDataContextManager))]
	public class ClientRateDataContextManagerTestCase : RatingHeaderDataContextManagerTestCase<ClientRateDataContextManager, ClientRate>
	{
		protected override ClientRate GetBusinessObjectForTesting() => Factory.NewWithValidTestData<ClientRate>();

		protected override RatingHeaderDataContextManager<ClientRate> GetNewContextManagerForTesting() => new ClientRateDataContextManager();
	}
}
