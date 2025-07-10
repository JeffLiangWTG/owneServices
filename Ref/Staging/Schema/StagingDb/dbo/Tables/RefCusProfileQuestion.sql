CREATE TABLE RefCusProfileQuestion
(
	XQ2_PK UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_RefCusProfileQuestion_XQ2_PK  DEFAULT (NEWID()),
	XQ2_XXX_NKProfileType VARCHAR(10) NOT NULL CONSTRAINT DF_RefCusProfileQuestion_XQ2_XXX_NKProfileType DEFAULT '',
	XQ2_XXX_ZZZ_NKDataGrouping VARCHAR(3) NOT NULL CONSTRAINT DF_RefCusProfileQuestion_XQ2_XXX_ZZZ_NKDataGrouping DEFAULT '',
	XQ2_XXX_ZZI_NKTariffType VARCHAR(5) NOT NULL CONSTRAINT DF_RefCusProfileQuestion_XQ2_XXX_ZZI_NKTariffType DEFAULT '',
	XQ2_XXX_ZZI_ZZZ_NKDataGrouping VARCHAR(3) NOT NULL CONSTRAINT DF_RefCusProfileQuestion_XQ2_XXX_ZZI_ZZZ_NKDataGrouping DEFAULT '',
	XQ2_QuestionCode VARCHAR(35) NOT NULL,
	XQ2_AnswerDataType VARCHAR(35) NOT NULL,
	XQ2_AnswerMaxLength SMALLINT NOT NULL CONSTRAINT DF_RefCusProfileQuestion_XQ2_AnswerMaxLength DEFAULT(0),
	XQ2_AnswerDecimalPlaces SMALLINT NOT NULL CONSTRAINT DF_RefCusProfileQuestion_XQ2_AnswerDecimalPlaces DEFAULT(0),
	XQ2_AnswerMask VARCHAR(50) NOT NULL CONSTRAINT DF_RefCusProfileQuestion_XQ2_AnswerMask DEFAULT(''),
	XQ2_AllowMultipleAnswers BIT NOT NULL CONSTRAINT DF_RefCusProfileQuestion_XQ2_AllowMultipleAnswers DEFAULT(0),
	XQ2_Name NVARCHAR(200) NOT NULL,
	XQ2_Text NVARCHAR(1000) NOT NULL,
	XQ2_Note NVARCHAR(2000) NOT NULL CONSTRAINT DF_RefCusProfileQuestion_XQ2_Note DEFAULT(''),
	XQ2_StartDate SMALLDATETIME NOT NULL CONSTRAINT DF_RefCusProfileQuestion_XQ2_StartDate DEFAULT ('1900-01-01'),
	XQ2_EndDate SMALLDATETIME NOT NULL CONSTRAINT DF_RefCusProfileQuestion_XQ2_EndDate DEFAULT ('2079-06-06 23:59'),
	XQ2_IsAnswerMandatory BIT NOT NULL CONSTRAINT DF_RefCusProfileQuestion_XQ2_IsAnswerMandatory DEFAULT(0),
	XQ2_ZZZ_NKDataGrouping VARCHAR(3) NOT NULL,
	CONSTRAINT PK_RefCusProfileQuestion PRIMARY KEY CLUSTERED (XQ2_PK ASC),
	CONSTRAINT CK_RefCusProfileQuestion_XQ2_AnswerDataType CHECK  (XQ2_AnswerDataType = 'BOOLEAN' OR XQ2_AnswerDataType = 'STRING' OR XQ2_AnswerDataType = 'NUMBER' OR XQ2_AnswerDataType = 'LIST' OR XQ2_AnswerDataType = 'DATE' OR XQ2_AnswerDataType = 'COMPOUND' OR XQ2_AnswerDataType = 'DYNAMIC'),
	CONSTRAINT CK_RefCusProfileQuestion_XQ2_AnswerMaxLength CHECK  (XQ2_AnswerMaxLength >= 0),
	CONSTRAINT CK_RefCusProfileQuestion_XQ2_AnswerDecimalPlaces CHECK  (XQ2_AnswerDecimalPlaces >= 0),
	CONSTRAINT CK_RefCusProfileQuestion_XQ2_StartDate_XQ2_EndDate CHECK  (XQ2_StartDate <= XQ2_EndDate)
)
GO
