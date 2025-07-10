using System;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.EntityFramework;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.Environment;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgBrandOrRelatedName))]
	sealed class OrgBrandOrRelatedNameTest : EnterpriseBusinessObjectTestCase
	{
		public void TestNoAuditLog()
		{
			var relatedName = Factory.NewWithValidTestData<OrgBrandOrRelatedName>();
			relatedName.P1_RelatedName = "First";
			Factory.Save();

			AssertEquals("Logs count", 0, relatedName.Logs.GetAllLogs().Count);
		}

		public void TestDeduplicationStarted()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());

			var relatedName = Factory.NewWithValidTestData<OrgBrandOrRelatedName>();
			var isFindingDuplicates = false;
			var orgHeader = relatedName.Header;
			orgHeader.DeduplicationStarted += (o, e) => isFindingDuplicates = true;
			((IDeduplicatable)orgHeader).ShouldRunDeduplication = true;
			relatedName.P1_RelatedName = "First";
			AssertEquals(true, isFindingDuplicates);
			isFindingDuplicates = false;

			relatedName.P1_RelatedName = "Second";
			AssertEquals(true, isFindingDuplicates);
			isFindingDuplicates = false;

			relatedName.Delete();
			AssertEquals(true, isFindingDuplicates);
			isFindingDuplicates = false;

			var relatedName2 = Factory.NewWithValidTestData<OrgBrandOrRelatedName>();
			var orgHeader2 = relatedName2.Header;
			orgHeader.DeduplicationStarted += (o, e) => isFindingDuplicates = true;
			relatedName2.Delete();
			AssertEquals(false, isFindingDuplicates);
		}

		public void TestSettingPatternRequiresRegen()
		{
			OrgBrandOrRelatedName brand = Factory.NewWithValidTestData<OrgBrandOrRelatedName>();
			brand.Header.PatternMatchRequiresRegen = false;
			Assert(!brand.Header.PatternMatchRequiresRegen);
			brand.P1_RelatedName = "ABC";
			Assert(brand.Header.PatternMatchRequiresRegen);
		}

		#region TestScreeningStatuses

		public void TestScreeningStatuses_ChangeWhenRelatedNameChanges()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var brand = org.BrandsOrRelatedNames.AddNew();
			brand.P1_RelatedName = "A Test Name";

			Factory.Save();

			org.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			brand.P1_RelatedName = "A New Name";

			Factory.Save();

			AssertEquals(ScreeningStatusesList.Codes.Unknown, org.OH_ScreeningStatus);
		}

		public void TestScreeningStatuses_ChangeWhenBrandOrRelatedNameAdded()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			Factory.Save();

			org.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			var brand = org.BrandsOrRelatedNames.AddNew();
			brand.P1_RelatedName = "A Test Name";

			Factory.Save();

			AssertEquals(ScreeningStatusesList.Codes.Unknown, org.OH_ScreeningStatus);
		}

		public void TestScreeningStatuses_NotChange()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var brand = org.BrandsOrRelatedNames.AddNew();
			brand.P1_RelatedName = "A Test Name";

			Factory.Save();

			org.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;

			Factory.Save();

			AssertEquals(ScreeningStatusesList.Codes.Matched, org.OH_ScreeningStatus);

			brand.P1_OH = Guid.NewGuid();
			brand.P1_RelatedName = "A New Name";
			AssertExceptionThrown<ZSaveException>(() => Factory.Save());
			AssertEquals(ScreeningStatusesList.Codes.Matched, org.OH_ScreeningStatus);
		}

		public void TestInvalidateByLocalDataChanges_ShouldInvalidateScreeningStatus()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var brand = header.BrandsOrRelatedNames.AddNew();
			brand.P1_RelatedName = "ORG Related Brand";
			var logCount = 0;

			Factory.Save();

			CombineAssertions("Should invalidate screening status and create screening log", () =>
			{
				AssertHasInvalidatedScreeningStatuses(() =>
				{
					brand.P1_RelatedName = "New Brand";
					header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
				});
				AssertHasInvalidatedScreeningStatuses(() =>
				{
					brand.P1_RelatedName = "Updated Brand";
					header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
				});
			});

			void AssertHasInvalidatedScreeningStatuses(Action action)
			{
				logCount++;
				action.Invoke();

				Factory.Save();

				AssertEquals(ScreeningStatusesList.Codes.Unknown, header.OH_ScreeningStatus);
				AssertEquals(DeniedPartyConstants.LogsScreeningStatus.InvalidatedByLocalDataChanges, header.ScreeningLogCollection[logCount - 1].PJ_Status);
				AssertEquals(logCount, header.ScreeningLogCollection.Count);
			}
		}

		public void TestInvalidateByLocalDataChanges_ShouldNotInvalidateScreeningStatus()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var brand = header.BrandsOrRelatedNames.AddNew();
			brand.P1_RelatedName = "New Brand";

			Factory.Save();

			CombineAssertions("Should not invalidate screening status", () =>
			{
				AssertHasInvalidatedScreeningStatuses(brand.P1_RelatedNameInfo, ScreeningStatusesList.Codes.Unknown);
				AssertHasInvalidatedScreeningStatuses(brand.P1_RelatedNameInfo, ScreeningStatusesList.Codes.PermanentClear);
			});

			void AssertHasInvalidatedScreeningStatuses(ZPropertyInfo propertyInfo, string screeningStatus)
			{
				header.OH_ScreeningStatus = screeningStatus;
				propertyInfo.SetValueFromString("Updated Brand");

				Factory.Save();

				AssertEquals(screeningStatus, header.OH_ScreeningStatus);
				AssertEquals(0, header.ScreeningLogCollection.Count);
			}
		}

		public void TestScreeningStatus_WhenDelete()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var brand = org.BrandsOrRelatedNames.AddNew();
			brand.P1_RelatedName = "First";
			Factory.Save();

			org.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			Factory.Save();

			AssertEquals("PreCondition", ScreeningStatusesList.Codes.Matched, org.OH_ScreeningStatus);

			brand.Delete();
			Factory.Save();

			AssertEquals(ScreeningStatusesList.Codes.Unknown, org.OH_ScreeningStatus);
		}

		#endregion

		#region ReadOnly Security

		public void TestReadOnlySecurity()
		{
			bool oldBrandsValue = Env.Security.OrgConfigModifyBrandsAndCompanyNames.IsAllowed;

			try
			{
				OrgBrandOrRelatedName testBrand = OrgInDB.BrandsOrRelatedNames.AddNew();

				Env.Security.OrgConfigModifyBrandsAndCompanyNames.IsAllowed = true;
				Assert("Access Allowed - Not ReadOnly", !testBrand.P1_RelatedNameInfo.ReadOnly);

				Env.Security.OrgConfigModifyBrandsAndCompanyNames.IsAllowed = false;
				Assert("Access NOT Allowed - ReadOnly", testBrand.P1_RelatedNameInfo.ReadOnly);
			}
			finally
			{
				Env.Security.OrgConfigModifyBrandsAndCompanyNames.IsAllowed = oldBrandsValue;
			}
		}

		OrgHeader OrgInDB
		{
			get
			{
				if (fOrgInDB == null)
				{
					ResetOrgInDB();
				}

				return fOrgInDB;
			}
		}
		OrgHeader fOrgInDB;

		void ResetOrgInDB()
		{
			fOrgInDB = new BusinessObjectFactory().LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "DEMORG");
		}

		#endregion
	}
}
