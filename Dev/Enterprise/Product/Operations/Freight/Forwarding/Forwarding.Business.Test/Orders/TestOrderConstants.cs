using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Business.Test
{
	public class TestOrderConstants : TestCase
	{
		public void TestContainerModeList()
		{
			CodeDescriptionPairList list = OrdersConstants.GetContainerModeList(Constants.TransportModes.Air);
			AssertEquals("Number Container Modes for this transport mode must be 4", 4, list.Count);
			AssertEquals("For this transport mode CodeAsString must bee", "LSE, ULD, CON, OTH", list.CodesAsString);

			list = OrdersConstants.GetContainerModeList(Constants.TransportModes.Sea);
			AssertEquals("Number Container Modes for this transport mode must be 7", 7, list.Count);
			AssertEquals("For this transport mode CodeAsString must bee", "FCL, LCL, BLK, LQD, BBK, ROR, OTH", list.CodesAsString);

			list = OrdersConstants.GetContainerModeList(Constants.TransportModes.AirSea);
			AssertEquals("Number Container Modes for this transport mode must be 4", 4, list.Count);
			AssertEquals("For this transport mode CodeAsString must bee", "LSE, ULD, LCL, OTH", list.CodesAsString);

			list = OrdersConstants.GetContainerModeList(Constants.TransportModes.SeaAir);
			AssertEquals("Number Container Modes for this transport mode must be 4", 4, list.Count);
			AssertEquals("For this transport mode CodeAsString must bee", "LCL, LSE, ULD, OTH", list.CodesAsString);

			list = OrdersConstants.GetContainerModeList(Constants.TransportModes.Rail);
			AssertEquals("Number Container Modes for this transport mode must be 6", 6, list.Count);
			AssertEquals("For this transport mode CodeAsString must bee", "FCL, LCL, BLK, LQD, BBK, OTH", list.CodesAsString);

			list = OrdersConstants.GetContainerModeList(Constants.TransportModes.Road);
			AssertEquals("Number Container Modes for this transport mode must be 5", 5, list.Count);
			AssertEquals("For this transport mode CodeAsString must bee", "FCL, FTL, LCL, LTL, OTH", list.CodesAsString);

			list = OrdersConstants.GetContainerModeList(Constants.TransportModes.Courier);
			AssertEquals("Number Container Modes for this transport mode must be 2", 2, list.Count);
			AssertEquals("For this transport mode CodeAsString must bee", "OBC, UNA", list.CodesAsString);

			list = OrdersConstants.GetContainerModeList(Constants.TransportModes.Mail);
			AssertEquals("Number Container Modes for this transport mode must be 1", 1, list.Count);
			AssertEquals("For this transport mode CodeAsString must bee", "MAI", list.CodesAsString);

			list = OrdersConstants.GetContainerModeList(Constants.TransportModes.Unknown);
			AssertEquals("Number Container Modes for this transport mode must be 5", 5, list.Count);
			AssertEquals("For this transport mode CodeAsString must bee", "FCL, LCL, BLK, BBK, OTH", list.CodesAsString);

			list = OrdersConstants.GetContainerModeList("");
			AssertEquals("Number Container Modes for this transport mode must be 14", 14, list.Count);
			AssertEquals("For this transport mode CodeAsString must bee", "LSE, ULD, CON, OBC, UNA, MAI, FCL, FTL, LCL, LTL, BLK, LQD, BBK, OTH", list.CodesAsString);
		}
	}
}
