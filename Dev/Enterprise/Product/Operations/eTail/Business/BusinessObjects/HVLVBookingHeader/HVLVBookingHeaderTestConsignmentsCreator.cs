using System.Data;
using CargoWise.Data;

namespace Enterprise.eTail.Business
{
	public static class HVLVBookingHeaderTestConsignmentsCreator
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public static void CreateConsignments(string bookingHeaderReference, int consignmentCount, string waybillPrefix)
		{
			var sqlText = $@"
DECLARE
	@booking_header_pk UNIQUEIDENTIFIER,
	@hch_cluster_key INT,
	@current_waybill_count INT;

SELECT @booking_header_pk = CAST(HVH_PK AS UNIQUEIDENTIFIER), @hch_cluster_key = CAST(HVH_ClusterKey AS INT) FROM dbo.HVLVBookingHeader WHERE HVH_BookingReference = @booking_header_reference;

BEGIN TRY
	BEGIN TRANSACTION tran_insert
		DECLARE
			@i int = 1,
			@count int = 1,
			@hvc_pk UNIQUEIDENTIFIER = NEWID(),
			@hvi_pk UNIQUEIDENTIFIER = NEWID();

		SELECT * INTO #TEMPHVLVConsignment FROM dbo.HVLVConsignment WHERE 1 = 0;
		SELECT * INTO #TEMPHVLVItem FROM dbo.HVLVItem WHERE 1 = 0;
		SELECT * INTO #TEMPHVLVItemLine FROM dbo.HVLVItemLine WHERE 1 = 0;

		WHILE @count <= @consignment_count
		BEGIN			
			INSERT INTO #TEMPHVLVConsignment (
				HVC_PK, HVC_HVH_BookingHeader,
				HVC_ConsignmentId, HVC_ClusterKey, HVC_WaybillNumber,
				HVC_GoodsDescription, HVC_GoodsValue, HVC_TransportValue, HVC_InsuranceValue, HVC_RX_NKGoodsValueCurrency,
				HVC_ConsigneeInstructions, HVC_ConsigneeName, HVC_ConsigneeAddress1, HVC_ConsigneeAddress2, HVC_ConsigneeCity, HVC_ConsigneeState, HVC_ConsigneePostcode, HVC_RN_NKConsigneeCountryCode, HVC_ConsigneeContact, HVC_ConsigneeEmail, HVC_ConsigneePhone, HVC_ConsigneeMobile, HVC_ConsigneeFax,
				HVC_ShipperName, HVC_ShipperAddress1, HVC_ShipperAddress2, HVC_ShipperCity, HVC_ShipperState, HVC_ShipperPostcode, HVC_RN_NKReturnCountryCode, HVC_RN_NKShipperCountryCode, HVC_ShipperContact, HVC_ShipperEmail, HVC_ShipperPhone, HVC_ShipperMobile, HVC_ShipperFax, HVC_ShipperReference, HVC_OA_ShipperAddress,
				HVC_Status,
				HVC_SystemCreateTimeUtc, HVC_SystemCreateUser, HVC_SystemCreateBranch, HVC_SystemCreateDepartment, HVC_SystemLastEditTimeUtc, HVC_SystemLastEditUser,
				HVC_ItemCount, HVC_ActualVolume, HVC_ManifestedVolume, HVC_VolumeUQ, HVC_ActualWeight, HVC_ManifestedWeight, HVC_WeightUQ,
				HVC_OA_DestinationDepot,
				HVC_OH_LastMileCarrier, HVC_OH_LastMileCarrierBookingAgent, HVC_PL_NKLastMileCarrierServiceLevel, HVC_CarrierAccountNumber,
				HVC_AuthorityToLeave,
				HVC_IsHazardous,
				HVC_IsPerishable,
				HVC_IsPersonalEffects,
				HVC_IsSelfBooked,
				HVC_IsSignatureRequired,
				HVC_IsTaxPrePaid,
				HVC_IsTimber,
				HVC_IsValidatedForUniqueness,
				HVC_RequiresFumigation,
				HVC_JE_ImportDeclaration,
				HVC_JE_ExportDeclaration,
				HVC_ImportCustomsClearanceStatus,
				HVC_ExportCustomsClearanceStatus,
				HVC_ReleaseStatus,
				HVC_ImportReleaseStatus,
				HVC_ExportReleaseStatus,
				HVC_INCO,
				HVC_PreScreeningStatus,
				HVC_DeniedPartyScreeningStatus,
				HVC_UndgClass,
				HVC_VendorIdentifier,
				HVC_ACASMessageStatus , HVC_ACASStatus,HVC_ACASInterchangeStatus,
				HVC_ConsigneeAddressValidationStatus, HVC_IsActive, HVC_IsTracked, HVC_GoodsDescriptionVersion,
				HVC_ReturnAddress1, HVC_ReturnAddress2, HVC_ReturnCity, HVC_ReturnContact, HVC_ReturnEmail, HVC_ReturnFax, HVC_ReturnMobile, HVC_ReturnName, HVC_ReturnPhone, HVC_ReturnPostcode, HVC_ReturnState,
				HVC_RS_NKServiceLevel,
				HVC_ReferenceIsUnique
				)

