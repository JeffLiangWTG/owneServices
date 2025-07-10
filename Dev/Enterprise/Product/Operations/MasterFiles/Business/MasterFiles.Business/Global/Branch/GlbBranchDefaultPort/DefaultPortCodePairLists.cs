using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public static class DefaultPortCodePairLists
	{
		public static CodeDescriptionPairList TransportModeList(bool isDefaultToShipment)
		{
			var result = new CodeDescriptionPairList(OLookUpEditType.CustomType);
			result.AddPair(Core.Constants.TransportModes.Air, Core.Constants.TransportModeDescriptions.Air);
			result.AddPair(Core.Constants.TransportModes.Sea, Core.Constants.TransportModeDescriptions.Sea);
			result.AddPair(Core.Constants.TransportModes.Road, Core.Constants.TransportModeDescriptions.Road);
			result.AddPair(Core.Constants.TransportModes.Rail, Core.Constants.TransportModeDescriptions.Rail);
			if (isDefaultToShipment)
			{
				result.AddPair(Core.Constants.TransportModes.Courier, Core.Constants.TransportModeDescriptions.Courier);
			}
			result.AddPair(Core.Constants.TransportModes.All, Core.Constants.TransportModeDescriptions.All);
			return result;
		}

		public static CodeDescriptionPairList ConsolContainerModeList(ZString transportMode)
		{
			var result = new CodeDescriptionPairList(OLookUpEditType.CustomType);
			result.AddRange(ObjectFactory.Get<IFreightCodePairListProvider>().GetConsolModeList(string.Empty, transportMode));
			result.AddPair(ResString.GetMultilingualString("BB4574DF-178F-4B21-BFB6-6A53243CA494", "ALL"), ResString.GetMultilingualString("662C00D6-5B52-475A-83A8-12D5BD99BCBE", "All"));
			result.RemoveCode(Core.Constants.ContainerModes.BuyersConsol);
			result.RemoveCode(Core.Constants.ContainerModes.ShippersConsol);
			return result;
		}

		public static CodeDescriptionPairList ShipmentContainerModeList(ZString transportMode)
		{
			var result = new CodeDescriptionPairList(OLookUpEditType.CustomType);
			result.AddRange(ObjectFactory.Get<IFreightCodePairListProvider>().GetContainerModeList(transportMode));
			result.AddPair(ResString.GetMultilingualString("9C06B310-12BD-413F-8F35-16B0A8053BC2", "ALL"), ResString.GetMultilingualString("B4ED5DC3-63A5-42E6-82BF-0CBA6739FB95", "All"));
			result.RemoveCode(Core.Constants.ContainerModes.BuyersConsol);
			result.RemoveCode(Core.Constants.ContainerModes.ShippersConsol);
			return result;
		}
	}
}
