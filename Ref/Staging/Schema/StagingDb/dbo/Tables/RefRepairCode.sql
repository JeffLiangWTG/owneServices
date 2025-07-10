CREATE TABLE [RefRepairCode] (
    [RRC_PK] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_RefRepairCode_RRC_PK] DEFAULT (NEWID()),
    [RRC_IsActive] BIT NOT NULL CONSTRAINT [DF_RefRepairCode_RRC_IsActive] DEFAULT (''),
    [RRC_Code] VARCHAR(10) NOT NULL CONSTRAINT [DF_RefRepairCode_RRC_Code] DEFAULT (''),
    [RRC_Description] VARCHAR(100) NOT NULL CONSTRAINT [DF_RefRepairCode_RRC_Description] DEFAULT (''),
    [RRC_Group] VARCHAR(15) NOT NULL CONSTRAINT [DF_RefRepairCode_RRC_Group] DEFAULT (''),
    [RRC_ServiceType] CHAR(3) NOT NULL CONSTRAINT [DF_RefRepairCode_RRC_ServiceType] DEFAULT (''),
    CONSTRAINT [PK_RefRepairCode] PRIMARY KEY CLUSTERED ([RRC_PK] ASC),
    CONSTRAINT [CK_RefRepairCode_RRC_Group] CHECK  ([RRC_Group] = 'CEDEX' OR [RRC_Group] = 'MERC'),
    CONSTRAINT [CK_RefRepairCode_RRC_ServiceType] CHECK ([RRC_ServiceType] = 'RPR' OR [RRC_ServiceType] = 'PRP' OR [RRC_ServiceType] = 'CLN' OR [RRC_ServiceType] = 'UPG')
)
GO
