CREATE TRIGGER [dbo].[TG_RefCusCodeList_INS_UPD] ON [dbo].[RefCusCodeList]
FOR INSERT, UPDATE
AS
BEGIN
	IF UPDATE(ZZD_ZZK_NKCodeType) OR UPDATE(ZZD_ZZZ_NKDataGrouping)
	BEGIN
		IF EXISTS
		(
			SELECT NULL FROM inserted I
			LEFT JOIN RefCusCodeType C ON C.ZZK_CodeType = I.ZZD_ZZK_NKCodeType AND C.ZZK_ZZZ_NKDataGrouping = I.ZZD_ZZZ_NKDataGrouping
			WHERE C.ZZK_PK IS NULL
		)
		THROW 58012, 'A RefCusCodeList was inserted/updated with invalid values in ZZD_ZZK_NKCodeType and/or ZZD_ZZZ_NKDataGrouping which are not found in RefCusCodeType.ZZK_CodeType and RefCusCodeType.ZZK_ZZZ_NKDataGrouping', 1;
	END
END
