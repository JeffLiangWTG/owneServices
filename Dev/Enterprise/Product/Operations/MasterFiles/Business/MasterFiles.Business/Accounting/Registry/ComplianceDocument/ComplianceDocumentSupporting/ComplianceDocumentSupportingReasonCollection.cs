using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class ComplianceDocumentSupportingReasonCollection : RegistryBusinessObjectCollection
	{
		public ComplianceDocumentSupportingReasonCollection()
		{
		}

		public ComplianceDocumentSupportingReasonCollection(BusinessObjectFactory factory, FallbackLevel fallbackLevel)
			: base(fallbackLevel, factory)
		{
		}

		public new ComplianceDocumentSupportingReason this[int i]
		{
			get { return (ComplianceDocumentSupportingReason)Elements[i]; }
		}

		public new ComplianceDocumentSupportingReason AddNew()
		{
			return (ComplianceDocumentSupportingReason)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ComplianceDocumentSupportingReasonCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ComplianceDocumentSupportingReason();
		}
	}
}
