namespace Enterprise.ReportTesting.Freight.Forwarding
{
	using System.Collections.Generic;
	using Enterprise.Freight.Forwarding.Module;
	using Enterprise.ReportTesting;

	[TemplateName("Shipment Detail Consol Report")]
	public class TestConsolwithShipmentTemplate : TemplateTestCase
	{
		protected override IEnumerable<string> SheetsNotRequiringColumnHeadings
		{
			get { return new string[] { "Template" }; }
		}
	}

	public class TestConsolwithShipment : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new ForwardingReportsModule(); }
		}

		public override string MenuName
		{
			get { return "Consol with Shipment"; }
		}

		public override string Hint
		{
			get
			{
				return @"The Consol with Shipment Report lists information from all top level shipments attached to a consol (i.e. top level co-load or assembly masters, BCN leads and subs, and all shipments with no master on this consol).

The following consol details will be shown: Vessel/Flight, Load, Discharge, ETA, ETD, Sending and Receiving Agents, Carrier.  

The following shipment details will be shown: Consignee, Consignor, Origin, Destination, Discharge, ETA, ETD, Weight and Volume.

The report can be limited by Consol Transport Mode (e.g. Air, Sea), Container Mode (e.g. LSE, BCN, FCL), Agents, Ports and ETA.
";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestConsolwithShipmentTemplate();
		}
	}
}
