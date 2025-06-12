SELECT
	SourceIP AS SourceIP,
	DestinationIP  AS DestinationIP,
	SUM(EgressBytes) AS EgressBytes,
	MIN(TimeStamp) AS MinTimeStamp,
	MAX(TimeStamp) AS MaxTimeStamp
  FROM [SolarwindsRestoreOct21toApr22].[dbo].[NetFlowFlows_ViewByConversation]
  where  [Port] = 1433
  and [NodeID] = 277
  and @startUTC < TimeStamp AND TimeStamp <= @endUTC
  Group by SourceIP, DestinationIP

