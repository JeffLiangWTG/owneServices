CREATE TABLE [RefMessagingBussCarrierInfoAttribute]
(
	[ZCA_PK] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_RefMessagingBussCarrierInfoAttribute_ZCA_PK] DEFAULT (NEWID()),	
	[ZCA_ZMC_CarrierInfo] UNIQUEIDENTIFIER NOT NULL,
	[ZCA_ZAI_AttributeInfo] UNIQUEIDENTIFIER NOT NULL,
	[ZCA_AttributeValue] VARCHAR(200) NOT NULL CONSTRAINT [DF_RefMessagingBussCarrierInfoAttribute_ZCA_AttributeValue] DEFAULT (''),

	CONSTRAINT [PK_RefMessagingBussCarrierInfoAttribute] PRIMARY KEY NONCLUSTERED([ZCA_PK] ASC),
	CONSTRAINT [FK_RefMessagingBussCarrierInfoAttribute_RefMessagingBussAttributeInfo] FOREIGN KEY([ZCA_ZAI_AttributeInfo]) REFERENCES [RefMessagingBussAttributeInfo] ([ZAI_PK]),
	CONSTRAINT [FK_RefMessagingBussCarrierInfoAttribute_RefMessagingBussCarrierInfo] FOREIGN KEY([ZCA_ZMC_CarrierInfo]) REFERENCES [RefMessagingBussCarrierInfo] ([ZMC_PK]),
	CONSTRAINT [CK_RefMessagingBussCarrierInfoAttribute_ZCA_AttributeValue] CHECK ([ZCA_AttributeValue]<>'')
)
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_RefMessagingBussCarrierInfoAttribute_ZCA_ZMC_CarrierInfo_ZCA_ZAI_AttributeInfo_ZCA_AttributeValue] ON [RefMessagingBussCarrierInfoAttribute]
(
	[ZCA_ZMC_CarrierInfo],
	[ZCA_ZAI_AttributeInfo],
	[ZCA_AttributeValue]
)
GO
