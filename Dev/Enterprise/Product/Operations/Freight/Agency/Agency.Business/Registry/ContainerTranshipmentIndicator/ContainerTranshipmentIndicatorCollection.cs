using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Agency.Business
{
	[XmlSerializerAssembly("Enterprise.Freight.Agency.Business.XmlSerializers")]
	public sealed class ContainerTranshipmentIndicatorCollection : RegistryBusinessObjectCollectionTemplate, IXmlSerializable
	{
		public ContainerTranshipmentIndicatorCollection()
			: base() { }

		#region NewAndPopulate

		public static ContainerTranshipmentIndicatorCollection NewAndPopulate()
		{
			ContainerTranshipmentIndicatorCollection result = new ContainerTranshipmentIndicatorCollection();
			result.AddNew(ContainerTranshipmentIndicator.Keys.Empty, "E", "M", "P");
			result.AddNew(ContainerTranshipmentIndicator.Keys.BreakBulk, "B", "U", "A");
			result.AddNew(ContainerTranshipmentIndicator.Keys.Laden, "0", "T", "0");
			return result;
		}

		#endregion

		public ContainerTranshipmentIndicator Empty
		{
			get { return Find(ContainerTranshipmentIndicator.Keys.Empty); }
		}

		public ContainerTranshipmentIndicator BreakBulk
		{
			get { return Find(ContainerTranshipmentIndicator.Keys.BreakBulk); }
		}

		public ContainerTranshipmentIndicator Laden
		{
			get { return Find(ContainerTranshipmentIndicator.Keys.Laden); }
		}

		public new ContainerTranshipmentIndicator this[int index]
		{
			get { return (ContainerTranshipmentIndicator)Elements[index]; }
		}

		public new ContainerTranshipmentIndicator AddNew()
		{
			return (ContainerTranshipmentIndicator)base.AddNew();
		}

		#region Implementation

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ContainerTranshipmentIndicatorCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ContainerTranshipmentIndicator();
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		ContainerTranshipmentIndicator AddNew(string key, string direct, string tranship, string domestic)
		{
			ContainerTranshipmentIndicator result = AddNew();

			using (result.GetValidationSuspender())
			using (result.SuspendSettingHasChanges())
			{
				result.Key = key;
				result.Direct = direct;
				result.Tranship = tranship;
				result.Domestic = domestic;
			}

			return result;
		}

		ContainerTranshipmentIndicator Find(string key)
		{
			foreach (ContainerTranshipmentIndicator indicator in this)
			{
				if (indicator.Key == key)
				{
					return indicator;
				}
			}
			return null;
		}

		#endregion

		#region IXmlSerializable Members

		const string ContainerTranshipmentIndicatorLabel = "ContainerTranshipmentIndicator";

		XmlSchema IXmlSerializable.GetSchema()
		{
			return null;
		}

		void IXmlSerializable.ReadXml(XmlReader reader)
		{
			RemoveAll();

			if (reader.IsEmptyElement)
			{
				reader.Skip();
			}
			else
			{
				reader.ReadStartElement();

				while (reader.IsStartElement(ContainerTranshipmentIndicatorLabel))
				{
					ContainerTranshipmentIndicator indicator = AddNew();
					((IXmlSerializable)indicator).ReadXml(reader);
				}

				reader.ReadEndElement();
			}
		}

		void IXmlSerializable.WriteXml(XmlWriter writer)
		{
			foreach (ContainerTranshipmentIndicator indicator in this)
			{
				writer.WriteStartElement(ContainerTranshipmentIndicatorLabel);
				((IXmlSerializable)indicator).WriteXml(writer);
				writer.WriteEndElement();
			}
		}

		#endregion
	}
}


