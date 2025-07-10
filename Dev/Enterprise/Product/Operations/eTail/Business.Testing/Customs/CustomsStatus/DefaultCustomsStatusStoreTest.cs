using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.eTail.Business.Testing
{
	public class DefaultCustomsStatusStoreTest : TestCaseWithFactory
	{
		public void TestCreateCustomsStatusStore()
		{
			var ieCustomsStatusStore = DefaultCustomsStatusStore.CreateCustomsStatusStore(CountryCodes.Ireland, Factory);
			Assert(ieCustomsStatusStore is IECustomsStatusStore);

			var nzCustomsStatusStore = DefaultCustomsStatusStore.CreateCustomsStatusStore(CountryCodes.NewZealand, Factory);
			Assert(nzCustomsStatusStore is NZCustomsStatusStore);

			var auCustomsStatusStore = DefaultCustomsStatusStore.CreateCustomsStatusStore(CountryCodes.Australia, Factory);
			Assert(auCustomsStatusStore is AUCustomsStatusStore);

			var usCustomsStatusStore = DefaultCustomsStatusStore.CreateCustomsStatusStore(CountryCodes.UnitedStates, Factory);
			Assert(usCustomsStatusStore is USCustomsStatusStore);

			var ukCustomsStatusStore = DefaultCustomsStatusStore.CreateCustomsStatusStore(CountryCodes.UnitedKingdom, Factory);
			Assert(ukCustomsStatusStore is GBCustomsStatusStore);

			var esCustomsStatusStore = DefaultCustomsStatusStore.CreateCustomsStatusStore(CountryCodes.Spain, Factory);
			Assert(esCustomsStatusStore is ESCustomsStatusStore);

			var defaultCustomsStatusStore = DefaultCustomsStatusStore.CreateCustomsStatusStore(CountryCodes.SouthAfrica, Factory);
			Assert(defaultCustomsStatusStore is DefaultCustomsStatusStore);
		}

		public void TestTryGetReleaseStatusAndCustomsStatusDescription()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Latvia))
			{
				var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
				var consignment = bookingHeader.Consignments.AddNew();
				CombineAssertions(() =>
				{
					AssertEquals("precondition - HVC_ReleaseStatus is NON by default", HVLVReleaseStatus.None, consignment.HVC_ReleaseStatus);
					AssertEquals("precondition - HVC_ImportReleaseStatus is NON by default", HVLVReleaseStatus.None, consignment.HVC_ImportReleaseStatus);
					AssertEquals("precondition - ReleaseStatusDescription is None by default", HVLVReleaseStatus.NoneDescription, consignment.ReleaseStatusDescription);
					AssertEquals("precondition - CustomsStatusDescription is empty by default", string.Empty, consignment.HVC_ImportCustomsClearanceStatus);
				});

				HVLVCustomsStatusTestHelper.AddRefCusCodeList(Factory, "UUU", "CSTA - Description Test", HVLVReleaseStatus.Held, RefCusCodeListTypes.Codes.CustomsStatus, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				HVLVCustomsStatusTestHelper.AddRefCusCodeList(Factory, "UUU", "CSTI - Description Test", HVLVReleaseStatus.Cleared, RefCusCodeListTypes.Codes.CustomsStatusForInterface, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				consignment.HVC_ImportCustomsClearanceStatus = "UUU";
				CombineAssertions(() =>
				{
					AssertEquals("HVC_ReleaseStatus should be held", HVLVReleaseStatus.Held, consignment.HVC_ReleaseStatus);
					AssertEquals("HVC_ImportReleaseStatus should be held", HVLVReleaseStatus.Held, consignment.HVC_ImportReleaseStatus);
					AssertEquals("ReleaseStatusDescription should be held", HVLVReleaseStatus.HeldDescription, consignment.ReleaseStatusDescription);
					AssertEquals("CustomsStatusDescription", "CSTA - Description Test", consignment.ImportCustomsClearanceStatusDescription);
				});

				var importDeclaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				importDeclaration.IsCancelled = false;
				consignment.HVC_JE_ImportDeclaration = importDeclaration.PK;
				consignment.HVC_ImportCustomsClearanceStatus = string.Empty;
				consignment.HVC_ImportCustomsClearanceStatus = "UUU";
				CombineAssertions(() =>
				{
					AssertEquals("HVC_ReleaseStatus should be cleared", HVLVReleaseStatus.Cleared, consignment.HVC_ReleaseStatus);
					AssertEquals("HVC_ImportReleaseStatus should be cleared", HVLVReleaseStatus.Cleared, consignment.HVC_ImportReleaseStatus);
					AssertEquals("ReleaseStatusDescription should be cleared", HVLVReleaseStatus.ClearedDescription, consignment.ReleaseStatusDescription);
					AssertEquals("CustomsStatusDescription", "CSTI - Description Test", consignment.ImportCustomsClearanceStatusDescription);
				});

				importDeclaration.IsCancelled = true;
				consignment.HVC_ImportCustomsClearanceStatus = string.Empty;
				consignment.HVC_ImportCustomsClearanceStatus = "UUU";
				CombineAssertions(() =>
				{
					AssertEquals("HVC_ReleaseStatus should be held", HVLVReleaseStatus.Held, consignment.HVC_ReleaseStatus);
					AssertEquals("HVC_ImportReleaseStatus should be held", HVLVReleaseStatus.Held, consignment.HVC_ImportReleaseStatus);
					AssertEquals("ReleaseStatusDescription should be held", HVLVReleaseStatus.HeldDescription, consignment.ReleaseStatusDescription);
					AssertEquals("CustomsStatusDescription", "CSTA - Description Test", consignment.ImportCustomsClearanceStatusDescription);
				});
			}
		}

		public void TestGetAllRefCusCodeList()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Latvia))
			{
				var expectedCodes = new (string, string)[]
				{
					("UUU", "CSTA - Description Test"), ("KKK", "CSTI - Description Test"),
				};

				HVLVCustomsStatusTestHelper.AddRefCusCodeList(Factory, "UUU", "CSTA - Description Test", HVLVReleaseStatus.Held, RefCusCodeListTypes.Codes.CustomsStatus, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				HVLVCustomsStatusTestHelper.AddRefCusCodeList(Factory, "KKK", "CSTI - Description Test", HVLVReleaseStatus.Cleared, RefCusCodeListTypes.Codes.CustomsStatusForInterface, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				var customsStatusStore = DefaultCustomsStatusStore.CreateCustomsStatusStore(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Factory);

				var actualCodeDescriptionPairList = customsStatusStore.GetAllRefCusCodeList(true);
				AssertCodeDescriptionPairList(actualCodeDescriptionPairList, expectedCodes);

				actualCodeDescriptionPairList = customsStatusStore.GetAllRefCusCodeList(false);
				AssertCodeDescriptionPairList(actualCodeDescriptionPairList, ("UUU", "CSTA - Description Test"));
			}
		}
	}
}
