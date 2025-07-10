using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DeniedPartyScreening.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.MasterFiles.GUI.Tests
{
	public class DpsMessageInfoTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			var sourceBizO = new List<DpsSourceWithParties>();
			var messageInfo = new DpsMessageInfoForTest(sourceBizO, true, 3);

			CombineAssertions(() =>
			{
				AssertEquals(sourceBizO, messageInfo.SourceBizOsExposed);
				AssertEquals(true, messageInfo.NeedShowMessageExposed);
				AssertEquals(3, messageInfo.ScreenedItemsCountExposed);
			});

			messageInfo = new DpsMessageInfoForTest(sourceBizO, false, 3);
			AssertEquals(false, messageInfo.NeedShowMessageExposed);

			messageInfo.AddResynchronizeStatus_Exposed(new DpsSourceWithParties(Factory.New<OrgHeader>(), Array.Empty<ScreeningParty>()), ScreeningStatusesList.Codes.JobCleared);
			AssertEquals(true, messageInfo.NeedShowMessageExposed);
		}

		public void TestAddFinalStatus()
		{
			var messageInfo = new DpsMessageInfoForTest(null, false, 0);
			AssertEquals(0, messageInfo.FinalStatusList_Exposed.Count);

			var pk = ZGuid.NewZGuid();
			messageInfo.AddFinalStatus_Exposed(pk, "ABC");

			CombineAssertions(() =>
			{
				AssertEquals(1, messageInfo.FinalStatusList_Exposed.Count);
				AssertEquals(pk, messageInfo.FinalStatusList_Exposed[0].PK);
				AssertEquals("ABC", messageInfo.FinalStatusList_Exposed[0].Status);
			});
		}

		public void TestShowMessageIfNeeded_ShowMessage()
		{
			InitTestInfo(out var sourceBizO, out var org1, out var org2);

			var messageInfo = new DpsMessageInfoForTest(sourceBizO, false, 2);
			messageInfo.ShowMessageIfNeeded_Exposed();
			AssertNullOrEmpty("No message showed", UnitTestUserNotification.Instance.LastMessage.Text);

			messageInfo = new DpsMessageInfoForTest(sourceBizO, true, 2);
			messageInfo.ShowMessageIfNeeded_Exposed();
			AssertNullOrEmpty("No message showed", UnitTestUserNotification.Instance.LastMessage.Text);

			messageInfo.AddFinalStatus_Exposed(org1.PK, ScreeningStatusesList.Codes.Clear);
			messageInfo.ShowMessageIfNeeded_Exposed();
			AssertNullOrEmpty("No message showed", UnitTestUserNotification.Instance.LastMessage.Text);

			messageInfo.AddFinalStatus_Exposed(org2.PK, ScreeningStatusesList.Codes.Matched);
			messageInfo.ShowMessageIfNeeded_Exposed();
			AssertNotNull("Message showed", UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessages();
			messageInfo = new DpsMessageInfoForTest(sourceBizO, false, 2);
			messageInfo.AddResynchronizeStatus_Exposed(new DpsSourceWithParties(org2, Array.Empty<ScreeningParty>()), ScreeningStatusesList.Codes.Matched);
			messageInfo.ShowMessageIfNeeded_Exposed();
			AssertNotNull("Message showed", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestShowMessageIfNeeded_ShowScreenedRecordsCount()
		{
			InitTestInfo(out var sourceBizO, out var org1, out _);
			var messageInfo = new DpsMessageInfoForTest(sourceBizO, true, 10);
			messageInfo.AddFinalStatus_Exposed(org1.PK, ScreeningStatusesList.Codes.Matched);
			messageInfo.ShowMessageIfNeeded_Exposed();
			AssertContains("10 Record(s) Screened.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestShowMessageIfNeeded_ShowUpdateOrNotStayInfo()
		{
			var statusList = new ScreeningStatusesList();
			InitTestInfo(out var sourceBizO, out var org1, out var org2);
			var messageInfo = new DpsMessageInfoForTest(sourceBizO, true, 10);
			messageInfo.AddFinalStatus_Exposed(org1.PK, ScreeningStatusesList.Codes.Matched);
			messageInfo.ShowMessageIfNeeded_Exposed();
			AssertContains($"{org1.OH_Code} will be updated to {ScreeningStatusesList.Codes.Matched} - {statusList.GetDescriptionFromCode(ScreeningStatusesList.Codes.Matched)}.", UnitTestUserNotification.Instance.LastMessage.Text);

			org2.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			messageInfo.ShowMessageIfNeeded_Exposed();
			AssertContains($"{org2.OH_Code} will remain as {ScreeningStatusesList.Codes.Matched} - {statusList.GetDescriptionFromCode(ScreeningStatusesList.Codes.Matched)}.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestShowMessageIfNeeded_ShowReason()
		{
			InitTestInfo(out var sourceBizO, out var org1, out var org2, out var org3, ScreeningStatusesList.Codes.Matched);
			var messageInfo = new DpsMessageInfoForTest(sourceBizO, true, 10);
			messageInfo.AddFinalStatus_Exposed(org1.PK, ScreeningStatusesList.Codes.Matched);
			messageInfo.AddFinalStatus_Exposed(org3.PK, ScreeningStatusesList.Codes.Unknown);
			messageInfo.ShowMessageIfNeeded_Exposed();
			AssertContains(@"There are 1 new MAT - Matched record(s) present.
There are 2 record(s) already screened.
There are 1 pre MAT - Matched record(s) present.
There are 1 record(s) has come back to UNK - Unknown.
", UnitTestUserNotification.Instance.LastMessage.Text);

			messageInfo.FinalStatusList_Exposed.Clear();
			messageInfo.AddFinalStatus_Exposed(org1.PK, ScreeningStatusesList.Codes.Matched);
			messageInfo.AddFinalStatus_Exposed(org2.PK, ScreeningStatusesList.Codes.Matched);
			messageInfo.AddFinalStatus_Exposed(org3.PK, ScreeningStatusesList.Codes.Matched);
			messageInfo.ShowMessageIfNeeded_Exposed();
			AssertContains(@"There are 3 new MAT - Matched record(s) present.
There are 1 record(s) already screened.
", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestShowMessageIfNeeded_AppendForceReScreenHint()
		{
			InitTestInfo(out var sourceBizO, out var org1, out _);
			org1.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			var messageInfo = new DpsMessageInfoForTest(sourceBizO, true, 10);
			messageInfo.ShowMessageIfNeeded_Exposed();
			AssertContains("To screen records that are CLR or MAT please use the Force Re-Screen option.", UnitTestUserNotification.Instance.LastMessage.Text);

			messageInfo.AddFinalStatus_Exposed(org1.PK, ScreeningStatusesList.Codes.Matched);
			messageInfo.ShowMessageIfNeeded_Exposed();
			AssertNotContains("To screen records that are CLR or MAT please use the Force Re-Screen option.", UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessages();
			messageInfo = new DpsMessageInfoForTest(sourceBizO, true, 0);
			messageInfo.AddFinalStatus_Exposed(org1.PK, ScreeningStatusesList.Codes.Matched);
			messageInfo.ShowMessageIfNeeded_Exposed();
			AssertContains("To screen records that are CLR or MAT please use the Force Re-Screen option.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestGetJobCodeAndType()
		{
			var org = (BusinessObject)Factory.New<IOrgHeader>();
			org[OrgHeaderSchema.OH_Code] = "TESTORG";

			var shipment = Factory.New<IForwardingShipment>();

			var declaration = Factory.New<IBaseJobDeclaration>();
			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			declaration.JE_JS = shipment.PK;

			var consol = Factory.New<IForwardingConsol>();

			var vessel = (BusinessObject)Factory.New<IRefVessel>();
			vessel[RefVesselSchema.RV_Code] = "ABJEXE SIMPLE";

			var whsOrder = Factory.New<IWhsOrder>();
			(whsOrder as BusinessObject).FillWithValidTestData();

			var whsReceive = Factory.New<IWhsReceive>();
			(whsReceive as BusinessObject).FillWithValidTestData();
			Factory.Save();

			var bookingHeader = (BusinessObject)Factory.New<eTail.Integration.IHVLVBookingHeader>();
			bookingHeader[HVLVBookingHeaderSchema.HVH_BookingReference] = "M0001";

			CombineAssertions(() =>
			{
				AssertEquals(("TESTORG", false), DpsMessageInfoForTest.GetJobCodeInfo_Exposed(org));
				AssertEquals(("ABJEXE SIMPLE", false), DpsMessageInfoForTest.GetJobCodeInfo_Exposed(vessel));
				AssertEquals(((string)consol.JK_UniqueConsignRef, true), DpsMessageInfoForTest.GetJobCodeInfo_Exposed((BusinessObject)consol));
				AssertEquals(("Doc Address", false), DpsMessageInfoForTest.GetJobCodeInfo_Exposed(Factory.New<JobDocAddress>()));
				AssertEquals(($"Declaration {declaration.JE_DeclarationReference}", true), DpsMessageInfoForTest.GetJobCodeInfo_Exposed((BusinessObject)declaration));
				AssertEquals(((string)shipment.JS_UniqueConsignRef, true), DpsMessageInfoForTest.GetJobCodeInfo_Exposed((BusinessObject)shipment));
				AssertEquals(((string)whsOrder.WD_DocketID, true), DpsMessageInfoForTest.GetJobCodeInfo_Exposed((BusinessObject)whsOrder));
				AssertEquals(("M0001", false), DpsMessageInfoForTest.GetJobCodeInfo_Exposed(bookingHeader));
				AssertEquals(((string)whsReceive.WD_DocketID, true), DpsMessageInfoForTest.GetJobCodeInfo_Exposed((BusinessObject)whsReceive));
			});

			declaration.JE_JS = ZGuid.Empty;
			AssertEquals((declaration.JE_DeclarationReference.ToString(), true), DpsMessageInfoForTest.GetJobCodeInfo_Exposed((BusinessObject)declaration));
		}

		public void TestNoRecordsHasBeenScreenedMessage()
		{
			InitTestInfo(out var sourceBizO, out var org, out _);
			org.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			var messageInfo = new DpsMessageInfoForTest(sourceBizO, true, 0);
			messageInfo.ShowMessageIfNeeded_Exposed();
			AssertContains("No Records Have Been Screened.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestResynchronizeMessage_JobResynchronized()
		{
			var statusList = new ScreeningStatusesList();
			var shipment = Factory.New<IForwardingShipment>();
			Factory.Save();

			shipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;

			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;

			var sourceBizOs = new List<DpsSourceWithParties>
			{
				new DpsSourceWithParties(shipment as BusinessObject, new [] { new ScreeningParty(shipment as BusinessObject, "Dummy", org) })
			};

			var messageInfo = new DpsMessageInfoForTest(sourceBizOs, true, 10);
			messageInfo.AddResynchronizeStatus_Exposed(new DpsSourceWithParties((BusinessObject)shipment, Array.Empty<ScreeningParty>()), ScreeningStatusesList.Codes.Clear);
			messageInfo.ShowMessageIfNeeded_Exposed();
			AssertContains($"{shipment.JS_UniqueConsignRef} will be updated to {ScreeningStatusesList.Codes.Clear} - {statusList.GetDescriptionFromCode(ScreeningStatusesList.Codes.Clear)} as we have re-assessed the screening status based on the status of all parties on the job.", UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessages();
			messageInfo = new DpsMessageInfoForTest(sourceBizOs, false, 10);
			messageInfo.AddResynchronizeStatus_Exposed(new DpsSourceWithParties((BusinessObject)shipment, Array.Empty<ScreeningParty>()), ScreeningStatusesList.Codes.Clear);
			messageInfo.ShowMessageIfNeeded_Exposed();
			AssertContains($"You have canceled the screen results, {shipment.JS_UniqueConsignRef} will be updated to {ScreeningStatusesList.Codes.Clear} - {statusList.GetDescriptionFromCode(ScreeningStatusesList.Codes.Clear)} as we have re-assessed the screening status based on the status of all parties on the job.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestResynchronizeMessage_JobStayOriginalScreeningStatus()
		{
			var statusList = new ScreeningStatusesList();
			var shipment = Factory.New<IForwardingShipment>();
			Factory.Save();

			shipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.Matched;

			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;

			var sourceBizOs = new List<DpsSourceWithParties>
			{
				new DpsSourceWithParties(shipment as BusinessObject, new [] { new ScreeningParty(shipment as BusinessObject, "Dummy", org) })
			};

			var messageInfo = new DpsMessageInfoForTest(sourceBizOs, true, 10);
			messageInfo.ShowMessageIfNeeded_Exposed();
			AssertContains($"{shipment.JS_UniqueConsignRef} will remain as {ScreeningStatusesList.Codes.Matched} - {statusList.GetDescriptionFromCode(ScreeningStatusesList.Codes.Matched)} based on the screening status of all parties on the job. The screen action will not override a processed record.", UnitTestUserNotification.Instance.LastMessage.Text);

			var whsOrder = Factory.New<IWhsOrder>() as BusinessObject;
			whsOrder[WhsDocketSchema.WD_ScreeningStatus] = ScreeningStatusesList.Codes.Matched;
			whsOrder[WhsDocketSchema.WD_DocketID] = "W00001001";
			sourceBizOs = new List<DpsSourceWithParties>
			{
				new DpsSourceWithParties(whsOrder, new [] { new ScreeningParty(whsOrder, "Dummy", org) })
			};

			messageInfo = new DpsMessageInfoForTest(sourceBizOs, true, 10);
			messageInfo.ShowMessageIfNeeded_Exposed();
			AssertContains($"W00001001 will remain as {ScreeningStatusesList.Codes.Matched} - {statusList.GetDescriptionFromCode(ScreeningStatusesList.Codes.Matched)} based on the screening status of all parties on the job. The screen action will not override a processed record.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		#region Implementation

		void InitTestInfo(out List<DpsSourceWithParties> sourceBizO, out OrgHeader org1, out OrgHeader org2)
		{
			UnitTestUserNotification.Instance.ClearMessages();

			org1 = Factory.New<OrgHeader>();
			org2 = Factory.New<OrgHeader>();

			org1.OH_Code = "DPSTEST1";
			org2.OH_Code = "DPSTEST2";

			org1.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			org2.OH_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;

			sourceBizO = new List<DpsSourceWithParties>()
			{
				new DpsSourceWithParties(org1, new [] { new ScreeningParty(org1, "Dummy", org1) }),
				new DpsSourceWithParties(org2, new [] { new ScreeningParty(org2, "Dummy", org2) })
			};
		}

		void InitTestInfo(out List<DpsSourceWithParties> sourceBizO, out OrgHeader org1, out OrgHeader org2, out OrgHeader org3, string org2ScreenStatus)
		{
			UnitTestUserNotification.Instance.ClearMessages();

			org1 = Factory.New<OrgHeader>();
			org2 = Factory.New<OrgHeader>();
			org3 = Factory.New<OrgHeader>();
			OrgHeader org4 = Factory.New<OrgHeader>();

			org1.OH_Code = "DPSTEST1";
			org2.OH_Code = "DPSTEST2";
			org3.OH_Code = "DPSTEST3";
			org4.OH_Code = "DPSTEST4";

			org1.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			org2.OH_ScreeningStatus = org2ScreenStatus;
			org3.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			org4.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;

			sourceBizO = new List<DpsSourceWithParties>()
			{
				new DpsSourceWithParties(org1, new [] { new ScreeningParty(org1, "Dummy", org1) }),
				new DpsSourceWithParties(org2, new [] { new ScreeningParty(org2, "Dummy", org1), new ScreeningParty(org2, "Dummy", org2), new ScreeningParty(org2, "Dummy", org3), new ScreeningParty(org2, "Dummy", org4) })
			};
		}

		class DpsMessageInfoForTest : DpsMessageInfo
		{
			public DpsMessageInfoForTest(List<DpsSourceWithParties> sourceBizOs, bool shouldShowMessage, int screenedItemsCount) : base(sourceBizOs, shouldShowMessage, screenedItemsCount)
			{
			}

			public List<DpsSourceWithParties> SourceBizOsExposed => SourceBizOs;
			public bool NeedShowMessageExposed => NeedShowMessage;
			public int ScreenedItemsCountExposed => ScreenedItemsCount;
			public static (string JobCode, bool IsJob) GetJobCodeInfo_Exposed(BusinessObject bizO) => GetJobCodeInfo(bizO);
			public List<StatusInfo> FinalStatusList_Exposed => FinalStatusList;
			public void ShowMessageIfNeeded_Exposed() => ShowMessageIfNeeded();
			public void AddResynchronizeStatus_Exposed(DpsSourceWithParties dpsSourceWithParties, string status) => AddResynchronizeStatus(dpsSourceWithParties, status);
			public void AddFinalStatus_Exposed(ZGuid pk, string status) => AddFinalStatus(pk, status);
		}

		#endregion
	}
}
