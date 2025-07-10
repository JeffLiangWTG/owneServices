CREATE VIEW RefCusProfileQuestionTableView_V1 AS
SELECT XQ2_PK,
XQ2_XXX_ProfileType,
XQ2_QuestionCode AS XQ2_Code,
XQ2_AnswerDataType,
XQ2_AnswerMaxLength,
XQ2_AnswerDecimalPlaces,
XQ2_AnswerMask,
XQ2_AllowMultipleAnswers,
XQ2_Name,
XQ2_Text,
XQ2_Note,
XQ2_StartDate,
XQ2_EndDate,
XQ2_IsAnswerMandatory,
XQ2_ZZZ_NKDataGrouping
FROM RefCusProfileQuestion
