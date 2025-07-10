using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Rating;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using WiseRates.Api.Client;
using WiseRates.Api.Model;

namespace Enterprise.Rating.Module.Testing
{
	[TestedType(typeof(CarrierChargeCodeModule))]
	public class CarrierChargeCodeModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID() => ModuleIDs.CarrierChargeCode;
		protected override bool HasController() => false;

		public void TestProperties()
		{
			using (var module = new CarrierChargeCodeModule())
			{
				AssertEquals(false, module.SupportsWorkflow);
				AssertEquals(false, module.AllowNew);
				AssertEquals(false, module.AllowEdit);
				AssertEquals(false, module.AllowDelete);
				AssertEquals(false, module.AllowView);
				AssertEquals(false, module.ShowRecentItems);
				AssertEquals(false, module.AllowUniversalCopy);
				AssertEquals(false, module.AllowCopyFilterGridHyperlinkToClipboard);
				AssertEquals(true, module.HasExportMenuItems);
			}
		}

		public void TestLoadCollection()
		{
			var allCodes = new UntranslatableCodeDescriptionPairList("");
			allCodes.AddPair("BBB", "Bunker Adjustment Factor");
			allCodes.AddPair("DDD", "Destination Security Surcharge");
			allCodes.AddPair("FF1", "International Freight");
			allCodes.AddPair("FF2", "Fuel Surcharge");
			allCodes.AddPair("ZZZ", "Import Container Detention");

			var wiseRatesClientMock = new Mock<IWiseRatesClient>();
			wiseRatesClientMock
				.Setup(c => c.GetAllChargeCodesWithMappingInfo(It.IsAny<string>()))
				.Returns(new[]
				{
					new ChargeCodeWithMappingInfo { Code = "BBB", Description = "Bunker Adjustment Factor", ForeignCode = "BBB", ForeignName = "Bunker Adjustment Factor", Carrier = "ACAC" },
					new ChargeCodeWithMappingInfo { Code = "DDD", Description = "Destination Security Surcharge", ForeignCode = "DDD", ForeignName = "Destination Security Surcharge", Carrier = "BCBC" },
					new ChargeCodeWithMappingInfo { Code = "FF1", Description = "International Freight", ForeignCode = "FF1", ForeignName = "International Freight", Carrier = "CDCD" },
					new ChargeCodeWithMappingInfo { Code = "FF2", Description = "Fuel Surcharge", ForeignCode = "FF2", ForeignName = "Fuel Surcharge", Carrier = "DEDE" },
					new ChargeCodeWithMappingInfo { Code = "ZZZ", Description = "Import Container Detention", ForeignCode = "ZZZ", ForeignName = "Import Container Detention", Carrier = "ASAS" },
				});

			var wiseRatesClientFactoryMock = new Mock<IWiseRatesClientFactory>();
			wiseRatesClientFactoryMock
				.Setup(f => f.TryCreate(
					It.IsAny<string>(),
					It.IsAny<int>(),
					It.IsAny<CancellationToken>(),
					It.IsAny<ILogger>()))
				.Returns((wiseRatesClientMock.Object, null));

			using (ObjectFactory.Substitute(wiseRatesClientFactoryMock.Object))
			{
				var localChargeCode1 = CreateChargeCode("ACC1", false, "BBB", "ACAC");
				var localChargeCode2 = CreateChargeCode("ACC2", false, "FF1", "CDCD");

				Factory.Save();

				var globalChargeCode1 = CreateChargeCode("ACC1", true, "BBB", "ACAC");
				var globalChargeCode2 = CreateChargeCode("ACC2", true, "DDD", "BCBC");

				Factory.Save();

				using (var module = new CarrierChargeCodeModuleForTest())
				{
					var query = new ZQuery();
					var list = module.LoadCollectionForTest(query);
					AssertEquals("BBB-ACC1-ACC1, DDD-ACC2-, FF1--ACC2, FF2--, ZZZ--", CodesAsString(list));

					query = new ZQuery();
					query.AddToFilter(MappedChargeCodeSchema.UCC_ForeignCode, SQLComparisonOperator.Equal, "BBB");
					list = module.LoadCollectionForTest(query);
					AssertEquals("BBB-ACC1-ACC1", CodesAsString(list));
					AssertEquals("Bunker Adjustment Factor", list[0].UCC_Description);

					query = new ZQuery();
					query.AddToFilter(MappedChargeCodeSchema.UCC_ForeignCode, SQLComparisonOperator.StartsWith, "F");
					list = module.LoadCollectionForTest(query);
					AssertEquals("FF1--ACC2, FF2--", CodesAsString(list));

					query = new ZQuery();
					query.AddToFilter(MappedChargeCodeSchema.UCC_ForeignName, SQLComparisonOperator.EndsWith, "Surcharge");
					list = module.LoadCollectionForTest(query);
					AssertEquals("DDD-ACC2-, FF2--", CodesAsString(list));

					query = new ZQuery();
					query.AddToFilter(MappedChargeCodeSchema.UCC_GlobalChargeCode, SQLComparisonOperator.Equal, "ACC2");
					list = module.LoadCollectionForTest(query);
					AssertEquals("DDD-ACC2-", CodesAsString(list));

					query = new ZQuery();
					query.AddToFilter(MappedChargeCodeSchema.UCC_LocalChargeCode, SQLComparisonOperator.Equal, "ACC2");
					list = module.LoadCollectionForTest(query);
					AssertEquals("FF1--ACC2", CodesAsString(list));
				}
			}
		}

