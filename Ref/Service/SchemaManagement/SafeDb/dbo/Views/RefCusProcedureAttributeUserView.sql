CREATE VIEW [dbo].[RefCusProcedureAttributeUserView]
WITH SCHEMABINDING
AS

SELECT
    ZXB_PK AS ZXB_PK,
    ZXB_ZZ6_ProcedureCode AS ZXB_ZZ6_ProcedureCode,
    ZXB_Name AS ZXB_Name,
    ZXB_Value AS ZXB_Value,
    ZZ6_ZZZ_NKDataGrouping AS ZXB_CountryOrGrouping
FROM [dbo].[RefCusProcedureAttribute]
JOIN [dbo].[RefCusProcedure] ON ZXB_ZZ6_ProcedureCode = ZZ6_PK
GO

CREATE PROCEDURE [dbo].[RefCusProcedureAttributeUserView_Ins]
    @ZXB_PK UNIQUEIDENTIFIER,
    @ZXB_ZZ6_ProcedureCode UNIQUEIDENTIFIER,
    @ZXB_Name VARCHAR(50),
    @ZXB_Value NVARCHAR(MAX),
    @ZXB_CountryOrGrouping VARCHAR(3)
AS
BEGIN

INSERT dbo.RefCusProcedureAttribute (ZXB_PK, ZXB_ZZ6_ProcedureCode, ZXB_Name, ZXB_Value)
SELECT @ZXB_PK, @ZXB_ZZ6_ProcedureCode, @ZXB_Name, @ZXB_Value
FROM dbo.RefCusProcedure WHERE ZZ6_PK = @ZXB_ZZ6_ProcedureCode AND ZZ6_ZZZ_NKDataGrouping = @ZXB_CountryOrGrouping

IF @@ROWCOUNT = 0 THROW 51000, 'INVALID DATA', 1;

END
GO

CREATE PROCEDURE [dbo].[RefCusProcedureAttributeUserView_Del]
    @ZXB_PK UNIQUEIDENTIFIER,
    @ZXB_CountryOrGrouping VARCHAR(3)
AS

DELETE attr
FROM dbo.RefCusProcedureAttribute attr
JOIN dbo.RefCusProcedure ON ZXB_ZZ6_ProcedureCode = ZZ6_PK
WHERE ZXB_PK = @ZXB_PK AND ZZ6_ZZZ_NKDataGrouping = @ZXB_CountryOrGrouping

IF @@ROWCOUNT = 0 THROW 51000, 'INVALID DATA', 1;

GO

CREATE PROCEDURE [dbo].[RefCusProcedureAttributeUserView_Ups]
    @ZXB_PK UNIQUEIDENTIFIER,
    @ZXB_Name VARCHAR(50),
    @ZXB_Value NVARCHAR(MAX),
    @ZXB_CountryOrGrouping VARCHAR(3)
AS
BEGIN

UPDATE attr SET
    ZXB_Name = @ZXB_Name,
    ZXB_Value = @ZXB_Value
FROM dbo.RefCusProcedureAttribute attr
JOIN dbo.RefCusProcedure ON ZXB_ZZ6_ProcedureCode = ZZ6_PK
WHERE ZXB_PK = @ZXB_PK AND ZZ6_ZZZ_NKDataGrouping = @ZXB_CountryOrGrouping

IF @@ROWCOUNT = 0 THROW 51000, 'INVALID DATA', 1;

END
