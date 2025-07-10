using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.RefDataRepo.Ent.Client.DataStorage;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Common.Contract_0_9;
using CargoWise.RefDbRepo.Common.Models;

namespace CargoWise.RefDataRepo.Ent.Client.Test
{
	class RefShippingLineMessagingRequirementTypeTest : OneODTableDataSetUpdaterTest<RefShippingLineMessagingRequirementType, IRefShippingLineMessagingRequirementType>
	{
		protected override string ColumnForUpdate => nameof(IRefShippingLineMessagingRequirementType.RST_Description);

		protected override void PrepareData()
		{
			base.PrepareData();
			conn.ExecuteNonQuery(@"
DELETE FROM dbo.RefShippingLineMessagingRequirementType;
INSERT INTO dbo.RefShippingLineMessagingRequirementType (RST_PK, RST_Code, RST_Description, RST_SystemCreateTimeUtc, RST_SystemCreateUser, RST_SystemLastEditTimeUtc, RST_SystemLastEditUser)
VALUES ('0D77AD03-BC7D-42A4-84DB-0185B296B4D4', 'TST', 'Test', GetUTCDate(), 'XX', GetUTCDate(), 'XX');");
		}
	}
	class UNDGCommonDataUpdaterTest : OneODTableDataSetUpdaterTest<UNDGCommonData, IUNDGCommonData>
	{
		protected override string ColumnForUpdate => nameof(IUNDGCommonData.DC_Descriptor);

		protected override void PrepareData()
		{
			base.PrepareData();
			conn.ExecuteNonQuery(@"
DELETE FROM dbo.UNDGCommonData;
INSERT INTO dbo.UNDGCommonData (DC_PK, DC_Language, DC_Type, DC_Index, DC_Descriptor, DC_IsSystem, DC_SystemLastEditUser, DC_SystemCreateTimeUtc, DC_SystemCreateUser, DC_SystemLastEditTimeUtc)
VALUES ('0D77AD03-BC7D-42A4-84DB-0185B296B4D4', 'test', 'TST', 'test', 'test', 1, 'XX', GetUTCDate(), 'XX', GetUTCDate());");
		}
	}

	class RefMaterialUpdaterTest : OneODTableDataSetUpdaterTest<RefMaterial, IRefMaterial>
	{
		protected override string ColumnForUpdate => nameof(IRefMaterial.RMC_Description);

		protected override void PrepareData()
		{
			base.PrepareData();
			conn.ExecuteNonQuery(@"
DELETE FROM dbo.RefMaterial;
INSERT INTO dbo.RefMaterial (RMC_PK, RMC_Code, RMC_Group, RMC_Description, RMC_IsActive, RMC_SystemLastEditUser, RMC_SystemCreateTimeUtc, RMC_SystemCreateUser, RMC_SystemLastEditTimeUtc)
VALUES ('94EBBD37-71EA-453E-AC67-0108A2D05261', 'AA', 'CEDEX', 'AAA', 1, 'XX', GetUTCDate(), 'XX', GetUTCDate());");
		}
	}

	class RefDamageUpdaterTest : OneODTableDataSetUpdaterTest<RefDamage, IRefDamage>
	{
		protected override string ColumnForUpdate => nameof(IRefDamage.RFM_Description);

		protected override void PrepareData()
		{
			base.PrepareData();
			conn.ExecuteNonQuery(@"
DELETE FROM dbo.RefDamage;
INSERT INTO dbo.RefDamage (RFM_PK, RFM_Code, RFM_Group, RFM_Description, RFM_IsActive, RFM_SystemLastEditUser, RFM_SystemCreateTimeUtc, RFM_SystemCreateUser, RFM_SystemLastEditTimeUtc)
VALUES ('8E48899A-D774-483C-B9F2-C79003B92E3A', 'AA', 'CEDEX', 'AAA', 1, 'XX', GetUTCDate(), 'XX', GetUTCDate());");
		}
	}

	class RefRepairCodeUpdaterTest : OneODTableDataSetUpdaterTest<RefRepairCode, IRefRepairCode>
	{
		protected override string ColumnForUpdate => nameof(IRefRepairCode.RRC_Description);

		protected override void PrepareData()
		{
			base.PrepareData();
			conn.ExecuteNonQuery(@"
DELETE FROM dbo.RefRepairCode;
INSERT INTO dbo.RefRepairCode (RRC_PK, RRC_Code, RRC_Group, RRC_Description, RRC_IsActive, RRC_ServiceType, RRC_SystemLastEditUser, RRC_SystemCreateTimeUtc, RRC_SystemCreateUser, RRC_SystemLastEditTimeUtc)
VALUES ('8E48899A-D774-483C-B9F2-C79003B92E3A', 'AA', 'CEDEX', 'AAA', 1, 'RPR', 'XX', GetUTCDate(), 'XX', GetUTCDate());");
		}
	}

	class RefUnitSectionUpdaterTest : OneODTableDataSetUpdaterTest<RefUnitSection, IRefUnitSection>
	{
		protected override string ColumnForUpdate => nameof(IRefUnitSection.RUS_Description);

