using System;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using WTG.RTUS.Interface.TestFramework;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Packing.DataTransfer.Universal.Testing
{
	public abstract class PackageParentExporterTestCase<T> : PackingTestCaseWithFactory
		where T : IPackingParent
	{
		#region TestGetDataObjectNotNull

		public void TestGetDataObjectNotNull()
		{
			var packingParent = GetPackingParent();
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(packingParent);
			var package = packageJob.Packages.AddNew();
			AssertNotNull(GetPackageWriter(package).GetDataObject(package));
		}

		#endregion

		#region TestIntegrationWithParentWriter

		public void TestIntegrationWithParentWriter()
		{
			var packingParent = GetPackingParent();
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(packingParent);
			var package = packageJob.Packages.AddNew();
			var writeManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, package));
			AssertNotNull(new PkgPackageUniversalShipmentDataObjectWriter(writeManager, DummyRequest.New()).GetDataObject(package));
		}

		#endregion

		#region TestWriterTypeCorrect

		public void TestWriterTypeCorrect()
		{
			AssertEquals(ParentWriterType, GetPackageWriter(Factory.New<PkgPackage>()).GetType());
		}

		#endregion

		protected abstract Type ParentWriterType { get; }

		#region Implementation

		protected abstract T GetPackingParent();

		DataObjectWriter<PkgPackage, UniversalShipment> GetPackageWriter(PkgPackage package)
		{
			var writeManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, package));
			return PackageParentJobMapper.GetWriter(ParentJobType, writeManager);
		}

		protected abstract ParentJobType ParentJobType { get; }

		#endregion
	}
}
