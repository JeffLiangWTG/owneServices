using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.SailingDataVendor.Business.Testing
{
	sealed class OneStopContainerEventRequestLineTest_ForConsol : OneStopContainerEventRequestLineTest
	{
		protected override CommonContainer NewContainer()
		{
			CommonContainer result = Consol.Containers.AddNew();
			result.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			return result;
		}

		protected override void SetETD(ZDateTime etd)
		{
			Consol.Transports.MostInterestingTransport.JW_ETD = etd;
		}

		protected override void SetETA(ZDateTime eta)
		{
			Consol.Transports.MostInterestingTransport.JW_ETA = eta;
		}

		CommonConsol Consol
		{
			get
			{
				if (consol == null)
				{
					consol = Factory.New<CommonConsol>();
				}
				return consol;
			}
		}
		CommonConsol consol;
	}
}
