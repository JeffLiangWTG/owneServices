using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.WarehouseExtensions.Testing
{
	public static class JobDeclarationWarehouseTestExtensions
	{
		public static void SetWarehouseType(this OrgAddress address, ZBool isVirtual)
		{
			if (address != null)
			{
				var query = new ZQuery(WhsWarehouseSchema.WW_OA_WarehouseAddress, address.PK);
				query.FetchOnlyFromLocalCache = !address.IsInDatabase;
				var warehouse = address.Factory.LoadTop1<Warehouse.Integration.IWhsWarehouse>(query);
				warehouse.WW_IsVirtualWarehouse = isVirtual;
			}
		}

		public static StmALog[] GetDataExportLogsInPostedOrder(this EnterpriseBusinessObject bizObj)
		{
			return bizObj.Logs.GetAllLogs().OfType<StmALog>().Where(x => x.SL_SE_NKEvent == Events.DataExportCode).OrderBy(x => x.SL_PostedTimeUtc).ToArray();
		}

		public static void AssertXMLMessageWasCreated(this BaseJobDeclaration declaration, ZString messageSubType, RecipientRoleType roleType)
		{
			var exportLog = declaration.Logs.MostRecentLogByEventTime(Events.DataExport);
			exportLog.AssertLogHasXMLMessage(declaration.JE_DeclarationReference, messageSubType, roleType);
		}

		public static void AssertLogHasXMLMessage(this StmALog log, ZString sourceKey, ZString messageSubType, RecipientRoleType roleType, ZString? messageText = null, DataContextType? dataSourceType = null)
		{
			TestCaseWithFactory.AssertNotNull(log);
			var relatedEDIMessage = log.RelatedEDIMessage;
			var message = relatedEDIMessage.Message;
			TestCaseWithFactory.AssertEquals(messageSubType, message.EM_MessageSubType);
			if (messageText.HasValue)
			{
				TestCaseWithFactory.AssertContains(messageText.Value, message.EM_MessageText);
			}
			var dataContex = relatedEDIMessage.DataContext;
			if (!dataSourceType.HasValue)
			{
				dataSourceType = DataContextType.CustomsDeclaration;
			}
			TestCaseWithFactory.AssertNotNull(dataContex.DataSourceCollection.FirstOrDefault(x => x.Type.GetValueOrDefault() == dataSourceType.ToString() && x.Key.GetValueOrDefault() == sourceKey));
			TestCaseWithFactory.AssertNotNull(dataContex.RecipientRoleCollection.FirstOrDefault(x => x.Code.GetValueOrDefault() == roleType));
		}
	}
}
