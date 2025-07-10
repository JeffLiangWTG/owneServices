using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Moq;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	sealed class JobVoyageMatcherTest : TestCaseWithFactory
	{
		#region BestMatch

		public void TestBestMatch_Sea()
		{
			var seaVoyage1 = UniversalTestHelper.CreateSeaVoyage(Factory, "TAIKO", "001");
			var seaVoyage2 = UniversalTestHelper.CreateSeaVoyage(Factory, "TAIKO", "002");
			var seaVoyage3 = UniversalTestHelper.CreateSeaVoyage(Factory, "PIONKIO", "001");

			var railVoyage = UniversalTestHelper.CreateRailVoyage(Factory, "TAIKO", "001");

			Factory.Save();

			var context = new Mock<IXmlEventValueObjectContextValueList>();
			var references = new JobVoyageReferences(context.Object)
			{
				TransportMode = Core.Constants.TransportModes.Sea,
				VesselName = "TAIKO",
				VoyageFlight = "001"
			};

			var matcher = new JobVoyageMatcher(Factory, references, new DummyLogger());

			AssertEquals("matches correct sea voyage", seaVoyage1, matcher.GetBestMatch());
		}

		public void TestBestMatch_Sea_CarrierIsUsedForMatching()
		{
			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			var carrier2 = Factory.NewWithValidTestData<OrgHeader>();

			var seaVoyage1 = UniversalTestHelper.CreateSeaVoyage(Factory, "TAIKO", "001", carrier1.PK);
			var seaVoyage2 = UniversalTestHelper.CreateSeaVoyage(Factory, "TAIKO", "001", carrier2.PK);
			var seaVoyage3 = UniversalTestHelper.CreateSeaVoyage(Factory, "TAIKO", "001");

			Factory.Save();

			var context = new Mock<IXmlEventValueObjectContextValueList>();
			var references = new JobVoyageReferences(context.Object)
			{
				TransportMode = Core.Constants.TransportModes.Sea,
				VesselName = "TAIKO",
				VoyageFlight = "001",
				CarrierPK = carrier2.PK
			};

			var matcher = new JobVoyageMatcher(Factory, references, new DummyLogger());

			AssertEquals("matches correct sea voyage", seaVoyage2, matcher.GetBestMatch());
		}

		public void TestBestMatch_Sea_CarrierCodeIsUsedForMatching()
		{
			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			var carrier2 = Factory.NewWithValidTestData<OrgHeader>();

			var carrierCustomsCode = Factory.NewWithValidTestData<OrgCusCode>();
			carrierCustomsCode.OK_OH = carrier2.PK;
			carrierCustomsCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;
			carrierCustomsCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			carrierCustomsCode.OK_CustomsRegNo = "LIAM";

			var seaVoyage1 = UniversalTestHelper.CreateSeaVoyage(Factory, "TAIKO", "001", carrier1.PK);
			var seaVoyage2 = UniversalTestHelper.CreateSeaVoyage(Factory, "TAIKO", "001", carrier2.PK);
			var seaVoyage3 = UniversalTestHelper.CreateSeaVoyage(Factory, "TAIKO", "001");

			Factory.Save();

			var context = new Mock<IXmlEventValueObjectContextValueList>();
			var references = new JobVoyageReferences(context.Object)
			{
				TransportMode = Core.Constants.TransportModes.Sea,
				VesselName = "TAIKO",
				VoyageFlight = "001",
				CarrierSCACCode = carrierCustomsCode.OK_CustomsRegNo
			};

			var matcher = new JobVoyageMatcher(Factory, references, new DummyLogger());

			AssertEquals("matches correct sea voyage", seaVoyage2, matcher.GetBestMatch());
		}

		public void TestBestMatch_Sea_CarrierOrgCodeIsUsedForMatching()
		{
			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			var carrier2 = Factory.NewWithValidTestData<OrgHeader>();

			carrier1.OH_Code = "CARR1";
			carrier2.OH_Code = "CARR2";

			var seaVoyage1 = UniversalTestHelper.CreateSeaVoyage(Factory, "TAIKO", "001", carrier1.PK);
			var seaVoyage2 = UniversalTestHelper.CreateSeaVoyage(Factory, "TAIKO", "001", carrier2.PK);
			var seaVoyage3 = UniversalTestHelper.CreateSeaVoyage(Factory, "TAIKO", "001");

			Factory.Save();

			var context = new Mock<IXmlEventValueObjectContextValueList>();
			var references = new JobVoyageReferences(context.Object)
			{
				TransportMode = Core.Constants.TransportModes.Sea,
				VesselName = "TAIKO",
				VoyageFlight = "001",
				CarrierOrgCode = "CARR2"
			};

			var matcher = new JobVoyageMatcher(Factory, references, new DummyLogger());

			AssertEquals("matches correct sea voyage", seaVoyage2, matcher.GetBestMatch());
		}

		public void TestBestMatch_Sea_UsingLloydsNumberMatch()
		{
			var seaVessel = Factory.NewWithValidTestData<RefVessel>();
			seaVessel.RV_Name = "LIAM";
			seaVessel.RV_LloydsNumber = "LLLOYD";
			seaVessel.RV_RadioCallSign = "LRAD";

			var seaVoyage1 = UniversalTestHelper.CreateSeaVoyage(Factory, "LIAM", "002");
			var seaVoyage2 = UniversalTestHelper.CreateSeaVoyage(Factory, "LIAM", "001");
			var seaVoyage3 = UniversalTestHelper.CreateSeaVoyage(Factory, "MAIL", "001");

			Factory.Save();

			var context = new Mock<IXmlEventValueObjectContextValueList>();
			var references = new JobVoyageReferences(context.Object)
			{
				TransportMode = Core.Constants.TransportModes.Sea,
				VoyageFlight = "001",
				LloydsNumber = seaVessel.RV_LloydsNumber,
				VesselCallSign = "BRAD"
			};

			var matcher = new JobVoyageMatcher(Factory, references, new DummyLogger());

			AssertEquals("matches correct sea voyage", seaVoyage2, matcher.GetBestMatch());
		}

		public void TestBestMatch_Sea_UsingVesselCallSignMatch()
		{
			var seaVessel = Factory.NewWithValidTestData<RefVessel>();
			seaVessel.RV_Name = "LIAM";
			seaVessel.RV_LloydsNumber = "LLLOYD";
			seaVessel.RV_RadioCallSign = "LRAD";

			var seaVoyage1 = UniversalTestHelper.CreateSeaVoyage(Factory, "LIAM", "002");
			var seaVoyage2 = UniversalTestHelper.CreateSeaVoyage(Factory, "LIAM", "001");
			var seaVoyage3 = UniversalTestHelper.CreateSeaVoyage(Factory, "MAIL", "001");

			Factory.Save();

			var context = new Mock<IXmlEventValueObjectContextValueList>();
			var references = new JobVoyageReferences(context.Object)
			{
				TransportMode = Core.Constants.TransportModes.Sea,
				VoyageFlight = "001",
				LloydsNumber = "BADLLOY",
				VesselCallSign = seaVessel.RV_RadioCallSign
			};

			var matcher = new JobVoyageMatcher(Factory, references, new DummyLogger());

			AssertEquals("matches correct sea voyage", seaVoyage2, matcher.GetBestMatch());
		}

		public void TestBestMatch_Sea_NoValidVesselMatch()
		{
			var seaVessel = Factory.NewWithValidTestData<RefVessel>();
			seaVessel.RV_Name = "LIAM";
			seaVessel.RV_LloydsNumber = "LLLOYD";
			seaVessel.RV_RadioCallSign = "LRAD";

			var seaVoyage1 = UniversalTestHelper.CreateSeaVoyage(Factory, "LIAM", "002");
			var seaVoyage2 = UniversalTestHelper.CreateSeaVoyage(Factory, "LIAM", "001");
			var seaVoyage3 = UniversalTestHelper.CreateSeaVoyage(Factory, "MAIL", "001");

			Factory.Save();

			var context = new Mock<IXmlEventValueObjectContextValueList>();
			var references = new JobVoyageReferences(context.Object)
			{
				TransportMode = Core.Constants.TransportModes.Sea,
				VoyageFlight = "001",
				LloydsNumber = "RADI",
				VesselCallSign = "LLOY"
			};

			var matcher = new JobVoyageMatcher(Factory, references, new DummyLogger());

			AssertEquals("with no matching parents will default to null", null, matcher.GetBestMatch());
		}

		public void TestCorrectFallbackOrdering_Sea()
		{
			var goodVessel = Factory.NewWithValidTestData<RefVessel>();
			goodVessel.RV_Name = "LIAM";
			goodVessel.RV_LloydsNumber = "LLLOYD";
			goodVessel.RV_RadioCallSign = "LRAD";

			var badVessel = Factory.NewWithValidTestData<RefVessel>();
			badVessel.RV_Name = "DMITRY";
			badVessel.RV_LloydsNumber = "DLLOYD";
			badVessel.RV_RadioCallSign = "DRAD";

			var seaVoyage1 = UniversalTestHelper.CreateSeaVoyage(Factory, "LIAM", "002");
			var seaVoyage2 = UniversalTestHelper.CreateSeaVoyage(Factory, "LIAM", "001");
			var seaVoyage3 = UniversalTestHelper.CreateSeaVoyage(Factory, "DMITRY", "001");

			Factory.Save();

			var context = new Mock<IXmlEventValueObjectContextValueList>();
			var references = new JobVoyageReferences(context.Object)
			{
				TransportMode = Core.Constants.TransportModes.Sea,
				VesselName = badVessel.RV_Name,
				VoyageFlight = "001",
				LloydsNumber = goodVessel.RV_LloydsNumber,
				VesselCallSign = badVessel.RV_RadioCallSign
			};

			var matcher = new JobVoyageMatcher(Factory, references, new DummyLogger());
			AssertEquals("matches correct sea voyage", seaVoyage2, matcher.GetBestMatch());

			references = new JobVoyageReferences(context.Object)
			{
				TransportMode = Core.Constants.TransportModes.Sea,
				VesselName = badVessel.RV_Name,
				VoyageFlight = "001",
				VesselCallSign = goodVessel.RV_RadioCallSign
			};
			matcher = new JobVoyageMatcher(Factory, references, new DummyLogger());
			AssertEquals("matches correct sea voyage", seaVoyage2, matcher.GetBestMatch());

			references = new JobVoyageReferences(context.Object)
			{
				TransportMode = Core.Constants.TransportModes.Sea,
				VesselName = goodVessel.RV_Name,
				VoyageFlight = "001",
			};
			matcher = new JobVoyageMatcher(Factory, references, new DummyLogger());
			AssertEquals("matches correct sea voyage", seaVoyage2, matcher.GetBestMatch());
		}

		public void TestBestMatch_Air()
		{
			var airVoyage1 = UniversalTestHelper.CreateAirVoyage(Factory, "QF001");
			airVoyage1.JV_FlightDate = new ZDateTime(2012, 7, 25, 10, 30, 0);

			var airVoyage2 = UniversalTestHelper.CreateAirVoyage(Factory, "QF001");
			airVoyage2.JV_FlightDate = new ZDateTime(2012, 7, 26, 10, 30, 0);

			var airVoyage3 = UniversalTestHelper.CreateAirVoyage(Factory, "QF002");
			airVoyage3.JV_FlightDate = new ZDateTime(2012, 7, 26, 09, 30, 0);

			var railVoyage = UniversalTestHelper.CreateRoadVoyage(Factory, "QF001");

			Factory.Save();

			var context = new Mock<IXmlEventValueObjectContextValueList>();
			var references = new JobVoyageReferences(context.Object)
			{
				TransportMode = Core.Constants.TransportModes.Air,
				VoyageFlight = "QF001",
				FlightDate = new ZDateTime(2012, 7, 26, 07, 35, 0)
			};

			var matcher = new JobVoyageMatcher(Factory, references, new DummyLogger());

			AssertEquals("matches correct sea voyage", airVoyage2, matcher.GetBestMatch());
		}

		public void TestBestMatch_Rail()
		{
			var railVoyage1 = UniversalTestHelper.CreateRailVoyage(Factory, "PACIFIC EXPRESS", "A1");
			var railVoyage2 = UniversalTestHelper.CreateRailVoyage(Factory, "PACIFIC EXPRESS", "A2");
			var railVoyage3 = UniversalTestHelper.CreateRailVoyage(Factory, "ZIG ZAG", "A2");

			var seaVoyage = UniversalTestHelper.CreateSeaVoyage(Factory, "PACIFIC EXPRESS", "A2");

			Factory.Save();

			var context = new Mock<IXmlEventValueObjectContextValueList>();
			var references = new JobVoyageReferences(context.Object)
			{
				TransportMode = Core.Constants.TransportModes.Rail,
				VesselName = "PACIFIC EXPRESS",
				VoyageFlight = "A2"
			};

			var matcher = new JobVoyageMatcher(Factory, references, new DummyLogger());

			AssertEquals("matches correct sea voyage", railVoyage2, matcher.GetBestMatch());
		}

		public void TestBestMatch_Road()
		{
			var roadVoyage1 = UniversalTestHelper.CreateRoadVoyage(Factory, "FOX-1");
			roadVoyage1.JV_FlightDate = new ZDateTime(2012, 7, 25, 10, 30, 0);

			var roadVoyage2 = UniversalTestHelper.CreateRoadVoyage(Factory, "FOX-1");
			roadVoyage2.JV_FlightDate = new ZDateTime(2012, 7, 26, 10, 30, 0);

			var roadVoyage3 = UniversalTestHelper.CreateRoadVoyage(Factory, "FOX-2");
			roadVoyage3.JV_FlightDate = new ZDateTime(2012, 7, 26, 09, 30, 0);

			var railVoyage = UniversalTestHelper.CreateAirVoyage(Factory, "FOX-2");

			Factory.Save();

			var context = new Mock<IXmlEventValueObjectContextValueList>();
			var references = new JobVoyageReferences(context.Object)
			{
				TransportMode = Core.Constants.TransportModes.Road,
				VoyageFlight = "FOX-1",
				FlightDate = new ZDateTime(2012, 7, 26, 07, 35, 0)
			};

			var matcher = new JobVoyageMatcher(Factory, references, new DummyLogger());

			AssertEquals("matches correct road voyage", roadVoyage2, matcher.GetBestMatch());
		}

		#endregion

		#region GetLatest

		public void TestGetLatest_Sea()
		{
			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			carrier1.OH_Code = "CARR1";
			var carrier2 = Factory.NewWithValidTestData<OrgHeader>();
			carrier2.OH_Code = "CARR2";

			var seaVoyage1 = UniversalTestHelper.CreateSeaVoyage(Factory, "TAIKO", "001", carrier1.PK);
			var seaVoyage2 = UniversalTestHelper.CreateSeaVoyage(Factory, "TAIKO", "001", carrier2.PK);

			using (seaVoyage1.Logs.AddedLog.LockForUpdatingKeyFieldsForTesting())
			{
				seaVoyage1.Logs.AddedLog.SL_EventTime = new ZDateTime(2012, 1, 1);
			}

			using (seaVoyage2.Logs.AddedLog.LockForUpdatingKeyFieldsForTesting())
			{
				seaVoyage2.Logs.AddedLog.SL_EventTime = new ZDateTime(2012, 1, 2);
			}

			Factory.Save();

			var context = new Mock<IXmlEventValueObjectContextValueList>();
			var references = new JobVoyageReferences(context.Object)
			{
				TransportMode = Core.Constants.TransportModes.Sea,
				VesselName = "TAIKO",
				VoyageFlight = "001",
				FlightDate = new ZDateTime(2012, 1, 2)
			};

			var logger = new TestErrorLogger();
			var matcher = new JobVoyageMatcher(Factory, references, logger);

			AssertEquals("does not match voyage as choices are too ambiguous", null, matcher.GetBestMatch());

			var expectedLines = new ZString[] {
				"Information - Found 2 matches using Combination Key Match. Finding a match was too ambiguous so the message was discarded. Possible matches include:",
				"Sailing Schedule (Vessel='TAIKO', Voyage='001', Carrier='CARR1')",
				"Sailing Schedule (Vessel='TAIKO', Voyage='001', Carrier='CARR2')"
			};
			var actualLines = ((ZString)logger.Logs).Split("\r\n");

			AssertContainsExactElementsInAnyOrder(expectedLines, actualLines);
		}

		public void TestGetLatest_Air()
		{
			var airVoyage1 = UniversalTestHelper.CreateAirVoyage(Factory, "QF001");
			airVoyage1.JV_FlightDate = new ZDateTime(2012, 1, 1, 10, 30, 0);

			var airVoyage2 = UniversalTestHelper.CreateAirVoyage(Factory, "QF001");
			airVoyage2.JV_FlightDate = new ZDateTime(2012, 1, 1, 09, 30, 0);

			using (airVoyage1.Logs.AddedLog.LockForUpdatingKeyFieldsForTesting())
			{
				airVoyage1.Logs.AddedLog.SL_EventTime = new ZDateTime(2012, 1, 1);
			}

			using (airVoyage2.Logs.AddedLog.LockForUpdatingKeyFieldsForTesting())
			{
				airVoyage2.Logs.AddedLog.SL_EventTime = new ZDateTime(2012, 1, 1);
			}

			Factory.Save();

			var context = new Mock<IXmlEventValueObjectContextValueList>();
			var references = new JobVoyageReferences(context.Object)
			{
				TransportMode = Core.Constants.TransportModes.Air,
				VoyageFlight = "QF001",
				FlightDate = new ZDateTime(2012, 1, 1, 09, 59, 0)
			};

			var matcher = new JobVoyageMatcher(Factory, references, new DummyLogger());

			AssertEquals("selects latest air voyage using closest flight date", airVoyage2, matcher.GetBestMatch());

			airVoyage1.JV_FlightDate = new ZDateTime(2012, 1, 1, 10, 30, 0);
			airVoyage2.JV_FlightDate = new ZDateTime(2012, 1, 1, 10, 30, 0);

			using (airVoyage1.Logs.AddedLog.LockForUpdatingKeyFieldsForTesting())
			{
				airVoyage1.Logs.AddedLog.SL_EventTime = new ZDateTime(2012, 1, 1);
			}

			using (airVoyage2.Logs.AddedLog.LockForUpdatingKeyFieldsForTesting())
			{
				airVoyage2.Logs.AddedLog.SL_EventTime = new ZDateTime(2012, 1, 2);
			}

			matcher = new JobVoyageMatcher(Factory, references, new DummyLogger());

			AssertEquals("selects latest air voyage using added log when flight date is inconclusive", airVoyage2, matcher.GetBestMatch());

			references.FlightDate = ZDateTime.Empty;

			matcher = new JobVoyageMatcher(Factory, references, new DummyLogger());

			AssertEquals("should not find any air voyages if the flight date is not valid", null, matcher.GetBestMatch());
		}

		public void TestGetLatest_Rail()
		{
			var railVoyage1 = UniversalTestHelper.CreateRailVoyage(Factory, "ORIENT EXPRESS", "A-1");
			var railVoyage2 = UniversalTestHelper.CreateRailVoyage(Factory, "ORIENT EXPRESS", "A-1");

			using (railVoyage1.Logs.AddedLog.LockForUpdatingKeyFieldsForTesting())
			{
				railVoyage1.Logs.AddedLog.SL_EventTime = new ZDateTime(2012, 1, 1);
			}

			using (railVoyage2.Logs.AddedLog.LockForUpdatingKeyFieldsForTesting())
			{
				railVoyage2.Logs.AddedLog.SL_EventTime = new ZDateTime(2012, 1, 2);
			}

			Factory.Save();

			var context = new Mock<IXmlEventValueObjectContextValueList>();
			var references = new JobVoyageReferences(context.Object)
			{
				TransportMode = Core.Constants.TransportModes.Rail,
				VesselName = "ORIENT EXPRESS",
				VoyageFlight = "A-1",
				FlightDate = new ZDateTime(2012, 1, 2)
			};

			var matcher = new JobVoyageMatcher(Factory, references, new DummyLogger());

			AssertEquals("does not match voyage as choices are too ambiguous", null, matcher.GetBestMatch());
		}

		public void TestGetLatest_Road()
		{
			var roadVoyage1 = UniversalTestHelper.CreateRoadVoyage(Factory, "FOX-1");
			roadVoyage1.JV_FlightDate = new ZDateTime(2012, 1, 1, 10, 30, 0);

			var roadVoyage2 = UniversalTestHelper.CreateRoadVoyage(Factory, "FOX-1");
			roadVoyage2.JV_FlightDate = new ZDateTime(2012, 1, 1, 09, 30, 0);

			using (roadVoyage1.Logs.AddedLog.LockForUpdatingKeyFieldsForTesting())
			{
				roadVoyage1.Logs.AddedLog.SL_EventTime = new ZDateTime(2012, 1, 1);
			}

			using (roadVoyage2.Logs.AddedLog.LockForUpdatingKeyFieldsForTesting())
			{
				roadVoyage2.Logs.AddedLog.SL_EventTime = new ZDateTime(2012, 1, 1);
			}

			Factory.Save();

			var context = new Mock<IXmlEventValueObjectContextValueList>();
			var references = new JobVoyageReferences(context.Object)
			{
				TransportMode = Core.Constants.TransportModes.Road,
				VoyageFlight = "FOX-1",
				FlightDate = new ZDateTime(2012, 1, 1, 09, 59, 0)
			};

			var matcher = new JobVoyageMatcher(Factory, references, new DummyLogger());

			AssertEquals("selects latest road voyage using closest flight date", roadVoyage2, matcher.GetBestMatch());

			roadVoyage1.JV_FlightDate = new ZDateTime(2012, 1, 1, 10, 30, 0);
			roadVoyage2.JV_FlightDate = new ZDateTime(2012, 1, 1, 10, 30, 0);

			using (roadVoyage1.Logs.AddedLog.LockForUpdatingKeyFieldsForTesting())
			{
				roadVoyage1.Logs.AddedLog.SL_EventTime = new ZDateTime(2012, 1, 1);
			}

			using (roadVoyage2.Logs.AddedLog.LockForUpdatingKeyFieldsForTesting())
			{
				roadVoyage2.Logs.AddedLog.SL_EventTime = new ZDateTime(2012, 1, 2);
			}

			matcher = new JobVoyageMatcher(Factory, references, new DummyLogger());

			AssertEquals("selects latest road voyage using added log when flight date is inconclusive", roadVoyage2, matcher.GetBestMatch());

			references.FlightDate = ZDateTime.Empty;

			matcher = new JobVoyageMatcher(Factory, references, new DummyLogger());

			AssertEquals("should not find any road voyages if the flight date is not valid", null, matcher.GetBestMatch());
		}

		#endregion

		#region TestBestMatch_Sea_CarrierIsInactive

		public void TestBestMatch_Sea_CarrierIsInactive()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "CARR";

			var seaVoyage = UniversalTestHelper.CreateSeaVoyage(Factory, "TAIKO", "001");
			seaVoyage.JV_OH_Line = carrier.PK;

			Factory.Save();

			var context = new Mock<IXmlEventValueObjectContextValueList>();
			var references = new JobVoyageReferences(context.Object)
			{
				TransportMode = Core.Constants.TransportModes.Sea,
				CarrierOrgCode = "CARR",
				VesselName = "TAIKO",
				VoyageFlight = "001"
			};

			var matcher = new JobVoyageMatcher(Factory, references, new DummyLogger());
			AssertNotNull(matcher.GetBestMatch());

			carrier.OH_IsActive = false;

			Factory.Save();

			matcher = new JobVoyageMatcher(Factory, references, new DummyLogger());
			AssertNull(matcher.GetBestMatch());
		}

		#endregion
	}
}
