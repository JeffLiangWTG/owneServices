using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
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
	public class CIN750DeconsNotificationLogsCreatorTest : TestCaseWithFactory
	{
		#region TestCreateMessageSentLog

		[TestDate(2024, 12, 10, 14, 33, 1)]
		public void TestCreateMessageSentLog()
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch("WH1");
			warehouse.WarehouseAddress.Address1 = "WH1Address";
			Helper.AddOrgCode(warehouse.WarehouseAddress, OrgCusCode.FranceCodeTypes.CIN, "C001");
			var dcn = Helper.CreateDispatchConsignment("DC0000001", warehouse.PK);
			var rcn = Helper.CreateReceiveConsignment("RC0000001", warehouse.PK);

			Helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.TempStorageDeclaration, "TST1");
			Helper.CreateAdditionalReference(rcn, "MAB-1", AdditionalReferenceTypes.Codes.MasterBill);
			rcn.WRC_HouseBillNumber = "HSB1";
			dcn.WDC_HouseBillNumber = "HSB1";

			var notification = new CIN750DeconsNotification("TransitDispatch", "DC0000001");
			notification.SourceBusinessObject = dcn;

			var fromDocPackingLine = new DocPackingLine(ZGuid.NewZGuid())
			{
				SourceID = rcn.WRC_JobID,
				AmountQuantity = 5,
				AmountWeight = 10,
				SourceReceiveConsignment = rcn,
				RefType = new CodeDescription(new CIN750RefTypes())
				{
					Code = CIN750RefTypes.Codes.MasterAirWaybill
				}
			};
			var toDocPackingLine = new DocPackingLine(ZGuid.NewZGuid())
			{
				SourceID = rcn.WRC_JobID,
				AmountQuantity = 2,
				AmountWeight = 10,
				SourceReceiveConsignment = rcn,
				RefType = new CodeDescription(new CIN750RefTypes())
				{
					Code = CIN750RefTypes.Codes.HouseAirWaybill
				}
			};

			notification.GoodsPairs = new List<Tuple<DocPackingLine, DocPackingLine>>() { new Tuple<DocPackingLine, DocPackingLine>(fromDocPackingLine, toDocPackingLine) };
			notification.RefType = new CodeDescription(new CIN750RefTypes())
			{
				Code = CIN750RefTypes.Codes.HouseAirWaybill
			};
			notification.SourceBusinessObject = dcn;
			var data = notification.MakeDynamic();

			var logCreator = new CIN750DeconsNotificationLogsCreator();
			logCreator.CreateMessageSentLog(dcn, data, "CIN750DeconsNotification", "Test");

			var msnEvents = dcn.Logs.GetAllLogs().OfType<StmALog>().Where(log => log.SL_SE_NKEvent == Events.MessageSentCode);
			var logReferences = msnEvents.Select(log => log.SL_Reference).ToArray();

			AssertEquals(2, logReferences.Length);
			AssertCollectionContains("RCN Decons notification log", "|ARG=241210143301000|HBL=HSB1|JOB=RC0000001|MBL=MAB-1|MST=CIN750DeconsNotification_From|OTY=5|PTP=AWB|RFN=EDIDATRC0000001|WGT=10.000", logReferences);
			AssertCollectionContains("DCN Decons notification log", "|ARG=241210143301000|HBL=HSB1|JOB=RC0000001|MBL=-|MST=CIN750DeconsNotification_To|OTY=2|PTP=HWB|RFN=EDIDATDC0000001|WGT=10.000", logReferences);
		}

		#endregion

		#region TestCreateWithdrawalSentLog

		public void TestCreateWithdrawalSentLog()
		{
			var logParent = Factory.New<WhsItemDispatchConsignment>();
			var rcn = Factory.New<WhsItemReceiveConsignment>();
			rcn.WRC_JobID = "RC0000001";
			var notification = new CIN750DeconsNotification("TransitDispatch", "DC0000001");

			var data = notification.MakeDynamic();

			var logCreator = new CIN750DeconsNotificationLogsCreator();
			AssertEquals(false, logCreator.CreateWithdrawalSentLog(logParent, data, "CIN750DeconsNotification", "Bollore", "Test Reason"));
		}

		#endregion

		protected WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory));
		WhsTransitTestHelper helper;
	}
}
