using System.Linq;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.Manifest.Business
{
	public partial class TRTransportTypes
	{
		public static CodeDescriptionPairList GetTransportTypesByTransportMode(ZString transportMode)
		{
			var allTypes = new TRTransportTypes();
			var result = new CodeDescriptionPairList();
			switch (transportMode)
			{
				case Core.Constants.TransportModes.Sea:
					result.AddPairsIfNotExist(allTypes.ToArray().Where(x => x.Code.StartsWith("1")));
					break;
				case Core.Constants.TransportModes.Rail:
					result.AddPairsIfNotExist(allTypes.ToArray().Where(x => x.Code.StartsWith("2")));
					break;
				case Core.Constants.TransportModes.Road:
					result.AddPairsIfNotExist(allTypes.ToArray().Where(x => x.Code.StartsWith("3")));
					break;
				case Core.Constants.TransportModes.Air:
					result.AddPairsIfNotExist(allTypes.ToArray().Where(x => x.Code.StartsWith("4")));
					break;
				case Core.Constants.TransportModes.Mail:
					result.AddPairsIfNotExist(allTypes.ToArray().Where(x => x.Code.StartsWith("5")));
					break;
				case Core.Constants.TransportModes.FixedTransportInstallations:
					result.AddPairsIfNotExist(allTypes.ToArray().Where(x => x.Code.StartsWith("7")));
					break;
				case Core.Constants.TransportModes.InlandWaterwayTransport:
					result.AddPairsIfNotExist(allTypes.ToArray().Where(x => x.Code.StartsWith("8")));
					break;
				case Core.Constants.TransportModes.OwnPropulsion:
					result.AddPairsIfNotExist(allTypes.ToArray().Where(x => x.Code.StartsWith("9")));
					break;
			}
			return result;
		}
	}
}
