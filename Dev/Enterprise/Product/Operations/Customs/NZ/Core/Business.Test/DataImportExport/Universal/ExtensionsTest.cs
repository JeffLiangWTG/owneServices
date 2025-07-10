using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Core.Constants;
using EventReferenceParameters = CargoWise.EventReference.Constants.EventReferenceParameters;

namespace Enterprise.Customs.NZ.Business.Data.Universal.Testing
{
	class ExtensionsTest : TestCaseWithFactory
	{
		public void TestFindMatchingMAWBsFromHVLVShipment_UseTRFLogWithRefNumber_WhenMasterHouseBillMatched()
		{
			AssertFindMatchingMAWBsFromHVLVShipment_UseTRFLog_WhenMasterHouseBillMatched(EventReferenceParameters.Codes.ReferenceNumber);
		}

		public void TestFindMatchingMAWBsFromHVLVShipment_UseTRFLogWithJobNumber_WhenMasterHouseBillMatched()
		{
			AssertFindMatchingMAWBsFromHVLVShipment_UseTRFLog_WhenMasterHouseBillMatched(EventReferenceParameters.Codes.JobNumber);
		}

		void AssertFindMatchingMAWBsFromHVLVShipment_UseTRFLog_WhenMasterHouseBillMatched(string jobNumberEventReferenceCode)
		{
			var mawbBO = Factory.New<CusMAWB>();
			mawbBO.CM_MAWB = "MASTER BILL";
			mawbBO.CM_MasterHouseBill = "HB1";

			var universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.Master };
			universalShipment.WayBillNumber = "MASTER BILL";

			var hvlvUniversalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			hvlvUniversalShipment.WayBillType = new WayBillType() { Code = "HWB" };
			hvlvUniversalShipment.WayBillNumber = "HB1";
			hvlvUniversalShipment.DataContext = DataContextFactory.New();
			hvlvUniversalShipment.DataContext.AddDataSource(DataContextType.ForwardingShipment, "testShipment");

			var mawbs = new CusMAWB.Loader(Factory).FindMatchingMAWBs(universalShipment, hvlvUniversalShipment);
			Assert("can't find existing mawbBo when mawbBo doesn't has TRF log", !mawbs.Any());

			mawbBO.Logs.AddNew(AutoEvents.Transferred, new KeyValuePair<string, string>[]
			{
				new KeyValuePair<string, string>(EventReferenceParameters.Codes.Type, ShipmentTypes.HighVolumeLowValue),
				new KeyValuePair<string, string>(jobNumberEventReferenceCode, "testShipment")
			});

			mawbs = new CusMAWB.Loader(Factory).FindMatchingMAWBs(universalShipment, hvlvUniversalShipment);
			CombineAssertions("find existing mawbBo when mawbBo has TRF log and master house bill matched", () =>
			{
				AssertEquals(1, mawbs.Length);
				AssertEquals(mawbBO.PK, mawbs[0].PK);
			});

			mawbBO.CM_MasterHouseBill = "HB2";
			mawbs = new CusMAWB.Loader(Factory).FindMatchingMAWBs(universalShipment, hvlvUniversalShipment);
			Assert("can't find existing mawbBo when master house bill doesn't matched", !mawbs.Any());
		}

		public void TestFindMatchingMAWBsFromHVLVShipment_FallBackToOriginMatchingLogic_WhenUseTRFLogCannotFindExistingBizo()
		{
			var mawbBO = Factory.New<CusMAWB>();
			mawbBO.CM_MAWB = "123";
			mawbBO.CM_FlightNo = "QF123";
			mawbBO.CM_RL_NKLoadPort = "AUSYD";
			mawbBO.CM_DepartureDate = new ZDateTime(2014, 04, 15, 12, 30, 00);
			mawbBO.CM_RL_NKDischargePort = "NZAKL";
			mawbBO.CM_ArrivalDate = new ZDateTime(2014, 04, 17, 16, 12, 00);
			mawbBO.CM_MasterHouseBill = "HB1";
			mawbBO.CM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-1);

			var universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.WayBillNumber = "123";
			universalShipment.VoyageFlightNo = "QF123";
			universalShipment.PortOfLoading = new UNLOCO() { Code = "AUSYD" };
			universalShipment.PortOfDischarge = new UNLOCO() { Code = "NZAKL" };
			universalShipment.SetDateCollection(() => new List<Date>());
			universalShipment.DateCollection.Add(new Date() { Type = DateType.DischargeDate, Value = new ZDateTime(2014, 04, 17, 8, 10, 00) });

			var hvlvUniversalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			hvlvUniversalShipment.WayBillType = new WayBillType() { Code = "HWB" };
			hvlvUniversalShipment.WayBillNumber = "HB1";

