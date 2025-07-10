CREATE VIEW RefCusProfileTableView_V2 AS
SELECT XX0_PK,
XX0_XXX_ProfileType,
XX0_AppliesToCode AS XX0_TariffCode,
XX0_QuestionCode,
XX0_StartDate,
XX0_EndDate,
XX0_ZZZ_NKDataGrouping,
XX0_AllowMultipleAnswers,
XX0_IsAnswerMandatory
FROM RefCusProfile
