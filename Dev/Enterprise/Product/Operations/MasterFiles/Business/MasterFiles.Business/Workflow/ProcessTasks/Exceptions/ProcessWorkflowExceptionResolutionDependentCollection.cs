using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class ProcessWorkflowExceptionResolutionDependentCollection : DependentBusinessObjectCollection<ProcessWorkflowExceptionResolution, ProcessWorkflowExceptionType>
	{
		readonly ProcessWorkflowExceptionType master;

		public ProcessWorkflowExceptionResolutionDependentCollection(ProcessWorkflowExceptionType master, ZQuery filter = null)
			: base(master, filter)
		{
			this.master = master;
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent => ProcessWorkflowExceptionResolutionSchema.WER_WET_Type;

		protected override void SetCollectionRelationships(BusinessObject child)
		{
			base.SetCollectionRelationships(child);
			((ProcessWorkflowExceptionResolution)child).WER_WET_Type = master.PK;
		}
	}
}
