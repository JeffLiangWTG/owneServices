CREATE TRIGGER RefCusCodeType_ZZK_ZZZ_NKDataGrouping
	ON RefCusCodeType
	FOR INSERT, Update
	AS
	IF EXISTS (SELECT 1 FROM inserted WHERE ZZK_ZZZ_NKDataGrouping = '')
	THROW 58014, 'A RefCusCodeType was inserted/updated with empty ZZK_ZZZ_NKDataGrouping', 1;
GO
