using System;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.TransportCommon.Business;
using Enterprise.TransportCommon.Business.Testing;

namespace Enterprise.TransportConsignment.Business.Testing
{
	class DtbBookingConsignmentNumberGenerationTest : DtbTransportNumberGenerationTest
	{
		#region Implementation

		protected override void AddCustomisationToRegistry(BillOfLadingNumberCustomisationsByServiceLevel customisation)
		{
			Registry.LandTransportRegistry.Instance.TransportConsignmentNumberFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, customisation);
		}

		protected override ZString FountainPrefixForTest
		{
			get { return "CN"; }
		}

		protected override DtbTransport GetSaveableTransportJob()
		{
			return Helper.CreateBookingConsignment();
		}

		#region Helper

		TransportBookingConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingConsignmentTestHelper(Factory)); }
		}
		TransportBookingConsignmentTestHelper helper;

		#endregion

		#endregion
	}
}
