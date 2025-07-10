using System;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using NUnit.Framework;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService.Test
{
	class BulkInsertDuplicateKeyExceptionHandlerFixture
	{
		string GetConnectionString(string dbName)
			=> TestConnectionString.GetAdmin(dbName);

		[Test]
		[CreateDatabase("C4F06107FF884298A833E694AF631338", DbSchema.RefDbRepoSafe, ActionTargets.Test)]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public async Task DeleteDuplicateRecord()
		{
			var dbName = CreateDatabaseAttribute.DbNamePrefix + "C4F06107FF884298A833E694AF631338";
			var connectionString = GetConnectionString(dbName);
			using var entities = new SafeDbContext(connectionString);
			try
			{
				var prepareDataSql = @"INSERT INTO [dbo].[RefDataGrouping] (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description)
VALUES(NEWID(), 'ZA', 'South Africa');
INSERT INTO [dbo].[RefCusTariffType] (ZZI_PK, ZZI_TariffType,ZZI_Description,ZZI_ZZZ_NKDataGrouping)
VALUES ('99CAB769-6E32-4A0D-9DFE-315270DD5DF2', 'TST', 'Test','ZA');

INSERT INTO [dbo].[RefCusTariff] (ZZ1_PK,ZZ1_ZZI_TariffType,ZZ1_TariffCode,ZZ1_IAMUnique,ZZ1_Description,ZZ1_StartDate,ZZ1_EndDate,ZZ1_ZZF_NKTaxOrFeeCode,ZZ1_ZZZ_NKDataGrouping,ZZ1_CompositeKeyOnZZ5)
VALUES ('45F88B43-378D-42B4-8803-B24CCDD7211D','99CAB769-6E32-4A0D-9DFE-315270DD5DF2','020322',0,'Description','2024-01-01 00:00:00','2079-06-06 23:59:00','VAT','ZA','');
INSERT INTO RefCusRate (ZZ2_PK,ZZ2_ZZ1_Tariff,ZZ2_StartDate,ZZ2_EndDate,ZZ2_RateFormula,ZZ2_SelectorFormula,ZZ2_ZZZ_NKDataGrouping)
VALUES ('92931F60-828D-4DAB-AAFE-529722ADA3CB','45F88B43-378D-42B4-8803-B24CCDD7211D','2024-01-01 00:00:00','2025-01-01 00:00:00','0','','ZA');
INSERT INTO RefCusApplicability (ZZT_PK,ZZT_ZZ2_Rate,ZZT_StartDate,ZZT_EndDate,ZZT_AdditionalCode,ZZT_OrderNumber)
VALUES (NEWID(),'92931F60-828D-4DAB-AAFE-529722ADA3CB','2024-01-01 00:00:00','2025-01-01 00:00:00','A','1');";
				await entities.Database.ExecuteSqlRawAsync(prepareDataSql);

				var applicability = new RefCusApplicability
				{
					ZZT_PK = Guid.NewGuid(),
					ZZT_ZZ2_Rate = Guid.Parse("92931F60-828D-4DAB-AAFE-529722ADA3CB"),
					ZZT_StartDate = new DateTime(2024, 1, 1),
					ZZT_EndDate = new DateTime(2026, 1, 1),
					ZZT_AdditionalCode = "A",
					ZZT_OrderNumber = "1"
				};
				entities.RefCusApplicabilities.Add(applicability);
				await entities.SaveChangesAsync();
			}
			catch (Exception exception)
			{
				var innerException = exception.InnerException;
				while (innerException != null)
				{
					if (innerException.GetType() == typeof(SqlException))
					{
						break;
					}
					innerException = innerException.InnerException;
				}
				using var transaction = entities.Database.BeginTransaction();
				var handler = new BulkInsertDuplicateKeyExceptionHandler<RefCusApplicability>(innerException as SqlException, entities);
				Assert.DoesNotThrowAsync(handler.DeleteDuplicateRecord);
			}
		}

		[Test]
		[CreateDatabase("C4F06107FF884298A833E694AF631338", DbSchema.RefDbRepoSafe, ActionTargets.Test)]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public async Task DeleteDuplicateRecord_QuotationMarkInValues()
		{
			var dbName = CreateDatabaseAttribute.DbNamePrefix + "C4F06107FF884298A833E694AF631338";
			var connectionString = GetConnectionString(dbName);
			using var entities = new SafeDbContext(connectionString);
			var prepareDataSql = @"
INSERT INTO [dbo].[RefDataGrouping] (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description)
VALUES(NEWID(), 'ZA', 'South Africa');
INSERT INTO [dbo].[RefVesselZZ] (ZZO_PK, ZZO_Code,ZZO_RadioCallSign,ZZO_VesselType,ZZO_RN_NKCountryOfReg,ZZO_LloydsNumber,ZZO_ZZZ_NKDataGrouping)
VALUES ('8AB5F3A0-71C5-4900-A625-875BDDAEBB21', 'CIELO D'' EUROPA', '5LET6','','','','ZA');
UPDATE [dbo].[RefDbVersionControl] SET RVC_Deleted=1 WHERE RVC_ParentPK='8AB5F3A0-71C5-4900-A625-875BDDAEBB21';
";
			await entities.Database.ExecuteSqlRawAsync(prepareDataSql);
			Assert.That(entities.RefVesselZZs.Any(x => x.ZZO_Code == "CIELO D' EUROPA"));

			try
			{
				var vesselZZ = new RefVesselZZ
				{
					ZZO_PK = Guid.NewGuid(),
					ZZO_Code = "CIELO D' EUROPA",
					ZZO_RadioCallSign = "5LET6",
					ZZO_VesselType = "",
					ZZO_RN_NKCountryOfReg = "",
					ZZO_LloydsNumber = "2",
					ZZO_ZZZ_NKDataGrouping = "ZA"
				};
				entities.RefVesselZZs.Add(vesselZZ);
				await entities.SaveChangesAsync();
				Assert.Fail("Should have thrown an exception");
			}
			catch (Exception exception)
			{
				var innerException = exception.InnerException;
				while (innerException != null)
				{
					if (innerException.GetType() == typeof(SqlException))
					{
						break;
					}
					innerException = innerException.InnerException;
				}
				using var transaction = entities.Database.BeginTransaction();
				var handler = new BulkInsertDuplicateKeyExceptionHandler<RefVesselZZ>(innerException as SqlException, entities);
				Assert.DoesNotThrowAsync(async () => await handler.DeleteDuplicateRecord());
				Assert.That(!entities.RefVesselZZs.Any(x => x.ZZO_Code == "CIELO D' EUROPA"));
			}
		}

		[Test]
		public void GetValuesFromExceptionMessage()
		{
			var message1 = @"Cannot insert duplicate key row in object 'dbo.RefCusApplicability' with unique index 'IX_RefCusApplicability_Rate_StartDate_TradeGroup_AdditionalCode_OrderNumber_Conditions_SecondTradeGroup'. The duplicate key value is (<NULL>, Aug  1 2008 12:00AM, 7229d837-15e8-4b5c-9910-a0fb928bd483, R054, , 38f67fd9-8936-4438-ae67-4641741ca5c6, 87a15ec6-e119-4253-83a7-2ea055a573e2).
The statement has been terminated.";
			Assert.DoesNotThrow(() => BulkInsertDuplicateKeyExceptionHandler<Type>.GetValuesFromExceptionMessage(message1).ToList());
			var result = BulkInsertDuplicateKeyExceptionHandler<Type>.GetValuesFromExceptionMessage(message1).ToList();
			Assert.That(result.Count, Is.EqualTo(7));
			Assert.That(result.ElementAt(0), Is.EqualTo("<NULL>"));
			Assert.That(result.ElementAt(1), Is.EqualTo("Aug  1 2008 12:00AM"));
			Assert.That(result.ElementAt(2), Is.EqualTo("7229d837-15e8-4b5c-9910-a0fb928bd483"));
			Assert.That(result.ElementAt(3), Is.EqualTo("R054"));
			Assert.That(result.ElementAt(4), Is.EqualTo(""));
			Assert.That(result.ElementAt(5), Is.EqualTo("38f67fd9-8936-4438-ae67-4641741ca5c6"));
			Assert.That(result.ElementAt(6), Is.EqualTo("87a15ec6-e119-4253-83a7-2ea055a573e2"));
		}

		[Test]
		public void GetValuesFromExceptionMessageThrowsExceptionWhenNotMatched()
		{
			var incorrectMessage = @"Wrong message, regex won't recognize";
			var exception = Assert.Throws<InvalidOperationException>(() => BulkInsertDuplicateKeyExceptionHandler<Type>.GetValuesFromExceptionMessage(incorrectMessage).ToList());
			Assert.That(exception.Message, Does.Contain("The exception message contains a character not recognize by the BulkInsertDuplicateKeyExceptionHandler."));
		}

		[Test]
		[CreateDatabase("C4F06107FF884298A833E694AF631338", DbSchema.RefDbRepoSafe, ActionTargets.Test)]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public async Task DeleteDuplicateRecordAndDependenciesFromDatabasePivotTables()
		{
			var dbName = CreateDatabaseAttribute.DbNamePrefix + "C4F06107FF884298A833E694AF631338";
			var connectionString = GetConnectionString(dbName);
			var vesselZZPk = Guid.NewGuid();
			var carrierCodePk = Guid.NewGuid();
			var vesselDeletedPk = Guid.NewGuid();
			var pivotPk = Guid.NewGuid();
			var pivot2Pk = Guid.NewGuid();
			var pivotWithDeletedVesselPk = Guid.NewGuid();

			using var entities = new SafeDbContext(connectionString);
			try
			{
				var prepareDataSql = $@"INSERT INTO [dbo].[RefDataGrouping] (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description)
VALUES(NEWID(), 'ZA', 'South Africa');

DECLARE @vesselPkDeleted UNIQUEIDENTIFIER = '{vesselDeletedPk}'
DECLARE @carrierCodePk UNIQUEIDENTIFIER = '{carrierCodePk}'

INSERT INTO RefVesselZZ (ZZO_PK, ZZO_Code, ZZO_RadioCallSign, ZZO_VesselType, ZZO_ZZZ_NKDataGrouping)
VALUES(@vesselPkDeleted, 'Jin Ming 82', 'BOPQ5', 'CV', 'ZA')

INSERT INTO RefCarrierCode (ZZ4_PK, ZZ4_Code, ZZ4_Description, ZZ4_IsSea, ZZ4_IsRoad, ZZ4_IsRail, ZZ4_IsAir, ZZ4_ZZZ_NKDataGrouping)
VALUES('8CF821A0-F0B7-4C2C-8B31-6C8742C09A16', 'NSAP', 'Norvic Shipping Asia Pte. Ltd.', 1, 0, 0, 0, 'ZA'),
('9BCCF131-4117-40A6-907A-72382D816D4D', 'SPI', 'Sinoway Project Inc', 1, 0, 0, 0, 'ZA')

INSERT INTO RefCarrierVesselPivot(ZZQ_PK, ZZQ_ZZ4, ZZQ_ZZO)
VALUES('{pivotPk}', '8CF821A0-F0B7-4C2C-8B31-6C8742C09A16', @vesselPkDeleted),
('{pivot2Pk}', '9BCCF131-4117-40A6-907A-72382D816D4D', @vesselPkDeleted);

UPDATE [dbo].[RefDbVersionControl] SET RVC_Deleted=1 WHERE RVC_ParentPK=@vesselPkDeleted";
				await entities.Database.ExecuteSqlRawAsync(prepareDataSql);

				var vesselZZ = new RefVesselZZ
				{
					ZZO_PK = vesselZZPk,
					ZZO_Code = "Jin Ming 82",
					ZZO_RadioCallSign = "BOPQ5",
					ZZO_VesselType = "CV",
					ZZO_ZZZ_NKDataGrouping = "ZA"
				};
				entities.RefVesselZZs.Add(vesselZZ);
				await entities.SaveChangesAsync();
			}
			catch (Exception exception)
			{
				var innerException = exception.InnerException;
				while (innerException != null)
				{
					if (innerException.GetType() == typeof(SqlException))
					{
						break;
					}
					innerException = innerException.InnerException;
				}
				using var transaction = entities.Database.BeginTransaction();
				var handler = new BulkInsertDuplicateKeyExceptionHandler<RefVesselZZ>(innerException as SqlException, entities);
				Assert.DoesNotThrowAsync(handler.DeleteDuplicateRecord);
				Assert.That(!entities.RefCarrierVesselPivots.Any(x => x.ZZQ_ZZO == vesselDeletedPk));
			}
		}
	}
}
