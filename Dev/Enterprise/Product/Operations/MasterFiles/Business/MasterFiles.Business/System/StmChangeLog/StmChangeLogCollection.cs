using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class StmChangeLogCollection : DependentBusinessObjectCollection<StmChangeLog, BusinessObject>
	{
		public StmChangeLogCollection(BusinessObject parent)
			: base(parent)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return StmChangeLogSchema.SY_ParentID; }
		}
	}
}
