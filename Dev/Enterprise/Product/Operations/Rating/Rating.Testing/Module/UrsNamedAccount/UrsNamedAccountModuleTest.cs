using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using WiseRates.Api.Client;

namespace Enterprise.Rating.Module.Testing
{
	[TestedType(typeof(UrsNamedAccountModule))]
	sealed class UrsNamedAccountModuleTest
		: ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID() => ModuleIDs.UrsNamedAccount;
		protected override bool HasController() => false;

		#region Properties

		public void TestProperties()
		{
			using (var module = new UrsNamedAccountModule())
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

		public void TestGetNewBusinessObjectCollection()
		{
			using (var module = new UrsNamedAccountModule())
			{
				Assert("Invalid type", module.GetNewBusinessObjectCollection() is OrgCarrierNamedAccountCollection);
			}
		}

		public void TestFilterBusinessObject()
		{
			using (var module = new UrsNamedAccountModule())
			{
				Assert("Invalid type", module.FilterBusinessObject is UrsNamedAccountFilterBusinessObject);

				var filterBo = module.FilterBusinessObject as UrsNamedAccountFilterBusinessObject;
				Assert("Should be filterBO for module", filterBo.CarrierFilterDisabled);
			}
		}

		#endregion

		#region LoadCollection

		public void TestCustomGridLoad_NoneMapped()
		{
			using (var module = new UrsNamedAccountModuleForTest())
			{
				using (ObjectFactory.Substitute(GetMockWiseRatesClientFactoryForTest(["Nike", "Nike Inc.", "Account"])))
				{
					var collection = module.LoadCollectionForTest(new ZQuery());
					AssertContainsExactElementsInAnyOrder(["Nike--", "Nike Inc.--", "Account--"], NamedAccountBizosAsString(collection));
				}
			}
		}

		public void TestCustomGridLoad_OneMapped()
		{
			using (var module = new UrsNamedAccountModuleForTest())
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();
				org.OH_FullName = "Org 1";
				org.OH_IsConsignee = true;

				var carrier = Factory.NewWithValidTestData<OrgHeader>();
				carrier.OH_FullName = "Carrier";

				var ona1 = Factory.New<OrgCarrierNamedAccount>();
				ona1.ONA_ForeignName = "Nike";
				ona1.ONA_OH_Organization = org.PK;

				var ona2 = Factory.New<OrgCarrierNamedAccount>();
				ona2.ONA_ForeignName = "Nike";
				ona2.ONA_OH_Organization = org.PK;
				ona2.ONA_OH_Carrier = carrier.PK;

				using (ObjectFactory.Substitute(GetMockWiseRatesClientFactoryForTest(["Nike", "Nike Inc.", "Account"])))
				{
					Factory.Save();

					var collection = module.LoadCollectionForTest(new ZQuery());
					AssertContainsExactElementsInAnyOrder(["Nike Inc.--", "Nike-Carrier-Org 1", "Nike--Org 1", "Account--"], NamedAccountBizosAsString(collection));
				}
			}
		}

