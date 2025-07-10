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
	public class JiraCustomFieldMap : RegistryBusinessObjectTemplateWithChildCollection
	{
		JiraCustomFieldMapCollection jiraEntityClassificationMap;

		public JiraCustomFieldMapCollection JiraClassificationMap
		{
			get
			{
				if (jiraEntityClassificationMap == null)
				{
					jiraEntityClassificationMap = new JiraCustomFieldMapCollection();
					RegisterEditableChildObject(jiraEntityClassificationMap);
				}

				return jiraEntityClassificationMap;
			}
		}

		#region RegistryBusinessObjectTemplate Overrides

		ZXmlSerializer collectionSerialiser;

		protected override IEnumerable<RegistryBusinessObjectCollectionTemplate> ChildCollections => new[] { JiraClassificationMap };

		ZXmlSerializer CollectionSerialiser
		{
			get
			{
				return collectionSerialiser ?? (collectionSerialiser = ZXmlSerializer.New(typeof(JiraCustomFieldMapCollection)));
			}
		}

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

		protected override void CopyCollectionsToClone(RegistryBusinessObjectTemplate clone, FallbackLevel currentFallbackLevel, BusinessObjectFactory factory)
		{
			base.CopyCollectionsToClone(clone, currentFallbackLevel, factory);

			var map = (JiraCustomFieldMap)clone;

			map.JiraClassificationMap.AddRange((BusinessObjectCollection)JiraClassificationMap.Clone(currentFallbackLevel, factory));
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			CollectionSerialiser.Serialize(writer, JiraClassificationMap);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			jiraEntityClassificationMap = (JiraCustomFieldMapCollection)CollectionSerialiser.Deserialize(reader);
		}

		#endregion RegistryBusinessObjectTemplate Overrides

		protected JiraCustomFieldMap CreateNewMapForClone()
		{
			return new JiraCustomFieldMap();
		}
	}
}
