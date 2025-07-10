using System;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(AgencyShipment))]
	internal class AgencyShipmentBOTest : EnterpriseBusinessObjectTestCase
	{
		#region Metadata
		protected override Type ExpectedMetadataType
		{
			get
			{
				return typeof(Metadata.Business.AgencyShipment);
			}
		}

		#endregion

		public void TestValidateUniversalCopyPreconditions()
		{
			var agencyBooking = Factory.New<AgencyBooking>();

			AssertEquals(null, agencyBooking.ValidateUniversalCopyPreconditions(null));

			agencyBooking.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			AssertEquals("This Booking has now been confirmed to Bill of Lading and is no longer accessible from the Booking module. Please access from Bill of Lading module.", agencyBooking.ValidateUniversalCopyPreconditions(null));
		}
	}
}
