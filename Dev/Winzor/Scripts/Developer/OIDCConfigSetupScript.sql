-- This script saves developers at least 15 mins of their time configuring OIDC settings locally.
-- This script is used to create OIDC config settings for single sign on
-- OIDC config setting contains the following vital information for OIDC login such as Authority URL, Client identifer, scopes and claims
-- To setup OIDC config: It's a one time setup. Login to your sql server, connect to your local database and execute the following script. 
INSERT INTO dbo.STMDATA
(SD_PK,
SD_Name,
SD_Owner,
SD_DepartmentGuid,
SD_Type,
SD_IsLogged,
SD_BinaryValue,
SD_GuidValue,
SD_IsCancelled,
SD_PreserveTestValue,
SD_SystemCreateTimeUtc,
SD_SystemCreateUser,
SD_SystemLastEditTimeUtc,
SD_SystemLastEditUser)
VALUES ('58DB9C00-8F78-4826-B90B-79C3CF1D30FC',
'OIDCConfig',
NULL,
NULL,
'BIN',
1,
CAST(N'<?xml version="1.0" encoding="utf-16"?><OIDCConfig><Version>1</Version><IsOIDCEnabled>Y</IsOIDCEnabled><OIDCServerTypeCode>AZU</OIDCServerTypeCode><AuthorityURL>https://loginsimulator.wisetechglobal.com/</AuthorityURL><ClientIdentifier>dummyIdP</ClientIdentifier><ArrayOfOIDCClaimsMapping><OIDCClaimsMapping><Version>1</Version><ClaimName>user_name</ClaimName><Identifier>GlbStaff.GS_LoginName</Identifier></OIDCClaimsMapping></ArrayOfOIDCClaimsMapping><ArrayOfOIDCScope><OIDCScope><Version>1</Version><ScopeName>dummyIdP</ScopeName></OIDCScope></ArrayOfOIDCScope></OIDCConfig>' AS VARBINARY(MAX)),
NULL,
0,
0,  
'2023-09-03 00:11:00',
'E',
'2023-09-03 00:11:00',
'E')
