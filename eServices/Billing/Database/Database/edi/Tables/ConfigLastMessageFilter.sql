CREATE TABLE edi.ConfigLastMessageFilter
(
	ML_ID BIGINT NOT NULL IDENTITY,
	ML_Category VARCHAR(3) NOT NULL,
	ML_PriceItemCode VARCHAR(3) NOT NULL,
	ML_DeduplicateRef2 BIT NOT NULL DEFAULT 0
)

go

create unique clustered index IX_ConfigLastMessageFilter on edi.ConfigLastMessageFilter(ML_Category, ML_PriceItemCode)
