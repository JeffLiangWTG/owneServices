using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.CFS.Business
{
	/// <summary>
	/// Summary description for CFSBuildConsolHelper.
	/// </summary>
	public class CFSBuildConsolHelper : BuildConsolHelper
	{
		public void AddBookingToCFSLoadList(CommonConsol consol, ZGuid shipmentPK)
		{
			bool originalAutomaticPackingState = consol.AutomaticallyUpdatePackLineContainers;
			consol.AutomaticallyUpdatePackLineContainers = false;

			var shipmentReceival = consol.Factory.Load<CFSShipment>(shipmentPK);
			if (shipmentReceival != null)
			{
				AddBookingToConsol(shipmentReceival, consol);
			}

			consol.AutomaticallyUpdatePackLineContainers = originalAutomaticPackingState;
		}

		protected override void AddBookingToConsol(CommonShipment booking, CommonConsol consol, ZGuid? quotedBookingPK = null)
		{
			base.AddBookingToConsol(booking, consol);
			booking.JS_JX = ZGuid.Empty;
		}

		protected override void AddLineToContainerAndConsol(CommonConsol consol, ZGuid containerPK, PackLine line)
		{
			if (consol is CFSLoadListConsol cfsLoadListConsol)
			{
				var container = consol.Factory.Load<CFSContainer>(containerPK);
				container.JC_OH_CFSClient = cfsLoadListConsol.JK_OH_Forwarder;

				var consolContainer = AddNewOrGetExistingContainerFromConsolContainers(consol, container);
				line.SetContainer(consol, consolContainer);
			}
		}
	}
}
