using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.SG.Registry;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.AWB.Messaging;
using Enterprise.Freight.Forwarding.AWB.Messaging.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.CMDMessaging.Testing
{
	sealed class CMDTest : CargoIMPTest
	{
		[ExpectNoExceptions()]
		public void TestValidator()
		{
			CMD cMDMessage;
			try
			{
				cMDMessage = new CMD(null, null);
				Fail("Exception should be thrown");
			}
			catch (OdysseyException e)
			{
				AssertEquals("Consol cannot be null or marked as deleted", e.Message);
			}

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			try
			{
				cMDMessage = new CMD(consol, null);
				Fail("Exception should be thrown");
			}
			catch (OdysseyException e)
			{
				AssertEquals("CMDData cannot be null", e.Message);
			}

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			cMDMessage = new CMD(consol, new CMDData(new CMDShipmentWrapper(shipment)));
			consol.Delete();
			try
			{
				cMDMessage = new CMD(consol, null);
				Fail("Exception should be thrown");
			}
			catch (OdysseyException e)
			{
				AssertEquals("Consol cannot be null or marked as deleted", e.Message);
			}
		}

		public void TestIsLate()
		{
			Transport transport = Consol.Transports[0];
			transport.JW_ATD = ZDateTime.Empty;
			Dummy.fNow = new ZDateTime(2005, 1, 10);
			AssertEquals(true, Dummy.ExposedIsLate);
			fCargoIMPMessage = null;
			Dummy.fNow = new ZDateTime(2005, 1, 10);
			transport.JW_ATD = new ZDateTime(2005, 1, 4);
			AssertEquals(false, Dummy.ExposedIsLate);
			fCargoIMPMessage = null;
			Dummy.fNow = new ZDateTime(2005, 1, 10);
			transport.JW_ATD = new ZDateTime(2005, 1, 2);
			AssertEquals(true, Dummy.ExposedIsLate);
			fCargoIMPMessage = null;
			Dummy.fNow = new ZDateTime(2005, 1, 10);
			transport.JW_ATD = ZDateTime.Empty;
			transport.JW_ETD = new ZDateTime(2005, 1, 3);
			AssertEquals(false, Dummy.ExposedIsLate);
			fCargoIMPMessage = null;
			Dummy.TestIsExport = false;
			Dummy.fNow = new ZDateTime(2005, 1, 10);
			transport.JW_ATA = new ZDateTime(2005, 1, 3);
			AssertEquals(false, Dummy.ExposedIsLate);
			fCargoIMPMessage = null;
			Dummy.TestIsExport = false;
			Dummy.fNow = new ZDateTime(2005, 1, 10);
			transport.JW_ATA = new ZDateTime(2004, 12, 30);
			AssertEquals(true, Dummy.ExposedIsLate);
			fCargoIMPMessage = null;
			Dummy.TestIsExport = false;
			Dummy.fNow = new ZDateTime(2005, 1, 10);
			transport.JW_ATA = ZDateTime.Empty;
			AssertEquals(true, Dummy.ExposedIsLate);
			fCargoIMPMessage = null;
			Dummy.TestIsExport = false;
			Dummy.fNow = new ZDateTime(2005, 1, 10);
			transport.JW_ATA = ZDateTime.Empty;
			transport.JW_ETA = new ZDateTime(2005, 1, 1);
			AssertEquals(false, Dummy.ExposedIsLate);
		}

		public void TestGetTotalPackageCount()
		{
			AssertEquals(0, Dummy.BaseGetTotalPackageCount(Consol));
			CommonShipment shipment1 = Consol.Shipments.AddNew();
			shipment1.JS_TotalPackageCount = 20;
			CommonShipment shipment2 = Consol.Shipments.AddNew();
			shipment2.JS_OuterPacks = 18;
			CommonShipment shipment3 = Consol.Shipments.AddNew();
			shipment3.JS_OuterPacks = 2;
			Consol.Shipments.AddNew();
			CommonShipment shipment4 = Consol.Shipments.AddNew();
			shipment4.JS_TotalPackageCount = 0;
			AssertEquals(40, Dummy.BaseGetTotalPackageCount(Consol));
			CommonShipment shipment5 = Consol.Shipments.AddNew();
			shipment5.JS_TotalPackageCount = 2;
			AssertEquals(42, Dummy.BaseGetTotalPackageCount(Consol));
		}

		public void TestToString()
		{
			AssertMessageCorrect();
		}

		public void TestToStringMessageID()
		{
			Dummy.fNow = new ZDateTime(2005, 1, 8, 23, 59, 59);
			Transport transport = Consol.Transports[0];
			transport.JW_ATD = new ZDateTime(2005, 1, 1);
			SetMasterBillNum();
			messageID = "A/N/N\r\n";
			AssertMessageCorrect();
			fCargoIMPMessage = null;
			messageID = "A/N/Y\r\n";
			Dummy.fNow = new ZDateTime(2005, 1, 9);
			AssertMessageCorrect();
			fCargoIMPMessage = null;
			transport.JW_ATA = ZDateTime.Empty;
			transport.JW_ETA = new ZDateTime(2005, 1, 1);
			messageID = "A/N/N\r\n";
			iMW = "";
			Dummy.TestIsExport = false;
			Dummy.fNow = new ZDateTime(2005, 1, 11, 23, 59, 59);
			AssertMessageCorrect();
			fCargoIMPMessage = null;
			messageID = "A/N/Y\r\n";
			Dummy.TestIsExport = false;
			Dummy.fNow = new ZDateTime(2005, 1, 12);
			AssertMessageCorrect();
		}

		public void TestToStringDirectConsol()
		{
			Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AddConsignee(Consol);
			mWB = "MWB/088-12345101JKTJTY/T77K234.99\r\n/LUXURY CAR A/HAR001\r\n/ELEPHANT WITH TRUNKS/HAR005\r\n/BIG ELEPHANTS/HAR110\r\n/BASTARD ELEPHANTS/HAR111\r\n";
			hWB = "";
			AssertMessageCorrect();
		}

		public void TestToStringMAWBShipperAndConsignee()
		{
			Consol.SetDefaultSendingForwarderAddress(ZGuid.Empty);
			sHP = "";
			AssertMessageCorrect();
			fCargoIMPMessage = null;
			Consol.SetDefaultReceivingForwarderAddress(ZGuid.Empty);
			cNE = "";
			AssertMessageCorrect();
		}

		public void TestToStringIMW()
		{
			Dummy.TestIsExport = false;
			iMW = "";
			AssertMessageCorrect();
			fCargoIMPMessage = null;
			Dummy.TestIsExport = true;
			iMW = "IMW/695-44813366\r\n";
			AssertMessageCorrect();
			fCargoIMPMessage = null;
			CMDData.TestImportConsol = null;
			iMW = "";
			AssertMessageCorrect();
		}

		public void TestToStringTPT()
		{
			CMDData.TestTDBPermitNos = Array.Empty<ZString>();
			tPT = "TPT/\r\n";
			AssertMessageCorrect();
			fCargoIMPMessage = null;
			CMDData.TestTDBPermitNos = new ZString[] { "TPT1", "TPT2" };
			tPT = "TPT/TDB/TPT1       \r\n/TPT2       \r\n";
			AssertMessageCorrect();
		}

		public void TestToStringTPTWithOldAndNewLengthPermits()
		{
			CMDData.TestTDBPermitNos = Array.Empty<ZString>();
			tPT = "TPT/\r\n";
			AssertMessageCorrect();
			fCargoIMPMessage = null;
			CMDData.TestTDBPermitNos = new ZString[] { "IG3J700003", "IG7H029293W" }; // one old 10 char, 1 new 11 char
			tPT = "TPT/TDB/IG3J700003 \r\n/IG7H029293W\r\n";
			AssertMessageCorrect();
		}

		public void TestToStringEXP()
		{
			CMDData.TestExemptionRemarks = "";
			eXP = "EXP/CD\r\n";
			AssertMessageCorrect();
			fCargoIMPMessage = null;
			CMDData.TestExemptionCode = "ZZ";
			eXP = "EXP/ZZ/\r\n";
			AssertMessageCorrect();
			fCargoIMPMessage = null;
			CMDData.TestExemptionRemarks = "testing 123";
			eXP = "EXP/ZZ/TESTING 123\r\n";
			AssertMessageCorrect();
		}

		public void TestToStringDUI()
		{
			Dummy.TestIsImport = true;
			dUI = "DUI/N\r\n";
			AssertMessageCorrect();
			fCargoIMPMessage = null;
			Dummy.TestIsImport = true;
			CMDData.TestGoodsDelivered = ZBool.True;
			dUI = "DUI/Y\r\n";
			AssertMessageCorrect();
			fCargoIMPMessage = null;
			Dummy.TestIsImport = false;
			CMDData.TestGoodsDelivered = ZBool.True;
			dUI = "";
			AssertMessageCorrect();
		}

		public void TestToStringFLT()
		{
			Transport departureFlight = Consol.Transports[0];
			departureFlight.JW_VoyageFlight = "QF01";
			SetMasterBillNum();
			fLT = "FLT/QF001/01JAN\r\n";
			AssertMessageCorrect();
			fCargoIMPMessage = null;
			departureFlight.JW_VoyageFlight = "GA3";
			SetMasterBillNum();
			fLT = "FLT/GA003/01JAN\r\n";
			AssertMessageCorrect();
			fCargoIMPMessage = null;
			departureFlight.JW_VoyageFlight = "GA";
			SetMasterBillNum();
			fLT = "FLT/GA/01JAN\r\n";
			AssertMessageCorrect();
			fCargoIMPMessage = null;
			departureFlight.JW_VoyageFlight = "GA2938";
			SetMasterBillNum();
			fLT = "FLT/GA2938/01JAN\r\n";
			AssertMessageCorrect();
			fCargoIMPMessage = null;
			departureFlight.JW_VoyageFlight = "SQ889912";
			SetMasterBillNum();
			fLT = "FLT/SQ88991/01JAN\r\n";
			AssertMessageCorrect();
		}

		public void TestMAWBSegmentWhenRoadConsolMAWBIncludesHyphen()
		{
			Dummy.TestIsExport = false;
			iMW = "";
			AssertMessageCorrect();
			fCargoIMPMessage = null;
			Consol.JK_TransportMode = Core.Constants.TransportModes.Road;
			Consol.JK_MasterBillNum = "695-44813366";
			Dummy.TestIsExport = true;
			mWB = "MWB/695-44813366JKTJTY/T77K234.99\r\n/AS PER MANIFEST\r\n";
			fLT = "FLT//\r\n";
			sHP = "";
			cNE = "";
			iMW = "IMW/695-44813366\r\n";
			AssertMessageCorrect();
			fCargoIMPMessage = null;
			CMDData.TestImportConsol = null;
			iMW = "";
			AssertMessageCorrect();
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			orgCompanyName = GlbCompany.CurrentCompany.GC_Name;
			orgStaffName = GlbStaff.CurrentUser.GS_FullName;
			orgHomePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			GlbCompany.CurrentCompany.GC_Name = "Test Company 123";
			GlbStaff.CurrentUser.GS_FullName = "Test User";
			ZQuery filter = new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.Equal, "IDJKT");
			var uNLOCO = Factory.LoadTop1<RefUNLOCO>(filter);
			uNLOCO.RL_HasAirport = true;
			uNLOCO.RL_IATA = "JKT";
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = uNLOCO.Code;
			SGCustomsDataRegistry.Instance.CargoAgentCode.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, "CX81");
			SGCustomsDataRegistry.Instance.CargoAgentRef.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, "TEST001");
			GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "012345678901");
		}

		protected override void TearDown()
		{
			base.TearDown();
			GlbCompany.CurrentCompany.GC_Name = orgCompanyName;
			GlbStaff.CurrentUser.GS_FullName = orgStaffName;
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = orgHomePort;
		}

		DummyCMD fCargoIMPMessage;
		protected override CargoIMP CargoIMPMessage
		{
			get
			{
				if (fCargoIMPMessage == null)
				{
					fCargoIMPMessage = new DummyCMD(Consol, CMDData);
					fCargoIMPMessage.fNow = new ZDateTime(2005, 1, 5);
					fCargoIMPMessage.TestIsExport = true;
					fCargoIMPMessage.MAWBTotalPackageCount = 77;
					fCargoIMPMessage.MAWBTotalWeight = 234.99M;
					fCargoIMPMessage.MAWBWeightUnit = "K";
				}

				return fCargoIMPMessage;
			}
		}

		DummyCMD Dummy
		{
			get
			{
				return (DummyCMD)CargoIMPMessage;
			}
		}

		protected override string Expected
		{
			get
			{
				return string.Concat("CMD/2\r\n", messageID, mWB, fLT, hWB, sHP, cNE, tPT, iMW, eXP, dUI, sND);
			}
		}

		ForwardingConsol fConsol;
		ForwardingConsol Consol
		{
			get
			{
				if (fConsol == null)
				{
					fConsol = CreateConsolForTest();
				}

				return fConsol;
			}
		}

		DummyCMDData fCMDData;
		DummyCMDData CMDData
		{
			get
			{
				if (fCMDData == null)
				{
					fCMDData = CreateCMDDataForTest();
				}

				return fCMDData;
			}
		}

		ForwardingConsol CreateConsolForTest()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_IsNeutralMaster = true;
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			ZQuery filter = new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.Equal, "GRJTY");
			var uNLOCO = Factory.LoadTop1<RefUNLOCO>(filter);
			uNLOCO.RL_HasAirport = true;
			uNLOCO.RL_IATA = "JTY";
			consol.JK_RL_NKDischargePort = uNLOCO.Code;
			OrgHeader sender = Factory.New<OrgHeader>();
			sender.OH_FullName = "Freight Sender 101";
			sender.Addresses[0].OA_Address1 = "111 Bourke Road Alexandria";
			sender.Addresses[0].OA_Address2 = "NSW 2000 Australia";
			consol.SetDefaultSendingForwarderAddress(sender);
			AddConsignee(consol);
			Transport primaryTransport = consol.Transports[0];
			primaryTransport.JW_ATD = new ZDateTime(2005, 1, 1);
			primaryTransport.JW_ATA = new ZDateTime(2005, 1, 2);
			primaryTransport.JW_TransportType = Core.Constants.TransportPlanningType.Flight1;
			primaryTransport.JW_VoyageFlight = "QF8898";
			primaryTransport.JW_ETD = new ZDateTime(2005, 1, 1, 18, 30, 22);
			// This has to be done last to ensure that MasterBillNum doesn't get changed when PortOfLoading / Discharge is modified.
			SetMasterBillNum(consol, "088", "12345101");
			return consol;
		}

		void SetMasterBillNum()
		{
			SetMasterBillNum(Consol, "088", "12345101");
		}

		void AddConsignee(ForwardingConsol consol)
		{
			OrgHeader receiver = Factory.New<OrgHeader>();
			receiver.OH_FullName = "Freight Receiver 102";
			receiver.Addresses[0].OA_Address1 = "101 Blablabla street Yummmmm";
			receiver.Addresses[0].OA_Address2 = "Cairo XXY131Z Egypt";
			consol.SetDefaultReceivingForwarderAddress(receiver);
		}

		DummyCMDData CreateCMDDataForTest()
		{
			DummyCMDData data = new DummyCMDData();
			data.TestHAWBGrossWeight = 56.78M;
			Goods goods1 = new Goods("Luxury Car A", "HAR001");
			Goods goods2 = new Goods("Elephant with trunks", "HAR005");
			Goods goods3 = new Goods("Big Elephants", "HAR110");
			Goods goods4 = new Goods("Bastard Elephants", "HAR111");
			data.TestHAWBNatureOfGoods = new Goods[] { goods1, goods2, goods3, goods4 };
			data.TestHAWBNoOfPieces = 89;
			data.TestHAWBSerialNo = "SERIAL123";
			data.TestHAWBWeightCode = "L";
			data.TestImportConsol = CreateImportConsolForTest("695", "44813366");
			data.TestTDBPermitNos = new ZString[] { "TDB0000101", "TDB0000102", "TDB0000103A", "TDB0000104B" };
			data.TestExemptionCode = "CD";
			data.TestExemptionRemarks = "blablabla";
			return data;
		}

		void SetMasterBillNum(ForwardingConsol consol, ZString airlinePrefix, ZString mAWBNo)
		{
			consol.MasterBillAirlinePrefix = airlinePrefix;
			consol.MasterBillMAWB = mAWBNo;
		}

		ForwardingConsol CreateImportConsolForTest(ZString airlinePrefix, ZString mAWBNo)
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			SetMasterBillNum(consol, airlinePrefix, mAWBNo);
			return consol;
		}

		ZString orgCompanyName;
		ZString orgStaffName;
		ZString orgHomePort;
		string messageID = "A/N/N\r\n";
		string mWB = "MWB/088-12345101JKTJTY/T77K234.99\r\n/AS PER MANIFEST\r\n";
		string fLT = "FLT/QF8898/01JAN\r\n";
		string hWB = "HWB/SERIAL123/89/L56.78\r\n/LUXURY CAR A/HAR001\r\n/ELEPHANT WITH TRUNKS/HAR005\r\n/BIG ELEPHANTS/HAR110\r\n/BASTARD ELEPHANTS/HAR111\r\n";
		string sHP = "SHP/FREIGHT SENDER 101\r\n/111 BOURKE ROAD ALEXANDRIA NSW 2000 AUSTRALIA\r\n";
		string cNE = "CNE/FREIGHT RECEIVER 102\r\n/101 BLABLABLA STREET YUMMMMM CAIRO XXY131Z EGYPT\r\n";
		string tPT = "TPT/TDB/TDB0000101 \r\n/TDB0000102 \r\n/TDB0000103A\r\n/TDB0000104B\r\n";
		string iMW = "IMW/695-44813366\r\n";
		string eXP = "EXP/CD/BLABLABLA\r\n";
		string dUI = "";
		readonly string sND = "SND/TEST USER/TEST COMPANY 123\r\n/012345678901        /CX81/TEST001\r\n";
		#endregion

		#region DummyCMD
		class DummyCMD : CMD
		{
			public DummyCMD(ForwardingConsol consol, CMDData data) : base(consol, data)
			{
			}

			public ZDateTime fNow;
			protected override ZDateTime Now
			{
				get
				{
					return fNow;
				}
			}

			public ZInt MAWBTotalPackageCount;
			protected override ZInt GetTotalPackageCount(ForwardingConsol consol)
			{
				return MAWBTotalPackageCount;
			}

			public ZDecimal MAWBTotalWeight;
			protected override ZDecimal GetTotalWeight(ForwardingConsol consol)
			{
				return MAWBTotalWeight;
			}

			public ZString MAWBWeightUnit;
			protected override ZString GetWeightUnit(ForwardingConsol consol)
			{
				return MAWBWeightUnit;
			}

			public bool TestIsExport = true;
			protected override bool IsExport
			{
				get
				{
					return TestIsExport;
				}
			}

			public bool TestIsImport;
			protected override bool IsImport
			{
				get
				{
					return TestIsImport;
				}
			}

			public ZInt BaseGetTotalPackageCount(ForwardingConsol consol)
			{
				return base.GetTotalPackageCount(consol);
			}

			public bool ExposedIsLate
			{
				get
				{
					return base.IsLate;
				}
			}
		}

		#endregion

		#region DummyCMDData
		class DummyCMDData : CMDData
		{
			public DummyCMDData() : base(null)
			{
			}

			public ZDecimal TestHAWBGrossWeight;
			public override ZDecimal HAWBGrossWeight
			{
				get
				{
					return TestHAWBGrossWeight;
				}
			}

			public Goods[] TestHAWBNatureOfGoods = Array.Empty<Goods>();
			public override Goods[] HAWBNatureOfGoods
			{
				get
				{
					return TestHAWBNatureOfGoods;
				}
			}

			public ZInt TestHAWBNoOfPieces;
			public override ZInt HAWBNoOfPieces
			{
				get
				{
					return TestHAWBNoOfPieces;
				}
			}

			public ZString TestHAWBSerialNo;
			public override ZString HAWBSerialNo
			{
				get
				{
					return TestHAWBSerialNo;
				}
			}

			public ZString TestHAWBWeightCode;
			public override ZString HAWBWeightCode
			{
				get
				{
					return TestHAWBWeightCode;
				}
			}

			public ForwardingConsol TestImportConsol;
			public override ForwardingConsol ImportConsol
			{
				get
				{
					return TestImportConsol;
				}
			}

			public ZString[] TestTDBPermitNos = Array.Empty<ZString>();
			public override ZString[] TDBPermitNos
			{
				get
				{
					return TestTDBPermitNos;
				}
			}

			public ZString TestExemptionCode;
			public override ZString ExemptionCode
			{
				get
				{
					return TestExemptionCode;
				}
			}

			public ZString TestExemptionRemarks;
			public override ZString ExemptionRemarks
			{
				get
				{
					return TestExemptionRemarks;
				}
			}

			public ZBool TestGoodsDelivered;
			public override ZBool GoodsDelivered
			{
				get
				{
					return TestGoodsDelivered;
				}
			}
		}
		#endregion
	}
}
