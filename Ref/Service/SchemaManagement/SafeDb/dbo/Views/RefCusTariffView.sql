CREATE VIEW [dbo].[RefCusTariffView]
WITH SCHEMABINDING
AS
SELECT [ZZ1_PK], [ZZ1_ZZI_TariffType], [ZZ1_TariffCode], [ZZ1_IAMUnique], [ZZ1_Description], [ZZ1_StartDate], [ZZ1_EndDate], [ZZ1_PublishedDate],
	[ZZ1_ZZF_NKTaxOrFeeCode], [ZZ1_ZZZ_NKDataGrouping], [ZZ1_CompositeKeyOnZZ5], RVC_ParentPK, RVC_LastUpdatedUTC, RVC_Deleted, RVC_DataSetId
FROM [dbo].[RefCusTariff]
JOIN [dbo].[RefDbVersionControl] ON ZZ1_PK = RVC_ParentPK AND RVC_ParentCode = 'ZZ1' AND RVC_IsPublished = 1
GO
CREATE UNIQUE CLUSTERED INDEX IX_RefCusTariffView_ParentPK_LastUpdatedUTC ON RefCusTariffView (RVC_LastUpdatedUTC DESC, RVC_ParentPK)
