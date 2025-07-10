using System.IO;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Environment.DataTransfer;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Environment.GUI.Testing
{
	public class SysMergeWarehouseXmlDataTransferExporterTest : WhsTestCaseWithFactoryEnv
	{
		#region TestAdapter

		public void TestAdapter()
		{
			var exporter = new SysMergeWarehouseXmlDataTransferExporterForTest();
			AssertEquals("Adapter", typeof(SysMergeWarehouseValueObjectDataAdapter), exporter.Adapter_Exposed.GetType());
		}

		#endregion

		#region TestExportToFile

		public void TestExportToFile()
		{
			var whs = Factory.NewWithValidTestData<WhsWarehouse>();
			Factory.Save();

			var director = new SysMergeWarehouseXmlDataTransferExporterForTest();
			director.DefaultFileName = "WhsXmlExportTestFile_CE4C69C4BDAC49089656406A7E596830";
			var expectedFileName = Path.Combine(EnvProxy.Instance.TempPath, director.DefaultFileName + ".xml");
			ZFormModaliser.FileNameToSelectInShowCommonDialog = expectedFileName;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			try
			{
				director.PromptUserAndExport(new BusinessObject[] { whs });
				Assert(File.Exists(expectedFileName));
			}
			finally
			{
				DeleteIfExists(expectedFileName);
			}
		}

		#endregion

		#region SysMergeWarehouseXmlDataTransferExporterForTest class

		class SysMergeWarehouseXmlDataTransferExporterForTest : SysMergeWarehouseXmlDataTransferExporter
		{
			public IValueObjectDataAdapter Adapter_Exposed
			{
				get { return Adapter; }
			}
		}

		#endregion
	}
}