			var mawbs = new CusMAWB.Loader(Factory).FindMatchingMAWBs(universalShipment, hvlvUniversalShipment);

			CombineAssertions("fall back to origin matching logic when use TRF log can't find existing bizo", () =>
			{
				AssertEquals(1, mawbs.Length);
				AssertEquals(mawbBO.PK, mawbs[0].PK);
			});
		}

		public void TestFindMatchingMAWBsFromHVLVShipment_WhenEnableConsolidateShipments()
		{
			using (HVLVDataRegistry.Instance.EnableHVLVMultiShipmentNZICRCRE.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var mawbBO = Factory.New<CusMAWB>();
				mawbBO.CM_MAWB = "MASTERBILL";
				mawbBO.CM_MasterHouseBill = "HB1";
				mawbBO.CM_SystemCreateTimeUtc = ZDateTime.Today;

				var universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
				universalShipment.WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.Master };
				universalShipment.WayBillNumber = "MASTERBILL";

				var hvlvUniversalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
				hvlvUniversalShipment.WayBillType = new WayBillType() { Code = "HWB" };
				hvlvUniversalShipment.WayBillNumber = "HB2";
				hvlvUniversalShipment.DataContext = DataContextFactory.New();
				hvlvUniversalShipment.DataContext.AddDataSource(DataContextType.ForwardingShipment, "testShipment2");

				var mawbs = new CusMAWB.Loader(Factory).FindMatchingMAWBs(universalShipment, hvlvUniversalShipment);
				Assert("can't find existing mawbBo when mawbBo doesn't have TRF log", !mawbs.Any());

				mawbBO.Logs.AddNew(AutoEvents.Transferred, new KeyValuePair<string, string>[]
				{
					new KeyValuePair<string, string>(EventReferenceParameters.Codes.Type, ShipmentTypes.HighVolumeLowValue),
					new KeyValuePair<string, string>(EventReferenceParameters.Codes.ReferenceNumber, "testShipment1")
				});
				Factory.Save();

				mawbs = new CusMAWB.Loader(Factory).FindMatchingMAWBs(universalShipment, hvlvUniversalShipment);
				CombineAssertions("find existing mawbBo when mawbBo has TRF log and ocean bill matched", () =>
				{
					AssertEquals(1, mawbs.Length);
					AssertEquals(mawbBO.PK, mawbs[0].PK);
				});

				var previousMAWBBO = Factory.New<CusMAWB>();
				previousMAWBBO.CM_MAWB = "MASTERBILL";
				previousMAWBBO.CM_MasterHouseBill = "HB0";
				previousMAWBBO.CM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-1);
				previousMAWBBO.Logs.AddNew(AutoEvents.Transferred, new KeyValuePair<string, string>[]
				{
					new KeyValuePair<string, string>(EventReferenceParameters.Codes.Type, ShipmentTypes.HighVolumeLowValue),
					new KeyValuePair<string, string>(EventReferenceParameters.Codes.ReferenceNumber, "testShipment0")
				});
				Factory.Save();

				mawbs = new CusMAWB.Loader(Factory).FindMatchingMAWBs(universalShipment, hvlvUniversalShipment);
				CombineAssertions("Should load all related MAWBs and order by created time desc", () =>
				{
					AssertEquals("Found all related MAWBs", 2, mawbs.Length);
					AssertEquals("Order by created time desc", mawbBO.PK, mawbs[0].PK);
				});
			}
		}

		public void TestGetDepartureFlightAndArrivalFlight()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.PortOfLoading = new UNLOCO() { Code = "NZAKL" };
			shipment.PortOfDischarge = new UNLOCO() { Code = "AUSYD" };

			shipment.SetTransportLegCollection(() => new DataObjectList<TransportLeg>());
			var leg1 = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance) { PortOfLoading = new UNLOCO() { Code = "NZAKL" }, PortOfDischarge = new UNLOCO() { Code = "USLAX" }, LegOrder = 1 };
			var leg2 = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance) { PortOfLoading = new UNLOCO() { Code = "USLAX" }, PortOfDischarge = new UNLOCO() { Code = "AUSYD" }, LegOrder = 2 };
			shipment.TransportLegCollection.Add(leg1);
			shipment.TransportLegCollection.Add(leg2);

			AssertEquals(leg1, shipment.GetDepartureTransportLeg());
			AssertEquals(leg2, shipment.GetArrivalTransportLeg());
		}

		public void TestFindMatchingMAWBsFromShipment()
		{
			var importMawb = Factory.New<CusMAWB>();
			importMawb.CM_MAWB = "123";
			importMawb.CM_FlightNo = "QF123";
			importMawb.CM_RL_NKLoadPort = "AUSYD";
			importMawb.CM_DepartureDate = new ZDateTime(2014, 04, 15, 12, 30, 00);
			importMawb.CM_RL_NKDischargePort = "NZAKL";
			importMawb.CM_ArrivalDate = new ZDateTime(2014, 04, 17, 16, 12, 00);
			importMawb.CM_MasterHouseBill = "HB1";
			importMawb.CM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-1);

			var exportMawb = Factory.New<CusMAWB>();
			exportMawb.CM_MAWB = "123";
			exportMawb.CM_FlightNo = "QF123";
			exportMawb.CM_RL_NKLoadPort = "NZAKL";
			exportMawb.CM_DepartureDate = new ZDateTime(2014, 04, 16, 12, 30, 00);
			exportMawb.CM_RL_NKDischargePort = "AUSYD";
			exportMawb.CM_ArrivalDate = new ZDateTime(2014, 04, 18, 16, 12, 00);
			exportMawb.CM_MasterHouseBill = "HB2";
			exportMawb.CM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-1);

			var inactiveExportMawb = Factory.New<CusMAWB>();
			inactiveExportMawb.CM_MAWB = "123";
			inactiveExportMawb.CM_FlightNo = "QF123";
			inactiveExportMawb.CM_RL_NKLoadPort = "NZAKL";
			inactiveExportMawb.CM_DepartureDate = new ZDateTime(2014, 04, 16, 12, 30, 00);
			inactiveExportMawb.CM_RL_NKDischargePort = "AUSYD";
			inactiveExportMawb.CM_ArrivalDate = new ZDateTime(2014, 04, 18, 16, 12, 00);
			inactiveExportMawb.CM_MasterHouseBill = "HB2";
			inactiveExportMawb.CM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-1);
			inactiveExportMawb.CM_IsActive = false;

			var dataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.WayBillNumber = "123";
			dataObject.VoyageFlightNo = "QF123";
			dataObject.PortOfLoading = new UNLOCO() { Code = "AUSYD" };
			dataObject.PortOfDischarge = new UNLOCO() { Code = "NZAKL" };
			dataObject.SetDateCollection(() => new List<Date>());
			dataObject.DateCollection.Add(new Date() { Type = DateType.DischargeDate, Value = new ZDateTime(2014, 04, 17, 8, 10, 00) });
			dataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>());
			dataObject.AdditionalBillCollection.Add(new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillType = new WayBillType() { Code = WayBillTypeList.Codes.MasterHouse }, ParentBillNumber = "123", BillNumber = "HB1" });

			var mawbs = new CusMAWB.Loader(Factory).FindMatchingMAWBs(dataObject, null);
			AssertEquals(1, mawbs.Length);
			AssertEquals(importMawb.PK, mawbs[0].PK);

			dataObject.PortOfLoading = new UNLOCO() { Code = "NZAKL" };
			dataObject.PortOfDischarge = new UNLOCO() { Code = "AUSYD" };
			dataObject.SetDateCollection(() => new List<Date>());
			dataObject.DateCollection.Add(new Date() { Type = DateType.LoadingDate, Value = new ZDateTime(2014, 04, 16, 2, 30, 00) });
			dataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>());
			dataObject.AdditionalBillCollection.Add(new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillType = new WayBillType() { Code = WayBillTypeList.Codes.MasterHouse }, ParentBillNumber = "123", BillNumber = "HB2" });

			mawbs = new CusMAWB.Loader(Factory).FindMatchingMAWBs(dataObject, null);
			AssertEquals(1, mawbs.Length);
			AssertEquals(exportMawb.PK, mawbs[0].PK);
		}

		public void TestInvalidMAWBRecyclePeriodWillMatchAll()
		{
			FreightDataRegistry.Instance.MAWBRecyclePeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
			var importMawb = Factory.New<CusMAWB>();
			importMawb.CM_MAWB = "123";
			importMawb.CM_FlightNo = "QF123";
			importMawb.CM_RL_NKLoadPort = "AUSYD";
			importMawb.CM_DepartureDate = new ZDateTime(2014, 04, 15, 12, 30, 00);
			importMawb.CM_RL_NKDischargePort = "NZAKL";
			importMawb.CM_ArrivalDate = new ZDateTime(2014, 04, 17, 16, 12, 00);
			importMawb.CM_MasterHouseBill = "HB1";
			importMawb.CM_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);

			var dataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.WayBillNumber = "123";
			dataObject.VoyageFlightNo = "QF123";
			dataObject.PortOfLoading = new UNLOCO() { Code = "AUSYD" };
			dataObject.PortOfDischarge = new UNLOCO() { Code = "NZAKL" };
			dataObject.SetDateCollection(() => new List<Date>());
			dataObject.DateCollection.Add(new Date() { Type = DateType.DischargeDate, Value = new ZDateTime(2014, 04, 17, 8, 10, 00) });
			dataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>());
			dataObject.AdditionalBillCollection.Add(new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillType = new WayBillType() { Code = WayBillTypeList.Codes.MasterHouse }, ParentBillNumber = "123", BillNumber = "HB1" });

			var mawbs = new CusMAWB.Loader(Factory).FindMatchingMAWBs(dataObject, null);
			AssertEquals(1, mawbs.Length);
			AssertEquals(importMawb.PK, mawbs[0].PK);
		}

		public void TestFindMatchingSCAOceanBills()
		{
			var oceanBill1 = Factory.New<CusSCAOceanBill>();
			oceanBill1.CB_OceanBill = "123";
			oceanBill1.CB_LloydsIMO = "aaa";
			oceanBill1.CB_Voyage = "QF123";
			oceanBill1.CB_MasterHouseBill = "HB1";
			oceanBill1.CB_IsActive = true;

			var oceanBill2 = Factory.New<CusSCAOceanBill>();
			oceanBill2.CB_OceanBill = "456";
			oceanBill2.CB_LloydsIMO = "aaa";
			oceanBill2.CB_Voyage = "QF123";
			oceanBill2.CB_MasterHouseBill = "HB1";
			oceanBill2.CB_IsActive = true;

			var inactiveBill1 = Factory.New<CusSCAOceanBill>();
			inactiveBill1.CB_OceanBill = "123";
			inactiveBill1.CB_LloydsIMO = "aaa";
			inactiveBill1.CB_Voyage = "QF123";
			inactiveBill1.CB_MasterHouseBill = "HB1";
			inactiveBill1.CB_IsActive = false;

			var dataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.WayBillNumber = "123";
			dataObject.VoyageFlightNo = "QF123";
			dataObject.LloydsIMO = "aaa";
			dataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>());
			dataObject.AdditionalBillCollection.Add(new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillType = new WayBillType() { Code = WayBillTypeList.Codes.MasterHouse }, ParentBillNumber = "123", BillNumber = "HB1" });

			var bills = new CusSCAOceanBill.Loader(Factory).FindMatchingSCAOceanBills(dataObject, null);
			AssertEquals(1, bills.Length);
			AssertEquals(oceanBill1.PK, bills[0].PK);
		}

		public void TestFindMatchingSCAOceanBills_WhenIsInHVLVAndRegistryEnabled()
		{
			using (HVLVDataRegistry.Instance.EnableHVLVMultiShipmentNZICRCRE.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var oceanBill1 = Factory.New<CusSCAOceanBill>();
				oceanBill1.CB_OceanBill = "123";
				oceanBill1.CB_LloydsIMO = "aaa";
				oceanBill1.CB_Voyage = "QF123";
				oceanBill1.CB_MasterHouseBill = "HB1";
				oceanBill1.CB_IsActive = true;

				var dataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
				dataObject.DataContext = DataContextFactory.New();
				dataObject.DataContext.AddDataSource(DataContextType.ForwardingConsol, string.Empty);
				dataObject.DataContext.AddDataSource(DataContextType.ForwardingShipment, string.Empty);
				dataObject.WayBillNumber = "123";
				dataObject.VoyageFlightNo = "QF123";
				dataObject.LloydsIMO = "aaa";

				var subShipmentDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
				subShipmentDataObject.ShipmentType = new CodeDescriptionPair { Code = "HVL" };

				dataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
				dataObject.SubShipmentCollection.Add(subShipmentDataObject);

				var bills = new CusSCAOceanBill.Loader(Factory).FindMatchingSCAOceanBills(dataObject, subShipmentDataObject);
				AssertEquals(1, bills.Length);
				AssertEquals(oceanBill1.PK, bills[0].PK);
			}
		}

		public void TestGetLowestLevelShipment()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { WayBillNumber = "1" };
			shipment.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			shipment.SubShipmentCollection.Add(new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { WayBillNumber = "2" });
			var subShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { WayBillNumber = "3" };
			shipment.SubShipmentCollection.Add(subShipment);
			subShipment.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			subShipment.SubShipmentCollection.Add(new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { WayBillNumber = "4" });
			var lowestShipments = shipment.GetLowestLevelShipment().ToList();
			AssertEquals(2, lowestShipments.Count);
			Assert(lowestShipments.Any(x => x.WayBillNumber.GetValueOrDefault() == "2"));
			Assert(lowestShipments.Any(x => x.WayBillNumber.GetValueOrDefault() == "4"));
		}
	}
}
