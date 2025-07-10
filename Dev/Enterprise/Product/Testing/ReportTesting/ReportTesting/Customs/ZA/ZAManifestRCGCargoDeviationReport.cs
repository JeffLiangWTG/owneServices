using Enterprise.Customs.Module;

namespace Enterprise.ReportTesting.Customs.ZA
{
	[TemplateName("ZA Manifest RCGCargo Deviation Report")]
	public class ZAManifestRCGCargoDeviationReportTemplate : TemplateTestCase
	{
	}

	public class ZAManifestRCGCargoDeviationReport : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new CustomsGlobalReportModule(); }
		}

		public override string MenuName
		{
			get
			{
				return "RCG Cargo Processing deviations in Manifests";
			}
		}

		public override string Hint
		{
			get
			{
				return @"This report identifies Manifests whose house bills are not cross-matched to any Declarations in the same company.
It is a list of all Manifests with a House Bill, irrespective of whether the House Bills are matched to a Declaration job or not.
Therefore, gaps and/or discrepancies in ""Matched to Declaration"" columns on the left hand side of this report indicate potential deviations that need to be handled in order to avoid consequences due to 'RCG deviations'.
The root cause of deviations might occur in either Manifests or Declaration so both Jobs may need to be reviewed and/or revised to eliminate potential deviations.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new ZAManifestRCGCargoDeviationReportTemplate();
		}
	}
}
