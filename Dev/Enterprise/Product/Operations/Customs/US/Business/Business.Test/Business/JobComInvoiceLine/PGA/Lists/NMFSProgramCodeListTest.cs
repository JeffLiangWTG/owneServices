using CargoWise.EntityFramework.Testing;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.Testing
{
	public class NMFSProgramCodeListTest : TestCaseWithFactory
	{
		public void TestGetListFor()
		{
			var fullList = new NMFSProgramCodeList();
			var list1 = NMFSProgramCodeList.GetListFor(Factory, false, false, false, false, false, false);
			var list2 = NMFSProgramCodeList.GetListFor(Factory, false, false, false, false, false, false);
			AssertEquals("Data should be cached", list1, list2);
			AssertEquals("Count", 0, list1.Count);

			list1 = NMFSProgramCodeList.GetListFor(Factory, true, false, false, false, false, false);
			AssertNotEquals("Data should be different", list1, list2);
			list2 = NMFSProgramCodeList.GetListFor(Factory, true, false, false, false, false, false);
			AssertEquals("Data should be cached", list1, list2);
			AssertEquals("Count", 1, list1.Count);
			AssertEquals(NMFSProgramCodeList.Codes._370, NMFSProgramCodeList.Descriptions._370, list1.GetDescriptionFromCode(NMFSProgramCodeList.Codes._370));

			list1 = NMFSProgramCodeList.GetListFor(Factory, true, false, true, false, false, false);
			AssertNotEquals("Data should be different", list1, list2);
			list2 = NMFSProgramCodeList.GetListFor(Factory, true, false, true, false, false, false);
			AssertEquals("Data should be cached", list1, list2);
			AssertEquals("Count", 2, list1.Count);
			AssertEquals(NMFSProgramCodeList.Codes._370, NMFSProgramCodeList.Descriptions._370, list1.GetDescriptionFromCode(NMFSProgramCodeList.Codes._370));
			AssertEquals(NMFSProgramCodeList.Codes.HMS, NMFSProgramCodeList.Descriptions.HMS, list1.GetDescriptionFromCode(NMFSProgramCodeList.Codes.HMS));

			list1 = NMFSProgramCodeList.GetListFor(Factory, true, true, true, false, false, false);
			AssertNotEquals("Data should be different", list1, list2);
			list2 = NMFSProgramCodeList.GetListFor(Factory, true, true, true, false, false, false);
			AssertEquals("Data should be cached", list1, list2);
			AssertEquals("Count", 3, list1.Count);
			AssertEquals(NMFSProgramCodeList.Codes._370, NMFSProgramCodeList.Descriptions._370, list1.GetDescriptionFromCode(NMFSProgramCodeList.Codes._370));
			AssertEquals(NMFSProgramCodeList.Codes.AMR, NMFSProgramCodeList.Descriptions.AMR, list1.GetDescriptionFromCode(NMFSProgramCodeList.Codes.AMR));
			AssertEquals(NMFSProgramCodeList.Codes.HMS, NMFSProgramCodeList.Descriptions.HMS, list1.GetDescriptionFromCode(NMFSProgramCodeList.Codes.HMS));

			list1 = NMFSProgramCodeList.GetListFor(Factory, true, true, true, true, false, false);
			AssertNotEquals("Data should be different", list1, list2);
			list2 = NMFSProgramCodeList.GetListFor(Factory, true, true, true, true, false, false);
			AssertEquals("Data should be cached", list1, list2);
			AssertEquals("If SIM is declared, it should be the fiest option", NMFSProgramCodeList.Codes.SIM, ((CodeDescriptionPair)list1[0]).Code);
			AssertEquals("", 4, list1.Count);
			AssertEquals(NMFSProgramCodeList.Codes._370, NMFSProgramCodeList.Descriptions._370, list1.GetDescriptionFromCode(NMFSProgramCodeList.Codes._370));
			AssertEquals(NMFSProgramCodeList.Codes.AMR, NMFSProgramCodeList.Descriptions.AMR, list1.GetDescriptionFromCode(NMFSProgramCodeList.Codes.AMR));
			AssertEquals(NMFSProgramCodeList.Codes.HMS, NMFSProgramCodeList.Descriptions.HMS, list1.GetDescriptionFromCode(NMFSProgramCodeList.Codes.HMS));
			AssertEquals(NMFSProgramCodeList.Codes.SIM, NMFSProgramCodeList.Descriptions.SIM, list1.GetDescriptionFromCode(NMFSProgramCodeList.Codes.SIM));

			list1 = NMFSProgramCodeList.GetListFor(Factory, true, true, true, true, true, false);
			AssertNotEquals("Data should be different", list1, list2);
			list2 = NMFSProgramCodeList.GetListFor(Factory, true, true, true, true, true, false);
			AssertEquals("Data should be cached", list1, list2);
			AssertEquals("If SIM is declared, it should be the fiest option", NMFSProgramCodeList.Codes.SIM, ((CodeDescriptionPair)list1[0]).Code);
			AssertEquals("", 5, list1.Count);
			AssertEquals(NMFSProgramCodeList.Codes._370, NMFSProgramCodeList.Descriptions._370, list1.GetDescriptionFromCode(NMFSProgramCodeList.Codes._370));
			AssertEquals(NMFSProgramCodeList.Codes.AMR, NMFSProgramCodeList.Descriptions.AMR, list1.GetDescriptionFromCode(NMFSProgramCodeList.Codes.AMR));
			AssertEquals(NMFSProgramCodeList.Codes.HMS, NMFSProgramCodeList.Descriptions.HMS, list1.GetDescriptionFromCode(NMFSProgramCodeList.Codes.HMS));
			AssertEquals(NMFSProgramCodeList.Codes.SIM, NMFSProgramCodeList.Descriptions.SIM, list1.GetDescriptionFromCode(NMFSProgramCodeList.Codes.SIM));
			AssertEquals(NMFSProgramCodeList.Codes.COA, NMFSProgramCodeList.Descriptions.COA, list1.GetDescriptionFromCode(NMFSProgramCodeList.Codes.COA));

			list1 = NMFSProgramCodeList.GetListFor(Factory, false, true, true, false, false, false);
			AssertNotEquals("Data should be different", list1, list2);
			list2 = NMFSProgramCodeList.GetListFor(Factory, false, true, true, false, false, false);
			AssertEquals("Data should be cached", list1, list2);
			AssertEquals("Count", 2, list1.Count);
			AssertEquals(NMFSProgramCodeList.Codes.AMR, NMFSProgramCodeList.Descriptions.AMR, list1.GetDescriptionFromCode(NMFSProgramCodeList.Codes.AMR));
			AssertEquals(NMFSProgramCodeList.Codes.HMS, NMFSProgramCodeList.Descriptions.HMS, list1.GetDescriptionFromCode(NMFSProgramCodeList.Codes.HMS));

			list1 = NMFSProgramCodeList.GetListFor(Factory, false, false, false, false, false, true);
			AssertEquals("Count", 2, list1.Count);
			AssertEquals(NMFSProgramCodeList.Codes.AMR, NMFSProgramCodeList.Descriptions.AMR, list1.GetDescriptionFromCode(NMFSProgramCodeList.Codes.AMR));
			AssertEquals(NMFSProgramCodeList.Codes.HMS, NMFSProgramCodeList.Descriptions.HMS, list1.GetDescriptionFromCode(NMFSProgramCodeList.Codes.HMS));
		}
	}
}
