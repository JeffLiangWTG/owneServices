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
	public abstract class JiraEntityClassificationMap<TCollection, TItem> : RegistryBusinessObjectTemplateWithChildCollection
		where TItem : JiraEntityClassificationMapItem, new()
		where TCollection : JiraEntityClassificationMapItemCollection<TItem>, new()
	{
		public TCollection JiraClassificationMap
		{
			get
			{
				if (jiraEntityClassificationMap == null)
				{
					jiraEntityClassificationMap = new TCollection();
					RegisterEditableChildObject(jiraEntityClassificationMap);
				}

				return jiraEntityClassificationMap;
			}
		}

		TCollection jiraEntityClassificationMap;

		#region RegistryBusinessObjectTemplate Overrides

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			foreach (var item in JiraClassificationMap)
			{
				item.RunPreSaveValidation();
			}
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return CreateNewMapForClone();
		}

		protected abstract JiraEntityClassificationMap<TCollection, TItem> CreateNewMapForClone();

		protected override IEnumerable<RegistryBusinessObjectCollectionTemplate> ChildCollections => new[] { JiraClassificationMap };

		protected override void CopyCollectionsToClone(RegistryBusinessObjectTemplate clone, FallbackLevel currentFallbackLevel, BusinessObjectFactory factory)
		{
			base.CopyCollectionsToClone(clone, currentFallbackLevel, factory);

			var map = (JiraEntityClassificationMap<TCollection, TItem>)clone;

			map.JiraClassificationMap.AddRange((BusinessObjectCollection)JiraClassificationMap.Clone(currentFallbackLevel, factory));
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			CollectionSerialiser.Serialize(writer, JiraClassificationMap);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			jiraEntityClassificationMap = (TCollection)CollectionSerialiser.Deserialize(reader);
		}

		ZXmlSerializer CollectionSerialiser
		{
			get
			{
				return collectionSerialiser ?? (collectionSerialiser = ZXmlSerializer.New(typeof(TCollection)));
			}
		}

		ZXmlSerializer collectionSerialiser;

		#endregion
	}
}
