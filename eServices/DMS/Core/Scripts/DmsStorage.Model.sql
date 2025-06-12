IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [DmsStorageCatalog] (
    [SC_Schema] nvarchar(128) NOT NULL,
    [SC_Name] nvarchar(128) NOT NULL,
    [SC_Type] varchar(50) NOT NULL,
    [SC_Version] varchar(50) NOT NULL,
    [SC_VersionMajor] AS (convert([int],left([SC_Version],charindex('.',[SC_Version])-(1)))),
    [SC_VersionMinor] AS (convert([int],substring([SC_Version],charindex('.',[SC_Version])+(1),case when charindex('-',[SC_Version])>(0) then charindex('-',[SC_Version])-(1) else len([SC_Version]) end-charindex('.',[SC_Version])))),
    [SC_VersionLabel] AS (substring([SC_Version],charindex('-',[SC_Version]),case when charindex('-',[SC_Version])>(0) then (len([SC_Version])-charindex('-',[SC_Version]))+(1) else (0) end)),
    [SC_ExpirationDays] int NULL,
    [SC_CreateTime] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
    [SC_CreateUser] nvarchar(250) NOT NULL DEFAULT ((suser_name())),
    [SC_DeleteTime] datetime2 NULL,
    [SC_DeleteUser] nvarchar(250) NULL,
    CONSTRAINT [PK_DmsStorageCatalog] PRIMARY KEY ([SC_Schema], [SC_Name])
);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20240914213655_Initial', N'8.0.8');
GO

COMMIT;
GO

