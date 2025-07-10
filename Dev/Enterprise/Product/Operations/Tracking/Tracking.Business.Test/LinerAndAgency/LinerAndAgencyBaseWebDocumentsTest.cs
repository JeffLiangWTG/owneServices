using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;

namespace Enterprise.Tracking.Business.Testing
{
	public abstract class LinerAndAgencyBaseWebDocumentsTest : IWebDocumentsSupportBaseTest
	{
		protected override IReadOnlyCollection<ZGuid> ExpectedDocRelatedPKs
		{
			get { return System.Array.Empty<ZGuid>(); }
		}

		protected abstract AgencyShipment GetBusinessObjectForHelper();

		protected AgencyShipment Shipment;

		protected override void SetUp()
		{
			base.SetUp();
			Shipment = GetBusinessObjectForHelper();
		}
	}
}
