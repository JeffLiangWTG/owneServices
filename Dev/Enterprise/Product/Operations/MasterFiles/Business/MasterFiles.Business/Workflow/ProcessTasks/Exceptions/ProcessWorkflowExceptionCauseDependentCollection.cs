using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class ProcessWorkflowExceptionCauseDependentCollection : DependentBusinessObjectCollection<ProcessWorkflowExceptionCause, ProcessWorkflowExceptionType>
	{
		readonly ProcessWorkflowExceptionType master;

		public ProcessWorkflowExceptionCauseDependentCollection(ProcessWorkflowExceptionType master, ZQuery filter = null)
			: base(master, filter)
		{
			this.master = master;
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent => ProcessWorkflowExceptionCauseSchema.WEC_WET_Type;

		protected override void SetCollectionRelationships(BusinessObject child)
		{
			base.SetCollectionRelationships(child);
			((ProcessWorkflowExceptionCause)child).WEC_WET_Type = master.PK;
		}
	}
}
