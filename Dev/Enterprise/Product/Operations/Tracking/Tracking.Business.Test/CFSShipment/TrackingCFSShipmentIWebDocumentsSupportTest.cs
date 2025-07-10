using System.Collections.Generic;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingCFSShipment))]
	sealed class TrackingCFSShipmentIWebDocumentsSupportTest : IWebDocumentsSupportBaseTest
	{
		protected override IWebDocumentsSupport GetNewBusinessObject()
		{
			var result = Factory.New<TrackingCFSShipment>();
			return result;
		}

		protected override IReadOnlyCollection<ZGuid> ExpectedDocRelatedPKs
		{
			get { return expectedDocRelatedPKs; }
		}

		readonly ZGuid[] expectedDocRelatedPKs = System.Array.Empty<ZGuid>();
	}
}
