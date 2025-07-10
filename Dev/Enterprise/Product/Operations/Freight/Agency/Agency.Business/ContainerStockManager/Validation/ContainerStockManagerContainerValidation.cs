using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business
{
	public class ContainerStockManagerContainerValidation : JobContainerValidation
	{
		public ContainerStockManagerContainerValidation(AgencyShipmentContainer container)
			: base(container) { }

		protected override void CheckJC_RC()
		{
			base.CheckJC_RC();

			RefContainerStock stock = Parent.Stock;
			RefContainer container = Parent.Container;

			if (stock != null && container != null && stock.R6_RC_ISOType != container.RC_ISOType)
			{
				Parent.JC_RCInfo.AddWarning(Res.GetString("d27dae56-704c-424f-9b63-94882d4d3334", "The ISO code recorded against the container '{0}' ({1}) does not match the ISO code for this container type ({2}).",
					Parent.JC_ContainerNum, stock.R6_RC_ISOType, container.RC_ISOType));
			}
		}

		protected override void CheckJC_IsShipperOwned()
		{
			base.CheckJC_IsShipperOwned();

			RefContainerStock stock = Parent.Stock;

			if (stock != null)
			{
				if (stock.R6_OwnerType == Enterprise.Core.Constants.ContainerOwnership.Codes.ShipperOwned)
				{
					if (!Parent.JC_IsShipperOwned)
					{
						Parent.JC_IsShipperOwnedInfo.AddWarning(Res.GetString("e2f9b1b8-a304-4065-b8a4-65d13a19abae", "This container is recorded as being shipper owned but the shipper owned flag has not been ticked."));
					}
				}
				else
				{
					if (Parent.JC_IsShipperOwned)
					{
						Parent.JC_IsShipperOwnedInfo.AddWarning(Res.GetString("41dcc162-acf1-453f-8019-875247fa646c", "This container is recorded as being non-shipper owned but the shipper owned flag has been ticked."));
					}
				}
			}
		}

		#region Implementation

		new BillOfLadingContainer Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (BillOfLadingContainer)base.Parent; }
		}

		#endregion
	}
}


