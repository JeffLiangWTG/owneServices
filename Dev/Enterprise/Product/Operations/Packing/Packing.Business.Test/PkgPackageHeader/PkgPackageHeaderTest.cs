using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Packing.Business.Testing
{
	[TestedType(typeof(PkgPackageHeader))]
	class PkgPackageHeaderTest : PackingBusinessObjectTestCase
	{
		#region TestIDocumentSupportable

		public void TestIDocumentSupportable()
		{
			var packageHeader = Factory.New<PkgPackageHeader>();
			var documentSupportable = (IDocumentSupportable)packageHeader;
			AssertEquals(packageHeader, documentSupportable.DocumentSupporter.BusinessObject);
			AssertEquals(typeof(PkgPackageHeaderDocumentSupporter), documentSupportable.DocumentSupporter.GetType());
		}

		#endregion

		#region TestISupportPackageIDGeneration

		public void TestISupportPackageIDGeneration()
		{
			Data.CreatePackingData();
			var packageID = Data.PackageJob.LoosePackageIDs.AddNew();
			packageID.KPH_PackageID = "ABC";
			packageID.CurrentPackageJob = Data.PackageJob;
			var iPackage = (ISupportPackageIDGeneration)packageID;
			AssertEquals(false, iPackage.IsContainer);
			AssertEquals(false, iPackage.IsBookedViaCarrier);
			AssertEquals("ABC", iPackage.KP_PackageID);
			AssertEquals(1, iPackage.KP_PackageQty);
			AssertEquals(packageID.CurrentPackageJob, iPackage.PackageJob);
			AssertEquals(false, iPackage.Packages.Any());
			AssertEquals(packageID.Factory, iPackage.Factory);
			AssertEquals(false, iPackage.ShouldGenerateIDOnSaving);
			AssertEquals(Data.PackageJob.PK, iPackage.PackageJobPK);

			iPackage.KP_PackageID = "DEF";
			AssertEquals("DEF", packageID.KPH_PackageID);
			iPackage.ShouldGenerateIDOnSaving = true;
			AssertEquals(true, iPackage.ShouldGenerateIDOnSaving);
		}

		public void TestCallAfterIDGenerated()
		{
			Data.CreatePackingData();

			ISupportPackageIDGenerationInternals package = Data.PackageJob.LoosePackageIDs.AddNew();
			AssertNoExceptionThrown(package.CallAfterIDGenerated);

			var count = 0;
			EventHandler afterIdEvent = (s, e) => count++;
			package.AfterIDGenerated += afterIdEvent;
			AssertEquals("Precondition.", 0, count);

			package.CallAfterIDGenerated();
			AssertEquals("Should have fired AfterIDGenerated.", 1, count);

			package.CallAfterIDGenerated();
			AssertEquals("Should have fired AfterIDGenerated.", 2, count);

			package.AfterIDGenerated -= afterIdEvent;
			package.CallAfterIDGenerated();
			AssertEquals("Should *not* have fired AfterIDGenerated.", 2, count);
		}

		#endregion

		#region TestCurrentPackageJob

		public void TestCurrentPackageJob()
		{
			var packageHeader = Factory.New<PkgPackageHeader>();
			AssertNull(packageHeader.CurrentPackageJob);
			AssertEquals("CurrentPackageJob should always be set if it needs to be used. ie PackageHeader > Document requires CurrentPackageJob. When delivering Documents, CurrentPackageJob should be set via the UI.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			var packageJob = Factory.New<PkgPackageJob>();
			packageHeader.CurrentPackageJob = packageJob;
			AssertEquals(packageJob, packageHeader.CurrentPackageJob);
		}

		#endregion

		#region TestDelete

		public void TestDelete()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			var packageHeader = packageJob.LoosePackageIDs.AddNew();

			var query = new ZQuery();
			query.AddToFilter(PkgPackageJobPackageHeaderPivotSchema.KPJ_KJ_PackageJob, packageJob.PK);
			var pivots = Factory.Load<PkgPackageJobPackageHeaderPivot>(query);
			AssertEquals("Pre-condition: should have 1 pivot.", 1, pivots.Length);

			var pivot = pivots[0];
			AssertEquals(false, pivot.IsDeleted);

			packageHeader.Delete();
			AssertEquals(true, pivot.IsDeleted);
		}

		#endregion
	}
}
