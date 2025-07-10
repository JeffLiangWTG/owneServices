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
using Moq;
using NUnit.Framework;
using WiseRates.Api.Client;
using WiseRates.Api.Model;

namespace Enterprise.Rating.Module.Testing
{
	[TestedType(typeof(UniversalChargeCodeModule))]
	public class UniversalChargeCodeModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID() => ModuleIDs.UniversalChargeCode;
		protected override bool HasController() => false;

		public void TestProperties()
		{
			using (var module = new UniversalChargeCodeModule())
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
					new ChargeCodeWithMappingInfo { Code = "BBB", Description = "Bunker Adjustment Factor" },
					new ChargeCodeWithMappingInfo { Code = "DDD", Description = "Destination Security Surcharge" },
					new ChargeCodeWithMappingInfo { Code = "FF1", Description = "International Freight" },
					new ChargeCodeWithMappingInfo { Code = "FF2", Description = "Fuel Surcharge" },
					new ChargeCodeWithMappingInfo { Code = "ZZZ", Description = "Import Container Detention" },
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
				var localChargeCode1 = CreateChargeCode("ACC1", false, "BBB");
				var localChargeCode2 = CreateChargeCode("ACC2", false, "FF1");

				Factory.Save();

				var globalChargeCode1 = CreateChargeCode("ACC1", true, "BBB");
				var globalChargeCode2 = CreateChargeCode("ACC2", true, "DDD");

				Factory.Save();

				using (var module = new UniversalChargeCodeModuleForTest())
				{
					var query = new ZQuery();
					var list = module.LoadCollectionForTest(query);
					AssertEquals("BBB-ACC1-ACC1, DDD-ACC2-, FF1--ACC2, FF2--, ZZZ--", CodesAsString(list));

					query = new ZQuery();
					query.AddToFilter(MappedChargeCodeSchema.UCC_Code, SQLComparisonOperator.Equal, "BBB");
					list = module.LoadCollectionForTest(query);
					AssertEquals("BBB-ACC1-ACC1", CodesAsString(list));
					AssertEquals("Bunker Adjustment Factor", list[0].UCC_Description);

					query = new ZQuery();
					query.AddToFilter(MappedChargeCodeSchema.UCC_Code, SQLComparisonOperator.StartsWith, "F");
					list = module.LoadCollectionForTest(query);
					AssertEquals("FF1--ACC2, FF2--", CodesAsString(list));

					query = new ZQuery();
					query.AddToFilter(MappedChargeCodeSchema.UCC_Description, SQLComparisonOperator.EndsWith, "Surcharge");
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

		AccChargeCode CreateChargeCode(string code, bool isGlobal, string universalChargeCode = "")
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = code;
			chargeCode.AC_Desc = code + " Description";
			chargeCode.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
			chargeCode.AC_GC = isGlobal ? ZGuid.Empty : Env.CurrentCompanyPK;

			if (!string.IsNullOrWhiteSpace(universalChargeCode))
			{
				var mapping = chargeCode.UniversalChargeCodeMappingsCollection.AddNew();
				mapping.AUP_Code = universalChargeCode;
			}

			return chargeCode;
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
					new ChargeCodeWithMappingInfo { Code = "BBB", Description = "Bunker Adjustment Factor" },
					new ChargeCodeWithMappingInfo { Code = "DDD", Description = "Destination Security Surcharge" },
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

				using (var module = new UniversalChargeCodeModuleForTest())
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
			new ChargeCodeWithMappingInfo { Code = "AAA", Description = "Bunker Adjustment Factor" },
			new ChargeCodeWithMappingInfo { Code = "BBB", Description = "Destination Security Surcharge" },
			new ChargeCodeWithMappingInfo { Code = "CCC", Description = "Fuel Surcharge" },
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

				using (var module = new UniversalChargeCodeModuleForTest())
				{
					var query = new ZQuery();
					var list = module.LoadCollectionForTest(query);
					AssertEquals("AAA--, BBB--ACC1, CCC--", CodesAsString(list));

					query = new ZQuery();
					query.AddToFilter(MappedChargeCodeSchema.UCC_Code, SQLComparisonOperator.Equal, "AAA");
					list = module.LoadCollectionForTest(query);
					AssertEquals("AAA--", CodesAsString(list));
					AssertEquals("Bunker Adjustment Factor", list[0].UCC_Description);

					query = new ZQuery();
					query.AddToFilter(MappedChargeCodeSchema.UCC_Code, SQLComparisonOperator.Equal, "BBB");
					list = module.LoadCollectionForTest(query);
					AssertEquals("BBB--ACC1", CodesAsString(list));
					AssertEquals("Destination Security Surcharge", list[0].UCC_Description);

					query = new ZQuery();
					query.AddToFilter(MappedChargeCodeSchema.UCC_Code, SQLComparisonOperator.Equal, "CCC");
					list = module.LoadCollectionForTest(query);
					AssertEquals("CCC--", CodesAsString(list));
					AssertEquals("Fuel Surcharge", list[0].UCC_Description);

					query = new ZQuery();
					query.AddToFilter(MappedChargeCodeSchema.UCC_Code, SQLComparisonOperator.Equal, "DDD");
					list = module.LoadCollectionForTest(query);
					AssertEquals(string.Empty, CodesAsString(list));
					string expectedMessage = string.Format("No results found.");
					var notifiedMessage = UnitTestUserNotification.Instance.LastMessage.Text;
					AssertEquals(expectedMessage, notifiedMessage);
					UnitTestUserNotification.Instance.ClearMessages();
				}
			}
		}

		static string CodesAsString(IEnumerable<UniversalChargeCodeBizo> list)
		{
			return string.Join(", ", list.Cast<UniversalChargeCodeBizo>()
				.Select(x => x.UCC_Code + "-" + x.UCC_GlobalChargeCode + "-" + x.UCC_LocalChargeCode)
				.OrderBy(x => x));
		}

		class UniversalChargeCodeModuleForTest : UniversalChargeCodeModule
		{
			public UniversalChargeCodeBizo[] LoadCollectionForTest(ZQuery query)
			{
				var result = PerformSearchCore(typeof(UniversalChargeCodeBizo), query);
				BindSearchResultToGrid(GridCollection, result);
				OnAfterPerformSearchCore();
				return GridCollection.Cast<UniversalChargeCodeBizo>().ToArray();
			}
		}
	}
}
