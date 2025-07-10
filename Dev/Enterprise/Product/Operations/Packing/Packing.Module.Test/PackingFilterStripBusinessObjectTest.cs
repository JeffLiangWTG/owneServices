using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Packing.Module.Testing
{
	[TestedType(typeof(PackingFilterStripBusinessObject))]
	internal class PackingFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Test Filter Max Length

		public void TestFilterMaxLength()
		{
			CombineAssertions(() =>
			{
				AssertEquals("MaxLength of Package ID/Container # should be set correctly.", PkgPackageHeaderSchema.KPH_PackageID.MaxLength, PackageIDFilter.MaxLength);
				AssertEquals("MaxLength of Packing Job ID should be set correctly.", ModuleNumberFilter.MultiplyMaxLength(PkgPackageJobSchema.KJ_JobID.MaxLength), PackingJobIDFilter.MaxLength);
				AssertEquals("MaxLength of Parent Job Ref # should be set correctly.", ModuleNumberFilter.MultiplyMaxLength(ViewPkgPackageJobParentsSchema.VI_JobNumber.MaxLength), ParentJobNoFilter.MaxLength);
			});
		}

		#endregion

		#region TestPackageID

		public void TestPackageID()
		{
			var data = new TestDataForPackingFilters(Factory);
			data.CreateTestData();

			var packageJobWithABC = data.PackageJobOnOrder1;
			var packageJobWithABCD = data.PackageJobOnTB;
			packageJobWithABC.Packages.AddNew().KP_PackageID = "abc";
			packageJobWithABCD.Packages.AddNew().KP_PackageID = "abcd";

			Factory.Save();

			PackageIDFilter.Property = "7";
			data.Asserter.AssertMatches("Should not find any PackingJobs.", PackageIDFilter);

			PackageIDFilter.Property = "ab";
			data.Asserter.AssertMatches("Should only find PackingJobs with Package with ID like ab%.", PackageIDFilter, packageJobWithABC, packageJobWithABCD);

			PackageIDFilter.Property = "abcd";
			data.Asserter.AssertMatches("Should only find the PackingJob with a Package with ID abcd", PackageIDFilter, packageJobWithABCD);
		}

		#endregion

		#region TestPackingJobID

		public void TestPackingJobID()
		{
			var data = new TestDataForPackingFilters(Factory);

			var packageJobMatchingFilter = Factory.New<PkgPackageJob>();
			var packageJobNotMatchingFilter = Factory.New<PkgPackageJob>();
			packageJobMatchingFilter.KJ_JobID = "P00000007";

			data.Asserter.AddToScope(packageJobMatchingFilter);
			data.Asserter.AddToScope(packageJobNotMatchingFilter);

			PackingJobIDFilter.Property = "123";
			data.Asserter.AssertMatches("Should not find any PackingJobs.", PackingJobIDFilter);

			PackingJobIDFilter.Property = "7";
			data.Asserter.AssertMatches("Should only find PackingJob with ID P00000007.", PackingJobIDFilter, packageJobMatchingFilter);
		}

		#endregion

		#region TestParentJobNo

		public void TestParentJobNo()
		{
			var data = new TestDataForPackingFilters(Factory);
			data.CreateTestData();

			ParentJobNoFilter.Property = "order1";
			data.Asserter.AssertMatches("Should only find the PackageJob attached to Order1.", ParentJobNoFilter, data.PackageJobOnOrder1);

			ParentJobNoFilter.Property = "Pick2";
			data.Asserter.AssertMatches("Should only find the PackageJob attached to Order2 (via Pick filter).", ParentJobNoFilter, data.PackageJobOnOrder2);

			ParentJobNoFilter.Property = "S00000001";
			data.Asserter.AssertMatches("Should only find the PackageJob attached to the Transport Booking with Shipment.", ParentJobNoFilter, data.PackageJobOnTBWithShipment);
		}

		#endregion

		#region TestParentJobStatus

		public void TestParentJobStatus()
		{
			var data = new TestDataForPackingFilters(Factory);
			data.CreateTestData();

			AssertEquals("Should Default to OPEN jobs.", FilterVisibility.AlwaysApplied, ParentJobStatusFilter.Visibility);
			AssertEquals("Should Default to OPEN jobs.", "OPN", ParentJobStatusFilter.DefaultProperty);

			// complete 2 of the 5 test jobs

			data.PackageJobOnOrder1.Packages.AddNew(); // prevent deletion of an empty PackageJob on FIN.
			data.Pick1[WhsPickSchema.WP_PickStatus] = "FIN";
			data.Pick1[WhsPickSchema.WP_FinalizedDateUtc] = ZDateTime.UtcNow;
			data.TB[DtbBookingConsolidationSchema.KB_Status] = "DLV";
			Factory.Save();

			ParentJobStatusFilter.Property = "OPN";
			data.Asserter.AssertMatches("Should only find the PackageJob attached to the open Order, TB-Shipment, TB-BOL, and TB-Receive.", ParentJobStatusFilter,
				data.PackageJobOnOrder2, data.PackageJobOnTBWithShipment, data.PackageJobOnTBWithBOL, data.PackageJobOnTBWithReceive);

			ParentJobStatusFilter.Property = "CLS";
			data.Asserter.AssertMatches("Should only find the PackageJob attached to the completed Order and TB.", ParentJobStatusFilter, data.PackageJobOnOrder1, data.PackageJobOnTB);

			ParentJobStatusFilter.Property = "ALL";
			data.Asserter.AssertMatches("Should find all PackageJobs.", ParentJobStatusFilter,
				data.PackageJobOnOrder1, data.PackageJobOnOrder2, data.PackageJobOnTB, data.PackageJobOnTBWithShipment, data.PackageJobOnTBWithBOL, data.PackageJobOnTBWithReceive, data.PackageJobOnTBDodgey);
		}

		#endregion

		#region TestParentJobType

		public void TestParentJobType()
		{
			var data = new TestDataForPackingFilters(Factory);
			data.CreateTestData();

			ParentJobTypeFilter.Property = "ORD";
			data.Asserter.AssertMatches("Should only find the PackageJob attached to the Orders.", ParentJobTypeFilter, data.PackageJobOnOrder1, data.PackageJobOnOrder2);

			ParentJobTypeFilter.Property = "BKG";
			data.Asserter.AssertMatches("Should only find the PackageJob attached to the stand-alone Transport Booking.", ParentJobTypeFilter, data.PackageJobOnTB);

			ParentJobTypeFilter.Property = "WHR";
			data.Asserter.AssertMatches("Should only find the PackageJob attached to the Transport Booking with Receive.", ParentJobTypeFilter, data.PackageJobOnTBWithReceive);

			ParentJobTypeFilter.Property = "SHP";
			data.Asserter.AssertMatches("Should only find the PackageJob attached to the Transport Booking with Shipment.", ParentJobTypeFilter, data.PackageJobOnTBWithShipment);

			ParentJobTypeFilter.Property = "ASH";
			data.Asserter.AssertMatches("Should only find the PackageJob attached to the Transport Booking with Bill of Lading.", ParentJobTypeFilter, data.PackageJobOnTBWithBOL);
		}

		#endregion

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new PackingFilterStripBusinessObject();
		}

		ModuleTextFilter PackageIDFilter
		{
			get { return (ModuleTextFilter)FilterStripBizO["Package ID/Container #"]; }
		}

		ModuleFountainFilter PackingJobIDFilter
		{
			get { return (ModuleFountainFilter)FilterStripBizO["Packing Job ID"]; }
		}

		ModuleTextFilter ParentJobNoFilter
		{
			get { return (ModuleTextFilter)FilterStripBizO["Parent Job Ref #"]; }
		}

		ModuleTextFilter ParentJobStatusFilter
		{
			get { return (ModuleTextFilter)FilterStripBizO["Parent Job Status"]; }
		}

		ModuleTextFilter ParentJobTypeFilter
		{
			get { return (ModuleTextFilter)FilterStripBizO["Parent Job Type"]; }
		}

		PackingFilterStripBusinessObject FilterStripBizO
		{
			get { return filterStripBizO ?? (filterStripBizO = (PackingFilterStripBusinessObject)GetNewFilterStripBusinessObject()); }
		}

		PackingFilterStripBusinessObject filterStripBizO;

		#endregion
	}
}
