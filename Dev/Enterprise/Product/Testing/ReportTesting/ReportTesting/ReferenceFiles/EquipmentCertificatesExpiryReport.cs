using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ReportTesting.ReferenceFiles
{
	[TemplateName("Equipment Certificates Expiry Report")]
	public class TestEquipmentCertificatesExpiryReport : TemplateTestCase
	{
	}

	public class EquipmentCertificatesExpiryReportTest : ReportTestCase
	{
		public override ZEmbeddedModule ModuleToTest
		{
			get { return (ZEmbeddedModule)ZModuleFactory.Instance.Create(ModuleIDs.RefFilesReports); }
		}

		public override string Hint
		{
			get { return "The Equipment Certificates Expiry Report shows Equipment/Vehicles details and their certificates/licenses/registration/next service dates and can be used to find the certificates/service info of a certain type that are close to expiry. This report can be scheduled to run at regular intervals to notify staff of upcoming renewals/services."; }
		}

		public override string MenuName
		{
			get { return "Equipment Certificates Expiry Report"; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestEquipmentCertificatesExpiryReport();
		}
	}
}
