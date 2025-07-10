using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Aga.Controls.Tree;
using Aga.Controls.Tree.NodeControls;
using CargoWise.Windows.UI;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	public partial class TradedSalesAnalysisTreeControl : ZUserControl
	{
		public TradedSalesAnalysisTreeControl()
		{
			InitializeComponent();

			foreach (var column in tree.Columns)
			{
				column.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(column.Width);
			}

			descriptionColumn.Header = Res.GetString("d6e1f46e-f961-43e5-bc6b-38029e54f9cf", "Description");
			grossRevenueColumn.Header = Res.GetString("cc328ef8-efb3-43ba-92d7-a45798c5eb70", "Revenue");
			jobRevenueColumn.Header = Res.GetString("d9f13b79-ba3e-4543-bd61-2fdb0a095bf8", "Job Revenue");
			jobCostColumn.Header = Res.GetString("53df2147-e8d5-4667-882b-37dc51d110e0", "Job Cost");
			jobProfitColumn.Header = Res.GetString("2ba5e9be-0f58-47b8-9a82-f51741cf21c3", "Job Profit");
			revenueCurrencyColumn.Header = Res.GetString("516b2b65-3e78-421c-b227-6b91da568aa3", "Curr.");

			unitCountColumn.Header = Res.GetString("0d3b6b69-1ba3-4a7a-8060-5636951b03f7", "Unit Count");
			palletCountColumn.Header = Res.GetString("a21de6ae-001f-497a-af8d-e3755347a010", "Pallet Count");
			lineCountColumn.Header = Res.GetString("b0e7afad-9137-43b2-a23d-e8957642b1ce", "Line Count");
			grossWeightColumn.Header = Res.GetString("c0086a43-ff3c-4d46-97fc-d99038428e87", "G. Weight");
			grossWeightUnitsColumn.Header = Res.GetString("e38cc911-6ac3-4e3b-aaf3-6d68b1d26411", "Units");
			netVolumeColumn.Header = Res.GetString("e0effcae-3d2c-4777-aae7-971fffd85e70", "Net Vol");
			netVolumeUnitsColumn.Header = Res.GetString("e94696b4-65ef-45bb-a678-099cc28b6a21", "Units");

			supplierColumn.Header = Res.GetString("bfa5e183-2501-48e9-8ed8-01ea11f99989", "Supplier");
			buyerColumn.Header = Res.GetString("0b695230-3e7b-4725-89a3-0b91bd0722ae", "Buyer");
			lastJobRegistrationColumn.Header = Res.GetString("367d5d8f-90bd-4df6-a499-7e9de6554d8d", "Last Job Registration");

			teuQuantityColumn.Header = Res.GetString("b9e1b6a5-891e-4814-a572-6de2c38a6b59", "TEU");

			RefreshJobRevenueColumnVisibility();

			descriptionNodeTextBox.DrawText += DescriptionNodeTextBox_DrawText;
#if !WINZOR
			tree.LineColourDeciding += Tree_LineColourDeciding;
#endif
			tree.SortPropertiesNeeded += Tree_SortPropertiesNeeded;
		}

		protected IEnumerable<TreeColumn> TreeColumns
		{
			get { return tree.Columns; }
		}

		#region DataBinding

		public new TradePeriodGrouping CurrentDataItem
		{
			get { return (TradePeriodGrouping)base.CurrentDataItem; }
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			if (CurrentDataItem != null)
			{
				CurrentDataItem.SubTree.Rebuilt -= SubTree_Rebuilt;

				ExpandedTreeNodes expandedTreeNodes;
				if (!ExpandedTreeNodesForNodes.TryGetValue(CurrentDataItem, out expandedTreeNodes))
				{
					expandedTreeNodes = new ExpandedTreeNodes();
					ExpandedTreeNodesForNodes[CurrentDataItem] = expandedTreeNodes;
				}

				expandedTreeNodes.Store(tree);
			}
			tree.Model = null;

			base.OnCurrentDataItemChanging(e);
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			if (CurrentDataItem != null)
			{
				CurrentDataItem.SubTree.Rebuilt += SubTree_Rebuilt;

				tree.Model = new TradedSalesTreeModelView(CurrentDataItem.SubTree);
				tree.SetSortColumn(descriptionColumn, SortOrder.Ascending);

				ExpandedTreeNodes expandedTreeNodes;
				if (ExpandedTreeNodesForNodes.TryGetValue(CurrentDataItem, out expandedTreeNodes))
				{
					expandedTreeNodes.Restore(tree);
				}
			}
		}

		void SubTree_Rebuilt(object sender, EventArgs e)
		{
			tree.Model = new TradedSalesTreeModelView(CurrentDataItem.SubTree);
			tree.SetSortColumn(descriptionColumn, SortOrder.Ascending);
		}

		void Tree_SortPropertiesNeeded(object sender, ZTreeViewSortEventArgs e)
		{
			if (e.Column == grossRevenueColumn)
			{
				e.SortProperties = new List<Tuple<string, SortOrder>>
				{
					Tuple.Create("GrossRevenueAsZDecimal", e.SortOrder)
				};
			}
			else if (e.Column == jobRevenueColumn)
			{
				e.SortProperties = new List<Tuple<string, SortOrder>>
				{
					Tuple.Create("JobRevenueAsZDecimal", e.SortOrder)
				};
			}
			else if (e.Column == jobCostColumn)
			{
				e.SortProperties = new List<Tuple<string, SortOrder>>
				{
					Tuple.Create("JobCostAsZDecimal", e.SortOrder)
				};
			}
			else if (e.Column == jobProfitColumn)
			{
				e.SortProperties = new List<Tuple<string, SortOrder>>
				{
					Tuple.Create("JobProfitAsZDecimal", e.SortOrder)
				};
			}
			else if (e.Column == grossWeightColumn)
			{
				e.SortProperties = new List<Tuple<string, SortOrder>>
				{
					Tuple.Create("GrossWeightAsZDecimal", e.SortOrder)
				};
			}
			else if (e.Column == netVolumeColumn)
			{
				e.SortProperties = new List<Tuple<string, SortOrder>>
				{
					Tuple.Create("NetVolumeAsZDecimal", e.SortOrder)
				};
			}
			else if (e.Column == teuQuantityColumn)
			{
				e.SortProperties = new List<Tuple<string, SortOrder>>
				{
					Tuple.Create("TEUQuantityAsZDecimal", e.SortOrder)
				};
			}
			else if (e.Column == lastJobRegistrationColumn)
			{
				e.SortProperties = new List<Tuple<string, SortOrder>>
				{
					Tuple.Create("LastJobRegistrationAsZDateTime", e.SortOrder)
				};
			}
		}
		#endregion

		#region Properties

		#region ProductCode

		public string ProductCode
		{
			get { return productCode; }
			set
			{
				productCode = value;
				unitCountColumn.IsVisible = IsWarehouse;
				palletCountColumn.IsVisible = IsWarehouse;
				lineCountColumn.IsVisible = IsWarehouse;
			}
		}
		string productCode;

		bool IsWarehouse
		{
			get { return productCode == SystemDefinedSalesProductList.Codes.Warehouse; }
		}

		#endregion

		#region ShowJobValueColumn

		[DefaultValue(false)]
		public bool ShowJobValueColumn
		{
			get { return showJobValueColumn; }
			set
			{
				if (showJobValueColumn != value)
				{
					showJobValueColumn = value;
					RefreshJobRevenueColumnVisibility();
				}
			}
		}
		bool showJobValueColumn;

		void RefreshJobRevenueColumnVisibility()
		{
			JobRevenueColumn.IsVisible = ShowJobValueColumn;
			JobCostColumn.IsVisible = ShowJobValueColumn;
			JobProfitColumn.IsVisible = ShowJobValueColumn;
		}

		protected TreeColumn JobRevenueColumn
		{
			get { return jobRevenueColumn; }
		}

		protected TreeColumn JobCostColumn
		{
			get { return jobCostColumn; }
		}

		protected TreeColumn JobProfitColumn
		{
			get { return jobProfitColumn; }
		}

		#endregion

		#region ShowTEUQuantityColumn
		public void SetTEUQuantityColumn()
		{
			TEUQuantityColumn.IsVisible =
				ProductCode == SystemDefinedSalesProductList.Codes.ForwardingShipment ||
				ProductCode == SystemDefinedSalesProductList.Codes.LinerAgency;
		}

		protected TreeColumn TEUQuantityColumn
		{
			get { return teuQuantityColumn; }
		}
		#endregion

		#endregion

		#region Graphics

#if !WINZOR

		void Tree_LineColourDeciding(object sender, TreeViewAdv.LineColourDecidingEventArgs e)
		{
			if (e.Node.IsLeaf)
			{
				e.Pen.Color = Color.Transparent;
			}
		}

#endif

		void DescriptionNodeTextBox_DrawText(object sender, DrawEventArgs e)
		{
			e.Font = new Font(e.Font, FontStyle.Bold);
		}

		#endregion

		#region ExpandTreeNodes

		readonly Dictionary<TradePeriodGrouping, ExpandedTreeNodes> ExpandedTreeNodesForNodes = new Dictionary<TradePeriodGrouping, ExpandedTreeNodes>();

		#endregion

		#region Classes

		public class SalesAnalysisNodeTextBox : NodeTextBox
		{
			public override object GetValue(TreeNodeAdv node)
			{
				if (ShouldHide(node))
				{
					return string.Empty;
				}
				return base.GetValue(node);
			}

			bool ShouldHide(TreeNodeAdv node)
			{
				var isSupplierBuyerGroupingNode = IsSupplierBuyerGroupingNode(node);
				if (DataPropertyName == AutoTradePeriodGrouping.Schema.LastJobRegistration)
				{
					return
						node.IsExpanded
						&& !node.IsLeaf; // some reason the leaf node can be expanded
				}
				else if (DataPropertyName == AutoTradePeriodGrouping.Schema.Description)
				{
					return isSupplierBuyerGroupingNode;
				}
				else if (DataPropertyName == AutoTradePeriodGrouping.Schema.SupplierCode || DataPropertyName == AutoTradePeriodGrouping.Schema.BuyerCode)
				{
					return !isSupplierBuyerGroupingNode;
				}
				if (isSupplierBuyerGroupingNode)
				{
					return true;
				}

				if (IsWarehouseServiceGrouped(node, OrgSalesWarehouseServiceTypesList.Codes.Orders, OrgSalesWarehouseServiceTypesList.Codes.Receipts))
				{
					var tradeAnalysis = GetTradedSalesAnalysis(node);
					if (TradePeriodGrouping.GetWarehouseOrdAndRecColumnsThatHaveDifferentJobAndSupplierPartTotals(tradeAnalysis.Factory).Contains(DataPropertyName))
					{
						return
							DataPropertyName == AutoTradePeriodGrouping.Schema.Count
							&& tradeAnalysis.MainGroupingType == SalesAnalysisMainGroupingTypeList.Codes.Product
							&& ((TradedSalesTreeNode)node.Tag).BizObjForBinding.SupplierPartPk.IsEmpty;
					}
					else if (TradePeriodGrouping.GetWarehouseOrdAndRecColumnsThatArePerJobRatherThanSupplierPart(tradeAnalysis.Factory).Contains(DataPropertyName))
					{
						return
							tradeAnalysis.MainGroupingType == SalesAnalysisMainGroupingTypeList.Codes.Product
							|| HasBeenGroupedBy(node, typeof(TradePeriodSupplierPartGrouper));
					}
				}
				else if (IsWarehouseServiceGrouped(node, OrgSalesWarehouseServiceTypesList.Codes.Storage))
				{
					var tradeAnalysis = GetTradedSalesAnalysis(node);
					var columns = TradePeriodGrouping.GetWarehouseStgUnusedColumns(tradeAnalysis.Factory);
					if (columns.Contains(DataPropertyName))
					{
						return true;
					}
				}

				return
					(node.IsExpanded && !node.IsLeaf)
					&& (!node.Children.Any() || !IsSupplierBuyerGroupingNode(node.Children.First()));
			}

			static TradedSalesAnalysis GetTradedSalesAnalysis(TreeNodeAdv node)
			{
				var detailGrouping = ((TradedSalesTreeNode)node.Tag).BizObjForBinding;
				return detailGrouping.SalesAnalysis;
			}

			static bool HasBeenGroupedBy(TreeNodeAdv node, Type grouperType)
			{
				if (GetNodeGrouperType(node) == grouperType)
				{
					return true;
				}
				if (node.Parent != null)
				{
					return HasBeenGroupedBy(node.Parent, grouperType);
				}
				return false;
			}

			static bool HasChildGrouper(TreeNodeAdv node, Type grouperType)
			{
				foreach (var child in node.Children)
				{
					if (GetNodeGrouperType(child) == grouperType)
					{
						return true;
					}
					if (HasChildGrouper(child, grouperType))
					{
						return true;
					}
				}

				return false;
			}

			static bool IsWarehouseServiceGrouped(TreeNodeAdv node, params string[] serviceTypesToCheck)
			{
				var detailGrouping = ((TradedSalesTreeNode)node?.Tag)?.BizObjForBinding;
				if (detailGrouping == null || detailGrouping.SalesAnalysis.SalesHeader.SalesProductCode != SystemDefinedSalesProductList.Codes.Warehouse)
				{
					// not a warehouse product
					return false;
				}

				var tradeAnalysis = detailGrouping.SalesAnalysis;
				if ((tradeAnalysis.MainGroupingType == SalesAnalysisMainGroupingTypeList.Codes.Service || HasBeenGroupedBy(node, typeof(TradePeriodServiceGrouper)))
					&& (serviceTypesToCheck.Any(t => t == detailGrouping.Service)))
				{
					// has been grouped by the specified seviceTypesToCheck
					return true;
				}

				if (HasChildGrouper(node, typeof(TradePeriodServiceGrouper)))
				{
					// children have been service grouped (therefore possible to be grouped by the specified serviceTypesToCheck)
					return true;
				}

				return false;
			}

			static Type GetNodeGrouperType(TreeNodeAdv node)
			{
				var treeNode = (TradedSalesTreeNode)node.Tag;
				if (treeNode == null)
				{
					return null;
				}

				return treeNode.BizObjForBinding.TradePeriodGrouperType;
			}

			static bool IsSupplierBuyerGroupingNode(TreeNodeAdv node)
			{
				return GetNodeGrouperType(node) == typeof(TradePeriodSupplierAndBuyerGrouper);
			}
		}

		public class ExpandedTreeNodes
		{
			public void Store(ZTreeViewAdv tree)
			{
				list.Clear();

				foreach (var child in tree.Root.Children)
				{
					StoreExpandedNodeTagsRecursive(child);
				}
			}

			void StoreExpandedNodeTagsRecursive(TreeNodeAdv node)
			{
				if (node.IsExpanded && node.Tag != null)
				{
					list.Add(node.Tag);

					foreach (var child in node.Children)
					{
						StoreExpandedNodeTagsRecursive(child);
					}
				}
			}

			public void Restore(ZTreeViewAdv tree)
			{
				foreach (var child in tree.Root.Children)
				{
					RestoreExpandedNodeTagsRecursive(child);
				}
			}

			void RestoreExpandedNodeTagsRecursive(TreeNodeAdv node)
			{
				if (node.Tag != null && list.Contains(node.Tag))
				{
					node.Expand();

					foreach (var child in node.Children)
					{
						RestoreExpandedNodeTagsRecursive(child);
					}
				}
			}

			readonly HashSet<object> list = new HashSet<object>();
		}

		#endregion
	}
}
