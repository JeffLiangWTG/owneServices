using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.eTail.Business.Testing
{
	public class HVLVCustomsStatusServiceTest : TestCaseWithFactory
	{
		public void TestGetService()
		{
			var customsStatusService = HVLVCustomsStatusService.GetService(Factory);
			AssertNotNull(customsStatusService);
			AssertEquals("HVLVCustomsStatusService is cached", customsStatusService, HVLVCustomsStatusService.GetService(Factory));
		}

		public void TestGetReleaseStatusAndCustomsStatusDescription()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Latvia))
			{
				HVLVCustomsStatusTestHelper.AddRefCusCodeList(Factory, "UUU", "CSTA - Description Test", HVLVReleaseStatus.Held, RefCusCodeListTypes.Codes.CustomsStatus, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				HVLVCustomsStatusTestHelper.AddRefCusCodeList(Factory, "KKK", "CSTI - Description Test", HVLVReleaseStatus.Cleared, RefCusCodeListTypes.Codes.CustomsStatusForInterface, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

				var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
				var consignment = bookingHeader.Consignments.AddNew();
				CombineAssertions(() =>
				{
					AssertEquals("precondition - HVC_ReleaseStatus is NON by default", HVLVReleaseStatus.None, consignment.HVC_ReleaseStatus);
					AssertEquals("precondition - HVC_ImportReleaseStatus is NON by default", HVLVReleaseStatus.None, consignment.HVC_ImportReleaseStatus);
					AssertEquals("precondition - ReleaseStatusDescription is None by default", HVLVReleaseStatus.NoneDescription, consignment.ReleaseStatusDescription);
					AssertEquals("precondition - CustomsStatusDescription is empty by default", string.Empty, consignment.HVC_ImportCustomsClearanceStatus);
				});

				consignment.HVC_ImportCustomsClearanceStatus = "UUU";
				CombineAssertions(() =>
				{
					AssertEquals("HVC_ReleaseStatus should be Held", HVLVReleaseStatus.Held, consignment.HVC_ReleaseStatus);
					AssertEquals("HVC_ImportReleaseStatus should be Held", HVLVReleaseStatus.Held, consignment.HVC_ImportReleaseStatus);
					AssertEquals("ReleaseStatusDescription should be Held", HVLVReleaseStatus.HeldDescription, consignment.ReleaseStatusDescription);
					AssertEquals("CustomsStatusDescription", "CSTA - Description Test", consignment.ImportCustomsClearanceStatusDescription);
				});

				var importDeclaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				importDeclaration.IsCancelled = false;
				consignment.HVC_JE_ImportDeclaration = importDeclaration.PK;
				consignment.HVC_ImportCustomsClearanceStatus = "KKK";
				CombineAssertions(() =>
				{
					AssertEquals("HVC_ReleaseStatus should be Cleared", HVLVReleaseStatus.Cleared, consignment.HVC_ReleaseStatus);
					AssertEquals("HVC_ImportReleaseStatus should be Cleared", HVLVReleaseStatus.Cleared, consignment.HVC_ImportReleaseStatus);
					AssertEquals("ReleaseStatusDescription should be Cleared", HVLVReleaseStatus.ClearedDescription, consignment.ReleaseStatusDescription);
					AssertEquals("CustomsStatusDescription", "CSTI - Description Test", consignment.ImportCustomsClearanceStatusDescription);
				});

				consignment.HVC_ImportCustomsClearanceStatus = "ZZZ";

				AssertContains("Detail info should be reported in Error Reporter",
					"Current Customs Status Codes is [ZZZ]\r\nOf type: [CSTI]\r\nSearched code list: [KKK]",
					ErrorReporter.LastMessageReported);

				ErrorReporter.Clear();

				importDeclaration.IsCancelled = true;
				consignment.HVC_ImportCustomsClearanceStatus = "UUU";
				CombineAssertions(() =>
				{
					AssertEquals("HVC_ReleaseStatus should be Held", HVLVReleaseStatus.Held, consignment.HVC_ReleaseStatus);
					AssertEquals("HVC_ImportReleaseStatus should be Held", HVLVReleaseStatus.Held, consignment.HVC_ImportReleaseStatus);
					AssertEquals("ReleaseStatusDescription should be Held", HVLVReleaseStatus.HeldDescription, consignment.ReleaseStatusDescription);
					AssertEquals("CustomsStatusDescription", "CSTA - Description Test", consignment.ImportCustomsClearanceStatusDescription);
				});
			}
		}
	}
}
