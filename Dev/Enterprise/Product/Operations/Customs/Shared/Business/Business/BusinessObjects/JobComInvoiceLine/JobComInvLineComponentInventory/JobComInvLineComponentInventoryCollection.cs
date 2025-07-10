using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class JobComInvLineComponentInventoryCollection : DependentBusinessObjectCollection<JobComInvLineComponentInventory, BaseJobComInvoiceLine>
	{
		public JobComInvLineComponentInventoryCollection(BaseJobComInvoiceLine associatedInvoiceLine) : base(associatedInvoiceLine)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return JobComInvLineComponentInventorySchema.JIV_JI; }
		}

		protected override bool AllowNewCore => false;
	}
}
