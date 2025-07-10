using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.InBond.Business
{
	public class USMovementHeaderResetCollection : NonPersistentBusinessObjectCollection<USMovementHeaderReset>
	{
		public USMovementHeaderResetCollection(CusInBondHeader header)
			: base(header.Factory)
		{
			this.header = header;
			PopulateObjects();
		}

		public readonly CusInBondHeader header;

		void PopulateObjects()
		{
			foreach (CusInBondMoveHeader moveHeader in header.MovementHeaders)
			{
				if (moveHeader.IsResetableToOriginal)
				{
					Add(new USMovementHeaderReset(moveHeader, this));
				}

				foreach (var movementDetail in moveHeader.MovementDetails)
				{
					foreach (var container in movementDetail.Containers)
					{
						if (container.IsResetableToOriginal)
						{
							Add(new USMovementHeaderReset(container, this));
						}
					}
				}
			}

			foreach (var bill in header.Bills)
			{
				if (bill.IsResetableToOriginal)
				{
					Add(new USMovementHeaderReset(bill, this));
				}
			}
		}

		public void ResetMoveHeadersCollectionToOriginal()
		{
			foreach (USMovementHeaderReset obj in this)
			{
				if (obj.RO_ResetToOriginal)
				{
					obj.ResetToOriginal();
				}
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotImplementedException();
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}
	}
}
