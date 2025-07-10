using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Packing.Business.Testing
{
	[TestedType(typeof(PkgPackageHold))]
	class PkgPackageHoldTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			var package = packageJob.Packages.AddNew();
			var holdReason = Factory.New<PkgPackageHold>();
			holdReason.KHR_KP_Package = package.PK;
			holdReason.KHR_WHC_NKHoldCode = "HEL";
			holdReason.KHR_AddedTime = DateTime.Now;
			holdReason.KHR_GS_NKAddedBy = "STF";
			return holdReason;
		}
	}
}
