using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.Warehouse.Transit.Document.Common;
using Enterprise.Warehouse.Transit.Document.DocDataObjects;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Document.Testing
{
	public class CIN750ConsNotificationLogsCreatorTest : TestCaseWithFactory
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
			rcn.WRC_HouseBillNumber = "HSB1";
			dcn.WDC_HouseBillNumber = "HSB1";
			Helper.CreateAdditionalReference(dcn, "MAB-1", AdditionalReferenceTypes.Codes.MasterBill);

			var notification = new CIN750ConsNotification("TransitDispatch", "DC0000001");
			notification.SourceBusinessObject = dcn;

			var fromDocPackingLine = new DocPackingLine(ZGuid.NewZGuid())
			{
				RefType = new CodeDescription(new CIN750RefTypes())
				{
					Code = CIN750RefTypes.Codes.HouseAirWaybill
				},
				SourceType = nameof(DataContextType.TransitReceive),
				SourceID = rcn.WRC_JobID,
				AmountQuantity = 2,
				AmountWeight = 10,
				SourceReceiveConsignment = rcn
			};
			var toDocPackingLine = new DocPackingLine(ZGuid.NewZGuid())
			{
				RefType = new CodeDescription(new CIN750RefTypes())
				{
					Code = CIN750RefTypes.Codes.MasterAirWaybill
				},
				SourceType = WhsItemDispatchConsignmentSchema.Constants.Prefix,
				SourceID = rcn.WRC_JobID,
				AmountQuantity = 1,
				AmountWeight = 10,
				SourceReceiveConsignment = rcn
			};

			notification.FromGoods = new List<DocPackingLine>()
			{
				fromDocPackingLine
			};
			notification.ToGoods = toDocPackingLine;

			var data = notification.MakeDynamic();

			var logCreator = new CIN750ConsNotificationLogsCreator();
			logCreator.CreateMessageSentLog(dcn, data, "CIN750ConsNotification", "Test");

			var msnEvents = dcn.Logs.GetAllLogs().OfType<StmALog>().Where(log => log.SL_SE_NKEvent == Events.MessageSentCode);
			var logReferences = msnEvents.Select(log => log.SL_Reference).ToArray();

			AssertEquals(2, logReferences.Length);
			AssertCollectionContains("Cons from good notification log", "|ARG=241210143301000|HBL=HSB1|JOB=RC0000001|MBL=-|MST=CIN750ConsNotification_From|OTY=2|PTP=HWB|RFN=EDIDATRC0000001|WGT=10.000", logReferences);
			AssertCollectionContains("Cons to good notification log", "|ARG=241210143301000|HBL=HSB1|JOB=DC0000001|MBL=MAB-1|MST=CIN750ConsNotification_To|OTY=1|PTP=AWB|RFN=EDIDATDC0000001|WGT=10.000", logReferences);
		}

		#endregion

		#region TestCreateWithdrawalSentLog

		public void TestCreateWithdrawalSentLog()
		{
			var logParent = Factory.New<WhsItemDispatchConsignment>();
			var rcn = Factory.New<WhsItemReceiveConsignment>();
			rcn.WRC_JobID = "RC0000001";
			var notification = new CIN750ConsNotification("TransitDispatch", "DC0000001");

			var data = notification.MakeDynamic();

			var logCreator = new CIN750ConsNotificationLogsCreator();
			AssertEquals(false, logCreator.CreateWithdrawalSentLog(logParent, data, "CIN750ConsNotification", "Bollore", "Test Reason"));
		}

		#endregion

		protected WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory));
		WhsTransitTestHelper helper;
	}
}
