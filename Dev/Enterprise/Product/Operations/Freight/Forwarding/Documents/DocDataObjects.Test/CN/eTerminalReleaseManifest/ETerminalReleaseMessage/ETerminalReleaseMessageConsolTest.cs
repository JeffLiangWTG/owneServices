using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.CN.Testing
{
	[TestedType(typeof(ETerminalReleaseMessageConsol))]
	sealed class ETerminalReleaseMessageConsolTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new ETerminalReleaseMessageConsol(Factory.New<ForwardingConsol>());
		}
	}
}
