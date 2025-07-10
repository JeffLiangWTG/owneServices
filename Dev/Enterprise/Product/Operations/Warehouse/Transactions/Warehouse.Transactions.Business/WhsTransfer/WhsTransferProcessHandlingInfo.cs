using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business.EventManagement;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	class WhsTransferProcessHandlingInfo : ProcessHandlingInfo
	{
		public WhsTransferProcessHandlingInfo(WhsTransfer transfer)
			: base(transfer)
		{
		}

		#region PopulateCascadingTargets

		protected override IEnumerable<CascadingLink> PopulateCascadingTargets(IStmALog logBeingAdded)
		{
			return Enumerable.Empty<CascadingLink>();
		}

		#endregion

		#region PopulatePropagationTargets

		protected override IEnumerable<PropagationLink> PopulatePropagationTargets()
		{
			if (Transfer.IsDeleted || !Transfer.IsFinalised)
			{
				yield break;
			}

			var vasOrder = GetVASOrder();
			if (vasOrder == null)
			{
				yield break;
			}

			bool isTransferIn = vasOrder.WVO_WD_TransferIntoServiceArea == Transfer.PK;
			yield return new PropagationLink(vasOrder, Enumerable.Empty<WhsVASOrder>(),
				isTransferIn ? (NoResString)"Transfer In" : (NoResString)"Transfer Out");
		}

		WhsVASOrder GetVASOrder()
		{
			var query = new ZQuery(WhsVASOrderSchema.WVO_WD_TransferIntoServiceArea, Transfer.PK);
			query.AddToFilter(JoinCondition.Or, WhsVASOrderSchema.WVO_WD_TransferOutOfServiceArea, Transfer.PK);
			return Transfer.Factory.LoadTop1<WhsVASOrder>(query);
		}

		protected override bool IsEventLogApplicableForPropagation(IStmALog logBeingAdded)
		{
			return base.IsEventLogApplicableForPropagation(logBeingAdded) && logBeingAdded.SL_SE_NKEvent.EqualsIgnoringCase(Events.ItemDocumentJobFinalisedCode);
		}

		WhsTransfer Transfer
		{
			get { return (WhsTransfer)LogParent; }
		}

		#endregion
	}
}