				VALUES (
				@hvc_pk,						-- HVC_PK
				@booking_header_pk,			    -- HVC_HVH_BookingHeader

				CONCAT('Consignment', @i),		-- HVC_ConsignmentId
				@hch_cluster_key,				-- HVC_ClusterKey
				CONCAT(@waybill_prefix, @i),	-- HVC_WaybillNumber

				'Some Goods',					-- HVC_GoodsDescription
				10,								-- HVC_GoodsValue
				10,								-- HVC_TransportValue
				10,								-- HVC_InsuranceValue
				'AUD',							-- HVC_RX_NKGoodsValueCurrency

				'Some random instructions',		--HVC_ConsigneeInstructions
				'Shawn',						--HVC_ConsigneeName
				'No. 123 ABC Street',			--HVC_ConsigneeAddress1
				'',								--HVC_ConsigneeAddress2
				'Sydney',						--HVC_ConsigneeCity
				'NSW',							--HVC_ConsigneeState
				'2000',							--HVC_ConsigneePostcode
				'AU',							--HVC_RN_NKConsigneeCountryCode
				'Shawn',						--HVC_ConsigneeContact
				'snw@xyz.com',					--HVC_ConsigneeEmail
				'0212345678',					--HVC_ConsigneePhone,
				'0412345678',					--HVC_ConsigneeMobile,
				'0212345678',					--HVC_ConsigneeFax,

				'Kevin',						--HVC_ShipperName
				'No. 456 DEF Street',			--HVC_ShipperAddress1
				'',								--HVC_ShipperAddress2
				'Auckland',						--HVC_ShipperCity
				'AUK',							--HVC_ShipperState
				'1000',							--HVC_ShipperPostcode
				'NZ',							--HVC_RN_NKReturnCountryCode
				'NZ',							--HVC_RN_NKShipperCountryCode
				'Kevin',						--HVC_ShipperContact
				'kc2@uvw.com',					--HVC_ShipperEmail
				'31234567',						--HVC_ShipperPhone
				'201234567',					--HVC_ShipperMobile
				'31234567',						--HVC_ShipperFax
				'',		                        -- HVC_ShipperReference
				null,	                        -- HVC_OA_ShipperAddress
				'BKD',							-- HVC_Status

				SYSUTCDATETIME(),				-- HVC_SystemCreateTimeUtc
				'E',							-- HVC_SystemCreateUser
				'BNE',							-- HVC_SystemCreateBranch
				'BRN',							-- HVC_SystemCreateDepartment
				SYSUTCDATETIME(),				-- HVC_SystemLastEditTimeUtc
				'E',							-- HVC_SystemLastEditUser

				0,		-- HVC_ItemCount
				0,		-- HVC_ActualVolume
				0,		-- HVC_ManifestedVolume
				'M3',	-- HVC_VolumeUQ
				0,		-- HVC_ActualWeight
				0,		-- HVC_ManifestedWeight
				'KG',	-- HVC_WeightUQ
				null,	-- HVC_OA_DestinationDepot
				null,	-- HVC_OH_LastMileCarrier
				null,	-- HVC_OH_LastMileCarrierBookingAgent
				'',		-- HVC_PL_NKLastMileCarrierServiceLevel
				'',		-- HVC_CarrierAccountNumber

				0,		-- HVC_AuthorityToLeave
				0,		-- HVC_IsHazardous
				0,		-- HVC_IsPerishable
				0,		-- HVC_IsPersonalEffects
				0,		-- HVC_IsSelfBooked
				0,		-- HVC_IsSignatureRequired
				0,		-- HVC_IsTaxPrePaid
				0,		-- HVC_IsTimber
				0,		-- HVC_IsValidatedForUniqueness
				0,		-- HVC_RequiresFumigation

				null,	-- HVC_JE_ImportDeclaration
				null,	-- HVC_JE_ImportDeclaration
				'',		-- HVC_ImportCustomsClearanceStatus
				'',		-- HVC_ExportCustomsClearanceStatus
				'NON',	-- HVC_ReleaseStatus
				'NON',	-- HVC_ImportReleaseStatus
				'NON',	-- HVC_ExportReleaseStatus

				'',		-- HVC_INCO

				'UNK',	-- HVC_PreScreeningStatus
				'NOT',	-- HVC_DeniedPartyScreeningStatus
				'',		-- HVC_UndgClass
				'',		-- HVC_VendorIdentifier
				'',     -- HVC_ACASMessageStatus
				'',     -- HVC_ACASStatus
				'' ,    -- HVC_ACASInterchangeStatus
				'NYV',  -- HVC_ConsigneeAddressValidationStatus
				1,		--HVC_IsActive
				0,		--HVC_IsTracked
				0,		--HVC_GoodsDescriptionVersion
				'',		-- HVC_ReturnAddress1
				'',		-- HVC_ReturnAddress2
				'',		-- HVC_ReturnCity
				'',		-- HVC_ReturnContact
				'',		-- HVC_ReturnEmail
				'',		-- HVC_ReturnFax
				'',		-- HVC_ReturnMobile
				'',		-- HVC_ReturnName
				'',		-- HVC_ReturnPhone
				'',		-- HVC_ReturnPostcode
				'',		-- HVC_ReturnState
				'STD', 	-- HVC_RS_NKServiceLevel
				1		-- HVC_ReferenceIsUnique
				);

