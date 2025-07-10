using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	internal class BaseCusSCAHouseTest : TestCaseWithFactory
	{
		public void TestDeleteAnyNewMessages_DoNotLoadMessagesDuringDelete()
		{
			var houseBill = Factory.New<TestCusSCAHouse>();
			var message = houseBill.Messages.AddNew();
			message.EM_ReceiveTransmit = "RCV";
			Factory.Save();
			var expected = new Dictionary<string, int>
			{
				{ EDIMessage.Schema.TableName, 1 }
			};
			AssertEquals(1, houseBill.Messages.Count);
			houseBill.Messages.Reload(true);
			AssertDbHits(expected, Factory, true);

			var newFactory = new BusinessObjectFactory();
			var newHouse = newFactory.Load<TestCusSCAHouse>(houseBill.PK);
			newHouse.DeleteAnyNewMessages();
			expected[EDIMessage.Schema.TableName] = 0;
			AssertDbHits(expected, newFactory, true);
		}

		public void TestPublishCusSCAHouseStatusChangedUniversalEvent_NoHVLV()
		{
			var oceanBill = Factory.NewWithValidTestData<TestCusSCAOceanBill>();
			var houseBill = Factory.New<TestCusSCAHouse>();
			houseBill.CA_CB = oceanBill.PK;
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			houseBill.CA_JS = shipment.PK;
			houseBill.CA_IsHVLV = false;
			houseBill.CA_ShipmentStatus = "HLD";
			houseBill.Factory.Save();

			houseBill.CA_ShipmentStatus = "CLR";
			houseBill.Factory.Save();

			var query = new ZQuery(StmALogSchema.SL_Parent, houseBill.PK);
			query.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, "|LOC");
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.CustomsEntryStatus.Code);
			var logs = Factory.Load<StmALog>(query);
			AssertEquals("0 log published", 0, logs.Length);
		}

		public void TestPublishCusSCAHouseStatusChangedUniversalEvent_NoNewCustomsStatus()
		{
			var oceanBill = Factory.NewWithValidTestData<TestCusSCAOceanBill>();
			var houseBill = Factory.New<TestCusSCAHouse>();
			houseBill.CA_CB = oceanBill.PK;
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			houseBill.CA_JS = shipment.PK;
			houseBill.CA_IsHVLV = true;
			houseBill.CA_ShipmentStatus = "HLD";
			houseBill.Factory.Save();

			houseBill.CA_ShipmentStatus = "HLD";
			houseBill.Factory.Save();

			var query = new ZQuery(StmALogSchema.SL_Parent, houseBill.PK);
			query.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, "|LOC");
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.CustomsEntryStatus.Code);
			var logs = Factory.Load<StmALog>(query);
			AssertEquals("0 log published", 0, logs.Length);
		}

		public void TestPublishCusSCAHouseStatusChangedUniversalEvent_Published()
		{
			var oceanBill = Factory.NewWithValidTestData<TestCusSCAOceanBill>();
			var houseBill = Factory.New<TestCusSCAHouse>();
			houseBill.CA_CB = oceanBill.PK;
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			houseBill.CA_JS = shipment.PK;
			houseBill.CA_IsHVLV = true;
			houseBill.CA_ShipmentStatus = "HLD";
			houseBill.CA_RL_NK_PortOfDestination = "AUSYD";
			houseBill.Factory.Save();

			houseBill.CA_ShipmentStatus = "CLR";
			houseBill.Factory.Save();

			var query = new ZQuery(StmALogSchema.SL_Parent, houseBill.PK);
			query.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, "|LOC");
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.CustomsEntryStatus.Code);
			var logs = Factory.Load<StmALog>(query);
			AssertEquals("One universal event should be created", 1, logs.Length);
			AssertEquals("Free text should be properly set", "|LOC=AUSYD|RES=REASON FOR TEST|SER=PCS|TYP=CLR", logs[0].SL_Reference);
		}

		public void TestUniversalDataContext()
		{
			var cusSCAHouse = Factory.New<TestCusSCAHouse>();
			AssertEquals(DataContextType.SeaHouseBill, cusSCAHouse.GetUniversalDataContextManager().DataContextType);
		}

		public void TestDeleteAnyNewMessages()
		{
			TestCusSCAHouse cusSCAHouse = Factory.New<TestCusSCAHouse>();
			EDIMessage message1 = cusSCAHouse.Messages.AddNew();
			message1.EM_ReceiveTransmit = "RCV";
			Factory.Save();
			EDIMessage message2 = cusSCAHouse.Messages.AddNew();
			message2.EM_ReceiveTransmit = "RCV";
			AssertEquals(2, cusSCAHouse.Messages.Count);
			Assert(!message1.IsDeleted);
			Assert(!message2.IsDeleted);
			cusSCAHouse.DeleteAnyNewMessages();
			AssertEquals(1, cusSCAHouse.Messages.Count);
			Assert(!message1.IsDeleted);
			Assert(message2.IsDeleted);
		}

		public void TestCusSCAHouseTypeDecider()
		{
			TestCusSCAOceanBill cusSCAOceanBill = Factory.New<TestCusSCAOceanBill>();
			TestCusSCAHouse cusSCAHouse = Factory.New<TestCusSCAHouse>();
			cusSCAHouse.CA_CB = cusSCAOceanBill.PK;

			Factory.Save();

			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			var cusSCAHouseInFactory1 = factory1.Load<BaseCusSCAHouse>(cusSCAHouse.PK);
			AssertEquals("Correct type", cusSCAHouseInFactory1.GetType(), typeof(TestCusSCAHouse));

			cusSCAOceanBill.CB_ApplicationCode = Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACIAir;
			Factory.Save();
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			var cusSCAHouseInFactory2 = factory2.Load<BaseCusSCAHouse>(cusSCAHouse.PK);
			AssertEquals("Correct type", cusSCAHouseInFactory2.GetType(), ObjectFactory.GetType<Integration.Customs.CA.ICusSCAHouse>());

			cusSCAOceanBill.CB_ApplicationCode = Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACISea;
			Factory.Save();
			BusinessObjectFactory factory3 = new BusinessObjectFactory();
			var cusSCAHouseInFactory3 = factory3.Load<BaseCusSCAHouse>(cusSCAHouse.PK);
			AssertEquals("Correct type", cusSCAHouseInFactory3.GetType(), ObjectFactory.GetType<Integration.Customs.CA.ICusSCAHouse>());

			cusSCAOceanBill.CB_ApplicationCode = Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACIRoad;
			Factory.Save();
			BusinessObjectFactory factory4 = new BusinessObjectFactory();
			var cusSCAHouseInFactory4 = factory4.Load<BaseCusSCAHouse>(cusSCAHouse.PK);
			AssertEquals("Correct type", cusSCAHouseInFactory4.GetType(), ObjectFactory.GetType<Integration.Customs.CA.ICusSCAHouse>());

			cusSCAOceanBill.CB_ApplicationCode = Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACIRail;
			Factory.Save();
			BusinessObjectFactory factory5 = new BusinessObjectFactory();
			var cusSCAHouseInFactory5 = factory5.Load<BaseCusSCAHouse>(cusSCAHouse.PK);
			AssertEquals("Correct type", cusSCAHouseInFactory5.GetType(), ObjectFactory.GetType<Integration.Customs.CA.ICusSCAHouse>());

			cusSCAOceanBill.CB_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			Factory.Save();
			BusinessObjectFactory factory6 = new BusinessObjectFactory();
			var cusSCAHouseInFactory6 = factory6.Load<BaseCusSCAHouse>(cusSCAHouse.PK);
			AssertEquals("Correct type", cusSCAHouseInFactory6.GetType(), ObjectFactory.GetType<Integration.Customs.AU.ICusSCAHouse>());

			cusSCAOceanBill.CB_ApplicationCode = Core.Constants.Customs.CusSCAOceanBillApplicationCodes.AustraliaLegacy;
			Factory.Save();
			BusinessObjectFactory factory7 = new BusinessObjectFactory();
			var cusSCAHouseInFactory7 = factory7.Load<BaseCusSCAHouse>(cusSCAHouse.PK);
			AssertEquals("Default type", cusSCAHouseInFactory7.GetType(), typeof(DefaultCusSCAHouse));

			cusSCAOceanBill.CB_ApplicationCode = "";
			Factory.Save();
			BusinessObjectFactory factory8 = new BusinessObjectFactory();
			var cusSCAHouseInFactory8 = factory8.Load<BaseCusSCAHouse>(cusSCAHouse.PK);
			AssertEquals("Default type", cusSCAHouseInFactory8.GetType(), typeof(DefaultCusSCAHouse));

			cusSCAOceanBill.CB_ApplicationCode = Core.Constants.Customs.ExpressApplicationCodes.NZ.TSWWriteOff;
			Factory.Save();
			BusinessObjectFactory factory9 = new BusinessObjectFactory();
			var cusSCAHouseInFactory9 = factory9.Load<BaseCusSCAHouse>(cusSCAHouse.PK);
			var typeGot = cusSCAHouseInFactory9.GetType();
			AssertEquals("Correct type - should be NZ CusSCAHouse object", cusSCAHouseInFactory9.GetType(), ObjectFactory.GetType<Integration.Customs.NZ.ICusSCAHouse>());
		}
	}
}
