using CargoWise.Types;

namespace Enterprise.Customs.NZ.Business
{
	partial class TranshipmentRequestModeOfMovement
	{
		public static ZBool IsSea(ZString code)
		{
			return code == Codes.Sea || code == Codes.SeaCV || code == Codes.SeaOV;
		}

		public static bool IsAir(string movementMode) => movementMode == TranshipmentRequestModeOfMovement.Codes.Air;
	}
}
