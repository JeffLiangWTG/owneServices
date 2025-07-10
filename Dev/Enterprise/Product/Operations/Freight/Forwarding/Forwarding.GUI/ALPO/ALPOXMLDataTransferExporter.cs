using System.Collections;
using System.Globalization;
using System.IO;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.GUI
{
	public class ALPOXMLDataTransferExporter : XmlDataTransferExporter
	{
		public ALPOXMLDataTransferExporter(IValueObjectDataAdapter adapter, bool checkForLicence)
			: base(adapter, checkForLicence)
		{
		}

		protected override void PromptUserAndExportCore(IList selectedElements)
		{
			ForwardingShipment shipment = (ForwardingShipment)selectedElements[0];
			if (ALPOHelper.GetShipmentTransport(shipment) != null || ALPOHelper.GetConsol(shipment) != null)
			{
				ZString fileNamePrefix = shipment.IsImport() ? "IMP_" : "EXP_";
				this.DefaultFileName = fileNamePrefix
									   + shipment.JS_UniqueConsignRef
									   + "_" + ZDateTime.Now.ToString("yyyyMMddhhmmss", CultureInfo.InvariantCulture);

				if (new ZString(FreightDataRegistry.Instance.ALPOExportFileName.Value).IsEmpty)
				{
					this.DefaultFileName = "ALPO_" + this.DefaultFileName;
				}
				else
				{
					this.DefaultFileName = FreightDataRegistry.Instance.ALPOExportFileName.Value + this.DefaultFileName;
				}

				if (!(new ZString(SystemDataRegistry.Instance.ALPOExportDirectory.Value).IsEmpty))
				{
					this.InitialDirectory = SystemDataRegistry.Instance.ALPOExportDirectory.Value;
				}
				RunExport(selectedElements);
			}
			else
			{
				Globals.Message.ShowWarning(AvailabilityWarning, AvailabilityCaption);
			}
		}

		public virtual void RunExport(IList selectedElements)
		{
			if (Directory.Exists(this.InitialDirectory))
			{
				string filename = Path.Combine(this.InitialDirectory, this.DefaultFileName + ".xml");
				base.ExportFile(selectedElements, new UnattendedXmlDataTransferExporterGUI(filename));
				Globals.Message.ShowInformation(Res.GetString("38F59D8A-2A66-405e-B813-37D2CC83D7E2", "File saved at {0}", filename));
			}
			else
			{
				base.PromptUserAndExportCore(selectedElements);
			}
		}

		static string AvailabilityWarning
		{
			get { return Res.GetString("42de7abc-3d6d-44a9-be13-e0cfdb953d02", "This option is only available for Sea Shipments with routing legs that load or discharge in \"DEBRE\", \"DEHAM\", \"DEBRV\" or \"DECUX\" ports."); }
		}
		static string AvailabilityCaption
		{
			get { return Res.GetString("662aa8e5-4e5c-42f1-8e56-765a2e5dcda4", "ALPO error"); }
		}

		protected override void DoExport(Stream file, IList selectedBusinessObjects, INotifications notifications)
		{
			var serialiser = GetSerialiser();
			serialiser.ExportXmlData(file, Adapter, selectedBusinessObjects, new ValueObjectExportContext(notifications), "", "", "");
		}

		public XmlValueObjectSerializer GetSerialiser()
		{
			return new ALPOXmlValueObjectSerializer(Adapter.ValueObjectType);
		}
	}
}
