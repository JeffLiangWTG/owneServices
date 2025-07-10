using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.Warehouse.Transit.Document.Common;
using Enterprise.Warehouse.Transit.Document.DocDataObjects;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Document.Testing
{
	public class CIN750OutNotificationLogsCreatorTest : TestCaseWithFactory
	{
		#region TestCreateMessageSentLog

		[TestDate(2024, 12, 10, 14, 33, 1)]
		public void TestCreateMessageSentLog()
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch("WH1");
			warehouse.WarehouseAddress.Address1 = "WH1Address";
			Helper.AddOrgCode(warehouse.WarehouseAddress, OrgCusCode.FranceCodeTypes.CIN, "C001");
			var logParent = Helper.CreateDispatchConsignment("DC0000001", warehouse.PK);

			var notification = new CIN750OutNotification("WhsItemDispatchConsignment", "DC0000001");
			notification.RefType = new CodeDescription(new CIN750RefTypes())
			{
				Code = CIN750RefTypes.Codes.MasterAirWaybill
			};
			notification.SourceBusinessObject = logParent;
			notification.RefCode = "MAB-1";
			notification.Goods = new List<DocPackingLine>() {
				new DocPackingLine(ZGuid.NewZGuid())
				{
					AmountQuantity = 5,
					AmountWeight = 10
				}
			};
			var data = notification.MakeDynamic();

			var logCreator = new CIN750OutNotificationLogsCreator();
			logCreator.CreateMessageSentLog(logParent, data, "CIN750OutNotification", "Bollore");

			var msnEvents = logParent
				.Logs
				.GetAllLogs()
				.OfType<StmALog>()
				.Where(log => log.SL_SE_NKEvent == Events.MessageSentCode)
				.ToArray();

			AssertEquals("MSN logs parameters for sent CIN 750 Out Notification", "|ARG=241210143301000|HBL=-|JOB=DC0000001|MBL=-|MST=CIN750OutNotification|OTY=5|PTP=AWB|RFN=EDIDATDC0000001|WGT=10", msnEvents.Single().SL_Reference);
		}

		#endregion

		#region TestCreateWithdrawalSentLog

		public void TestCreateWithdrawalSentLog()
		{
			var logParent = Factory.New<WhsItemDispatchConsignment>();
			var notification = new CIN750InNotification("WhsItemDispatchConsignment", "DC0000001");
			var data = notification.MakeDynamic();

			var logCreator = new CIN750OutNotificationLogsCreator();
			AssertEquals(false, logCreator.CreateWithdrawalSentLog(logParent, data, "CIN750OutNotification", "Bollore", "Test Reason"));
		}

		#endregion

		#region Implement

		protected WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory));
		WhsTransitTestHelper helper;

		#endregion
	}
}
