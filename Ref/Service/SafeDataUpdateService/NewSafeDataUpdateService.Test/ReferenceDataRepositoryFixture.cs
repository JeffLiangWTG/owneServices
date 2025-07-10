using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Moq;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService.Test
{
	[TestFixture]
	[CreateDatabase("A682BF7C1FB74F9EB84F86FB5D47AB88", DbSchema.RefDbRepoSafe)]
	[Property("DAT:CapabilityRequirements", "SQL2019+")]
	public class ReferenceDataRepositoryFixture
	{
		[Test]
		public async Task BulkInsertBeforeUpdateCauseContraintsViolation()
		{
			var connectionString = GetConnectionString();
			using (var entities = new ReferenceDataRepository(false, connectionString))
			{
				if (!entities.Get<RefDataGrouping>().Any(x => x.ZZZ_DataGrouping == "AU"))
				{
					entities.Add(new RefDataGrouping
					{
						ZZZ_PK = Guid.NewGuid(),
						ZZZ_DataGrouping = "AU",
						ZZZ_Description = "AU"
					});
				}
				entities.Add(new RefCusTaxOrFeeType
				{
					ZX0_PK = Guid.NewGuid(),
					ZX0_TaxOrFeeType = "TYP",
					ZX0_Description = "TYP"
				});
				var tax = new RefCusTaxOrFee
				{
					ZZF_PK = Guid.NewGuid(),
					ZZF_ZX0_NKTaxOrFeeType = "TYP",
					ZZF_Code = "COD",
					ZZF_Description = "DESC",
					ZZF_Maximum = 1,
					ZZF_Minimum = 0,
					ZZF_Value = 20,
					ZZF_StartDate = new DateTime(1900, 01, 01),
					ZZF_EndDate = new DateTime(2079, 06, 06),
					ZZF_ZZZ_NKDataGrouping = "AU"
				};
				entities.Add(tax);
				await entities.SaveChangesAsync(null, true);
				var newTax = new RefCusTaxOrFee
				{
					ZZF_PK = Guid.NewGuid(),
					ZZF_ZX0_NKTaxOrFeeType = "TYP",
					ZZF_Code = "COD",
					ZZF_Description = "DESC",
					ZZF_Maximum = 1,
					ZZF_Minimum = 0,
					ZZF_Value = 20,
					ZZF_StartDate = new DateTime(2020, 01, 01),
					ZZF_EndDate = new DateTime(2079, 06, 06),
					ZZF_ZZZ_NKDataGrouping = "AU"
				};
				tax.ZZF_EndDate = new DateTime(2019, 12, 31);
				entities.Update(tax);
				entities.Add(newTax);
				Assert.DoesNotThrow(() => entities.SaveChangesAsync(null, true).Wait());
			}
		}

		[TestCaseSource(nameof(ViewEntityTestCases))]
		public async Task UpdateLastEditedUser_WhenUpdateView(Action<ReferenceDataRepository> prepareDependencies, Func<ReferenceDataRepository, object> createView, Action<object> updateAction, Func<object, Guid> getPk)
		{
			var connectionString = GetConnectionString();
			using var entities = new ReferenceDataRepository(false, connectionString);
			prepareDependencies(entities);
			await entities.SaveChangesAsync(null);

			var viewEntity = createView(entities);
			entities.Add(viewEntity);
			await entities.SaveChangesAsync("USER1", true);
			var pk = getPk(viewEntity);
			var versionControl = entities.Get<RefDbVersionControl>().FirstOrDefault(x => x.RVC_ParentPK == pk);
			Assert.That(versionControl?.RVC_LastEditedUser, Is.EqualTo("USER1"));

			updateAction(viewEntity);
			entities.Update(viewEntity);
			await entities.SaveChangesAsync("USER2", true);
			versionControl = entities.Get<RefDbVersionControl>().FirstOrDefault(x => x.RVC_ParentPK == pk);
			Assert.That(versionControl?.RVC_LastEditedUser, Is.EqualTo("USER2"));
		}

		static IEnumerable ViewEntityTestCases
		{
			get
			{
				yield return new TestCaseData(
					new Action<ReferenceDataRepository>(entities => { }),
					new Func<ReferenceDataRepository, object>(entities => new RefAccTaxRateUserView
					{
						ZAT_PK = Guid.NewGuid(),
						ZAT_RN_NKCountry = "CR",
						ZAT_ReferenceRateType = "LOW3",
						ZAT_StartDate = new DateTime(1900, 01, 01),
						ZAT_EndDate = new DateTime(2079, 06, 06, 23, 59, 0),
						ZAT_RateNumerator = 19,
						ZAT_RateDenominator = 1,
					}),
					new Action<object>(entity => ((RefAccTaxRateUserView)entity).ZAT_RateNumerator = 29),
					new Func<object, Guid>(entity => ((RefAccTaxRateUserView)entity).ZAT_PK)
					).SetName("RefAccTaxRateUserView");

				yield return new TestCaseData(
					new Action<ReferenceDataRepository>(entities => { }),
					new Func<ReferenceDataRepository, object>(entities => new RefStlScriptUserView
					{
						STL_PK = Guid.NewGuid(),
						STL_FeatureCode = "ULC",
						STL_RoleName = "Customs & Country Specific Integrations",
						STL_ModuleName = "Other Parties Customs Messaging (non Broker/Agent)",
						STL_FunctionName = "Ecommerce Section 321 Type 86",
						STL_FeatureName = "Ecommerce - Customs Section 321",
						STL_DataGranularity = "TRN",
						STL_CompanyCode = "gc.GC_Code",
						STL_BranchCode = "gb.GB_Code",
						STL_TransactionDateUtc = "ce.CE_SystemCreateTimeUtc",
						STL_CreatingUserCode = "ce.CE_SystemCreateUser",
						STL_GuidReference = "ulb.ULB_PK",
						STL_BillingReference1 = "ulh.ULH_JobNumber",
						STL_BillingReference2 = "ce.CE_EntryNum",
						STL_BillingReference3 = "ulb.ULB_HouseBill",
						STL_BillingReference4 = "ulh.ULH_UseCode",
						STL_AdditionalRefs = "",
						STL_TransactionCount = "1",
						STL_PreparationScript = "",
						STL_FromClause = "CusUSLVConsignment ulb INNER JOIN CusUSLVClearance ulh ON ulh.ULH_PK = ulb.ULB_ULH INNER JOIN CusEntryNum ce ON ce.CE_ParentID = ulb.ULB_PK AND ce.CE_EntryType = 'ENS' AND ce.CE_RN_NKCountryCode = 'US' AND ce.CE_Category = 'CUS' INNER JOIN GlbBranch gb ON gb.GB_PK = ulh.ULH_GB INNER JOIN GlbCompany gc ON gc.GC_PK = gb.GB_GC",
						STL_WhereClause = "ulh.ULH_UseCode <> 'HVL' AND ce.CE_EntryNum <> ''",
						STL_ActiveOn = "ALL",
						STL_MinCW1Version = "",
						STL_MaxCW1Version = "",
						STL_DateType = "DTE",
					}),
					new Action<object>(entity => ((RefStlScriptUserView)entity).STL_TransactionCount = "2"),
					new Func<object, Guid>(entity => ((RefStlScriptUserView)entity).STL_PK)
					).SetName("RefStlScriptUserView");

				yield return new TestCaseData(
					new Action<ReferenceDataRepository>(entities => { }),
					new Func<ReferenceDataRepository, object>(entities => new RefShippingLineUserView
					{
						RSL_PK = Guid.NewGuid(),
						RSL_IsActive = true,
						RSL_IsNVO = false,
						RSL_CarrierName = "aaaa",
						RSL_StandardCarrierAlphaCode = "bbbb",
						RSL_CargoWiseOneCode = "cccc",
						RSL_OceanCarrierMessagingAvailable = false,
						RSL_GlobalSailingScheduleAvailable = false,
						RSL_ContainerAutomationAvailable = false,
						RSL_CargoSphereRatesAvailable = false,
						RSL_InvoiceAvailable = false,
						RSL_IsSystem = true,
						RSL_IsPublished = true,
						RSL_IsCW1User = true,
						RSL_EHubIds = "1234",
						RSL_BookingRequestAvailable = false,
						RSL_ShippingInstructionAvailable = false,
						RSL_VerifiedGrossContainerWeightAvailable = false,
						RSL_ShippingOrderAvailable = false,
						RSL_EManifestAvailable = false,
						RSL_IsShippingLine = true,
						RSL_IsEditable = true,
					}),
					new Action<object>(entity => ((RefShippingLineUserView)entity).RSL_CarrierName = "dddd"),
					new Func<object, Guid>(entity => ((RefShippingLineUserView)entity).RSL_PK)
					).SetName("RefShippingLineUserView");

				yield return new TestCaseData(
					new Action<ReferenceDataRepository>(entities => {
						entities.Add(new RefCusCodeType
						{
							ZZK_PK = Guid.NewGuid(),
							ZZK_CodeType = "USFPC",
							ZZK_Description = "Package",
							ZZK_MaxLength = 0,
							ZZK_ZZZ_NKDataGrouping = "CN",
							ZZK_IsReadonly = true,
						});
						entities.Add(new RefDataGrouping
						{
							ZZZ_PK = Guid.NewGuid(),
							ZZZ_DataGrouping = "CN",
							ZZZ_Description = "South Africar",
						});
					}),
					new Func<ReferenceDataRepository, object>(entities => new RefCusCodeListUserView
					{
						ZZD_PK = Guid.NewGuid(),
						ZZD_CodeType = "USFPC", // [RefCusCodeType].[ZZK_CodeType]
						ZZD_Code = "PE2",
						ZZD_CountryOrGrouping =	"CN", // [RefCusCodeType].[ZZK_ZZZ_NKDataGrouping]
						ZZD_Description = "Pallet, modular, collars 80cms * 120cms",
						ZZD_StartDate = new DateTime(2020, 06, 07),
						ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 0),
					}),
					new Action<object>(entity => ((RefCusCodeListUserView)entity).ZZD_Description = "Pallet, modular, collars 90cms * 130cms"),
					new Func<object, Guid>(entity => ((RefCusCodeListUserView)entity).ZZD_PK)
					).SetName("RefCusCodeListUserView");

				yield return new TestCaseData(
					new Action<ReferenceDataRepository>(entities =>
					{
						entities.Add(new RefDataGrouping
						{
							ZZZ_PK = Guid.NewGuid(),
							ZZZ_DataGrouping = "TK",
							ZZZ_Description = "South Africar",
						});
					}),
					new Func<ReferenceDataRepository, object>(entities => new RefCusProcedureUserView
					{
						ZZ6_PK = Guid.NewGuid(),
						ZZ6_Category = "IM",
						ZZ6_ProcedureCode = "40",
						ZZ6_PreviousProcedureCode = "78",
						ZZ6_Concession = "309",
						ZZ6_Description = "Importation of goods for the purpose of repair, alteration, modification or refurbishment",
						ZZ6_CountryOrGrouping = "TK",
						ZZ6_ShipmentType = "IMP",
						ZZ6_CalculateDuty = true,
						ZZ6_Group = "51P",
						ZZ6_LandedCost = false,
						ZZ6_IntoWarehouse = "N",
						ZZ6_OutOfWarehouse = "N",
						ZZ6_IntoInwardProcessing = "Y",
						ZZ6_OutOfInwardProcessing = "N",
						ZZ6_IntoOutwardProcessing = "N",
						ZZ6_OutofOutwardProcessing = "N",
						ZZ6_IntoTemporaryImport = "N",
						ZZ6_OutOfTemporaryImport = "N",
						ZZ6_IntoTemporaryExport = "N",
						ZZ6_OutOfTemporaryExport = "N",
						ZZ6_StartDate = new DateTime(2020, 06, 07),
						ZZ6_EndDate = new DateTime(2079, 06, 06, 23, 59, 0),
						ZZ6_CalculateVAT = true,
						ZZ6_IsGuaranteeConsumed = "Y",
						ZZ6_IsGuaranteeReleased = "N",
						ZZ6_IsTransit = "N",
					}),
					new Action<object>(entity => ((RefCusProcedureUserView)entity).ZZ6_Category = "PRO"),
					new Func<object, Guid>(entity => ((RefCusProcedureUserView)entity).ZZ6_PK)
					).SetName("RefCusProcedureUserView");
			}
		}

		[Test]
		public async Task SystemVersion()
		{
			DataSetChangeHistory history = null;
			var zatPK = Guid.NewGuid();
			var connectionString = GetConnectionString();
			using (var entities = new ReferenceDataRepository(false, connectionString))
			{
				var accTaxRate = new RefAccTaxRateUserView
				{
					ZAT_PK = zatPK,
					ZAT_RN_NKCountry = "PY",
					ZAT_ReferenceRateType = "STD",
					ZAT_StartDate = new DateTime(1900, 01, 01),
					ZAT_EndDate = new DateTime(2079, 06, 06, 23, 59, 0),
					ZAT_RateNumerator = 19,
					ZAT_RateDenominator = 1
				};
				entities.Add(accTaxRate);
				await entities.SaveChangesAsync(null);
				history = entities.Get<DataSetChangeHistory>().FirstOrDefault(x => x.DCH_ParentPK == zatPK && x.DCH_ParentCode == "ZAT");
				var taxRate = entities.Get<RefAccTaxRate>().FirstOrDefault(x => x.ZAT_PK == zatPK);
				var sysStartTime = entities.Entry(taxRate).Property<DateTime>("ZAT_SysStartTime").CurrentValue;
				Assert.GreaterOrEqual(history.DCH_ChangeTime, sysStartTime);
				accTaxRate.ZAT_RateNumerator = 29;
				entities.Update(accTaxRate);
				await entities.SaveChangesAsync(null);
			}

			var systemVersionContext = new SystemVersionContext();
			systemVersionContext.SystemVersionUTC = history.DCH_ChangeTime.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ", CultureInfo.InvariantCulture);
			using (var entities = new ReferenceDataRepository(false, connectionString, new SystemVersionInterceptor(systemVersionContext)))
			{
				try
				{
					Assert.AreEqual(19, entities.Get<RefAccTaxRateUserView>().FirstOrDefault(x => x.ZAT_PK == zatPK).ZAT_RateNumerator);
				}
				catch (Exception)
				{
					Assert.AreEqual(history.DCH_ChangeTime.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ", CultureInfo.InvariantCulture), systemVersionContext.SystemVersionUTC);
					throw;
				}
			}
		}

		[TestCaseSource(nameof(BulkInsertTestCase))]
		public async Task BulkInsert(Action<ReferenceDataRepository> setup, Action<ReferenceDataRepository> assertion, Action<Exception> exceptionAssertion)
		{
			var connectionString = GetConnectionString();
			using (var entities = new ReferenceDataRepository(false, connectionString))
			{
				setup(entities);
				try
				{
					await entities.SaveChangesAsync(null, true);
				}
				catch (Exception exception)
				{
					exceptionAssertion(exception);
				}
			}
			using (var entities = new ReferenceDataRepository(false, connectionString))
			{
				assertion(entities);
			}
		}

		[Test]
		public async Task CheckForeignKeys()
		{
			var connectionString = GetConnectionString();
			using (var entities = new ReferenceDataRepository(false, connectionString))
			{
				Exception exception = null;
				try
				{
					entities.Add(new RefCusCodeList { ZZD_PK = Guid.NewGuid(), ZZD_Code = "CCC", ZZD_ZZK_NKCodeType = "ANY", ZZD_ZZZ_NKDataGrouping = "ANY", ZZD_Description = "Desc", ZZD_StartDate = new DateTime(1900, 01, 01), ZZD_EndDate = new DateTime(2079, 06, 06) });
					await entities.SaveChangesAsync(null, true);
				}
				catch (SqlException ex)
				{
					exception = ex;
				}
				Assert.IsNotNull(exception);
				Assert.True(exception.Message.Contains("The INSERT statement conflicted with the FOREIGN KEY constraint"));
			}
		}

		[Test]
		public async Task CheckConstraints()
		{
			var connectionString = GetConnectionString();
			using (var entities = new ReferenceDataRepository(false, connectionString))
			{
				Exception exception = null;
				try
				{
					entities.Add(new RefCusCodeList { ZZD_PK = Guid.NewGuid(), ZZD_Code = "CCC", ZZD_ZZK_NKCodeType = "ANY", ZZD_ZZZ_NKDataGrouping = "AU", ZZD_Description = "", ZZD_StartDate = new DateTime(1900, 01, 01), ZZD_EndDate = new DateTime(2079, 06, 06) });
					await entities.SaveChangesAsync(null, true);
				}
				catch (SqlException ex)
				{
					exception = ex;
				}
				Assert.IsNotNull(exception);
				Assert.True(exception.Message.Contains("The INSERT statement conflicted with the CHECK constraint"));
			}
		}

		[Test]
		[CreateDatabase("DCF8E15C7F4D4902A7258C44FA87A586", DbSchema.RefDbRepoSafe)]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public async Task TestInsertingBulkWithNoOrder()
		{
			var connectionString = GetConnectionString();
			var tariffType = new RefCusTariffType { ZZI_PK = Guid.NewGuid(), ZZI_Description = "TariffType", ZZI_TariffType = "Type1", ZZI_ZZ9_NKNomenclatureGroupType = "ZA", ZZI_ZZZ_NKDataGrouping = "BR" };

			using (var entities = new ReferenceDataRepository(false, connectionString))
			{
				entities.Add(new RefCusTariff { ZZ1_ZZI_TariffType = tariffType.ZZI_PK, ZZ1_PK = Guid.NewGuid(), ZZ1_TariffCode = "CODE1", ZZ1_IAMUnique = 1, ZZ1_Description = "Tariff 1", ZZ1_StartDate = new DateTime(1900, 1, 1), ZZ1_EndDate = new DateTime(2000, 1, 1), ZZ1_ZZF_NKTaxOrFeeCode = "ABC", ZZ1_ZZZ_NKDataGrouping = "BR", ZZ1_CompositeKeyOnZZ5 = "AAX" });
				entities.Add(tariffType);
				entities.Add(new RefDataGrouping { ZZZ_DataGrouping = "BR", ZZZ_Description = "group", ZZZ_PK = Guid.NewGuid() });
				await entities.SaveChangesAsync(null, true);
			}
			using (var entities = new ReferenceDataRepository(false, connectionString))
			{
				var tariff = entities.Get<RefCusTariff>().First();
				var refCusRate = new RefCusRate
				{
					ZZ2_PK = Guid.NewGuid(),
					ZZ2_ZZ1_Tariff = tariff.ZZ1_PK,
					ZZ2_StartDate = new DateTime(1900, 1, 1),
					ZZ2_EndDate = new DateTime(2000, 1, 1),
					ZZ2_RateFormula = "Rate",
					ZZ2_SelectorFormula = ">",
					ZZ2_ZZZ_NKDataGrouping = "BR",
					ZZ2_RX_NKCurrencyOverride = ""
				};

				var tariff2 = new RefCusTariff { ZZ1_ZZI_TariffType = tariffType.ZZI_PK, ZZ1_PK = Guid.NewGuid(), ZZ1_TariffCode = "CODE2", ZZ1_IAMUnique = 1, ZZ1_Description = "Tariff 2", ZZ1_StartDate = new DateTime(1900, 1, 1), ZZ1_EndDate = new DateTime(2000, 1, 1), ZZ1_ZZF_NKTaxOrFeeCode = "ABC", ZZ1_ZZZ_NKDataGrouping = "BR", ZZ1_CompositeKeyOnZZ5 = "AAX" };
				var refCusRates2 = new RefCusRate
				{
					ZZ2_PK = Guid.NewGuid(),
					ZZ2_ZZ1_Tariff = tariff2.ZZ1_PK,
					ZZ2_StartDate = new DateTime(1900, 1, 1),
					ZZ2_EndDate = new DateTime(2000, 1, 1),
					ZZ2_RateFormula = "Rate2",
					ZZ2_SelectorFormula = ">",
					ZZ2_ZZZ_NKDataGrouping = "BR",
					ZZ2_RX_NKCurrencyOverride = ""
				};

				entities.Add(refCusRates2);
				entities.Add(tariff2);
				entities.Add(refCusRate);

				Assert.DoesNotThrow(() => entities.SaveChangesAsync(null, true).Wait());
			}

			using (var entities = new ReferenceDataRepository(false, connectionString))
			{
				Assert.That(entities.Get<RefCusTariff>().Where(x => x.ZZ1_TariffCode.StartsWith("CODE")).Count(), Is.EqualTo(2));
				Assert.That(entities.Get<RefCusRate>().Where(x => x.ZZ2_ZZZ_NKDataGrouping == "BR").Count(), Is.EqualTo(2));
			}
		}

		static IEnumerable BulkInsertTestCase
		{
			get
			{
				yield return new TestCaseData(new Action<ReferenceDataRepository>(entities =>
				{
					entities.Add(new RefCusCodeType
					{
						ZZK_PK = Guid.NewGuid(),
						ZZK_CodeType = "PKG",
						ZZK_Description = "Package",
						ZZK_MaxLength = 0,
						ZZK_ZZZ_NKDataGrouping = "BR",
						ZZK_IsReadonly = true
					});
					entities.Add(new RefCusCodeType
					{
						ZZK_PK = Guid.NewGuid(),
						ZZK_CodeType = "TTT",
						ZZK_Description = "Something",
						ZZK_MaxLength = 0,
						ZZK_ZZZ_NKDataGrouping = "BR",
						ZZK_IsReadonly = true
					});
					entities.Add(new RefCusCodeType
					{
						ZZK_PK = Guid.NewGuid(),
						ZZK_CodeType = "ANY",
						ZZK_Description = "ANY CODETYPE",
						ZZK_MaxLength = 0,
						ZZK_ZZZ_NKDataGrouping = "BR",
						ZZK_IsReadonly = true
					});
				}),
					new Action<ReferenceDataRepository>(entities =>
					{
						Assert.AreEqual(3, entities.Get<RefCusCodeType>().Count());
					}),
					new Action<Exception>(exception => Assert.Fail($"Exception is not expected: {exception.Message}")))
				{
					TestName = "{m}_CleanData"
				};

				yield return new TestCaseData(new Action<ReferenceDataRepository>(entities =>
				{
					var parentGuid = Guid.NewGuid();
					var guid = Guid.NewGuid();

					entities.Add(new RefDataGrouping { ZZZ_DataGrouping = "AU", ZZZ_Description = "group", ZZZ_PK = Guid.NewGuid() });
					entities.Add(new RefCusCodeType { ZZK_CodeType = "ABC", ZZK_Description = "desc", ZZK_IsReadonly = true, ZZK_PK = Guid.NewGuid(), ZZK_MaxLength = 0, ZZK_ZZZ_NKDataGrouping = "AU" });
					entities.Add(new RefCusCodeListAttributeName
					{
						ZXE_PK = Guid.NewGuid(),
						ZXE_Name = "ROLE",
						ZXE_ZZK_NKCodeType = "ABC",
						ZXE_ZZZ_NKDataGrouping = "AU",
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
					entities.Add(new RefCusCodeList
					{
						ZZD_PK = parentGuid,
						ZZD_ZZK_NKCodeType = "ABC",
						ZZD_Code = "DEF",
						ZZD_Description = "description",
						ZZD_StartDate = DateTime.Now,
						ZZD_EndDate = DateTime.Now,
						ZZD_ZZZ_NKDataGrouping = "AU"
					});
					entities.Add(new RefCusCodeListAttribute { ZZE_PK = guid, ZZE_ZXE_NKName = "ROLE", ZZE_Value = "EXP", ZZE_ZZD_CodeList = parentGuid });
					entities.Add(new RefCusCodeOrAttributeTransportMode
					{
						ZZU_PK = Guid.NewGuid(),
						ZZU_ZZE_Attribute = guid,
						ZZU_TransportMode = "AIR"
					});
					entities.Add(new RefCusCodeOrAttributeTransportMode
					{
						ZZU_PK = Guid.NewGuid(),
						ZZU_ZZE_Attribute = guid,
						ZZU_TransportMode = "SEA"
					});
				}),
					new Action<ReferenceDataRepository>(entities =>
					{
						Assert.AreEqual(2, entities.Get<RefCusCodeOrAttributeTransportMode>().Count());
					}),
					new Action<Exception>(exception => Assert.Fail($"Exception is not expected: {exception.Message}")))
				{
					TestName = "{m}_NullData"
				};

				yield return new TestCaseData(new Action<ReferenceDataRepository>(entities =>
				{
					entities.Add(new RefDataGrouping { ZZZ_DataGrouping = "ZA", ZZZ_Description = "group", ZZZ_PK = Guid.NewGuid() });
					var refCarrierCode = new RefCarrierCode { ZZ4_ZZZ_NKDataGrouping = "ZA", ZZ4_Code = "CD1", ZZ4_Description = " DESC1", ZZ4_PK = Guid.NewGuid(), ZZ4_IsAir = true };
					entities.Add(refCarrierCode);
				}),
					new Action<ReferenceDataRepository>(entities => { }),
					new Action<Exception>(exception => Assert.Fail("Exception is not expected"))
					)
				{ TestName = "{m}_AdditionalAttributeShouldNotThrow" };

				yield return new TestCaseData(new Action<ReferenceDataRepository>(entities =>
				{
					entities.Add(new RefUNLOCO
					{
						RL_Code = "TESTT",
						RL_GeoLocation = new Point(10, 20) { SRID = 4326 },
						RL_PortName = "Test",
						RL_NameWithDiacriticals = "Test",
						RL_IATA = "TST",
						RL_IATARegionCode = "TST",
						RL_CoOrdinates = "12 E 12 N",
						RL_RN_NKCountryCode = "AU",
						RL_IsActive = true,
					});
				}),
					new Action<ReferenceDataRepository>(entities =>
					{
						Assert.AreEqual(1, entities.Get<RefUNLOCO>().Count());
					}),
					new Action<Exception>(exception => Assert.Fail($"Exception is not expected: {exception.Message}")))
				{
					TestName = "{m}_GeographyDataType"
				};
			}
		}

		[Test]
		public void SaveChangeBulkInsertRetryExceptions()
		{
			var bulkInsertMock = new Mock<IBulkInsertCore>();
			bulkInsertMock.Setup(x => x.SaveChangeWithBulkInsertCore(It.IsAny<EntityEntry[]>(), It.IsAny<Dictionary<EntityEntry, EntityState>>(), It.IsAny<SafeDbContext>())).Throws(new Exception());

			var connectionString = GetConnectionString();
			using (var entities = new ReferenceDataRepository(true, connectionString))
			{
				entities.Add(new RefAccTaxRate() { ZAT_PK = Guid.NewGuid(), ZAT_RateDenominator = 1, ZAT_RateNumerator = 0, ZAT_ReferenceRateType = "TYP", ZAT_RN_NKCountry = "AU", ZAT_StartDate = DateTime.UtcNow, ZAT_EndDate = DateTime.MaxValue });
				Assert.That(async () => await entities.SaveChangeWithBulkInsert(bulkInsertMock.Object), Throws.Exception.TypeOf(typeof(Exception)));
				bulkInsertMock.Verify(x => x.SaveChangeWithBulkInsertCore(It.IsAny<EntityEntry[]>(), It.IsAny<Dictionary<EntityEntry, EntityState>>(), It.IsAny<SafeDbContext>()), Times.Once);

				bulkInsertMock.Reset();
				var sqlException = new SqlExceptionBuilder().WithErrorNumber(KnownSqlExceptionsNumbers.Timeout).Build();
				bulkInsertMock.Setup(x => x.SaveChangeWithBulkInsertCore(It.IsAny<EntityEntry[]>(), It.IsAny<Dictionary<EntityEntry, EntityState>>(), It.IsAny<SafeDbContext>())).Throws(sqlException);
				var exception = Assert.Throws<SqlException>(() => entities.SaveChangeWithBulkInsert(bulkInsertMock.Object).GetAwaiter().GetResult());
				Assert.AreEqual(KnownSqlExceptionsNumbers.Timeout, exception.Number);
				bulkInsertMock.Verify(x => x.SaveChangeWithBulkInsertCore(It.IsAny<EntityEntry[]>(), It.IsAny<Dictionary<EntityEntry, EntityState>>(), It.IsAny<SafeDbContext>()), Times.Exactly(3));

				bulkInsertMock.Reset();
				bulkInsertMock.SetupSequence(x => x.SaveChangeWithBulkInsertCore(It.IsAny<EntityEntry[]>(), It.IsAny<Dictionary<EntityEntry, EntityState>>(), It.IsAny<SafeDbContext>()))
					.Throws(sqlException)
					.Throws(sqlException)
					.Returns(Task.FromResult(1));
				Assert.DoesNotThrow(() => entities.SaveChangeWithBulkInsert(bulkInsertMock.Object).GetAwaiter().GetResult());
				bulkInsertMock.Verify(x => x.SaveChangeWithBulkInsertCore(It.IsAny<EntityEntry[]>(), It.IsAny<Dictionary<EntityEntry, EntityState>>(), It.IsAny<SafeDbContext>()), Times.Exactly(3));

				bulkInsertMock.Reset();
				sqlException = new SqlExceptionBuilder().WithErrorNumber(KnownSqlExceptionsNumbers.TransactionDeadlock).Build();
				bulkInsertMock.Setup(x => x.SaveChangeWithBulkInsertCore(It.IsAny<EntityEntry[]>(), It.IsAny<Dictionary<EntityEntry, EntityState>>(), It.IsAny<SafeDbContext>())).Throws(sqlException);
				exception = Assert.Throws<SqlException>(() => entities.SaveChangeWithBulkInsert(bulkInsertMock.Object).GetAwaiter().GetResult());
				Assert.AreEqual(KnownSqlExceptionsNumbers.TransactionDeadlock, exception.Number);
				bulkInsertMock.Verify(x => x.SaveChangeWithBulkInsertCore(It.IsAny<EntityEntry[]>(), It.IsAny<Dictionary<EntityEntry, EntityState>>(), It.IsAny<SafeDbContext>()), Times.Exactly(3));

				bulkInsertMock.Reset();
				bulkInsertMock.SetupSequence(x => x.SaveChangeWithBulkInsertCore(It.IsAny<EntityEntry[]>(), It.IsAny<Dictionary<EntityEntry, EntityState>>(), It.IsAny<SafeDbContext>()))
					.Throws(sqlException)
					.Throws(sqlException)
					.Returns(Task.FromResult(1));
				Assert.DoesNotThrow(() => entities.SaveChangeWithBulkInsert(bulkInsertMock.Object).GetAwaiter().GetResult());
				bulkInsertMock.Verify(x => x.SaveChangeWithBulkInsertCore(It.IsAny<EntityEntry[]>(), It.IsAny<Dictionary<EntityEntry, EntityState>>(), It.IsAny<SafeDbContext>()), Times.Exactly(3));
			}
		}

		[Test]
		public async Task GetWithExpand()
		{

			var tariffTypePk = Guid.NewGuid();
			var expiredParentRecordPk = Guid.NewGuid();
			var rateCodePk = Guid.NewGuid();
			var rate1Pk = Guid.NewGuid();
			var rate2Pk = Guid.NewGuid();
			var applicability1Pk = Guid.NewGuid();
			var connectionString = GetConnectionString();
			var rateTypePk = Guid.NewGuid();

			var rateType = new RefCusRateType
			{
				ZZR_PK = rateTypePk,
				ZZR_CustomsValueFormula = "",
				ZZR_Description = "Desc",
				ZZR_IsPayable = true,
				ZZR_RateType = "RT",
				ZZR_ZZZ_NKDataGrouping = "ZZ",
				ZZR_RX_NKFormulaCurrency = ""
			};

			var tariffType = new RefCusTariffType
			{
				ZZI_Description = "type",
				ZZI_PK = tariffTypePk,
				ZZI_TariffType = "TA",
				ZZI_ZZZ_NKDataGrouping = "ZZ",
				ZZI_HasFormulaSpecificQuestions = true,
				ZZI_ZZR_RateType = rateTypePk,
				ZZI_ZZ9_NKNomenclatureGroupType = ""
			};

			var tariff = new RefCusTariff
			{
				ZZ1_PK = expiredParentRecordPk,
				ZZ1_Description = "To Be Cloned",
				ZZ1_TariffCode = "0101",
				ZZ1_ZZZ_NKDataGrouping = "ZZ",
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
				ZY1_ZZZ_NKDataGrouping = "ZZ",
				ZY1_InternalUse = true,
				ZY1_ZZR_RateType = rateTypePk,
			};
			var rate1 = new RefCusRate
			{
				ZZ2_PK = rate1Pk,
				ZZ2_ZZ1_Tariff = tariff.ZZ1_PK,
				ZZ2_ZZZ_NKDataGrouping = "ZZ",
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
				ZZ2_ZZZ_NKDataGrouping = "ZZ",
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
				ZZA_ZZZ_NKDataGrouping = "ZZ"
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
				ZZC_PK = Guid.NewGuid(),
				ZZC_ZZT_Applicability = applicability1.ZZT_PK,
				ZZC_ZZA_TradeGroup = tradeGroup.ZZA_PK
			};

			using (var entities = new ReferenceDataRepository(false, connectionString))
			{
				entities.Add(new RefDataGrouping
				{
					ZZZ_PK = Guid.NewGuid(),
					ZZZ_DataGrouping = "ZZ",
					ZZZ_Description = "South Africa"
				});
				await entities.SaveChangesAsync(null);

				entities.Add(rateType);
				entities.Add(tariffType);
				entities.Add(rateCode);
				entities.Add(tradeGroup);
				await entities.SaveChangesAsync(null);
				tariff.RefCusRates = new[] { rate1, rate2 };

				entities.Add(tariff);
				rate1.RefCusApplicabilities = new[] { applicability1 };
				entities.Add(rate1);
				entities.Add(rate2);

				entities.Add(applicability1);
				entities.Add(excludedTradeGroup);
				await entities.SaveChangesAsync(null);

				var dict = new Dictionary<Type, List<Type>>();
				var result = entities.GetWithExpand<RefCusTariff>(dict);
				Assert.NotNull(result);
				Assert.That(result.Count(), Is.EqualTo(1));
				Assert.That(result.First().RefCusRates.Count, Is.EqualTo(2));
				Assert.That(result.First().RefCusRates.First(x => x.ZZ2_PK == rate1Pk).RefCusApplicabilities.Count, Is.EqualTo(1));
				Assert.That(result.First().RefCusRates.First(x => x.ZZ2_PK == rate1Pk).RefCusApplicabilities.First().ZZT_AdditionalCode, Is.EqualTo("AC1"));
				Assert.That(dict[typeof(RefCusTariff)].Any(x => x.GetGenericArguments()[0] == typeof(RefCusRate)));
				Assert.That(dict[typeof(RefCusRate)].Any(x => x.GetGenericArguments()[0] == typeof(RefCusApplicability)));
			}
		}

		string GetConnectionString()
		{
			var dbName = CreateDatabaseAttribute.DbNamePrefix + "A682BF7C1FB74F9EB84F86FB5D47AB88";
			var connectionString = TestConnectionString.GetAdmin(dbName);
			return connectionString;
		}
	}
}
