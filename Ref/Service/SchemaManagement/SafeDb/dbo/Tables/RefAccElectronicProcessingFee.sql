CREATE TABLE RefAccElectronicProcessingFee
(
	[EPF_PK] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_RefAccElectronicProcessingFee_EPF_PK] DEFAULT NEWID(),
	[EPF_SystemCode] VARCHAR(3) NOT NULL,
	[EPF_Category] VARCHAR(3) NOT NULL,
	[EPF_Code] VARCHAR(3) NOT NULL,
	[EPF_Description] NVARCHAR(250) NOT NULL,
	[EPF_Currency] VARCHAR(3) NOT NULL,
	[EPF_Price] DECIMAL(18, 6) NOT NULL CONSTRAINT [DF_RefAccElectronicProcessingFee_EPF_Price] DEFAULT(0),
	[EPF_ValidFrom] SMALLDATETIME NOT NULL CONSTRAINT [DF_RefAccElectronicProcessingFee_EPF_ValidFrom] DEFAULT ('1900-01-01'),
	[EPF_CountryCode] CHAR(2) NOT NULL CONSTRAINT [DF_RefAccElectronicProcessingFee_EPF_CountryCode] DEFAULT '',
	[EPF_JobDirection] VARCHAR(3) NOT NULL CONSTRAINT [DF_RefAccElectronicProcessingFee_EPF_JobDirection] DEFAULT '',
	[EPF_SysStartTime] DATETIME2 GENERATED ALWAYS AS ROW START NOT NULL CONSTRAINT [DF_RefAccElectronicProcessingFee_EPF_SysStartTime] DEFAULT SYSUTCDATETIME(),
	[EPF_SysEndTime] DATETIME2 GENERATED ALWAYS AS ROW END NOT NULL CONSTRAINT [DF_RefAccElectronicProcessingFee_EPF_SysEndTime] DEFAULT CONVERT(DATETIME2, '9999-12-31 23:59:59.9999999'),
	PERIOD FOR SYSTEM_TIME ([EPF_SysStartTime], [EPF_SysEndTime]),
	CONSTRAINT [PK_RefAccElectronicProcessingFee] PRIMARY KEY CLUSTERED ([EPF_PK] ASC),
	CONSTRAINT [CK_RefAccElectronicProcessingFee_EPF_SystemCode] CHECK ([EPF_SystemCode] <> ''),
	CONSTRAINT [CK_RefAccElectronicProcessingFee_EPF_Category] CHECK ([EPF_Category] <> ''),
	CONSTRAINT [CK_RefAccElectronicProcessingFee_EPF_Code] CHECK ([EPF_Code] <> ''),
	CONSTRAINT [CK_RefAccElectronicProcessingFee_EPF_Description] CHECK ([EPF_Description] <> ''),
	CONSTRAINT [CK_RefAccElectronicProcessingFee_EPF_Currency] CHECK ([EPF_Currency] <> ''))
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.RefAccElectronicProcessingFeeHistory))
GO
CREATE UNIQUE NONCLUSTERED INDEX IX_RefAccElectronicProcessingFee_EPF_SystemCode_Category_Code_CountryCode_JobDirection_Currency_ValidFrom ON RefAccElectronicProcessingFee(EPF_SystemCode, EPF_Category, EPF_Code, EPF_CountryCode, EPF_JobDirection, EPF_Currency, EPF_ValidFrom)
GO
ALTER TABLE RefAccElectronicProcessingFee SET (LOCK_ESCALATION = DISABLE);
GO
