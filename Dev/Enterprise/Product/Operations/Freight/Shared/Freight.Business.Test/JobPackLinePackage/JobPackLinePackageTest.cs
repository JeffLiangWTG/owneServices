using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Freight;
using Enterprise.Packing.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(JobPackLinePackage))]
	sealed class JobPackLinePackageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestJobPackLinePackageCanBeLoadedUsingIJobPackLinePackageAndJPP_KP_Packge()
		{
			var packageParent = Factory.New<DummyBusinessObject>();
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = packageParent.PK;
			packageJob.KJ_ParentTableCode = packageParent.TablePrefix;

			var pkg = Factory.New<PkgPackage>();
			pkg.KP_KJ_ParentPackageJob = packageJob.PK;
			pkg.KP_Volume = 10m;
			pkg.KP_Sequence = 1;
			pkg.KP_F3_NKPackType = "PKG";
			var packageId = pkg.PK;

			var jobShipment = (IForwardingShipment)Factory.New(ObjectFactory.GetType(typeof(IForwardingShipment)));
			var packLine = Factory.New<PackLine>();
			packLine.JL_JS = jobShipment.PK;
			var packLineId = packLine.PK;

			var jobPackLinePackage = Factory.New<JobPackLinePackage>();
			jobPackLinePackage.JPP_KP_Packge = packageId;
			jobPackLinePackage.JPP_JL_PackLine = packLineId;
			Factory.Save();

			var anotherJobPackLinePackages = (IJobPackLinePackage)Factory.LoadTop1(ObjectFactory.GetType(typeof(IJobPackLinePackage)), new ZQuery(JobPackLinePackageSchema.JPP_KP_Packge, packageId));
			AssertEquals("It's the same jobPackLinePackage", jobPackLinePackage.PK, anotherJobPackLinePackages.PK);
			AssertEquals("JPP_JL_PackLine is the same", jobPackLinePackage.JPP_JL_PackLine, anotherJobPackLinePackages.JPP_JL_PackLine);
		}

		public void TestJobPackLinePackageCannotBeSavedWithDuplicatePackage()
		{
			var shipment = (IForwardingShipment)Factory.New(ObjectFactory.GetType(typeof(IForwardingShipment)));
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = shipment.PK;
			packageJob.KJ_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			var packLine1 = Factory.New<PackLine>();
			packLine1.JL_PackLineId = "PL0001";
			packLine1.JL_JS = shipment.PK;

			var packLine2 = Factory.New<PackLine>();
			packLine2.JL_PackLineId = "PL0002";
			packLine2.JL_JS = shipment.PK;

			var pkg = Factory.New<PkgPackage>();
			pkg.KP_KJ_ParentPackageJob = packageJob.PK;
			pkg.KP_Volume = 10m;
			pkg.KP_Sequence = 1;
			pkg.KP_F3_NKPackType = "PKG";
			pkg.KP_PackageID = "PH0123456";
			pkg.KP_PreviousPackLineID = "PL0003";
			pkg.KP_ExternalReference = "TUAT999";

			var jobPackLinePackage2 = Factory.New<JobPackLinePackage>();
			jobPackLinePackage2.JPP_KP_Packge = pkg.PK;
			jobPackLinePackage2.JPP_JL_PackLine = packLine2.PK;

			Factory.Save();

			var jobPackLinePackage1 = Factory.New<JobPackLinePackage>();
			jobPackLinePackage1.JPP_KP_Packge = pkg.PK;
			jobPackLinePackage1.JPP_JL_PackLine = packLine1.PK;

			var exception = AssertExceptionThrown<ZCannotSaveException>("Should fail to save when there is a duplicate package", jobPackLinePackage1.OnSaving);
			var expectedMessage = $@"A package can only be associated with one packline.
The following package:
Package ID: PH0123456
Package PK: {pkg.PK}
Packline ID: PL0001
External Reference: TUAT999
Previous Packline ID: PL0003
is already associated with another packline:
Packline ID: PL0002,
Packline PK: {packLine2.PK}";
			AssertEquals(expectedMessage, exception.Message);
		}
	}
}
