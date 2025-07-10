using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.Packing.Business;
using Enterprise.UniversalDataBuss.Integration;
using Moq;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsCarrierLabelsBatchPrintingInfoTest : WhsTestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new WhsCarrierLabelsBatchPrintingInfo(
				null,
				new[] { new KeyValuePair<ZGuid, DocumentCommandCollection>(ZGuid.Empty, new DocumentCommandCollection(Factory.New<PkgPackage>())) },
				new[] { new PackageToPackingParentInfo(Factory.New<WhsOrder>(), Factory.New<PkgPackage>(), "ABC") }));

			AssertExceptionThrown<ArgumentNullException>(() => new WhsCarrierLabelsBatchPrintingInfo(
				new[] { new Mock<ITopLevelDataObject>().Object },
				null,
				new[] { new PackageToPackingParentInfo(Factory.New<WhsOrder>(), Factory.New<PkgPackage>(), "ABC") }));

			AssertExceptionThrown<ArgumentNullException>(() => new WhsCarrierLabelsBatchPrintingInfo(
				new[] { new Mock<ITopLevelDataObject>().Object },
				new[] { new KeyValuePair<ZGuid, DocumentCommandCollection>(ZGuid.Empty, new DocumentCommandCollection(Factory.New<PkgPackage>())) },
				null));
		}
	}
}
