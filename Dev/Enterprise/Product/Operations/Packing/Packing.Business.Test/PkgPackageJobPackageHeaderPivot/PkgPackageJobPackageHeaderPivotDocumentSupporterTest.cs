using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Packing.Business.Testing
{
	public class PkgPackageJobPackageHeaderPivotDocumentSupporterTest : PackingTestCaseWithFactory
	{
		#region TestDocumentSupporterSupportedDataContext

		public void TestDocumentSupporterSupportedDataContext()
		{
			var packageHeaderPivot = Factory.New<PkgPackageJobPackageHeaderPivot>();
			AssertEquals("Core.Constants.DataContext.GenericFreightJob is Supported", true, ((IDocumentSupportable)packageHeaderPivot).DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.GenericFreightJob)));
			AssertEquals("Core.Constants.DataContext.GenericBasicLabel is Supported", true, ((IDocumentSupportable)packageHeaderPivot).DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.GenericBasicLabel)));
		}

		#endregion

		#region TestBusinessContext

		public void TestBusinessContext()
		{
			AssertEquals(BusinessContext.PackageHeader, PackageHeaderPivotDocumentSupporter.BusinessContext);
		}

		#endregion

		#region TestCustomisationSecurityCheckpoint

		public void TestCustomisationSecurityCheckpoint()
		{
			AssertEquals(Env.Security.None, PackageHeaderPivotDocumentSupporter.CustomisationSecurityCheckpoint);
		}

		#endregion

		#region TestGetDocumentWrappers

		public void TestGetDocumentWrappers_GenericDeliveryID()
		{
			Data.CreatePackingData();
			var packageHeader = Data.PackageJob.LoosePackageIDs.AddNew();
			packageHeader.CurrentPackageJob = Data.PackageJob;
			packageHeader.KPH_PackageID = "ABC";
			var testMenu = Factory.New<StmMenuItem>();
			Factory.Save();

			var packageHeaderDocumentSupporter = ((IDocumentSupportable)packageHeader).DocumentSupporter;
			var wrappers1 = packageHeaderDocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericFreightJob, testMenu);
			AssertEquals("Loose package will be selected", 1, wrappers1.Length);

			var wrappers2 = packageHeaderDocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericBasicLabel, testMenu);
			AssertEquals("Loose package will be selected", 1, wrappers2.Length);

			var wrappers3 = packageHeaderDocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericCarrierLabel, testMenu);
			AssertEquals("Nothing will be selected", 0, wrappers3.Length);
		}

		public void TestGetDocumentWrappers_NoException()
		{
			Data.CreatePackingData();
			var packageHeader = Data.PackageJob.LoosePackageIDs.AddNew();
			packageHeader.CurrentPackageJob = Data.PackageJob;
			packageHeader.KPH_PackageID = "ABC";
			var testMenu = Factory.New<StmMenuItem>();
			Factory.Save();

			var packageHeaderDocumentSupporter = ((IDocumentSupportable)packageHeader).DocumentSupporter;
			AssertNoExceptionThrown(() =>
			{
				var wrappers = packageHeaderDocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericFreightJob, testMenu);
				AssertEquals("Loose package will be selected", 1, wrappers.Length);
				var loosePackageWrapper = wrappers.First();
				AssertEquals(1, ((BusinessObjectCollection)loosePackageWrapper["Packages"]).Count);
			});
		}

		#endregion

		#region TestFixSequence

		public void TestFixSequence()
		{
			Data.CreatePackingData();

			var pivot = Data.PackageJob.LoosePackagePivots.AddNew();
			pivot.PackageHeader.KPH_PackageID = "ABC";

			var package1 = Data.PackageJob.Packages.AddNew();
			package1.KP_Sequence = 3;
			package1.KP_PackageID = "P1";
			var package2 = Data.PackageJob.Packages.AddNew();
			package2.KP_Sequence = 3;
			package2.KP_PackageID = "P2";
			var package3 = Data.PackageJob.Packages.AddNew();
			package3.KP_Sequence = 6;
			package3.KP_PackageID = "P3";
			var package4 = Data.PackageJob.Packages.AddNew();
			package4.KP_Sequence = 9;
			package4.KP_PackageID = "P4";
			var testMenu = Factory.New<StmMenuItem>();
			Factory.Save();

			var pivotDocumentSupporter = ((IDocumentSupportable)pivot).DocumentSupporter;
			var wrappers = pivotDocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericBasicLabel, testMenu);

			var fixedSequence = Data.PackageJob.Packages.Select(p => p.KP_Sequence);
			var expectedSequence = new ZShort[] { 1, 2, 3, 4 };
			AssertContainsExactElementsInAnyOrder(expectedSequence, fixedSequence);
		}

		#endregion

		#region PackageHeaderDocumentSupporter

		PkgPackageJobPackageHeaderPivotDocumentSupporter PackageHeaderPivotDocumentSupporter
		{
			get
			{
				if (packagePivotDocumentSupporter == null)
				{
					var packageHeaderPivot = Factory.New<PkgPackageJobPackageHeaderPivot>();
					packagePivotDocumentSupporter = (PkgPackageJobPackageHeaderPivotDocumentSupporter)((IDocumentSupportable)packageHeaderPivot).DocumentSupporter;
				}
				return packagePivotDocumentSupporter;
			}
		}

		PkgPackageJobPackageHeaderPivotDocumentSupporter packagePivotDocumentSupporter;

		#endregion
	}
}
