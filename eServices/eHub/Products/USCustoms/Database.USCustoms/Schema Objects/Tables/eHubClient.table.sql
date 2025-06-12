CREATE TABLE [dbo].[eHubClient] (
    [CC_PK]               UNIQUEIDENTIFIER NOT NULL,
    [CC_ID]               VARCHAR (36)     NOT NULL,
    [CC_FriendlyName]     VARCHAR (128)    NOT NULL,
    [CC_Odyssey_OH]       UNIQUEIDENTIFIER NOT NULL,
    [CC_DistributionZone] UNIQUEIDENTIFIER NULL,
    [CC_EmailAddress]     VARCHAR (128)    NOT NULL,
    [CC_Password]         VARCHAR (200)    NOT NULL
);

