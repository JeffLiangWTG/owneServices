using System.Linq;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Packing.Business.Testing
{
	public class PkgPackageHeaderDocumentSupporterTest : PackingTestCaseWithFactory
	{
		public void TestDocumentSupporterBusinessContext()
		{
			var packageHeader = Factory.New<PkgPackageHeader>();
			packageHeader.KPH_PackageID = "ABC";
			AssertEquals("BusinessContext", BusinessContext.PackageHeader, ((IDocumentSupportable)packageHeader).DocumentSupporter.BusinessContext);
		}

		public void TestDocumentSupporterSupportedDataContext()
		{
			var packageHeader = Factory.New<PkgPackageHeader>();
			packageHeader.KPH_PackageID = "ABC";
			AssertEquals("Core.Constants.DataContext.GenericFreightJob is Supported", true, ((IDocumentSupportable)packageHeader).DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.GenericFreightJob)));
			AssertEquals("Core.Constants.DataContext.GenericBasicLabel is Supported", true, ((IDocumentSupportable)packageHeader).DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.GenericBasicLabel)));
		}

		#region TestBusinessContext

		public void TestBusinessContext()
		{
			AssertEquals(BusinessContext.PackageHeader, PackageHeaderDocumentSupporter.BusinessContext);
		}

		#endregion

		#region TestCustomisationSecurityCheckpoint

		public void TestCustomisationSecurityCheckpoint()
		{
			AssertEquals(Env.Security.None, PackageHeaderDocumentSupporter.CustomisationSecurityCheckpoint);
		}

		#endregion

		#region TestGetDocumentWrappers_GenericDeliveryID

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

		#endregion

		#region TestFixSequence

		public void TestFixSequence()
		{
			Data.CreatePackingData();
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

			var header1 = package1.GetPackageHeader();
			header1.CurrentPackageJob = Data.PackageJob;
			var packageHeaderDocumentSupporter = ((IDocumentSupportable)header1).DocumentSupporter;
			var wrappers = packageHeaderDocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericBasicLabel, testMenu);

			var fixedSequence = Data.PackageJob.Packages.Select(p => p.KP_Sequence);
			var expectedSequence = new ZShort[] { 1, 2, 3, 4 };
			AssertContainsExactElementsInAnyOrder(expectedSequence, fixedSequence);
		}

		#endregion

		#region PackageHeaderDocumentSupporter

		PkgPackageHeaderDocumentSupporter PackageHeaderDocumentSupporter
		{
			get
			{
				if (packageDocumentSupporter == null)
				{
					var packageHeader = Factory.New<PkgPackageHeader>();
					packageHeader.KPH_PackageID = "ABC";
					packageDocumentSupporter = (PkgPackageHeaderDocumentSupporter)((IDocumentSupportable)packageHeader).DocumentSupporter;
				}
				return packageDocumentSupporter;
			}
		}

		PkgPackageHeaderDocumentSupporter packageDocumentSupporter;

		#endregion
	}
}
