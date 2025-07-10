CREATE TYPE SourcePKList AS TABLE (
	PK uniqueidentifier,
	TablePrefix VARCHAR(3) NOT NULL
)
GO
CREATE PROCEDURE [dbo].RecordDataSetChangeHistory(
	@sourcePK SourcePKList READONLY
)
AS
BEGIN
	DECLARE @transactionId bigint, @startTime datetime;
	SELECT @startTime = create_date FROM sys.databases WHERE name = 'tempdb';
	SET @transactionId = CURRENT_TRANSACTION_ID();

	MERGE DataSetChangeHistory
	USING @sourcePK
	ON DCH_ParentPK = PK AND DCH_ParentCode = TablePrefix AND
		DCH_TransactionIdStartTime = @startTime AND DCH_TransactionId = @transactionId
	WHEN MATCHED
		THEN UPDATE SET DCH_ChangeTime = SYSUTCDATETIME(), DCH_User = dbo.GetUserId()
	WHEN NOT MATCHED
		THEN INSERT (DCH_PK, DCH_ParentPK, DCH_ParentCode, DCH_TransactionIdStartTime, DCH_TransactionId)
		VALUES (newid(), PK, TablePrefix, @startTime, @transactionId);
END
