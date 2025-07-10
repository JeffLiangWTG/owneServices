CREATE TABLE [RefAirline] (
	[RM_PK] UNIQUEIDENTIFIER NOT NULL,
	[RM_IsActive] BIT NOT NULL CONSTRAINT [DF_RefAirline_RM_IsActive] DEFAULT 1,
	[RM_AirlineName1] VARCHAR(40) NOT NULL CONSTRAINT [DF_RefAirline_RM_AirlineName1] DEFAULT '',
	[RM_AirlineName2] VARCHAR(40) NOT NULL CONSTRAINT [DF_RefAirline_RM_AirlineName2] DEFAULT '',
	[RM_AccountingCode] VARCHAR(4) NOT NULL CONSTRAINT [DF_RefAirline_RM_AccountingCode] DEFAULT '',
	[RM_ThreeLetterCode] VARCHAR(3) NOT NULL CONSTRAINT [DF_RefAirline_RM_ThreeLetterCode] DEFAULT '',
	[RM_TwoCharacterCode] VARCHAR(2) NOT NULL CONSTRAINT [DF_RefAirline_RM_TwoCharacterCode] DEFAULT '',
	[RM_DuplicateFlagIndicator] BIT NOT NULL CONSTRAINT [DF_RefAirline_RM_DuplicateFlagIndicator] DEFAULT 0,
	[RM_AddressLine1] VARCHAR(40) NOT NULL CONSTRAINT [DF_RefAirline_RM_AddressLine1] DEFAULT '',
	[RM_AddressLine2] VARCHAR(40) NOT NULL CONSTRAINT [DF_RefAirline_RM_AddressLine2] DEFAULT '',
	[RM_AirlineCity] VARCHAR(25) NOT NULL CONSTRAINT [DF_RefAirline_RM_AirlineCity] DEFAULT '',
	[RM_AirlineState] VARCHAR(20) NOT NULL CONSTRAINT [DF_RefAirline_RM_AirlineState] DEFAULT '',
	[RM_AirlineCountry] VARCHAR(44) NOT NULL CONSTRAINT [DF_RefAirline_RM_AirlineCountry] DEFAULT '',
	[RM_AirlinePostalCode] VARCHAR(10) NOT NULL CONSTRAINT [DF_RefAirline_RM_AirlinePostalCode] DEFAULT '',
	[RM_RN_NKAirlineCountry] VARCHAR(2) NOT NULL CONSTRAINT [DF_RefAirline_RM_RN_NKAirlineCountry] DEFAULT '',
	[RM_ReservationsDeptTeletype] VARCHAR(8) NOT NULL CONSTRAINT [DF_RefAirline_RM_ReservationsDeptTeletype] DEFAULT '',
	[RM_ReservationsContactName] VARCHAR(20) NOT NULL CONSTRAINT [DF_RefAirline_RM_ReservationsContactName] DEFAULT '',
	[RM_ReservationsContactTitle] VARCHAR(20) NOT NULL CONSTRAINT [DF_RefAirline_RM_ReservationsContactTitle] DEFAULT '',
	[RM_ReservationsContactTeletype] VARCHAR(8) NOT NULL CONSTRAINT [DF_RefAirline_RM_ReservationsContactTeletype] DEFAULT '',
	[RM_EmergencyTeletype] VARCHAR(8) NOT NULL CONSTRAINT [DF_RefAirline_RM_EmergencyTeletype] DEFAULT '',
	[RM_EmergencyContactName] VARCHAR(20) NOT NULL CONSTRAINT [DF_RefAirline_RM_EmergencyContactName] DEFAULT '',
	[RM_EmergencyContactTitle] VARCHAR(20) NOT NULL CONSTRAINT [DF_RefAirline_RM_EmergencyContactTitle] DEFAULT '',
	[RM_MembershipFlagSITA] BIT NOT NULL CONSTRAINT [DF_RefAirline_RM_MembershipFlagSITA] DEFAULT 0,
	[RM_MembershipFlagARINC] BIT NOT NULL CONSTRAINT [DF_RefAirline_RM_MembershipFlagARINC] DEFAULT 0,
	[RM_MembershipFlagIATA] BIT NOT NULL CONSTRAINT [DF_RefAirline_RM_MembershipFlagIATA] DEFAULT 0,
	[RM_MembershipFlagATA] BIT NOT NULL CONSTRAINT [DF_RefAirline_RM_MembershipFlagATA] DEFAULT 0,
	[RM_TypeOfOperationsCode] VARCHAR(1) NOT NULL CONSTRAINT [DF_RefAirline_RM_TypeOfOperationsCode] DEFAULT '',
	[RM_AccountingSecondaryFlag] VARCHAR(1) NOT NULL CONSTRAINT [DF_RefAirline_RM_AccountingSecondaryFlag] DEFAULT '',
	[RM_AirlinePrefix] VARCHAR(3) NOT NULL CONSTRAINT [DF_RefAirline_RM_AirlinePrefix] DEFAULT '',
	[RM_AirlinePrefixSecondaryFlag] VARCHAR(1) NOT NULL CONSTRAINT [DF_RefAirline_RM_AirlinePrefixSecondaryFlag] DEFAULT '',
	[RM_EagleAddedAirlinePrefixOrAccountingCode] VARCHAR(3) NOT NULL CONSTRAINT [DF_RefAirline_RM_EagleAddedAirlinePrefixOrAccountingCode] DEFAULT '',
	[RM_LabelShortName] VARCHAR(35) NOT NULL CONSTRAINT [DF_RefAirline_RM_LabelShortName] DEFAULT '',
	[RM_IsCASSControlled] BIT NOT NULL CONSTRAINT [DF_RefAirline_RM_IsCASSControlled] DEFAULT 0,
	[RM_ContactNameOCIIdentifier] VARCHAR(2) NOT NULL CONSTRAINT [DF_RefAirline_RM_ContactNameOCIIdentifier] DEFAULT '',
	[RM_ContactPhoneOCIIdentifier] VARCHAR(2) NOT NULL CONSTRAINT [DF_RefAirline_RM_ContactPhoneOCIIdentifier] DEFAULT '',
	[RM_AirlineLogo] VARBINARY(MAX) NULL,
	[RM_SysStartTime] DATETIME2 GENERATED ALWAYS AS ROW START NOT NULL CONSTRAINT [DF_RM_SysStartTime] DEFAULT sysutcdatetime(),
	[RM_SysEndTime] DATETIME2 GENERATED ALWAYS AS ROW END NOT NULL CONSTRAINT [DF_RM_SysEndTime] DEFAULT CONVERT(DATETIME2, '9999-12-31 23:59:59.9999999'),
	PERIOD FOR SYSTEM_TIME ([RM_SysStartTime], [RM_SysEndTime]),
	CONSTRAINT [PK_RefAirline] PRIMARY KEY CLUSTERED ([RM_PK] ASC),
	CONSTRAINT [CK_RefAirline_RM_RN_NKAirlineCountry] CHECK (LEN(RM_RN_NKAirlineCountry) = 0 OR LEN(RM_RN_NKAirlineCountry) = 2)
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.RefAirlineHistory))
GO
CREATE UNIQUE NONCLUSTERED INDEX [NR_UX__RM_EagleAddedAirlinePrefixOrAccountingCode] ON [RefAirline] ([RM_EagleAddedAirlinePrefixOrAccountingCode] ASC)
WHERE RM_EagleAddedAirlinePrefixOrAccountingCode <> ''
GO
CREATE UNIQUE NONCLUSTERED INDEX [NR_UX__RM_ThreeLetterCode] ON [RefAirline] ([RM_ThreeLetterCode] ASC)
WHERE RM_ThreeLetterCode <> ''
GO
CREATE UNIQUE NONCLUSTERED INDEX [NR_UX__RM_AirlineName1] ON [RefAirline] ([RM_AirlineName1] ASC)
WHERE RM_EagleAddedAirlinePrefixOrAccountingCode = '' AND RM_ThreeLetterCode = ''
GO
ALTER TABLE RefAirline SET (LOCK_ESCALATION = DISABLE);
