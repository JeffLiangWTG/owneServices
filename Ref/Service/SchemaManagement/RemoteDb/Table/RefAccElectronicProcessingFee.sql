CREATE TABLE RefAccElectronicProcessingFee
(
	[EPF_PK] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_RefAccElectronicProcessingFee_EPF_PK] DEFAULT NEWID(),
	[EPF_SystemCode] VARCHAR(3) NOT NULL,
	[EPF_Category] VARCHAR(3) NOT NULL,
	[EPF_Code] VARCHAR(3) NOT NULL,
	[EPF_Description] NVARCHAR(250) NOT NULL,
	[EPF_CountryCode] CHAR(2) NOT NULL CONSTRAINT [DF_RefAccElectronicProcessingFee_EPF_CountryCode] DEFAULT '',
	[EPF_JobDirection] VARCHAR(3) NOT NULL CONSTRAINT [DF_RefAccElectronicProcessingFee_EPF_JobDirection] DEFAULT '',
	[EPF_Currency] VARCHAR(3) NOT NULL,
	[EPF_Price] DECIMAL(18, 6) NOT NULL CONSTRAINT [DF_RefAccElectronicProcessingFee_EPF_Price] DEFAULT(0),
	[EPF_ValidFrom] SMALLDATETIME NOT NULL CONSTRAINT [DF_RefAccElectronicProcessingFee_EPF_ValidFrom] DEFAULT ('1900-01-01'),

	CONSTRAINT [CK_RefAccElectronicProcessingFee_EPF_SystemCode] CHECK ([EPF_SystemCode] <> ''),
	CONSTRAINT [CK_RefAccElectronicProcessingFee_EPF_Category] CHECK ([EPF_Category] <> ''),
	CONSTRAINT [CK_RefAccElectronicProcessingFee_EPF_Code] CHECK ([EPF_Code] <> ''),
	CONSTRAINT [CK_RefAccElectronicProcessingFee_EPF_Description] CHECK ([EPF_Description] <> ''),
	CONSTRAINT [CK_RefAccElectronicProcessingFee_EPF_Currency] CHECK ([EPF_Currency] <> ''),
)
GO
CREATE UNIQUE CLUSTERED INDEX IX_RefAccElectronicProcessingFee_EPF_SystemCode_Category_Code_CountryCode_JobDirection_Currency_ValidFrom ON RefAccElectronicProcessingFee(EPF_SystemCode, EPF_Category, EPF_Code, EPF_CountryCode, EPF_JobDirection, EPF_Currency, EPF_ValidFrom)
GO
