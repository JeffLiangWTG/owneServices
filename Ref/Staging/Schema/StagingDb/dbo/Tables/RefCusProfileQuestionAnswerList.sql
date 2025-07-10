CREATE TABLE RefCusProfileQuestionAnswerList
(
	XQ4_PK UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_RefCusProfileQuestionAnswerList_XQ4_PK  DEFAULT (NEWID()),
	XQ4_XQ2_Question UNIQUEIDENTIFIER NOT NULL,
	XQ4_Value NVARCHAR(100) NOT NULL,
	XQ4_Description NVARCHAR(500) NOT NULL,
	CONSTRAINT PK_RefCusProfileQuestionAnswerList PRIMARY KEY CLUSTERED (XQ4_PK ASC),
	CONSTRAINT FK_RefCusProfileQuestionAnswerList_RefCusProfileQuestion FOREIGN KEY(XQ4_XQ2_Question) REFERENCES RefCusProfileQuestion (XQ2_PK),
)
GO
CREATE NONClUSTERED INDEX IX_RefCusProfileQuestionAnswerList_XQ4_XQ2_Question_XQ4_Value ON RefCusProfileQuestionAnswerList (XQ4_XQ2_Question ASC, XQ4_Value ASC)
GO
