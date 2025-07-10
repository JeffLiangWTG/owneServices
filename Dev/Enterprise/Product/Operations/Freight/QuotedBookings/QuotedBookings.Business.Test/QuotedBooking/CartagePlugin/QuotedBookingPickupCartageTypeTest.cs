using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.QuotedBookings.Business.Test
{
	public class QuotedBookingPickupCartageTypeTest : TestCaseWithFactory
	{
		public void TestCartageJobType()
		{
			ForwardingShipment shp = QuotedBooking.CreateNewBooking(Factory);
			shp.JS_PackingMode = Constants.ContainerModes.FCL;
			QuotedBookingPickupCartageType dect = new QuotedBookingPickupCartageType(QuotedBooking.New(ZGuid.Empty, shp.PK, Factory));
			AssertEquals(Constants.CartageJobType.NEW_FCLExportToSHP, dect.CartageJobType);
			JobDocAddress adr = Factory.NewWithValidTestData<JobDocAddress>();
			OrgAddress orgadr = Factory.New<OrgAddress>();
			OrgHeader oh = Factory.New<OrgHeader>();
			oh.OH_Code = "!#$AFSD";
			orgadr.OA_Address1 = "dasda";
			orgadr.OA_OH = oh.PK;
			adr.E2_OA_Address = orgadr.PK;
			shp.JS_OA_ExportReceivingDepot = orgadr.PK;
			Factory.Save();
			AssertEquals(Constants.CartageJobType.NEW_FCLExportPack, dect.CartageJobType);
			shp.JS_TransportMode = Constants.TransportModes.Road;
			QuotedBookingPickupCartageType qb = new QuotedBookingPickupCartageType(QuotedBooking.New(ZGuid.Empty, shp.PK, Factory));
			AssertEquals(Constants.CartageJobType.NEW_LCLExport, qb.CartageJobType);
		}

		public void TestEstimatedCartagePickup()
		{
			ForwardingShipment shp = QuotedBooking.CreateNewBooking(Factory);
			QuotedBookingPickupCartageType qb = new QuotedBookingPickupCartageType(QuotedBooking.New(ZGuid.Empty, shp.PK, Factory));
			ZDateTime now = ZDateTime.Now;
			shp.DocsAndCartage.JP_EstimatedPickup = now;
			AssertEquals(now, qb.EstimatedCartagePickup);
		}

		public void TestEstimatedCartageDelivery()
		{
			ForwardingShipment shp = QuotedBooking.CreateNewBooking(Factory);
			QuotedBookingPickupCartageType qb = new QuotedBookingPickupCartageType(QuotedBooking.New(ZGuid.Empty, shp.PK, Factory));
			ZDateTime now = ZDateTime.Now;
			shp.DocsAndCartage.JP_PickupRequiredBy = now;
			AssertEquals(now, qb.EstimatedCartageDelivery);
		}

		public void TestDropMode()
		{
			ForwardingShipment shp = QuotedBooking.CreateNewBooking(Factory);
			QuotedBookingPickupCartageType qb = new QuotedBookingPickupCartageType(QuotedBooking.New(ZGuid.Empty, shp.PK, Factory));
			shp.DocsAndCartage.JP_FCLPickupEquipmentNeeded = Constants.FCLEquipmentNeeded.SideLoader;
			AssertEquals(Constants.FCLEquipmentNeeded.SideLoader, qb.DropMode);
		}

		public void TestOriginScheduleDates()
		{
			var sailingHelper = new SailingsForTestClasses(Factory);
			sailingHelper.SetupFCLLCLDates(sailingHelper.SydLaxSailing);
			var shipment = QuotedBooking.CreateNewBooking(Factory);
			var qb = new QuotedBookingPickupCartageType(QuotedBooking.New(ZGuid.Empty, shipment.PK, Factory));
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_JX = sailingHelper.SydLaxSailing.PK;
			Assert(!qb.FCLReceivalCommences.IsEmpty);
			AssertEquals(sailingHelper.SydLaxSailing.JX_JA_CTOReceivalCommences, qb.FCLReceivalCommences);
			Assert(!qb.FCLCutOff.IsEmpty);
			AssertEquals(sailingHelper.SydLaxSailing.JX_JA_CTOCutOff, qb.FCLCutOff);
			Assert(!qb.LCLReceivalCommences.IsEmpty);
			AssertEquals(sailingHelper.SydLaxSailing.JX_DepotReceivalCommences, qb.LCLReceivalCommences);
			Assert(!qb.LCLCutOff.IsEmpty);
			AssertEquals(sailingHelper.SydLaxSailing.JX_DepotCutOff, qb.LCLCutOff);
		}

		public void TestGetCartageAddressReturnsShipmentCFSAddressForQuotesCTOAddress()
		{
			var shipment = QuotedBooking.CreateNewBooking(Factory);
			JobDocAddress shipmentCFSAddress = shipment.GetDepartureCFSDocAddress;
			var quote = new QuotedBookingPickupCartageType(QuotedBooking.New(ZGuid.Empty, shipment.PK, Factory));
			AssertNotNull("Pre-condition: shipment should have valid a CFS address", shipmentCFSAddress);
			AssertEquals(shipmentCFSAddress, quote.GetCartageAddress(LocalCartageJobOrgTypeList.Codes.CFS));
			AssertEquals(shipmentCFSAddress, quote.GetCartageAddress(LocalCartageJobOrgTypeList.Codes.CTO));
		}

		public void TestGetMatchingDirectionCodes()
		{
			var shipment = QuotedBooking.CreateNewBooking(Factory);
			var cartageType = new QuotedBookingPickupCartageType(QuotedBooking.New(ZGuid.Empty, shipment.PK, Factory));
			AssertArrayEqualsByElements("GetMatchingDirectionCodes() returned the wrong values", new ZString[] { "EXP", "ORG" }, cartageType.GetMatchingDirectionCodes().ToArray());
		}
	}
}
