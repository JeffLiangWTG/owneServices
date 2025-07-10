CREATE TRIGGER [dbo].[TG_RefCusCodeListAttribute_INS_UPD] ON [dbo].[RefCusCodeListAttribute]
FOR INSERT, UPDATE
AS
BEGIN
	IF UPDATE(ZZE_ZXE_NKName) OR UPDATE(ZZE_ZZD_CodeList)
	BEGIN
		IF EXISTS
		(
			SELECT TOP 1 1 
			FROM inserted i
			LEFT JOIN RefCusCodeList li ON i.ZZE_ZZD_CodeList = li.ZZD_PK
			LEFT JOIN RefCusCodeListAttributeName n ON n.ZXE_Name = i.ZZE_ZXE_NKName AND n.ZXE_ZZK_NKCodeType = li.ZZD_ZZK_NKCodeType AND n.ZXE_ZZZ_NKDataGrouping = li.ZZD_ZZZ_NKDataGrouping
			WHERE n.ZXE_PK IS NULL
		)
		THROW 502082019, 'A RefCusCodeListAttribute was not inserted/updated due to an invalid/non-existent RefCusCodeListAttributeName', 1;
	END
END
