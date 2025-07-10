CREATE TABLE RefCusProfileQuestionPathway
(
	XQP_PK UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_RefCusProfileQuestionPathway_XQP_PK  DEFAULT (NEWID()),
	XQP_XQ2_NKQuestionParent VARCHAR(35) NOT NULL CONSTRAINT DF_RefCusProfileQuestionPathway_XQP_XQ2_NKQuestionParent DEFAULT '',
	XQP_XQ2_NKQuestionStartDateParent SMALLDATETIME NOT NULL CONSTRAINT DF_RefCusProfileQuestionPathway_XQP_XQ2_NKQuestionStartDateParent DEFAULT ('1900-01-01'),
	XQP_XQ2_ZZZ_NKDataGroupingParent VARCHAR(3) NOT NULL CONSTRAINT DF_RefCusProfileQuestionPathway_XQP_XQ2_ZZZ_NKDataGroupingParent DEFAULT '',
	XQP_XQ2_NKQuestionChild VARCHAR(35) NOT NULL CONSTRAINT DF_RefCusProfileQuestionPathway_XQP_XQ2_NKQuestionChild DEFAULT '',
	XQP_XQ2_NKQuestionStartDateChild SMALLDATETIME NOT NULL CONSTRAINT DF_RefCusProfileQuestionPathway_XQP_XQ2_NKQuestionStartDateChild DEFAULT ('1900-01-01'),
	XQP_XQ2_ZZZ_NKDataGroupingChild VARCHAR(3) NOT NULL CONSTRAINT DF_RefCusProfileQuestionPathway_XQP_XQ2_ZZZ_NKDataGroupingChild DEFAULT '',
	XQP_XQ2_XXX_NKProfileType VARCHAR(10) NOT NULL CONSTRAINT DF_RefCusProfileQuestionPathway_XQP_XQ2_XXX_NKProfileType DEFAULT '',
	XQP_XQ2_XXX_ZZZ_NKDataGrouping VARCHAR(3) NOT NULL CONSTRAINT DF_RefCusProfileQuestionPathway_XQP_XQ2_XXX_ZZZ_NKDataGrouping DEFAULT '',
	XQP_XQ2_XXX_ZZI_NKTariffType VARCHAR(5) NOT NULL CONSTRAINT DF_RefCusProfileQuestionPathway_XQP_XQ2_XXX_ZZI_NKTariffType DEFAULT '',
	XQP_XQ2_XXX_ZZI_ZZZ_NKDataGrouping VARCHAR(3) NOT NULL CONSTRAINT DF_RefCusProfileQuestionPathway_XQP_XQ2_XXX_ZZI_ZZZ_NKDataGrouping DEFAULT '',
	XQP_Description NVARCHAR(200) NOT NULL CONSTRAINT DF_RefCusProfileQuestionPathway_XQP_Description DEFAULT(''),
	XQP_StartDate SMALLDATETIME NOT NULL CONSTRAINT DF_RefCusProfileQuestionPathway_XQP_StartDate DEFAULT ('1900-01-01'),
	XQP_EndDate SMALLDATETIME NOT NULL CONSTRAINT DF_RefCusProfileQuestionPathway_XQP_EndDate DEFAULT ('2079-06-06 23:59'),
	XQP_ConditionToProceedFormula NVARCHAR(500) NOT NULL CONSTRAINT DF_RefCusProfileQuestionPathway_XQP_ConditionToProceedFormula DEFAULT(''),
	XQP_AllowMultipleAnswers BIT NOT NULL CONSTRAINT DF_RefCusProfileQuestionPathway_XQP_AllowMultipleAnswers DEFAULT(0),
	XQP_IsAnswerMandatory BIT NOT NULL CONSTRAINT DF_RefCusProfileQuestionPathway_XQP_IsAnswerMandatory DEFAULT(0),
	CONSTRAINT PK_RefCusProfileQuestionPathway PRIMARY KEY CLUSTERED (XQP_PK ASC),
	CONSTRAINT CK_RefCusProfileQuestionPathway_XQP_StartDate_XQP_EndDate CHECK  (XQP_StartDate <= XQP_EndDate)
)
GO
