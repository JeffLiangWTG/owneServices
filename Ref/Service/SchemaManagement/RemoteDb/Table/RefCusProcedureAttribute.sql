CREATE TABLE RefCusProcedureAttribute
(
[ZXB_PK] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_RefCusProcedureAttribute_ZXB_PK] DEFAULT(NEWID()),
[ZXB_ZZ6_ProcedureCode] UNIQUEIDENTIFIER NOT NULL,
[ZXB_Name] VARCHAR(50) NOT NULL CONSTRAINT [DF_RefCusProcedureAttribute_ZXB_Name] DEFAULT(''),
[ZXB_Value] NVARCHAR(MAX) NOT NULL CONSTRAINT [DF_RefCusProcedureAttribute_ZXB_Value] DEFAULT(''),

CONSTRAINT [PK_RefCusProcedureAttribute] PRIMARY KEY CLUSTERED ([ZXB_PK] ASC),
CONSTRAINT [FK_RefCusProcedureAttribute_RefCusProcedure] FOREIGN KEY([ZXB_ZZ6_ProcedureCode]) REFERENCES [RefCusProcedure] ([ZZ6_PK]) ON DELETE CASCADE,
)
GO
CREATE NONCLUSTERED INDEX IX_RefCusProcedureAttribute_ZXB_ZZ6_ProcedureCode_ZXB_Name ON RefCusProcedureAttribute(ZXB_ZZ6_ProcedureCode, ZXB_Name)
GO
