/*
TODO
----
1. Consider creating non-clustered primary keys for transactional tables if using GUIDs.  Will aid in write performance
2. Make outbox envelope's sender PK + message tracking ID a unique index
*/
------------------------------------------------------------------------------------------------------------------------------------------------
-- Drop And Create eHubService Login And User
------------------------------------------------------------------------------------------------------------------------------------------------

/*
USE [eHubTransactions]
GO

IF  EXISTS (SELECT * FROM sys.database_principals WHERE name = N'CORPORATE\ehubservice')
DROP USER [CORPORATE\ehubservice]
GO

USE [master]
GO

IF  EXISTS (SELECT * FROM sys.server_principals WHERE name = N'CORPORATE\ehubservice')
DROP LOGIN [CORPORATE\ehubservice]
GO

CREATE LOGIN [CORPORATE\ehubservice] FROM WINDOWS WITH DEFAULT_DATABASE=[master], DEFAULT_LANGUAGE=[us_english]
GO

USE [eHubTransactions]
GO

CREATE USER [CORPORATE\ehubservice] FOR LOGIN [CORPORATE\ehubservice] WITH DEFAULT_SCHEMA=[dbo]
GO
*/

------------------------------------------------------------------------------------------------------------------------------------------------
-- Drop Tables
------------------------------------------------------------------------------------------------------------------------------------------------

USE [eHubTransactions]
GO

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_CC_PK]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[eHubClient] DROP CONSTRAINT [DF_CC_PK]
END
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[eHubClientAccess_CP_Client_FK2_eHubClient_RRR_121]') AND parent_object_id = OBJECT_ID(N'[dbo].[eHubClientAccess]'))
ALTER TABLE [dbo].[eHubClientAccess] DROP CONSTRAINT [eHubClientAccess_CP_Client_FK2_eHubClient_RRR_121]
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[eHubClientCode_CM_CC_FK2_eHubClient_RRR_121]') AND parent_object_id = OBJECT_ID(N'[dbo].[eHubClientCode]'))
ALTER TABLE [dbo].[eHubClientCode] DROP CONSTRAINT [eHubClientCode_CM_CC_FK2_eHubClient_RRR_121]
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[eHubClientCode_CM_SC_FK2_eHubStandardCode_RRR_121]') AND parent_object_id = OBJECT_ID(N'[dbo].[eHubClientCode]'))
ALTER TABLE [dbo].[eHubClientCode] DROP CONSTRAINT [eHubClientCode_CM_SC_FK2_eHubStandardCode_RRR_121]
GO

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_CM_PK]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[eHubClientCode] DROP CONSTRAINT [DF_CM_PK]
END

GO

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_CT_PK]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[eHubCodeType] DROP CONSTRAINT [DF_CT_PK]
END

GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[eHubMessage_DC_DT_Target_FK2_eHubMessageType_RRR_121]') AND parent_object_id = OBJECT_ID(N'[dbo].[eHubMessage]'))
ALTER TABLE [dbo].[eHubMessage] DROP CONSTRAINT [eHubMessage_DC_DT_Target_FK2_eHubMessageType_RRR_121]
GO

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_DC_PK]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[eHubMessage] DROP CONSTRAINT [DF_DC_PK]
END

GO

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_DT_PK]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[eHubMessageType] DROP CONSTRAINT [DF_DT_PK]
END

GO

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF__eHubMessa__DT_Is__52593CB8]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[eHubMessageType] DROP CONSTRAINT [DF__eHubMessa__DT_Is__52593CB8]
END

GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[eHubOutboxEnvelope_OI_CC_Recipient_FK2_eHubClient_RRR_121]') AND parent_object_id = OBJECT_ID(N'[dbo].[eHubOutboxEnvelope]'))
ALTER TABLE [dbo].[eHubOutboxEnvelope] DROP CONSTRAINT [eHubOutboxEnvelope_OI_CC_Recipient_FK2_eHubClient_RRR_121]
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[eHubOutboxEnvelope_OI_CC_Sender_FK2_eHubClient_RRR_121]') AND parent_object_id = OBJECT_ID(N'[dbo].[eHubOutboxEnvelope]'))
ALTER TABLE [dbo].[eHubOutboxEnvelope] DROP CONSTRAINT [eHubOutboxEnvelope_OI_CC_Sender_FK2_eHubClient_RRR_121]
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[eHubOutboxEnvelope_OI_DC_Message_FK2_eHubMessage_RRR_121]') AND parent_object_id = OBJECT_ID(N'[dbo].[eHubOutboxEnvelope]'))
ALTER TABLE [dbo].[eHubOutboxEnvelope] DROP CONSTRAINT [eHubOutboxEnvelope_OI_DC_Message_FK2_eHubMessage_RRR_121]
GO

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_OI_PK]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[eHubOutboxEnvelope] DROP CONSTRAINT [DF_OI_PK]
END

GO

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF__eHubOutbo__OI_UT__4222D4EF]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[eHubOutboxEnvelope] DROP CONSTRAINT [DF__eHubOutbo__OI_UT__4222D4EF]
END

GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[eHubStandardCode_SC_CT_FK2_eHubCodeType_RRR_121]') AND parent_object_id = OBJECT_ID(N'[dbo].[eHubStandardCode]'))
ALTER TABLE [dbo].[eHubStandardCode] DROP CONSTRAINT [eHubStandardCode_SC_CT_FK2_eHubCodeType_RRR_121]
GO

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_SC_PK]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[eHubStandardCode] DROP CONSTRAINT [DF_SC_PK]
END

GO

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_eHubTransformationSet_TS_PK]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[eHubTransformationSet] DROP CONSTRAINT [DF_eHubTransformationSet_TS_PK]
END

GO

