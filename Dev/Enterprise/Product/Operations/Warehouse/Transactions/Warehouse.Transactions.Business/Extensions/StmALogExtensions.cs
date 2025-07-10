using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public static class StmALogExtensions
	{
		public static bool IsHoldCodeChangeEvent(this IStmALog log)
		{
			string type;
			return
				log != null &&
				log.SL_SE_NKEvent.EqualsIgnoringCase(Events.ChangeOfIdentifierCode) &&
				log.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, out type) &&
				((ZString)type).EqualsIgnoringCase(Constants.EventReferenceParameterTypes.HoldCode);
		}
	}
}
