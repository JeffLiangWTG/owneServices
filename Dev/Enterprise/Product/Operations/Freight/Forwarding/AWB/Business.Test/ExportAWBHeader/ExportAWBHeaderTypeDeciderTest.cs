using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.AWB.Business.Testing
{
	sealed class ExportAWBHeaderTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			var typeDecider = new ExportAWBHeaderTypeDecider();

			var header = Factory.New<ExportAWBHeader>();
			var row = ((INeedRow)header).Row;

			AssertEquals("EH_Table == ''", typeof(ExportAWBHeader), typeDecider.GetTypeForLoad(row, Factory));

			header.EH_Table = JobShipmentSchema.Constants.TableName;
			AssertEquals("EH_Table == 'JobShipment'", "ShipmentExportAWBHeader", typeDecider.GetTypeForLoad(row, Factory).Name);

			header.EH_Table = JobConsolSchema.Constants.TableName;
			AssertEquals("EH_Table == 'JobConsol'", "ConsolExportAWBHeader", typeDecider.GetTypeForLoad(row, Factory).Name);

			header.EH_Table = DummyBizoSchema.Constants.TableName;
			AssertEquals("EH_Table == 'DummyBizo'", "MockExportAWBHeader", typeDecider.GetTypeForLoad(row, Factory).Name);

			header.EH_Table = ExportAWBHeaderSchema.Constants.TableName;
			AssertEquals("EH_Table == 'ExportAWBHeader'", typeof(ExportAWBHeader), typeDecider.GetTypeForLoad(row, Factory));
		}
	}
}
