using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.TransportConsignment.Business.Testing;
using Enterprise.TransportConsignment.DataTransfer.Universal.Common;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportConsignment.DataTransfer.Universal.Testing
{
	class DeleteDetachedPackageHelperTest : OrganizationAddressTestHelper
	{
		#region TestDetachedPackagesDeleted
		public void TestDetachedPackagesDeleted()
		{
			try
			{
				var helper = new TransportConsignmentTestHelper(Factory.BOFactory);
				DummyWithPacking.TypeDecider.TypeForLoadOverride = typeof(DummyWithPacking);
				var dummyPackingParent = Factory.BOFactory.New<DummyWithPacking>();
				dummyPackingParent.JobNoForPackingParent = "abc";
				var packageJob = PkgPackageJob.LoadOrCreatePackageJob(dummyPackingParent);
				var outerPallet = helper.CreatePackage(packageJob, "PLT", "PLT-1");
				var innerCarton1 = outerPallet.Packages.AddNew("CTN", "CTN-1");
				var innerCarton2 = outerPallet.Packages.AddNew("CTN", "CTN-2");
				var innerBox = outerPallet.Packages.AddNew("BOX", "BOX-1");
				var outerPalletID = Factory.Load<PkgPackageHeader>(outerPallet.KP_KPH_PackageHeader);
				var innerBoxID = Factory.Load<PkgPackageHeader>(innerBox.KP_KPH_PackageHeader);
				Factory.SaveForTesting();
				var newFactory = new UniversalObjectFactory();
				var outerPalletRow = DataObjectReader.GetColumnIndexerFromRow(newFactory.RowFactory.Load(PkgPackageSchema.Constants.TableName, new ZQuery(PkgPackageSchema.PK, outerPallet.PK)).Single());
				var innerCarton1Row = DataObjectReader.GetColumnIndexerFromRow(newFactory.RowFactory.Load(PkgPackageSchema.Constants.TableName, new ZQuery(PkgPackageSchema.PK, innerCarton1.PK)).Single());
				var innerCarton2Row = DataObjectReader.GetColumnIndexerFromRow(newFactory.RowFactory.Load(PkgPackageSchema.Constants.TableName, new ZQuery(PkgPackageSchema.PK, innerCarton2.PK)).Single());
				var innerBoxRow = DataObjectReader.GetColumnIndexerFromRow(newFactory.RowFactory.Load(PkgPackageSchema.Constants.TableName, new ZQuery(PkgPackageSchema.PK, innerBox.PK)).Single());
				foreach (var package in new[] { outerPalletRow, innerBoxRow })
				{
					package.SetValue(PkgPackageSchema.KP_KJ_ParentPackageJob, ZGuid.Empty);
				}

				innerCarton1Row.SetValue(PkgPackageSchema.KP_KP_ParentPackage, ZGuid.Empty);
				innerCarton1Row.SetValue(PkgPackageSchema.KP_Sequence, (ZInt)1);
				innerCarton2Row.SetValue(PkgPackageSchema.KP_KP_ParentPackage, ZGuid.Empty);
				innerCarton2Row.SetValue(PkgPackageSchema.KP_Sequence, (ZInt)2);
				DeleteDetachedPackageHelper.DeleteDetachedPackages(newFactory);
				newFactory.SaveForTesting();
				var bizOFactory = new BusinessObjectFactory();
				AssertNotNull("innerCarton1 should not be deleted.", bizOFactory.Load<PkgPackage>(innerCarton1.PK));
				AssertNotNull("innerCarton2 should not be deleted.", bizOFactory.Load<PkgPackage>(innerCarton2.PK));
				AssertNull("Original Outer Pallet should be deleted.", bizOFactory.Load<PkgPackage>(outerPallet.PK));
				AssertNull("Original Outer PalletID should be deleted.", bizOFactory.Load<PkgPackageHeader>(outerPalletID.PK));
				AssertNull("innerBox should be deleted.", bizOFactory.Load<PkgPackage>(innerBox.PK));
				AssertNull("innerBoxID should be deleted.", bizOFactory.Load<PkgPackageHeader>(innerBoxID.PK));
			}
			finally
			{
				DummyWithPacking.TypeDecider.TypeForLoadOverride = null;
			}
		}

		#endregion
	}
}
