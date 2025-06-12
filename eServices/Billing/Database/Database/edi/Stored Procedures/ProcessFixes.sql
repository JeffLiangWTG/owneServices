CREATE PROCEDURE [edi].[ProcessFixes]
AS

SET NOCOUNT ON;

UPDATE edi.StagingBatch
	SET TX_PriceItemCode =
		CASE TX_PriceItemCode
			WHEN 'PRO' THEN 'USP'
			WHEN 'REC' THEN 'URC'
			WHEN 'ECI' THEN 'ECM'
			WHEN 'ACE' THEN 'ACX'
			WHEN 'ACI' THEN 'ACM'
			WHEN 'ACT' THEN 'ACN'
			WHEN 'CFS' THEN 'CFM'
			WHEN 'ORD' THEN 'ORM'
			WHEN 'WOR' THEN 'WBM'
		END
	WHERE edi.StagingBatch.TX_ReportingSource = 'ENT'
	AND edi.StagingBatch.TX_Category = 'STL'
	AND edi.StagingBatch.TX_PriceItemCode in ('PRO','REC','ECI','ACE','ACI','ACT','CFS','ORD','WOR');
	
UPDATE edi.StagingBatch
	SET TX_PriceItemCode =
		CASE TX_PriceItemCode
			WHEN 'ICB' THEN 'IFB'
			WHEN 'ICG' THEN 'IFG'
		END
	WHERE edi.StagingBatch.TX_ReportingSource = 'ENT'
	AND edi.StagingBatch.TX_Category = 'STL'
	AND edi.StagingBatch.TX_PriceItemCode in ('ICB', 'ICG')
	AND edi.StagingBatch.TX_Reference2 = ''
	AND edi.StagingBatch.TX_Reference4 is null;

UPDATE edi.StagingBatch
	SET TX_BillableCount = 1
	WHERE edi.StagingBatch.TX_ReportingSource = 'ENT'
	AND edi.StagingBatch.TX_Category = 'STL'
	AND edi.StagingBatch.TX_PriceItemCode = 'USR'
	AND edi.StagingBatch.TX_BillableCount > 1;


DELETE edi.StagingBatch
WHERE edi.StagingBatch.TX_ReportingSource = 'ENT'
	AND edi.StagingBatch.TX_Category = 'STL'
	AND edi.StagingBatch.TX_PriceItemCode = 'USR'
	AND ((edi.StagingBatch.TX_Reference2 like '%hosted%(edi.support)')
		OR (edi.StagingBatch.TX_Reference2 = 'CargoWise One Support (CW1Support)')
		OR (edi.StagingBatch.TX_Reference2 = 'CargoWise Support (CWSupport)')
		OR (edi.StagingBatch.TX_Reference2 like '%hosting%(edi.support)')
		OR (edi.StagingBatch.TX_Reference2 like '%wisecloud%(edi.')
		OR (edi.StagingBatch.TX_Reference2 like '%wisecloud%(edi.s)')
		OR (edi.StagingBatch.TX_Reference2 like '%wisecloud%(edi.su)')
		OR (edi.StagingBatch.TX_Reference2 like '%wisecloud%(edi.sup)')
		OR (edi.StagingBatch.TX_Reference2 like '%wisecloud%(edi.supp)')
		OR (edi.StagingBatch.TX_Reference2 like '%wisecloud%(edi.suppo)')
		OR (edi.StagingBatch.TX_Reference2 like '%wisecloud%(edi.suppor)')
		OR (edi.StagingBatch.TX_Reference2 like '%wisecloud%(edi.support)')
		OR (edi.StagingBatch.TX_Reference2 like 'edi%support%(edi.support)')
		OR (edi.StagingBatch.TX_Reference2 like '%support%(hosting.support)')
		OR (edi.StagingBatch.TX_Reference2 like 'this is for%support%(edi.support)')
		OR (edi.StagingBatch.TX_Reference2 like 'this is for%support%(glow.support)'));


RETURN 0
