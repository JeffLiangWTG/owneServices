using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.ProcessManagement.Business
{
	[XmlSerializerAssembly("Enterprise.ProcessManagement.Business.XmlSerializers")]
	public class RecipientSourceFallbackHeader : RegistryBusinessObjectTemplateWithChildCollection
	{
		public static RecipientSourceFallbackHeader GetDefaultValue()
		{
			var header = new RecipientSourceFallbackHeader();
			var codeList = new RecipientSourceTypeList();

			for (var i = 1; i <= codeList.Count; i++)
			{
				var item = header.SourceCollection.AddNew();

				item.SourceType = codeList[i - 1].Code;
			}

			return header;
		}

		public RecipientSourceFallbackHeader()
		{
		}

		#region Properties

		public RecipientSourceCollection SourceCollection
		{
			get
			{
				if (sourceCollection == null)
				{
					sourceCollection = new RecipientSourceCollection();
					RegisterEditableChildObject(sourceCollection);
				}

				return sourceCollection;
			}
		}

		RecipientSourceCollection sourceCollection;

		#endregion

		#region RegistryBusinessObjectTemplateWithChildCollection Overrides

		protected override IEnumerable<RegistryBusinessObjectCollectionTemplate> ChildCollections => new[] { SourceCollection };

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new RecipientSourceFallbackHeader();
		}

		protected override void CopyCollectionsToClone(RegistryBusinessObjectTemplate clone, FallbackLevel currentFallbackLevel, BusinessObjectFactory factory)
		{
			base.CopyCollectionsToClone(clone, currentFallbackLevel, factory);

			var header = (RecipientSourceFallbackHeader)clone;

			header.SourceCollection.AddRange((BusinessObjectCollection)SourceCollection.Clone(currentFallbackLevel, factory));
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			CollectionSerialiser.Serialize(writer, SourceCollection);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			sourceCollection = (RecipientSourceCollection)CollectionSerialiser.Deserialize(reader);
		}

		ZXmlSerializer CollectionSerialiser
		{
			get
			{
				return collectionSerialiser ?? (collectionSerialiser = ZXmlSerializer.New(typeof(RecipientSourceCollection)));
			}
		}

		ZXmlSerializer collectionSerialiser;

		#endregion
	}
}
