CREATE VIEW [dbo].[SourceDataUserView]
AS
SELECT
	SDA_PK,
	SDA_SubSource,
	SDA_Source,
	SDA_SourceTime,
	case 
	when CHARINDEX('\', SDA_Filename) > 0
		THEN RIGHT(SDA_Filename, CHARINDEX('\', REVERSE(SDA_Filename)) -1)
	ELSE
		SDA_Filename
	END AS SDA_Filename,
	SDA_CreatedTime,
	SDA_Status,
	SDA_NotProcessedUntil
FROM [SourceData]
