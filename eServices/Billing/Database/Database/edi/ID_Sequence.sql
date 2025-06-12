-- ID sequence for Chargeable and Usage records not created from Staging table records and so needing an ID
-- Sequence decrements from -1 so as not to class with Staging TX_ID
CREATE SEQUENCE [edi].[ID_Sequence]
		AS BIGINT
		START WITH -1
		INCREMENT BY -1
		NO MAXVALUE
		NO CYCLE
		CACHE 1000
