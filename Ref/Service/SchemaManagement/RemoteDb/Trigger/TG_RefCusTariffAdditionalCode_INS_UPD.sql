CREATE TRIGGER [dbo].[TG_RefCusTariffAdditionalCode_INS_UPD] ON [dbo].[RefCusTariffAdditionalCode]
FOR INSERT, UPDATE
AS
BEGIN
	IF UPDATE(ZY2_ZZZ_NKDataGrouping) OR UPDATE(ZY2_ZY3_NKCategory)
	BEGIN
		IF EXISTS
		(
			SELECT * FROM inserted I
			LEFT JOIN RefCusTariffAdditionalCodeCategory C ON C.ZY3_ZZZ_NKDataGrouping = I.ZY2_ZZZ_NKDataGrouping AND C.ZY3_Category = I.ZY2_ZY3_NKCategory
			WHERE C.ZY3_PK IS NULL
		)
		THROW 58011, 'A RefCusTariffAdditionalCode was inserted/updated with invalid values in ZY2_ZY3_NKCategory and/or ZY2_ZZZ_NKDataGrouping which are not found in RefCusTariffAdditionalCodeCategory.ZY3_Category and RefCusTariffAdditionalCodeCategory.ZY3_ZZZ_NKDataGrouping', 1;
	END
END