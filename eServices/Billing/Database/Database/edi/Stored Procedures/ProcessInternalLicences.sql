CREATE PROCEDURE [edi].[ProcessInternalLicences]
	@period int
AS

SET NOCOUNT ON;

WITH IH as
(
select *,
	ROW_NUMBER() OVER(PARTITION BY DatabaseNumber ORDER BY ValidFromUtc DESC) AS InternalLicenseRow
	from edi.InternalLicenceDatabaseCodeHistory
)

UPDATE SB
SET
ProcessingStatus = 255,
DatabaseNumber = IH.DatabaseNumber
FROM edi.StagingBatch SB
INNER JOIN IH ON
	SB.TX_ClientNumber = IH.TenantID AND (TX_Category = IH.Category OR (IH.Category = '' AND TX_Category = IH.Product))
WHERE IH.InternalLicenseRow = 1
AND IH.TenantID != ''
AND edi.ClientNumberIsTenantId(TX_Category) = 1

RETURN 0
