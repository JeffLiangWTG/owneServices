using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.SailingDataVendor.Business.Testing
{
	sealed class OneStopContainerEventDataVendorTest_ForConsol : OneStopContainerEventDataVendorTest
	{
		protected override CommonContainer NewContainer()
		{
			CommonContainer result = Consol.Containers.AddNew();
			result.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			return result;
		}

		protected override void SetContainerNumber(CommonContainer container, ZString containerNum)
		{
			container.JC_ContainerNum = containerNum;
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
					consol = (CommonConsol)Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();
					consol.JK_RL_NKLoadPort = "AUSYD";
					consol.JK_RL_NKDischargePort = "NZAKL";
				}
				return consol;
			}
		}
		CommonConsol consol;
	}
}
