using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.SailingDataVendor.Business.Forwarding
{
	sealed class OneStopCarrierCodePairListTest : TestCaseWithFactory
	{
		public void TestListElements()
		{
			JobVesselScheduleBase domesticPort1 = NewJobVesselSchedule("AUSYD", new ZDateTime(2020, 1, 1), new ZDateTime(2020, 1, 2), "Lloyds", "TestVoyage", "TestVoyage");
			JobVesselScheduleBase domesticPort2 = NewJobVesselSchedule("AUSYD", new ZDateTime(2020, 1, 3), new ZDateTime(2020, 1, 4), "Lloyds", "TestVoyage", "TestVoyage");
			JobVesselScheduleBase domesticPort3 = NewJobVesselSchedule("AUSYD", new ZDateTime(2020, 1, 5), new ZDateTime(2020, 1, 6), "Lloyds", "TestVoyage", "TestVoyage");
			domesticPort1.EV_LineOperator = "LO1";
			domesticPort1.EV_OperatorsDescription = "Line Operator 1";
			domesticPort2.EV_LineOperator = "LO2";
			domesticPort2.EV_OperatorsDescription = "Line Operator 2";
			domesticPort3.EV_LineOperator = "LO3";
			domesticPort3.EV_OperatorsDescription = "Line Operator 3";
			Factory.Save();

			CodeDescriptionPairList oneStopCarrierList = new CodeDescriptionPairList(new OneStopCarrierCodePairList(Factory).GetOneStopCarrierCodePairListForFilter());

			AssertEquals("There should be 3 1-Stop line operators for the voyage", 3, oneStopCarrierList.Count);
			AssertEquals("1st operator - not on file", "LO1", oneStopCarrierList[0].Code);
			AssertEquals("1st operator - not on file", "Line Operator 1 (not on file)", oneStopCarrierList[0].Description);
			AssertEquals("2nd operator - not on file", "LO2", oneStopCarrierList[1].Code);
			AssertEquals("2nd operator - not on file", "Line Operator 2 (not on file)", oneStopCarrierList[1].Description);
			AssertEquals("3rd operator - not on file", "LO3", oneStopCarrierList[2].Code);
			AssertEquals("3rd operator - not on file", "Line Operator 3 (not on file)", oneStopCarrierList[2].Description);
		}

		public void TestListElementsForDuplication()
		{
			JobVesselScheduleBase domesticPort1 = NewJobVesselSchedule("AUSYD", new ZDateTime(2020, 1, 1), new ZDateTime(2020, 1, 2), "Lloyds", "TestVoyage", "TestVoyage");
			JobVesselScheduleBase domesticPort2 = NewJobVesselSchedule("AUSYD", new ZDateTime(2020, 1, 3), new ZDateTime(2020, 1, 4), "Lloyds", "TestVoyage", "TestVoyage");
			JobVesselScheduleBase domesticPort3 = NewJobVesselSchedule("AUSYD", new ZDateTime(2020, 1, 5), new ZDateTime(2020, 1, 6), "Lloyds", "TestVoyage", "TestVoyage");
			domesticPort1.EV_LineOperator = "LO1";
			domesticPort1.EV_OperatorsDescription = "Line Operator 1";
			domesticPort2.EV_LineOperator = "LO2";
			domesticPort2.EV_OperatorsDescription = "Line Operator 2";
			domesticPort3.EV_LineOperator = "LO2";
			domesticPort3.EV_OperatorsDescription = "Line Operator 2";
			Factory.Save();

			CodeDescriptionPairList oneStopCarrierList = new CodeDescriptionPairList(new OneStopCarrierCodePairList(Factory).GetOneStopCarrierCodePairListForFilter());

			AssertEquals("There should be 2 unique 1-Stop line operators for the voyage", 2, oneStopCarrierList.Count);
			AssertEquals("1st operator - not on file", "LO1", oneStopCarrierList[0].Code);
			AssertEquals("1st operator - not on file", "Line Operator 1 (not on file)", oneStopCarrierList[0].Description);
			AssertEquals("2nd operator - not on file", "LO2", oneStopCarrierList[1].Code);
			AssertEquals("2nd operator - not on file", "Line Operator 2 (not on file)", oneStopCarrierList[1].Description);
		}

		JobVesselScheduleBase NewJobVesselSchedule(ZString portCode, ZDateTime eTA, ZDateTime eTD, ZString lloyds, ZString voyageIn, ZString voyageOut)
		{
			JobVesselScheduleBase result = Factory.New<JobVesselScheduleBase>();
			result.EV_RL_NKPortCode = portCode;
			result.EV_ETA = eTA;
			result.EV_ETD = eTD;
			result.EV_IMOLloydsNumber = "Lloyds";
			result.EV_ShipOperatorVoyageIn = voyageIn;
			result.EV_ShipOperatorVoyageOut = voyageOut;
			return result;
		}
	}
}
