CREATE TABLE [dbo].[RefTimeZone](
	[R2_PK] [uniqueidentifier] NOT NULL CONSTRAINT DF_RefTimeZone_R2_PK DEFAULT (NEWID()),
	[R2_CivilianTimeZoneFullName] [varchar](80) NOT NULL CONSTRAINT [DF_RefTimeZone_R2_CivilianTimeZoneFullName] DEFAULT '',
	[R2_CivilianTimeZoneCode] [varchar](10) NOT NULL CONSTRAINT [DF_RefTimeZone_R2_CivilianTimeZoneCode] DEFAULT '',
	[R2_MilitaryTimeZoneCode] [varchar](2) NOT NULL CONSTRAINT [DF_RefTimeZone_R2_MilitaryTimeZoneCode] DEFAULT '',
	[R2_OffsetMinutesFromUTC] SMALLINT NOT NULL CONSTRAINT [DF_RefTimeZone_R2_OffsetMinutesFromUTC] DEFAULT ((0)),
	[R2_R3_TimeZoneSet] uniqueidentifier NOT NULL,
	[R2_Type] [varchar](3) NOT NULL  CONSTRAINT [DF_RefTimeZone_R2_Type] DEFAULT '',
 CONSTRAINT [PK_RefTimeZone] PRIMARY KEY CLUSTERED ( [R2_PK]  ASC),
 CONSTRAINT FK_RefTimeZoneSet_R2_R3_TimeZoneSet FOREIGN KEY(R2_R3_TimeZoneSet) REFERENCES RefTimeZoneSet(R3_PK)
)
GO
CREATE NONCLUSTERED INDEX [IX_RefTimeZone_R2_R3_TimeZoneSet] ON [RefTimeZone] (R2_R3_TimeZoneSet)
GO
CREATE TRIGGER RefTimeZone_Delete
ON RefTimeZone
INSTEAD OF DELETE
AS

DELETE en
FROM deleted
JOIN RefTimeZoneRule en ON R2_PK = R4_R2

DELETE en
FROM RefTimeZone en
JOIN deleted ON en.R2_PK = deleted.R2_PK

GO
