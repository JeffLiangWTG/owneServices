using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Packing.DataTransfer.Universal.Testing
{
	public class PkgPackageJobDataObjectReaderMatchAndRelabelOnPreviousPackageIDTest : MatchAndRelabelOnPreviousPackageIDTest<PkgPackageJob>
	{
		TestDataForPacking Data => data ?? (data = new TestDataForPacking(Factory.BOFactory));

		TestDataForPacking data;

		protected override PkgPackageJob PackageJob => Data.PackageJob;

		protected override void SetUp()
		{
			base.SetUp();
			Data.CreatePackingData();
		}

		protected override DataObjectReader<IDataObject, PkgPackageJob> GetReader(DataObjectList<PackingLine> packingLineCollection)
		{
			var packageParentData = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			packageParentData.SetPackingLineCollection(() => packingLineCollection);

			return new PkgPackageJobDataObjectReader(packageParentData, Logger, Factory, Data.Dummy, ImportOption.MatchAndRelabelOnPreviousPackageID);
		}
	}
}
