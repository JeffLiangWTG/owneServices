using System;
using System.Collections.Generic;
using Enterprise.Packing.Business;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class CreatePickByBOMTransferLineResultTest : WhsTestCaseWithFactory
	{
		public void TestCreatePickByBOMTransferLineResult()
		{
			var kitTransferLines = new HashSet<WhsTransferLine>();
			var packages = new List<PkgPackage>();
			AssertExceptionThrown<ArgumentNullException>(() => new CreatePickByBOMTransferLineResult(null, packages));
			AssertExceptionThrown<ArgumentNullException>(() => new CreatePickByBOMTransferLineResult(kitTransferLines, null));

			var (transferLinesToIgnoreWhenSettingLocation, kitPackages) = new CreatePickByBOMTransferLineResult(kitTransferLines, packages);
			AssertEquals(kitTransferLines, transferLinesToIgnoreWhenSettingLocation);
			AssertEquals(packages, kitPackages);
		}
	}
}
