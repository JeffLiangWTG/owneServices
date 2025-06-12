CREATE PROCEDURE [edi].[CW1SystemsBehindOnSTLMilestones]
AS
	DECLARE @dateToCheck DATE = DATEADD(day, -1, DATEADD(hour, -4, GETDATE()));
	DECLARE @periodToCheck int = YEAR(@dateToCheck) * 100 + MONTH(@dateToCheck);
	DECLARE @expectedMilestones int = DAY(@dateToCheck);
	DECLARE @previousMonth DATE = DATEADD(month, -1, @dateToCheck);
	DECLARE @previousPeriod int = YEAR(@previousMonth) * 100 + MONTH(@previousMonth);

	WITH HadMilestonesLastMonthButNotThisMonth as
	(
		SELECT Distinct 0 ML_Milestones, ML_EnterpriseCode, ML_ServerCode, ML_HostedLocation, ML_DatabaseNumber, ML_IsActive, ML_IsTeardownInProgress
		FROM edi.CW1STLMilestones(@previousPeriod) LM
		WHERE NOT EXISTS (SELECT TOP(1) * FROM edi.CW1STLMilestones(@periodToCheck) TM WHERE LM.ML_DatabaseNumber = TM.ML_DatabaseNumber)
	),
	Milestones as
	(
		SELECT * FROM edi.CW1STLMilestones(@periodToCheck)
		UNION
		SELECT * FROM HadMilestonesLastMonthButNotThisMonth
	)

	SELECT ML_Milestones MB_Milestones, ML_EnterpriseCode MB_EnterpriseCode, ML_ServerCode MB_ServerCode, ML_HostedLocation MB_HostedLocation
	FROM Milestones MB
	WHERE ML_Milestones < @expectedMilestones AND ML_IsActive = 1 AND ML_IsTeardownInProgress = 0 AND EXISTS (SELECT TOP(1) * FROM edi.CW1STLMilestones(@previousPeriod) LM WHERE MB.ML_DatabaseNumber = LM.ML_DatabaseNumber)
