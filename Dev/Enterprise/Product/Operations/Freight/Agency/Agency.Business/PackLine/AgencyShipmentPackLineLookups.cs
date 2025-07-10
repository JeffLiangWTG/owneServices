using CargoWise.EntityFramework;
using Enterprise.Freight.Common.Business;

namespace Enterprise.Freight.Agency.Business
{
	public class AgencyShipmentPackLineLookups : JobPackLinesLookups
	{
		public AgencyShipmentPackLineLookups(AgencyShipmentPackLine packline)
			: base(packline) { }

		public IBusinessObjectCollection Containers
		{
			get
			{
				if (Parent.Shipment == null)
				{
					return new ActiveBusinessObjectCollection<AgencyBookingContainer>(Factory, ZQuery.NoResultQuery);
				}
				else if (Parent.Shipment.IsBillOfLadingStage)
				{
					return Parent.Shipment.RealContainers;
				}
				else
				{
					return Parent.Shipment.BookedContainers;
				}
			}
		}

		#region Implementation

		protected new AgencyShipmentPackLine Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (AgencyShipmentPackLine)base.Parent; }
		}

		#endregion
	}
}
