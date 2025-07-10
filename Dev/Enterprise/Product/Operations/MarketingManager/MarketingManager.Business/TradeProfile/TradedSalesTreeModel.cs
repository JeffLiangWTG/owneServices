using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MarketingManager.Business
{
	public class TradedSalesTreeModel : ZTreeModel<TradePeriodGrouping>
	{
		public TradedSalesTreeModel(TradedSalesAnalysis salesAnalysis)
			: base(salesAnalysis.Factory)
		{
			this.salesAnalysis = salesAnalysis;
		}

		readonly TradedSalesAnalysis salesAnalysis;

		#region Properties

		[BusinessObjectTestExclude]
		public IEnumerable<OrgTradePeriod> TradePeriods
		{
			get { return tradePeriods; }
			set
			{
				if (tradePeriods != value)
				{
					tradePeriods = value;
					RebuildNodes();
				}
			}
		}
		IEnumerable<OrgTradePeriod> tradePeriods;

		[BusinessObjectTestExclude]
		public IEnumerable<TradePeriodGrouper> Groupers
		{
			get { return groupers; }
			set
			{
				if (groupers != value)
				{
					groupers = value;
					RebuildNodes();
				}
			}
		}
		IEnumerable<TradePeriodGrouper> groupers;

		#endregion

		#region Build

		void RebuildNodes()
		{
			if (!isRebuildSuspended)
			{
				rootNodes.Clear();
				if (tradePeriods == null || groupers == null || !groupers.Any())
				{
					return;
				}

				foreach (var grouper in groupers)
				{
					grouper.AddFetchHints(tradePeriods);
				}

				foreach (var node in RecursivelyBuildGroupings(tradePeriods, 0).Select(x => (TradedSalesTreeNode)CreateNewNode(x)))
				{
					rootNodes.Add(node);
				}

				OnRebuilt();
			}
			else
			{
				shouldRebuildOnSuspenderDispose = true;
			}
		}

		protected void OnRebuilt()
		{
			if (Rebuilt != null)
			{
				Rebuilt(this, EventArgs.Empty);
			}
		}
		public event EventHandler Rebuilt;

		IEnumerable<TradePeriodGrouping> RecursivelyBuildGroupings(IEnumerable<OrgTradePeriod> currentTradePeriods, int grouperIndex)
		{
			var grouper = Groupers.ElementAt(grouperIndex);
			var childNodes = new List<TradePeriodGrouping>();
			foreach (var groupingItem in grouper.GetGroupings(currentTradePeriods))
			{
				if (grouperIndex == Groupers.Count() - 1)
				{
					yield return new TradePeriodGrouping(salesAnalysis, groupingItem.GroupKey, groupingItem.GroupedTradePeriods, grouper.GetType());
				}
				else
				{
					var childGroupings = RecursivelyBuildGroupings(groupingItem.GroupedTradePeriods, grouperIndex + 1);
					if (childGroupings.Any())
					{
						yield return new TradePeriodGrouping(salesAnalysis, groupingItem.GroupKey, childGroupings, grouper.GetType());
					}
					else
					{
						yield return new TradePeriodGrouping(salesAnalysis, groupingItem.GroupKey, groupingItem.GroupedTradePeriods, grouper.GetType());
					}
				}
			}
		}

		#region Rebuild Suspender

		public IDisposable GetRebuildSuspender()
		{
			isRebuildSuspended = true;
			return new DisposableAction(() =>
			{
				isRebuildSuspended = false;
				if (shouldRebuildOnSuspenderDispose)
				{
					RebuildNodes();
				}

				shouldRebuildOnSuspenderDispose = false;
			});
		}
		bool shouldRebuildOnSuspenderDispose;
		bool isRebuildSuspended;

		#endregion

		#endregion

		#region RootNodes

		protected override ZNodeCollection<TradePeriodGrouping> GetRootNodes()
		{
			return rootNodes;
		}
		readonly TradedSalesTreeNodeCollection rootNodes = new TradedSalesTreeNodeCollection();

		#endregion

		#region New Node

		protected override ZNode<TradePeriodGrouping> CreateNewNodeCore(ZTreeModel<TradePeriodGrouping> treeModel, TradePeriodGrouping bizObj)
		{
			return new TradedSalesTreeNode(this, bizObj);
		}

		#endregion
	}
}
