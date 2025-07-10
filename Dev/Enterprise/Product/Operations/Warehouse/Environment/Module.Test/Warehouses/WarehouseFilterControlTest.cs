using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Module.Testing
{
	class WarehouseFilterControlTest : TestCaseWithFactory
	{
		#region TestReleaseGroupColumnVisibility

		public void TestReleaseGroupColumnVisibility_TaskManagementEnabled()
			=> TestReleaseGroupColumnVisibility(taskManagementEnabled: true);

		public void TestReleaseGroupColumnVisibility_TaskManagementDisabled()
			=> TestReleaseGroupColumnVisibility(taskManagementEnabled: false);

		void TestReleaseGroupColumnVisibility(bool taskManagementEnabled)
		{
			using (WarehouseDataRegistry.Instance.ExposeWarehouseTaskManagement.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, taskManagementEnabled))
			using (var form = new ZForm())
			using (var control = new WarehouseFilterControl(new WhsWarehouseCollection(Factory), new WarehouseFilterBusinessObject()))
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals(taskManagementEnabled, !control.Grid.ColumnStyles.Cast<ZGridColumnInfo>().Single(c => c.ColumnName == WhsWarehouseSchema.Constants.WW_GG_ReleaseGroup).IsUnavailable);
			}
		}

		#endregion
	}

	class WarehouseFilterControlDbHitsTest : WhsEnvFilterControlDBHitsTestCase<WhsWarehouseCollection, WarehouseFilterBusinessObject>
	{
		protected override Dictionary<string, int> GetBaseHits()
		{
			var dict = new Dictionary<string, int>();
			dict.Add(WhsWarehouseSchema.Constants.TableName, 1);
			return dict;
		}

		protected override bool ShouldCheckForUnusedFetchHints(string columnName) => false;

		protected override Dictionary<string, Dictionary<string, int>> GetExpectedHitsDictionary()
		{
			var baseHits = GetBaseHits();
			var dict = new Dictionary<string, Dictionary<string, int>>();
			dict.Add(WhsWarehouseSchema.Constants.WW_OA_WarehouseAddress, new Dictionary<string, int>(baseHits) { { OrgAddressSchema.Constants.TableName, 1 } });
			dict.Add(nameof(WhsWarehouse.CountryCode), new Dictionary<string, int>(baseHits) { { OrgAddressSchema.Constants.TableName, 1 } });
			dict.Add(WhsWarehouseSchema.Constants.WW_GB_RelatedCompanyBranch, new Dictionary<string, int>(baseHits) { { GlbBranchSchema.Constants.TableName, 1 } });
			dict.Add(WhsWarehouseSchema.Constants.WW_GG_ReleaseGroup, new Dictionary<string, int>(baseHits) { { GlbGroupSchema.Constants.TableName, 1 } });
			dict.Add(nameof(WhsWarehouse.IsWarehouseFreeStoreEnabled), new Dictionary<string, int>(baseHits) { { WhsAreaSchema.Constants.TableName, 1 } });
			dict.Add(nameof(WhsWarehouse.IsWarehouseBondEnabled), new Dictionary<string, int>(baseHits) { { WhsAreaSchema.Constants.TableName, 1 } });
			dict.Add(nameof(WhsWarehouse.IsWarehouseExciseEnabled), new Dictionary<string, int>(baseHits) { { WhsAreaSchema.Constants.TableName, 1 } });
			dict.Add(nameof(WhsWarehouse.IsInwardProcessingEnabled), new Dictionary<string, int>(baseHits) { { WhsAreaSchema.Constants.TableName, 1 } });
			return dict;
		}

		protected override WhsWarehouseCollection GetNewCollection(BusinessObjectFactory factory) => new WhsWarehouseCollection(factory);
		protected override WarehouseFilterBusinessObject GetNewFilterBusinessObject() => new WarehouseFilterBusinessObject();
		protected override ZFilterStripControl GetNewFilterControl(WhsWarehouseCollection collection, WarehouseFilterBusinessObject filterBizO) => new WarehouseFilterControl(collection, filterBizO);

		protected override void SetupData()
		{
			const int setsOfWarehouses = 3;

			WarehouseDataRegistry.Instance.ExposeWarehouseTaskManagement.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			for (var i = 0; i < setsOfWarehouses; i++)
			{
				var releaseGroup = Helper.CreateReleaseGroup($"RG{i}", $"RG{i}");
				var productWarehouse = Helper.CreateWarehouse($"PW{i}");
				productWarehouse.WW_WarehouseType = WarehouseTypes.Codes.Product;
				productWarehouse.WW_GG_ReleaseGroup = releaseGroup.PK;

				var transitWhs = Helper.CreateWarehouse($"TW{i}");
				transitWhs.WW_WarehouseType = WarehouseTypes.Codes.Transit;

				var containerYard = Helper.CreateWarehouse($"CY{i}");
				containerYard.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;
			}

			Factory.Save();
		}
	}
}
