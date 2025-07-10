using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.LVS.Business.Testing
{
	public class MessageSendingHelperTest : TestCaseWithFactory
	{
		public void TestIsConsignmentPendingForSendOriginalMessage()
		{
			var consignment1 = Factory.New<CusUSLVConsignment>();
			consignment1.ULB_MessageStatus = ImportMessageStatusList.Codes.OriginalRequestPending;

			var consignment2 = Factory.New<CusUSLVConsignment>();
			consignment2.ULB_MessageStatus = ImportMessageStatusList.Codes.ClearArrival;

			Assert(MessageSendingHelper.IsConsignmentPendingForSendOriginalMessage(consignment1));
			Assert(!MessageSendingHelper.IsConsignmentPendingForSendOriginalMessage(consignment2));
		}

		public void TestIsConsignmentNotWaitingForResponse()
		{
			var consignment1 = Factory.New<CusUSLVConsignment>();
			consignment1.ULB_MessageStatus = "AAV";

			var consignment2 = Factory.New<CusUSLVConsignment>();
			consignment2.ULB_MessageStatus = "CAV";

			AssertEquals("Consignment1 should be waiting for response", false, MessageSendingHelper.IsConsignmentNotWaitingForResponse(consignment1));
			AssertEquals("Consignment2 should not be waiting for response", true, MessageSendingHelper.IsConsignmentNotWaitingForResponse(consignment2));
		}

		public void TestHasValidStatusOnConsignment_SendingOriginalMessage()
		{
			var consignment = Factory.New<CusUSLVConsignment>();
			consignment.ULB_MessageStatus = "AAV";
			consignment.InitAction(Messaging.Business.UpdateActionCode.Add);

			AssertEquals("Should have valid status", true, MessageSendingHelper.HasValidStatusOnConsignment(consignment));
		}

		public void TestHasValidStatusOnConsignment_SendingOriginalMessage_EntryStatusNotEmpty()
		{
			var consignment = Factory.New<CusUSLVConsignment>();
			var entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_EntryType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entryNumber.CE_EntryStatus = CRLReleaseStatusList.Codes.HLD;
			entryNumber.CE_ParentID = consignment.PK;
			consignment.InitAction(Messaging.Business.UpdateActionCode.Add);

			AssertEquals("Should have invalid status", false, MessageSendingHelper.HasValidStatusOnConsignment(consignment));
		}

		public void TestHasValidStatusOnConsignment_SendingOriginalMessage_MessageStatusIsCSA()
		{
			var consignment = Factory.New<CusUSLVConsignment>();
			consignment.ULB_MessageStatus = "CSA";
			consignment.InitAction(Messaging.Business.UpdateActionCode.Add);

			AssertEquals("Should have invalid status", false, MessageSendingHelper.HasValidStatusOnConsignment(consignment));
		}

		public void TestHasValidStatusOnConsignment_SendingReplacementMessage()
		{
			var consignment = Factory.New<CusUSLVConsignment>();
			var entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_EntryType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entryNumber.CE_EntryStatus = CRLReleaseStatusList.Codes.HLD;
			entryNumber.CE_ParentID = consignment.PK;
			consignment.InitAction(Messaging.Business.UpdateActionCode.Replace);

			AssertEquals("Should have valid status", true, MessageSendingHelper.HasValidStatusOnConsignment(consignment));
		}

		public void TestHasValidStatusOnConsignment_SendingReplacementMessage_EntryStatusIsEmpty()
		{
			var consignment = Factory.New<CusUSLVConsignment>();
			consignment.InitAction(Messaging.Business.UpdateActionCode.Replace);

			AssertEquals("Should have invalid status", false, MessageSendingHelper.HasValidStatusOnConsignment(consignment));
		}

		public void TestHasValidStatusOnConsignment_SendingReplacementMessage_EntryStatusNotValid()
		{
			var consignment = Factory.New<CusUSLVConsignment>();
			var entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_EntryType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entryNumber.CE_EntryStatus = CRLReleaseStatusList.Codes.CAN;
			entryNumber.CE_ParentID = consignment.PK;
			consignment.InitAction(Messaging.Business.UpdateActionCode.Replace);

			AssertEquals("Should have invalid status", false, MessageSendingHelper.HasValidStatusOnConsignment(consignment));
		}

		public void TestHasValidStatusOnConsignment_SendingUpdateMessage()
		{
			var consignment = Factory.New<CusUSLVConsignment>();
			var entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_EntryType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entryNumber.CE_EntryStatus = CRLReleaseStatusList.Codes.REL;
			entryNumber.CE_ParentID = consignment.PK;
			consignment.InitAction(Messaging.Business.UpdateActionCode.Update);
			AssertEquals("Should have valid status", true, MessageSendingHelper.HasValidStatusOnConsignment(consignment));

			entryNumber.CE_EntryStatus = CRLReleaseStatusList.Codes.HLD;
			AssertEquals("Should have valid status", true, MessageSendingHelper.HasValidStatusOnConsignment(consignment));
		}

		public void TestHasValidStatusOnConsignment_SendingUpdateMessage_EntryStatusIsEmpty()
		{
			var consignment = Factory.New<CusUSLVConsignment>();
			var entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_EntryType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entryNumber.CE_EntryStatus = ZString.Empty;
			entryNumber.CE_ParentID = consignment.PK;
			consignment.InitAction(Messaging.Business.UpdateActionCode.Update);

			AssertEquals("Should have invalid status", false, MessageSendingHelper.HasValidStatusOnConsignment(consignment));
		}

		public void TestHasValidStatusOnConsignment_SendingUpdateMessage_EntryStatusNotValid()
		{
			var consignment = Factory.New<CusUSLVConsignment>();
			var entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_EntryType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entryNumber.CE_EntryStatus = CRLReleaseStatusList.Codes.CAN;
			entryNumber.CE_ParentID = consignment.PK;
			consignment.InitAction(Messaging.Business.UpdateActionCode.Update);
			AssertEquals("Should have invalid status", false, MessageSendingHelper.HasValidStatusOnConsignment(consignment));

			entryNumber.CE_EntryStatus = CRLReleaseStatusList.Codes.DEL;
			AssertEquals("Should have invalid status", false, MessageSendingHelper.HasValidStatusOnConsignment(consignment));
		}

		public void TestHasValidStatusOnConsignment_SendingDeleteMessage()
		{
			var consignment = Factory.New<CusUSLVConsignment>();
			var entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_EntryType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entryNumber.CE_EntryStatus = CRLReleaseStatusList.Codes.REL;
			entryNumber.CE_ParentID = consignment.PK;
			consignment.InitAction(Messaging.Business.UpdateActionCode.Delete);

			AssertEquals("Should have valid status", true, MessageSendingHelper.HasValidStatusOnConsignment(consignment));
		}

		public void TestHasValidStatusOnConsignment_SendingDeleteMessage_EntryStatusIsEmpty()
		{
			var consignment = Factory.New<CusUSLVConsignment>();
			consignment.InitAction(Messaging.Business.UpdateActionCode.Delete);

			AssertEquals("Should have invalid status", false, MessageSendingHelper.HasValidStatusOnConsignment(consignment));
		}

		public void TestHasValidStatusOnConsignment_SendingDeleteMessage_EntryStatusNotValid()
		{
			var consignment = Factory.New<CusUSLVConsignment>();
			var entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_EntryType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entryNumber.CE_EntryStatus = CRLReleaseStatusList.Codes.CAN;
			entryNumber.CE_ParentID = consignment.PK;
			consignment.InitAction(Messaging.Business.UpdateActionCode.Delete);

			AssertEquals("Should have invalid status", false, MessageSendingHelper.HasValidStatusOnConsignment(consignment));
		}

		public void TestCheckHasMessageErrorsOnClearanceOrConsignment()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var staff = Factory.New<GlbStaff>();
			staff.GS_WorkPhone = "+886602134";
			staff.GS_LoginName = "HappyUser";
			staff.GS_FullName = "Clean";
			staff.GS_IsSystemAccount = false;
			staff.GS_GB_HomeBranch = branch.PK;
			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.GS_LoginName, branch.PK.ToGuid(), Guid.Empty))
			{
				var clearance = Factory.New<CusUSLVClearance>();
				clearance.ULH_ContactPhone = "123456";
				clearance.ULH_ContainerMode = "ABC";
				clearance.ULH_TransportMode = "AIR";
				clearance.ULH_EntryFilerCode = "XJ5";
				clearance.ULH_MasterBillIssuerSCAC = "AA";
				clearance.ULH_CarrierSCAC = "AA";
				clearance.ULH_DischargeDate = ZDate.Today;

				var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
				var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
				var helper = new UniversalReferenceTestDataHelper(Factory);
				helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
				helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "0011", "US11 Port of Discharge", startDate, endDate);
				Factory.Save();

				clearance.ULH_PortOfEntry = "0011";

				AssertEquals("Should have no message error", false, MessageSendingHelper.CheckHasMessageErrorsOnClearanceOrConsignment(clearance, clearance.CusUSLVConsignments.OfType<CusUSLVConsignment>()));
			}
		}

		public void TestCheckHasMessageErrorsOnClearanceOrConsignment_ClearanceHasMessageError()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();

			AssertEquals("Should have message error", true, MessageSendingHelper.CheckHasMessageErrorsOnClearanceOrConsignment(clearance, clearance.CusUSLVConsignments.OfType<CusUSLVConsignment>()));
		}

		public void TestCheckHasMessageErrorsOnClearanceOrConsignment_ChildrenConsignmentsHasMessageError()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_WorkPhone = "+886605555";
			staff.GS_LoginName = "TestMe";
			staff.GS_FullName = "Pass";
			staff.GS_IsSystemAccount = false;
			staff.GS_GB_HomeBranch = branch.PK;
			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.GS_LoginName, branch.PK.ToGuid(), Guid.Empty))
			{
				var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
				clearance.ULH_ContactPhone = "123456";
				clearance.ULH_ContainerMode = "ABC";
				clearance.ULH_TransportMode = "AIR";
				clearance.ULH_EntryFilerCode = "XJ5";
				clearance.ULH_MasterBillIssuerSCAC = "AA";
				clearance.ULH_CarrierSCAC = "AA";
				clearance.ULH_DischargeDate = ZDate.Today;

				var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
				var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
				var helper = new UniversalReferenceTestDataHelper(Factory);
				helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
				helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "0011", "US11 Port of Discharge", startDate, endDate);
				Factory.Save();

				clearance.ULH_PortOfEntry = "0011";

				AssertEquals("Clearance alone should have no message error", false, MessageSendingHelper.CheckHasMessageErrorsOnClearanceOrConsignment(clearance, clearance.CusUSLVConsignments.OfType<CusUSLVConsignment>()));

				clearance.CusUSLVConsignments.AddNew();

				AssertEquals("Should have message error with added consignment", true, MessageSendingHelper.CheckHasMessageErrorsOnClearanceOrConsignment(clearance, clearance.CusUSLVConsignments.OfType<CusUSLVConsignment>()));
			}
		}

		public void TestCheckHasMessageErrorsOnClearanceOrConsignment_SkipValidationForNonSelectedConsignments()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_WorkPhone = "+886605555";
			staff.GS_LoginName = "TestMe";
			staff.GS_FullName = "Pass";
			staff.GS_IsSystemAccount = false;
			staff.GS_GB_HomeBranch = branch.PK;
			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.GS_LoginName, branch.PK.ToGuid(), Guid.Empty))
			{
				var clearance = Factory.New<CusUSLVClearance>();
				clearance.ULH_ContactPhone = "123456";
				clearance.ULH_ContainerMode = "ABC";
				clearance.ULH_TransportMode = "AIR";
				clearance.ULH_EntryFilerCode = "XJ5";
				clearance.ULH_MasterBillIssuerSCAC = "AA";
				clearance.ULH_CarrierSCAC = "AA";
				clearance.ULH_DischargeDate = ZDate.Today;

				var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
				var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
				var helper = new UniversalReferenceTestDataHelper(Factory);
				helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
				helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "0011", "US11 Port of Discharge", startDate, endDate);
				Factory.Save();

				clearance.ULH_PortOfEntry = "0011";

				var consignment = Factory.New<CusUSLVConsignment>();
				consignment.RunPreSaveValidation();

				Assert("pre-condition", consignment.HasMessageErrors);

				clearance.CusUSLVConsignments.Add(consignment);

				AssertEquals("Should return false as no consignment is validated", false, MessageSendingHelper.CheckHasMessageErrorsOnClearanceOrConsignment(clearance, Enumerable.Empty<CusUSLVConsignment>()));
			}
		}

		public void TestGetFirstConsignmentHasMessageErrorsAndConsignmentsWithoutMessageErrors()
		{
			var consignment1 = Factory.New<CusUSLVConsignment>();
			consignment1.ULB_NumberOfPacks = 1;
			consignment1.FirstCusUSLVItemTariff = "2517.10.0015";
			consignment1.FirstCusUSLVItemCountryOfOrigin = "AU";
			consignment1.FirstCusUSLVItemLineValue = 5.0;
			SetUpConsignmentWithValidTestData(consignment1);

			var consignment2 = Factory.New<CusUSLVConsignment>();
			consignment2.ULB_NumberOfPacks = 0;
			SetUpConsignmentWithValidTestData(consignment2);

			var consignmentsForMessaging = new[] { new CusUSLVConsignmentForMessaging(consignment1), new CusUSLVConsignmentForMessaging(consignment2) };
			var firstConsignmentWithError = MessageSendingHelper.GetFirstConsignmentHasMessageErrors(consignmentsForMessaging);
			var consignmentsWithNoError = MessageSendingHelper.GetConsignmentsWithoutMessageErrors(consignmentsForMessaging);
			AssertEquals("First consignment with error should be consignment2", consignment2.PK, firstConsignmentWithError.Consignment.PK);
			AssertEquals("Should contain 1 consignment", 1, consignmentsWithNoError.Count());
			AssertEquals("Consignment with no error should be consignment1", consignment1.PK, consignmentsWithNoError.First().Consignment.PK);
		}

		#region Implmentation

		RefCusTaxOrFee deminimus;

		protected override void SetUp()
		{
			base.SetUp();
			if (deminimus == null)
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				deminimus = helper.CreateTaxOrFee("DEM", 1000m, Core.Constants.CountryCodes.UnitedStates);
				Factory.Save();
			}
		}

		void SetUpConsignmentWithValidTestData(CusUSLVConsignment consignment)
		{
			consignment.ULB_SellerName = "someone";
			consignment.ULB_SellerCity = "somewhere";
			consignment.ULB_SellerAddress1 = "somewhere";
			consignment.ULB_RN_NKSellerCountry = "AU";
			consignment.ULB_ConsigneeName = "Ian";
			consignment.ULB_ConsigneeCity = "syd";
			consignment.ULB_ConsigneeAddress1 = "xx";
			consignment.ULB_RN_NKConsigneeCountry = "AU";
		}

		#endregion
	}
}
