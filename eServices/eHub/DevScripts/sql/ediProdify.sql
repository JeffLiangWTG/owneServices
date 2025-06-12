-- Adds extra tables to an ediProdCache DB named ediProd, such that eHub can treat it as an actual ediProd DB
use ediProd

CREATE TABLE EDIInterchange (
	EI_SystemLastEditTimeUtc DATETIME,
	EI_Status NVARCHAR(MAX),
	EI_ReceiveTransmit NVARCHAR(MAX),
	EI_IsActive BIT,
)