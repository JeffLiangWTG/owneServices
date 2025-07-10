using System.Collections;
using System.IO;
using CargoWise.ComponentModel;
using Enterprise.Customs.Business;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.DataTransfer
{
	public class DeclarationXmlDataTransferExporter : XmlDataTransferExporter
	{
		public DeclarationXmlDataTransferExporter(DeclarationValueObjectDataAdapter adapter, bool checkLicence)
			: base(adapter, checkLicence)
		{
		}

		public new DeclarationValueObjectDataAdapter Adapter
		{
			get { return (DeclarationValueObjectDataAdapter)base.Adapter; }
		}

		#region Export

		protected override void DoExport(Stream file, IList selectedBusinessObjects, INotifications notify)
		{
			if (selectedBusinessObjects.Count <= 0)
			{
				return;
			}

			var interchange = new DeclarationWithConsolShipmentDetailExporter().ExportDeclarationWithRelatedConsolShipmentDetails((BaseJobDeclaration)selectedBusinessObjects[0], Adapter, null);
			var interchangeSerialiser = new XmlValueObjectSerializer(typeof(XmlInterchange));
			interchangeSerialiser.Serialize(file, interchange);
		}

		#endregion
	}
}
