using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public class ReturnData
	{
		public ReturnData()
		{
			Result = false;
			ResultString = ZString.Empty;
			ResultDecimal = ZDecimal.Zero;
		}
		public ZBool Result;
		public ZString ResultString;
		public ZDecimal ResultDecimal;
	}
}
