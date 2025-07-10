using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.InBond.Business
{
	public class CusInBondMoveHeaderCollection : Customs.Business.CusInBondMoveHeaderCollection
	{
		public CusInBondMoveHeaderCollection(CusInBondHeader master)
			: base(master)
		{
		}

		public new CusInBondMoveHeader this[int index]
		{
			get { return (CusInBondMoveHeader)base[index]; }
		}

		public new CusInBondMoveHeader AddNew()
		{
			return (CusInBondMoveHeader)base.AddNew();
		}

		public IReadOnlyList<ZString> UniqeInBondCarrierIDList
		{
			get
			{
				if (uniqeInBondCarrierIDListCached == null)
				{
					uniqeInBondCarrierIDListCached = new CachedProperty<IReadOnlyList<ZString>>(Factory, delegate
					{
						var list = new List<ZString>();
						foreach (CusInBondMoveHeader moveHeader in this)
						{
							if (!moveHeader.BM_InBondCarrierID.IsEmpty && !list.Contains(moveHeader.BM_InBondCarrierID))
							{
								list.Add(moveHeader.BM_InBondCarrierID);
							}
						}
						return list;
					});
				}
				return uniqeInBondCarrierIDListCached.Value;
			}
		}
		CachedProperty<IReadOnlyList<ZString>> uniqeInBondCarrierIDListCached;

		public void UnLockAllInBondNumberAllocationMutex()
		{
			foreach (CusInBondMoveHeader moveHeader in this)
			{
				moveHeader.UnLockInBondNumberAllocationMutex();
			}
		}

		#region Implementation

		internal void MarkCommoditiesAsNeedingValidation()
		{
			MarkCommoditiesAsNeedingValidation((x) =>
			{
				// do nothing
			});
		}

		internal void MarkCommoditiesAsNeedingValidation(Action<CusInBondCargoDesc> preMarkingAction)
		{
			foreach (CusInBondMoveHeader moveHeader in this)
			{
				moveHeader.MarkCommoditiesAsNeedingValidation(preMarkingAction);
			}
		}

		protected override bool AllowNew
		{
			get
			{
				var master = (CusInBondHeader)Relationship.Master;
				return master != null && !master.ShouldSynchronise;
			}
		}

		#endregion
	}
}
