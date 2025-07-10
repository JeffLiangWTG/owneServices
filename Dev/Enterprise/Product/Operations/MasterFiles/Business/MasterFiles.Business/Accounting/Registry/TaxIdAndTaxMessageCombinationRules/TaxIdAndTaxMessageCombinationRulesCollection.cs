using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class TaxIdAndTaxMessageCombinationRulesCollection : RegistryBusinessObjectCollectionTemplate
	{
		public TaxIdAndTaxMessageCombinationRulesCollection()
		{
		}

		public new TaxIdAndTaxMessageCombinationRules this[int i]
		{
			get { return SetParent((TaxIdAndTaxMessageCombinationRules)Elements[i]); }
		}

		public new TaxIdAndTaxMessageCombinationRules AddNew()
		{
			return SetParent((TaxIdAndTaxMessageCombinationRules)base.AddNew());
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var result = new TaxIdAndTaxMessageCombinationRulesCollection();
			result.CurrentFallbackLevel = fallbackLevel;
			return result;
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return SetParent(new TaxIdAndTaxMessageCombinationRules());
		}

		TaxIdAndTaxMessageCombinationRules SetParent(TaxIdAndTaxMessageCombinationRules method)
		{
			method.CurrentFallbackLevel = CurrentFallbackLevel;
			return method;
		}
	}
}
