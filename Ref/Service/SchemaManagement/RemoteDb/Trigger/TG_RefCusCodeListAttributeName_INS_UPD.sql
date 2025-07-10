CREATE TRIGGER [dbo].[TG_RefCusCodeListAttributeName_INS_UPD] ON [dbo].RefCusCodeListAttributeName
FOR INSERT, UPDATE
AS
BEGIN
	IF UPDATE(ZXE_ZZK_NKCodeType) OR UPDATE(ZXE_ZZK_NKCodeTypeForValueList) OR UPDATE(ZXE_ZZZ_NKDataGrouping)
	BEGIN
		IF EXISTS
		(
			SELECT NULL FROM inserted I
			LEFT JOIN RefCusCodeType C ON C.ZZK_CodeType = I.ZXE_ZZK_NKCodeType AND C.ZZK_ZZZ_NKDataGrouping = I.ZXE_ZZZ_NKDataGrouping
			LEFT JOIN RefCusCodeType D ON D.ZZK_CodeType = I.ZXE_ZZK_NKCodeTypeForValueList AND D.ZZK_ZZZ_NKDataGrouping = I.ZXE_ZZZ_NKDataGrouping
			WHERE C.ZZK_PK IS NULL OR (I.ZXE_ZZK_NKCodeTypeForValueList <> '' AND D.ZZK_PK IS NULL)
		)
		THROW 58013, 'A RefCusCodeListAttributeName was inserted/updated with invalid values in ZXE_ZZK_NKCodeType and/or ZXE_ZZK_NKCodeTypeForValueList and/or ZXE_ZZZ_NKDataGrouping which are not found in RefCusCodeType.ZZK_CodeType and RefCusCodeType.ZZK_ZZZ_NKDataGrouping', 1;
	END
END
