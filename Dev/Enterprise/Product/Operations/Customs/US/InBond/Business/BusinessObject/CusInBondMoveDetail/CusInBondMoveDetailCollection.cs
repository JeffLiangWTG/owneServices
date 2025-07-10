namespace Enterprise.Customs.US.InBond.Business
{
	public class CusInBondMoveDetailCollection : US.Business.CusInBondMoveDetailCollection<CusInBondMoveDetail>
	{
		public CusInBondMoveDetailCollection(CusInBondMoveHeader master)
			: base(master)
		{
		}

		public CusInBondMoveDetailCollection(CusInBondBill master)
			: base(master)
		{
		}

		public bool IsWaitingForResponseOrHasBeenReportedToCustoms
		{
			get
			{
				bool result = false;
				foreach (CusInBondMoveDetail moveDetail in this)
				{
					if (moveDetail.IsWaitingForResponse || (!moveDetail.LogManager.HasAWithdrawnLog && moveDetail.LogManager.HasAClearLog))
					{
						result = true;
						break;
					}
				}
				return result;
			}
		}

		public bool CanDeleteAll
		{
			get
			{
				bool result = true;
				foreach (CusInBondMoveDetail moveDetail in this)
				{
					if (!moveDetail.CanDelete)
					{
						result = false;
						break;
					}
				}
				return result;
			}
		}

		protected override void SetDefaultsForNewElementCore(CusInBondMoveDetail newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			var header = newElement.InBondHeader;
			if (newElement.IsDetailedInBond && header != null && !header.IsAir)
			{
				newElement.Containers.AddNew();
			}
		}

		protected override bool AllowNew
		{
			get
			{
				var moveHeader = Relationship.Master as CusInBondMoveHeader;
				if (moveHeader == null || moveHeader.IsDeleted)
				{
					var master = Relationship.Master as CusInBondBill;
					return master != null && !master.IsDeleted && !master.ShouldSynchronise;
				}
				else
				{
					return !moveHeader.ShouldSynchronise;
				}
			}
		}
	}
}
