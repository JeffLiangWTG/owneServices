using CargoWise.Data;
using Enterprise.Integration;

namespace Enterprise.MasterData.ServiceTask.Deduplication
{
	static class DeduplicationQueuePreProcessor
	{
		const string sqlText = @"
DECLARE @LargeOrgContactsThreshold INT = 100;
DECLARE @NumberOfEmailAddressesThreshold INT = 20;
DECLARE @NumberOfPhoneNumbersThreshold INT = 50;
DECLARE @MaxDomainUniquenessRatio FLOAT = 0.6;
DECLARE @MinEmailUniquenessRatio FLOAT = 0.8;
DECLARE @MinPhoneUniquenessRatio FLOAT = 0.8;
DECLARE @MinNonEmptyFieldsRatio FLOAT = 1.0;
DECLARE @MaxEmptyContactsRatio FLOAT = 0.9;

DECLARE @DirtyQUEs TABLE
(
	OrgPk UNIQUEIDENTIFIER
);

WITH Data1 AS 
(
	SELECT OC_OH AS OrgPK, 
		COUNT(1) AS ContactCount,
		SUM(CASE WHEN OC_Phone = '' THEN 0 ELSE 1 END) AS PhoneCount,
		COUNT(DISTINCT OC_Phone) - MAX(CASE WHEN OC_Phone = '' THEN 1 ELSE 0 END) AS DistinctPhones,
		SUM(CASE WHEN OC_Email = '' THEN 0 ELSE 1 END) AS EmailCount,
		COUNT(DISTINCT RIGHT(OC_Email, LEN(OC_Email) - CHARINDEX('@', OC_Email))) - MAX(CASE WHEN OC_Email = '' THEN 1 ELSE 0 END) AS DistinctDomains,
		COUNT(DISTINCT OC_Email) - MAX(CASE WHEN OC_Email = '' THEN 1 ELSE 0 END) AS DistinctEmails,
		SUM(CASE WHEN OC_Title = '' THEN 0 ELSE 1 END) AS TitleCount,
		SUM(CASE WHEN OC_Mobile = '' THEN 0 ELSE 1 END) AS MobileCount,
		SUM(CASE WHEN OC_Fax = '' THEN 0 ELSE 1 END) AS FaxCount,
		SUM(CASE WHEN OC_Mobile = '' AND OC_Title = '' AND OC_Fax = '' AND OC_Phone = '' AND OC_Email = '' THEN 1 ELSE 0 END) AS EmptyContacts
	FROM dbo.OrgContact
	JOIN dbo.PatternMatchingResult
	ON PMT_MasterPK = OC_OH
	WHERE PMT_Status = 'QUE'	 
	GROUP BY OC_OH
),
Data2 AS
(
	SELECT OrgPK FROM Data1
	WHERE
	(ContactCount > @LargeOrgContactsThreshold AND 
		(
			IIF(PhoneCount > @NumberOfPhoneNumbersThreshold, CAST(DistinctPhones AS FLOAT) / CAST(PhoneCount AS FLOAT), 1) < @MinPhoneUniquenessRatio OR
			IIF(EmailCount > @NumberOfEmailAddressesThreshold, CAST(DistinctDomains AS FLOAT) / CAST(EmailCount AS FLOAT), 0) > @MaxDomainUniquenessRatio OR
			IIF(EmailCount > @NumberOfEmailAddressesThreshold, CAST(DistinctEmails AS FLOAT) / CAST(EmailCount AS FLOAT), 0) < @MinEmailUniquenessRatio	OR
			CAST((PhoneCount + EmailCount + TitleCount + MobileCount + FaxCount) AS FLOAT) / CAST(ContactCount AS FLOAT) < @MinNonEmptyFieldsRatio OR			
			CAST(EmptyContacts AS FLOAT) / CAST(ContactCount AS FLOAT) > @MaxEmptyContactsRatio
		)
	)
)

DELETE FROM dbo.PatternMatchingResult
OUTPUT DELETED.PMT_MasterPK INTO @DirtyQUEs
WHERE PMT_MasterPK in (SELECT * FROM Data2)

INSERT INTO dbo.PatternMatchingResult (PMT_PK, PMT_MasterPK, PMT_MasterTableCode,  PMT_TargetPK, PMT_TargetTableCode, PMT_FoundTimeUtc, PMT_Status, PMT_GS_NKExcludeBy, PMT_ScorePercent)
SELECT 
	NEWID(), t.OrgPK, 'OH', NULL, '', getutcdate(), 'EXC', '', 0 
FROM 
	(SELECT DISTINCT OrgPK FROM @DirtyQUEs) t

UPDATE dbo.PatternMatchingResult
SET PMT_Status = 'QUP'
WHERE PMT_Status = 'QUE'
";

		const string deleteDirtyPatternMatchingResultsSqlText = @"
DELETE FROM dbo.PatternMatchingResult WHERE PMT_Status = 'QUP' AND PMT_MasterTableCode = 'OH' AND NOT EXISTS (SELECT 0 FROM dbo.OrgHeader WHERE OH_PK = PMT_MasterPK)
DELETE FROM dbo.PatternMatchingResult WHERE PMT_Status = 'QUP' AND PMT_MasterTableCode = 'PER' AND NOT EXISTS (SELECT 0 FROM dbo.GlbPerson WHERE PER_PK = PMT_MasterPK)";

		internal static void Process(ILogger logger)
		{
			Db.Connection.ExecuteNonQuery(sqlText);
			Db.Connection.ExecuteNonQuery(deleteDirtyPatternMatchingResultsSqlText);
		}
	}
}
