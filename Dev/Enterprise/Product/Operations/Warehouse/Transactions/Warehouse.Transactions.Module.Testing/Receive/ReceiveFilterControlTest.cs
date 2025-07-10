using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	public class ReceiveFilterControlTest : WhsFilterControlDBHitsTestCase<WhsReceiveCollection, ReceiveFilterBusinessObject>
	{
		#region TestNewZFilterStrip

		public void TestNewZFilterStrip()
		{
			var collection = new WhsReceiveCollection(Factory);
			var filterBizO = new ReceiveFilterBusinessObject();
			using (var filterControl = GetNewFilterControl(collection, filterBizO))
			{
				var strip = filterBizO.FilterStrips.AddNew();
				using (var filterStrip = filterControl.AddFilterStrip(strip))
				{
					AssertType<WhsWorkflowFilterStrip>(filterStrip);
				}
			}
		}

		#endregion

		#region GetNewFilterStripControl

		protected override ZFilterStripControl GetNewFilterStripControl()
		{
			var receives = new WhsReceiveCollection(Factory);
			var filterBO = new ReceiveFilterBusinessObject();
			return new ReceiveFilterControl(receives, filterBO);
		}

		protected override void SetupData()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			for (int index = 0; index < 5; index++)
			{
				var postfix = index.ToString();
				var client = Helper.CreateClient("O" + postfix);
				var part1 = Helper.CreateProduct(client, "p1" + postfix);
				var part2 = Helper.CreateProduct(client, "p2" + postfix);
				var warehouse = Helper.CreateWarehouse("W" + postfix, "L" + postfix);
				var receive = Helper.CreateWhsReceive(client, warehouse, "R" + postfix);
				Helper.CreateWhsReceiveInventoryLine(receive, part1, 1m, warehouse.DefaultLocation);
				Helper.CreateWhsReceiveInventoryLine(receive, part2, 1m, warehouse.DefaultLocation);
				receive.FinaliseDocketWithoutUserConfirmation();
				Assert(receive.IsFinalised);
			}
			Factory.Save();
		}

		protected override Dictionary<string, int> GetBaseHits()
		{
			var baseHits = new Dictionary<string, int>();
			baseHits.Add(WhsDocketSchema.Constants.TableName, 1);
			return baseHits;
		}

		protected override Dictionary<string, Dictionary<string, int>> GetExpectedHitsDictionary()
		{
			var baseHits = GetBaseHits();
			var hitsDictionary = new Dictionary<string, Dictionary<string, int>>();

			var clientHits = new Dictionary<string, int>();
			clientHits.Add(OrgHeaderSchema.Constants.TableName, 1);
			clientHits.Add(WhsDocketSchema.Constants.TableName, 1);
			hitsDictionary.Add(nameof(WhsReceive.WD_OH_Client), clientHits);

			var warehouseHits = new Dictionary<string, int>(baseHits);
			warehouseHits.Add(WhsWarehouseSchema.Constants.TableName, 1);
			hitsDictionary.Add(nameof(WhsReceive.WD_WW_Whs), warehouseHits);

			var transportCoNameOrPKHits = new Dictionary<string, int>(baseHits);
			transportCoNameOrPKHits.Add(JobDocAddressSchema.Constants.TableName, 1);
			hitsDictionary.Add(nameof(WhsReceive.TransportCoNameOrPK), transportCoNameOrPKHits);

			var totalUnitsHits = new Dictionary<string, int>(baseHits);
			totalUnitsHits.Add(WhsDocketLineSchema.Constants.TableName, 1);
			hitsDictionary.Add(nameof(WhsReceive.WD_TotalUnitsFromLines), totalUnitsHits);

			var expectedQuantityHits = new Dictionary<string, int>(baseHits);
			expectedQuantityHits.Add(WhsDocketLineSchema.Constants.TableName, 1);
			hitsDictionary.Add(nameof(WhsReceive.WD_TotalExpectedQuantity), expectedQuantityHits);

			var totalPalletsReceivedHits = new Dictionary<string, int>(baseHits);
			totalPalletsReceivedHits.Add(WhsDocketLineSchema.Constants.TableName, 1);
			hitsDictionary.Add(nameof(WhsReceive.TotalPalletsReceived), totalPalletsReceivedHits);

			var containerIDHits = new Dictionary<string, int>(baseHits);
			containerIDHits.Add(WhsDocketContainerSchema.Constants.TableName, 1);
			hitsDictionary.Add(nameof(WhsReceive.ContainerID), containerIDHits);

			var containerTypeHits = new Dictionary<string, int>(baseHits);
			containerTypeHits.Add(WhsDocketContainerSchema.Constants.TableName, 1);
			hitsDictionary.Add(nameof(WhsReceive.ContainerType), containerTypeHits);

			var receiveGoodsHandlingInstructionsHits = new Dictionary<string, int>(baseHits);
			receiveGoodsHandlingInstructionsHits.Add(OrgHeaderSchema.Constants.TableName, 1);
			receiveGoodsHandlingInstructionsHits.Add(StmNoteSchema.Constants.TableName, 1);
			hitsDictionary.Add(nameof(WhsReceive.ReceiveGoodsHandlingInstructions), receiveGoodsHandlingInstructionsHits);

			var productCountHits = new Dictionary<string, int>(baseHits);
			productCountHits.Add(WhsDocketLineSchema.Constants.TableName, 1);
			hitsDictionary.Add(nameof(WhsReceive.ProductCount), productCountHits);

			var transportCoNameHits = new Dictionary<string, int>(baseHits);
			transportCoNameHits.Add(JobDocAddressSchema.Constants.TableName, 1);
			hitsDictionary.Add(nameof(WhsReceive.TransportCoName), transportCoNameHits);

			var supplierNameHits = new Dictionary<string, int>(baseHits);
			supplierNameHits.Add(JobDocAddressSchema.Constants.TableName, 1);
			hitsDictionary.Add(nameof(WhsReceive.SupplierName), supplierNameHits);

			var clientNameHits = new Dictionary<string, int>(baseHits);
			clientNameHits.Add(OrgHeaderSchema.Constants.TableName, 1);
			hitsDictionary.Add(nameof(WhsReceive.ClientName), clientNameHits);

			var vehicleNoHits = new Dictionary<string, int>(baseHits);
			vehicleNoHits.Add(WhsDocketReferenceSchema.Constants.TableName, 1);
			hitsDictionary.Add(nameof(WhsReceive.VehicleNo), vehicleNoHits);

			var supplierDocAddressHits = new Dictionary<string, int>(baseHits);
			supplierDocAddressHits.Add(JobDocAddressSchema.Constants.TableName, 1);
			hitsDictionary.Add(nameof(WhsReceive.SupplierDocAddress) + "+" + nameof(JobDocAddress.OrganisationNameOrPK), supplierDocAddressHits);

			var workflowItemsHits = new Dictionary<string, int>(baseHits);
			workflowItemsHits.Add(ProcessTasksSchema.Constants.TableName, 1);
			hitsDictionary.Add(nameof(WhsReceive.WorkflowItems) + "+" + nameof(ProcessTaskCollection.Milestones) + "+" + nameof(MilestoneCollectionView.LastMilestone) + "+" + nameof(ProcessTask.P9_SE_NKMilestoneEvent), workflowItemsHits);
			hitsDictionary.Add(nameof(WhsReceive.WorkflowItems) + "+" + nameof(ProcessTaskCollection.Milestones) + "+" + nameof(MilestoneCollectionView.LastMilestone) + "+" + nameof(ProcessTask.DescriptionWithReference), workflowItemsHits);
			hitsDictionary.Add(nameof(WhsReceive.WorkflowItems) + "+" + nameof(ProcessTaskCollection.Milestones) + "+" + nameof(MilestoneCollectionView.NextMilestone) + "+" + nameof(ProcessTask.P9_SE_NKMilestoneEvent), workflowItemsHits);
			hitsDictionary.Add(nameof(WhsReceive.WorkflowItems) + "+" + nameof(ProcessTaskCollection.Milestones) + "+" + nameof(MilestoneCollectionView.NextMilestone) + "+" + nameof(ProcessTask.DescriptionWithReference), workflowItemsHits);
			hitsDictionary.Add(nameof(WhsReceive.WorkflowItems) + "+" + nameof(ProcessTaskCollection.Milestones) + "+" + nameof(MilestoneCollectionView.NextMilestone) + "+" + nameof(ProcessTask.P9_ScheduledDateForBinding), workflowItemsHits);
			hitsDictionary.Add(nameof(WhsReceive.WorkflowItems) + "+" + nameof(ProcessTaskCollection.Milestones) + "+" + nameof(MilestoneCollectionView.LastMilestone) + "+" + nameof(ProcessTask.P9_ActualDateForBinding), workflowItemsHits);

			var jhHits = new Dictionary<string, int>(baseHits);
			jhHits.Add(JobHeaderSchema.Constants.TableName, 1);
			hitsDictionary.Add(nameof(WhsReceive.JobHeader) + "+" + nameof(WhsReceive.JobHeader.JH_ProfitLossReasonCode), jhHits);
			hitsDictionary.Add(nameof(WhsReceive.JobHeader) + "+" + nameof(WhsReceive.JobHeader.JH_TotalProfitRevenueMargin), jhHits);

			return hitsDictionary;
		}

		protected override WhsReceiveCollection GetNewCollection(BusinessObjectFactory factory)
		{
			return new WhsReceiveCollection(factory);
		}

		protected override ReceiveFilterBusinessObject GetNewFilterBusinessObject() => new ReceiveFilterBusinessObject();

		protected override ZFilterStripControl GetNewFilterControl(WhsReceiveCollection collection, ReceiveFilterBusinessObject filterBizO)
		{
			return new ReceiveFilterControl(collection, filterBizO);
		}

		public void TestReceiveCategory()
		{
			using (var form = new ZForm())
			using (var receiveFilterControl = GetNewFilterStripControl())
			{
				form.Controls.Add(receiveFilterControl);
				form.Show();
				var styles = receiveFilterControl.FindSingle<ZGrid>().ColumnStyles.Cast<ZGridColumnInfo>();

				var receiveCategory = styles.SingleOrDefault(s => s.ColumnName == WhsDocketSchema.WD_ReceiveCategory.Name);

				AssertNotEquals("receiveCategory should be available", "available", !receiveCategory.IsUnavailable);
			}
		}

		public void TestProfitLossReasonColumn()
		{
			using (var filterControl = GetNewFilterStripControl())
			{
				var profitLossReason = nameof(WhsReceive.JobHeader) + "+" + nameof(WhsReceive.JobHeader.JH_ProfitLossReasonCode);
				var column = filterControl.FilteredGrid.GetColumnStyle(profitLossReason);
				AssertNotNull("Profit/Loss Reason column", column);
				Assert("Profit/Loss Reason column should be hidden by default", !column.IsVisible);
			}
		}

		public void TestMarginColumn()
		{
			using (var filterControl = GetNewFilterStripControl())
			{
				var margin = nameof(WhsReceive.JobHeader) + "+" + nameof(WhsReceive.JobHeader.JH_TotalProfitRevenueMargin);
				var column = filterControl.FilteredGrid.GetColumnStyle(margin);
				AssertNotNull("Margin% column", column);
				Assert("Margin% column should be hidden by default", !column.IsVisible);
			}
		}

		public void TestTaskPlanningStatusColumn()
		{
			var taskPlanningStatus = nameof(WhsReceive.WD_TaskPlanningStatus);

			using (WarehouseDataRegistry.Instance.ExposeWarehouseTaskManagement.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var filterControl = GetNewFilterStripControl())
			{
				var column = filterControl.FilteredGrid.GetColumnStyle(taskPlanningStatus);
				AssertNotNull("Task Planning Status column", column);
				Assert("Task Planning Status column should be unavailable", column.IsUnavailable);
			}

			using (WarehouseDataRegistry.Instance.ExposeWarehouseTaskManagement.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var filterControl = GetNewFilterStripControl())
			{
				var column = filterControl.FilteredGrid.GetColumnStyle(taskPlanningStatus);
				AssertNotNull("Task Planning Status column", column);
				Assert("Task Planning Status column should not be unavailable", !column.IsVisible);
			}
		}

		#endregion

		#region WorkflowDescriptorCode

		protected override string WorkflowDescriptorCode => WorkflowDescriptors.WhsReceiveWorkflowDescriptorCode;

		#endregion

		protected override IEnumerable<string> GetTableNamesToIgnoreForUnusedFetchHints(string columnName)
		{
			var tableNamesToIgnore = base.GetTableNamesToIgnoreForUnusedFetchHints(columnName);
			if (columnName == nameof(WhsReceive.WD_TotalUnitsFromLines)
				|| columnName == nameof(WhsReceive.WD_TotalExpectedQuantity)
				|| columnName == nameof(WhsReceive.TotalPalletsReceived)
				|| columnName == nameof(WhsReceive.ProductCount))
			{
				tableNamesToIgnore = tableNamesToIgnore.Append(
					new[]
					{
						OrgSupplierPartSchema.Constants.TableName,
						WhsLocationViewSchema.Constants.TableName,
						GenAddOnColumnSchema.Constants.TableName
					});
			}
			else if (columnName.StartsWith(nameof(WhsReceive.TransportCo), StringComparison.Ordinal))
			{
				tableNamesToIgnore = tableNamesToIgnore.Append(JobHeaderSchema.Constants.TableName);
			}
			else if (columnName == nameof(WhsReceive.ReceiveGoodsHandlingInstructions))
			{
				tableNamesToIgnore = tableNamesToIgnore.Append(OrgCompanyDataSchema.Constants.TableName);
			}

			return tableNamesToIgnore;
		}
	}
}
