using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.eTail.Business
{
	public class HVLVConsignmentProcessTaskCollection : ProcessTaskCollection
	{
		public HVLVConsignmentProcessTaskCollection(HVLVConsignment consignment)
			: base(consignment)
		{
		}

		new HVLVConsignment Parent => (HVLVConsignment)base.Parent;

		public new HVLVConsignmentProcessTask this[int index] => (HVLVConsignmentProcessTask)Elements[index];

		public new HVLVConsignmentProcessTask AddNew() => (HVLVConsignmentProcessTask)base.AddNew();

		public override ProcessTaskCollection CreateNewCollection() => new HVLVConsignmentProcessTaskCollection(Parent);

		public override ZString OriginCountry => Parent.OriginCountry;

		public override ZString DestinationCountry => Parent.DestinationCountry;

		protected override ZQuery CreateRelationshipFilter()
		{
			if (Parent.IsBeingExportedToUniversalXml)
			{
				return ZQuery.NoResultQuery;
			}

			return base.CreateRelationshipFilter();
		}
	}
}
