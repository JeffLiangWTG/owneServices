using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class ComplianceDocumentRePrintRestrictionCollection : RegistryBusinessObjectCollectionTemplate
	{
		public ComplianceDocumentRePrintRestrictionCollection()
		{
		}

		public ComplianceDocumentRePrintRestrictionCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public new ComplianceDocumentRePrintRestriction this[int x]
		{
			get { return (ComplianceDocumentRePrintRestriction)base[x]; }
		}

		public new ComplianceDocumentRePrintRestriction AddNew()
		{
			return (ComplianceDocumentRePrintRestriction)base.AddNew();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ComplianceDocumentRePrintRestriction();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ComplianceDocumentRePrintRestrictionCollection();
		}
	}
}
