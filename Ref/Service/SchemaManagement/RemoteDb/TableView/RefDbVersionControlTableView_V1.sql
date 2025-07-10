CREATE VIEW RefDbVersionControlTableView_V1 AS
SELECT [RVC_PK],
[RVC_DataSet],
[RVC_LastUpdatedUtc],
[RVC_DataSetGet],
[RVC_ClientID],
[RVC_UpdaterVersion]
FROM RefDbVersionControl