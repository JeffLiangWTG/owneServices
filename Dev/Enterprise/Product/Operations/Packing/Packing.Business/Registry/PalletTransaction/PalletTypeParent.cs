using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Packing.Business
{
	[XmlSerializerAssembly("Enterprise.Packing.Business.XmlSerializers")]
	public class PalletTypeParent : RegistryBusinessObjectTemplate
	{
		[BusinessObjectTestExclude] // setter is valid here
		public PalletTypeCollection Types
		{
			get { return types ?? (types = new PalletTypeCollection()); }
			private set
			{
				types = value;
				RegisterEditableChildObject(types);
			}
		}

		PalletTypeCollection types;

		ZXmlSerializer CollectionSerialiser
		{
			get { return collectionSerialiser ?? (collectionSerialiser = ZXmlSerializer.New(typeof(PalletTypeCollection))); }
		}

		ZXmlSerializer collectionSerialiser;

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			CollectionSerialiser.Serialize(writer, Types);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Types = (PalletTypeCollection)CollectionSerialiser.Deserialize(reader);
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var result = new PalletTypeParent();
			result.Types = (PalletTypeCollection)Types.Clone(fallbackLevel, factory);
			return result;
		}
	}
}
