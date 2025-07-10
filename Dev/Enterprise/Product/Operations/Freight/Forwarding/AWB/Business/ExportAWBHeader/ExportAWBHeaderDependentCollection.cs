using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.AWB.Business
{
	public class ExportAWBHeaderDependentCollection : DependentBusinessObjectCollection<ExportAWBHeader, ExportAWBHeader>
	{
		public ExportAWBHeaderDependentCollection(ExportAWBHeader master)
			: base(master)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return ExportAWBHeaderSchema.EH_EH_Parent; }
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var newChild = (ExportAWBHeader)child;
			newChild.EH_AWBType = AWBTypeList.Codes.House;
		}
	}
}
