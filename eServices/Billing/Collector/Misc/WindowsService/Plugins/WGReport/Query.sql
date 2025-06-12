SET NOCOUNT ON;

;WITH CTE AS
(
    SELECT 
        CONVERT(VARCHAR(MAX), IncrementId) AS IncrementId,
        RecordedTime, 
        ServerName,
        DatabaseName,
        UsageType,
        CONVERT(INT, CPUUsageSeconds) AS CPUUsageSeconds,
        CONVERT(VARCHAR(MAX), SampleSeconds) AS SampleSeconds,
        SourceOfQuery,
        ROW_NUMBER() OVER(PARTITION BY ServerName, DatabaseName ORDER BY RecordedTime DESC) AS RowNum,
        MAX(CONVERT(INT, CPUUsageSeconds)) OVER(PARTITION BY ServerName, DatabaseName) AS MaxCPUUsageSeconds
    FROM dbo.fn_CPUUsage(@startUTC, @endUTC)
	WHERE CPUUsageSeconds >= 0
)
SELECT 
    IncrementId,
    RecordedTime, 
    ServerName,
    DatabaseName,
    UsageType,
    CPUUsageSeconds,
    SampleSeconds,
    SourceOfQuery
FROM CTE
WHERE 
    CPUUsageSeconds > 0
UNION ALL
SELECT 
    IncrementId,
    RecordedTime, 
    ServerName,
    DatabaseName,
    UsageType,
    CPUUsageSeconds,
    SampleSeconds,
    SourceOfQuery
FROM CTE
WHERE 
    MaxCPUUsageSeconds = 0 AND RowNum = 1