/****** Object:  Table [dbo].[eHubClient]    Script Date: 07/21/2010 15:09:28 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[eHubClient]') AND type in (N'U'))
DROP TABLE [dbo].[eHubClient]
GO

/****** Object:  Table [dbo].[eHubClientAccess]    Script Date: 08/31/2010 14:49:31 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[eHubClientAccess]') AND type in (N'U'))
DROP TABLE [dbo].[eHubClientAccess]
GO

/****** Object:  Table [dbo].[eHubClientCode]    Script Date: 07/21/2010 15:09:28 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[eHubClientCode]') AND type in (N'U'))
DROP TABLE [dbo].[eHubClientCode]
GO

/****** Object:  Table [dbo].[eHubClientRelationship]    Script Date: 07/28/2010 09:37:09 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[eHubClientRelationship]') AND type in (N'U'))
DROP TABLE [dbo].[eHubClientRelationship]
GO

/****** Object:  Table [dbo].[eHubCodeType]    Script Date: 07/21/2010 15:09:28 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[eHubCodeType]') AND type in (N'U'))
DROP TABLE [dbo].[eHubCodeType]
GO

/****** Object:  Table [dbo].[eHubMessage]    Script Date: 07/21/2010 15:09:28 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[eHubMessage]') AND type in (N'U'))
DROP TABLE [dbo].[eHubMessage]
GO

/****** Object:  Table [dbo].[eHubMessageType]    Script Date: 07/21/2010 15:09:28 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[eHubMessageType]') AND type in (N'U'))
DROP TABLE [dbo].[eHubMessageType]
GO

/****** Object:  Table [dbo].[eHubOutboxEnvelope]    Script Date: 07/21/2010 15:09:28 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[eHubOutboxEnvelope]') AND type in (N'U'))
DROP TABLE [dbo].[eHubOutboxEnvelope]
GO

/****** Object:  Table [dbo].[eHubStandardCode]    Script Date: 07/21/2010 15:09:28 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[eHubStandardCode]') AND type in (N'U'))
DROP TABLE [dbo].[eHubStandardCode]
GO

/****** Object:  Table [dbo].[eHubTransformationMapping]    Script Date: 07/21/2010 15:09:28 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[eHubTransformationMapping]') AND type in (N'U'))
DROP TABLE [dbo].[eHubTransformationMapping]
GO

/****** Object:  Table [dbo].[eHubTransformationSet]    Script Date: 07/21/2010 15:09:28 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[eHubTransformationSet]') AND type in (N'U'))
DROP TABLE [dbo].[eHubTransformationSet]
GO

/****** Object:  Table [dbo].[eHubTransformationType]    Script Date: 07/21/2010 15:09:28 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[eHubTransformationType]') AND type in (N'U'))
DROP TABLE [dbo].[eHubTransformationType]
GO

/****** Object:  Table [dbo].[eHubZone]    Script Date: 08/23/2010 14:43:30 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[eHubZone]') AND type in (N'U'))
DROP TABLE [dbo].[eHubZone]
GO

------------------------------------------------------------------------------------------------------------------------------------------------
-- Table Creation
------------------------------------------------------------------------------------------------------------------------------------------------

/****** Object:  Table [dbo].[eHubClient]    Script Date: 07/21/2010 15:09:28 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[eHubClient](
	[CC_PK] [uniqueidentifier] NOT NULL,
	[CC_ID] [varchar](36) NOT NULL,
	[CC_FriendlyName] [varchar](128) NOT NULL DEFAULT(''),
	[CC_Odyssey_OH] [uniqueidentifier] NOT NULL,
	[CC_DistributionZone] [uniqueidentifier] NULL,
	[CC_EmailAddress] [varchar](128) NOT NULL,
	[CC_Password] [varchar](200) NOT NULL,
 CONSTRAINT [PK_UX__CC_PK] PRIMARY KEY CLUSTERED 
(
	[CC_PK] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

/****** Object:  Table [dbo].[eHubClientAccess]    Script Date: 08/31/2010 14:47:45 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[eHubClientAccess](
	[CP_PK] [uniqueidentifier] NOT NULL,
	[CP_Client] [uniqueidentifier] NOT NULL,
	[CP_Permisision] [varchar](128) NOT NULL,
 CONSTRAINT [PK_UX__CP_PK] PRIMARY KEY CLUSTERED 
(
	[CP_PK] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO


/****** Object:  Table [dbo].[eHubClientCode]    Script Date: 07/21/2010 15:09:28 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[eHubClientCode](
	[CM_PK] [uniqueidentifier] NOT NULL,
	[CM_SC] [uniqueidentifier] NOT NULL,
	[CM_ClientCode] [varchar](35) NOT NULL,
	[CM_CC] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_UX__CM_PK] PRIMARY KEY CLUSTERED 
(
	[CM_PK] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

/****** Object:  Table [dbo].[eHubClientRelationship]    Script Date: 07/28/2010 09:16:38 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[eHubClientRelationship](
	[CR_CC_Sender] [uniqueidentifier] NOT NULL,
	[CR_CC_Recipient] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_eHubClientRelationship] PRIMARY KEY CLUSTERED 
(
	[CR_CC_Sender] ASC,
	[CR_CC_Recipient] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

USE [eHubTransactions]
GO

/****** Object:  Table [dbo].[eHubCodeType]    Script Date: 07/21/2010 15:09:28 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[eHubCodeType](
	[CT_PK] [uniqueidentifier] NOT NULL,
	[CT_Name] [varchar](35) NOT NULL,
	[CT_Description] [varchar](128) NOT NULL,
 CONSTRAINT [PK_UX__CT_PK] PRIMARY KEY CLUSTERED 
(
	[CT_PK] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

USE [eHubTransactions]
GO

/****** Object:  Table [dbo].[eHubMessage]    Script Date: 07/21/2010 15:09:28 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[eHubMessage](
	[DC_PK] [uniqueidentifier] NOT NULL,
	[DC_Content] [xml] NOT NULL,
	[DC_DT_Target] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_UX__DC_PK] PRIMARY KEY CLUSTERED 
(
	[DC_PK] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

USE [eHubTransactions]
GO

/****** Object:  Table [dbo].[eHubMessageType]    Script Date: 07/21/2010 15:09:28 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[eHubMessageType](
	[DT_PK] [uniqueidentifier] NOT NULL,
	[DT_Code] [varchar](200) NOT NULL,
	[DT_IsFlatFile] [bit] NOT NULL,
 CONSTRAINT [PK_UX__DT_PK] PRIMARY KEY CLUSTERED 
(
	[DT_PK] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

USE [eHubTransactions]
GO

/****** Object:  Table [dbo].[eHubOutboxEnvelope]    Script Date: 07/21/2010 15:09:28 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[eHubOutboxEnvelope](
	[OI_PK] [uniqueidentifier] NOT NULL,
	[OI_CC_Sender] [uniqueidentifier] NOT NULL,
	[OI_CC_Recipient] [uniqueidentifier] NOT NULL,
	[OI_DC_Message] [uniqueidentifier] NULL,
	[OI_InternalTrackingID] [varchar](36) NOT NULL,
	[OI_EnvelopeTrackingID] [varchar](36) NULL,
	[OI_MessageTrackingID] [varchar](36) NULL,
	[OI_OverrideFilename] [nvarchar](512) NULL,
	[OI_OverrideEmailSubject] [nvarchar](512) NULL,
	[OI_IsBatch] [bit] NOT NULL,
	[OI_Status] [smallint] NOT NULL,
	[OI_InsertUTC] [datetime] NOT NULL,
	[OI_LastUpdateUTC] [datetime] NULL,
 CONSTRAINT [PK_UX__OI_PK] PRIMARY KEY CLUSTERED 
(
	[OI_PK] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'-1 = Faulted, 0 = AwaitingDelivery, 1 = Delivered' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'eHubOutboxEnvelope', @level2type=N'COLUMN',@level2name=N'OI_Status'
GO

USE [eHubTransactions]
GO

/****** Object:  Table [dbo].[eHubStandardCode]    Script Date: 07/21/2010 15:09:28 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[eHubStandardCode](
	[SC_PK] [uniqueidentifier] NOT NULL,
	[SC_Code] [varchar](35) NOT NULL,
	[SC_Description] [varchar](128) NOT NULL,
	[SC_CT] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_UX__SC_PK] PRIMARY KEY CLUSTERED 
(
	[SC_PK] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

USE [eHubTransactions]
GO

/****** Object:  Table [dbo].[eHubTransformationMapping]    Script Date: 07/21/2010 15:09:28 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[eHubTransformationMapping](
	[TM_TS_PK] [uniqueidentifier] NOT NULL,
	[TM_Order] [tinyint] NOT NULL,
	[TM_TT_PK] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_eHubTransformationMapping] PRIMARY KEY CLUSTERED 
(
	[TM_TS_PK] ASC,
	[TM_Order] ASC,
	[TM_TT_PK] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

USE [eHubTransactions]
GO

/****** Object:  Table [dbo].[eHubTransformationSet]    Script Date: 07/21/2010 15:09:28 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[eHubTransformationSet](
	[TS_PK] [uniqueidentifier] NOT NULL,
	[TS_CC_Sender] [uniqueidentifier] NOT NULL,
	[TS_CC_Recipient] [uniqueidentifier] NOT NULL,
	[TS_DT_Source] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_eHubTransformationSet] PRIMARY KEY CLUSTERED 
(
	[TS_PK] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

USE [eHubTransactions]
GO

/****** Object:  Table [dbo].[eHubTransformationType]    Script Date: 07/21/2010 15:09:28 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[eHubTransformationType](
	[TT_PK] [uniqueidentifier] NOT NULL,
	[TT_DT_Source] [uniqueidentifier] NOT NULL,
	[TT_DT_Target] [uniqueidentifier] NOT NULL,
	[TT_TransformationType] [varchar](512) NULL,
 CONSTRAINT [PK_UX__TT_PK] PRIMARY KEY CLUSTERED 
(
	[TT_PK] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

/****** Object:  Table [dbo].[eHubZone]    Script Date: 08/23/2010 14:42:53 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[eHubZone](
	[ZZ_PK] [uniqueidentifier] NOT NULL,
	[ZZ_ID] [varchar](50) NOT NULL,
 CONSTRAINT [PK_eHubZone] PRIMARY KEY CLUSTERED 
(
	[ZZ_PK] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

------------------------------------------------------------------------------------------------------------------------------------------------
-- Foreign Key Assignment
------------------------------------------------------------------------------------------------------------------------------------------------

ALTER TABLE [dbo].[eHubClient] ADD  CONSTRAINT [DF_CC_PK]  DEFAULT (newid()) FOR [CC_PK]
GO

ALTER TABLE [dbo].[eHubClientAccess]  WITH CHECK ADD  CONSTRAINT [eHubClientAccess_CP_Client_FK2_eHubClient_RRR_121] FOREIGN KEY([CP_Client])
REFERENCES [dbo].[eHubClient] ([CC_PK])
GO


ALTER TABLE [dbo].[eHubClientCode]  WITH CHECK ADD  CONSTRAINT [eHubClientCode_CM_CC_FK2_eHubClient_RRR_121] FOREIGN KEY([CM_CC])
REFERENCES [dbo].[eHubClient] ([CC_PK])
GO

ALTER TABLE [dbo].[eHubClientCode] CHECK CONSTRAINT [eHubClientCode_CM_CC_FK2_eHubClient_RRR_121]
GO

ALTER TABLE [dbo].[eHubClientCode]  WITH CHECK ADD  CONSTRAINT [eHubClientCode_CM_SC_FK2_eHubStandardCode_RRR_121] FOREIGN KEY([CM_SC])
REFERENCES [dbo].[eHubStandardCode] ([SC_PK])
GO

ALTER TABLE [dbo].[eHubClientCode] CHECK CONSTRAINT [eHubClientCode_CM_SC_FK2_eHubStandardCode_RRR_121]
GO

ALTER TABLE [dbo].[eHubClientCode] ADD  CONSTRAINT [DF_CM_PK]  DEFAULT (newid()) FOR [CM_PK]
GO

ALTER TABLE [dbo].[eHubCodeType] ADD  CONSTRAINT [DF_CT_PK]  DEFAULT (newid()) FOR [CT_PK]
GO

ALTER TABLE [dbo].[eHubMessage]  WITH CHECK ADD  CONSTRAINT [eHubMessage_DC_DT_Target_FK2_eHubMessageType_RRR_121] FOREIGN KEY([DC_DT_Target])
REFERENCES [dbo].[eHubMessageType] ([DT_PK])
GO

ALTER TABLE [dbo].[eHubMessage] CHECK CONSTRAINT [eHubMessage_DC_DT_Target_FK2_eHubMessageType_RRR_121]
GO

ALTER TABLE [dbo].[eHubMessage] ADD  CONSTRAINT [DF_DC_PK]  DEFAULT (newid()) FOR [DC_PK]
GO

ALTER TABLE [dbo].[eHubMessageType] ADD  CONSTRAINT [DF_DT_PK]  DEFAULT (newid()) FOR [DT_PK]
GO

ALTER TABLE [dbo].[eHubMessageType] ADD  DEFAULT ((0)) FOR [DT_IsFlatFile]
GO

ALTER TABLE [dbo].[eHubOutboxEnvelope]  WITH CHECK ADD  CONSTRAINT [eHubOutboxEnvelope_OI_CC_Recipient_FK2_eHubClient_RRR_121] FOREIGN KEY([OI_CC_Recipient])
REFERENCES [dbo].[eHubClient] ([CC_PK])
GO

ALTER TABLE [dbo].[eHubOutboxEnvelope] CHECK CONSTRAINT [eHubOutboxEnvelope_OI_CC_Recipient_FK2_eHubClient_RRR_121]
GO

ALTER TABLE [dbo].[eHubOutboxEnvelope]  WITH CHECK ADD  CONSTRAINT [eHubOutboxEnvelope_OI_CC_Sender_FK2_eHubClient_RRR_121] FOREIGN KEY([OI_CC_Sender])
REFERENCES [dbo].[eHubClient] ([CC_PK])
GO

ALTER TABLE [dbo].[eHubOutboxEnvelope] CHECK CONSTRAINT [eHubOutboxEnvelope_OI_CC_Sender_FK2_eHubClient_RRR_121]
GO

ALTER TABLE [dbo].[eHubOutboxEnvelope] ADD  CONSTRAINT [DF_OI_PK]  DEFAULT (newid()) FOR [OI_PK]
GO

ALTER TABLE [dbo].[eHubOutboxEnvelope] ADD  DEFAULT (GETUTCDATE()) FOR [OI_InsertUTC]
GO

ALTER TABLE [dbo].[eHubStandardCode]  WITH CHECK ADD  CONSTRAINT [eHubStandardCode_SC_CT_FK2_eHubCodeType_RRR_121] FOREIGN KEY([SC_CT])
REFERENCES [dbo].[eHubCodeType] ([CT_PK])
GO

ALTER TABLE [dbo].[eHubStandardCode] CHECK CONSTRAINT [eHubStandardCode_SC_CT_FK2_eHubCodeType_RRR_121]
GO

ALTER TABLE [dbo].[eHubStandardCode] ADD  CONSTRAINT [DF_SC_PK]  DEFAULT (newid()) FOR [SC_PK]
GO

ALTER TABLE [dbo].[eHubTransformationSet] ADD  CONSTRAINT [DF_eHubTransformationSet_TS_PK]  DEFAULT (newid()) FOR [TS_PK]
GO

------------------------------------------------------------------------------------------------------------------------------------------------
-- Drop Stored Procedures
------------------------------------------------------------------------------------------------------------------------------------------------

USE [eHubTransactions]
GO

/****** Object:  StoredProcedure [dbo].[DeleteOutboxEnvelopeByIsDeliveredUTC]    Script Date: 08/24/2010 11:42:12 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DeleteOutboxEnvelopeByIsDeliveredUTC]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[DeleteOutboxEnvelopeByIsDeliveredUTC]
GO

/****** Object:  StoredProcedure [dbo].[DeleteOutboxEnvelopeByTrackingID]    Script Date: 08/24/2010 11:42:12 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DeleteOutboxEnvelopeByTrackingID]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[DeleteOutboxEnvelopeByTrackingID]
GO

/****** Object:  StoredProcedure [dbo].[InsertOutboxMessage]    Script Date: 08/24/2010 11:42:12 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InsertOutboxMessage]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[InsertOutboxMessage]
GO

/****** Object:  StoredProcedure [dbo].[InsertOutboxEnvelope]    Script Date: 09/01/2010 12:10:49 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InsertOutboxEnvelope]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[InsertOutboxEnvelope]
GO

/****** Object:  StoredProcedure [dbo].[SelectClientExists]    Script Date: 08/24/2010 11:42:12 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SelectClientExists]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[SelectClientExists]
GO

/****** Object:  StoredProcedure [dbo].[CheckAccess]    Script Date: 08/31/2010 15:05:53 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CheckAccess]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[CheckAccess]
GO

/****** Object:  StoredProcedure [dbo].[SelectClientRelationshipExists]    Script Date: 08/24/2010 11:42:12 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SelectClientRelationshipExists]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[SelectClientRelationshipExists]
GO

/****** Object:  StoredProcedure [dbo].[SelectOutboxEnvelopesBySchedule]    Script Date: 08/24/2010 11:42:12 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SelectOutboxEnvelopesBySchedule]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[SelectOutboxEnvelopesBySchedule]
GO

/****** Object:  StoredProcedure [dbo].[SelectOutboxMessageBatchByRecipientID]    Script Date: 08/24/2010 11:42:12 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SelectOutboxMessageBatchByRecipientID]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[SelectOutboxMessageBatchByRecipientID]
GO

/****** Object:  StoredProcedure [dbo].[SelectOutboxMessageByEnvelopeID]    Script Date: 08/24/2010 11:42:12 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SelectOutboxMessageByEnvelopeID]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[SelectOutboxMessageByEnvelopeID]
GO

/****** Object:  StoredProcedure [dbo].[SelectOutboxMessagesByRecipientID]    Script Date: 08/24/2010 11:42:12 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SelectOutboxMessagesByRecipientID]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[SelectOutboxMessagesByRecipientID]
GO

/****** Object:  StoredProcedure [dbo].[SelectOutboxScheduledEnvelopeCount]    Script Date: 08/24/2010 11:42:13 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SelectOutboxScheduledEnvelopeCount]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[SelectOutboxScheduledEnvelopeCount]
GO

/****** Object:  StoredProcedure [dbo].[SelectTransformsByPartiesMessage]    Script Date: 08/24/2010 11:42:13 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SelectTransformsByPartiesMessage]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[SelectTransformsByPartiesMessage]
GO

/****** Object:  StoredProcedure [dbo].[ValidatePassword]    Script Date: 08/24/2010 11:42:13 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ValidatePassword]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[ValidatePassword]
GO

/****** Object:  StoredProcedure [dbo].[ValidatePassword]    Script Date: 08/24/2010 11:42:13 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdatePassword]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdatePassword]
GO

/****** Object:  StoredProcedure [dbo].[SelectEnvelopeStatusByTrackingID]    Script Date: 09/03/2010 09:46:40 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SelectEnvelopeStatusByTrackingID]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[SelectEnvelopeStatusByTrackingID]
GO
------------------------------------------------------------------------------------------------------------------------------------------------
-- Create Stored Procedures
------------------------------------------------------------------------------------------------------------------------------------------------

USE [eHubTransactions]
GO

/****** Object:  StoredProcedure [dbo].[DeleteOutboxEnvelopeByIsDeliveredUTC]    Script Date: 08/24/2010 11:41:19 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[DeleteOutboxEnvelopeByIsDeliveredUTC]
	@DaysToKeep SMALLINT
AS

DELETE
	dbo.eHubOutboxEnvelope
WHERE
	OI_Status = 1 AND
	OI_InsertUTC <= DATEADD(HOUR, @DaysToKeep * -24, GETUTCDATE())

GO

/****** Object:  StoredProcedure [dbo].[DeleteOutboxEnvelopeByTrackingID]    Script Date: 08/24/2010 11:41:19 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[DeleteOutboxEnvelopeByTrackingID] @TrackingID VARCHAR(36)
AS

IF @TrackingID IS NULL OR NOT EXISTS ( SELECT 1 FROM eHubOutboxEnvelope WHERE OI_EnvelopeTrackingID = @TrackingID)
BEGIN
	DECLARE @errorMsg VARCHAR(400)
	SET @errorMsg = 'Could not find batch with tracking ID: ' + @TrackingID
	RAISERROR(@errorMsg, 16, 1)
	RETURN
END

DECLARE @EnvelopePK uniqueidentifier
DECLARE @MessagePK uniqueidentifier
DECLARE @EnvelopeCountLinkedToMessage int

SELECT @EnvelopePK = OI_PK, @MessagePK = OI_DC_Message FROM eHubOutboxEnvelope WHERE OI_EnvelopeTrackingID = @TrackingID
SELECT @EnvelopeCountLinkedToMessage = COUNT(*) FROM eHubOutboxEnvelope WHERE OI_DC_Message = @MessagePK

DELETE eHubOutboxEnvelope WHERE OI_EnvelopeTrackingID = @TrackingID
IF (@EnvelopeCountLinkedToMessage = 1)
BEGIN
	DELETE FROM eHubMessage WHERE DC_PK = @MessagePK
END
GO

/****** Object:  StoredProcedure [dbo].[InsertOutboxMessage]    Script Date: 09/01/2010 12:11:18 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[InsertOutboxMessage]
@MessagePK VARCHAR(36)
,@TargetMessageType VARCHAR(256)
,@Content XML
AS

IF NOT EXISTS ( SELECT 1 FROM [dbo].[eHubMessageType] WITH (NOLOCK) WHERE DT_Code = @TargetMessageType )
BEGIN
	DECLARE @errorMsg VARCHAR(400)
	SET @errorMsg = 'Message type not registered in database ' + @TargetMessageType
	RAISERROR(@errorMsg, 16, 1)
	RETURN
END

DECLARE @MessageTypePK UNIQUEIDENTIFIER
SELECT @MessageTypePK = DT_PK FROM [dbo].[eHubMessageType] WITH (NOLOCK) WHERE DT_Code = @TargetMessageType

-- Check this record has not been inserted before AND it has a reference in the envelope table
IF NOT EXISTS ( SELECT 1 FROM [dbo].[eHubMessage] WITH (NOLOCK) WHERE DC_PK = @MessagePK )
AND EXISTS ( SELECT 1 FROM [dbo].[eHubOutboxEnvelope] WITH (NOLOCK) WHERE OI_DC_Message = @MessagePK )
BEGIN
	INSERT INTO
		[dbo].[eHubMessage] WITH (ROWLOCK) (
		DC_PK,
		DC_Content,
		DC_DT_Target)
	VALUES (
		@MessagePK,
		@Content,
		@MessageTypePK)
END
GO

/****** Object:  StoredProcedure [dbo].[InsertOutboxEnvelope]    Script Date: 09/03/2010 15:42:34 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[InsertOutboxEnvelope]
@MessagePK VARCHAR(36) = NULL
,@SenderID VARCHAR(36)
,@RecipientID VARCHAR(36)
,@EnvelopeTrackingID VARCHAR(36)
,@MessageTrackingID VARCHAR(36)
,@InternalTrackingID VARCHAR(36)
,@IsBatch BIT
,@OverrideEmailSubject NVARCHAR(512) = NULL
,@OverrideFilename NVARCHAR(512) =  NULL
,@Status SMALLINT = 0 -- -1 = Faulted, 0 = Ready For Distribution
AS
DECLARE @errorMsg VARCHAR(400)
IF NOT EXISTS ( SELECT 1 FROM [dbo].[eHubClient] WITH (NOLOCK) WHERE CC_ID = @SenderID )
BEGIN
	SET @errorMsg = 'sender not there ' + @SenderID
	RAISERROR(@errorMsg, 16, 1)
	RETURN
END
DECLARE @SenderPK UNIQUEIDENTIFIER
SELECT @SenderPK = CC_PK FROM [dbo].[eHubClient] WITH (NOLOCK) WHERE CC_ID = @SenderID
IF NOT EXISTS ( SELECT 1 FROM [dbo].[eHubClient] WITH (NOLOCK) WHERE CC_ID = @RecipientID )
BEGIN
	SET @errorMsg = 'recipient not there'
	RAISERROR(@errorMsg, 16, 1)
	RETURN
END
DECLARE @RecipientPK UNIQUEIDENTIFIER
SELECT @RecipientPK = CC_PK FROM [dbo].[eHubClient] WITH (NOLOCK) WHERE CC_ID = @RecipientID

-- Check if this record has been inserted before
IF NOT EXISTS ( SELECT 1 FROM [dbo].[eHubOutboxEnvelope] WITH (NOLOCK) WHERE OI_CC_Sender = @SenderPK AND OI_MessageTrackingID = @MessageTrackingID )
BEGIN	
	INSERT INTO
		[dbo].[eHubOutboxEnvelope] WITH (ROWLOCK) (
		OI_PK,
		OI_CC_Sender,
		OI_CC_Recipient,
		OI_DC_Message,
		OI_InternalTrackingID,
		OI_EnvelopeTrackingID,
		OI_MessageTrackingID,
		OI_OverrideEmailSubject,
		OI_OverrideFilename,
		OI_IsBatch,
		OI_Status,
		OI_InsertUTC,
		OI_LastUpdateUTC
		)
	VALUES (
		NEWID(),
		@SenderPK,
		@RecipientPK,
		@MessagePK,
		@InternalTrackingID,
		@EnvelopeTrackingID,
		@MessageTrackingID,
		@OverrideEmailSubject,
		@OverrideFilename,
		@IsBatch,
		@Status,
		GETUTCDATE(),
		NULL)
END
-- So, the envelope exists.  Check if it is currently a failure and we are inserting 'ready for distribution' envelope and message record PK was provided
ELSE IF @Status = 0 AND RTRIM(LTRIM(ISNULL(@MessagePK, ''))) <> '' AND EXISTS ( SELECT 1 FROM [dbo].[eHubOutboxEnvelope] WITH (NOLOCK) WHERE OI_CC_Sender = @SenderPK AND OI_MessageTrackingID = @MessageTrackingID AND OI_Status = -1 )
BEGIN
	DECLARE @EnvelopePK UNIQUEIDENTIFIER
	SELECT @EnvelopePK = OI_PK FROM [dbo].[eHubOutboxEnvelope] WITH (NOLOCK) WHERE OI_CC_Sender = @SenderPK AND OI_MessageTrackingID = @MessageTrackingID
	
	-- Update the envelope record indicating it's ready to distribute
	UPDATE
		[dbo].[eHubOutboxEnvelope] WITH (ROWLOCK)
	SET
		OI_DC_Message = @MessagePK,
		OI_Status = 0,
		OI_LastUpdateUTC = GETUTCDATE()
	WHERE
		OI_PK = @EnvelopePK
END
GO

/****** Object:  StoredProcedure [dbo].[SelectClientExists]    Script Date: 08/24/2010 11:41:19 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[SelectClientExists]
	@ID VARCHAR(36)
AS

IF EXISTS ( SELECT 1 FROM dbo.eHubClient WITH (NOLOCK) WHERE CC_ID = @ID )
	SELECT CAST(1 AS BIT)
ELSE
	SELECT CAST(0 AS BIT)

GO

/****** Object:  StoredProcedure [dbo].[CheckAccess]    Script Date: 08/31/2010 15:03:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[CheckAccess] @ClientID VARCHAR(36), @Operation VARCHAR(128)
AS
DECLARE @allowed bit = 0

select @allowed = 1 from eHubClientAccess with (nolock) 
inner join eHubClient with (nolock) on CP_Client = CC_PK and CC_ID = @ClientID
where CP_Permisision= @Operation

select @allowed
GO
/****** Object:  StoredProcedure [dbo].[SelectClientRelationshipExists]    Script Date: 08/24/2010 11:41:19 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[SelectClientRelationshipExists]
	@SenderID VARCHAR(36),
	@RecipientID VARCHAR(36)
AS

DECLARE @Exists BIT = 0
SELECT
	@Exists = 1
FROM
	[dbo].[eHubClientRelationship] cr WITH (NOLOCK)
INNER JOIN
	[dbo].[eHubClient] s WITH (NOLOCK) ON s.CC_PK = cr.CR_CC_Sender
INNER JOIN
	[dbo].[eHubClient] r WITH (NOLOCK) ON r.CC_PK = cr.CR_CC_Recipient
WHERE
	s.CC_ID = @SenderID AND
	r.CC_ID = @RecipientID

SELECT @Exists

GO

/****** Object:  StoredProcedure [dbo].[SelectOutboxEnvelopesBySchedule]    Script Date: 08/24/2010 11:41:19 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[SelectOutboxEnvelopesBySchedule]
	@DistributionZone VARCHAR(50)
AS

SET FMTONLY OFF

DECLARE @CurrentHighPrecisionUtcDateTime DATETIME
SET @CurrentHighPrecisionUtcDateTime = GETUTCDATE()

DECLARE @CurrentUtcDateTime DATETIME
SELECT @CurrentUtcDateTime = DATEADD(MILLISECOND, -DATEPART(MILLISECOND, @CurrentHighPrecisionUtcDateTime), @CurrentHighPrecisionUtcDateTime)
SELECT @CurrentUtcDateTime = DATEADD(SECOND, -DATEPART(SECOND, @CurrentUtcDateTime), @CurrentUtcDateTime)

DECLARE @CurrentUtcTime TIME
SELECT @CurrentUtcTime = CONVERT(TIME, @CurrentUtcDateTime)

--Select the data 
SELECT
	oe.OI_PK AS MessageEnvelopeID
INTO
	#Envelopes
FROM
	dbo.eHubOutboxEnvelope oe WITH (NOLOCK)
INNER JOIN
	dbo.eHubClient c WITH (NOLOCK) ON c.CC_PK = oe.OI_CC_Recipient
INNER JOIN
	dbo.eHubZone z WITH (NOLOCK) ON z.ZZ_PK = c.CC_DistributionZone
WHERE
	oe.OI_IsBatch = 0 AND
	oe.OI_Status = 0 AND
	z.ZZ_ID = @DistributionZone

-- Return the envelope IDs to the caller
SELECT
	MessageEnvelopeID
FROM
	#Envelopes

UPDATE
      dbo.eHubOutboxMessage WITH (ROWLOCK)
SET
      OI_Status = 1
      ,OI_LastUpdateUTC = @CurrentDateTimeUTC
WHERE
      OI_PK in (SELECT MessageEnvelopeID FROM #Envelopes)

GO

/****** Object:  StoredProcedure [dbo].[SelectOutboxMessageBatchByRecipientID]    Script Date: 08/24/2010 11:41:19 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[SelectOutboxMessageBatchByRecipientID] 
	@RecipientID VARCHAR(36)
AS

SELECT TOP 1
	@RecipientID AS RecipientID,	
	oe.OI_InternalTrackingID AS InternalTrackingID,
	oe.OI_EnvelopeTrackingID AS RecipientTrackingID,
	DC_Content AS MessageContent,
	datalength(m.DC_Content) AS MessageSize,
	mt.DT_Code AS MessageType,
	mt.DT_IsFlatFile AS IsFlatFileSchema
FROM
	dbo.eHubOutboxEnvelope oe WITH (NOLOCK)
INNER JOIN
	dbo.eHubClient c ON c.CC_PK = oe.OI_CC_Recipient	
INNER JOIN
	dbo.eHubMessage m WITH (NOLOCK) ON m.DC_PK = oe.OI_DC_Message
INNER JOIN
	dbo.eHubMessageType mt WITH (NOLOCK) ON mt.DT_PK = m.DC_DT_Target
WHERE
	oe.OI_IsBatch = 1 AND
	oe.OI_Status = 0 AND
	c.CC_ID = @RecipientID

GO

/****** Object:  StoredProcedure [dbo].[SelectOutboxMessageByEnvelopeID]    Script Date: 08/24/2010 11:41:19 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[SelectOutboxMessageByEnvelopeID] 
@MessageEnvelopeID VARCHAR(36)
AS

DECLARE @MessageID UNIQUEIDENTIFIER
SELECT @MessageID = OI_DC_Message FROM dbo.eHubOutboxEnvelope WITH (NOLOCK) WHERE OI_PK = @MessageEnvelopeID

--Select the data 
SELECT
	r.CC_ID AS RecipientID,
	s.CC_ID AS SenderID,
	oe.OI_InternalTrackingID AS InternalTrackingID,
	oe.OI_EnvelopeTrackingID AS EnvelopeTrackingID,
	oe.OI_MessageTrackingID AS MessageTrackingID,
	m.DC_Content AS MessageContent,
	mt.DT_Code AS MessageType,
	mt.DT_IsFlatFile AS IsFlatFileSchema,
	oe.OI_OverrideFilename AS OverrideFilename,
	oe.OI_OverrideEmailSubject AS OverrideEmailSubject
FROM
	dbo.eHubOutboxEnvelope oe WITH (NOLOCK)
INNER JOIN
	dbo.eHubMessage m WITH (NOLOCK) ON m.DC_PK = oe.OI_DC_Message
INNER JOIN
	dbo.eHubMessageType mt WITH (NOLOCK) ON mt.DT_PK = m.DC_DT_Target
INNER JOIN
	dbo.eHubClient r WITH (NOLOCK) ON r.CC_PK = oe.OI_CC_Recipient
INNER JOIN
	dbo.eHubClient s WITH (NOLOCK) ON s.CC_PK = oe.OI_CC_Sender
WHERE
	oe.OI_PK = @MessageEnvelopeID AND
	oe.OI_Status = 0

--Delete the message envelope record just selected
UPDATE
	dbo.eHubOutboxEnvelope WITH (ROWLOCK)
SET
	OI_DC_Message = NULL,
	OI_Status = 1,
	OI_LastUpdateUTC = GETUTCDATE()
WHERE
	OI_PK = @MessageEnvelopeID

--Delete the message record just selected if not bound to another envelope
DELETE FROM
	dbo.eHubMessage WITH (ROWLOCK)
WHERE
	DC_PK = @MessageID AND
	NOT EXISTS ( SELECT 1 FROM dbo.eHubOutboxEnvelope WITH (NOLOCK) WHERE OI_DC_Message = DC_PK AND OI_Status = 0)
GO

/****** Object:  StoredProcedure [dbo].[SelectOutboxMessagesByRecipientID]    Script Date: 08/24/2010 11:41:19 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[SelectOutboxMessagesByRecipientID] 
@RequestID VARCHAR(36)
,@RecipientID VARCHAR(36)
,@LargeMessageBytes INT
AS

SET FMTONLY OFF

--Create a table with message IDs required
SELECT
	e.OI_PK AS EnvelopeID,
	e.OI_DC_Message AS MessageID,
	DATALENGTH(m.DC_Content) AS MessageBytes,
	e.OI_InsertUTC AS ReceivedDate
INTO
	#Envelopes
FROM
	dbo.eHubOutboxEnvelope e WITH (NOLOCK)
INNER JOIN
	dbo.eHubMessage m WITH (NOLOCK) ON m.DC_PK = e.OI_DC_Message
INNER JOIN
	dbo.eHubClient c ON c.CC_PK = e.OI_CC_Recipient
WHERE
	e.OI_IsBatch = 0 AND
	e.OI_Status = 0 AND
	c.CC_ID = @RecipientID

/*
Remove the envelope IDs to apply the following rules:
1. If the earliest message is over the large message threshold then just return that message
2. If the earliest message is under the large message threshold then return all messages up to the large message threshold
*/
-- Refactor this code to be more efficient. Maybe filter directly on the temp table build query above instead
DECLARE @EnvelopeID UNIQUEIDENTIFIER
DECLARE @MinReceivedDate DATETIME
SELECT @MinReceivedDate = MIN(ReceivedDate) FROM #Envelopes
IF EXISTS (SELECT 1 FROM #Envelopes WHERE ReceivedDate = @MinReceivedDate AND MessageBytes > @LargeMessageBytes)
BEGIN
	-- Found a large message so delete all the other envelopes leaving only 1
	SET @EnvelopeID = (SELECT TOP 1 EnvelopeID FROM #Envelopes WHERE ReceivedDate = @MinReceivedDate AND MessageBytes > @LargeMessageBytes)
	DELETE #Envelopes WHERE EnvelopeID <> @EnvelopeID
END
ELSE
BEGIN
	-- Delete all envelopes over that push the resulting message over the large message threshold, retaining a FIFO approach
	IF (SELECT SUM(MessageBytes) FROM #Envelopes) > @LargeMessageBytes
	BEGIN
		DECLARE @CurrentMessageBytes INT = 0
		DECLARE @MessageBytes INT
		
		DECLARE EnvelopeSum CURSOR FOR
		SELECT EnvelopeID, MessageBytes FROM #Envelopes ORDER BY ReceivedDate ASC
		OPEN EnvelopeSum
		FETCH NEXT FROM EnvelopeSum INTO @EnvelopeID, @MessageBytes
		WHILE @@FETCH_STATUS = 0
		BEGIN
			SET @CurrentMessageBytes = @CurrentMessageBytes + @MessageBytes
			IF @CurrentMessageBytes > @LargeMessageBytes
			BEGIN
				DELETE #Envelopes WHERE EnvelopeID = @EnvelopeID
			END
			FETCH NEXT FROM EnvelopeSum INTO @EnvelopeID, @MessageBytes
		END
		CLOSE EnvelopeSum
		DEALLOCATE EnvelopeSum
	END
END

--Select the data 
SELECT
	@RecipientID AS RecipientID,
	@RequestID AS RequestID,
	oe.OI_InternalTrackingID AS InternalTrackingID,
	sc.CC_ID AS SenderID,
	oe.OI_EnvelopeTrackingID AS EnvelopeTrackingID,
	oe.OI_MessageTrackingID AS MessageTrackingID,
	m.DC_Content AS MessageContent,
	mt.DT_Code AS MessageType,
	mt.DT_IsFlatFile AS IsFlatFileSchema
FROM
	dbo.eHubOutboxEnvelope oe WITH (NOLOCK)
INNER JOIN
	#Envelopes e ON e.EnvelopeID = oe.OI_PK
INNER JOIN
	dbo.eHubMessage m WITH (NOLOCK) ON m.DC_PK = oe.OI_DC_Message
INNER JOIN
	dbo.eHubMessageType mt WITH (NOLOCK) ON mt.DT_PK = m.DC_DT_Target
INNER JOIN
	dbo.eHubClient sc WITH (NOLOCK) ON sc.CC_PK = oe.OI_CC_Sender
WHERE
	oe.OI_IsBatch = 0 AND
	oe.OI_Status = 0
	
UNION ALL

SELECT
	@RecipientID AS RecipientID,
	@RequestID AS RequestID,
	NULL AS InternalTrackingID,
	NULL AS SenderID,
	NULL AS EnvelopeTrackingID,
	NULL AS MessageTrackingID,
	NULL AS MessageContent,
	NULL AS MessageType,
	NULL AS IsFlatFileSchema
WHERE
	NOT EXISTS ( SELECT 1 FROM #Envelopes )

--Update the message envelope records just selected
UPDATE
	[dbo].[eHubOutboxEnvelope] WITH (ROWLOCK)
SET
	OI_DC_Message = NULL,
	OI_Status = 1,
	OI_LastUpdateUTC = GETUTCDATE()
WHERE
	OI_PK IN ( SELECT EnvelopeID FROM #Envelopes )

--Delete the message records just selected that are not bound to another envelope
DELETE FROM
	dbo.eHubMessage WITH (ROWLOCK)
WHERE
	DC_PK IN ( SELECT MessageID FROM #Envelopes ) AND
	NOT EXISTS ( SELECT 1 FROM dbo.eHubOutboxEnvelope WITH (NOLOCK) WHERE OI_DC_Message = DC_PK AND OI_Status = 0 )
GO

/****** Object:  StoredProcedure [dbo].[SelectOutboxScheduledEnvelopeCount]    Script Date: 08/24/2010 11:41:19 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[SelectOutboxScheduledEnvelopeCount]
	@DistributionZone VARCHAR(50)
AS

DECLARE @CurrentUtcDateTime DATETIME
SET @CurrentUtcDateTime = GETUTCDATE()

SELECT
	COUNT(1)
FROM
	dbo.eHubOutboxEnvelope oe WITH (NOLOCK)
INNER JOIN
	dbo.eHubClient c WITH (NOLOCK) ON c.CC_PK = oe.OI_CC_Recipient
INNER JOIN
	dbo.eHubZone z WITH (NOLOCK) ON z.ZZ_PK = c.CC_DistributionZone
WHERE
	oe.OI_IsBatch = 0 AND
	oe.OI_Status = 0 AND
	z.ZZ_ID = @DistributionZone
GO

/****** Object:  StoredProcedure [dbo].[SelectTransformsByPartiesMessage]    Script Date: 08/24/2010 11:41:19 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[SelectTransformsByPartiesMessage]
	@SenderID VARCHAR(36), 
	@RecipientID VARCHAR(36),
	@SourceType VARCHAR(200)
AS

SELECT
	TransformType.TT_TransformationType,
	TargetType.DT_Code
FROM
	dbo.eHubTransformationSet TransformSet WITH (NOLOCK)
INNER JOIN
	dbo.eHubClient Sender WITH (NOLOCK) ON Sender.CC_PK = TransformSet.TS_CC_Sender AND Sender.CC_ID = @SenderID
INNER JOIN
	dbo.eHubClient Recipient WITH (NOLOCK) ON Recipient.CC_PK = TransformSet.TS_CC_Recipient AND Recipient.CC_ID = @RecipientID
INNER JOIN
	dbo.eHubMessageType SourceType WITH (NOLOCK) ON SourceType.DT_PK = TransformSet.TS_DT_Source AND SourceType.DT_Code = @SourceType
INNER JOIN
	dbo.eHubTransformationMapping TransformMapping WITH (NOLOCK) ON TransformMapping.TM_TS_PK = TransformSet.TS_PK
INNER JOIN
	dbo.eHubTransformationType TransformType WITH (NOLOCK) ON TransformType.TT_PK = TransformMapping.TM_TT_PK
INNER JOIN
	dbo.eHubMessageType TargetType WITH (NOLOCK) ON TargetType.DT_PK = TransformType.TT_DT_Target
GO


/****** Object:  StoredProcedure [dbo].[UpdatePassword]    Script Date: 08/27/2010 14:58:26 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UpdatePassword]
	@ClientID VARCHAR(36),
	@Password VARCHAR(200)
AS

IF NOT EXISTS ( SELECT 1 FROM eHubClient WITH (NOLOCK) WHERE CC_ID = @ClientID )
BEGIN
	DECLARE @errorMsg VARCHAR(400)
	SET @errorMsg = 'Could not find client with ID: ' + @ClientID
	RAISERROR(@errorMsg, 16, 1)
	RETURN
END

UPDATE eHubClient SET CC_Password = @Password WHERE CC_ID = @ClientID

SELECT
	CC_EmailAddress, CC_FriendlyName
FROM
	eHubClient WITH (NOLOCK)
WHERE CC_ID = @ClientID
GO

/****** Object:  StoredProcedure [dbo].[ValidatePassword]    Script Date: 08/31/2010 09:22:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[ValidatePassword]
	@ClientID VARCHAR(36),
	@Password VARCHAR(200)
AS
	DECLARE @Exists BIT = 0
	
	SELECT 
		@Exists = 1 
	FROM eHubClient WITH (NOLOCK) 
	WHERE CC_ID = @ClientID AND CC_Password = @Password

	SELECT @Exists
GO

/****** Object:  StoredProcedure [dbo].[SelectEnvelopeStatusByTrackingID]    Script Date: 09/03/2010 14:20:52 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[SelectEnvelopeStatusByTrackingID] @TrackingID VARCHAR(36), @SenderID VARCHAR(36)
AS

DECLARE @errorMsg VARCHAR(400)
IF NOT EXISTS ( SELECT 1 FROM [dbo].[eHubClient] WITH (NOLOCK) WHERE CC_ID = @SenderID )
BEGIN
	SET @errorMsg = 'sender not there ' + @SenderID
	RAISERROR(@errorMsg, 16, 1)
	RETURN
END

DECLARE @Result INT = 2

SELECT TOP 1 @Result = OI_Status FROM eHubOutboxEnvelope WITH (READPAST)
INNER JOIN eHubClient WITH (NOLOCK) ON OI_CC_Sender = CC_PK
WHERE OI_EnvelopeTrackingID = @TrackingID

SELECT @Result
GO

---------------------------------------------------------------------------------------------------------------------------------------------
-- Stored Procedure Rights Assignment
---------------------------------------------------------------------------------------------------------------------------------------------

GRANT EXECUTE ON [dbo].[DeleteOutboxEnvelopeByTrackingID] TO [CORPORATE\ehubservice]
GO

GRANT EXECUTE ON [dbo].[DeleteOutboxEnvelopeByIsDeliveredUTC] TO [CORPORATE\ehubservice]
GO

GRANT EXECUTE ON [dbo].[InsertOutboxMessage] TO [CORPORATE\ehubservice]
GO

GRANT EXECUTE ON [dbo].[InsertOutboxEnvelope] TO [CORPORATE\ehubservice]
GO

GRANT EXECUTE ON [dbo].[SelectClientExists] TO [CORPORATE\ehubservice]
GO

GRANT EXECUTE ON [dbo].[CheckAccess] TO [CORPORATE\ehubservice]
GO

GRANT EXECUTE ON [dbo].[SelectClientRelationshipExists] TO [CORPORATE\ehubservice]
GO

GRANT EXECUTE ON [dbo].[SelectOutboxEnvelopesBySchedule] TO [CORPORATE\ehubservice]
GO

GRANT EXECUTE ON [dbo].[SelectOutboxMessageByEnvelopeID] TO [CORPORATE\ehubservice]
GO

GRANT EXECUTE ON [dbo].[SelectOutboxMessagesByRecipientID] TO [CORPORATE\ehubservice]
GO

GRANT EXECUTE ON [dbo].[SelectOutboxScheduledEnvelopeCount] TO [CORPORATE\ehubservice]
GO

GRANT EXECUTE ON [dbo].[UpdatePassword] TO [CORPORATE\ehubservice]
GO

GRANT EXECUTE ON [dbo].[SelectEnvelopeStatusByTrackingID] TO [CORPORATE\ehubservice]
GO

GRANT EXECUTE ON [dbo].[ValidatePassword] TO [CORPORATE\ehubservice]
GO

GRANT EXECUTE ON [dbo].[SelectTransformsByPartiesMessage] TO [CORPORATE\ehubservice]
GO

GRANT EXECUTE ON [dbo].[SelectOutboxMessageBatchByRecipientID] TO [CORPORATE\ehubservice]
GO

---------------------------------------------------------------------------------------------------------------------------------------------
-- Default Data Population
---------------------------------------------------------------------------------------------------------------------------------------------

INSERT INTO [eHubTransactions].[dbo].[eHubMessageType] ([DT_PK],[DT_Code],[DT_IsFlatFile])
SELECT 'B11FB464-3D96-4D3A-B7C8-045F90E3B30C','http://cargowise.com/ehub/gateway/2010/06#RetrieveResponse',0
GO

INSERT INTO [eHubTransactions].[dbo].[eHubZone] ([ZZ_PK],[ZZ_ID])
SELECT '75419F4C-C522-4890-BD5D-BCA5E12268F6','AU' UNION
SELECT 'ADEF274E-C8FE-4929-8185-B16215BF775B','UK' UNION
SELECT 'CF2E0AA7-CF7D-442A-9BC1-43E38F80897F','US'
GO

