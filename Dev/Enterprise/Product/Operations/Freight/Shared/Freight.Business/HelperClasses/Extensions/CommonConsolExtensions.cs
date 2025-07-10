using CargoWise.Types;

namespace Enterprise.Freight.Business.Extensions
{
	public static class CommonConsolExtensions
	{
		public static string LogReference(this CommonConsol consol, bool checkIsInDatabase)
		{
			return checkIsInDatabase && !consol.IsInDatabase ? (ZString)consol.PK.ToString() : consol.JK_UniqueConsignRef;
		}

		public static bool IsDomesticRailOrRoad(this CommonConsol consol)
		{
			return (consol.JK_TransportMode == Core.Constants.TransportModes.Rail || consol.JK_TransportMode == Core.Constants.TransportModes.Road) && consol.IsDomesticFreight;
		}

		public static bool IsDomesticSeaOrAir(this CommonConsol consol)
		{
			return (consol.IsSea || consol.IsAir) && consol.IsDomesticFreight;
		}
	}
}