		AccChargeCode CreateChargeCode(string code, bool isGlobal, string carrierChargeCode = "", string scac = "")
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = code;
			chargeCode.AC_Desc = code + " Description";
			chargeCode.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
			chargeCode.AC_GC = isGlobal ? ZGuid.Empty : Env.CurrentCompanyPK;

			if (!string.IsNullOrWhiteSpace(carrierChargeCode))
			{
				var orgHeader = CreateOrganizationWithScac(scac);

				var mapping = chargeCode.UniversalChargeCodeMappingsCollection.AddNew();
				mapping.AUP_Type = "CAR";
				mapping.AUP_Code = carrierChargeCode;
				mapping.AUP_TransportMode = "SEA";
				mapping.AUP_OH_Carrier = orgHeader.PK;
			}

			return chargeCode;
		}

		OrgHeader CreateOrganizationWithScac(string scac)
		{
			var refShippingLine = Factory.Load<RefShippingLine>(new ZQuery(RefShippingLineSchema.RSL_StandardCarrierAlphaCode, scac)).FirstOrDefault();
			if (refShippingLine != null)
			{
				return Factory.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_RSL_ShippingLine, refShippingLine.PK)).FirstOrDefault();
			}

			refShippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			orgHeader.OH_IsShippingLine = true;
			orgHeader.OH_RSL_ShippingLine = refShippingLine.PK;
			refShippingLine.RSL_StandardCarrierAlphaCode = scac;

			return orgHeader;
		}

		public void TestLoadCollection_FromNewChargeCodeWithDuplicateUniversalCode()
		{
			var allCodes = new UntranslatableCodeDescriptionPairList("");
			allCodes.AddPair("BBB", "Bunker Adjustment Factor");
			allCodes.AddPair("DDD", "Destination Security Surcharge");

			var wiseRatesClientMock = new Mock<IWiseRatesClient>();
			wiseRatesClientMock
				.Setup(c => c.GetAllChargeCodesWithMappingInfo(It.IsAny<string>()))
				.Returns(new[]
				{
					new ChargeCodeWithMappingInfo { Code = "BBB", Description = "Bunker Adjustment Factor", ForeignCode = "BBB", ForeignName = "Bunker Adjustment Factor", Carrier = "KDKD" },
					new ChargeCodeWithMappingInfo { Code = "DDD", Description = "Destination Security Surcharge", ForeignCode = "DDD", ForeignName = "Destination Security Surcharge", Carrier = "SDSD" },
				});

			var wiseRatesClientFactoryMock = new Mock<IWiseRatesClientFactory>();
			wiseRatesClientFactoryMock
				.Setup(f => f.TryCreate(
					It.IsAny<string>(),
					It.IsAny<int>(),
					It.IsAny<CancellationToken>(),
					It.IsAny<ILogger>()))
				.Returns((wiseRatesClientMock.Object, null));

			var helper = new Business.Testing.TestHelper(Factory);
			var chargeCodeInDb = helper.ChargeCodes["ACC1"];
			var chargeCodeInDbMapping = chargeCodeInDb.UniversalChargeCodeMappingsCollection.AddNew();
			chargeCodeInDbMapping.AUP_Code = "BBB";

			Factory.Save();

			using (ObjectFactory.Substitute(wiseRatesClientFactoryMock.Object))
			{
				var chargeCodeNotInDb = Factory.New<AccChargeCode>();
				var chargeCodeNotInDbMapping = chargeCodeNotInDb.UniversalChargeCodeMappingsCollection.AddNew();
				chargeCodeNotInDbMapping.AUP_Code = "BBB";
				chargeCodeNotInDb.AC_GC = Env.CurrentCompanyPK;

				using (var module = new CarrierChargeCodeModuleForTest())
				{
					var query = new ZQuery();
					var list = module.LoadCollectionForTest(query);
					AssertEquals("BBB--ACC1, DDD--", CodesAsString(list));
					list = module.LoadCollectionForTest(query);
					AssertEquals("load again has same result", "BBB--ACC1, DDD--", CodesAsString(list));
				}
			}
		}

		public void TestLoadCollection_WithUnmappedCodes()
		{
			var allCodes = new UntranslatableCodeDescriptionPairList("");
			allCodes.AddPair("AAA", "Bunker Adjustment Factor");
			allCodes.AddPair("BBB", "Destination Security Surcharge");
			allCodes.AddPair("CCC", "Fuel Surcharge");

			var wiseRatesClientMock = new Mock<IWiseRatesClient>();
			wiseRatesClientMock
				.Setup(c => c.GetAllChargeCodesWithMappingInfo(It.IsAny<string>()))
				.Returns(new[]
				{
					new ChargeCodeWithMappingInfo { Code = "AAA", Description = "Bunker Adjustment Factor", ForeignCode = "AAA", ForeignName = "Bunker Adjustment Factor", Carrier = "ASAS" },
					new ChargeCodeWithMappingInfo { Code = "BBB", Description = "Destination Security Surcharge", ForeignCode = "BBB", ForeignName = "Destination Security Surcharge", Carrier = "CSCS" },
					new ChargeCodeWithMappingInfo { Code = "CCC", Description = "Fuel Surcharge", ForeignCode = "CCC", ForeignName = "Fuel Surcharge", Carrier = "HELH" },
				});

			var wiseRatesClientFactoryMock = new Mock<IWiseRatesClientFactory>();
			wiseRatesClientFactoryMock
				.Setup(f => f.TryCreate(
					It.IsAny<string>(),
					It.IsAny<int>(),
					It.IsAny<CancellationToken>(),
					It.IsAny<ILogger>()))
				.Returns((wiseRatesClientMock.Object, null));

			var helper = new Business.Testing.TestHelper(Factory);
			var chargeCodeInDb = helper.ChargeCodes["ACC1"];
			var chargeCodeInDbMapping = chargeCodeInDb.UniversalChargeCodeMappingsCollection.AddNew();
			chargeCodeInDbMapping.AUP_Code = "BBB";

			Factory.Save();

			using (ObjectFactory.Substitute(wiseRatesClientFactoryMock.Object))
			{
				var chargeCodeNotInDb = Factory.New<AccChargeCode>();
				var chargeCodeNotInDbMapping = chargeCodeNotInDb.UniversalChargeCodeMappingsCollection.AddNew();
				chargeCodeNotInDbMapping.AUP_Code = "BBB";
				chargeCodeNotInDb.AC_GC = Env.CurrentCompanyPK;

				using (var module = new CarrierChargeCodeModuleForTest())
				{
					var query = new ZQuery();
					var list = module.LoadCollectionForTest(query);
					AssertEquals("AAA--, BBB--ACC1, CCC--", CodesAsString(list));

					query = new ZQuery();
					query.AddToFilter(MappedChargeCodeSchema.UCC_ForeignCode, SQLComparisonOperator.Equal, "AAA");
					list = module.LoadCollectionForTest(query);
					AssertEquals("AAA--", CodesAsString(list));
					AssertEquals("Bunker Adjustment Factor", list[0].UCC_Description);

					query = new ZQuery();
					query.AddToFilter(MappedChargeCodeSchema.UCC_ForeignCode, SQLComparisonOperator.Equal, "BBB");
					list = module.LoadCollectionForTest(query);
					AssertEquals("BBB--ACC1", CodesAsString(list));
					AssertEquals("Destination Security Surcharge", list[0].UCC_Description);

					query = new ZQuery();
					query.AddToFilter(MappedChargeCodeSchema.UCC_ForeignCode, SQLComparisonOperator.Equal, "CCC");
					list = module.LoadCollectionForTest(query);
					AssertEquals("CCC--", CodesAsString(list));
					AssertEquals("Fuel Surcharge", list[0].UCC_Description);

					query = new ZQuery();
					query.AddToFilter(MappedChargeCodeSchema.UCC_ForeignCode, SQLComparisonOperator.Equal, "DDD");
					list = module.LoadCollectionForTest(query);
					AssertEquals(string.Empty, CodesAsString(list));
					string expectedMessage = string.Format("No results found.");
					var notifiedMessage = UnitTestUserNotification.Instance.LastMessage.Text;
					AssertEquals(expectedMessage, notifiedMessage);
					UnitTestUserNotification.Instance.ClearMessages();
				}
			}
		}

		static string CodesAsString(IEnumerable<CarrierChargeCodeBizo> list)
		{
			return string.Join(", ", list.Cast<CarrierChargeCodeBizo>()
				.Select(x => x.UCC_ForeignCode + "-" + x.UCC_GlobalChargeCode + "-" + x.UCC_LocalChargeCode)
				.OrderBy(x => x));
		}

		class CarrierChargeCodeModuleForTest : CarrierChargeCodeModule
		{
			public CarrierChargeCodeBizo[] LoadCollectionForTest(ZQuery query)
			{
				var result = PerformSearchCore(typeof(CarrierChargeCodeBizo), query);
				BindSearchResultToGrid(GridCollection, result);
				OnAfterPerformSearchCore();
				return GridCollection.Cast<CarrierChargeCodeBizo>().ToArray();
			}
		}
	}
}
