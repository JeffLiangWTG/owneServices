CREATE TABLE RefTimeZone(
	[R2_PK] [uniqueidentifier] NOT NULL CONSTRAINT [DF_RefTimeZone_R2_PK] DEFAULT (NEWID()),
	[R2_CivilianTimeZoneFullName] [varchar](80) NOT NULL CONSTRAINT [DF_RefTimeZone_R2_CivilianTimeZoneFullName] DEFAULT '',
	[R2_CivilianTimeZoneCode] [varchar](10) NOT NULL CONSTRAINT [DF_RefTimeZone_R2_CivilianTimeZoneCode] DEFAULT '',
	[R2_MilitaryTimeZoneCode] [varchar](2) NOT NULL CONSTRAINT [DF_RefTimeZone_R2_MilitaryTimeZoneCode] DEFAULT '',
	[R2_OffsetMinutesFromUTC] SMALLINT NOT NULL CONSTRAINT [DF_RefTimeZone_R2_OffsetMinutesFromUTC] DEFAULT ((0)),
	[R2_R3_TimeZoneSet] uniqueidentifier NOT NULL,
	[R2_Type] [varchar](3) NOT NULL CONSTRAINT [DF_RefTimeZone_R2_Type] DEFAULT '',
	[R2_SysStartTime] DATETIME2 GENERATED ALWAYS AS ROW START NOT NULL CONSTRAINT [DF_R2_SysStartTime] DEFAULT CONVERT(DATETIME2, '1900-01-01 0:0:0.0000000'),
    [R2_SysEndTime] DATETIME2 GENERATED ALWAYS AS ROW END NOT NULL CONSTRAINT [DF_R2_SysEndTime] DEFAULT CONVERT(DATETIME2, '9999-12-31 23:59:59.9999999'),
    PERIOD FOR SYSTEM_TIME ([R2_SysStartTime], [R2_SysEndTime]),
    CONSTRAINT [PK_RefTimeZone] PRIMARY KEY CLUSTERED ( [R2_PK]  ASC),
    CONSTRAINT [FK_RefTimeZoneSet_R2_R3_TimeZoneSet] FOREIGN KEY([R2_R3_TimeZoneSet]) REFERENCES [RefTimeZoneSet](R3_PK)
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.RefTimeZoneHistory))
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_RefTimeZone_R2_R3_TimeZoneSet_R2_Type] ON [RefTimeZone] (R2_R3_TimeZoneSet,R2_Type)
GO
ALTER TABLE RefTimeZone SET (LOCK_ESCALATION = DISABLE);
