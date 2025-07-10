using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public static class IJobCostingPlugInHelper
	{
		public static ZString GetDirectionCode(Directions? directionEnum)
		{
			if (directionEnum == null || directionEnum == Directions.Unknown)
			{
				return ZString.Empty;
			}

			switch (directionEnum)
			{
				case Directions.Import:
					return Core.Constants.FreightShipmentDirection.Code.Import;
				case Directions.Export:
					return Core.Constants.FreightShipmentDirection.Code.Export;
				case Directions.Domestic:
					return Core.Constants.FreightShipmentDirection.Code.Domestic;
				default:
					return Core.Constants.FreightShipmentDirection.Code.Other;
			}
		}
	}
}
