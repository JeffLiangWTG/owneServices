using System.Linq;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business
{
	public abstract class MilestoneOrTriggerCollectionView : WorkflowItemCollectionView
	{
		public MilestoneOrTriggerCollectionView(ProcessTaskCollection collection)
			: base(collection)
		{
		}

		public ProcessTask this[ZGuid pk]
		{
			get { return this.Cast<ProcessTask>().FirstOrDefault(milestone => milestone.PK == pk); }
		}

		public ProcessTask this[Event eventType]
		{
			get { return this[eventType.Code]; }
		}

		public ProcessTask this[ZString eventTypeCode]
		{
			get { return this.Cast<ProcessTask>().FirstOrDefault(milestone => milestone.P9_SE_NKMilestoneEvent == eventTypeCode); }
		}
	}
}
