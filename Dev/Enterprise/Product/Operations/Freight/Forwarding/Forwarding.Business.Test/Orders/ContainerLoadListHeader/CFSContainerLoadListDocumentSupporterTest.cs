using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(CFSContainerLoadListDocumentSupporter))]
	sealed class CFSContainerLoadListDocumentSupporterTest : DocumentSupporterTest
	{
		#region Implementation

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.New<CFSContainerLoadList>();
		}

		#endregion
	}
}
