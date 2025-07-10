using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public abstract class WhsDocketProcessTasksCollection : ProcessTaskCollection
	{
		#region Constructors 

		protected WhsDocketProcessTasksCollection(WhsDocket docket)
			: base(docket)
		{
		}

		#endregion

		public new WhsDocketProcessTasks AddNew()
		{
			return (WhsDocketProcessTasks)base.AddNew();
		}

		public new WhsDocketProcessTasks this[int index]
		{
			get { return (WhsDocketProcessTasks)Elements[index]; }
		}

		public override bool HasNewConditionBeenMetSinceLastSave()
		{
			return Parent.IsInDatabase && ((ICancellable)Parent).IsCancelledHasChanged;
		}

		#region Implementation

		new WhsDocket Parent
		{
			get { return (WhsDocket)base.Parent; }
		}

		#endregion
	}
}
