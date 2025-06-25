namespace CargoWise.Billing.Collectors.Logistics
{
	public class TransportBookingIntelligence : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => false;
		public override string FeatureCode => "TBR";
		public override string FeatureName => "Transport Booking Intelligence Reporting";
		public override string TransactionDateUtc => "KN_SystemLastEditTimeUtc";
		public override string CreatingUserCode => "KN_SystemCreateUser";
		public override string TransactionCount => "1";
		public override string RoleName => "Logistics and Supply Chain";
		public override string ModuleName => "Domestic Transport";
		public override string FunctionName => "Transport Booking Intelligence Reporting";
		public override string CompanyCode => "GC_Code";
		public override string BranchCode => "GB_Code";
		public override string BillingReference1 => "KM_JobID";
		public override string BillingReference2 => "KM_RatingFreightMode";
		public override string BillingReference3 => "KB_ParentTableCode";
		public override string BillingReference4 => "KN_InstructionType";
		public override string GuidReference => "CAST(CONVERT(BINARY(16), HASHBYTES('SHA1', CONCAT(CAST(KN_PK AS NVARCHAR(36)), ISNULL(CAST(KK_PK AS NVARCHAR(36)), '')))) AS UNIQUEIDENTIFIER)";
		public override string PreparationScript => @"
		WITH TransportBookingInfo
		AS( SELECT KN_InstructionType = DBI.KN_InstructionType,
			KB_ParentTableCode = DBC.KB_ParentTableCode,
			KM_RatingFreightMode = DB.KM_RatingFreightMode,
			KM_JobID = DB.KM_JobID,
			GB_Code = GB.GB_Code,
			GC_Code = GC.GC_Code,
			KK_PK = DBConf.KK_PK,
			KN_PK = DBI.KN_PK,
			KN_SystemCreateUser = DBI.KN_SystemCreateUser,
			KN_SystemLastEditTimeUtc = DBI.KN_SystemLastEditTimeUtc,
			Estimated = DBConf.KK_Estimated,
			Actual = DBConf.KK_Actual,
			RequiredFrom = DBConf.KK_RequiredFrom,
			RequiredTo = DBConf.KK_RequiredTo,
			TransportCompany = OH.OH_Code,
			PostalCode = ISNULL(NULLIF(JDA2.E2_PostCode, ''), OA2.OA_PostCode),
			City = ISNULL(NULLIF(JDA2.E2_City, ''), OA2.OA_City),
			State = ISNULL(NULLIF(JDA2.E2_State, ''), OA2.OA_State),
			Country = ISNULL(NULLIF(JDA2.E2_RN_NKCountryCode, ''), OA2.OA_RN_NKCountryCode),
			CreatedDate = IIF(DATEDIFF(DAY,DB.KM_SystemCreateTimeUtc,DB.KM_SystemLastEditTimeUtc) = 0, DB.KM_SystemCreateTimeUtc, NULL),
			Direction = DBT.KT_Direction,
			Geoloc = IIF(JDA2.E2_AddressOverride = 1 OR JDA2.E2_OA_Address IS NULL,JDA2.E2_GeoLocation,OA2.OA_GeoLocation).ToString(),
			IsFirstInstruction = IIF(DBI.KN_Sequence = 1, 'Y', 'N'),
			IsLastInstruction = IIF(DBI.KN_Sequence = MAX(DBI.KN_Sequence) OVER (PARTITION BY DB.KM_PK), 'Y', 'N'),
			OrganisationType = JDA2.E2_AddressType,
			PacklineCount = CP.PacklineCount,
			ContainerCount = CP.ContainerCount,
			OtherPackageTypeCount  = CP.OtherPackageTypeCount,
			ContainerMode = CP.ContainerMode,
			ContainerType = CP.ContainerType,
			ConfirmationType = DBConf.KK_ConfirmationType
			FROM dbo.[DtbBookingInstruction] AS DBI
			JOIN dbo.[DtbBooking] AS DB ON DB.KM_PK = DBI.KN_KM_BookingMovement -- Every BookingInstruction has a Booking
			LEFT JOIN dbo.[DTBBookingConsolidation] DBC ON DBC.KB_PK = DB.KM_KB_Booking -- Booking may or may not have a BookingConsolidation
			LEFT JOIN dbo.[DtbBookingConfirmation] DBConf ON DBConf.KK_KN_BookingInstruction = DBI.KN_PK -- BookingInstruction may or may not have a BookingConfirmation
			LEFT JOIN dbo.[DtbBookingTmpl] DBT ON DBT.KT_Code = DB.KM_KT_NKBookingTemplate -- Booking may or may not have a Booking Template 
			LEFT JOIN dbo.[JobDocAddress] JDA ON JDA.E2_ParentID = DB.KM_PK -- Match JobDocAddress E2_ParentID with Booking KM_PK
			AND JDA.E2_ParentTableCode = 'KM' -- Ensure ParentTableCode is 'KM'
			AND JDA.E2_AddressType = 'TRA' -- Ensure AddressType is 'TRA' (Transport Company)
			LEFT JOIN dbo.[OrgAddress] OA ON OA.OA_PK = JDA.E2_OA_Address -- Match E2_OA_Address from JobDocAddress to OrgAddress OA_PK
			LEFT JOIN dbo.[OrgHeader] OH ON OH.OH_PK = OA.OA_OH -- Match OrgAddress OA_OH to OrgHeader OH_PK
			LEFT JOIN dbo.[JobDocAddress] JDA2 ON JDA2.E2_ParentID = DBI.KN_PK -- Match JobDocAddress E2_ParentID with BookingInstruction KK_PK
			AND JDA2.E2_ParentTableCode = 'KN' -- Ensure ParentTableCode is 'KN'
			LEFT JOIN dbo.[OrgAddress] OA2 ON JDA2.E2_OA_Address = OA2.OA_PK
			LEFT JOIN dbo.[GlbBranch] GB ON DB.KM_GB_Branch = GB.GB_PK
			LEFT JOIN dbo.[GlbCompany] GC ON GB.GB_GC = GC.GC_PK
			LEFT JOIN (SELECT
							COUNT(DBP.KD_PK) AS 'PacklineCount',
							SUM(CASE WHEN PKG.KP_F3_NKPackType = 'CNT' THEN DBP.KD_Quantity ELSE 0 END) AS 'ContainerCount',
							SUM(CASE WHEN PKG.KP_F3_NKPackType <> 'CNT' THEN DBP.KD_Quantity ELSE 0 END) AS 'OtherPackageTypeCount',
							DBP.KD_KN_BookingInstruction,
							STRING_AGG(CASE WHEN PKG.KP_F3_NKPackType = 'CNT' THEN TRIM(NULLIF(PKC.K0_ContainerMode,'')) END, ',') WITHIN GROUP (ORDER by PKC.K0_ContainerMode,RC.RC_Code) AS 'ContainerMode' ,
							STRING_AGG(CASE WHEN PKG.KP_F3_NKPackType = 'CNT' THEN TRIM(NULLIF(RC.RC_Code,'')) END, ',') WITHIN GROUP (ORDER by PKC.K0_ContainerMode,RC.RC_Code) AS 'ContainerType'
						FROM [dbo].[DtbBookingInstructionPkgDivot] DBP
						LEFT JOIN [dbo].[PkgPackage] PKG ON PKG.KP_PK = DBP.KD_KP_Package
						LEFT JOIN [dbo].[PkgPackageContainer] PKC ON PKC.K0_KP_Package = PKG.KP_PK
						LEFT JOIN [dbo].[RefContainer] RC ON RC.RC_PK = PKC.K0_RC_ContainerType
						GROUP BY DBP.KD_KN_BookingInstruction) CP
			ON CP.KD_KN_BookingInstruction = DBI.KN_PK
		)
		";
		public override string FromClause => @"TransportBookingInfo";
		public override string AdditionalRefs => @"CONVERT(VARBINARY(MAX), CONVERT(VARCHAR(MAX),(SELECT Estimated,Actual,RequiredFrom,RequiredTo,TransportCompany,PostalCode,City,State,Country,CreatedDate,Direction,Geoloc,IsFirstInstruction,IsLastInstruction,OrganisationType,PacklineCount,ContainerCount,OtherPackageTypeCount,ContainerMode,ContainerType,ConfirmationType
		FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)))";

		public override string WhereClause => string.Empty;
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Daily;
		public override string DateType => RefStlDateType.DateTime;
		public override string ActiveOn => "ALL";
	}
}
