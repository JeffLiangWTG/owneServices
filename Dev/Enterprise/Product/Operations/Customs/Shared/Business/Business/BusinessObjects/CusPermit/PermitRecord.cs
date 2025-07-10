using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public class PermitRecord
	{
		public SharedCusPermitHeader PermitHeader;
		public ZDecimal Value;
		public ZDecimal Quantity;
		public List<ZString> ErrorMessages = new List<ZString>();
		public ZString Procedure;
	}
}
