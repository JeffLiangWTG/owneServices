using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.CFS.Business
{
	public class TallyPackLineCollection : CFSPackLineCollection
	{
		public TallyPackLineCollection(PackUnpackShipment master, BusinessObjectFactory factory) : base(master, factory)
		{ }

		#region BusinessObjectOverrides

		public override void RemoveAndDelete(BusinessObject elementToDelete)
		{
			if (elementToDelete.IsInDatabase)
			{
				NotifyAttemptToDeleteSavedPackline();
			}
			else
			{
				base.RemoveAndDelete(elementToDelete);
			}
		}

		public new TallyPackLine this[int index]
		{
			get { return (TallyPackLine)Elements[index]; }
		}

		public new TallyPackLine AddNew()
		{
			return (TallyPackLine)base.AddNew();
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return typeof(TallyPackLine);
		}

		#endregion

		#region Events

		public event EventHandler AttemptedToDeleteSavedPack;
		void NotifyAttemptToDeleteSavedPackline()
		{
			if (AttemptedToDeleteSavedPack != null)
			{
				AttemptedToDeleteSavedPack(this, EventArgs.Empty);
			}
		}

		#endregion
	}
}
