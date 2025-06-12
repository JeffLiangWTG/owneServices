

DECLARE @SHIPPING_INSTRUCTION_PK uniqueidentifier = 'A8F34356-F459-4805-8026-9CB072B7E0A3'
DECLARE @OCM_Splitting_PK uniqueidentifier = '5178B543-5EF4-4F40-BDBD-EE7A370661AC'
DECLARE @OCM_MutipleRecipientsCopying_PK uniqueidentifier = '3FAEAF17-8756-421B-A427-28EECB061081'

DECLARE @SHIPPING_INSTRUCTION_RULE uniqueidentifier = (SELECT TOP 1 CC_RR FROM eHubClient WHERE CC_PK = @SHIPPING_INSTRUCTION_PK)
DECLARE @OCM_Splitting_RULE uniqueidentifier = (SELECT TOP 1 CC_RR FROM eHubClient WHERE CC_PK = @OCM_Splitting_PK)
DECLARE @OCM_MutipleRecipientsCopying_RULE uniqueidentifier = (SELECT TOP 1 CC_RR FROM eHubClient WHERE CC_PK = @OCM_MutipleRecipientsCopying_PK)

DECLARE @eHubRoutingRule TABLE
(
	[RR_PK] [uniqueidentifier] NOT NULL,
	[RR_Condition_Expression] [nvarchar](max) NULL,
	[RR_Group_RR_GroupRule] [uniqueidentifier] NULL,
	[RR_Group_MatchMultiple] [bit] NULL,
	[RR_Success_CC_Recipient] [uniqueidentifier] NULL,
	[RR_Success_RR_SubRule] [uniqueidentifier] NULL,
	[RR_Failed_ErrorCode] [nvarchar](50) NULL,
	[RR_Failed_ErrorDescription] [nvarchar](2000) NULL,
	[RR_Failed_RR_SubRule] [uniqueidentifier] NULL,
	[RR_Result_Value] [nvarchar](2000) NULL,
	[RR_Success_SP_Provider] [uniqueidentifier] NULL,
	[RR_LastUpdateUTC] [datetime2](7) NULL,
	[RR_Group_Ordering] [int] NULL,
	[RR_Group_Name] [nvarchar](50) NULL
)

-- Find Rules recursively

;WITH x (RR_PK,	RR_Condition_Expression,	RR_Group_RR_GroupRule,	RR_Group_MatchMultiple,	RR_Success_CC_Recipient,	RR_Success_RR_SubRule,	RR_Failed_ErrorCode,	RR_Failed_ErrorDescription,	RR_Failed_RR_SubRule,	RR_Result_Value,	RR_Success_SP_Provider,	RR_LastUpdateUTC,	RR_Group_Ordering, RR_Group_Name)
 AS
 (
   select RR_PK,	RR_Condition_Expression,	RR_Group_RR_GroupRule,	RR_Group_MatchMultiple,	RR_Success_CC_Recipient,	RR_Success_RR_SubRule,	RR_Failed_ErrorCode,	RR_Failed_ErrorDescription,	RR_Failed_RR_SubRule,	RR_Result_Value,	RR_Success_SP_Provider,	RR_LastUpdateUTC,	RR_Group_Ordering, RR_Group_Name
   from eHubRoutingRule WITH (NOLOCK)
   where RR_PK IN (@SHIPPING_INSTRUCTION_RULE, @OCM_Splitting_RULE, @OCM_MutipleRecipientsCopying_RULE)
   UNION ALL
   select t.RR_PK,	t.RR_Condition_Expression,	t.RR_Group_RR_GroupRule,	t.RR_Group_MatchMultiple,	t.RR_Success_CC_Recipient,	t.RR_Success_RR_SubRule,	t.RR_Failed_ErrorCode,	t.RR_Failed_ErrorDescription,	t.RR_Failed_RR_SubRule,	t.RR_Result_Value,	t.RR_Success_SP_Provider,	t.RR_LastUpdateUTC,	t.RR_Group_Ordering, t.RR_Group_Name
   from eHubRoutingRule as t WITH (NOLOCK) inner join
		x as parent on ((t.RR_Group_RR_GroupRule = parent.RR_PK AND (parent.RR_Group_Name NOT IN ('SplitingWhiteList', 'SplitingBlackList', 'MultipleRecipientsCopying', 'CarrierBookingAgent', 'CarrierHandlingAgent', 'DefaultCarrier') OR parent.RR_Group_Name IS NULL)) OR t.RR_PK = parent.RR_Success_RR_SubRule OR t.RR_PK = parent.RR_Failed_RR_SubRule) 
 )
