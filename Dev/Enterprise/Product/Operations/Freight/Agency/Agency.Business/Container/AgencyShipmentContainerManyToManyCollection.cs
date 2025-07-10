using System;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.Agency.Business
{
	public class AgencyShipmentContainerManyToManyCollection : CommonContainerManyToManyCollection
	{
		public AgencyShipmentContainerManyToManyCollection(AgencyShipmentPackLine parent)
			: base(parent)
		{
		}

		public new AgencyShipmentContainer this[int index]
		{
			get { return (AgencyShipmentContainer)Elements[index]; }
		}

		public new AgencyShipmentContainer AddNew()
		{
			return (AgencyShipmentContainer)base.AddNew();
		}

		protected override Type TypeOfRelationshipBusinessObject
		{
			get { return typeof(JobContainerPackPivot); }
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return typeof(AgencyShipmentContainer);
		}
	}
}
