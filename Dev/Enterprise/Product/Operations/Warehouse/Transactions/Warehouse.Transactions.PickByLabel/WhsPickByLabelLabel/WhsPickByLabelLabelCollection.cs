using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.PickByLabel
{
	public class WhsPickByLabelLabelCollection : DependentBusinessObjectCollection<WhsPickByLabelLabel, WhsPickByLabelJob>
	{
		public WhsPickByLabelLabelCollection(WhsPickByLabelJob master, BusinessObjectFactory factory)
			: base(master, factory)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent => WhsPickByLabelLabelSchema.WTL_WTK_PickByLabelJob;
	}
}

// Add tests to PickByLabel.Testing project.
