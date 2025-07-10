CREATE VIEW RefGlbReleaseNoteTableView_V1 AS
SELECT ZGF_PK,
ZGF_IsValid,
ZGF_Category,
ZGF_RN_NKCountryForReleaseNote,
ZGF_Summary,
ZGF_URL,
ZGF_ReleaseNoteDate,
ZGF_Section,
ZGF_MinVersion,
ZGF_QuickStartPK
FROM RefGlbReleaseNote
