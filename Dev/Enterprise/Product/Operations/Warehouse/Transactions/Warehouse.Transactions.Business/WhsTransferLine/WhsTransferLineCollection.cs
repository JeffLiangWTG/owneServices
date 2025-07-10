using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsTransferLineCollection : WhsDocketLineCollection
	{
		public WhsTransferLineCollection(WhsTransfer master)
			: base(master)
		{
		}

		public WhsTransferLineCollection(WhsTransfer master, ZQuery filter)
			: base(master, filter)
		{
		}

		public WhsTransferLineCollection(WhsTransferLine master)
			: base(master, new ZQuery(WhsDocketLineSchema.WE_IsOriginalInventory, true), WhsDocketLineSchema.WE_WE_MatchingLine)
		{
		}

		public new WhsTransferLine this[int index]
		{
			get { return (WhsTransferLine)(base[index]); }
		}

		public virtual new WhsTransferLine AddNew()
		{
			return (WhsTransferLine)base.AddNew();
		}

		#region WhsDocketLineCollection Overrides

		// Tested: TestFinaliseTransferDbHits in Enterprise.Warehouse.Transactions.GUI.Testing.TransferEntryFormTest
		protected override void OnLoadingIntoCollectionCore(WhsDocketLine loadingObject)
		{
			Factory.AddFetchHint(WhsPickLineSchema.WZ_WE_TransactionLine, loadingObject.PK);
			base.OnLoadingIntoCollectionCore(loadingObject);
		}

		#endregion

		#region IBindingList Members

		protected override bool AllowNew => base.AllowNew && Transfer.IsMasterTransfer && !Transfer.IsAutoCreatedTransfer && !Transfer.IsReadyForPlanningOrPlanned;

		#endregion

		#region Implementations

		WhsTransfer Transfer
		{
			get { return (WhsTransfer)Docket; }
		}

		#endregion
	}
}
