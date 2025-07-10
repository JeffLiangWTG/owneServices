CREATE TABLE RefCusProfileQuestionAnswerListLanguage
(
	XAL_PK UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_RefCusProfileQuestionAnswerListLanguage_XAL_PK DEFAULT (NEWID()),
	XAL_XQ4_QuestionAnswer UNIQUEIDENTIFIER NOT NULL,
	XAL_Description NVARCHAR(500) NOT NULL,
	XAL_ZX6_NKLanguage VARCHAR(3) NOT NULL,
	XAL_DataSetPK UNIQUEIDENTIFIER,
	XAL_DataSetCode VARCHAR(3),
	XAL_SysStartTime DATETIME2 GENERATED ALWAYS AS ROW START NOT NULL CONSTRAINT DF_XAL_SysStartTime DEFAULT SYSUTCDATETIME(),
	XAL_SysEndTime DATETIME2 GENERATED ALWAYS AS ROW END NOT NULL CONSTRAINT DF_XAL_SysEndTime DEFAULT CONVERT(DATETIME2, '9999-12-31 23:59:59.9999999'),
	PERIOD FOR SYSTEM_TIME (XAL_SysStartTime, XAL_SysEndTime),

	CONSTRAINT PK_RefCusProfileQuestionAnswerListLanguage PRIMARY KEY CLUSTERED (XAL_PK ASC),
	CONSTRAINT FK_RefCusProfileQuestionAnswerListLanguage_XAL_XQ4_QuestionAnswer FOREIGN KEY(XAL_XQ4_QuestionAnswer) REFERENCES RefCusProfileQuestionAnswerList (XQ4_PK),
	CONSTRAINT CK_RefCusProfileQuestionAnswerListLanguage_XAL_Description CHECK (XAL_Description <> ''),
	CONSTRAINT FK_RefCusProfileQuestionAnswerListLanguage_XAL_ZX6_NKLanguage FOREIGN KEY(XAL_ZX6_NKLanguage) REFERENCES RefLanguageType (ZX6_Language)
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.RefCusProfileQuestionAnswerListLanguageHistory))
GO
CREATE UNIQUE INDEX IX_RefCusProfileQuestionAnswerListLanguage_XAL_XQ4_QuestionAnswer_XAL_ZX6_NKLanguage ON RefCusProfileQuestionAnswerListLanguage (XAL_XQ4_QuestionAnswer ASC, XAL_ZX6_NKLanguage ASC)
GO
ALTER TABLE RefCusProfileQuestionAnswerListLanguage SET (LOCK_ESCALATION = DISABLE);
