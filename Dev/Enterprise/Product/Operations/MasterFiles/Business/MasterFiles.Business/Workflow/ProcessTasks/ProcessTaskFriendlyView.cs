using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	[CodeProperty("TaskID"), DescriptionProperty("Description")]
	public class ProcessTaskFriendlyView : NonPersistentBusinessObject
	{
		public ProcessTaskFriendlyView(ProcessTask processTask)
			: base()
		{
			this.processTask = processTask;
		}

		protected override ZGuid GetPK()
		{
			return processTask != null ? this.processTask.PK : ZGuid.Empty;
		}

		readonly ProcessTask processTask;

		public ZString TaskID
		{
			get { return processTask.P9_TaskID; }
		}

		public ZString Description => string.Format("{0}  {1}  {2}", processTask.P9_Sequence, processTask.P9_Description, (processTask.ProcessHeader != null) ? processTask.ProcessHeader.Description : ZString.Empty);
	}
}
