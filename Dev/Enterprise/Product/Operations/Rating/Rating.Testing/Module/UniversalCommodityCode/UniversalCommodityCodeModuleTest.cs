using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Rating;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Moq;
using NUnit.Framework;
using WiseRates.Api.Client;
using WiseRates.Api.Model;

namespace Enterprise.Rating.Module.Testing
{
	[TestedType(typeof(UniversalCommodityCodeModule))]
	public class UniversalCommodityCodeModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID() => ModuleIDs.UniversalCommodityCode;
		protected override bool HasController() => false;

		public void TestProperties()
		{
			using (var module = new UniversalCommodityCodeModule())
			{
				AssertEquals(false, module.SupportsWorkflow);
				AssertEquals(false, module.AllowNew);
				AssertEquals(false, module.AllowEdit);
				AssertEquals(false, module.AllowDelete);
				AssertEquals(false, module.AllowView);
				AssertEquals(false, module.ShowRecentItems);
				AssertEquals(false, module.AllowUniversalCopy);
				AssertEquals(false, module.AllowCopyFilterGridHyperlinkToClipboard);
			}
		}

		public void TestLoadCollection()
		{
			var wiseRatesClientMock = new Mock<IWiseRatesClient>();
			wiseRatesClientMock
				.Setup(c => c.GetCommodityGroups(It.IsAny<string>()))
				.Returns(new[]
				{
					new RefCommodityGroup { Code = "UCG1", Description = "DES1" },
					new RefCommodityGroup { Code = "UCG2", Description = "DES2" },
					new RefCommodityGroup { Code = "UCG3", Description = "DES3" },
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
				var refCommodityCode1 = Factory.New<RefCommodityCode>();
				refCommodityCode1.RH_UniversalCommodityGroup = "UCG1";
				refCommodityCode1.RH_FN_NKNMFC = ZString.Empty;
				refCommodityCode1.RH_Code = "COM1";
				refCommodityCode1.RH_Description = "des1";

				var refCommodityCode2 = Factory.New<RefCommodityCode>();
				refCommodityCode2.RH_UniversalCommodityGroup = "UCG1";
				refCommodityCode2.RH_FN_NKNMFC = ZString.Empty;
				refCommodityCode2.RH_Code = "COM2";
				refCommodityCode2.RH_Description = "des2";

				var refCommodityCode3 = Factory.New<RefCommodityCode>();
				refCommodityCode3.RH_UniversalCommodityGroup = "UCG3";
				refCommodityCode3.RH_FN_NKNMFC = ZString.Empty;
				refCommodityCode3.RH_Code = "COM3";
				refCommodityCode3.RH_Description = "des3";

				var refCommodityCode4 = Factory.New<RefCommodityCode>();
				refCommodityCode4.RH_UniversalCommodityGroup = "UCG4";
				refCommodityCode4.RH_FN_NKNMFC = ZString.Empty;
				refCommodityCode4.RH_Code = "COM4";
				refCommodityCode4.RH_Description = "des4";

				Factory.Save();

				using (var module = new UniversalCommodityCodeModuleForTest())
				{
					var query = new ZQuery();
					var list = module.LoadCollectionForTest(query);
					AssertEquals("UCG1-DES1-COM1-des1, UCG1-DES1-COM2-des2, UCG2-DES2--, UCG3-DES3-COM3-des3", CodesAsString(list));

					query = new ZQuery().AddToFilter(UniversalCommodityCodeSchema.RH_UniversalCommodityGroup, SQLComparisonOperator.Equal, "UCG2");
					list = module.LoadCollectionForTest(query);
					AssertEquals("UCG2-DES2--", CodesAsString(list));

					query = new ZQuery();
					query.AddToFilter(UniversalCommodityCodeSchema.RH_UniversalCommodityGroup, SQLComparisonOperator.Contains, "2");
					list = module.LoadCollectionForTest(query);
					AssertEquals("UCG2-DES2--", CodesAsString(list));

					query = new ZQuery();
					query.AddToFilter(UniversalCommodityCodeSchema.RH_UniversalCommodityGroupDescription, SQLComparisonOperator.Contains, "3");
					list = module.LoadCollectionForTest(query);
					AssertEquals("UCG3-DES3-COM3-des3", CodesAsString(list));

					query = new ZQuery();
					query.AddToFilter(UniversalCommodityCodeSchema.RH_Code, SQLComparisonOperator.Contains, "1");
					list = module.LoadCollectionForTest(query);
					AssertEquals("UCG1-DES1-COM1-des1", CodesAsString(list));

					query = new ZQuery();
					query.AddToFilter(UniversalCommodityCodeSchema.RH_Description, SQLComparisonOperator.Contains, "4");
					list = module.LoadCollectionForTest(query);
					AssertEquals("", CodesAsString(list));
				}
			}
		}

		static string CodesAsString(IEnumerable<BusinessObject> list)
		{
			return string.Join(", ", list.Cast<UniversalCommodityCodeBizo>()
				.Select(x => $"{x.RH_UniversalCommodityGroup}-{x.RH_UniversalCommodityGroupDescription}-{x.RH_Code}-{x.RH_Description}")
				.OrderBy(x => x));
		}

		class UniversalCommodityCodeModuleForTest : UniversalCommodityCodeModule
		{
			public UniversalCommodityCodeBizo[] LoadCollectionForTest(ZQuery query)
			{
				var result = PerformSearchCore(typeof(UniversalCommodityCodeBizo), query);
				BindSearchResultToGrid(GridCollection, result);
				OnAfterPerformSearchCore();
				return GridCollection.Cast<UniversalCommodityCodeBizo>().ToArray();
			}
		}
	}
}