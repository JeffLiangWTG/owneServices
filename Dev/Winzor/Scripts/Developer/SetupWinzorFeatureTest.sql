-- This script enables Winzor hybrid mode for ALL users
-- Creates a user: username - hybrid1, password - test

BEGIN TRAN
DECLARE @user VARCHAR(MAX) = 'hybrid1';
DECLARE @true VARBINARY(MAX) = CAST(N'True' as varbinary(max))
DECLARE @email               VARBINARY(MAX) = CAST(N'default@example.com' AS VARBINARY(MAX))
DECLARE @remoteConnectorMode VARBINARY(MAX) = CAST(N'ANY' AS VARBINARY(MAX))
DECLARE @updateFrequency     VARBINARY(MAX) = CAST(N'0' AS VARBINARY(MAX))
DECLARE @featureTestGroup    UNIQUEIDENTIFIER = '94755E71-A87A-4034-8DFA-785773A49607';


IF EXISTS(SELECT *
		  FROM dbo.StmData
		  WHERE [SD_Name] = 'FeatureTestModeEnabled')
	UPDATE dbo.StmData
	SET [SD_BinaryValue] = @true
	WHERE [SD_Name] = 'FeatureTestModeEnabled';

ELSE
	INSERT dbo.StmData
		([SD_PK],
		 [SD_Type],
		 [SD_Name],
		 [SD_IsLogged],
		 [SD_SystemLastEditUser],
		 [SD_SystemCreateUser],
		 [SD_BinaryValue])
	VALUES
		(NEWID(),
		 'BOL',
		 'FeatureTestModeEnabled',
		 1,
		 'E',
		 'E',
		 @true);

IF EXISTS(SELECT *
		  FROM dbo.StmData
		  WHERE [SD_Name] = 'MailboxEmailAddress')
	UPDATE dbo.StmData
	SET [SD_BinaryValue] = @email
	WHERE [SD_Name] = 'MailboxEmailAddress';


ELSE
	INSERT dbo.StmData
		([SD_PK],
		 [SD_Type],
		 [SD_Name],
		 [SD_IsLogged],
		 [SD_SystemLastEditUser],
		 [SD_SystemCreateUser],
		 [SD_BinaryValue])
	VALUES
		(NEWID(),
		 'BOL',
		 'MailboxEmailAddress',
		 1,
		 'E',
		 'E',
		 @email);

IF EXISTS(SELECT *
		  FROM dbo.StmData
		  WHERE [SD_Name] = 'EmailDestinationOverride')
	UPDATE dbo.StmData
	SET [SD_BinaryValue] = @email
	WHERE [SD_Name] = 'EmailDestinationOverride';


ELSE
	INSERT dbo.StmData
		([SD_PK],
		 [SD_Type],
		 [SD_Name],
		 [SD_IsLogged],
		 [SD_SystemLastEditUser],
		 [SD_SystemCreateUser],
		 [SD_BinaryValue])
	VALUES
		(NEWID(),
		 'BOL',
		 'EmailDestinationOverride',
		 1,
		 'E',
		 'E',
		 @email);

IF EXISTS(SELECT *
		  FROM dbo.StmData
		  WHERE [SD_Name] = 'RemoteAppAllowEDocAccessWithoutConnectorMode')
	UPDATE dbo.StmData
	SET [SD_BinaryValue] = @remoteConnectorMode
	WHERE [SD_Name] = 'RemoteAppAllowEDocAccessWithoutConnectorMode';


ELSE
	INSERT dbo.StmData
		([SD_PK],
		 [SD_Type],
		 [SD_Name],
		 [SD_IsLogged],
		 [SD_SystemLastEditUser],
		 [SD_SystemCreateUser],
		 [SD_BinaryValue])
	VALUES
		(NEWID(),
		 'STR',
		 'RemoteAppAllowEDocAccessWithoutConnectorMode',
		 1,
		 'E',
		 'E',
		 @remoteConnectorMode);

IF EXISTS(SELECT *
		  FROM dbo.StmData
		  WHERE [SD_Name] = 'StaffDetailsUpdateFrequency')
	UPDATE dbo.StmData
	SET [SD_BinaryValue] = @updateFrequency
	WHERE [SD_Name] = 'StaffDetailsUpdateFrequency';


ELSE
	INSERT dbo.StmData
		([SD_PK],
		 [SD_Type],
		 [SD_Name],
		 [SD_IsLogged],
		 [SD_SystemLastEditUser],
		 [SD_SystemCreateUser],
		 [SD_BinaryValue])
	VALUES
		(NEWID(),
		 'INT',
		 'StaffDetailsUpdateFrequency',
		 1,
		 'E',
		 'E',
		 @updateFrequency);

