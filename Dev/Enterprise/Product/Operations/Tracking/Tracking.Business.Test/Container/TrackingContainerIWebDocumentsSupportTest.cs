using System.Collections.Generic;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingContainer))]
	public class TrackingContainerIWebDocumentsSupportTest : IWebDocumentsSupportBaseTest
	{
		protected override IReadOnlyCollection<ZGuid> ExpectedDocRelatedPKs
		{
			get { return System.Array.Empty<ZGuid>(); }
		}

		protected override IWebDocumentsSupport GetNewBusinessObject()
		{
			return Factory.New<TrackingContainer>();
		}
	}
}
