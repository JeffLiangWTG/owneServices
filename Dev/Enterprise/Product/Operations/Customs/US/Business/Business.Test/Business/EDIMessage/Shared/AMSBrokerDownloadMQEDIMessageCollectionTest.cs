using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(AMSBrokerDownloadMQEDIMessageCollection))]
	sealed class AMSBrokerDownloadMQEDIMessageCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new AMSBrokerDownloadMQEDIMessageCollection(Factory);
	}
}
