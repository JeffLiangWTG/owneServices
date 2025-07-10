CREATE VIEW RefUNLOCOUtcOffsetTableView_V1 AS
SELECT [RLO_PK],
[RLO_RL_NKCode],
[RLO_StartTimeUtc],
[RLO_EndTimeUtc],
[RLO_OffsetMinutesFromUtc],
(CONVERT([DECIMAL](5,2),[RLO_OffsetMinutesFromUtc]/(60.0))) AS [RLO_OffsetFromUtc]
FROM RefUNLOCOUtcOffset
