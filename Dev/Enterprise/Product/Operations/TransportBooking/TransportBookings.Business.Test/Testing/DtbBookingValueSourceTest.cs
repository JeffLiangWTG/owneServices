using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportCommon.Business;
using Keys = Enterprise.Registry.Business.BillOfLadingNumberCustomisationElement.Keys;

namespace Enterprise.TransportBookings.Business.Testing
{
	public class DtbBookingValueSourceTest : TestCaseWithFactory
	{
		public void TestServiceLevel()
		{
			var booking = GetNewBooking();
			var valueProviders = new NumberGeneratorValueProviderCollection();
			valueProviders.AddRange(new DomesticValueSource(booking));
			booking.KM_RS_NKServiceLevel = "D2D";

			AssertEquals("ServiceLevel", "D2D", valueProviders[Keys.ServiceLevel].GetValue(Generator, ""));
		}

		DtbBooking GetNewBooking()
		{
			return Factory.NewWithValidTestData<DtbBooking>();
		}

		NumberGenerator Generator
		{
			get
			{
				if (generator == null)
				{
					generator = new NumberGenerator();
					generator.Factory = Factory;
					generator.Context = new NumberGeneratorContext();
				}
				return generator;
			}
		}

		NumberGenerator generator;
	}
}
