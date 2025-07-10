using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class InvoiceRemittanceCustomisationElementCollection : RegistryBusinessObjectCollectionTemplate
	{
		public InvoiceRemittanceCustomisationElementCollection()
			: base()
		{
		}

		public InvoiceRemittanceCustomisationElementCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public InvoiceRemittanceCustomisationElementCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new InvoiceRemittanceCustomisationElementCollection(fallbackLevel, factory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new InvoiceRemittanceCustomisationElement(CurrentFallbackLevel, CurrentFactory);
		}

		public new InvoiceRemittanceCustomisationElement AddNew()
		{
			return (InvoiceRemittanceCustomisationElement)base.AddNew();
		}

		public new InvoiceRemittanceCustomisationElement this[int x]
		{
			get { return (InvoiceRemittanceCustomisationElement)base[x]; }
		}

		public InvoiceRemittanceCustomisationElement this[string key]
		{
			get
			{
				foreach (InvoiceRemittanceCustomisationElement element in this)
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
			var elementNames = InvoiceRemittanceCustomisationElement.GetAllElementNames();
			foreach (var name in elementNames)
			{
				this.AddNew().ElementName = name;
			}
		}

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;

		protected override bool AllowSort => true;

		internal InvoiceRemittanceConfiguration ParentConfiguration { get; set; }
	}
}
