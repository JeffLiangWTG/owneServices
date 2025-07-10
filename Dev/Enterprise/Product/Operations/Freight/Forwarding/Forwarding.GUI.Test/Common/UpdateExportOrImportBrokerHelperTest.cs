using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class UpdateExportOrImportBrokerHelperTest : TestCaseWithFactory
	{
		public void TestUpdateImportBroker()
		{
			var eventArgs = new BrokerDefaultingEventArgs(DocAddressType.ImportBroker);
			AssertEquals(DocAddressType.ImportBroker, eventArgs.BrokerType);
			AssertEquals(false, eventArgs.ShouldUpdateBroker);

			UpdateExportOrImportBrokerHelper.UpdateExportOrImportBroker(new object(), eventArgs);

			var msg = UnitTestUserNotification.Instance.LastMessage;
			AssertEquals("Import Broker", msg.Caption);
			AssertEquals("Consignee has been changed. Do you wish to update the Import Broker?", msg.Text);
			AssertEquals(ZMessageBoxIcon.Question, msg.Context.Icon);
			AssertEquals(true, msg.Context.ShowCheckboxOnly);
		}

		public void TestUpdateExportBroker()
		{
			var eventArgs = new BrokerDefaultingEventArgs(DocAddressType.ExportBroker);
			AssertEquals(DocAddressType.ExportBroker, eventArgs.BrokerType);
			AssertEquals(false, eventArgs.ShouldUpdateBroker);

			UpdateExportOrImportBrokerHelper.UpdateExportOrImportBroker(new object(), eventArgs);

			var msg = UnitTestUserNotification.Instance.LastMessage;
			AssertEquals("Export Broker", msg.Caption);
			AssertEquals("Consignor has been changed. Do you wish to update the Export Broker?", msg.Text);
			AssertEquals(ZMessageBoxIcon.Question, msg.Context.Icon);
			AssertEquals(true, msg.Context.ShowCheckboxOnly);
		}
	}
}
