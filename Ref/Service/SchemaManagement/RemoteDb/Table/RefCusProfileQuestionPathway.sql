CREATE TABLE RefCusProfileQuestionPathway
(
	XQP_PK UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_RefCusProfileQuestionPathway_XQP_PK  DEFAULT (NEWID()),
	XQP_XQ2_QuestionParent UNIQUEIDENTIFIER NOT NULL,
	XQP_XQ2_QuestionChild UNIQUEIDENTIFIER NOT NULL,
	XQP_Description NVARCHAR(200) NOT NULL CONSTRAINT DF_RefCusProfileQuestionPathway_XQP_Description DEFAULT(''),
	XQP_StartDate SMALLDATETIME NOT NULL CONSTRAINT DF_RefCusProfileQuestionPathway_XQP_StartDate DEFAULT ('1900-01-01'),
	XQP_EndDate SMALLDATETIME NOT NULL CONSTRAINT DF_RefCusProfileQuestionPathway_XQP_EndDate DEFAULT ('2079-06-06 23:59'),
	XQP_ConditionToProceedFormula NVARCHAR(500) NOT NULL CONSTRAINT DF_RefCusProfileQuestionPathway_XQP_ConditionToProceedFormula DEFAULT(''),
	XQP_AllowMultipleAnswers BIT NOT NULL CONSTRAINT DF_RefCusProfileQuestionPathway_XQP_AllowMultipleAnswers DEFAULT(0),
	XQP_IsAnswerMandatory BIT NOT NULL CONSTRAINT DF_RefCusProfileQuestionPathway_XQP_IsAnswerMandatory DEFAULT(0),

	CONSTRAINT PK_RefCusProfileQuestionPathway PRIMARY KEY CLUSTERED (XQP_PK ASC),
	CONSTRAINT FK_RefCusProfileQuestionPathway_XQP_XQ2_QuestionParent FOREIGN KEY(XQP_XQ2_QuestionParent) REFERENCES RefCusProfileQuestion (XQ2_PK),
	CONSTRAINT FK_RefCusProfileQuestionPathway_XQP_XQ2_QuestionChild FOREIGN KEY(XQP_XQ2_QuestionChild) REFERENCES RefCusProfileQuestion (XQ2_PK),
	CONSTRAINT CK_RefCusProfileQuestionPathway_XQP_StartDate_XQP_EndDate CHECK  (XQP_StartDate <= XQP_EndDate),
)
GO
CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusProfileQuestionPathway_XQP_XQ2_QuestionParent_XQP_XQ2_QuestionChild_XQP_StartDate ON RefCusProfileQuestionPathway (XQP_XQ2_QuestionParent ASC, XQP_XQ2_QuestionChild ASC, XQP_StartDate ASC) 
GO
ALTER TABLE RefCusProfileQuestionPathway SET (LOCK_ESCALATION = DISABLE);
