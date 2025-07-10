using System;
using System.Data;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using Enterprise.StabilityChecker;

namespace Enterprise.Customs.ServiceTasks.Testing
{
	class NumberRangeThresholdRunOutCheckerTest : TestCaseWithFactory
	{
		public void TestCheckNumberRangeThresholdRunOut()
		{
			var connection = ((IDbConnected)Factory).Connection;

			var sqlText = @"
INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_RX_NKLocalCurrency, GC_RN_NKCountryCode, GC_Name, GC_IsActive) VALUES (@Company1PK, 'GC1', 'USD', 'US', 'COMPANY 1', 1)
INSERT dbo.GlbBranch (GB_PK, GB_GC, GB_Code, GB_BranchName, GB_IsActive) VALUES (@Company1Branch1PK, @Company1PK, 'GB1', 'BRANCH 1', 1)
INSERT dbo.GlbBranch (GB_PK, GB_GC, GB_Code, GB_BranchName, GB_IsActive) VALUES (@Company1Branch2PK, @Company1PK, 'GB2', 'BRANCH 2', 1)
INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_RX_NKLocalCurrency, GC_RN_NKCountryCode, GC_Name, GC_IsActive) VALUES (@Company2PK, 'GC2', 'USD', 'US', 'COMPANY 2', 1)
INSERT dbo.GlbBranch (GB_PK, GB_GC, GB_Code, GB_BranchName, GB_IsActive) VALUES (@Company2Branch1PK, @Company2PK, 'GB4', 'BRANCH 4', 1)
INSERT dbo.GlbBranch (GB_PK, GB_GC, GB_Code, GB_BranchName, GB_IsActive) VALUES (@Company2Branch2PK, @Company2PK, 'GB5', 'BRANCH 4', 1)
INSERT dbo.GlbBranch (GB_PK, GB_GC, GB_Code, GB_BranchName, GB_IsActive) VALUES (@Company2Branch3PK, @Company2PK, 'GB6', 'BRANCH 4', 0)
INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_RX_NKLocalCurrency, GC_RN_NKCountryCode, GC_Name, GC_IsActive) VALUES (@Company3PK, 'GC3', 'USD', 'US', 'COMPANY 3', 0)
INSERT dbo.GlbBranch (GB_PK, GB_GC, GB_Code, GB_BranchName, GB_IsActive) VALUES (@Company3Branch1PK, @Company3PK, 'GB7', 'BRANCH 4', 0)

INSERT dbo.StmNums (SN_Name, SN_Owner, SN_Value, SN_MinimumValue, SN_MaximumValue) VALUES ('C#AU-CEN _XJ2|1', @Company1PK, 400, 100, 500)
INSERT dbo.StmNums (SN_Name, SN_Owner, SN_Value, SN_MinimumValue, SN_MaximumValue) VALUES ('C#AU-CEN _XJ2|2', @Company1PK, 600, 600, 700)
INSERT dbo.StmNumberRange (SNR_PK, SNR_Name, SNR_Owner, SNR_ThresholdRunOutWarning) VALUES (NEWID(), 'C#AU-CEN _XJ2', @Company1PK, 100)

INSERT dbo.StmNums (SN_Name, SN_Owner, SN_Value, SN_MinimumValue, SN_MaximumValue) VALUES ('C#AU-CEN _XJ2|1', @Company1Branch1PK, 400, 100, 500)
INSERT dbo.StmNums (SN_Name, SN_Owner, SN_Value, SN_MinimumValue, SN_MaximumValue) VALUES ('C#AU-CEN _XJ2|2', @Company1Branch1PK, 600, 600, 700)
INSERT dbo.StmNumberRange (SNR_PK, SNR_Name, SNR_Owner, SNR_ThresholdRunOutWarning) VALUES (NEWID(), 'C#AU-CEN _XJ2', @Company1Branch1PK, 300)

INSERT dbo.StmNums (SN_Name, SN_Owner, SN_Value, SN_MinimumValue, SN_MaximumValue) VALUES ('C#AU-CEN _XJ3|1', @Company1Branch3PK, 400, 100, 500)
INSERT dbo.StmNumberRange (SNR_PK, SNR_Name, SNR_Owner, SNR_ThresholdRunOutWarning) VALUES (NEWID(), 'C#AU-CEN _XJ3', @Company1Branch3PK, 200)

INSERT dbo.StmNums (SN_Name, SN_Owner, SN_Value, SN_MinimumValue, SN_MaximumValue) VALUES ('C#AU-CEN _XJ2|1', @Company1Branch2PK, 400, 100, 500)
INSERT dbo.StmNumberRange (SNR_PK, SNR_Name, SNR_Owner, SNR_ThresholdRunOutWarning) VALUES (NEWID(), 'C#AU-CEN _XJ2', @Company1Branch2PK, 101)

