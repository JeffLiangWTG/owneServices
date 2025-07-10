CREATE TRIGGER [dbo].[TG_RefCusTariffAdditionalCode_INS_UPD_Parents] ON [dbo].[RefCusTariffAdditionalCode]
FOR INSERT, UPDATE
AS
BEGIN
	IF UPDATE(ZY2_ZZZ_NKDataGrouping) OR UPDATE(ZY2_ParentAdditionalCode) OR UPDATE(ZY2_ZY3_NKParentCategory) OR UPDATE(ZY2_ZZ1_Tariff) OR UPDATE(ZY2_ZZW_NationalCode)
	BEGIN
		IF EXISTS
		(
			SELECT * FROM inserted I
			LEFT JOIN RefCusTariffAdditionalCode C
			ON C.ZY2_ZZZ_NKDataGrouping = I.ZY2_ZZZ_NKDataGrouping
				AND C.ZY2_AdditionalCode = I.ZY2_ParentAdditionalCode
				AND C.ZY2_ZY3_NKCategory = I.ZY2_ZY3_NKParentCategory
				AND ((C.ZY2_ZZ1_Tariff IS NOT NULL AND C.ZY2_ZZ1_Tariff = I.ZY2_ZZ1_Tariff) OR (C.ZY2_ZZW_NationalCode IS NOT NULL AND C.ZY2_ZZW_NationalCode = I.ZY2_ZZW_NationalCode))
			WHERE (I.ZY2_ParentAdditionalCode <> '' OR I.ZY2_ZY3_NKParentCategory <> '') AND C.ZY2_PK IS NULL
		)
		THROW 58015, 'A RefCusTariffAdditionalCode was inserted/updated with invalid values in ZY2_ParentAdditionalCode or ZY2_ZY3_NKParentCategory which are not found in RefCusTariffAdditionalCode.ZY2_ZZ1_Tariff or RefCusTariffAdditionalCode.ZY2_ZZW_NationalCode having matched ZY2_AdditionalCode and ZY2_ZY3_NKCategory.', 1;
	END
END
