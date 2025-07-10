using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(CYContainerLoadListDocumentSupporter))]
	sealed class CYContainerLoadListDocumentSupporterTest : DocumentSupporterTest
	{
		#region Implementation

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.New<CYContainerLoadList>();
		}

		#endregion
	}
}
