using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Tracking.Business.ImporterSecurityFiling;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingCusISFHeader))]
	sealed class TrackingISFHeaderIWebDocumentsSupportTest : IWebDocumentsSupportBaseTest
	{
		protected override IReadOnlyCollection<ZGuid> ExpectedDocRelatedPKs
		{
			get { return System.Array.Empty<ZGuid>(); }
		}

		protected override IWebDocumentsSupport GetNewBusinessObject()
		{
			return Factory.New<TrackingCusISFHeader>();
		}
	}
}
