using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.GUI.Workflow
{
	public class StmJobQueueCollection : NonPersistentBusinessObjectCollection<StmJobQueueViewModel>
	{
		public StmJobQueueCollection(StmALog wteLog)
		{
			this.wteLog = wteLog;
		}

		public override void Load()
		{
			if (wteLog != null)
			{
				var query = new ZQuery(StmJobQueueSchema.SJ_ALogReference, wteLog.PK);

				var jobQueues = wteLog.Factory.Load<IQueuedLog>(query);

				foreach (var jobQueue in jobQueues)
				{
					Add(new StmJobQueueViewModel(jobQueue));
				}
			}
		}

		#region NonPersistentBusinessObjectCollection Overrides

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException();
		}

		protected override bool AllowNewCore => false;
		protected override bool AllowRemoveCore => false;

		#endregion

		#region Implementation

		readonly StmALog wteLog;

		#endregion
	}
}
