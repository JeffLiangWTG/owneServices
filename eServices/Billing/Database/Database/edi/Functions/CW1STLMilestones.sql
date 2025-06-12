CREATE FUNCTION [edi].[CW1STLMilestones](@period int)
RETURNS TABLE AS
RETURN
(
	WITH MostRecentHistory  as
	(
		select EnterpriseCode, DatabaseNumber, ServerCode, HostedLocation, IsActive, IsTeardownInProgress
		FROM dbo.GetLatestLicense()
	),
	DistinctMilestonesThisMonth as
	(
	  SELECT Distinct CH_Reference1, CH_Reference2, MostRecentHistory.EnterpriseCode, MostRecentHistory.ServerCode, MostRecentHistory.HostedLocation, MostRecentHistory.DatabaseNumber, MostRecentHistory.IsActive, MostRecentHistory.IsTeardownInProgress
	  FROM edi.Chargeable CH
	  INNER JOIN MostRecentHistory on MostRecentHistory.DatabaseNumber = CH.CH_DatabaseNumber
	  WHERE CH_Period = @period AND CH_Category = 'STL' AND CH_PriceItemCode = 'STL'
	)

	SELECT Count(*) ML_Milestones, EnterpriseCode ML_EnterpriseCode, ServerCode ML_ServerCode, HostedLocation ML_HostedLocation, DatabaseNumber ML_DatabaseNumber, IsActive ML_IsActive, IsTeardownInProgress ML_IsTeardownInProgress
	FROM DistinctMilestonesThisMonth
	GROUP BY EnterpriseCode, ServerCode, HostedLocation, DatabaseNumber, IsActive, IsTeardownInProgress
)
