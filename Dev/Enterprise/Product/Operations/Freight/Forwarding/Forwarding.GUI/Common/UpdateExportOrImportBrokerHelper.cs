using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.DialogDefault;

namespace Enterprise.Freight.Forwarding.GUI
{
	public static class UpdateExportOrImportBrokerHelper
	{
		public static void UpdateExportOrImportBroker(object sender, BrokerDefaultingEventArgs e)
		{
			var isExportBroker = e.BrokerType == DocAddressType.ExportBroker;
			var caption = isExportBroker
				? ResString.GetMultilingualString("241882a7-7dd1-457f-9d39-e0bad9442b1e", "Export Broker")
				: ResString.GetMultilingualString("a219e93c-e6e3-42ad-97ff-2c64e0bee0a2", "Import Broker");
			var dialogID = new ZGuid(isExportBroker ? "ad12a41e-c5c0-4823-a898-221b37a3391f" : "a98bec6c-d554-412e-aa78-569dc78516e3");
			var message = isExportBroker
				? ResString.GetMultilingualString("28530291-d0e6-4ca0-807c-55fb2afc6599", "Consignor has been changed. Do you wish to update the Export Broker?")
				: ResString.GetMultilingualString("e4d198b3-077e-4590-984c-da23c57c90d8", "Consignee has been changed. Do you wish to update the Import Broker?");

			var dialogContext = new DialogDefaultContext(
					dialogID,
					caption,
					ZMessageBoxButtons.YesNo,
					ZMessageBoxIcon.Question,
					null,
					showCheckboxOnly: true);

			var dialogResult = Globals.Message.ShowOrDefault(dialogContext, message);
			e.ShouldUpdateBroker = dialogResult == ZDialogResult.Yes;
		}
	}
}