insert into @eHubRoutingRule SELECT * from x ORDER BY RR_Group_RR_GroupRule, RR_Group_Ordering

-- Find Rules of COMPUTED Facts recursively

;WITH x (RR_PK,	RR_Condition_Expression,	RR_Group_RR_GroupRule,	RR_Group_MatchMultiple,	RR_Success_CC_Recipient,	RR_Success_RR_SubRule,	RR_Failed_ErrorCode,	RR_Failed_ErrorDescription,	RR_Failed_RR_SubRule,	RR_Result_Value,	RR_Success_SP_Provider,	RR_LastUpdateUTC,	RR_Group_Ordering, RR_Group_Name)
 AS
 (
   select RR_PK,	RR_Condition_Expression,	RR_Group_RR_GroupRule,	RR_Group_MatchMultiple,	RR_Success_CC_Recipient,	RR_Success_RR_SubRule,	RR_Failed_ErrorCode,	RR_Failed_ErrorDescription,	RR_Failed_RR_SubRule,	RR_Result_Value,	RR_Success_SP_Provider,	RR_LastUpdateUTC,	RR_Group_Ordering, RR_Group_Name
   from eHubRoutingRule  WITH (NOLOCK)
   where RR_PK IN (SELECT RX_RR_ComputeRule FROM eHubRoutingRuleFact WHERE RX_RR IN (@SHIPPING_INSTRUCTION_RULE, @OCM_Splitting_RULE, @OCM_MutipleRecipientsCopying_RULE) AND RX_RR_ComputeRule IS NOT NULL)
   UNION ALL
   select t.RR_PK,	t.RR_Condition_Expression,	t.RR_Group_RR_GroupRule,	t.RR_Group_MatchMultiple,	t.RR_Success_CC_Recipient,	t.RR_Success_RR_SubRule,	t.RR_Failed_ErrorCode,	t.RR_Failed_ErrorDescription,	t.RR_Failed_RR_SubRule,	t.RR_Result_Value,	t.RR_Success_SP_Provider,	t.RR_LastUpdateUTC,	t.RR_Group_Ordering, t.RR_Group_Name
   from eHubRoutingRule as t  WITH (NOLOCK) inner join
       x as parent on t.RR_Group_RR_GroupRule = parent.RR_PK OR t.RR_PK = parent.RR_Success_RR_SubRule OR t.RR_PK = parent.RR_Failed_RR_SubRule
 )
insert into @eHubRoutingRule SELECT * from x ORDER BY RR_Group_RR_GroupRule, RR_Group_Ordering

UPDATE @eHubRoutingRule
SET RR_LastUpdateUTC = NULL

declare @XML1 xml
declare @XML2 xml
declare @XML3 xml

SET @XML1 = (SELECT * FROM eHubClient WITH (NOLOCK) WHERE CC_PK IN (@SHIPPING_INSTRUCTION_PK, @OCM_Splitting_PK, @OCM_MutipleRecipientsCopying_PK) OR CC_PK IN (SELECT RR_Success_CC_Recipient FROM @eHubRoutingRule WHERE RR_Success_CC_Recipient IS NOT NULL) OR CC_ID IN ('OCM_SUBSHIPMENT_SPLIT', 'OCM_CONTAINER_SPLIT') for xml path ('eHubClient'))
SET @XML2 = (SELECT * FROM eHubRoutingRuleFact WITH (NOLOCK) WHERE RX_RR  IN (@SHIPPING_INSTRUCTION_RULE, @OCM_Splitting_RULE, @OCM_MutipleRecipientsCopying_RULE) for xml path ('eHubRoutingRuleFact'))
SET @XML3 = (SELECT * FROM @eHubRoutingRule for xml path ('eHubRoutingRule'))
DECLARE @siteMapXml XML 

SET @siteMapXml = (SELECT @XML1, @XML2, @XML3 FOR XML PATH(''))

SELECT '<?xml version="1.0" encoding="utf-8" ?><eHubTransactions xmlns="http://tempuri.org/eHubTransactions.xsd"><!-- Autogenerated from database server. Please don''t touch this file! and read ReadMe.md to know how to update this file. --><!-- Autogenerated from database server. Please don''t touch this file! and read ReadMe.md to know how to update this file. --><!-- Autogenerated from database server. Please don''t touch this file! and read ReadMe.md to know how to update this file. -->' + CAST(@siteMapXml as nvarchar(max)) + '</eHubTransactions>'
