CREATE TABLE RefCusProfileQuestionAnswerListLanguage
(
	XAL_PK UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_RefCusProfileQuestionAnswerListLanguage_XAL_PK DEFAULT (NEWID()),
	XAL_XQ4_QuestionAnswer UNIQUEIDENTIFIER NOT NULL,
	XAL_Description NVARCHAR(500) NOT NULL,
	XAL_ZX6_NKLanguage VARCHAR(3) NOT NULL,
	CONSTRAINT PK_RefCusProfileQuestionAnswerListLanguage PRIMARY KEY CLUSTERED (XAL_PK ASC),
	CONSTRAINT FK_RefCusProfileQuestionAnswerListLanguage_XAL_XQ4_QuestionAnswer FOREIGN KEY(XAL_XQ4_QuestionAnswer) REFERENCES RefCusProfileQuestionAnswerList (XQ4_PK)
)
GO
CREATE NONCLUSTERED INDEX IX_RefCusProfileQuestionAnswerListLanguage_XAL_XQ4_QuestionAnswer_XAL_ZX6_NKLanguage ON RefCusProfileQuestionAnswerListLanguage (XAL_XQ4_QuestionAnswer ASC, XAL_ZX6_NKLanguage ASC)
GO
