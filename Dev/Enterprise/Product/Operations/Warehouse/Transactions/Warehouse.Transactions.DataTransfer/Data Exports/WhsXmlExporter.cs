using System.IO;
using CargoWise.ComponentModel;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.DataTransfer
{
	public abstract class WhsXmlExporter<TDocket, TValueObject>
		where TDocket : WhsDocket
		where TValueObject : IValueObject
	{
		protected WhsXmlExporter(WhsValueObjectDataAdapter<TDocket, TValueObject> adapter)
		{
			Adapter = adapter;
		}

		#region Export

		public void Export(TDocket docket, TextWriter writer, INotifications notify)
		{
			ExportCore(docket, writer, notify);
		}

		#endregion

		#region Export Core

		void ExportCore(TDocket docket, TextWriter writer, INotifications notify)
		{
			if (docket != null)
			{
				var buffer = new NotificationBuffer(notify);
				ExportToXml(docket, writer, buffer);
				if (!buffer.HasErrors)
				{
					docket.Factory.Save();
				}
			}
		}

		#endregion

		#region Xml Interchange

		public Xsd.XmlInterchange XmlInterchange => xmlInterchange;

		#endregion

		#region Implementation

		#region Export To Xml

		protected virtual void ExportToXml(TDocket docket, TextWriter writer, NotificationBuffer notify)
		{
			xmlInterchange = Adapter.ToXmlInterchange(new TDocket[] { docket }, new ValueObjectExportContext(notify));
			var interchangeSerialiser = new XmlValueObjectSerializer(typeof(Xsd.XmlInterchange));
			interchangeSerialiser.Serialize(writer, xmlInterchange);
		}

		#endregion

		protected readonly WhsValueObjectDataAdapter<TDocket, TValueObject> Adapter;
		protected Xsd.XmlInterchange xmlInterchange;

		#endregion
	}
}
