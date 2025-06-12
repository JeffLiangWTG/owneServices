CREATE PROCEDURE [edi].[SyncFakeLicencesForTesting]
AS
BEGIN TRY
	BEGIN TRAN;
	DECLARE @LicenceDataNumMax int = 0;

	SELECT TOP 1 @LicenceDataNumMax = DatabaseNumber FROM edi.LicenceDatabaseCodeHistory
	ORDER BY DatabaseNumber DESC;

	WITH MissingLicenceTransactions AS
	(
		SELECT
			ROW_NUMBER() OVER (PARTITION BY LEFT(edi.StagingBatch.TX_ClientID, 3) + RIGHT(edi.StagingBatch.TX_ClientID, 3) ORDER BY TX_ID) AS OccurrenceNum, 
			*
		FROM edi.StagingBatch 
		WHERE TX_ID NOT IN (
			SELECT edi.StagingBatch.TX_ID 
			FROM edi.StagingBatch
			JOIN edi.LicenceDatabaseCodeHistory
			ON
				edi.LicenceDatabaseCodeHistory.EnterpriseCode = LEFT(edi.StagingBatch.TX_ClientID, 3)
				AND edi.LicenceDatabaseCodeHistory.ServerCode = RIGHT(edi.StagingBatch.TX_ClientID, 3)
		)
		AND LEN(TX_ClientID) = 9
		AND TX_ClientID != '?????????'
	),
	MissingLicenceHistories as
	(
		SELECT 
			LEFT(MissingLicenceTransactions.TX_ClientID, 3) as EnterpriseCode, 
			RIGHT(MissingLicenceTransactions.TX_ClientID, 3) as ServerCode,
			ROW_NUMBER() OVER (ORDER BY TX_ID) AS RecordNum
		FROM MissingLicenceTransactions WHERE OccurrenceNum = 1
	)
	INSERT edi.LicenceDatabaseCodeHistory(SystemId, DatabaseNumber, EnterpriseCode, ServerCode, ValidFromUtc, HostedLocation, Product, IsActive, LicenceType)
	SELECT edi.Base27Encode(@LicenceDataNumMax + RecordNum), @LicenceDataNumMax + RecordNum, EnterpriseCode, ServerCode, GETDATE(), 'SYD', 'CW1', 1, 'PRD'
	FROM MissingLicenceHistories;


	WITH MissingCompanyTransactions AS
	(
		SELECT
			ROW_NUMBER() OVER (PARTITION BY SB.TX_ClientID ORDER BY SB.TX_ClientID) AS OccurrenceNum,
			SB.TX_ClientID, 
			LH.DatabaseNumber, 
			HighestExistingCompany.CompanyNumber AS MaxCompanyNumber
		FROM edi.StagingBatch SB
		INNER JOIN edi.LicenceDatabaseCodeHistory LH 
		ON
			LH.EnterpriseCode = LEFT(SB.TX_ClientID, 3)
			AND LH.ServerCode = RIGHT(SB.TX_ClientID, 3)
		OUTER APPLY
		(
			SELECT TOP 1 CompanyNumber 
			FROM edi.ClientCompanyCodeHistory CH 
			WHERE CH.DatabaseNumber = LH.DatabaseNumber
			ORDER BY CompanyNumber DESC
		) AS HighestExistingCompany
		WHERE TX_ID NOT IN (
			SELECT SB2.TX_ID 
			FROM edi.StagingBatch SB2
			INNER JOIN edi.LicenceDatabaseCodeHistory LH2 ON
				LH2.EnterpriseCode = LEFT(SB2.TX_ClientID, 3)
				AND LH2.ServerCode = RIGHT(SB2.TX_ClientID, 3)
			INNER JOIN edi.ClientCompanyCodeHistory CH2 ON
				CH2.CompanyCode = SUBSTRING(SB2.TX_ClientID, 4, 3)
				AND LH2.DatabaseNumber = CH2.DatabaseNumber
			WHERE SUBSTRING(SB2.TX_ClientID, 4, 3) <> '???'
			UNION
			SELECT SB2.TX_ID 
			FROM edi.StagingBatch SB2
			INNER JOIN edi.LicenceDatabaseCodeHistory LH2 ON 
				LH2.EnterpriseCode = LEFT(SB2.TX_ClientID, 3)
				AND LH2.ServerCode = RIGHT(SB2.TX_ClientID, 3)
			WHERE SUBSTRING(SB2.TX_ClientID, 4, 3) = '???'
				AND EXISTS (SELECT 1 FROM edi.ClientCompanyCodeHistory CH2 WHERE LH2.DatabaseNumber = CH2.DatabaseNumber)
		)
	),
	MissingCompanyHistories AS
	(
		SELECT
		CASE 
			WHEN SUBSTRING(TX_ClientID, 4, 3) = '???' THEN 'AAA' 
			ELSE SUBSTRING(TX_ClientID, 4, 3) 
		END AS CompanyCode, 
		DatabaseNumber,
		CASE
			WHEN SUBSTRING(TX_ClientID, 4, 3) = '???' THEN COALESCE(MaxCompanyNumber, 0) + 1
			ELSE ROW_NUMBER() OVER (PARTITION BY DatabaseNumber ORDER BY TX_ClientID) + COALESCE(MaxCompanyNumber, 0)
		END AS CompanyNumber
		FROM MissingCompanyTransactions
		WHERE OccurrenceNum = 1
	)
	SELECT * INTO #TempMissingCompanyHistories FROM MissingCompanyHistories

	INSERT INTO edi.ClientCompany(DatabaseNumber, CompanyNumber, ValidFromUtc, LCC_PK, CountryCode)
	SELECT DatabaseNumber, CompanyNumber , GETDATE(), NEWID(), 'TS'
	FROM #TempMissingCompanyHistories T

	INSERT edi.ClientCompanyCodeHistory(DatabaseNumber, CompanyNumber, CompanyCode, ValidFromUtc)
	SELECT T.DatabaseNumber, CompanyNumber, CompanyCode, GETDATE()
	FROM #TempMissingCompanyHistories T

	DROP TABLE #TempMissingCompanyHistories
	COMMIT
END TRY
BEGIN CATCH
	ROLLBACK;
	THROW;
END CATCH
RETURN 0
