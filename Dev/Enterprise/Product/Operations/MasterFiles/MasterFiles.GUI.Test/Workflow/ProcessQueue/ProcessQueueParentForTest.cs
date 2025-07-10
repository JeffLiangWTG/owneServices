using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class ProcessQueueParentForTest : NonPersistentBusinessObject, IProcessQueueParent
	{
		public ProcessQueueParentForTest(BusinessObjectFactory factory) : base(factory)
		{
			CurrentQueue.HasChanges = false;
		}

		#region IProcessQueueParent Members

		public ActiveProcessQueueCollection ActiveProcessQueueForBinding
		{
			get { return Helper.ActiveProcessQueueForBinding; }
		}

		public ProcessQueue CurrentQueue
		{
			get { return Helper.CurrentQueue; }
		}

		public override string TablePrefix
		{
			get { return "$$"; }
		}

		ProcessQueueParentHelper Helper
		{
			get
			{
				if (fHelper == null)
				{
					fHelper = new ProcessQueueParentHelper(this);
				}
				return fHelper;
			}
		}

		ProcessQueueParentHelper fHelper;

		#endregion
	}
}
