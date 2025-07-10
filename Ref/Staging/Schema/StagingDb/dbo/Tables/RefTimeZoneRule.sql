CREATE TABLE [dbo].[RefTimeZoneRule](
	[R4_PK] [uniqueidentifier] NOT NULL CONSTRAINT DF_RefTimeZoneRule_R4_PK DEFAULT (NEWID()),
	[R4_FromYear] [int] NOT NULL CONSTRAINT [DF_RefTimeZoneRule_R4_FromYear] DEFAULT ((0)),
	[R4_ToYear] [int] NOT NULL CONSTRAINT [DF_RefTimeZoneRule_R4_ToYear] DEFAULT ((0)),
	[R4_StartOrEndRule] [varchar](3) NOT NULL CONSTRAINT [DF_RefTimeZoneRule_R4_StartOrEndRule] DEFAULT (''),
	[R4_DaylightSavingDayWeekDate] [varchar](3) NOT NULL CONSTRAINT [DF_RefTimeZoneRule_R4_DaylightSavingDayWeekDate] DEFAULT (''),
	[R4_DaylightSavingDate] [smalldatetime] NULL,
	[R4_DaylightSavingDayCount] [tinyint] NOT NULL CONSTRAINT [DF_RefTimeZoneRule_R4_DaylightSavingDayCount] DEFAULT ((0)),
	[R4_DaylightSavingDayName] [varchar](3) NOT NULL CONSTRAINT [DF_RefTimeZoneRule_R4_DaylightSavingDayName] DEFAULT (''),
	[R4_DaylightSavingMonth] [varchar](3) NOT NULL CONSTRAINT [DF_RefTimeZoneRule_R4_DaylightSavingMonth] DEFAULT (''),
	[R4_TypeOfTime] [varchar](3) NOT NULL CONSTRAINT [DF_RefTimeZoneRule_R4_TypeOfTime] DEFAULT (''),
	[R4_R2] [uniqueidentifier] NOT NULL,
	CONSTRAINT PK_RefTimeZoneRule_R4_PK PRIMARY KEY NONCLUSTERED (R4_PK ASC),
	CONSTRAINT FK_RefTimeZoneRule_RefTimeZone FOREIGN KEY(R4_R2) REFERENCES RefTimeZone(R2_PK),
	CONSTRAINT [CK_RefTimeZoneRule_R4_ToYear] CHECK ([R4_ToYear] >= [R4_FromYear] OR [R4_ToYear] = 0)
)
GO
CREATE NONCLUSTERED INDEX [IX_RefTimeZoneRule_R4_R2] ON [RefTimeZoneRule] ([R4_R2])
