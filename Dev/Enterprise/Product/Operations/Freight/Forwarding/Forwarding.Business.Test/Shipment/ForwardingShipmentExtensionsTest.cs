using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ForwardingShipmentExtensionsTest : TestCaseWithFactory
	{
		#region TestIsFHLShipment

		public void TestIsFHLShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			shipment.JS_TransportMode = Constants.TransportModes.Road;

			AssertEquals("STD ROA is not FHL shipment", false, shipment.IsFHLShipment());

			shipment.JS_TransportMode = Constants.TransportModes.Air;

			AssertEquals("STD AIR is FHL shipment", true, shipment.IsFHLShipment());

			shipment.JS_ShipmentType = Constants.ShipmentTypes.BlindCoLoadMaster;
			AssertEquals("CLB AIR is not FHL shipment", false, shipment.IsFHLShipment());

			shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			AssertEquals("CLD AIR with no master is FHL shipment", true, shipment.IsFHLShipment());

			var master = Factory.New<ForwardingShipment>();
			master.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;

			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			shipment.JS_JS_ColoadMasterShipment = master.PK;

			AssertEquals("STD AIR with CLD master is not FHL shipment", false, shipment.IsFHLShipment());
		}

		#endregion

		#region TestIsTakingPartInElectronicMessagingExchangeBetweenNVOCCAndForwarder

		public void TestIsTakingPartInElectronicMessagingExchangeBetweenNVOCCAndForwarder_NullShipment()
		{
			Assert("no exception on null shipment",
				!((ForwardingShipment)null).IsTakingPartInElectronicMessagingExchangeBetweenNVOCCAndForwarder());
		}

		public void TestIsTakingPartInElectronicMessagingExchangeBetweenNVOCCAndForwarder_TemplateShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			((ITemplateRecordProvider)shipment).IsTemplateRecord = true;

			var dbHits = Factory.DatabaseLoadCount;
			Assert("IsTakingPartInElectronicMessagingExchangeBetweenNVOCCAndForwarder for template shipment", !shipment.IsTakingPartInElectronicMessagingExchangeBetweenNVOCCAndForwarder());
			AssertEquals("no additional db hits for template shipment", dbHits, Factory.DatabaseLoadCount);
		}

		public void TestIsTakingPartInElectronicMessagingExchangeBetweenNVOCCAndForwarder_QuotedBookingLoadedFromTemplate()
		{
			var builder = ObjectFactory.New<IQuotedBookingBuilder>();

			var quotedBooking = builder.CreateNew(QuoteBookingType.BookingWithQuote, Factory);

			var templateRecord = Factory.New<StmTemplateRecord>();
			var templateRecordProvider1 = (ITemplateRecordProvider)quotedBooking;
			templateRecordProvider1.TemplateRecord = templateRecord;

			using (((IDbConnected)Factory).Connection.BeginTransactionWithManager())
			{
				templateRecordProvider1.SaveToTemplateRecord();
			}

			AssertEquals("Template Record is not in the database yet", false, templateRecord.IsInDatabase);

			Factory.Save();

			AssertEquals("Template Record is already in the database", true, templateRecord.IsInDatabase);

			var quotedBooking2 = builder.CreateNew(QuoteBookingType.BookingWithQuote, Factory);
			var templateRecordProvider2 = (ITemplateRecordProvider)quotedBooking2;
			templateRecordProvider2.LoadFromTemplateRecord(templateRecord);

			var dbHits = Factory.DatabaseLoadCount;
			Assert("IsTakingPartInElectronicMessagingExchangeBetweenNVOCCAndForwarder for template shipment", !((ForwardingShipment)quotedBooking2.ForwardingShipment).IsTakingPartInElectronicMessagingExchangeBetweenNVOCCAndForwarder());
			AssertEquals("no additional db hits for template shipment", dbHits, Factory.DatabaseLoadCount);
		}

		public void TestIsTakingPartInElectronicMessagingExchangeBetweenNVOCCAndForwarder()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;

			var dbHits = Factory.DatabaseLoadCount;
			Assert("IsTakingPartInElectronicMessagingExchangeBetweenNVOCCAndForwarder for unsaved shipment", !shipment.IsTakingPartInElectronicMessagingExchangeBetweenNVOCCAndForwarder());
			AssertEquals("no additional db hits for unsaved shipment", dbHits, Factory.DatabaseLoadCount);

			Factory.Save();

			dbHits = Factory.DatabaseLoadCount;
			Assert("IsTakingPartInElectronicMessagingExchangeBetweenNVOCCAndForwarder for saved SEA shipment", !shipment.IsTakingPartInElectronicMessagingExchangeBetweenNVOCCAndForwarder());
			AssertEquals("additional db hits for saved SEA shipment", dbHits + 1, Factory.DatabaseLoadCount);

			Factory.ClearQueryCache();

			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			dbHits = Factory.DatabaseLoadCount;
			Assert("IsTakingPartInElectronicMessagingExchangeBetweenNVOCCAndForwarder for saved non SEA shipment", !shipment.IsTakingPartInElectronicMessagingExchangeBetweenNVOCCAndForwarder());
			AssertEquals("no additional db hits for saved non SEA shipment", dbHits, Factory.DatabaseLoadCount);
		}

		public void TestIsTakingPartInElectronicMessagingExchangeBetweenNVOCCAndForwarder_ElectronicShippingInstruction() => AssertIsTakingPartInElectronicMessagingExchangeBetweenNVOCCAndForwarder_Log(
			true,
			new KeyValuePair<string, string>("NEW", ShipmentStatusList.Codes.ElectronicShippingInstruction));

		public void TestIsTakingPartInElectronicMessagingExchangeBetweenNVOCCAndForwarder_WebBooking() => AssertIsTakingPartInElectronicMessagingExchangeBetweenNVOCCAndForwarder_Log(
			true,
			new KeyValuePair<string, string>("NEW", ShipmentStatusList.Codes.WebBooking));

		public void TestIsTakingPartInElectronicMessagingExchangeBetweenNVOCCAndForwarder_ElectronicBooking() => AssertIsTakingPartInElectronicMessagingExchangeBetweenNVOCCAndForwarder_Log(
			true,
			new KeyValuePair<string, string>("NEW", ShipmentStatusList.Codes.ElectronicBooking));

		public void TestIsTakingPartInElectronicMessagingExchangeBetweenNVOCCAndForwarder_NonNVOCCLog() => AssertIsTakingPartInElectronicMessagingExchangeBetweenNVOCCAndForwarder_Log(
			false,
			new KeyValuePair<string, string>("ZZZ", "hello"));

		void AssertIsTakingPartInElectronicMessagingExchangeBetweenNVOCCAndForwarder_Log(bool expectedIsTakingPartInElectronicMessagingExchangeBetweenNVOCCAndForwarder, params KeyValuePair<string, string>[] stuLogParameters)
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			var log = shipment.Logs.AddNew(Events.StatusUpdated, stuLogParameters);
			Factory.Save();

			AssertEquals($"IsTakingPartInElectronicMessagingExchangeBetweenNVOCCAndForwarder for shipment having STU {log.SL_Reference}",
				expectedIsTakingPartInElectronicMessagingExchangeBetweenNVOCCAndForwarder,
				shipment.IsTakingPartInElectronicMessagingExchangeBetweenNVOCCAndForwarder());
		}

		#endregion

		public void TestChargesCacheKey_ReturnsCorrectString()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var cacheKey = shipment.GetChargesCacheKey();

			AssertEquals("ChargesCacheKey is correct", $"Charges_{shipment.PK}", cacheKey);
		}
	}
}
