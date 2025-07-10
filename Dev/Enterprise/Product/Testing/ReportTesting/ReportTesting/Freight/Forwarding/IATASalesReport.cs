using System.Collections.Generic;
using Enterprise.Freight.Forwarding.Module;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ReportTesting.Freight.Forwarding
{
	[TemplateName("IATA Sales Report")]
	public class TestIATASalesReport : TemplateTestCase
	{
		protected override IEnumerable<string> SheetsNotRequiringColumnHeadings
		{
			get
			{
				return new string[]
				{
					"IATA Sales",
					"Airline Cost Details Analysis"
				};
			}
		}
	}

	public class IATASalesReportTest : ReportTestCase
	{
		public override ZEmbeddedModule ModuleToTest
		{
			get { return new ForwardingReportsModule(); }
		}

		public override string Hint
		{
			get
			{
				return
@"The report supports two templates: 'IATA Sales' and 'Airline Cost Details Analysis'.

The 'IATA Sales' report displays the MAWB No, Issue date, Load and Discharge Ports, MAWB Weight and MAWB Cost, as required by IATA in the Agents' Report.
The MAWB Issued date is a mandatory selection criteria, plus it can be filtered by Airline.
The report will only include MAWB’s where the Final Master has been Printed and/or where the FWB has been submitted.

The 'Airline Cost Details Analysis' report in addition to the above columns shows Consol Charge Codes and Local Cost Amounts against airline carriers, as well as Total Shipment Chargeable (the sum of chargeable weights of shipments), and is designed to be used for internal analysis.";
			}
		}

		public override string MenuName
		{
			get { return "IATA Sales"; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestIATASalesReport();
		}
	}
}
