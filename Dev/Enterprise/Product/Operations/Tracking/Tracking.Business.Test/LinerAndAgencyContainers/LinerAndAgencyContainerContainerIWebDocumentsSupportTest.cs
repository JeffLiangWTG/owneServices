using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(LinerAndAgencyContainer))]
	sealed class LinerAndAgencyContainerContainerIWebDocumentsSupportTest : IWebDocumentsSupportBaseTest
	{
		protected override IReadOnlyCollection<ZGuid> ExpectedDocRelatedPKs
		{
			get { return System.Array.Empty<ZGuid>(); }
		}

		protected override IWebDocumentsSupport GetNewBusinessObject()
		{
			var agencyShipment = Factory.New<AgencyShipment>();
			var container = Factory.New<LinerAndAgencyContainer>();
			container.JC_JS_FCLBookingOnlyLink = agencyShipment.PK;
			return container;
		}
	}
}
