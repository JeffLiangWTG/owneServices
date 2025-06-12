CREATE PROCEDURE [edi].[ProcessClientNumberByTenantId]
AS
	update edi.StagingBatch
	set DatabaseNumber = NewDatabaseNumber,
		CompanyNumber = 1
	from edi.StagingBatch staging
	cross apply
	(
		select top 1 NewDatabaseNumber = dh.DatabaseNumber
		from edi.LicenceDatabaseCodeHistory dh
		where TX_ClientNumber = TenantId AND (TX_Category = dh.Category OR (dh.Category = '' AND TX_Category = dh.Product))
		order by dh.ValidFromUtc desc
	) a
	where DatabaseNumber = 0 and edi.ClientNumberIsTenantId(TX_Category) = 1;

RETURN 0