			INSERT INTO #TEMPHVLVItem (
				HVI_PK,
				HVI_F3_NKPackType,
				HVI_ManifestedWeight,
				HVI_ManifestedVolume,
				HVI_HVL_LoadList,
				HVI_JS_LoadedOnShipment,
				HVI_ClusterKey,
				HVI_HVC_Consignment,
				HVI_ActualWeight,
				HVI_ActualVolume,
				HVI_KM_LastMileTransportBooking,
				HVI_Status,
				HVI_CurrentBarcode,
				HVI_IsValidatedForUniqueness,
				HVI_ShipperReference,
				HVI_IsDamaged,
				HVI_IsPillaged,
				HVI_IsUllaged,
				HVI_IsUnmanifestedAtDestination,
				HVI_ContainerNumber,
				HVI_DestinationFirstUsageTimeUtc,
				HVI_Height,
				HVI_HVO_OuterPackage,
				HVI_IsActive,
				HVI_Length,
				HVI_OriginFirstUsageTimeUtc,
				HVI_SecurityFilingFirstUsageTimeUtc,
				HVI_ShipperFirstUsageTimeUtc,
				HVI_UnitOfDimension,
				HVI_Width,
				HVI_IsScannedAtDestination,
				HVI_ReleaseStatus,
				HVI_ImportReleaseStatus,
				HVI_ExportReleaseStatus,
				HVI_UsageType,
				HVI_SystemCreateTimeUtc,
				HVI_SystemCreateUser,
				HVI_SystemLastEditTimeUtc,
				HVI_SystemLastEditUser,
				HVI_GoodsDescription,
				HVI_CarrierBookingStatus,
				HVI_ItemId,
				HVI_LastUsageCode
				)
				VALUES (
				@hvi_pk,								-- HVI_PK
				'PKG',									-- HVI_F3_NKPackType
				10, 									-- HVI_ManifestedWeight
				10, 									-- HVI_ManifestedVolume
				null,									-- HVI_HVL_LoadList
				null,							        -- HVI_JS_LoadedOnShipment
				@hch_cluster_key,						-- HVI_ClusterKey
				@hvc_pk,								-- HVI_HVC_Consignment
				0.000,									-- HVI_ActualWeight
				0.000,									-- HVI_ActualVolume
				null,									-- HVI_KM_LastMileTransportBooking
				'MAN',									-- HVI_Status
				'',										-- HVI_CurrentBarcode
				0,										-- HVI_IsValidatedForUniqueness
				'',										-- HVI_ShipperReference
				0,										-- HVI_IsDamaged
				0,										-- HVI_IsPillaged
				0,										-- HVI_IsUllaged
				0,										-- HVI_IsUnmanifestedAtDestination
				'',										-- HVI_ContainerNumber
				null,									-- HVI_DestinationFirstUsageTimeUtc
				0,										-- HVI_Height,
				null,									-- HVI_HVO_OuterPackage,
				1,										-- HVI_IsActive,
				0,										-- HVI_Length,
				SYSUTCDATETIME(),						-- HVI_OriginFirstUsageTimeUtc,
				SYSUTCDATETIME(),						-- HVI_SecurityFilingFirstUsageTimeUtc,
				SYSUTCDATETIME(),						-- HVI_ShipperFirstUsageTimeUtc,
				'',										-- HVI_UnitOfDimension,
				0,										-- HVI_Width,
				0,										-- HVI_IsScannedAtDestination,
				'N',									-- HVI_ReleaseStatus,
				'N',									-- HVI_ImportReleaseStatus,
				'N',									-- HVI_ExportReleaseStatus,
				'S',									-- HVI_UsageType,
				SYSUTCDATETIME(),						-- HVI_SystemCreateTimeUtc,
				'E',									-- HVI_SystemCreateUser,
				SYSUTCDATETIME(),						-- HVI_SystemLastEditTimeUtc,
				'E',									-- HVI_SystemLastEditUser,
				'',										-- HVI_GoodsDescription,
				'NON',									-- HVI_CarrierBookingStatus,
				CONCAT('Item', @i),						-- HVI_ItemId,
				''										-- HVI_LastUsageCode
				)

			INSERT INTO #TEMPHVLVItemLine(
				HVS_PK,
				HVS_ClusterKey,
				HVS_CC_Lookup,
				HVS_HVI_HVLVItem,
				HVS_RN_NKOriginCountryCode,
				HVS_OriginTariff,
				HVS_DestinationTariff,
				HVS_ItemURL,
				HVS_GoodsDescription,
				HVS_Quantity,
				HVS_NetWeight,
				HVS_GrossWeight,
				HVS_WeightUnit,
				HVS_CustomsValue,
				HVS_IntrinsicValue,
				HVS_ProductCode,
				HVS_SystemCreateTimeUtc,
				HVS_SystemCreateUser,
				HVS_SystemLastEditTimeUtc,
				HVS_SystemLastEditUser,
				HVS_OriginGoodsDescription
				)
				VALUES(
				NEWID(),								-- HVS_PK,
				@hch_cluster_key,						-- HVS_ClusterKey,
				null,									-- HVS_CC_Lookup,
				@hvi_pk,								-- HVS_HVI_HVLVItem,
				'',										-- HVS_RN_NKOriginCountryCode,
				'',										-- HVS_OriginTariff,
				'',										-- HVS_DestinationTariff,
				'',										-- HVS_ItemURL,
				'',										-- HVS_GoodsDescription,
				1,										-- HVS_Quantity,
				0.000,									-- HVS_NetWeight,
				10.000,									-- HVS_GrossWeight,
				'KG',									-- HVS_WeightUnit,
				10.0000,								-- HVS_CustomsValue,
				0.0000,									-- HVS_IntrinsicValue,
				'',										-- HVS_ProductCode,
				SYSUTCDATETIME(),						-- HVS_SystemCreateTimeUtc,
				'E',									-- HVS_SystemCreateUser,
				SYSUTCDATETIME(),						-- HVS_SystemLastEditTimeUtc,
				'E',									-- HVS_SystemLastEditUser,
				''										-- HVS_OriginGoodsDescription
				);

			SET @i = @i + 1;
			SET @count = @count + 1;
			SET @hvc_pk = NEWID();
			SET @hvi_pk = NEWID();
		END

		INSERT INTO dbo.HVLVConsignment SELECT * FROM #TEMPHVLVConsignment;
		INSERT INTO dbo.HVLVItem SELECT * FROM #TEMPHVLVItem;
		INSERT INTO dbo.HVLVItemLine SELECT * FROM #TEMPHVLVItemLine;

		DROP TABLE #TEMPHVLVConsignment;
		DROP TABLE #TEMPHVLVItem;
		DROP TABLE #TEMPHVLVItemLine;

	COMMIT TRANSACTION tran_insert
