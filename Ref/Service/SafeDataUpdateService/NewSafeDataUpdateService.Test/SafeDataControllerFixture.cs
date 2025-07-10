using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.Common.Web.Auth;
using CargoWise.RefDbRepo.NewSafeDataUpdateService.Controllers;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.EntityFrameworkCore;
using Moq;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService.Test;

[TestFixture]
class SafeDataControllerFixture
{
	[Test]
	public void SingleResult()
	{
		var dummy1 = new Dummy { XXX_PK = Guid.NewGuid() };
		var dummy2 = new Dummy { XXX_PK = Guid.NewGuid() };
		var repoMock = new Mock<IReferenceDataRepository>();
		repoMock.Setup(x => x.Get<Dummy>()).Returns(new[] { dummy1, dummy2 }.AsQueryable());
		var controllerContext = SetupControllerContext(repoMock.Object);
		var controller = new SafeDataController<Dummy>(new Mock<IAuthorizationHelper>().Object) { ControllerContext = controllerContext };

		Assert.That(controller.Get(dummy1.XXX_PK).FirstOrDefault(), Is.EqualTo(dummy1));
	}

	[Test]
	public void SingleResultWithIdColumn()
	{
		var dummy1 = new DummyWithId { Id = Guid.NewGuid() };
		var dummy2 = new DummyWithId { Id = Guid.NewGuid() };
		var repoMock = new Mock<IReferenceDataRepository>();
		repoMock.Setup(x => x.Get<DummyWithId>()).Returns(new[] { dummy1, dummy2 }.AsQueryable());
		var controllerContext = SetupControllerContext(repoMock.Object);
		var controller = new SafeDataController<DummyWithId>(new Mock<IAuthorizationHelper>().Object) { ControllerContext = controllerContext };

		Assert.That(controller.Get(dummy1.Id).FirstOrDefault(), Is.EqualTo(dummy1));
	}

