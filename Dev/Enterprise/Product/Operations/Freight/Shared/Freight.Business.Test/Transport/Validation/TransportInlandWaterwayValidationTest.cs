using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Business.Testing
{
	sealed class TransportInlandWaterwayValidationTest : BusinessObjectValidationTestCase
	{
		public void TestUsingCorrectValidation()
		{
			AssertEquals(typeof(TransportInlandWaterwayValidation), TransportValidation.New(Transport).GetType());
			AssertEquals(typeof(TransportInlandWaterwayValidation), Transport.Validation.GetType());
		}

		#region Implementation

		Transport Transport
		{
			get
			{
				if (transport == null)
				{
					transport = Factory.New<CommonShipment>().Transports.AddNew();
					transport.JW_TransportMode = Core.Constants.TransportModes.InlandWaterwayTransport;
				}
				return transport;
			}
		}
		Transport transport;

		#endregion
	}
}
