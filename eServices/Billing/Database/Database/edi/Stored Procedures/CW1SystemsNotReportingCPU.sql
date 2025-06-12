CREATE PROCEDURE [edi].[CW1SystemsNotReportingCPU]
AS
	DECLARE @dateToCheck DATE = DATEADD(day, -1, DATEADD(hour, -4, GETDATE()));
	DECLARE @periodToCheck int = YEAR(@dateToCheck) * 100 + MONTH(@dateToCheck);
	DECLARE @previousPeriod int = edi.GetPreviousPeriod(@periodToCheck);
	DECLARE @nextPeriod int = edi.GetNextPeriod(@periodToCheck);
	DECLARE @expectedMilestones int = DAY(@dateToCheck);
	DECLARE @twoDaysAgo DATE = DATEADD(day, -2, GETUTCDATE());

	WITH databasesReportingCPU AS
	(
		SELECT Distinct US_DatabaseNumber
		FROM edi.Usage
		WHERE US_Period In (@previousPeriod, @periodToCheck, @nextPeriod) AND US_Category = 'WGR' AND US_PriceItemCode = 'WGR'
		AND US_DatabaseNumber != 0 AND US_ServiceOccuredUtc >= @twoDaysAgo
	),
	WisetechHostedDBsWithExpectedMilestones AS
	(
		SELECT currMil.*
		FROM edi.CW1STLMilestones(@periodToCheck) currMil
		lEFT JOIN edi.CW1STLMilestones(@previousPeriod) prevMil ON prevMil.ML_DatabaseNumber = currMil.ML_DatabaseNumber
		WHERE currMil.ML_HostedLocation != 'NCW' AND currMil.ML_Milestones >= @expectedMilestones AND currMil.ML_Milestones + prevMil.ML_Milestones > 1
	)

	SELECT ML_EnterpriseCode, ML_ServerCode, ML_HostedLocation
	from WisetechHostedDBsWithExpectedMilestones
	WHERE ML_IsActive = 1 AND ML_IsTeardownInProgress = 0 
	AND NOT EXISTS (SELECT * FROM databasesReportingCPU WHERE US_DatabaseNumber = ML_DatabaseNumber)
