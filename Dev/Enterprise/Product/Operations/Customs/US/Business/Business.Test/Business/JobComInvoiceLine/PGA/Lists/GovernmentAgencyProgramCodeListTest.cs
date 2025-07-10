using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	class GovernmentAgencyProgramCodeListTest : CargoWise.EntityFramework.Testing.TestCaseWithFactory
	{
		public void TestIsPGAAllowed()
		{
			var governmentAgencyProgramList = new GovernmentAgencyProgramCodeList();
			governmentAgencyProgramList.AddPair(GovernmentAgencyProgramCodeList.NMFS, GovernmentAgencyProgramCodeList.NMFS);
			var allGovernmentProgramCodesList = governmentAgencyProgramList.ToArray().Select(x => x.Code).ToList();
			var supportedAgencyProgramCodes = allGovernmentProgramCodesList.ToList();
			AssertIsPGAAllowed(EntryTypeList.Codes.ConsumptionFreeDutiable, allGovernmentProgramCodesList, supportedAgencyProgramCodes);
			AssertIsPGAAllowed(EntryTypeList.Codes.ConsumptionADDCVD, allGovernmentProgramCodesList, supportedAgencyProgramCodes);

			supportedAgencyProgramCodes = allGovernmentProgramCodesList.ToList();
			AssertIsPGAAllowed(EntryTypeList.Codes.ConsumptionQuotaVisa, allGovernmentProgramCodesList, supportedAgencyProgramCodes);
			AssertIsPGAAllowed(EntryTypeList.Codes.ConsumptionADDCVDQuotaVisa, allGovernmentProgramCodesList, supportedAgencyProgramCodes);

			supportedAgencyProgramCodes = allGovernmentProgramCodesList.ToList();
			supportedAgencyProgramCodes.Remove(GovernmentAgencyProgramCodeList.Codes.Lacey);
			AssertIsPGAAllowed(EntryTypeList.Codes.InformalFreeDutiable, allGovernmentProgramCodesList, supportedAgencyProgramCodes);

			AssertIsPGAAllowed(EntryTypeList.Codes.InformalQuotaVisa, allGovernmentProgramCodesList, supportedAgencyProgramCodes);

			supportedAgencyProgramCodes = allGovernmentProgramCodesList.ToList();
			supportedAgencyProgramCodes.Remove(GovernmentAgencyProgramCodeList.Codes.Lacey);
			supportedAgencyProgramCodes.Remove(GovernmentAgencyProgramCodeList.Codes.DEA);
			supportedAgencyProgramCodes.Remove(GovernmentAgencyProgramCodeList.Codes.OMC);
			supportedAgencyProgramCodes.Remove(GovernmentAgencyProgramCodeList.Codes._370);
			supportedAgencyProgramCodes.Remove(GovernmentAgencyProgramCodeList.Codes.COA);
			supportedAgencyProgramCodes.Remove(GovernmentAgencyProgramCodeList.Codes.AMR);
			supportedAgencyProgramCodes.Remove(GovernmentAgencyProgramCodeList.Codes.HMS);
			supportedAgencyProgramCodes.Remove(GovernmentAgencyProgramCodeList.Codes.SIMP);
			supportedAgencyProgramCodes.Remove(GovernmentAgencyProgramCodeList.NMFS);
			supportedAgencyProgramCodes.Remove(GovernmentAgencyProgramCodeList.Codes.TTB);
			AssertIsPGAAllowed(EntryTypeList.Codes.Warehouse, allGovernmentProgramCodesList, supportedAgencyProgramCodes);

			supportedAgencyProgramCodes.Clear();
			AssertIsPGAAllowed(EntryTypeList.Codes.ReWarehouse, allGovernmentProgramCodesList, supportedAgencyProgramCodes);

			supportedAgencyProgramCodes = allGovernmentProgramCodesList.ToList();
			supportedAgencyProgramCodes.Remove(GovernmentAgencyProgramCodeList.Codes.Lacey);
			supportedAgencyProgramCodes.Remove(GovernmentAgencyProgramCodeList.Codes.TTB);
			AssertIsPGAAllowed(EntryTypeList.Codes.TemporaryImportationBond, allGovernmentProgramCodesList, supportedAgencyProgramCodes);

			supportedAgencyProgramCodes.Clear();
			supportedAgencyProgramCodes.Add(GovernmentAgencyProgramCodeList.Codes.Lacey);
			supportedAgencyProgramCodes.Add(GovernmentAgencyProgramCodeList.Codes.DEA);
			supportedAgencyProgramCodes.Add(GovernmentAgencyProgramCodeList.Codes._370);
			supportedAgencyProgramCodes.Add(GovernmentAgencyProgramCodeList.Codes.COA);
			supportedAgencyProgramCodes.Add(GovernmentAgencyProgramCodeList.Codes.HMS);
			supportedAgencyProgramCodes.Add(GovernmentAgencyProgramCodeList.Codes.AMR);
			supportedAgencyProgramCodes.Add(GovernmentAgencyProgramCodeList.Codes.SIMP);
			supportedAgencyProgramCodes.Add(GovernmentAgencyProgramCodeList.NMFS);
			supportedAgencyProgramCodes.Add(GovernmentAgencyProgramCodeList.Codes.OMC);
			supportedAgencyProgramCodes.Add(GovernmentAgencyProgramCodeList.Codes.TTB);
			AssertIsPGAAllowed(EntryTypeList.Codes.WarehouseWithdrawalConsumption, allGovernmentProgramCodesList, supportedAgencyProgramCodes);
			AssertIsPGAAllowed(EntryTypeList.Codes.WarehouseWithdrawalQuota, allGovernmentProgramCodesList, supportedAgencyProgramCodes);
			AssertIsPGAAllowed(EntryTypeList.Codes.WarehouseWithdrawalADDCVD, allGovernmentProgramCodesList, supportedAgencyProgramCodes);
			AssertIsPGAAllowed(EntryTypeList.Codes.WarehouseWithdrawalADDCVDQuotaVisa, allGovernmentProgramCodesList, supportedAgencyProgramCodes);

			supportedAgencyProgramCodes = allGovernmentProgramCodesList.ToList();
			supportedAgencyProgramCodes.Remove(GovernmentAgencyProgramCodeList.Codes._370);
			supportedAgencyProgramCodes.Remove(GovernmentAgencyProgramCodeList.Codes.COA);
			supportedAgencyProgramCodes.Remove(GovernmentAgencyProgramCodeList.Codes.AMR);
			supportedAgencyProgramCodes.Remove(GovernmentAgencyProgramCodeList.Codes.HMS);
			supportedAgencyProgramCodes.Remove(GovernmentAgencyProgramCodeList.Codes.SIMP);
			supportedAgencyProgramCodes.Remove(GovernmentAgencyProgramCodeList.NMFS);
			AssertIsPGAAllowed(EntryTypeList.Codes.GovernmentDutiable, allGovernmentProgramCodesList, supportedAgencyProgramCodes);
		}

		public void TestPGADataForWeeklyEntriesForAMS()
		{
			var governmentAgencyProgramList = new GovernmentAgencyProgramCodeList();
			governmentAgencyProgramList.AddPair(GovernmentAgencyProgramCodeList.Codes.AMS, GovernmentAgencyProgramCodeList.Codes.AMS);
			var amsAllowed = GovernmentAgencyProgramCodeList.IsPGAAllowed(true, false, false, false, true, EntryTypeList.Codes.ConsumptionFTZ, GovernmentAgencyProgramCodeList.Codes.AMS);
			AssertEquals("Should be included in ENS message", true, amsAllowed);
			amsAllowed = GovernmentAgencyProgramCodeList.IsPGAAllowed(false, true, false, false, true, EntryTypeList.Codes.ConsumptionFTZ, GovernmentAgencyProgramCodeList.Codes.AMS);
			AssertEquals("Should Not be included in SE message", false, amsAllowed);
		}

		void AssertIsPGAAllowed(ZString entryType, List<string> allProgramCodesList, List<string> supportedProgramCodesList)
		{
			foreach (var supportedCode in supportedProgramCodesList)
			{
				AssertEquals(ZString.Format(notificationMessage, supportedCode, entryType), true, GovernmentAgencyProgramCodeList.IsPGAAllowed(true, false, true, false, false, entryType, supportedCode));
			}

			var unsupportedProgramCodesList = allProgramCodesList.Except(supportedProgramCodesList);
			foreach (var unsupportedCode in unsupportedProgramCodesList)
			{
				AssertEquals(ZString.Format(notificationMessage, unsupportedCode, entryType), false, GovernmentAgencyProgramCodeList.IsPGAAllowed(true, false, true, false, false, entryType, unsupportedCode));
			}
		}
		readonly ZString notificationMessage = "Please check if PGA {0} is supported to be send in Entry Summary message when entry type is '{1}'.";
	}
}
