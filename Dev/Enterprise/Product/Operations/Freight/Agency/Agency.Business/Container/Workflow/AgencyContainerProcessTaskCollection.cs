using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business
{
	public class AgencyContainerProcessTaskCollection : ContainerProcessTaskCollection
	{
		public AgencyContainerProcessTaskCollection(AgencyShipmentContainer container)
			: base(container)
		{
		}

		public new AgencyContainerProcessTask this[int index]
		{
			get { return (AgencyContainerProcessTask)Elements[index]; }
		}

		public new AgencyContainerProcessTask AddNew()
		{
			return (AgencyContainerProcessTask)base.AddNew();
		}

		public override ProcessTaskCollection CreateNewCollection()
		{
			return new AgencyContainerProcessTaskCollection(Parent);
		}

		new AgencyShipmentContainer Parent
		{
			get { return (AgencyShipmentContainer)base.Parent; }
		}
	}
}






