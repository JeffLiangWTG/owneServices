CREATE TABLE RefCusCodeOrAttributeTransportMode(
	[ZZU_PK] [uniqueidentifier] NOT NULL,
	[ZZU_TransportMode] [varchar](3) NOT NULL,
	[ZZU_ZZD_CodeList] [uniqueidentifier] NULL,
	[ZZU_ZZE_Attribute] [uniqueidentifier] NULL,
	CONSTRAINT PK_RefCusCodeOrAttributeTransportMode PRIMARY KEY CLUSTERED( ZZU_PK ASC ),
	CONSTRAINT FK_RefCusCodeOrAttributeTransportMode_RefCusCodeList FOREIGN KEY(ZZU_ZZD_CodeList) REFERENCES RefCusCodeList (ZZD_PK),
	CONSTRAINT FK_RefCusCodeOrAttributeTransportMode_RefCusCodeListAttribute FOREIGN KEY(ZZU_ZZE_Attribute) REFERENCES RefCusCodeListAttribute (ZZE_PK),
	CONSTRAINT CK_RefCusCodeOrAttributeTransportMode_ZZU_TransportMode CHECK (([ZZU_TransportMode]='ROA' OR [ZZU_TransportMode]='SEA' OR [ZZU_TransportMode]='AIR' OR [ZZU_TransportMode]='RAI' OR [ZZU_TransportMode]='MAI' OR [ZZU_TransportMode]='FIX' OR [ZZU_TransportMode]='INW')),
	CONSTRAINT CK_RefCusCodeOrAttributeTransportMode_CodeListOrAttributeOnly CHECK (ZZU_ZZD_CodeList IS NOT NULL OR ZZU_ZZE_Attribute IS NOT NULL),
	CONSTRAINT CK_RefCusCodeOrAttributeTransportMode_CodeListOrAttributeAtLeast CHECK (ZZU_ZZD_CodeList IS NULL OR ZZU_ZZE_Attribute IS NULL),
)
GO
CREATE NONCLUSTERED INDEX [IX_RefCusCodeOrAttributeTransportMode_ZZU_ZZD_CodeList] ON [RefCusCodeOrAttributeTransportMode] ([ZZU_ZZD_CodeList])
GO
CREATE NONCLUSTERED INDEX [IX_RefCusCodeOrAttributeTransportMode_ZZU_ZZE_Attribute] ON [RefCusCodeOrAttributeTransportMode] ([ZZU_ZZE_Attribute])
