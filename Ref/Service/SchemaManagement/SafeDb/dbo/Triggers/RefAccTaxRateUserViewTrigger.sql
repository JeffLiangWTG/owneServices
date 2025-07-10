CREATE TRIGGER RefAccTaxRateUserView_Version_Create ON RefAccTaxRateUserView
INSTEAD OF INSERT
AS
BEGIN
	INSERT dbo.RefAccTaxRate (ZAT_PK, ZAT_RN_NKCountry, ZAT_ReferenceRateType, ZAT_StartDate, ZAT_EndDate, ZAT_RateNumerator, ZAT_RateDenominator)
	SELECT ZAT_PK, ZAT_RN_NKCountry, ZAT_ReferenceRateType, ZAT_StartDate, ZAT_EndDate, ZAT_RateNumerator, ZAT_RateDenominator
	FROM inserted

	UPDATE r
	SET r.RVC_Deleted = ~ZAT_IsPublished, r.RVC_LastEditedUser = dbo.GetUserId()
	FROM RefDbVersionControl r
	JOIN inserted ON ZAT_PK = RVC_ParentPK and RVC_ParentCode = 'ZAT'
END
GO

CREATE TRIGGER RefAccTaxRateUserView_Version_Update ON RefAccTaxRateUserView
INSTEAD OF UPDATE
AS
BEGIN
	UPDATE r SET
		r.ZAT_RN_NKCountry = i.ZAT_RN_NKCountry,
		r.ZAT_ReferenceRateType = i.ZAT_ReferenceRateType,
		r.ZAT_StartDate = i.ZAT_StartDate,
		r.ZAT_EndDate = i.ZAT_EndDate,
		r.ZAT_RateNumerator = i.ZAT_RateNumerator,
		r.ZAT_RateDenominator = i.ZAT_RateDenominator
	FROM RefAccTaxRate r
	JOIN inserted i ON r.ZAT_PK = i.ZAT_PK

	UPDATE r
	SET r.RVC_Deleted = ~ZAT_IsPublished, r.RVC_LastEditedUser = dbo.GetUserId()
	FROM RefDbVersionControl r
	JOIN inserted ON ZAT_PK = RVC_ParentPK and RVC_ParentCode = 'ZAT'
END
GO
