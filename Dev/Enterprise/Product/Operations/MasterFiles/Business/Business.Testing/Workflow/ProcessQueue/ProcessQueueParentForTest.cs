using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ProcessQueueParentForTest : DummyBusinessObject, IProcessQueueParent
	{
		public ProcessQueueParentForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region IProcessQueueParent Members

		public ActiveProcessQueueCollection ActiveProcessQueueForBinding
		{
			get
			{
				if (fActiveProcessQueueForBinding == null)
				{
					fActiveProcessQueueForBinding = new ActiveProcessQueueCollection(Factory);
				}
				return fActiveProcessQueueForBinding;
			}
		}

		public ProcessQueue CurrentQueue
		{
			get { return null; }
		}

		public override string TablePrefix
		{
			get { return "_1"; }
		}

		ActiveProcessQueueCollection fActiveProcessQueueForBinding;

		#endregion
	}
}
