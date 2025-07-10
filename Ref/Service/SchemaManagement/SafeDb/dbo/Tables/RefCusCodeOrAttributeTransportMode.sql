CREATE TABLE RefCusCodeOrAttributeTransportMode(
	[ZZU_PK] UNIQUEIDENTIFIER NOT NULL,
	[ZZU_TransportMode] VARCHAR(3) NOT NULL,
	[ZZU_ZZD_CodeList] UNIQUEIDENTIFIER NULL,
	[ZZU_ZZE_Attribute] UNIQUEIDENTIFIER NULL,
	[ZZU_DataSetPK] UNIQUEIDENTIFIER,
	[ZZU_DataSetCode] VARCHAR(3),
	[ZZU_SysStartTime] DATETIME2 GENERATED ALWAYS AS ROW START NOT NULL CONSTRAINT [DF_ZZU_SysStartTime] DEFAULT CONVERT(DATETIME2, '1900-01-01 0:0:0.0000000'),
	[ZZU_SysEndTime] DATETIME2 GENERATED ALWAYS AS ROW END NOT NULL CONSTRAINT [DF_ZZU_SysEndTime] DEFAULT CONVERT(DATETIME2, '9999-12-31 23:59:59.9999999'),
	PERIOD FOR SYSTEM_TIME ([ZZU_SysStartTime], [ZZU_SysEndTime]),
	CONSTRAINT [PK_RefCusCodeOrAttributeTransportMode] PRIMARY KEY CLUSTERED( [ZZU_PK] ASC ),
	CONSTRAINT [FK_RefCusCodeOrAttributeTransportMode_RefCusCodeList] FOREIGN KEY([ZZU_ZZD_CodeList]) REFERENCES RefCusCodeList ([ZZD_PK]),
	CONSTRAINT [FK_RefCusCodeOrAttributeTransportMode_RefCusCodeListAttribute] FOREIGN KEY([ZZU_ZZE_Attribute]) REFERENCES RefCusCodeListAttribute ([ZZE_PK]),
	CONSTRAINT [CK_RefCusCodeOrAttributeTransportMode_ZZU_TransportMode] CHECK (([ZZU_TransportMode]='ROA' OR [ZZU_TransportMode]='SEA' OR [ZZU_TransportMode]='AIR' OR [ZZU_TransportMode]='RAI' OR [ZZU_TransportMode]='MAI' OR [ZZU_TransportMode]='FIX' OR [ZZU_TransportMode]='INW')),
	CONSTRAINT [CK_RefCusCodeOrAttributeTransportMode_CodeListOrAttributeOnly] CHECK ([ZZU_ZZD_CodeList] IS NOT NULL OR [ZZU_ZZE_Attribute] IS NOT NULL),
	CONSTRAINT [CK_RefCusCodeOrAttributeTransportMode_CodeListOrAttributeAtLeast] CHECK ([ZZU_ZZD_CodeList] IS NULL OR [ZZU_ZZE_Attribute] IS NULL),
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.RefCusCodeOrAttributeTransportModeHistory))
GO
CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusCodeOrAttributeTransportMode_ZZU_TransportMode_ZZU_ZZD_CodeList_ZZU_ZZE_Attribute ON RefCusCodeOrAttributeTransportMode(ZZU_TransportMode, ZZU_ZZD_CodeList, ZZU_ZZE_Attribute)
GO
CREATE NONCLUSTERED INDEX IX_RefCusCodeOrAttributeTransportMode_ZZU_ZZD_CodeList ON RefCusCodeOrAttributeTransportMode(ZZU_ZZD_CodeList) WHERE ZZU_ZZD_CodeList IS NOT NULL
GO
CREATE NONCLUSTERED INDEX IX_RefCusCodeOrAttributeTransportMode_ZZU_ZZE_Attribute ON RefCusCodeOrAttributeTransportMode(ZZU_ZZE_Attribute) WHERE ZZU_ZZE_Attribute IS NOT NULL
GO
ALTER TABLE RefCusCodeOrAttributeTransportMode SET (LOCK_ESCALATION = DISABLE);
