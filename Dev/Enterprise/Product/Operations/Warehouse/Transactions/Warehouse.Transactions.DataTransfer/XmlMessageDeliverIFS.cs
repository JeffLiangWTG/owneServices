using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.MessageDelivery;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.DataTransfer
{
	class XmlMessageDeliverIFS : XmlMessageDeliver
	{
		public XmlMessageDeliverIFS(MessageProcessorCommunicationModesResult modes, BusinessObject bizObjToDeliver, IValueObjectDataAdapter dataAdapter, ProcessTaskNotification action)
			: base(modes, bizObjToDeliver, bizObjToDeliver as IJobNumber, dataAdapter, null, action)
		{
		}

		#region GetXmlStreamToDeliver

		/// <summary>
		/// We need to create our own export Stream for 2 reasons:
		/// (1)	The value object of the IFSDataAdapter is of type Xsd.ConNote so we can handle an import file containing multiple con notes.
		///		However, we actually need to create a value object of type Xsd.ConNoteObject for export. 
		/// (2) The IFS system cannot handle our normal XML file with all the company info, etc at the start. So we just have to serialize the 
		///     Xsd.ConNoteObject value object ourselves.
		/// </summary>
		protected override Stream GetXmlStreamToDeliver(BusinessObject bizObj, IValueObjectDataAdapter dataAdapter, INotifications context)
		{
			var result = new MemoryStream();

			var conNote = (Xsd.ConNote)dataAdapter.ExportToValueObject((WhsOrder)bizObj, new ValueObjectExportContext(context));

			var conNoteObject = new Xsd.ConNoteObject();
			conNoteObject.ConNote.Add(conNote);

			var serializer = new XmlValueObjectSerializer(typeof(Xsd.ConNoteObject));
			serializer.Serialize(result, conNoteObject);
			result.Position = 0;

			return result;
		}

		#endregion

	}
}
