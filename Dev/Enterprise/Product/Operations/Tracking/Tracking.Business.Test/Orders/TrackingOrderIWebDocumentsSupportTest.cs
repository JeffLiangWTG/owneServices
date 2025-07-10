using System.Collections.Generic;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingOrder))]
	sealed class TrackingOrderIWebDocumentsSupportTest : IWebDocumentsSupportBaseTest
	{
		protected override IWebDocumentsSupport GetNewBusinessObject()
		{
			return Factory.New<TrackingOrder>();
		}

		protected override IReadOnlyCollection<ZGuid> ExpectedDocRelatedPKs
		{
			get { return System.Array.Empty<ZGuid>(); }
		}
	}
}
