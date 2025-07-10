using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.eTail.Integration;

namespace Enterprise.eTail.Business
{
	public class HVLVConsignmentTransactionParticipant : SaveInTransactionActionWithFactory
	{
		public HVLVConsignmentTransactionParticipant(BusinessObjectFactory factory)
			: base(factory)
		{
			this.factory = factory;
		}

		readonly BusinessObjectFactory factory;
		readonly HashSet<HVLVConsignment> consigments = new HashSet<HVLVConsignment>();

		public static void Register(HVLVConsignment parent)
		{
			var parentFactory = parent.Factory;

			var participant = parentFactory.SaveInTransactionActions.OfType<HVLVConsignmentTransactionParticipant>().FirstOrDefault();

			if (participant == null)
			{
				participant = new HVLVConsignmentTransactionParticipant(parentFactory);
				parentFactory.SaveInTransactionActions.Add(participant);
			}

			participant.consigments.Add(parent);
		}

		public static void UnRegister(IHVLVConsignmentCollection consignmentList)
		{
			foreach (HVLVConsignment consignment in consignmentList)
			{
				UnRegisterCore(consignment);
			}
		}

		protected override bool AllowTransactionWithOtherParticipant => true;

		public static void UnRegister(HVLVConsignment consignment)
		{
			UnRegisterCore(consignment);
		}

		static void UnRegisterCore(HVLVConsignment parent)
		{
			var parentFactory = parent.Factory;

			var existingParticipant = parentFactory.SaveInTransactionActions.OfType<HVLVConsignmentTransactionParticipant>().FirstOrDefault();
			if (existingParticipant != null)
			{
				existingParticipant.consigments.Remove(parent);
				parentFactory.SaveInTransactionActions.Remove(existingParticipant);
			}
		}

		protected override IChangedTableNames SaveInTransaction()
		{
			return ChangedTableNames.Empty;
		}

		protected override void OnAllTransactionsCommitted(IChangedTableNames changedTableNames)
		{
			if (consigments.Any())
			{
				factory.ReloadAllSafe(consigments);
				consigments.Clear();
			}
		}
	}
}
