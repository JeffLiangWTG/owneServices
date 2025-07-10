using System.Linq;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.eTail.Business.Testing
{
	[TestedType(typeof(HVLVBookingHeader))]
	class HVLVBookingHeaderWorkflowProviderTest : WorkflowProviderTest<HVLVBookingHeader, HVLVBookingHeaderProcessTaskCollection>
	{
		public void TestGetTemplateSelectionCriteria_eTailer()
		{
			Header.BillToParty.OA_Code = "FLOOGERJOBBIN"; // because NewWithValidTestData creates duplicate values, causing unique index failure
			AssertGetTemplateFilterCriteria(Header.BillToParty.OA_OHInfo, ProcessTaskTemplate.P0_OH_ClientInfo, Client.PK, Client2.PK, ZGuid.Empty);
		}

		public void TestGetTemplateSelectionCriteria_Dispatch()
		{
			Header.HVH_OA_DispatchAddress = Header.Factory.NewWithValidTestData<OrgAddress>().PK;
			AssertGetTemplateFilterCriteria<ZString>(Header.DispatchAddress.OA_RL_NKRelatedPortCodeInfo, ProcessTaskTemplate.P0_LoadPortCountryInfo, "AUSYD", "USLAX", ZString.Empty);
		}

		public void TestGetTemplateSelectionCriteria_ServiceLevel()
		{
			AssertGetTemplateFilterCriteria<ZString>(Header.HVH_RS_NKBookingServiceLevelInfo, ProcessTaskTemplate.P0_SubType1Info, "STD", "D2D", ZString.Empty);
		}

		public void TestGetTemplateSelectionCriteria_Branch()
		{
			var company = Header.Factory.NewWithValidTestData<GlbCompany>();

			var branch1 = company.Branches.AddNew();
			branch1.GB_Code = "ABC";
			var branch2 = company.Branches.AddNew();
			branch2.GB_Code = "XYZ";

			ProcessTaskTemplate.P0_GC = company.PK;
			ProcessTaskTemplate.P0_GB = branch2.PK;
			ProcessTaskTemplate.P0_ProcessType = WorkflowDescriptors.HVLVBookingHeaderWorkflowDescriptorCode;

			Header.Factory.Save();

			AssertEquals("Precondition; ProcessTaskTemplate has a Task", 1, ProcessTaskTemplate.WorkflowItems.Tasks.Count);

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch1.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				Header.WorkflowItems.Tasks.CreateItemsFromTemplate();
				AssertEquals(0, Header.WorkflowItems.Tasks.Count);
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch2.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				Header.WorkflowItems.Tasks.CreateItemsFromTemplate();
				AssertEquals(1, Header.WorkflowItems.Tasks.Count);
			}
		}

		public void TestIsBookingConfirmed_CreatedAsFalse_ShouldNotAddMSFLog()
		{
			var header = Factory.NewWithValidTestData<HVLVBookingHeader>();
			header.HVH_IsBookingConfirmed = false;
			Factory.Save();
			var msfLogs = header.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.ManifestSubmittedToForwarder.Code);
			AssertEquals("There is no MSF event when created booking header is booked", 0, msfLogs.Count());
		}

		public void TestIsBookingConfirmed_CreatedAsTrue_ShouldAddMSFLog()
		{
			var header = Factory.NewWithValidTestData<HVLVBookingHeader>();
			header.HVH_IsBookingConfirmed = true;
			Factory.Save();
			var msfLogs = header.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.ManifestSubmittedToForwarder.Code && !log.SL_IsCancelled);
			AssertEquals("There is one non-cancelled MSF event when created booking header is confirmed", 1, msfLogs.Count());
		}

		public void TestIsBookingConfirmed_WhenChangeToTrueAndSave_ShouldAddMSFLog()
		{
			var header = Factory.NewWithValidTestData<HVLVBookingHeader>();
			header.HVH_IsBookingConfirmed = false;
			Factory.Save();

			header.HVH_IsBookingConfirmed = true;
			Factory.Save();
			var msfLogs = header.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.ManifestSubmittedToForwarder.Code && !log.SL_IsCancelled);
			AssertEquals("One non-cancelled MSF event is added after changing booked => confirmed", 1, msfLogs.Count());
		}

		public void TestIsBookingConfirmed_WhenChangeToFalseAndSave_ShouldCancelLatestMSFLog()
		{
			var header = Factory.NewWithValidTestData<HVLVBookingHeader>();

			header.HVH_IsBookingConfirmed = true;
			Factory.Save();

			var msfLogs = header.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.ManifestSubmittedToForwarder.Code);
			AssertEquals("There is one MSF event after inserting confirmed", 1, msfLogs.Count());
			var nonCancelledMSFLog = header.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.ManifestSubmittedToForwarder.Code && !log.SL_IsCancelled);
			AssertEquals("There is one non-cancelled MSF event after inserting confirmed", 1, nonCancelledMSFLog.Count());

			header.HVH_IsBookingConfirmed = false;
			Factory.Save();

			msfLogs = header.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.ManifestSubmittedToForwarder.Code);
			AssertEquals("There is one MSF events after changing confirmed => booked", 1, msfLogs.Count());
			var cancelledMSFLog = header.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.ManifestSubmittedToForwarder.Code && log.SL_IsCancelled);
			AssertEquals("There is one cancelled MSF event after changing confirmed => booked", 1, cancelledMSFLog.Count());
		}

		public void TestIsBookingConfirmed_WhenIsTrueAndSaveWithoutChange_ShouldNotAddMSFLog()
		{
			var header = Factory.NewWithValidTestData<HVLVBookingHeader>();

			header.HVH_IsBookingConfirmed = true;
			Factory.Save();
			var firstMSFLogCount = header.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.ManifestSubmittedToForwarder.Code).Count();
			var firstNonCancelledMSFLogCount = header.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.ManifestSubmittedToForwarder.Code && !log.SL_IsCancelled).Count();

			header.HVH_ClusterKey = 0;
			Factory.Save();
			var secondMSFLogCount = header.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.ManifestSubmittedToForwarder.Code).Count();
			var secondNonCancelledMSFLogCount = header.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.ManifestSubmittedToForwarder.Code && !log.SL_IsCancelled).Count();
			AssertEquals("No new MSF event is added if save without changing confirmed", firstMSFLogCount, secondMSFLogCount);
			AssertEquals("No MSF event changes status if save without changing confirmed", firstNonCancelledMSFLogCount, secondNonCancelledMSFLogCount);
		}

		public void TestIsBookingConfirmed_WhenIsFalseAndSaveWithoutChange_ShouldNotCancelLatestMSFLog()
		{
			var header = Factory.NewWithValidTestData<HVLVBookingHeader>();

			header.HVH_IsBookingConfirmed = false;
			Factory.Save();

			header.Logs.AddNew(AutoEvents.ManifestSubmittedToForwarder);

			var firstMSFLogCount = header.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.ManifestSubmittedToForwarder.Code).Count();
			var firstNonCancelledMSFLogCount = header.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.ManifestSubmittedToForwarder.Code && !log.SL_IsCancelled).Count();

			header.HVH_ClusterKey = 0;
			Factory.Save();
			var secondMSFLogCount = header.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.ManifestSubmittedToForwarder.Code).Count();
			var secondNonCancelledMSFLogCount = header.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.ManifestSubmittedToForwarder.Code && !log.SL_IsCancelled).Count();
			AssertEquals("No new MSF event is added if save without changing confirmed", firstMSFLogCount, secondMSFLogCount);
			AssertEquals("No MSF event changes status if save without changing confirmed", firstNonCancelledMSFLogCount, secondNonCancelledMSFLogCount);
		}

		public void TestIsBookingConfirmed_WhenChangeToTrueAndNotSave_ShouldNotAddMSFLog()
		{
			var header = Factory.NewWithValidTestData<HVLVBookingHeader>();

			header.HVH_IsBookingConfirmed = false;
			Factory.Save();

			var firstMSFLogCount = header.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.ManifestSubmittedToForwarder.Code).Count();

			header.HVH_IsBookingConfirmed = true;

			var secondMSFLogCount = header.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.ManifestSubmittedToForwarder.Code).Count();

			AssertEquals("No new MSF event is added if changing to confirmed without save", firstMSFLogCount, secondMSFLogCount);
		}

		public void TestIsBookingConfirmed_WhenChangeToFalseAndNotSave_ShouldNotCancelLatestMSFLog()
		{
			var header = Factory.NewWithValidTestData<HVLVBookingHeader>();

			header.HVH_IsBookingConfirmed = true;
			Factory.Save();

			var firstMSFLogCount = header.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.ManifestSubmittedToForwarder.Code).Count();
			var firstNonCancelledMSFLogCount = header.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.ManifestSubmittedToForwarder.Code && !log.SL_IsCancelled).Count();

			header.HVH_IsBookingConfirmed = false;

			var secondMSFLogCount = header.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.ManifestSubmittedToForwarder.Code).Count();
			var secondNonCancelledMSFLogCount = header.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.ManifestSubmittedToForwarder.Code && !log.SL_IsCancelled).Count();
			AssertEquals("No new MSF event is added if changing to booked without save", firstMSFLogCount, secondMSFLogCount);
			AssertEquals("No MSF event changes status if changing to booked without save", firstNonCancelledMSFLogCount, secondNonCancelledMSFLogCount);
		}

		#region Implementation

		protected override ZString ExpectedWorkflowType
		{
			get { return WorkflowDescriptors.HVLVBookingHeaderWorkflowDescriptorCode; }
		}

		HVLVBookingHeader Header => BusinessObject;

		#endregion
	}
}
