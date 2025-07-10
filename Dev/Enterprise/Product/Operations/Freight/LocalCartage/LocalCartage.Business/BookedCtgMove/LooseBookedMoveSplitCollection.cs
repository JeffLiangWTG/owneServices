using CargoWise.EntityFramework;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class LooseBookedMoveSplitCollection : NonPersistentBusinessObjectCollection<LooseBookedMoveSplit>
	{
		public LooseBookedMoveSplitCollection(LooseBookedMoveSplitMaster master)
			: base(master.Factory)
		{
			Master = master;
		}
		readonly LooseBookedMoveSplitMaster Master;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new LooseBookedMoveSplit(Master);
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}
	}
}
