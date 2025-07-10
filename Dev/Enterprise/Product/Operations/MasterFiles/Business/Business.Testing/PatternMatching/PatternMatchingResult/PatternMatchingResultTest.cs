using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(PatternMatchingResult))]
	sealed class PatternMatchingResultTest : EnterpriseBusinessObjectTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			Result = CreateResult();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			PatternMatchingResult result = base.GetNewBusinessObjectForDeleteTest(factory) as PatternMatchingResult;
			result.PMT_GS_NKExcludeBy = "";
			result.PMT_Status = "PDU";
			result.PMT_FoundTimeUtc = DateTime.Now;
			result.PMT_MasterPK = ZGuid.NewZGuid();
			result.PMT_MasterTableCode = "OH";
			result.PMT_TargetPK = ZGuid.NewZGuid();
			result.PMT_TargetTableCode = "OH";
			result.PMT_ScorePercent = 0;
			return result;
		}

		PatternMatchingResult CreateResult()
		{
			var result = Factory.New<PatternMatchingResult>();
			result.PMT_MasterPK = ZGuid.NewZGuid();
			result.PMT_TargetPK = ZGuid.NewZGuid();
			result.PMT_Status = "PDU";
			result.PMT_FoundTimeUtc = ZDateTime.UtcNow;
			result.PMT_MasterTableCode = result.PMT_TargetTableCode = "OH";
			Factory.Save();
			return result;
		}

		public void TestFinalStatusAndDescription()
		{
			AssertEquals("PDU", Result.FinalStatus);
			AssertEquals("Potential duplicate", Result.FinalStatusDescription);

			Result = CreateResult();
			var tig = CreateResult();
			tig.PMT_Status = "TIG";
			tig.PMT_GS_NKExcludeBy = "USR";
			tig.PMT_MasterPK = Result.PMT_MasterPK;
			tig.PMT_TargetPK = Result.PMT_TargetPK;
			AssertEquals("PDU", Result.FinalStatus);
			AssertEquals("Potential duplicate", Result.FinalStatusDescription);

			Result = CreateResult();
			tig = CreateResult();
			tig.PMT_Status = "TIG";
			tig.PMT_GS_NKExcludeBy = GlbStaff.CurrentUser.GS_Code;
			tig.PMT_MasterPK = Result.PMT_MasterPK;
			tig.PMT_TargetPK = Result.PMT_TargetPK;
			AssertEquals("TIG", Result.FinalStatus);
			AssertEquals("Temporarily ignored", Result.FinalStatusDescription);

			Result = CreateResult();
			var pig = CreateResult();
			pig.PMT_GS_NKExcludeBy = "USR";
			pig.PMT_Status = "PIG";
			pig.PMT_MasterPK = Result.PMT_MasterPK;
			pig.PMT_TargetPK = Result.PMT_TargetPK;
			AssertEquals("PIG", Result.FinalStatus);
			AssertEquals("Permanently ignored", Result.FinalStatusDescription);
		}

		public void TestConfidencDescription()
		{
			Result.PMT_ScorePercent = 5;
			AssertEquals("None confidence", "None", Result.PMT_Confidence);
			Result.PMT_ScorePercent = 35;
			AssertEquals("Low confidence", "Low", Result.PMT_Confidence);
			Result.PMT_ScorePercent = 55;
			AssertEquals("Medium confidence", "Medium", Result.PMT_Confidence);
			Result.PMT_ScorePercent = 85;
			AssertEquals("High confidence", "High", Result.PMT_Confidence);
			Result.PMT_ScorePercent = 155;
			AssertEquals("Exact confidence", "Exact", Result.PMT_Confidence);
		}

		public void TestExcludedByListFactoryNotOwnedByCurrentThread()
		{
			var thread = new System.Threading.Thread(() =>
			{
				Result.ExcludedByList.ToString();
			});
			AssertNoExceptionThrown("No exception from other thread visit", () =>
			{
				thread.Start();
				thread.Join();
			});
		}

		public void TestExcludedByListNotStartWithComma()
		{
			var masterPK = ZGuid.NewZGuid();
			var targetPK = ZGuid.NewZGuid();
			var result1 = Factory.New<PatternMatchingResult>();
			var result2 = Factory.New<PatternMatchingResult>();
			result1.PMT_MasterPK = result2.PMT_MasterPK = masterPK;
			result1.PMT_TargetPK = result2.PMT_TargetPK = targetPK;
			result2.PMT_Status = PatternMatchingResult.StatusCodes.TemporaryIgnore;
			result2.PMT_GS_NKExcludeBy = "USR";
			AssertNotEquals(", USR", result1.ExcludedByList);
			AssertEquals("Empty User code trimmed", "USR", result1.ExcludedByList);
		}

		public void TestStatusDescription()
		{
			Result.PMT_Status = PatternMatchingResult.StatusCodes.Error;
			AssertEquals("Error Status", "Error", Result.PMT_StatusDescription);
			Result.PMT_Status = PatternMatchingResult.StatusCodes.Excluded;
			AssertEquals("Excluded Status", "Excluded", Result.PMT_StatusDescription);
			Result.PMT_Status = PatternMatchingResult.StatusCodes.NoDuplicates;
			AssertEquals("Excluded Status", "No duplicates", Result.PMT_StatusDescription);
			Result.PMT_Status = PatternMatchingResult.StatusCodes.PermanentIgnore;
			AssertEquals("Excluded Status", "Permanently ignored", Result.PMT_StatusDescription);
			Result.PMT_Status = PatternMatchingResult.StatusCodes.PotentialDuplicate;
			AssertEquals("Excluded Status", "Potential duplicate", Result.PMT_StatusDescription);
			Result.PMT_Status = PatternMatchingResult.StatusCodes.TemporaryIgnore;
			AssertEquals("Excluded Status", "Temporarily ignored", Result.PMT_StatusDescription);
		}

		public void TestTargetOrgHeaderProperties()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_FullName = "Test Org Full Name";
			orgHeader.OH_RL_NKClosestPort = "USCHI";
			orgHeader.OH_IsDebtor = true;
			orgHeader.OH_IsForwarder = true;
			orgHeader.OH_Code = "TEST11";

			Result.PMT_TargetPK = orgHeader.PK;

			AssertEquals("Target OrgCode", "TEST11", Result.PMT_TargetOrgCode);
			AssertEquals("Target OrgName", "Test Org Full Name", Result.PMT_TargetOrgName);
			AssertEquals("Target OrgUNLOCO", "USCHI", Result.PMT_TargetOrgUNLOCO);
			AssertEquals("Target OrgType", "Debtor, Forwarder", Result.PMT_TargetOrgType);
		}

		public void TestOperatorList()
		{
			var masterPK = ZGuid.NewZGuid();
			var targetPK = ZGuid.NewZGuid();

			var result1 = Factory.New<PatternMatchingResult>();
			var result2 = Factory.New<PatternMatchingResult>();
			var result3 = Factory.New<PatternMatchingResult>();
			result1.PMT_MasterPK = result2.PMT_MasterPK = result3.PMT_MasterPK = masterPK;
			result1.PMT_TargetPK = result2.PMT_TargetPK = result3.PMT_TargetPK = targetPK;
			result1.PMT_Status = result2.PMT_Status = result3.PMT_Status = "TIG";
			result1.PMT_FoundTimeUtc = ZDateTime.BrettsBirthday;
			result2.PMT_FoundTimeUtc = ZDateTime.BrettsBirthday.AddDays(-1);
			result3.PMT_FoundTimeUtc = ZDateTime.BrettsBirthday.AddDays(-2);
			result1.PMT_GS_NKExcludeBy = "AAA";
			result2.PMT_GS_NKExcludeBy = "BBB";
			result3.PMT_GS_NKExcludeBy = "CCC";

			AssertEquals("OperatorList has 3 staffs", "CCC, BBB, AAA", result1.ExcludedByList);
			AssertEquals("OperatorList has 3 staffs", "", result1.ExcludedByForEveryone);

			result1.PMT_Status = "PIG";
			AssertEquals("OperatorList has 3 staffs", "CCC, BBB", result1.ExcludedByList);
			AssertEquals("OperatorList has 3 staffs", "AAA", result1.ExcludedByForEveryone);
		}

		public void TestExcludeByAndExcluedeByForEveryOneInBothWay()
		{
			var masterPK = ZGuid.NewZGuid();
			var targetPK = ZGuid.NewZGuid();
			var result1 = Factory.New<PatternMatchingResult>();
			var result2 = Factory.New<PatternMatchingResult>();
			result1.PMT_MasterPK = result2.PMT_TargetPK = masterPK;
			result1.PMT_TargetPK = result2.PMT_MasterPK = targetPK;
			result1.PMT_Status = PatternMatchingResult.StatusCodes.TemporaryIgnore;
			result1.PMT_GS_NKExcludeBy = "AAA";
			result2.PMT_Status = PatternMatchingResult.StatusCodes.PermanentIgnore;
			result2.PMT_GS_NKExcludeBy = "BBB";

			AssertEquals("Should exclude in both way", "AAA", result1.ExcludedByList);
			AssertEquals("Should exclude in both way", "AAA", result2.ExcludedByList);

			AssertEquals("Should exclude in both way", "BBB", result1.ExcludedByForEveryone);
			AssertEquals("Should exclude in both way", "BBB", result2.ExcludedByForEveryone);
		}

		public void TestCreatePatternMatchingResultConcurrencyPolicyIsIgnored()
		{
			//Arrange
			var master = Factory.NewWithValidTestData<OrgHeader>();

			//Act
			var patternMatchingResult = PatternMatchingResult.CreatePatternMatchingResult(OrgHeaderSchema.Constants.Prefix, master.PK, Factory, PatternMatchingResult.StatusCodes.PotentialDuplicate);

			//Assert
			AssertConcurrencyPolicy(ConcurrencyPolicy.Ignore, patternMatchingResult, PatternMatchingResultSchema.PMT_MasterPK.Name);
			AssertConcurrencyPolicy(ConcurrencyPolicy.Ignore, patternMatchingResult, PatternMatchingResultSchema.PMT_MasterTableCode.Name);
			AssertConcurrencyPolicy(ConcurrencyPolicy.Ignore, patternMatchingResult, PatternMatchingResultSchema.PMT_Status.Name);
			AssertConcurrencyPolicy(ConcurrencyPolicy.Ignore, patternMatchingResult, PatternMatchingResultSchema.PMT_FoundTimeUtc.Name);
			AssertConcurrencyPolicy(ConcurrencyPolicy.Ignore, patternMatchingResult, PatternMatchingResultSchema.PMT_ScorePercent.Name);
		}

		public void AssertConcurrencyPolicy(ConcurrencyPolicy expectedPolicy, BusinessObject bizo, string property)
		{
			var isAsExpected = bizo.FindPropertyInfo(property).ConcurrencyPolicy == expectedPolicy;
			AssertEquals(true, isAsExpected);
		}

		PatternMatchingResult Result;
	}
}