IF (SELECT SFT_PK
	FROM dbo.StmFeatureTest
	WHERE SFT_FeatureName = 'WINZOR'
	  AND SFT_GG_Group = @featureTestGroup) IS NULL
	BEGIN
		INSERT
		INTO dbo.StmFeatureTest
			([SFT_PK],
			 [SFT_IsActive],
			 [SFT_FeatureName],
			 [SFT_GG_Group],
			 [SFT_SystemCreateUser],
			 [SFT_SystemCreateTimeUtc],
			 [SFT_SystemLastEditUser],
			 [SFT_SystemLastEditTimeUtc])
		VALUES
			(NEWID(),
			 1,
			 'WINZOR',
			 @featureTestGroup,
			 'E',
			 GETUTCDATE(),
			 'E',
			 GETUTCDATE())
	END
IF (SELECT SFT_PK
	FROM dbo.StmFeatureTest
	WHERE SFT_FeatureName = 'WINZORALL'
	  AND SFT_GG_Group = @featureTestGroup) IS NULL
	BEGIN
		INSERT
		INTO dbo.StmFeatureTest
			([SFT_PK],
			 [SFT_IsActive],
			 [SFT_FeatureName],
			 [SFT_GG_Group],
			 [SFT_SystemCreateUser],
			 [SFT_SystemCreateTimeUtc],
			 [SFT_SystemLastEditUser],
			 [SFT_SystemLastEditTimeUtc])
		VALUES
			(NEWID(),
			 1,
			 'WINZORALL',
			 @featureTestGroup,
			 'E',
			 GETUTCDATE(),
			 'E',
			 GETUTCDATE())
	END

DECLARE @staffId UNIQUEIDENTIFIER = (SELECT GS_PK
									 FROM dbo.GlbStaff
									 WHERE GS_Code = 'HYB')

IF @staffId IS NULL
	BEGIN
		SET @staffId = NEWID();

		INSERT [dbo].[GlbStaff]
			([GS_PK],
			 [GS_Code],
			 [GS_IsActive],
			 [GS_LoginName],
			 [GS_IsController],
			 [GS_City],
			 [GS_State],
			 [GS_Postcode],
			 [GS_FullName],
			 [GS_IsOperational],
			 [GS_CanLogin],
			 [GS_ValidationStatus],
			 [GS_ActivityTrackingStatus],
			 [GS_PasswordHash],
			 [GS_PasswordHashIterations],
			 [GS_PasswordSalt],
			 [GS_GB_HomeBranch],
			 [GS_GE_HomeDepartment],
			 [GS_SystemCreateUser],
			 [GS_SystemCreateTimeUtc],
			 [GS_SystemLastEditUser],
			 [GS_SystemLastEditTimeUtc])
		VALUES
			(@staffId,
			 N'HYB',
			 1,
			 @user,
			 1,
			 'Sydney',
			 'NSW',
			 '5000',
			 'Hybrid',
			 1,
			 1,
			 'NYV',
			 'CMP',
			 0xA9596713E3E6F2E8DE6A5F2D840BE51DF258052C,
			 200000,
			 0xFF9D98980B60E60395C31007EAD9D875,
			 'fdd429d2-648c-4895-8f9f-06e90ded2be5',
			 '86bb1c22-0865-4685-996e-d56cbd136491',
			 'E',
			 GETUTCDATE(),
			 'E',
			 GETUTCDATE())
	END

IF (SELECT [GK_PK]
	FROM [GlbGroupLink]
	WHERE [GK_GG] = @featureTestGroup
	  AND [GK_GS] = @staffId) IS NULL
	BEGIN

		INSERT [dbo].[GlbGroupLink]
			([GK_PK],
			 [GK_IsValid],
			 [GK_MembershipType],
			 [GK_SkillLevel],
			 [GK_GG],
			 [GK_GS],
			 [GK_CapacityLimitPercent],
			 [GK_AutoVersion],
			 [GK_SystemCreateUser],
			 [GK_SystemCreateTimeUtc],
			 [GK_SystemLastEditUser],
			 [GK_SystemLastEditTimeUtc])
		VALUES
			(NEWID(),
			 1,
			 N'UDF',
			 0,
			 @featureTestGroup,
			 @staffId,
			 100,
			 0,
			 'E',
			 GETUTCDATE(),
			 'E',
			 GETUTCDATE())
	END

COMMIT TRAN
