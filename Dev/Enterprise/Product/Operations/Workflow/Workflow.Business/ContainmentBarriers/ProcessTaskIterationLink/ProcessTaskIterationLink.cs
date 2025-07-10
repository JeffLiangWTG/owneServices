using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Workflow.Integration;

namespace Enterprise.Workflow.Business
{
	public class ProcessTaskIterationLink : AutoProcessTaskIterationLink, IProcessTaskIterationLink
	{
		public ProcessTaskIterationLink(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region BusinessObject Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			P9I_LinkType = IterationLinkTypeList.Codes.QualityIterationTask;
			P9I_Sequence = 1;
		}

		public override void Delete()
		{
			TaskPivots.DeleteAll();

			base.Delete();
		}

		#endregion

		#region Properties

		#region P9I_LinkType

		[List("Lookups.LinkTypes")]
		public override ZString P9I_LinkType
		{
			get { return base.P9I_LinkType; }
			set { base.P9I_LinkType = value; }
		}

		#endregion

		#endregion

		#region Related Business Objects

		public IProcessHeader IterationWorkflow
		{
			get { return Factory.Load<IProcessHeader>(P9I_FH_IterationWorkflow); }
		}

		IProcessTask IProcessTaskIterationLink.ContainmentBarrierTask
		{
			get { return ContainmentBarrierTask; }
		}
		public new ProcessTask IterationTask
		{
			get { return Factory.Load<ProcessTask>(P9I_P9_IterationTask); }
		}

		public IProcessTaskIterationLinkPivotCollection TaskPivots => taskPivots ?? (taskPivots = new ProcessTaskIterationLinkPivotCollection(this));

		ProcessTaskIterationLinkPivotCollection taskPivots;

		#endregion
	}
}
