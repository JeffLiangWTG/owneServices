using System.IO;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.DataTransfer
{
	public class XmlValueObjectSerializerIFS : XmlValueObjectSerializer
	{
		public XmlValueObjectSerializerIFS()
			: base(typeof(Xsd.ConNote))
		{
		}

		#region ImportXmlData

		public override void ImportXmlData(Stream reader, IValueObjectDataAdapter dataAdapter, IBusinessObjectCollection collection, BusinessObjectFactoryProvider factoryProvider, INotifications notifications)
		{
			ApplyHacksToMakeImportFileElementNamesMatchXsd(ref reader);

			base.ImportXmlData(reader, dataAdapter, collection, factoryProvider, notifications);
		}

		/// <summary>
		/// The IFS XML Import File has different names for some elements when compared to the export file. We rename them to match so we can maintain just a single xsd file.
		/// </summary>
		void ApplyHacksToMakeImportFileElementNamesMatchXsd(ref Stream stream)
		{
			ZString xmlString;

			using (var sr = new StreamReader(stream))
			{
				xmlString = sr.ReadToEnd();
			}

			xmlString = xmlString.Replace("SmartFreight_Export_File", "ConNoteObject");
			xmlString = xmlString.Replace("Connote", "ConNote");

			byte[] byteArray = Encoding.ASCII.GetBytes(xmlString);

			stream = new MemoryStream(byteArray);
		}

		/// <summary>
		/// Each ConNote value object may contain multiple Freight Line details. Each Freight Line corresponds to a matching Enterprise Warehouse Order that we need to update.
		/// Hence, we need to update multiple business objects for a single value object (Normally this is a one to one relationship).
		/// The 'LastOrderToUpdate' flag is used to tell the DataAdapter that this is the last order on this conNote value object. The DataAdapter then checks that the   
		/// costs applied to each order will match the total cost for the ConNote. If there is a discrepancy caused by rounding, the billing on the last order is adjusted as required.
		/// </summary>
		protected override BusinessObject CreateOrUpdateFromValueObject(IValueObjectDataAdapter dataAdapter, IBusinessObjectCollection collection, IValueObject valueObject, IValueObjectImportContext context)
		{
			var conNote = (Xsd.ConNote)valueObject;

			var totalOrders = GetTotalOrdersOnConNote(conNote.FreightLineDetails);
			var currentOrderNo = 0;

			foreach (Xsd.FreightLineDetails freightLine in conNote.FreightLineDetails)
			{
				if (!freightLine.Ref.IsEmpty)
				{
					currentOrderNo++;

					var ifsAdapter = (WhsOrderCartageValueObjectDataAdapterIFS)dataAdapter;
					ifsAdapter.OrderExternalReference = freightLine.Ref;
					ifsAdapter.IsLastOrderToUpdate = (currentOrderNo == totalOrders);

					base.CreateOrUpdateFromValueObject(dataAdapter, collection, valueObject, context);
				}
			}

			return null;
		}

		int GetTotalOrdersOnConNote(Xsd.FreightLineDetailsCollection freightLines)
		{
			var result = 0;

			foreach (Xsd.FreightLineDetails line in freightLines)
			{
				if (!line.Ref.IsEmpty)
				{
					result++;
				}
			}

			return result;
		}

		#endregion
	}
}
