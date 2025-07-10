using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(AgencyShipmentDocumentSupporter))]
	internal class AgencyShipmentDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestGetBODocDataProvidersNotFoundMessage_NoContainers()
		{
			var booking = Factory.New<AgencyBooking>();
			booking.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			booking.JS_UniqueConsignRef = "S00001";
			booking.ShippingContainers.RemoveAndDeleteAll();
			var contextArray = new[] { Constants.DataContext.AgencyContainer, Constants.DataContext.Container, Constants.DataContext.CartageAdvice, Constants.DataContext.GenericFreightJobByContainerIfFCL };
			var menu = Factory.New<StmMenuItem>();
			var expectedMessage = "No containers are entered for Shipping Booking S00001.";
			AssertNotFoundMessage(booking, menu, contextArray, true, expectedMessage);
			booking.ShippingContainers.AddNew();
			AssertNotFoundMessage(booking, menu, contextArray, false, ZString.Empty);
		}

		public void TestGetBODocDataProvidersNotFoundMessage_IMO()
		{
			var booking = Factory.New<AgencyBooking>();
			booking.JS_UniqueConsignRef = "S00001";
			booking.OuterPackLines.RemoveAndDeleteAll();
			var contextArray = new[] { Constants.DataContext.IMO };
			var menu = Factory.New<StmMenuItem>();
			var expectedMessage = "Shipping Booking S00001 doesn't have a packline with commodity HAZ.";
			AssertNotFoundMessage(booking, menu, contextArray, true, expectedMessage);
			var packLine = booking.OuterPackLines.AddNew();
			packLine.JL_RH_NKCommodityCode = Constants.CargoTypes.Hazardous;
			AssertNotFoundMessage(booking, menu, contextArray, false, ZString.Empty);
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.New<AgencyBooking>();
		}
	}
}
