using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Recruiter.Business
{
	[XmlSerializerAssembly("Enterprise.Recruiter.Business.XmlSerializers")]
	public class EmailParsingRuleCollection : RegistryBusinessObjectCollectionTemplate
	{
		public new EmailParsingRule this[int i] => (EmailParsingRule)Elements[i];

		public new EmailParsingRule AddNew() => (EmailParsingRule)base.AddNew();

		protected override BusinessObject CreateNonPersistentBusinessObject() => new EmailParsingRule();

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			=> new EmailParsingRuleCollection();

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;
	}
}