		protected override void PrepareData()
		{
			base.PrepareData();
			conn.ExecuteNonQuery(@"
DELETE FROM dbo.RefUnitSection;
INSERT INTO dbo.RefUnitSection (RUS_PK, RUS_Code, RUS_Group, RUS_Description, RUS_SystemLastEditUser, RUS_SystemCreateTimeUtc, RUS_SystemCreateUser, RUS_SystemLastEditTimeUtc)
VALUES ('8E48899A-D774-483C-B9F2-C79003B92E3A', 'AA', 'CEDEX', 'AAA', 'XX', GetUTCDate(), 'XX', GetUTCDate());");
		}
	}

	class RefEquipmentGradeUpdaterTest : OneODTableDataSetUpdaterTest<RefEquipmentGrade, IRefEquipmentGrade>
	{
		protected override string ColumnForUpdate => nameof(IRefEquipmentGrade.REG_Description);

		protected override void PrepareData()
		{
			base.PrepareData();
			conn.ExecuteNonQuery(@"
DELETE FROM dbo.RefEquipmentGrade;
INSERT INTO dbo.RefEquipmentGrade (REG_PK, REG_IsActive, REG_Code, REG_Description, REG_SystemCreateTimeUtc, REG_SystemCreateUser, REG_SystemLastEditTimeUtc, REG_SystemLastEditUser)
VALUES ('8E48899A-D774-483C-B9F2-C79003B95E2A', 1, 'CAD', 'AAA', GetUTCDate(), 'XX', GetUTCDate(),'XX');");
		}
	}

	class RefMRComponentCodeUpdaterTest : OneODTableDataSetUpdaterTest<RefMRComponentCode, IRefMRComponentCode>
	{
		protected override string ColumnForUpdate => nameof(IRefMRComponentCode.RCC_Description);

		protected override void PrepareData()
		{
			base.PrepareData();
			conn.ExecuteNonQuery(@"
DELETE FROM dbo.RefMRComponentCode;
INSERT INTO dbo.RefMRComponentCode (RCC_PK, RCC_Code, RCC_Group, RCC_Description, RCC_IsActive, RCC_Machinery, RCC_Structural, RCC_TankCleaning, RCC_TankRepair, RCC_SystemLastEditUser, RCC_SystemCreateTimeUtc, RCC_SystemCreateUser, RCC_SystemLastEditTimeUtc)
VALUES ('94EBBD37-71EA-453E-AC67-0108A2D05261', 'AA', 'CEDEX', 'AAA', 1, 1, 1, 1, 1, 'XX', GetUTCDate(), 'XX', GetUTCDate());");
		}
	}

	class RefComplianceCommodityAlertUpdaterTest : OneODTableDataSetUpdaterTest<RefComplianceCommodityAlert, IRefComplianceCommodityAlert>
	{
		protected override string ColumnForUpdate => nameof(IRefComplianceCommodityAlert.RCR_IsActive);
		protected override object ValueForUpdate => true;

		protected override IDataSetUpdater GetUpdater(IServerProxy proxy)
		{
			return new OneTableDataSetUpdater<RefComplianceCommodityAlert, IRefComplianceCommodityAlert>(proxy, dbHelper, versionControlManager, false, true);
		}

		protected override void PrepareData()
		{
			base.PrepareData();
			conn.ExecuteNonQuery(@"
DELETE FROM dbo.RefComplianceCommodityAlert;
INSERT INTO dbo.RefComplianceCommodityAlert (RCR_PK,RCR_IsActive,RCR_AlertCode,RCR_AlertDescription,RCR_AlertName,RCR_AlertType,RCR_CountryRegion,RCR_PublishYear,RCR_SourceURL,RCR_TradeDirection, RCR_SystemCreateTimeUtc, RCR_SystemLastEditTimeUtc,RCR_SystemCreateUser, RCR_SystemLastEditUser)
VALUES ('94EBBD37-71EA-453E-AC67-0108A2D05261', 0, 'AA', 'AA', 'AA', 'NOM', 'XX', 1999, 'XX', 'EXP', GetUTCDate(), GetUTCDate(), 'SYD','E')");
		}
	}

	[UseSnapshotProtection]
	abstract class OneODTableDataSetUpdaterTest<TServer, TStorage> : OneTableDataSetUpdaterTest<TServer, TStorage>
		where TServer : RefDataSet
		where TStorage : class, IDataSetStorage
	{
		protected override IDataSetUpdater GetUpdater(IServerProxy proxy)
		{
			return new OneTableDataSetUpdater<TServer, TStorage>(proxy, dbHelper, versionControlManager);
		}

		protected override void SetUp()
		{
			conn = ((IDbConnectionInternals)Db.NewAdminConnection()).ADOConnection;
			dbHelper = new DBHelper(conn);
			versionControlManager = new RefVersionControlManager(dbHelper);
			versionControlManager.IsMainDb = true;
		}
	}
}
