begin transaction

DECLARE @CARGOWISE_BK_CCPK			uniqueIdentifier = '67E1893C-D619-4EE0-B45B-BF0DE3F0CF7A'	-- SELECT NEWID()
DECLARE @CARGOWISE_SI_CCPK			uniqueIdentifier = '2228A0B8-5059-4DC5-A4FB-03E4B58419CF'	-- SELECT NEWID()
DECLARE @CARGOWISE_VM_CCPK			uniqueIdentifier = 'F7284055-250F-47CF-8008-2579FB0BC785'	-- SELECT NEWID()
DECLARE @CARGOWISE_BC_CCPK			uniqueIdentifier = 'E7093B83-5FEC-4403-8031-48484346B055'	-- SELECT NEWID()
DECLARE @CARGOWISE_AC_CCPK			uniqueIdentifier = '0C6CAF05-1E90-4B13-A664-870FD1535789'	-- SELECT NEWID()
DECLARE @CARGOWISE_BL_CCPK			uniqueIdentifier = 'AE0759BE-C18D-43B7-B0EC-FF6674FA0513'	-- SELECT NEWID()
DECLARE @CARGOWISE_CT_CCPK			uniqueIdentifier = '586881D6-BADC-43CD-B9A8-31EB2D55A1C6'	-- SELECT NEWID()

INSERT INTO eHubClient (CC_PK, CC_ID, CC_FriendlyName, CC_Odyssey_OH, CC_DistributionZone, CC_EmailAddress, CC_Password, CC_OwnerCategory, CC_SystemCategory)
VALUES
(@CARGOWISE_BK_CCPK, 'CARGOWISE_BK', 'CargoWise Booking Request','00000000-0000-0000-0000-000000000000', '75419F4C-C522-4890-BD5D-BCA5E12268F6', '', '', 'Service Provider' , 'Third Party')

INSERT INTO eHubClient (CC_PK, CC_ID, CC_FriendlyName, CC_Odyssey_OH, CC_DistributionZone, CC_EmailAddress, CC_Password, CC_OwnerCategory, CC_SystemCategory)
VALUES
(@CARGOWISE_SI_CCPK, 'CARGOWISE_SI', 'CargoWise Shipping Instruction','00000000-0000-0000-0000-000000000000', '75419F4C-C522-4890-BD5D-BCA5E12268F6', '', '', 'Service Provider' , 'Third Party')

INSERT INTO eHubClient (CC_PK, CC_ID, CC_FriendlyName, CC_Odyssey_OH, CC_DistributionZone, CC_EmailAddress, CC_Password, CC_OwnerCategory, CC_SystemCategory)
VALUES
(@CARGOWISE_VM_CCPK, 'CARGOWISE_VM', 'CargoWise Verified Gross Mass','00000000-0000-0000-0000-000000000000', '75419F4C-C522-4890-BD5D-BCA5E12268F6', '', '', 'Service Provider' , 'Third Party')

INSERT INTO eHubClient (CC_PK, CC_ID, CC_FriendlyName, CC_Odyssey_OH, CC_DistributionZone, CC_EmailAddress, CC_Password, CC_OwnerCategory, CC_SystemCategory)
VALUES
(@CARGOWISE_BC_CCPK, 'CARGOWISE_BC', 'CargoWise Booking Confirmation','00000000-0000-0000-0000-000000000000', '75419F4C-C522-4890-BD5D-BCA5E12268F6', '', '', 'Service Provider' , 'Third Party')

INSERT INTO eHubClient (CC_PK, CC_ID, CC_FriendlyName, CC_Odyssey_OH, CC_DistributionZone, CC_EmailAddress, CC_Password, CC_OwnerCategory, CC_SystemCategory)
VALUES
(@CARGOWISE_AC_CCPK, 'CARGOWISE_AC', 'CargoWise Acknowledgment','00000000-0000-0000-0000-000000000000', '75419F4C-C522-4890-BD5D-BCA5E12268F6', '', '', 'Service Provider' , 'Third Party')

INSERT INTO eHubClient (CC_PK, CC_ID, CC_FriendlyName, CC_Odyssey_OH, CC_DistributionZone, CC_EmailAddress, CC_Password, CC_OwnerCategory, CC_SystemCategory)
VALUES
(@CARGOWISE_BL_CCPK, 'CARGOWISE_BL', 'CargoWise Draft BL Data','00000000-0000-0000-0000-000000000000', '75419F4C-C522-4890-BD5D-BCA5E12268F6', '', '', 'Service Provider' , 'Third Party')

INSERT INTO eHubClient (CC_PK, CC_ID, CC_FriendlyName, CC_Odyssey_OH, CC_DistributionZone, CC_EmailAddress, CC_Password, CC_OwnerCategory, CC_SystemCategory)
VALUES
(@CARGOWISE_CT_CCPK, 'CARGOWISE_CT', 'CargoWise Cargo Tracking','00000000-0000-0000-0000-000000000000', '75419F4C-C522-4890-BD5D-BCA5E12268F6', '', '', 'Service Provider' , 'Third Party')

rollback