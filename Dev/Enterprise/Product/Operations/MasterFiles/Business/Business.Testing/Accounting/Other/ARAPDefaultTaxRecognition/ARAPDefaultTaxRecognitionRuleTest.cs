using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ARAPDefaultTaxRecognitionRule))]
	class ARAPDefaultTaxRecognitionRuleTest : RegistryBusinessObjectTemplateTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();

			configuration = new ARAPDefaultTaxRecognitionRule();
			configuration.LoginCompanyCountryRuleCode = "IEU";
			configuration.OrganizationCountryRuleCode = "OEU";
			configuration.TaxRecognitionCode = "NON";
			configurations = new ARAPDefaultTaxRecognitionRuleCollection();
			configurations.Add(configuration);
		}
		ARAPDefaultTaxRecognitionRule configuration;
		ARAPDefaultTaxRecognitionRuleCollection configurations;

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			ARAPDefaultTaxRecognitionRule item = new ARAPDefaultTaxRecognitionRule();
			item.LoginCompanyCountryRuleCode = "IEU";
			item.OrganizationCountryRuleCode = "OEU";
			item.TaxRecognitionCode = "NON";
			return item;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected new ARAPDefaultTaxRecognitionRule BizObj
		{
			get { return (ARAPDefaultTaxRecognitionRule)base.BizObj; }
		}

		protected virtual ARAPDefaultTaxRecognitionRuleCollection GetAuthorisationSettingsCollection()
		{
			return new ARAPDefaultTaxRecognitionRuleCollection();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}
	}
}
