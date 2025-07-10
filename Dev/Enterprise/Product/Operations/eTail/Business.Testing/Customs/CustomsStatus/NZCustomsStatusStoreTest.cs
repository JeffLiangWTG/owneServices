using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.NZ;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Customs.NZ;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.eTail.Business.Testing
{
	sealed class NZCustomsStatusStoreTest : TestCaseWithFactory
	{
		public void TestTryGetReleaseStatusAndCustomsStatusDescription()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.NewZealand))
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

				var importDeclaration = (IJobDeclaration)Factory.New(ObjectFactory.GetType<IJobDeclaration>());
				importDeclaration.IsCancelled = false;
				consignment.HVC_JE_ImportDeclaration = importDeclaration.PK;
				consignment.HVC_ImportCustomsClearanceStatus = "MAN";
				CombineAssertions(() =>
				{
					AssertEquals("HVC_ReleaseStatus should be none", HVLVReleaseStatus.None, consignment.HVC_ReleaseStatus);
					AssertEquals("HVC_ImportReleaseStatus should be none", HVLVReleaseStatus.None, consignment.HVC_ImportReleaseStatus);
					AssertEquals("ReleaseStatusDescription should be none", HVLVReleaseStatus.NoneDescription, consignment.ReleaseStatusDescription);
					AssertEquals("CustomsStatusDescription", "Manual Entry - Cannot be Sent to Customs (Formal Only)", consignment.ImportCustomsClearanceStatusDescription);
				});

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

		public void TestNZCodeDescriptionPairList()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.NewZealand))
			{
				HVLVCustomsStatusTestHelper.AddRefCusCodeList(Factory, "UUU", "CSTA - Description Test", HVLVReleaseStatus.Held, RefCusCodeListTypes.Codes.CustomsStatus, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				HVLVCustomsStatusTestHelper.AddRefCusCodeList(Factory, "KKK", "CSTI - Description Test", HVLVReleaseStatus.Cleared, RefCusCodeListTypes.Codes.CustomsStatusForInterface, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

				var expectedList = NZCustomsStatusStore.FormalEntryStatusList.Select(cstiCustomsStatusInfo =>
					(cstiCustomsStatusInfo.Code, cstiCustomsStatusInfo.Description)).ToList();
				expectedList.Add(("UUU", "CSTA - Description Test"));

				var nzCustomsStatusStore = new NZCustomsStatusStore(Factory);
				var actualList = nzCustomsStatusStore.GetAllRefCusCodeList(true);
				AssertCodeDescriptionPairList(actualList, expectedList.ToArray());
			}
		}

		public void TestOverridenMembers()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.NewZealand))
			{
				var customsStatusStore = new NZCustomsStatusStore(Factory);
				var customsStatusStoreType = typeof(NZCustomsStatusStore);

				var countryCodeProperty = customsStatusStoreType.GetProperty("CountryCode", BindingFlags.NonPublic | BindingFlags.Instance);
				var countryCode = (ZString?)countryCodeProperty?.GetValue(customsStatusStore);
				AssertNotNull(countryCode);
				AssertEquals(CountryCodes.NewZealand, countryCode);

				var getCustomsStatusListForCodeTypeMethod =
					customsStatusStoreType.GetMethod("GetCustomsStatusListForCodeType",
						BindingFlags.Instance | BindingFlags.NonPublic);
				AssertNotNull(getCustomsStatusListForCodeTypeMethod);
				var customsStatusList = getCustomsStatusListForCodeTypeMethod?.Invoke(customsStatusStore,
					new object[] { RefCusCodeListTypes.Codes.CustomsStatusForInterface }) as IEnumerable<CustomsStatusInfo>;
				AssertNotNull(customsStatusList);

				var expectedList = NZCustomsStatusStore.FormalEntryStatusList.ToList();
				AssertContainsExactElementsInAnyOrder(expectedList, customsStatusList);
			}
		}

		public void TestWhetherConsignmentHasFormalDeclarationForNZ()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.NewZealand))
			{
				HVLVCustomsStatusTestHelper.AddRefCusCodeList(Factory, "UUU", "CSTA - Description Test", HVLVReleaseStatus.Held, RefCusCodeListTypes.Codes.CustomsStatus, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				HVLVCustomsStatusTestHelper.AddRefCusCodeList(Factory, "MAN", "CSTA - MAN - Description Test", HVLVReleaseStatus.Cleared, RefCusCodeListTypes.Codes.CustomsStatus, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

				var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
				var consignment = bookingHeader.Consignments.AddNew();
				consignment.HVC_ImportCustomsClearanceStatus = "UUU";
				CombineAssertions("Should take CSTA UUU code and its release status(Held)", () =>
				{
					AssertEquals("HVC_ReleaseStatus should be Held", HVLVReleaseStatus.Held, consignment.HVC_ReleaseStatus);
					AssertEquals("HVC_ImportReleaseStatus should be Held", HVLVReleaseStatus.Held, consignment.HVC_ImportReleaseStatus);
					AssertEquals("ReleaseStatusDescription should be Held", HVLVReleaseStatus.HeldDescription, consignment.ReleaseStatusDescription);
					AssertEquals("CustomsStatusDescription", "CSTA - Description Test", consignment.ImportCustomsClearanceStatusDescription);
				});

				var jobDeclaration = (IJobDeclaration)Factory.New(ObjectFactory.GetType<IJobDeclaration>());
				consignment.HVC_JE_ImportDeclaration = jobDeclaration.PK;
				jobDeclaration.JE_MessageSubType = "ECI";
				consignment.HVC_ImportCustomsClearanceStatus = "MAN";
				CombineAssertions("Should take CSTA MAN code and its release status(Cleared)", () =>
				{
					AssertEquals("HVC_ReleaseStatus should be Held", HVLVReleaseStatus.Cleared, consignment.HVC_ReleaseStatus);
					AssertEquals("HVC_ImportReleaseStatus should be Held", HVLVReleaseStatus.Cleared, consignment.HVC_ImportReleaseStatus);
					AssertEquals("ReleaseStatusDescription should be Held", HVLVReleaseStatus.ClearedDescription, consignment.ReleaseStatusDescription);
					AssertEquals("CustomsStatusDescription", "CSTA - MAN - Description Test", consignment.ImportCustomsClearanceStatusDescription);
				});

				consignment.HVC_ImportCustomsClearanceStatus = "UUU";
				CombineAssertions("Should take CSTA UUU code and its release status(Held)", () =>
				{
					AssertEquals("HVC_ReleaseStatus should be Held", HVLVReleaseStatus.Held, consignment.HVC_ReleaseStatus);
					AssertEquals("HVC_ImportReleaseStatus should be Held", HVLVReleaseStatus.Held, consignment.HVC_ImportReleaseStatus);
					AssertEquals("ReleaseStatusDescription should be Held", HVLVReleaseStatus.HeldDescription, consignment.ReleaseStatusDescription);
					AssertEquals("CustomsStatusDescription", "CSTA - Description Test", consignment.ImportCustomsClearanceStatusDescription);
				});

				jobDeclaration.JE_MessageSubType = "NON";
				consignment.HVC_ImportCustomsClearanceStatus = "MAN";
				CombineAssertions("Should take CSTI MAN code and its release status(None)", () =>
				{
					AssertEquals("HVC_ReleaseStatus should be Held", HVLVReleaseStatus.None, consignment.HVC_ReleaseStatus);
					AssertEquals("HVC_ImportReleaseStatus should be Held", HVLVReleaseStatus.None, consignment.HVC_ImportReleaseStatus);
					AssertEquals("ReleaseStatusDescription should be Held", HVLVReleaseStatus.NoneDescription, consignment.ReleaseStatusDescription);
					AssertEquals("CustomsStatusDescription", AmalgamatedStatusList.Descriptions.ManualEntryCannotBeSentToCustomsFormalOnly, consignment.ImportCustomsClearanceStatusDescription);
				});
			}
		}
	}
}
