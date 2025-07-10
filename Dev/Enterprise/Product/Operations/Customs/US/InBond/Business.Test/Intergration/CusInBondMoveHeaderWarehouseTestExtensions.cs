using CargoWise.Types;
using Enterprise.Customs.Business.WarehouseExtensions.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.InBond.Business.WarehouseExtensions.Testing
{
	internal static class CusInBondMoveHeaderWarehouseTestExtensions
	{
		public static StmALog AssertXMLMessageWasCreated(this CusInBondMoveHeader moveHeader, ZString messageSubType, RecipientRoleType roleType)
		{
			var exportLog = moveHeader.Logs.MostRecentLogByEventTime(Events.DataExport);
			exportLog.AssertLogHasXMLMessage(moveHeader.InBondNumberAndJobReference, messageSubType, roleType, dataSourceType: DataContextType.WarehouseInBond);
			return exportLog;
		}
	}
}
