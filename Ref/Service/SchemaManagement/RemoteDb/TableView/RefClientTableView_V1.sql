CREATE VIEW RefClientTableView_V1 AS
SELECT RCT_PK,
RCT_ClientID,
RCT_Certificate,
RCT_LegacyCertificate,
RCT_Signature
FROM RefClient
