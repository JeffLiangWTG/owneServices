using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.TrolleyPicking
{
	public class WhsPickTrolleySlotCollection : ActiveBusinessObjectCollection<WhsPickTrolleySlot>
	{
		public WhsPickTrolleySlotCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public WhsPickTrolleySlotCollection(WhsPickTrolleyJob trolleyJob)
			: base(trolleyJob.Factory, new ZQuery(WhsPickTrolleySlotSchema.WTS_WTJ_TrolleyJob, trolleyJob.PK))
		{
			TrolleyJob = trolleyJob;
		}

		readonly WhsPickTrolleyJob TrolleyJob;

		#region SetDefaultsForNewElementCore

		protected override void SetDefaultsForNewElementCore(WhsPickTrolleySlot newTrolleySlot)
		{
			base.SetDefaultsForNewElementCore(newTrolleySlot);

			if (TrolleyJob != null)
			{
				newTrolleySlot.WTS_WTJ_TrolleyJob = TrolleyJob.PK;
			}
		}

		#endregion
	}
}

// Add tests to TrolleyPicking.Testing project.
