using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class CertificateTypeCollection : RegistryBusinessObjectCollectionTemplate, IXmlSerializable, ICodeDescriptionPairList, ICertificateTypeCollection
	{
		public CertificateType Find(ZString code)
		{
			foreach (CertificateType type in this)
			{
				if (type.Code == code)
				{
					return type;
				}
			}

			return null;
		}

		public bool IsDuplicated(ZString code)
		{
			bool duplicated = false;
			foreach (CertificateType type in this)
			{
				if (type.Code == code)
				{
					if (duplicated)
					{
						return true;
					}
					else
					{
						duplicated = true;
					}
				}
			}

			return false;
		}

		public ZBool IsMandatory(ZString code)
		{
			CertificateType type = Find(code);
			return type == null ? ZBool.False : type.IsMandatory;
		}

		public ZBool IsUnique(ZString code)
		{
			CertificateType type = Find(code);
			return type == null ? ZBool.False : type.IsUnique;
		}

		public ZBool IsSystem(ZString code)
		{
			CertificateType type = Find(code);
			return type == null ? ZBool.False : type.IsSystem;
		}

		#region Overrides

		public new CertificateType this[int index]
		{
			get { return (CertificateType)Elements[index]; }
		}

		public new CertificateType AddNew()
		{
			return (CertificateType)base.AddNew();
		}

		#endregion

		#region IXmlSerializable Members

		const string CertificateType = "CertificateType";

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

				while (reader.IsStartElement(CertificateType))
				{
					CertificateType type = AddNew();
					((IXmlSerializable)type).ReadXml(reader);
				}

				reader.ReadEndElement();
			}
		}

		void IXmlSerializable.WriteXml(XmlWriter writer)
		{
			foreach (CertificateType type in this)
			{
				writer.WriteStartElement(CertificateType);
				((IXmlSerializable)type).WriteXml(writer);
				writer.WriteEndElement();
			}
		}

		#endregion

		#region ICodeDescriptionPairList Members

		public bool ContainsCode(object code)
		{
			return Find(code as string) != null;
		}

		public string GetDescriptionFromCode(string code)
		{
			CertificateType type = Find(code);
			return type == null ? null : type.Description.ToString();
		}

		#endregion

		#region ICertificateTypeCollection Members

		void ICertificateTypeCollection.AddNew(ZString code, ZString description, bool isMandatory, bool isUnique, bool isSystem, ZString alertType)
		{
			AddNew().SetupValues(code, description, isMandatory, isUnique, isSystem, alertType);
		}

		#endregion

		#region Implementation

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CertificateTypeCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CertificateType(this);
		}

		#endregion
	}
}
