CREATE TABLE RefCusPreference
(
	[ZZS_PK] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_RefCusPreference_ZZS_PK] DEFAULT NEWID(),
	[ZZS_Preference] VARCHAR(10) NOT NULL,
	[ZZS_Description] NVARCHAR(500) NOT NULL,
	[ZZS_ZZZ_NKDataGrouping] VARCHAR(3) NOT NULL,

	CONSTRAINT [PK_RefCusPreference] PRIMARY KEY CLUSTERED( [ZZS_PK] ASC ),
	CONSTRAINT [CK_RefCusPreference_ZZS_Preference] CHECK ([ZZS_Preference] <>''),
	CONSTRAINT [CK_RefCusPreference_ZZS_Description] CHECK ([ZZS_Description] <>''),
	CONSTRAINT [FK_RefCusPreference_RefDataGrouping] FOREIGN KEY([ZZS_ZZZ_NKDataGrouping]) REFERENCES [RefDataGrouping] ([ZZZ_DataGrouping])
)
GO
CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusPreference_ZZS_ZZZ_NKDataGrouping_ZZS_Preference   ON RefCusPreference (ZZS_ZZZ_NKDataGrouping, ZZS_Preference)
GO
