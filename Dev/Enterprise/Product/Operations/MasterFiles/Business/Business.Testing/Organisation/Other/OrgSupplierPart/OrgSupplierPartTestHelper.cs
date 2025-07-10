using System;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	[SuppressMessage("CargoWiseOne", "CW1107:DoNotUseDbConnectionMethods", Justification = "Testing code")]
	public static class OrgSupplierPartTestHelper
	{
		public static ZGuid GetWarehouseMainAddressPK(BusinessObjectFactory factory, ZGuid warehousePK)
		{
			var warehouse = factory.Load<OrgHeader>(warehousePK);
			if (warehouse == null)
			{
				warehouse = factory.New<OrgHeader>();
				warehouse.OH_Code = "W1";
				warehouse.OH_RL_NKClosestPort = factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode)).RL_Code;
				warehouse.MainAddress.OA_Address1 = "W1 ADDRESS 1";
				warehouse.MainAddress.LocalControlledPremisesID = "23423";
				warehouse.MainAddress.OA_RN_NKCountryCode = "US";
			}
			factory.Save();

			return warehouse.MainAddress.PK;
		}

		public static ZGuid GetImporterPK(BusinessObjectFactory factory)
		{
			var importer = factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "IMP");
			if (importer == null)
			{
				importer = factory.New<OrgHeader>();
				importer.OH_Code = "IMP";
				importer.OH_FullName = "TestImp";
				importer.MainAddress.OA_Address1 = "IMP ADDRESS 1";
				importer.MiscServ.OM_IMPartAttrib1Name = "VIN1";
				importer.MiscServ.OM_IMPartAttrib1Type = "NON";
				importer.CompanyData.OB_IMUsedBondedWhs = true;
				importer.OH_IsConsignee = true;
				importer.OH_IsWarehouseClient = true;
			}
			factory.Save();

			return importer.PK;
		}

		public static Guid GetNewTransactionPK(DbConnection testConnection, OrgSupplierPart part, ZGuid warehouseMainAddressPK, ZGuid importerPK)
		{
			var batchPk = Guid.NewGuid();
			var transactionPk = Guid.NewGuid();
			var sql = $@"INSERT INTO dbo.CusWHSOperatorTransactionBatch (WOB_PK, WOB_Batch, WOB_GC_Company, WOB_OA_Warehouse, WOB_SystemCreateTimeUtc, WOB_SystemCreateUser, WOB_SystemLastEditTimeUtc, WOB_SystemLastEditUser) 
					     VALUES ({batchPk.ToSqlGuid()}, 'Test Batch 1', {GlbCompany.CurrentCompany.PK.ToSqlGuid()}, {warehouseMainAddressPK.ToSqlGuid()}, GetUtcDate(), '~BP', GetUtcDate(), '~BP');

						 INSERT INTO dbo.CusWHSOperatorTransaction (WOT_PK, WOT_OP_Product, WOT_Status, WOT_TransactionDate, WOT_OwnerReference, WOT_OH_ProductOwner, WOT_Quantity, WOT_WOB_CusWHSTransactionBatch, WOT_BatchLineNo, WOT_SystemCreateTimeUtc, WOT_SystemCreateUser, WOT_SystemLastEditTimeUtc, WOT_SystemLastEditUser) 
					     VALUES ({transactionPk.ToSqlGuid()}, {part.PK.ToSqlGuid()}, 'CAN', GetUtcDate(), 'test ref', {importerPK.ToSqlGuid()}, 1, {batchPk.ToSqlGuid()}, 1, GetUtcDate(), '~BP', GetUtcDate(), '~BP');";

			using (var cmd = testConnection.Command(sql))
			{
				cmd.ExecuteNonQuery();
			}

			return transactionPk;
		}

		public static void UpdateTransactionStatus(DbConnection testConnection, Guid transactionPK, string status)
		{
			var sql = $@"UPDATE dbo.CusWHSOperatorTransaction
						 SET WOT_Status = '{status}', WOT_SystemLastEditTimeUtc = GetUtcDate(), WOT_SystemLastEditUser = 'IP'
						 WHERE WOT_PK = {transactionPK.ToSqlGuid()}";

			using (var cmd = testConnection.Command(sql))
			{
				cmd.ExecuteNonQuery();
			}
		}
	}
}
