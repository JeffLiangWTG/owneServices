using System.Collections;
using System.Globalization;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class ShipmentWithConsolExporter : IXmlDataTransferExporter
	{
		public void Export(ForwardingShipment shipment)
		{
			ExportCore(shipment);
		}

		protected virtual void ExportCore(ForwardingShipment shipment)
		{
			CommonConsol consol = shipment.LocalConsol;
			if (consol != null)
			{
				var adapter = new ForwardingConsolWithShipmentValueObjectDataAdapter(shipment);
				var director = GetXmlDirector(adapter);
				director.DefaultFileName = shipment.JS_UniqueConsignRef + "_" + ZDateTime.Now.ToString("yyyyMMddhhmmss", CultureInfo.InvariantCulture);
				if (!(new ZString(SystemDataRegistry.Instance.ShipmentAsCustomDeclarationExportDirectory.Value).IsEmpty))
				{
					director.InitialDirectory = SystemDataRegistry.Instance.ShipmentAsCustomDeclarationExportDirectory.Value;
				}
				director.PromptUserAndExport(new[] { consol });
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("f195cb22-748a-4269-ae3b-e8445c9962aa", "Link the Shipment to a Consol before running this data export."), Res.GetString("79eaad80-53b3-41d6-bf54-c03a01fa6bea", "Error exporting Shipment With Consol to XML"));
			}
		}

		public void PromptUserAndExport(IList selectedElements)
		{
			foreach (ForwardingShipment shipment in selectedElements)
			{
				if (Env.Security.MaintainShipmentExportToXml.IsAllowed)
				{
					Export(shipment);
				}
				else
				{
					Env.Security.MaintainShipmentExportToXml.ShowError();
				}
			}
		}

		public void PromptUserAndExport(CargoWise.EntityFramework.ZQuery exportQuery)
		{
			throw new System.NotImplementedException();
		}

		protected virtual XmlDataTransferExporter GetXmlDirector(IValueObjectDataAdapter adapter)
		{
			return new XmlDataTransferExporter(adapter, true);
		}
	}
}