INSERT dbo.StmNums (SN_Name, SN_Owner, SN_Value, SN_MinimumValue, SN_MaximumValue) VALUES ('C#US-CEN _XJ2|1', @Company2PK, 400, 100, 500)
INSERT dbo.StmNums (SN_Name, SN_Owner, SN_Value, SN_MinimumValue, SN_MaximumValue) VALUES ('C#US-CEN _XJ2|2', @Company2PK, 600, 600, 700)
INSERT dbo.StmNumberRange (SNR_PK, SNR_Name, SNR_Owner, SNR_ThresholdRunOutWarning) VALUES (NEWID(), 'C#US-CEN _XJ2', @Company2PK, 300)

INSERT dbo.StmNums (SN_Name, SN_Owner, SN_Value, SN_MinimumValue, SN_MaximumValue, SN_CanRollover) VALUES ('C#US-CEN _XJ2|1', @Company2Branch1PK, 501, 100, 500, 0)
INSERT dbo.StmNums (SN_Name, SN_Owner, SN_Value, SN_MinimumValue, SN_MaximumValue) VALUES ('C#US-CEN _XJ2|2', @Company2Branch1PK, 600, 600, 700)
INSERT dbo.StmNumberRange (SNR_PK, SNR_Name, SNR_Owner, SNR_ThresholdRunOutWarning) VALUES (NEWID(), 'C#US-CEN _XJ2', @Company2Branch1PK, 100)

INSERT dbo.StmNums (SN_Name, SN_Owner, SN_Value, SN_MinimumValue, SN_MaximumValue, SN_CanRollover) VALUES ('C#US-ABI _XJ2|1', @Company2Branch2PK, 501, 100, 500, 0)
INSERT dbo.StmNums (SN_Name, SN_Owner, SN_Value, SN_MinimumValue, SN_MaximumValue) VALUES ('C#US-ABI _XJ2|2', @Company2Branch2PK, 600, 600, 700)
INSERT dbo.StmNumberRange (SNR_PK, SNR_Name, SNR_Owner, SNR_ThresholdRunOutWarning) VALUES (NEWID(), 'C#US-ABI _XJ2', @Company2Branch2PK, 101)

INSERT dbo.StmNums (SN_Name, SN_Owner, SN_Value, SN_MinimumValue, SN_MaximumValue, SN_CanRollover) VALUES ('C#US-CEN _XJ4|1', @Company2Branch2PK, 301, 100, 300, 0)
INSERT dbo.StmNums (SN_Name, SN_Owner, SN_Value, SN_MinimumValue, SN_MaximumValue) VALUES ('C#US-CEN _XJ4|2', @Company2Branch2PK, 601, 600, 700)
INSERT dbo.StmNumberRange (SNR_PK, SNR_Name, SNR_Owner, SNR_ThresholdRunOutWarning) VALUES (NEWID(), 'C#US-CEN _XJ4', @Company2Branch2PK, 100)

INSERT dbo.StmNums (SN_Name, SN_Owner, SN_Value, SN_MinimumValue, SN_MaximumValue, SN_CanRollover) VALUES ('C#US-CEN _XJ5|1', @Company2Branch3PK, 301, 100, 300, 0)
INSERT dbo.StmNums (SN_Name, SN_Owner, SN_Value, SN_MinimumValue, SN_MaximumValue) VALUES ('C#US-CEN _XJ5|2', @Company2Branch3PK, 601, 600, 700)
INSERT dbo.StmNumberRange (SNR_PK, SNR_Name, SNR_Owner, SNR_ThresholdRunOutWarning) VALUES (NEWID(), 'C#US-CEN _XJ5', @Company2Branch3PK, 100)

INSERT dbo.StmNums (SN_Name, SN_Owner, SN_Value, SN_MinimumValue, SN_MaximumValue) VALUES ('C#AU-CEN _XJ6|1', @Company3PK, 299, 100, 400)
INSERT dbo.StmNumberRange (SNR_PK, SNR_Name, SNR_Owner, SNR_ThresholdRunOutWarning) VALUES (NEWID(), 'C#AU-CEN _XJ6', @Company3PK, 100)

