CREATE TABLE RefCusProfileQuestionAttribute
(
	XQ3_PK UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_RefCusProfileQuestionAttribute_XQ3_PK  DEFAULT (NEWID()),
	XQ3_XQ2_Question UNIQUEIDENTIFIER NOT NULL,
	XQ3_Name VARCHAR(35) NOT NULL,
	XQ3_Value NVARCHAR(MAX) NOT NULL

	CONSTRAINT PK_RefCusProfileQuestionAttribute PRIMARY KEY CLUSTERED (XQ3_PK ASC),
	CONSTRAINT FK_RefCusProfileQuestionAttribute_RefCusProfileQuestion FOREIGN KEY(XQ3_XQ2_Question) REFERENCES RefCusProfileQuestion (XQ2_PK),
	CONSTRAINT CK_RefCusProfileQuestionAttribute_XQ3_Name CHECK (XQ3_Name <> ''),
	CONSTRAINT CK_RefCusProfileQuestionAttribute_XQ3_Value CHECK (XQ3_Value <> '')
)
GO
CREATE NONCLUSTERED INDEX IX_RefCusProfileQuestionAttribute_XQ3_XQ2_Question_XQ3_Name ON RefCusProfileQuestionAttribute (XQ3_XQ2_Question ASC, XQ3_Name ASC)
GO
ALTER TABLE RefCusProfileQuestionAttribute SET (LOCK_ESCALATION = DISABLE);
