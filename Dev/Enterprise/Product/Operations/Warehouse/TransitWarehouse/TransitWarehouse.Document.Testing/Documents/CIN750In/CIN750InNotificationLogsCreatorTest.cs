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
	public class CIN750InNotificationLogsCreatorTest : TestCaseWithFactory
	{
		#region TestCreateMessageSentLog

		[TestDate(2024, 12, 10, 14, 33, 1)]
		public void TestCreateMessageSentLog()
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch("WH1");
			warehouse.WarehouseAddress.Address1 = "WH1Address";
			Helper.AddOrgCode(warehouse.WarehouseAddress, OrgCusCode.FranceCodeTypes.CIN, "C001");
			var logParent = Helper.CreateReceiveConsignment("RC0000001", warehouse.PK);

			var notification = new CIN750InNotification("TransitReceive", "RC0000001");
			notification.RefType = new CodeDescription(new CIN750RefTypes())
			{
				Code = CIN750RefTypes.Codes.Reference
			};
			notification.RefCode = "RC0000001";
			notification.Goods = new List<DocPackingLine>() {
				new DocPackingLine(ZGuid.NewZGuid())
				{
					AmountQuantity = 5,
					AmountWeight = 10
				}
			};
			notification.SourceBusinessObject = logParent;
			var data = notification.MakeDynamic();

			var logCreator = new CIN750InNotificationLogsCreator();
			logCreator.CreateMessageSentLog(logParent, data, "CIN750InNotification", "Bollore");

			var msnEvents = logParent
				.Logs
				.GetAllLogs()
				.OfType<StmALog>()
				.Where(log => log.SL_SE_NKEvent == Events.MessageSentCode)
				.ToArray();

			AssertEquals("MSN logs parameters for sent CIN 750 In Notification", "|ARG=241210143301000|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750InNotification|OTY=5|PTP=REF|RFN=EDIDATRC0000001|WGT=10.000", msnEvents.Single().SL_Reference);
		}

		#endregion

		#region TestCreateWithdrawalSentLog

		public void TestCreateWithdrawalSentLog()
		{
			var logParent = Factory.New<WhsItemReceiveConsignment>();
			var notification = new CIN750InNotification("TransitReceive", "RC0000001");
			var data = notification.MakeDynamic();

			var logCreator = new CIN750InNotificationLogsCreator();
			AssertEquals(false, logCreator.CreateWithdrawalSentLog(logParent, data, "CIN750InNotification", "Bollore", "Test Reason"));
		}

		#endregion

		#region Implement

		protected WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory));
		WhsTransitTestHelper helper;

		#endregion
	}
}