	[Test]
	[TransactionedTestCase]
	[Property("DAT:CapabilityRequirements", "SQL2019+")]
	public async Task DeleteOrExpire()
	{
		var codeList1PK = Guid.NewGuid();
		var codeList2PK = Guid.NewGuid();
		var codeList3PK = Guid.NewGuid();
		var attr1PK = Guid.NewGuid();
		var attr2PK = Guid.NewGuid();
		var connectionString = GetConnectionString();
		await using (var entities = new SafeDbContext(connectionString))
		{
			entities.RefDataGroupings.Add(new RefDataGrouping
			{
				ZZZ_PK = Guid.NewGuid(),
				ZZZ_DataGrouping = "ZA",
				ZZZ_Description = "South Africar"
			});
			entities.RefCusCodeTypes.Add(new RefCusCodeType
			{
				ZZK_PK = Guid.NewGuid(),
				ZZK_CodeType = "PKG",
				ZZK_Description = "Package",
				ZZK_MaxLength = 0,
				ZZK_ZZZ_NKDataGrouping = "ZA"
			});
			await entities.SaveChangesAsync();
			entities.RefCusCodeListAttributeNames.Add(new RefCusCodeListAttributeName
			{
				ZXE_PK = Guid.NewGuid(),
				ZXE_Name = "A",
				ZXE_ZZK_NKCodeType = "PKG",
				ZXE_ZZZ_NKDataGrouping = "ZA",
				ZXE_Description = "AA",
				ZXE_IsMandatory = false,
				ZXE_AllowDuplicates = false,
				ZXE_IsValueMandatory = false,
				ZXE_ValueDataType = string.Empty,
				ZXE_MinLengthOrValue = 0,
				ZXE_MaxLengthOrValue = 0,
				ZXE_DecimalPlaces = 0,
				ZXE_ColumnCaption = ""
			});
			await entities.SaveChangesAsync();
			entities.RefCusCodeLists.Add(new RefCusCodeList
			{
				ZZD_PK = codeList1PK,
				ZZD_ZZK_NKCodeType = "PKG",
				ZZD_Code = "PE1",
				ZZD_Description = "Pallet, modular, collars 80cms * 120cms ",
				ZZD_StartDate = new DateTime(2019, 01, 01),
				ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 0),
				ZZD_ZZZ_NKDataGrouping = "ZA"
			});
			entities.RefCusCodeLists.Add(new RefCusCodeList
			{
				ZZD_PK = codeList2PK,
				ZZD_ZZK_NKCodeType = "PKG",
				ZZD_Code = "PE2",
				ZZD_Description = "Pallet, modular, collars 80cms * 120cms ",
				ZZD_StartDate = new DateTime(2018, 01, 01),
				ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 0),
				ZZD_ZZZ_NKDataGrouping = "ZA"
			});
			entities.RefCusCodeLists.Add(new RefCusCodeList
			{
				ZZD_PK = codeList3PK,
				ZZD_ZZK_NKCodeType = "PKG",
				ZZD_Code = "PE3",
				ZZD_Description = "Pallet, modular, collars 80cms * 120cms ",
				ZZD_StartDate = new DateTime(2018, 01, 01),
				ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 0),
				ZZD_ZZZ_NKDataGrouping = "ZA"
			});
			entities.RefCusCodeListAttributes.Add(new RefCusCodeListAttribute
			{
				ZZE_PK = attr1PK,
				ZZE_ZZD_CodeList = codeList1PK,
				ZZE_ZXE_NKName = "A",
				ZZE_Value = "A"
			});
			entities.RefCusCodeListAttributes.Add(new RefCusCodeListAttribute
			{
				ZZE_PK = attr2PK,
				ZZE_ZZD_CodeList = codeList2PK,
				ZZE_ZXE_NKName = "A",
				ZZE_Value = "A"
			});
			await entities.SaveChangesAsync();
			var version = entities.RefDbVersionControls.FirstOrDefault(x => x.RVC_ParentPK == codeList3PK);
			version.RVC_Deleted = true;
			entities.RefDbVersionControls.Attach(version);
			entities.Entry(version).State = EntityState.Modified;
			await entities.SaveChangesAsync();
			using (var repo = new ReferenceDataRepository(entities))
			{
				var controllerContext = SetupControllerContext(repo);
				var auth = new Mock<IAuthorizationHelper>();
				auth.Setup(x => x.IsAuthorized<RefCusCodeList>(It.IsAny<string>())).Returns(true);
				auth.Setup(x => x.IsAuthorized<RefCusCodeListAttribute>(It.IsAny<string>())).Returns(true);

				//Expire
				var controller1 = new DataSetHeaderController<RefCusCodeList>(auth.Object) { ControllerContext = controllerContext };
				var parameters = new ODataActionParameters
				{
					{ "IDs", new[] { codeList1PK, codeList2PK, codeList3PK } },
					{ "expiredTime", new DateTimeOffset(new DateTime(2019, 01, 01)) }
				};
				await controller1.BatchExpire(parameters);

				//Delete
				var controller2 = new SafeDataController<RefCusCodeListAttribute>(auth.Object) { ControllerContext = controllerContext };
				parameters = new ODataActionParameters
				{
					{ "IDs", new[] { attr1PK } },
					{ "expiredTime", new DateTimeOffset(new DateTime(2019, 01, 01)) }
				};
				await controller2.BatchExpire(parameters);
			}
		}

		await using (var entities = new SafeDbContext(connectionString))
		{
			var codeList1 = entities.RefCusCodeLists.FirstOrDefault(x => x.ZZD_PK == codeList1PK);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 0), codeList1.ZZD_EndDate);
			var codeList2 = entities.RefCusCodeLists.FirstOrDefault(x => x.ZZD_PK == codeList2PK);
			Assert.AreEqual(new DateTime(2019, 01, 01), codeList2.ZZD_EndDate);
			Assert.NotNull(entities.RefCusCodeListAttributes.FirstOrDefault(x => x.ZZE_PK == attr1PK));
			Assert.NotNull(entities.RefCusCodeListAttributes.FirstOrDefault(x => x.ZZE_PK == attr2PK));
			var codeList3 = entities.RefCusCodeLists.FirstOrDefault(x => x.ZZD_PK == codeList3PK);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 0), codeList3.ZZD_EndDate);
		}
	}

	[Test]
	[TransactionedTestCase]
	[Property("DAT:CapabilityRequirements", "SQL2019+")]
	public async Task DeleteOrExpireFutureData()
	{
		var tariffPK1 = Guid.NewGuid();
		var tariffPK2 = Guid.NewGuid();
		var tariffTypePk = Guid.NewGuid();
		var conditionTypePk = Guid.NewGuid();
		var conditionCodePk = Guid.NewGuid();
		var conditionPK = Guid.NewGuid();
		var tradeGroupPk = Guid.NewGuid();
		var appPk1 = Guid.NewGuid();
		var appPk2 = Guid.NewGuid();
		var exTradePk1 = Guid.NewGuid();
		var exTradePk2 = Guid.NewGuid();
		var connectionString = GetConnectionString();
		await using (var entities = new SafeDbContext(connectionString))
		{
			entities.RefDataGroupings.Add(new RefDataGrouping
			{
				ZZZ_PK = Guid.NewGuid(),
				ZZZ_DataGrouping = "GB",
				ZZZ_Description = "United Kingdom"
			});
			await entities.SaveChangesAsync();

			entities.RefCusTradeGroups.Add(new RefCusTradeGroup
			{
				ZZA_PK = tradeGroupPk,
				ZZA_TradeGroup = "KH",
				ZZA_Description = "Cambodia",
				ZZA_ZZZ_NKDataGrouping = "GB",
				ZZA_StartDate = new DateTime(1900, 1, 1),
				ZZA_EndDate = new DateTime(2079, 06, 06, 23, 59, 0)
			});
			entities.RefCusTariffTypes.Add(new RefCusTariffType
			{
				ZZI_Description = "type",
				ZZI_PK = tariffTypePk,
				ZZI_TariffType = "TA",
				ZZI_ZZZ_NKDataGrouping = "GB",
				ZZI_HasFormulaSpecificQuestions = true,
				ZZI_ZZ9_NKNomenclatureGroupType = ""
			});
			entities.RefCusTariffs.Add(new RefCusTariff
			{
				ZZ1_PK = tariffPK1,
				ZZ1_Description = "Tariff test",
				ZZ1_TariffCode = "0101",
				ZZ1_ZZZ_NKDataGrouping = "GB",
				ZZ1_StartDate = new DateTime(1900, 01, 01),
				ZZ1_EndDate = new DateTime(2079, 06, 06, 23, 59, 0),
				ZZ1_IAMUnique = 1,
				ZZ1_CompositeKeyOnZZ5 = "0101",
				ZZ1_ZZI_TariffType = tariffTypePk,
				ZZ1_ZZF_NKTaxOrFeeCode = ""
			});
			entities.RefCusTariffs.Add(new RefCusTariff
			{
				ZZ1_PK = tariffPK2,
				ZZ1_Description = "Tariff test",
				ZZ1_TariffCode = "0102",
				ZZ1_ZZZ_NKDataGrouping = "GB",
				ZZ1_StartDate = new DateTime(2023, 03, 26),
				ZZ1_EndDate = new DateTime(2079, 06, 06, 23, 59, 0),
				ZZ1_IAMUnique = 1,
				ZZ1_CompositeKeyOnZZ5 = "0102",
				ZZ1_ZZI_TariffType = tariffTypePk,
				ZZ1_ZZF_NKTaxOrFeeCode = ""
			});
			entities.RefCusConditionTypes.Add(new RefCusConditionType
			{
				ZX2_PK = conditionTypePk,
				ZX2_ConditionType = "715",
				ZX2_ZZZ_NKDataGrouping = "GB",
				ZX2_ConditionClass = "CLASS",
				ZX2_Description = "Type test"
			});
			entities.RefCusConditionCodes.Add(new RefCusConditionCode
			{
				ZY7_PK = conditionCodePk,
				ZY7_ConditionCode = "AAA",
				ZY7_Description = "DES",
				ZY7_ZZZ_NKDataGrouping = "GB"
			});
			await entities.SaveChangesAsync();

			entities.RefCusConditions.Add(new RefCusCondition
			{
				ZX1_PK = conditionPK,
				ZX1_ZZ1_Tariff = tariffPK1,
				ZX1_Comment = "Condition Y: Other conditions",
				ZX1_IsExport = true,
				ZX1_IsImport = false,
				ZX1_ZZZ_NKDataGrouping = "GB",
				ZX1_Source = "GB Tariff",
				ZX1_StartDate = new DateTime(1900, 1, 1),
				ZX1_EndDate = new DateTime(2079, 06, 06, 23, 59, 0),
				ZX1_ZX2_ConditionType = conditionTypePk,
				ZX1_ZY7_NKConditionCode = "AAA",
				ZX1_AdditionalComment = "comment"
			});
			await entities.SaveChangesAsync();

			entities.RefCusApplicabilities.Add(new RefCusApplicability
			{
				ZZT_PK = appPk1,
				ZZT_ZX1_Conditions = conditionPK,
				ZZT_AdditionalCode = "",
				ZZT_OrderNumber = "",
				ZZT_StartDate = new DateTime(2022, 1, 29),
				ZZT_EndDate = new DateTime(2023, 9, 30)
			});
			entities.RefCusApplicabilities.Add(new RefCusApplicability
			{
				ZZT_PK = appPk2,
				ZZT_ZX1_Conditions = conditionPK,
				ZZT_AdditionalCode = "",
				ZZT_OrderNumber = "",
				ZZT_StartDate = new DateTime(2023, 10, 1),
				ZZT_EndDate = new DateTime(2079, 06, 06, 23, 59, 0)
			});
			await entities.SaveChangesAsync();

			entities.RefCusExcludedTradeGroups.Add(new RefCusExcludedTradeGroup
			{
				ZZC_PK = exTradePk1,
				ZZC_ZZT_Applicability = appPk1,
				ZZC_ZZA_TradeGroup = tradeGroupPk
			});
			entities.RefCusExcludedTradeGroups.Add(new RefCusExcludedTradeGroup
			{
				ZZC_PK = exTradePk2,
				ZZC_ZZT_Applicability = appPk2,
				ZZC_ZZA_TradeGroup = tradeGroupPk
			});
			await entities.SaveChangesAsync();
			using (var repo = new ReferenceDataRepository(entities))
			{
				var controllerContext = SetupControllerContext(repo);
				var auth = new Mock<IAuthorizationHelper>();
				auth.Setup(x => x.IsAuthorized<RefCusTariff>(It.IsAny<string>())).Returns(true);
				auth.Setup(x => x.IsAuthorized<RefCusCondition>(It.IsAny<string>())).Returns(true);
				auth.Setup(x => x.IsAuthorized<RefCusApplicability>(It.IsAny<string>())).Returns(true);

				//Expire
				var controller1 = new DataSetHeaderController<RefCusTariff>(auth.Object) { ControllerContext = controllerContext };
				var parms = new ODataActionParameters() { { "IDs", new[] { tariffPK1, tariffPK2 } }, { "expiredTime", new DateTimeOffset(new DateTime(2023, 03, 25, 00, 14, 36)) }, { "isExpirable", true } };
				await controller1.BatchExpire(parms);

				//Expire
				var controller2 = new SafeDataController<RefCusCondition>(auth.Object) { ControllerContext = controllerContext };
				parms = new ODataActionParameters() { { "IDs", new[] { conditionPK } }, { "expiredTime", new DateTimeOffset(new DateTime(2023, 03, 25, 00, 14, 36)) }, { "isExpirable", true } };
				await controller2.BatchExpire(parms);

				//DeleteOrExpire
				var controller3 = new SafeDataController<RefCusApplicability>(auth.Object) { ControllerContext = controllerContext };
				parms = new ODataActionParameters() { { "IDs", new[] { appPk1, appPk2 } }, { "expiredTime", new DateTimeOffset(new DateTime(2023, 03, 25, 00, 14, 36)) }, { "isExpirable", true } };
				await controller3.BatchExpire(parms);
			}
		}

		await using (var entities = new SafeDbContext(connectionString))
		{
			var tariff1 = entities.RefCusTariffs.FirstOrDefault(x => x.ZZ1_PK == tariffPK1);
			Assert.AreEqual(new DateTime(2023, 03, 25, 00, 14, 36), tariff1.ZZ1_EndDate);
			var tariff2 = entities.RefCusTariffs.FirstOrDefault(x => x.ZZ1_PK == tariffPK2);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 0), tariff2.ZZ1_EndDate);
			var condition = entities.RefCusConditions.FirstOrDefault(x => x.ZX1_PK == conditionPK);
			Assert.AreEqual(new DateTime(2023, 03, 25, 00, 15, 00), condition.ZX1_EndDate);
			var app1 = entities.RefCusApplicabilities.FirstOrDefault(x => x.ZZT_PK == appPk1);
			Assert.AreEqual(new DateTime(2023, 03, 25, 00, 15, 00), app1.ZZT_EndDate);
			Assert.Null(entities.RefCusApplicabilities.FirstOrDefault(x => x.ZZT_PK == appPk2));
			Assert.NotNull(entities.RefCusExcludedTradeGroups.FirstOrDefault(x => x.ZZC_PK == exTradePk1));
		}
	}

	[Test]
	[TransactionedTestCase]
	[Property("DAT:CapabilityRequirements", "SQL2019+")]
	public async Task DeleteOrExpireOnlyEndOfTime()
	{
		var codeList1PK = Guid.NewGuid();
		var codeList2PK = Guid.NewGuid();
		var codeList3PK = Guid.NewGuid();
		var connectionString = GetConnectionString();
		await using (var entities = new SafeDbContext(connectionString))
		{
			entities.RefDataGroupings.Add(new RefDataGrouping
			{
				ZZZ_PK = Guid.NewGuid(),
				ZZZ_DataGrouping = "ZA",
				ZZZ_Description = "South Africa"
			});
			entities.RefCusCodeTypes.Add(new RefCusCodeType
			{
				ZZK_PK = Guid.NewGuid(),
				ZZK_CodeType = "PKG",
				ZZK_Description = "Package",
				ZZK_MaxLength = 0,
				ZZK_ZZZ_NKDataGrouping = "ZA"
			});
			await entities.SaveChangesAsync();
			entities.RefCusCodeListAttributeNames.Add(new RefCusCodeListAttributeName
			{
				ZXE_PK = Guid.NewGuid(),
				ZXE_Name = "A",
				ZXE_ZZK_NKCodeType = "PKG",
				ZXE_ZZZ_NKDataGrouping = "ZA",
				ZXE_Description = "AA",
				ZXE_IsMandatory = false,
				ZXE_AllowDuplicates = false,
				ZXE_IsValueMandatory = false,
				ZXE_ValueDataType = string.Empty,
				ZXE_MinLengthOrValue = 0,
				ZXE_MaxLengthOrValue = 0,
				ZXE_DecimalPlaces = 0,
				ZXE_ColumnCaption = ""
			});
			await entities.SaveChangesAsync();
			entities.RefCusCodeLists.Add(new RefCusCodeList
			{
				ZZD_PK = codeList1PK,
				ZZD_ZZK_NKCodeType = "PKG",
				ZZD_Code = "PE1",
				ZZD_Description = "Pallet, modular, collars 80cms * 120cms ",
				ZZD_StartDate = new DateTime(2019, 01, 01),
				ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 0),
				ZZD_ZZZ_NKDataGrouping = "ZA"
			});
			entities.RefCusCodeLists.Add(new RefCusCodeList
			{
				ZZD_PK = codeList2PK,
				ZZD_ZZK_NKCodeType = "PKG",
				ZZD_Code = "PE2",
				ZZD_Description = "Pallet, modular, collars 80cms * 120cms ",
				ZZD_StartDate = new DateTime(2018, 01, 01),
				ZZD_EndDate = new DateTime(2019, 06, 06, 23, 59, 0),
				ZZD_ZZZ_NKDataGrouping = "ZA"
			});
			entities.RefCusCodeLists.Add(new RefCusCodeList
			{
				ZZD_PK = codeList3PK,
				ZZD_ZZK_NKCodeType = "PKG",
				ZZD_Code = "PE3",
				ZZD_Description = "Pallet, modular, collars 80cms * 120cms ",
				ZZD_StartDate = new DateTime(2018, 01, 01),
				ZZD_EndDate = new DateTime(2018, 12, 31, 23, 59, 0),
				ZZD_ZZZ_NKDataGrouping = "ZA"
			});
			await entities.SaveChangesAsync();

			var auth = new Mock<IAuthorizationHelper>();
			auth.Setup(x => x.IsAuthorized<RefCusCodeList>(It.IsAny<string>())).Returns(true);
			auth.Setup(x => x.IsAuthorized<RefCusCodeListAttribute>(It.IsAny<string>())).Returns(true);
			using (var repo = new ReferenceDataRepository(entities))
			{
				var controllerContext = SetupControllerContext(repo);
				var controller1 = new SafeDataController<RefCusCodeList>(auth.Object) { ControllerContext = controllerContext };
				var parameters = new ODataActionParameters
				{
					{ "IDs", new[] { codeList1PK, codeList2PK, codeList3PK } },
					{ "expiredTime", new DateTimeOffset(new DateTime(2019, 01, 02, 07, 08, 0)) },
					{ "isExpirable", true }
				};
				await controller1.BatchExpire(parameters);
			}
		}

		await using (var entities = new SafeDbContext(connectionString))
		{
			var codeList1 = entities.RefCusCodeLists.FirstOrDefault(x => x.ZZD_PK == codeList1PK);
			Assert.AreEqual(new DateTime(2019, 01, 02, 07, 08, 0), codeList1.ZZD_EndDate, "CodeList 1 should expire");

			var codeList2 = entities.RefCusCodeLists.FirstOrDefault(x => x.ZZD_PK == codeList2PK);
			Assert.AreEqual(new DateTime(2019, 01, 02, 07, 08, 0), codeList2.ZZD_EndDate, "CodeList 2 should expire");

			var codeList3 = entities.RefCusCodeLists.FirstOrDefault(x => x.ZZD_PK == codeList3PK);
			Assert.AreEqual(new DateTime(2018, 12, 31, 23, 59, 0), codeList3.ZZD_EndDate, "CodeList 3 should remain unchanged");
		}
	}

	[Test]
	[TransactionedTestCase]
	[Property("DAT:CapabilityRequirements", "SQL2019+")]
	public async Task DeleteOrExpireOnDataHeader()
	{
		var codeTypePK = Guid.NewGuid();
		var connectionString = GetConnectionString();
		await using (var entities = new SafeDbContext(connectionString))
		{
			entities.RefDataGroupings.Add(new RefDataGrouping
			{
				ZZZ_PK = Guid.NewGuid(),
				ZZZ_DataGrouping = "ZA",
				ZZZ_Description = "South Africa"
			});
			entities.RefCusCodeTypes.Add(new RefCusCodeType
			{
				ZZK_PK = codeTypePK,
				ZZK_CodeType = "PKG",
				ZZK_Description = "Package",
				ZZK_MaxLength = 0,
				ZZK_ZZZ_NKDataGrouping = "ZA"
			});
			await entities.SaveChangesAsync();

			var auth = new Mock<IAuthorizationHelper>();
			auth.Setup(x => x.IsAuthorized<RefCusCodeType>(It.IsAny<string>())).Returns(true);
			using (var repo = new ReferenceDataRepository(entities))
			{
				var controllerContext = SetupControllerContext(repo);
				var controller1 = new DataSetHeaderController<RefCusCodeType>(auth.Object) { ControllerContext = controllerContext };
				var parameters = new ODataActionParameters
				{
					{ "IDs", new[] { codeTypePK } }
				};
				await controller1.BatchDelete(parameters);
			}
		}

		await using (var entities = new SafeDbContext(connectionString))
		{
			var versionControl = entities.RefDbVersionControls.FirstOrDefault(x => x.RVC_ParentPK == codeTypePK);
			Assert.True(versionControl.RVC_Deleted);
		}
	}

	[Test]
	[TransactionedTestCase]
	[Property("DAT:CapabilityRequirements", "SQL2019+")]
	public async Task CloneExpiredRecordDependencyIntoNewRecord()
	{
		var expiredParentRecordPk = Guid.NewGuid();
		var newParentRecordPK = Guid.NewGuid();
		var connectionString = GetConnectionString();
		var languagePK = Guid.NewGuid();
		var languageToIgnorePk = Guid.NewGuid();
		var dataProcessingClonePK = Guid.NewGuid();
		Guid newClonedRecordPK;

		var languages = new RefCusCodeListLanguage[]
		{
			new RefCusCodeListLanguage
			{
				ZXA_Description = "OI",
				ZXA_PK = languagePK,
				ZXA_ZX6_NKLanguage = "EN",
				ZXA_ZZD_CodeList = expiredParentRecordPk
			},
			new RefCusCodeListLanguage
			{
				ZXA_Description = "OBA",
				ZXA_PK = languageToIgnorePk,
				ZXA_ZX6_NKLanguage = "PT",
				ZXA_ZZD_CodeList = expiredParentRecordPk
			}
		};

		var expiredRecord = new RefCusCodeList
		{
			ZZD_PK = expiredParentRecordPk,
			ZZD_ZZK_NKCodeType = "PKG",
			ZZD_Code = "PE1",
			ZZD_Description = "Pallet, modular, collars 80cms * 120cms",
			ZZD_StartDate = new DateTime(2019, 01, 01),
			ZZD_EndDate = new DateTime(2020, 06, 06, 23, 59, 0),
			ZZD_ZZZ_NKDataGrouping = "ZA"
		};

		await using (var entities = new SafeDbContext(connectionString))
		{
			entities.RefLanguageTypes.Add(new RefLanguageType
			{
				ZX6_Description = "English",
				ZX6_Language = "EN",
				ZX6_PK = Guid.NewGuid()
			});
			entities.RefLanguageTypes.Add(new RefLanguageType
			{
				ZX6_Description = "Portuguese",
				ZX6_Language = "PT",
				ZX6_PK = Guid.NewGuid()
			});
			entities.RefDataGroupings.Add(new RefDataGrouping
			{
				ZZZ_PK = Guid.NewGuid(),
				ZZZ_DataGrouping = "ZA",
				ZZZ_Description = "South Africar"
			});
			entities.RefCusCodeTypes.Add(new RefCusCodeType
			{
				ZZK_PK = Guid.NewGuid(),
				ZZK_CodeType = "PKG",
				ZZK_Description = "Package",
				ZZK_MaxLength = 0,
				ZZK_ZZZ_NKDataGrouping = "ZA"
			});
			await entities.SaveChangesAsync();
			entities.RefCusCodeListAttributeNames.Add(new RefCusCodeListAttributeName
			{
				ZXE_PK = Guid.NewGuid(),
				ZXE_Name = "A",
				ZXE_ZZK_NKCodeType = "PKG",
				ZXE_ZZZ_NKDataGrouping = "ZA",
				ZXE_Description = "AA",
				ZXE_IsMandatory = false,
				ZXE_AllowDuplicates = false,
				ZXE_IsValueMandatory = false,
				ZXE_ValueDataType = string.Empty,
				ZXE_MinLengthOrValue = 0,
				ZXE_MaxLengthOrValue = 0,
				ZXE_DecimalPlaces = 0,
				ZXE_ColumnCaption = ""
			});
			await entities.SaveChangesAsync();
			entities.RefCusCodeLists.Add(expiredRecord);
			var newParentRecord = new RefCusCodeList
			{
				ZZD_PK = newParentRecordPK,
				ZZD_ZZK_NKCodeType = "PKG",
				ZZD_Code = "PE2",
				ZZD_Description = "Pallet, modular, collars 80cms * 120cms",
				ZZD_StartDate = new DateTime(2020, 06, 07),
				ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 0),
				ZZD_ZZZ_NKDataGrouping = "ZA",
			};
			entities.RefCusCodeLists.Add(newParentRecord);
			entities.RefCusCodeListLanguages.AddRange(languages);
			expiredRecord.RefCusCodeListLanguages = languages;
			await entities.SaveChangesAsync();
		}

		await using (var entities = new SafeDbContext(connectionString))
		{
			var auth = new Mock<IAuthorizationHelper>();
			auth.Setup(x => x.IsAuthorized<RefCusCodeList>(It.IsAny<string>())).Returns(true);
			using (var repo = new ReferenceDataRepository(entities))
			{
				var controllerContext = SetupControllerContext(repo);
				var controller1 = new DataSetHeaderController<RefCusCodeList>(auth.Object) { ControllerContext = controllerContext };

				var exceptItems = new Dictionary<string, List<Guid>>
				{
					{ "ZXA", new List<Guid>() { languageToIgnorePk } }
				};
				var cloneProcessObject = new CloneProcessObject
				{
					DataProcessingClonePK = dataProcessingClonePK,
					ExpiredRecordPk = expiredRecord.ZZD_PK,
					NewRecordPk = newParentRecordPK,
					ExceptionListForCloning = exceptItems
				};
				var cloneProcessObjectsJson = JsonConvert.SerializeObject(new List<CloneProcessObject> { cloneProcessObject });
				var parms = new ODataActionParameters
				{
					{ "cloneProcessObjectsJson", cloneProcessObjectsJson }
				};
				var cloneResultsJson = controller1.CloneExistingRecordChildrenIntoNewRecord(parms);
				var cloneResults = JsonConvert.DeserializeObject<IEnumerable<CloneProcessResult>>(cloneResultsJson);
				Assert.AreEqual(1, cloneResults.Count());
				var clonedRecord = cloneResults.First();
				Assert.AreEqual("RefCusCodeListLanguage", clonedRecord.ClonedRecordTypeName);
				Assert.AreEqual(languagePK, clonedRecord.OriginalRecordPK);
				Assert.AreEqual(newParentRecordPK, clonedRecord.ClonedRecordExpirableAncestorPK);
				Assert.AreEqual(newParentRecordPK, clonedRecord.DataSetPK);
				newClonedRecordPK = clonedRecord.ClonedRecordPK;

				var jObject = clonedRecord.ClonedRecord as JObject;
				var language = jObject.ToObject<RefCusCodeListLanguage>();
				Assert.NotNull(language);
				Assert.That(language.ZXA_ZX6_NKLanguage, Is.EqualTo("EN"));
				Assert.AreEqual(language.ZXA_PK, newClonedRecordPK);
			}
		}
	}

	[Test]
	[TransactionedTestCase]
	[Property("DAT:CapabilityRequirements", "SQL2019+")]
	public async Task CloneExpiredRecordDependencyIntoNewRecordMoreLevels()
	{
		var tariffTypePk = Guid.NewGuid();
		var expiredParentRecordPk = Guid.NewGuid();
		var newParentRecordPK = Guid.NewGuid();
		var rateCodePk = Guid.NewGuid();
		var rate1Pk = Guid.NewGuid();
		var rate2Pk = Guid.NewGuid();
		var applicability1Pk = Guid.NewGuid();
		var excludedTradeGroupPK = Guid.NewGuid();
		var connectionString = GetConnectionString();
		var rateTypePk = Guid.NewGuid();
		var dataProcessingClonePK = Guid.NewGuid();

		var rateType = new RefCusRateType
		{
			ZZR_PK = rateTypePk,
			ZZR_CustomsValueFormula = "",
			ZZR_Description = "Desc",
			ZZR_IsPayable = true,
			ZZR_RateType = "RT",
			ZZR_ZZZ_NKDataGrouping = "ZA",
			ZZR_RX_NKFormulaCurrency = ""
		};

		var tariffType = new RefCusTariffType
		{
			ZZI_Description = "type",
			ZZI_PK = tariffTypePk,
			ZZI_TariffType = "TA",
			ZZI_ZZZ_NKDataGrouping = "ZA",
			ZZI_HasFormulaSpecificQuestions = true,
			ZZI_ZZR_RateType = rateTypePk,
			ZZI_ZZ9_NKNomenclatureGroupType = ""
		};

		var tariff = new RefCusTariff
		{
			ZZ1_PK = expiredParentRecordPk,
			ZZ1_Description = "To Be Cloned",
			ZZ1_TariffCode = "0101",
			ZZ1_ZZZ_NKDataGrouping = "ZA",
			ZZ1_StartDate = new DateTime(2019, 06, 07),
			ZZ1_EndDate = new DateTime(2020, 06, 06, 23, 59, 0),
			ZZ1_IAMUnique = 1,
			ZZ1_CompositeKeyOnZZ5 = "0101",
			ZZ1_ZZI_TariffType = tariffTypePk,
			ZZ1_ZZF_NKTaxOrFeeCode = ""
		};

		var rateCode = new RefCusRateCode
		{
			ZY1_PK = rateCodePk,
			ZY1_Description = "description",
			ZY1_RateCode = "RC",
			ZY1_ZZZ_NKDataGrouping = "ZA",
			ZY1_InternalUse = true,
			ZY1_ZZR_RateType = rateTypePk,
		};
		var rate1 = new RefCusRate
		{
			ZZ2_PK = rate1Pk,
			ZZ2_ZZ1_Tariff = tariff.ZZ1_PK,
			ZZ2_ZZZ_NKDataGrouping = "ZA",
			ZZ2_DataSetCode = "ZZ1",
			ZZ2_DataSetPK = tariff.ZZ1_PK,
			ZZ2_RateFormula = "1",
			ZZ2_StartDate = DateTime.Now,
			ZZ2_EndDate = DateTime.Now.AddDays(2),
			ZZ2_RateFormulaDerivedFrom = "",
			ZZ2_SelectorFormula = "",
			ZZ2_ZY1_RateCode = rateCodePk,
			ZZ2_RX_NKCurrencyOverride = ""
		};
		var rate2 = new RefCusRate
		{
			ZZ2_PK = rate2Pk,
			ZZ2_ZZ1_Tariff = tariff.ZZ1_PK,
			ZZ2_ZZZ_NKDataGrouping = "ZA",
			ZZ2_DataSetCode = "ZZ1",
			ZZ2_DataSetPK = tariff.ZZ1_PK,
			ZZ2_RateFormula = "0",
			ZZ2_StartDate = DateTime.Now,
			ZZ2_EndDate = DateTime.Now.AddDays(2),
			ZZ2_RateFormulaDerivedFrom = "",
			ZZ2_SelectorFormula = "",
			ZZ2_ZY1_RateCode = rateCodePk,
			ZZ2_RX_NKCurrencyOverride = ""
		};

		var tradeGroup = new RefCusTradeGroup
		{
			ZZA_Description = "Desc",
			ZZA_PK = Guid.NewGuid(),
			ZZA_StartDate = DateTime.Now,
			ZZA_EndDate = DateTime.Now.AddDays(2),
			ZZA_TradeGroup = "TG",
			ZZA_ZZZ_NKDataGrouping = "ZA"
		};

		var applicability1 = new RefCusApplicability
		{
			ZZT_AdditionalCode = "AC1",
			ZZT_PK = applicability1Pk,
			ZZT_ZZ2_Rate = rate1Pk,
			ZZT_DataSetCode = "ZZ1",
			ZZT_DataSetPK = tariff.ZZ1_PK,
			ZZT_OrderNumber = "",
			ZZT_StartDate = DateTime.Now,
			ZZT_EndDate = DateTime.Now.AddDays(2),
			ZZT_ZZA_TradeGroup = tradeGroup.ZZA_PK
		};

		var excludedTradeGroup = new RefCusExcludedTradeGroup
		{
			ZZC_DataSetCode = "ZZ1",
			ZZC_DataSetPK = tariff.ZZ1_PK,
			ZZC_PK = excludedTradeGroupPK,
			ZZC_ZZT_Applicability = applicability1.ZZT_PK,
			ZZC_ZZA_TradeGroup = tradeGroup.ZZA_PK
		};

		await using (var entities = new SafeDbContext(connectionString))
		{
			entities.RefDataGroupings.Add(new RefDataGrouping
			{
				ZZZ_PK = Guid.NewGuid(),
				ZZZ_DataGrouping = "ZA",
				ZZZ_Description = "South Africa"
			});
			await entities.SaveChangesAsync();

			entities.RefCusRateTypes.Add(rateType);
			entities.RefCusTariffTypes.Add(tariffType);
			entities.RefCusRateCodes.Add(rateCode);
			entities.RefCusTradeGroups.Add(tradeGroup);
			await entities.SaveChangesAsync();

			entities.RefCusTariffs.Add(tariff);
			var newParentRecord = new RefCusTariff
			{
				ZZ1_PK = newParentRecordPK,
				ZZ1_Description = "New Tariff",
				ZZ1_TariffCode = "0101",
				ZZ1_StartDate = new DateTime(2020, 06, 07),
				ZZ1_EndDate = new DateTime(2079, 06, 06, 23, 59, 0),
				ZZ1_ZZZ_NKDataGrouping = "ZA",
				ZZ1_IAMUnique = 1,
				ZZ1_CompositeKeyOnZZ5 = "0101",
				ZZ1_ZZI_TariffType = tariffTypePk,
				ZZ1_ZZF_NKTaxOrFeeCode = ""
			};
			entities.RefCusTariffs.Add(newParentRecord);
			tariff.RefCusRates = new[] { rate1, rate2 };
			entities.RefCusRates.Add(rate1);
			entities.RefCusRates.Add(rate2);

			rate1.RefCusApplicabilities = new[] { applicability1 };
			entities.RefCusApplicabilities.Add(applicability1);
			entities.RefCusExcludedTradeGroups.Add(excludedTradeGroup);
			await entities.SaveChangesAsync();
		}

		IEnumerable<CloneProcessResult> cloneResults = null;
		await using (var entities = new SafeDbContext(connectionString))
		{
			var auth = new Mock<IAuthorizationHelper>();
			auth.Setup(x => x.IsAuthorized<RefCusTariff>(It.IsAny<string>())).Returns(true);
			using (var repo = new ReferenceDataRepository(entities))
			{
				var controllerContext = SetupControllerContext(repo);
				var controller1 = new DataSetHeaderController<RefCusTariff>(auth.Object) { ControllerContext = controllerContext };
				var exceptItems = new Dictionary<string, List<Guid>>
				{
					{ "ZZ2", [rate2Pk] }
				};
				var cloneProcessObject = new CloneProcessObject
				{
					DataProcessingClonePK = dataProcessingClonePK,
					ExpiredRecordPk = tariff.ZZ1_PK,
					NewRecordPk = newParentRecordPK,
					ExceptionListForCloning = exceptItems
				};
				var cloneProcessObjectsJson = JsonConvert.SerializeObject(new List<CloneProcessObject> { cloneProcessObject });
				var parms = new ODataActionParameters
				{
					{ "cloneProcessObjectsJson", cloneProcessObjectsJson }
				};
				var cloneResultsJson = controller1.CloneExistingRecordChildrenIntoNewRecord(parms);
				cloneResults = JsonConvert.DeserializeObject<IEnumerable<CloneProcessResult>>(cloneResultsJson);
				Assert.AreEqual(3, cloneResults.Count());
				CollectionAssert.AreEquivalent(new[] { "RefCusRate", "RefCusApplicability", "RefCusExcludedTradeGroup" }, cloneResults.Select(x => x.ClonedRecordTypeName));
				Assert.True(cloneResults.All(x => x.DataSetPK == newParentRecordPK));

				var clonedRate = cloneResults.First(x => x.ClonedRecordTypeName == "RefCusRate");
				var jObject = clonedRate.ClonedRecord as JObject;
				var rateResult = jObject.ToObject<RefCusRate>();
				Assert.NotNull(rateResult);
				Assert.That(rateResult.ZZ2_RateFormula, Is.EqualTo("1"));
				Assert.AreEqual(rate1Pk, clonedRate.OriginalRecordPK);
				Assert.AreEqual(rateResult.ZZ2_PK, clonedRate.ClonedRecordPK);
				Assert.AreEqual(newParentRecordPK, clonedRate.ClonedRecordExpirableAncestorPK);

				var clonedApplicability = cloneResults.First(x => x.ClonedRecordTypeName == "RefCusApplicability");
				jObject = clonedApplicability.ClonedRecord as JObject;
				var appResult = jObject.ToObject<RefCusApplicability>();
				Assert.NotNull(appResult);
				Assert.That(appResult.ZZT_AdditionalCode, Is.EqualTo("AC1"));
				Assert.AreEqual(applicability1Pk, clonedApplicability.OriginalRecordPK);
				Assert.AreEqual(appResult.ZZT_PK, clonedApplicability.ClonedRecordPK);
				Assert.AreEqual(rateResult.ZZ2_PK, clonedApplicability.ClonedRecordExpirableAncestorPK);

				var clonedExcludedTradeGroup = cloneResults.First(x => x.ClonedRecordTypeName == "RefCusExcludedTradeGroup");
				jObject = clonedExcludedTradeGroup.ClonedRecord as JObject;
				var excludedTradeGroupResult = jObject.ToObject<RefCusExcludedTradeGroup>();
				Assert.NotNull(excludedTradeGroupResult);
				Assert.AreEqual(excludedTradeGroup.ZZC_ZZA_TradeGroup, excludedTradeGroupResult.ZZC_ZZA_TradeGroup);
				Assert.AreEqual(excludedTradeGroupPK, clonedExcludedTradeGroup.OriginalRecordPK);
				Assert.AreEqual(excludedTradeGroupResult.ZZC_PK, clonedExcludedTradeGroup.ClonedRecordPK);
				Assert.AreEqual(appResult.ZZT_PK, clonedExcludedTradeGroup.ClonedRecordExpirableAncestorPK);
			}
		}
	}

	[Test]
	[TransactionedTestCase]
	[Property("DAT:CapabilityRequirements", "SQL2019+")]
	public async Task DeleteOrExpireWithIsActiveColumn()
	{
		var complianceListPK = Guid.NewGuid();
		var connectionString = GetConnectionString();
		await using (var entities = new SafeDbContext(connectionString))
		{
			entities.RefComplianceLists.Add(new RefComplianceList
			{
				RCL_PK = complianceListPK,
				RCL_IsActive = true,
				RCL_ListCode = "AA",
				RCL_ListName = "Name1",
				RCL_ListType = "Type1",
				RCL_ListDescription = "AAA",
				RCL_ListPublisher = "Publisher1",
				RCL_PublisherDescription = "BBB",
				RCL_PublisherJurisdiction = "AAA",
				RCL_MainSourceURL = "AAA",
				RCL_SecondarySourceURL = "BBB"
			});
			await entities.SaveChangesAsync();

			var version = entities.RefDbVersionControls.FirstOrDefault(x => x.RVC_ParentPK == complianceListPK);
			version.RVC_Deleted = false;
			entities.RefDbVersionControls.Attach(version);
			entities.Entry(version).State = EntityState.Modified;
			await entities.SaveChangesAsync();

			var auth = new Mock<IAuthorizationHelper>();
			auth.Setup(x => x.IsAuthorized<RefComplianceList>(It.IsAny<string>())).Returns(true);
			using (var repo = new ReferenceDataRepository(entities))
			{
				var controllerContext = SetupControllerContext(repo);
				var controller = new SafeDataController<RefComplianceList>(auth.Object) { ControllerContext = controllerContext };
				var parms = new ODataActionParameters
				{
					{ "IDs", new[] { complianceListPK } },
				};
				await controller.BatchInActive(parms);
			}
		}

		await using (var entities = new SafeDbContext(connectionString))
		{
			var complianceList = entities.RefComplianceLists.First(x => x.RCL_PK == complianceListPK);
			Assert.False(complianceList.RCL_IsActive);
			var version = entities.RefDbVersionControls.First(x => x.RVC_ParentPK == complianceListPK);
			Assert.False(version.RVC_Deleted);
		}
	}

	[Test]
	[TransactionedTestCase]
	[Property("DAT:CapabilityRequirements", "SQL2019+")]
	public async Task DeleteOrExpireWithUserOverrideColumn()
	{
		var unlocoIsUserOverridePK = Guid.NewGuid();
		var unlocoPK = Guid.NewGuid();
		var connectionString = GetConnectionString();
		await using (var entities = new SafeDbContext(connectionString))
		{
			entities.RefUNLOCOs.Add(new RefUNLOCO
			{
				RL_PK = unlocoIsUserOverridePK,
				RL_Code = "TESTT",
				RL_GeoLocation = new Point(10, 20) { SRID = 4326 },
				RL_PortName = "Test",
				RL_NameWithDiacriticals = "Test",
				RL_IATA = "TST",
				RL_IATARegionCode = "TST",
				RL_CoOrdinates = "12 E 12 N",
				RL_RN_NKCountryCode = "AU",
				RL_IsActive = true,
				RL_UserOverride = true
			});
			entities.RefUNLOCOs.Add(new RefUNLOCO
			{
				RL_PK = unlocoPK,
				RL_Code = "TESTS",
				RL_GeoLocation = new Point(20, 20) { SRID = 4326 },
				RL_PortName = "Tess",
				RL_NameWithDiacriticals = "Tess",
				RL_IATA = "TSS",
				RL_IATARegionCode = "TSS",
				RL_CoOrdinates = "22 E 12 N",
				RL_RN_NKCountryCode = "AU",
				RL_IsActive = true,
				RL_UserOverride = false
			});
			await entities.SaveChangesAsync();
			var versionIsUserOverride = entities.RefDbVersionControls.FirstOrDefault(x => x.RVC_ParentPK == unlocoIsUserOverridePK);
			versionIsUserOverride.RVC_Deleted = false;
			var version = entities.RefDbVersionControls.FirstOrDefault(x => x.RVC_ParentPK == unlocoPK);
			version.RVC_Deleted = false;
			entities.RefDbVersionControls.Attach(version);
			entities.Entry(version).State = EntityState.Modified;
			await entities.SaveChangesAsync();

			var auth = new Mock<IAuthorizationHelper>();
			auth.Setup(x => x.IsAuthorized<RefUNLOCO>(It.IsAny<string>())).Returns(true);
			using (var repo = new ReferenceDataRepository(entities))
			{
				var controllerContext = SetupControllerContext(repo);
				var controller = new SafeDataController<RefUNLOCO>(auth.Object) { ControllerContext = controllerContext };
				var parms = new ODataActionParameters
				{
					{ "IDs", new[] { unlocoIsUserOverridePK, unlocoPK } }
				};
				await controller.BatchInActive(parms);
			}
		}

		await using (var entities = new SafeDbContext(connectionString))
		{
			var unlocoIsUserOverride = entities.RefUNLOCOs.First(x => x.RL_PK == unlocoIsUserOverridePK);
			Assert.AreEqual("TESTT", unlocoIsUserOverride.RL_Code);
			Assert.AreEqual("Test", unlocoIsUserOverride.RL_PortName);
			Assert.AreEqual("TST", unlocoIsUserOverride.RL_IATA);
			Assert.True(unlocoIsUserOverride.RL_IsActive);
			var versionIsUserOverride = entities.RefDbVersionControls.First(x => x.RVC_ParentPK == unlocoIsUserOverridePK);
			Assert.False(versionIsUserOverride.RVC_Deleted);
			var unloco = entities.RefUNLOCOs.First(x => x.RL_PK == unlocoPK);
			Assert.AreEqual("TESTS", unloco.RL_Code);
			Assert.AreEqual("Tess", unloco.RL_PortName);
			Assert.AreEqual("TSS", unloco.RL_IATA);
			Assert.False(unloco.RL_IsActive);
			var version = entities.RefDbVersionControls.First(x => x.RVC_ParentPK == unlocoPK);
			Assert.False(version.RVC_Deleted);
		}
	}

	[Test]
	[TransactionedTestCase]
	[Property("DAT:CapabilityRequirements", "SQL2019+")]
	public async Task Delete()
	{
		var codeTypePK = Guid.NewGuid();
		var codeListPK = Guid.NewGuid();
		var attributePK = Guid.NewGuid();
		var connectionString = GetConnectionString();
		await using (var entities = new SafeDbContext(connectionString))
		{
			entities.RefDataGroupings.Add(new RefDataGrouping
			{
				ZZZ_PK = Guid.NewGuid(),
				ZZZ_DataGrouping = "ZA",
				ZZZ_Description = "South Africa"
			});
			entities.RefCusCodeTypes.Add(new RefCusCodeType
			{
				ZZK_PK = codeTypePK,
				ZZK_CodeType = "PKG",
				ZZK_Description = "Package",
				ZZK_MaxLength = 0,
				ZZK_ZZZ_NKDataGrouping = "ZA"
			});
			await entities.SaveChangesAsync();

			var auth = new Mock<IAuthorizationHelper>();
			auth.Setup(x => x.IsAuthorized(It.IsAny<RefCusCodeType>(), It.IsAny<string>())).Returns(true);

			using (var repo = new ReferenceDataRepository(entities))
			{
				var controllerContext = SetupControllerContext(repo);
				var controller = new DataSetHeaderController<RefCusCodeType>(auth.Object) { ControllerContext = controllerContext };
				await controller.Delete(codeTypePK);
			}
		}

		await using (var entities = new SafeDbContext(connectionString))
		{
			var codeType = entities.RefCusCodeTypes.FirstOrDefault(x => x.ZZK_PK == codeTypePK);
			Assert.NotNull(codeType);
			var versionControl = entities.RefDbVersionControls.FirstOrDefault(x => x.RVC_ParentPK == codeTypePK);
			Assert.True(versionControl.RVC_Deleted);
		}
	}

	[Test]
	[TransactionedTestCase]
	[Property("DAT:CapabilityRequirements", "SQL2019+")]
	public async Task ForceDelete()
	{
		var airlinePK1 = Guid.NewGuid();
		var airlinePK2 = Guid.NewGuid();
		var connectionString = GetConnectionString();
		await using (var entities = new SafeDbContext(connectionString))
		{
			var airline1 = new RefAirline();
			var airline2 = new RefAirline();
			foreach (var property in typeof(RefAirline).GetProperties())
			{
				if (property.PropertyType == typeof(string))
				{
					property.SetValue(airline1, "");
					property.SetValue(airline2, "");
				}
			}
			airline1.RM_PK = airlinePK1;
			airline1.RM_EagleAddedAirlinePrefixOrAccountingCode = "001";
			airline1.RM_ThreeLetterCode = "AA1";
			airline1.RM_AirlineName1 = "Name1";
			airline2.RM_PK = airlinePK2;
			entities.RefAirlines.Add(airline1);
			entities.RefAirlines.Add(airline2);
			await entities.SaveChangesAsync();

			var auth = new Mock<IAuthorizationHelper>();
			auth.Setup(x => x.IsAuthorized<RefAirline>(It.IsAny<string>())).Returns(true);
			using (var repo = new ReferenceDataRepository(entities))
			{
				var controllerContext = SetupControllerContext(repo);
				var controller = new DataSetHeaderController<RefAirline>(auth.Object) { ControllerContext = controllerContext };
				var parms = new ODataActionParameters
				{
					{ "IDs", new[] { airlinePK1 } }
				};
				await controller.ForceDelete(parms);
			}
		}

		await using (var entities = new SafeDbContext(connectionString))
		{
			var refAirline = entities.RefAirlines.FirstOrDefault(x => x.RM_PK == airlinePK1);
			Assert.Null(refAirline);
			var version = entities.RefDbVersionControls.FirstOrDefault(x => x.RVC_ParentPK == airlinePK1);
			Assert.Null(version);
			refAirline = entities.RefAirlines.FirstOrDefault(x => x.RM_PK == airlinePK2);
			Assert.NotNull(refAirline);
			version = entities.RefDbVersionControls.FirstOrDefault(x => x.RVC_ParentPK == airlinePK2);
			Assert.NotNull(version);
		}
	}

	string GetConnectionString()
	{
		var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
		var connectionString = TestConnectionString.GetAdmin(dbName);
		return connectionString;
	}

	internal static ControllerContext SetupControllerContext(IReferenceDataRepository repo, HttpRequest request = null)
	{
		var controllerContext = new ControllerContext();
		var httpContext = new Mock<HttpContext>();
		object sharedRepo = Tuple.Create(repo, false);
		httpContext.Setup(x => x.Items.TryGetValue("Batch_DbContext", out sharedRepo)).Returns(true);
		if (request != null)
		{
			httpContext.Setup(x => x.Request).Returns(request);
		}
		controllerContext.HttpContext = httpContext.Object;
		return controllerContext;
	}
}
