using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingBulkSailingConsolGenerator))]
	sealed class ForwardingBulkSailingConsolGeneratorNonPersistenBizObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new ForwardingBulkSailingConsolGenerator(Factory);
		}
	}
}
