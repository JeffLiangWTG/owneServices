using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public partial class ACEDrawbackActionCodeList
	{
		public static ZBool IsManufacturedAction(ZString actionCode)
		{
			return actionCode == Codes.Manufactured
				|| actionCode == Codes.ManuAndTrans;
		}
	}
}
