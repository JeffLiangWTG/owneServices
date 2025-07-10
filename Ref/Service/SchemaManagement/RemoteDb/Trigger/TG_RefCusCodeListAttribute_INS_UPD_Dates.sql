CREATE TRIGGER [dbo].[TG_RefCusCodeListAttribute_INS_UPD_Dates] ON [dbo].[RefCusCodeListAttribute]
FOR INSERT, UPDATE
AS
BEGIN
	IF UPDATE(ZZE_StartDate) OR UPDATE(ZZE_EndDate) OR UPDATE(ZZE_ZXE_NKName) OR UPDATE(ZZE_ZZD_CodeList)
	BEGIN
		IF EXISTS
		(
			SELECT NULL FROM inserted I
			JOIN RefCusCodeList L ON L.ZZD_PK = I.ZZE_ZZD_CodeList
			JOIN RefCusCodeListAttributeName N ON N.ZXE_Name = I.ZZE_ZXE_NKName AND N.ZXE_ZZK_NKCodeType = L.ZZD_ZZK_NKCodeType AND N.ZXE_ZZZ_NKDataGrouping = L.ZZD_ZZZ_NKDataGrouping
			WHERE N.ZXE_IsDateRangeUsed = 0 AND I.ZZE_StartDate IS NOT NULL
		)
		THROW 512082024, 'RefCusCodeListAttribute was inserted with non-null values for ZZE_StartDate and/or ZZE_EndDate while ZXE_IsDateRangeUsed = 0', 1;
	END

	IF UPDATE(ZZE_StartDate) OR UPDATE(ZZE_EndDate) OR UPDATE(ZZE_Value) OR UPDATE(ZZE_ZXE_NKName) OR UPDATE(ZZE_ZZD_CodeList)
	BEGIN
		IF EXISTS
		(
			SELECT NULL FROM inserted I
			JOIN RefCusCodeListAttribute A ON A.ZZE_ZZD_CodeList = I.ZZE_ZZD_CodeList
				AND A.ZZE_ZXE_NKName = I.ZZE_ZXE_NKName
				AND A.ZZE_PK <> I.ZZE_PK
				AND A.ZZE_Value = I.ZZE_Value
				AND ISNULL(A.ZZE_StartDate, '1900-01-01 00:00') BETWEEN ISNULL(I.ZZE_StartDate, '1900-01-01 00:00') AND ISNULL(I.ZZE_EndDate, '2079-06-06 23:59')
		)
		THROW 513082024, 'RefCusCodeListAttribute was inserted with overlaping ZZE_StartDate/ZZE_EndDate values', 1;
	END
END
