CREATE VIEW [dbo].[RefCusTariffRelationshipView]
WITH SCHEMABINDING
AS
SELECT [ZZH_PK], [ZZH_ZZ1_Tariff], [ZZH_ZZI_TariffType], [ZZH_TariffCode], [ZZH_DataSetPK], [ZZH_DataSetCode], RVC_ParentPK, RVC_LastUpdatedUTC, RVC_DataSetId
FROM [dbo].[RefCusTariffRelationship]
JOIN [dbo].[RefDbVersionControl] ON ZZH_ZZ1_Tariff = RVC_ParentPK AND RVC_ParentCode = 'ZZ1' AND RVC_IsPublished = 1
GO
CREATE UNIQUE CLUSTERED INDEX IX_RefCusTariffRelationshipView_ZZ2_PK_ParentPK_LastUpdatedUTC ON RefCusTariffRelationshipView (RVC_LastUpdatedUTC DESC, RVC_ParentPK ASC, ZZH_PK)