END TRY
BEGIN CATCH
	PRINT '[Error] ' + ERROR_MESSAGE();
	ROLLBACK TRANSACTION tran_insert;
	THROW;
END CATCH
";

			using (var connection = Db.Connection)
			using (connection.TemporarySetDefaultCommandTimeOut(3600))
			{
				using (var cmd = connection.Command(sqlText))
				{
					cmd.CommandType = CommandType.Text;
					cmd.AddParameter("@booking_header_reference", SqlDbType.VarChar, bookingHeaderReference);
					cmd.AddParameter("@consignment_count", SqlDbType.Int, consignmentCount);
					cmd.AddParameter("@waybill_prefix", SqlDbType.VarChar, waybillPrefix);
					cmd.ExecuteNonQuery();
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public static void DeleteExistingConsignments(string bookingHeaderReference)
		{
			var sqlText = $@"
BEGIN TRANSACTION tran_delete
	DECLARE
		@booking_header_pk UNIQUEIDENTIFIER,
		@cluster_key INT;

	SELECT @booking_header_pk = CAST(HVH_PK AS UNIQUEIDENTIFIER) FROM dbo.HVLVBookingHeader WHERE HVH_BookingReference = @booking_header_reference;
	SELECT @cluster_key = CAST(HVC_ClusterKey AS INT) FROM dbo.HVLVConsignment WHERE HVC_HVH_BookingHeader = @booking_header_pk;

	DELETE FROM dbo.HVLVItemLine WHERE HVS_ClusterKey = @cluster_key;
	DELETE FROM dbo.HVLVItem WHERE HVI_ClusterKey = @cluster_key;
	DELETE FROM dbo.HVLVConsignment WHERE HVC_ClusterKey = @cluster_key;

	IF @@ERROR != 0
		ROLLBACK TRANSACTION tran_delete
	ELSE
COMMIT TRANSACTION tran_delete;
";
			using (var connection = Db.Connection)
			using (connection.TemporarySetDefaultCommandTimeOut(3600))
			{
				using (var cmd = connection.Command(sqlText))
				{
					cmd.CommandType = CommandType.Text;
					cmd.AddParameter("@booking_header_reference", SqlDbType.VarChar, bookingHeaderReference);
					cmd.ExecuteNonQuery();
				}
			}
		}
	}
}
