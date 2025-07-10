namespace Enterprise.Warehouse.Transactions.Business
{
	public enum WhsWorkOrderLineFilterStrategy
	{
		All,
		AssemblyLinesForReceive,
		AssemblyLinesForPick,
		DisassemblyLinesForPick,
		DisassemblyLinesForPutaway,
		AssemblyLinesOrExpanded // default
	}

	public class WhsWorkOrderLineCollection : WhsComponentOrderLineCollection
	{
		public WhsWorkOrderLineCollection(WhsWorkOrder master)
			: this(master, WhsWorkOrderLineFilterStrategy.AssemblyLinesOrExpanded)
		{
		}

		public WhsWorkOrderLineCollection(WhsWorkOrder master, WhsWorkOrderLineFilterStrategy filterStrategy)
			: base(master)
		{
			FilterStrategy = filterStrategy;
		}

		protected override object[] GetCollectionState() => new object[] { FilterStrategy };

		#region Indexer

		public new WhsWorkOrderLine this[int index]
		{
			get { return (WhsWorkOrderLine)(base[index]); }
		}

		#endregion

		#region AddNew / EndNew

		public new WhsWorkOrderLine AddNew()
		{
			return (WhsWorkOrderLine)base.AddNew();
		}

		public new void EndNew(int index)
		{
			base.EndNew(index);
		}

		#endregion

		#region FilterStrategy

		public WhsWorkOrderLineFilterStrategy FilterStrategy
		{
			get { return filterStrategy; }
			private set
			{
				bool strategyChanged = (value != filterStrategy);
				filterStrategy = value;

				if (strategyChanged)
				{
					RefreshAll(Factory);
				}
			}
		}

		WhsWorkOrderLineFilterStrategy filterStrategy;

		#endregion

		#region MatchesFilterCore

		protected override bool MatchesFilterCore(WhsDocketLine element, bool fetchOnlyFromLocalCache)
		{
			var line = (WhsWorkOrderLine)element;
			WhsWorkOrderLine parentLine;
			bool isVisible = true;

			switch (FilterStrategy)
			{
				case WhsWorkOrderLineFilterStrategy.All:
					isVisible = true;
					break;

				case WhsWorkOrderLineFilterStrategy.AssemblyLinesOrExpanded:
					isVisible = line.BOM.IsExpanded || !line.BOM.IsComponent;
					break;

				case WhsWorkOrderLineFilterStrategy.AssemblyLinesForReceive:
					isVisible = line.BOM.IsTopLevelProduct;
					break;

				case WhsWorkOrderLineFilterStrategy.AssemblyLinesForPick:
					parentLine = line.ParentLine;
					isVisible = (parentLine != null && parentLine.WE_WE_ParentDocketLine.IsEmpty);
					break;

				case WhsWorkOrderLineFilterStrategy.DisassemblyLinesForPutaway:
					parentLine = line.ParentLine;
					isVisible = (parentLine != null && parentLine.WE_WE_ParentDocketLine.IsEmpty &&
						parentLine.SupplierPart.OP_CanDisassembleKit &&
						parentLine.SupplierPart.BillOfMaterials.IsReusableComponent(line.SupplierPart, line.WE_F3_NKPackType)); // items like packing tape won't be resuable
					break;

				case WhsWorkOrderLineFilterStrategy.DisassemblyLinesForPick:
					isVisible = line.BOM.IsTopLevelProduct && line.SupplierPart.OP_CanDisassembleKit;
					break;
			}

			return isVisible && base.MatchesFilterCore(element, fetchOnlyFromLocalCache);
		}

		#endregion

		#region BOA/BOD eDoc Printing Support

		internal void ResetSupplierPartDocManagerInfo()
		{
			foreach (WhsWorkOrderLine item in this)
			{
				item.ResetSupplierPartDocManagerInfo();
			}
		}

		#endregion
	}
}