		public void TestCustomGridLoad_Filtered()
		{
			using (var module = new UrsNamedAccountModuleForTest())
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();
				org.OH_FullName = "Org 1";
				org.OH_IsConsignee = true;

				var org2 = Factory.NewWithValidTestData<OrgHeader>();
				org2.OH_FullName = "Org 2";
				org2.OH_IsConsignee = true;

				var carrier = Factory.NewWithValidTestData<OrgHeader>();
				carrier.OH_FullName = "Carrier";

				var ona1 = Factory.New<OrgCarrierNamedAccount>();
				ona1.ONA_ForeignName = "Nike";
				ona1.ONA_OH_Organization = org.PK;

				var ona2 = Factory.New<OrgCarrierNamedAccount>();
				ona2.ONA_ForeignName = "Nike";
				ona2.ONA_OH_Organization = org2.PK;
				ona2.ONA_OH_Carrier = carrier.PK;

				using (ObjectFactory.Substitute(GetMockWiseRatesClientFactoryForTest(["Nike", "Nike Inc.", "Account"])))
				{
					Factory.Save();

					var query = new ZQuery();
					query.AddToFilter(OrgCarrierNamedAccountSchema.ONA_ForeignName, SQLComparisonOperator.StartsWith, "Nike");
					AssertContainsExactElementsInAnyOrder(["Nike Inc.--", "Nike-Carrier-Org 2", "Nike--Org 1"], NamedAccountBizosAsString(module.LoadCollectionForTest(query)));

					query = new ZQuery();
					query.AddToFilter(OrgCarrierNamedAccountSchema.ONA_OH_Carrier, SQLComparisonOperator.Equal, carrier.PK);
					AssertContainsExactElementsInAnyOrder(["Nike-Carrier-Org 2"], NamedAccountBizosAsString(module.LoadCollectionForTest(query)));

					query = new ZQuery();
					query.AddToFilter(OrgCarrierNamedAccountSchema.ONA_OH_Organization, SQLComparisonOperator.Equal, org.PK);
					AssertContainsExactElementsInAnyOrder(["Nike--Org 1"], NamedAccountBizosAsString(module.LoadCollectionForTest(query)));

					var nameQuery = new ZDBOnlyQuery(typeof(OrgCarrierNamedAccount));
					var orgQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgCarrierNamedAccountSchema.ONA_OH_Carrier);
					orgQuery.AddToFilter(OrgHeaderSchema.OH_FullName, SQLComparisonOperator.StartsWith, "C");
					nameQuery.AddSubQuery(orgQuery, JoinCondition.And);
					AssertContainsExactElementsInAnyOrder(["Nike-Carrier-Org 2"], NamedAccountBizosAsString(module.LoadCollectionForTest(nameQuery)));

					nameQuery = new ZDBOnlyQuery(typeof(OrgCarrierNamedAccount));
					orgQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgCarrierNamedAccountSchema.ONA_OH_Organization);
					orgQuery.AddToFilter(OrgHeaderSchema.OH_FullName, SQLComparisonOperator.StartsWith, "O");
					nameQuery.AddSubQuery(orgQuery, JoinCondition.And);
					AssertContainsExactElementsInAnyOrder(["Nike-Carrier-Org 2", "Nike--Org 1"], NamedAccountBizosAsString(module.LoadCollectionForTest(nameQuery)));
				}
			}
		}

		#endregion

		static IWiseRatesClientFactory GetMockWiseRatesClientFactoryForTest(IEnumerable<string> namedAccounts)
		{
			var wiseRatesClientMock = new Mock<IWiseRatesClient>();
			wiseRatesClientMock
				.Setup(m => m.GetNamedAccounts(It.IsAny<string>()))
				.Returns(namedAccounts.ToHashSet());

			var wiseRatesClientFactoryMock = new Mock<IWiseRatesClientFactory>();
			wiseRatesClientFactoryMock
				.Setup(f => f.TryCreate(
					It.IsAny<string>(),
					It.IsAny<int>(),
					It.IsAny<CancellationToken>(),
					It.IsAny<ILogger>()))
				.Returns((wiseRatesClientMock.Object, string.Empty));

			return wiseRatesClientFactoryMock.Object;
		}

		static IEnumerable<string> NamedAccountBizosAsString(IEnumerable<OrgCarrierNamedAccount> list)
		{
			return list
				.Select(x =>
					x.ONA_ForeignName + "-" +
					(x.Carrier?.OH_FullName ?? "") + "-" +
					(x.Organization?.OH_FullName ?? ""))
				.OrderBy(x => x);
		}

		class UrsNamedAccountModuleForTest : UrsNamedAccountModule
		{
			public OrgCarrierNamedAccount[] LoadCollectionForTest(ZQuery query)
			{
				var result = PerformSearchCore(typeof(OrgCarrierNamedAccount), query);
				BindSearchResultToGrid(GridCollection, result);
				OnAfterPerformSearchCore();
				return GridCollection.Cast<OrgCarrierNamedAccount>().ToArray();
			}
		}
	}
}


