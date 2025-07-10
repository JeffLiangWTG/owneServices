using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	sealed class ShipmentActionLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestManifestMessageActionCodes()
		{
			var shipment = Factory.New<Trip>().Shipments.AddNew();
			var lookups = new ShipmentAction(shipment, MessageTypes.Codes.eManifest).Lookups;
			AssertEquals("ActionCodes.Count when shipment is NOT lodged", 2, lookups.ActionCodes.Count);
			Assert(lookups.ActionCodes.ContainsCode(MessageActionCodes.Codes.Original));
			Assert(lookups.ActionCodes.ContainsCode(MessageActionCodes.Codes.DoNotSend));
			shipment.B0_ReleaseStatus = ShipmentEntryStatusList.Codes.Accepted;
			AssertEquals("ActionCodes.Count when shipment is lodged", 3, lookups.ActionCodes.Count);
			Assert(lookups.ActionCodes.ContainsCode(MessageActionCodes.Codes.Change));
			Assert(lookups.ActionCodes.ContainsCode(MessageActionCodes.Codes.Cancellation));
			Assert(lookups.ActionCodes.ContainsCode(MessageActionCodes.Codes.DoNotSend));
		}

		public void TestTripMessageActionCodes()
		{
			var shipment = Factory.New<Trip>().Shipments.AddNew();
			var lookups = new ShipmentAction(shipment, MessageTypes.Codes.CompleteTrip).Lookups;
			AssertEquals("ActionCodes.Count when shipment is NOT lodged", 1, lookups.ActionCodes.Count);
			Assert(lookups.ActionCodes.ContainsCode(MessageActionCodes.Codes.DoNotSend));
			shipment.B0_ReleaseStatus = ShipmentEntryStatusList.Codes.Accepted;
			AssertEquals("ActionCodes.Count when shipment is lodged but not linked", 2, lookups.ActionCodes.Count);
			Assert(lookups.ActionCodes.ContainsCode(MessageActionCodes.Codes.Link));
			Assert(lookups.ActionCodes.ContainsCode(MessageActionCodes.Codes.DoNotSend));
			shipment.B0_ReleaseStatus = MessageActionCodes.Codes.Link;
			AssertEquals("ActionCodes.Count when shipment is linked", 2, lookups.ActionCodes.Count);
			Assert(lookups.ActionCodes.ContainsCode(MessageActionCodes.Codes.DeLink));
			Assert(lookups.ActionCodes.ContainsCode(MessageActionCodes.Codes.DoNotSend));
			shipment.B0_ReleaseStatus = ZString.Empty;
			shipment.B0_ShipmentType = ShipmentTypes.Codes.SplitShipment;
			AssertEquals("ActionCodes.Count when shipment is split", 2, lookups.ActionCodes.Count);
			Assert(lookups.ActionCodes.ContainsCode(MessageActionCodes.Codes.Link));
			Assert(lookups.ActionCodes.ContainsCode(MessageActionCodes.Codes.DoNotSend));
		}

		public void TestAmendmentReasonCodes()
		{
			var shipment = Factory.New<Trip>().Shipments.AddNew();
			var lookups = new ShipmentAction(shipment, MessageTypes.Codes.eManifest).Lookups;
			var codes = ((CodeDescriptionPairList)lookups.AmendmentReasonCodes);
			AssertEquals("Shipment amendment codes, non-in-bond movement", "01, 02, 03, 04, 05, 06, 11, 18, 19, 27, 28, 29, 30", codes.CodesAsString);
			shipment.B0_ShipmentType = ShipmentTypes.Codes.Inbond;
			codes = ((CodeDescriptionPairList)lookups.AmendmentReasonCodes);
			AssertEquals("Shipment amendment codes, in-bond movement", "16, 17, 18, 19, 26, 27, 28, 29, 30", codes.CodesAsString);
		}
	}
}
