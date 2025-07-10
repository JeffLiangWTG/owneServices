using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class ARAPDefaultTaxRecognitionRuleCollection : RegistryBusinessObjectCollectionTemplate
	{
		public ARAPDefaultTaxRecognitionRuleCollection()
		{
		}

		public ARAPDefaultTaxRecognitionRuleCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public new ARAPDefaultTaxRecognitionRule this[int x]
		{
			get { return (ARAPDefaultTaxRecognitionRule)base[x]; }
		}

		public new ARAPDefaultTaxRecognitionRule AddNew()
		{
			return (ARAPDefaultTaxRecognitionRule)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ARAPDefaultTaxRecognitionRuleCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ARAPDefaultTaxRecognitionRule();
		}

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;
	}
}
