CREATE PROCEDURE [edi].[ProcessWGR] AS
BEGIN
	WITH H as
	(
	SELECT *,
		ROW_NUMBER() OVER(PARTITION BY DatabaseNumber ORDER BY ValidFromUtc DESC) AS LicenseRow
		FROM edi.LicenceDatabaseCodeHistory
	)

	UPDATE SB
	SET ProcessingStatus = 255
	FROM edi.StagingBatch SB INNER JOIN H ON SB.DatabaseNumber = H.DatabaseNumber AND H.LicenseRow = 1
	WHERE SB.TX_Category = 'WGR' AND SB.TX_PriceItemCode = 'WGR' AND H.LicenceType <> 'PRD' 
	
	RETURN 0
END
