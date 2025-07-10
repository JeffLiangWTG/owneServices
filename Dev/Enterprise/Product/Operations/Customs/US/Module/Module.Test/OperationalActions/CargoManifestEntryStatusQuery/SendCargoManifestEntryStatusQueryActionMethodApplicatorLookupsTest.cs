using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Module.OperationalActions.Testing
{
	sealed class SendCargoManifestEntryStatusQueryActionMethodApplicatorLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestActionList()
		{
			var applicator = new SendCargoManifestEntryStatusQueryActionMethodApplicator(new BusinessObjectFactory());
			var actionList = new SendCargoManifestEntryStatusQueryActionMethodApplicatorLookups(applicator).ActionList;
			AssertEquals("Action should have 5 options", 5, actionList.Count);

			AssertEquals("Entry", CargoManifestStatusQueryActionList.Descriptions.Entry, actionList.GetDescriptionFromCode(CargoManifestStatusQueryActionList.Codes.Entry));
			AssertEquals("HAWB", CargoManifestStatusQueryActionList.Descriptions.HAWB, actionList.GetDescriptionFromCode(CargoManifestStatusQueryActionList.Codes.HAWB));
			AssertEquals("MAWB", CargoManifestStatusQueryActionList.Descriptions.MAWB, actionList.GetDescriptionFromCode(CargoManifestStatusQueryActionList.Codes.MAWB));
			AssertEquals("OceanRailTruckBill", CargoManifestStatusQueryActionList.Descriptions.OceanRailTruckBill, actionList.GetDescriptionFromCode(CargoManifestStatusQueryActionList.Codes.OceanRailTruckBill));
			AssertEquals("InBond", CargoManifestStatusQueryActionList.Descriptions.InBond, actionList.GetDescriptionFromCode(CargoManifestStatusQueryActionList.Codes.InBond));
		}

		public void TestOutputOptionList()
		{
			var applicator = new SendCargoManifestEntryStatusQueryActionMethodApplicator(new BusinessObjectFactory());
			var outputOptionList = new SendCargoManifestEntryStatusQueryActionMethodApplicatorLookups(applicator).OutputOptionList;
			AssertEquals("OutputOption should have 3 options", 3, outputOptionList.Count);

			AssertEquals("MostRecentResults", LimitOutputCodeList.Descriptions._0MostRecentResults, outputOptionList.GetDescriptionFromCode(LimitOutputCodeList.Codes._0MostRecentResults));
			AssertEquals("AllAvailableResults", LimitOutputCodeList.Descriptions._1Last5Results, outputOptionList.GetDescriptionFromCode(LimitOutputCodeList.Codes._1Last5Results));
			AssertEquals("LastFiveResults", LimitOutputCodeList.Descriptions._2AllAvailableResults, outputOptionList.GetDescriptionFromCode(LimitOutputCodeList.Codes._2AllAvailableResults));
		}
	}
}
