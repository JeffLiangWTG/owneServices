using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	abstract class WhsDocketFetchStrategyTest<T> : TestCaseWithFactory where T : WhsDocket
	{
		#region TestFetchForLoad

		public void TestFetchForLoad()
		{
			var data = new TestDataSimpleEnvironment(Factory, saveFactory_doNotUseForNewTests: false);
			var gP20 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");

			for (int index = 0; index < 5; index++)
			{
				var postfix = index.ToString();
				var warehouse = Helper.CreateWarehouse("W" + postfix, "R", 2, 1);
				var org = Helper.CreateClient("o" + postfix);
				var part = Helper.CreateProduct(org, "p" + postfix);
				Factory.Save();

				var docket = CreateDocketAndLine(org, warehouse, "D" + postfix, part, 1m);

				var container = Factory.New<WhsDocketContainer>();
				container.WC_WD = docket.PK;
				container.WC_ContainerNum = "Container" + postfix;
				container.WC_RC = gP20.PK;
			}

			Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var dockets = newFactory.Load<T>(new ZQuery(WhsDocketSchema.WD_ExternalReference, SQLComparisonOperator.StartsWith, "0"));

			var expetedDbHits = new Dictionary<string, int>();
			expetedDbHits.Add(WhsDocketSchema.Constants.TableName, 1);

			AssertDbHits(expetedDbHits, newFactory);
		}

		#endregion

		#region Implementation

		protected virtual WhsDocketFetchStrategy GetNewFetchStrategy()
		{
			return new WhsDocketFetchStrategy(Factory.New<T>());
		}

		protected WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}
		WhsTestHelperFunctions helper;

		protected abstract T CreateDocketAndLine(OrgHeader org, WhsWarehouse warehouse, string docketId, OrgSupplierPart part, decimal numberOfUnits);

		#endregion

		public Dictionary<string, int> expectedDbHits { get; set; }
	}
}
