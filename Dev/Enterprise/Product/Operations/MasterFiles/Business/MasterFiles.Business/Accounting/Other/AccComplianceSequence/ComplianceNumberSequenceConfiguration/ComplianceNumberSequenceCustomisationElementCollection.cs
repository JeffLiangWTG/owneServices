using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Registry.Business.ComplianceNumberSequenceCustomisationElement;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class ComplianceNumberSequenceCustomisationElementCollection : RegistryBusinessObjectCollectionTemplate
	{
		public ComplianceNumberSequenceCustomisationElementCollection()
			: base()
		{
		}

		public ComplianceNumberSequenceCustomisationElementCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ComplianceNumberSequenceCustomisationElementCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ComplianceNumberSequenceCustomisationElementCollection(fallbackLevel, factory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ComplianceNumberSequenceCustomisationElement(CurrentFallbackLevel, CurrentFactory);
		}

		public new ComplianceNumberSequenceCustomisationElement AddNew()
		{
			return (ComplianceNumberSequenceCustomisationElement)base.AddNew();
		}

		public new ComplianceNumberSequenceCustomisationElement this[int x]
		{
			get { return (ComplianceNumberSequenceCustomisationElement)base[x]; }
		}

		public ComplianceNumberSequenceCustomisationElement this[string key]
		{
			get
			{
				foreach (ComplianceNumberSequenceCustomisationElement element in this)
				{
					if (element.ElementName == key)
					{
						return element;
					}
				}
				return null;
			}
		}

		public void PopulateElements()
		{
			var elementNames = GetAllElementNames();
			foreach (var name in elementNames)
			{
				var element = this.AddNew();
				element.ElementName = name;
				if (element.ElementName == ElementNames.SequenceNumber)
				{
					element.Include = true;
					element.Order = 50;
				}
			}
		}

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;

		protected override bool AllowSort => true;

		internal ComplianceNumberSequenceConfiguration ParentConfiguration { get; set; }
	}
}
