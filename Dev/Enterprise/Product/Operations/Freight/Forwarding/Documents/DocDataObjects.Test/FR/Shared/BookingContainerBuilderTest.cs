using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;

namespace Enterprise.Freight.Forwarding.Documents.Testing.FR
{
	sealed class BookingContainerBuilderTest : TestCaseWithFactory
	{
		public void TestPopulateContainerGenset()
		{
			var container = Factory.New<CommonContainer>();
			container.JC_IsControlledAtmosphere = true;
			container.JC_RefrigGeneratorID = "";
			var bookingContainer = new BookingContainerBuilder().Build(container, System.Array.Empty<BookingPackingLine>());

			Assert(!bookingContainer.Genset);

			container.JC_RefrigGeneratorID = "TestNumber1";
			bookingContainer = new BookingContainerBuilder().Build(container, System.Array.Empty<BookingPackingLine>());

			Assert(bookingContainer.Genset);

			container.JC_IsControlledAtmosphere = false;
			bookingContainer = new BookingContainerBuilder().Build(container, System.Array.Empty<BookingPackingLine>());

			Assert(!bookingContainer.Genset);
		}
	}
}
