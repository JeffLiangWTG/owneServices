using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.CFS.Business
{
	/// <summary>
	/// Summary description for CFSCodeDescriptionPairLists.
	/// </summary>
	/// 
	public class ContainerModeCodeDescriptionPairList : BookingContainerModeCodeDescriptionPairList
	{
		public ContainerModeCodeDescriptionPairList(ZString transportMode) : base(transportMode)
		{
			if (transportMode == Constants.TransportModes.Sea)
			{
				AddPair(Constants.ContainerModes.BuyersConsol, Constants.ContainerModeDescriptions.BuyersConsol);
			}
		}
	}
}

