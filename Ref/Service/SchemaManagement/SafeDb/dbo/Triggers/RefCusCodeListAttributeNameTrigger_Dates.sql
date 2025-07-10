CREATE TRIGGER [dbo].[RefCusCodeListAttributeNameTrigger_Dates] ON [dbo].RefCusCodeListAttributeName
FOR UPDATE
AS
BEGIN
	IF UPDATE(ZXE_IsDateRangeUsed)
	BEGIN
		IF EXISTS
		(
			SELECT NULL FROM inserted I
			JOIN RefCusCodeList L ON L.ZZD_ZZK_NKCodeType = I.ZXE_ZZK_NKCodeType AND L.ZZD_ZZZ_NKDataGrouping = I.ZXE_ZZZ_NKDataGrouping
			JOIN RefCusCodeListAttribute A ON A.ZZE_ZZD_CodeList = L.ZZD_PK AND A.ZZE_ZXE_NKName = I.ZXE_Name
			WHERE I.ZXE_IsDateRangeUsed = 0 AND A.ZZE_StartDate IS NOT NULL
		)
		THROW 512082024, 'RefCusCodeListAttribute was inserted with non-null values for ZZE_StartDate and/or ZZE_EndDate while ZXE_IsDateRangeUsed = 0', 1;
	END
END