INSERT dbo.StmNums (SN_Name, SN_Owner, SN_Value, SN_MinimumValue, SN_MaximumValue, SN_CanRollover) VALUES ('C#US-CEN _XJ6|1', @Company3Branch1PK, 301, 100, 300, 0)
INSERT dbo.StmNums (SN_Name, SN_Owner, SN_Value, SN_MinimumValue, SN_MaximumValue) VALUES ('C#US-CEN _XJ6|2', @Company3Branch1PK, 601, 600, 700)
INSERT dbo.StmNumberRange (SNR_PK, SNR_Name, SNR_Owner, SNR_ThresholdRunOutWarning) VALUES (NEWID(), 'C#US-CEN _XJ6', @Company3Branch1PK, 100)

INSERT dbo.StmNums (SN_Name, SN_Owner, SN_Value, SN_MinimumValue, SN_MaximumValue) VALUES ('C#AU-CEN _XJ5|1', @Company1PK, 299, 100, 400)
INSERT dbo.StmNumberRange (SNR_PK, SNR_Name, SNR_Owner, SNR_ThresholdRunOutWarning) VALUES (NEWID(), 'C#AU-CEN _XJ5', @Company1PK, 100)

exec dbo.FountainGetNexts 'C#AU-CEN _XJ5|1', @Company1PK, 1, 1, 99999999, 0, 1;
exec dbo.FountainGetNexts 'C#AU-CEN _XJ5|1', @Company1PK, 1, 1, 99999999, 0, 1;
exec dbo.FountainGetNexts 'C#AU-CEN _XJ5|1', @Company1PK, 1, 1, 99999999, 0, 1;
";
			var company1PK = Guid.NewGuid();
			var company1Branch1PK = Guid.NewGuid();
			var company1Branch2PK = Guid.NewGuid();
			var company1Branch3PK = Guid.NewGuid();
			var company2PK = Guid.NewGuid();
			var company2Branch1PK = Guid.NewGuid();
			var company2Branch2PK = Guid.NewGuid();
			var company2Branch3PK = Guid.NewGuid();
			var company3PK = Guid.NewGuid();
			var company3Branch1PK = Guid.NewGuid();
			var cmd = connection.Command(sqlText);
			cmd.AddParameter("@Company1PK", SqlDbType.UniqueIdentifier, company1PK);
			cmd.AddParameter("@Company1Branch1PK", SqlDbType.UniqueIdentifier, company1Branch1PK);
			cmd.AddParameter("@Company1Branch2PK", SqlDbType.UniqueIdentifier, company1Branch2PK);
			cmd.AddParameter("@Company1Branch3PK", SqlDbType.UniqueIdentifier, company1Branch3PK);
			cmd.AddParameter("@Company2PK", SqlDbType.UniqueIdentifier, company2PK);
			cmd.AddParameter("@Company2Branch1PK", SqlDbType.UniqueIdentifier, company2Branch1PK);
			cmd.AddParameter("@Company2Branch2PK", SqlDbType.UniqueIdentifier, company2Branch2PK);
			cmd.AddParameter("@Company2Branch3PK", SqlDbType.UniqueIdentifier, company2Branch3PK);
			cmd.AddParameter("@Company3PK", SqlDbType.UniqueIdentifier, company3PK);
			cmd.AddParameter("@Company3Branch1PK", SqlDbType.UniqueIdentifier, company3Branch1PK);
			cmd.ExecuteNonQuery();

			var numberRangeChecker = new NumberRangeThresholdRunOutChecker();
			var results = numberRangeChecker.Check();
			AssertEquals(1, results.Length);
			AssertEquals(StabilityResultLevel.Warning, results[0].StabilityLevel);

			AssertMultilineASCIIEquals("Thresh Hold Warning expected", @"The following number range has reach the thresh hold warning mark:
Owner='GB1 - BRANCH 1', Type='CEN', Country='AU', FountainName='XJ2', ThresholdRunOutWarning='300', Available='202'
Owner='GB2 - BRANCH 2', Type='CEN', Country='AU', FountainName='XJ2', ThresholdRunOutWarning='101', Available='101'
Owner='UNKNOWN', Type='CEN', Country='AU', FountainName='XJ3', ThresholdRunOutWarning='200', Available='101'
Owner='GC1 - COMPANY 1', Type='CEN', Country='AU', FountainName='XJ5', ThresholdRunOutWarning='100', Available='99'
Owner='GB5 - BRANCH 4', Type='ABI', Country='US', FountainName='XJ2', ThresholdRunOutWarning='101', Available='101'
Owner='GC2 - COMPANY 2', Type='CEN', Country='US', FountainName='XJ2', ThresholdRunOutWarning='300', Available='202'
Owner='GB5 - BRANCH 4', Type='CEN', Country='US', FountainName='XJ4', ThresholdRunOutWarning='100', Available='100'", results[0].Description);
		}
	}
}
