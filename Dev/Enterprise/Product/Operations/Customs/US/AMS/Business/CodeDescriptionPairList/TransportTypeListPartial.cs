using CargoWise.EntityFramework;
using CargoWise.Types;
//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.AMS.Business
{
	partial class TransportTypeList
	{
		public static CodeDescriptionPairList GetConveyanceTransportTypeList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("USAMSConveyanceTransportTypeList", delegate
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(Codes.VesselNonContainer, Descriptions.VesselNonContainer);
				result.AddPair(Codes.VesselContainer, Descriptions.VesselContainer);
				result.AddPair(Codes.Rail, Descriptions.Rail);
				return result;
			});
		}

		public static ZString GetFreightTransportType(ZString code)
		{
			var result = ZString.Empty;
			switch (code)
			{
				case Codes.VesselContainer:
				case Codes.VesselNonContainer:
					result = Core.Constants.TransportModes.Sea;
					break;
				case Codes.Rail:
					result = Core.Constants.TransportModes.Rail;
					break;
			}
			return result;
		}
	}
}
