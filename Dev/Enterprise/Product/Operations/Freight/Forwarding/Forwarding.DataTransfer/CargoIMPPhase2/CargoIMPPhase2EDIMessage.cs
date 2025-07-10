using System.Data;

using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class CargoIMPPhase2EDIMessage : EDIMessage
	{
		public CargoIMPPhase2EDIMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
