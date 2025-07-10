using System.Collections.Generic;

namespace Enterprise.ReportTesting.Freight
{
	[TemplateName("CFS Container Arrival Report")]
	public class TestCFSContainerArrivalReport : TemplateTestCase
	{
		protected override List<string> GetExcludedSheetNames()
		{
			return new List<string>() { "Container Arrival Report" };
		}
	}

	[TemplateName("CFS Container Dispatch Report")]
	public class TestCFSContainerDispatchReport : TemplateTestCase
	{
		protected override List<string> GetExcludedSheetNames()
		{
			return new List<string>() { "Container Dispatch Report" };
		}
	}

	[TemplateName("CFS Container Stock Report")]
	public class TestCFSContainerStockReport : TemplateTestCase
	{
	}

	[TemplateName("CFS Container Unpack Summary Report")]
	public class TestCFSContainerUnpackSummaryReport : TemplateTestCase
	{
	}

	[TemplateName("CFS Daily Receival Report")]
	public class TestCFSDailyReceivalReport : TemplateTestCase
	{
		protected override IEnumerable<string> SheetsNotRequiringColumnHeadings
		{
			get { return new string[] { "Template" }; }
		}
	}

	[TemplateName("CFS Load Lists with No Invoicing Job Report")]
	public class TestCFSLoadListswithNoInvoicingJobReport : TemplateTestCase
	{
	}

	[TemplateName("CFS Shipment Fumigation Report")]
	public class TestCFSShipmentFumigationReport : TemplateTestCase
	{
	}

	[TemplateName("CFS Shipments with No Invoicing Job Report")]
	public class TestCFSShipmentswithNoInvoicingJobReport : TemplateTestCase
	{
	}

	[TemplateName("CFS Unpack Stock Report")]
	public class TestCFSUnpackStockReport : TemplateTestCase
	{
		protected override IEnumerable<string> SheetsNotRequiringColumnHeadings
		{
			get { return new string[] { "CFS Import Unpack Stock Report" }; }
		}
	}

	[TemplateName("Consignor Buyer Report")]
	public class TestConsignorBuyerReport : TemplateTestCase
	{
	}

	[TemplateName("Consol Containers By Carrier Report")]
	public class TestConsolContainersByCarrierReport : TemplateTestCase
	{
		protected override IEnumerable<string> SheetsNotRequiringColumnHeadings
		{
			get
			{
				return new string[]
				{
					"ConsoleContainersByCarrier"
				};
			}
		}
	}

	[TemplateName("Consol Summary Report")]
	public class TestConsolSummaryReport : TemplateTestCase
	{
	}

	[TemplateName("Consol with no shipments report")]
	public class TestConsolwithnoshipmentsreport : TemplateTestCase
	{
		protected override IEnumerable<string> SheetsNotRequiringColumnHeadings
		{
			get
			{
				return new string[]
				{
					"Template"
				};
			}
		}
	}

	[TemplateName("Container Fumigation Report")]
	public class TestContainerFumigationReport : TemplateTestCase
	{
	}

	[TemplateName("Current Export Shipments By Client")]
	public class TestCurrentExportShipmentsByClient : TemplateTestCase
	{
	}

	[TemplateName("Export Manifest Report")]
	public class TestExportManifestReport : TemplateTestCase
	{
		protected override IEnumerable<string> SheetsNotRequiringColumnHeadings
		{
			get
			{
				return new string[]
				{
						"Template"
				};
			}
		}
	}

	[TemplateName("Forwarding Registrations by User")]
	public class TestForwardingRegistrationsbyUser : TemplateTestCase
	{
		protected override IEnumerable<string> SheetsNotRequiringColumnHeadings
		{
			get { return new string[] { "Forwarding RegistrationsByUser" }; }
		}
	}

	[TemplateName("Shipment Trade Stats by UNLOCO")]
	public class TestShipmentTradeStatsbyUNLOCOReport : TemplateTestCase
	{
		protected override IEnumerable<string> SheetsNotRequiringColumnHeadings
		{
			get { return new string[] { "ShipmentTradeStatsCountryUNLOCO" }; }
		}
	}

	[TemplateName("Shipment Trade Stats by Zone")]
	public class TestShipmentTradeStatsbyZoneReport : TemplateTestCase
	{
		protected override IEnumerable<string> SheetsNotRequiringColumnHeadings
		{
			get { return new string[] { "ShipmentTradeStatsByZone" }; }
		}
	}

	[TemplateName("Shipments Forecast Freight & CFS")]
	public class TestShipmentsForecastFreightAndCFS : TemplateTestCase
	{
		protected override IEnumerable<string> SheetsNotRequiringColumnHeadings
		{
			get
			{
				return new string[]
				{
					"Group By Vessel and Voyage",
					"Group By Arrival or Departure"
				};
			}
		}
	}

	[TemplateName("Shipments with no consol Report")]
	public class TestShipmentswithnoconsolReport : TemplateTestCase
	{
	}

	[TemplateName("Sailing Schedule")]
	public class TestSchedule : TemplateTestCase
	{
	}

	[TemplateName("Load List Volume and Profit Summary")]
	public class TestLoadListVolumeandProfitSummary : TemplateTestCase
	{
		protected override IEnumerable<string> SheetsNotRequiringColumnHeadings
		{
			get
			{
				return new string[]
				{
						"Template"
				};
			}
		}
	}

	[TemplateName("MAWB - Borrowed In Bills Report")]
	public class TestMAWB_BorrowedInBillsReport : TemplateTestCase
	{
		protected override IEnumerable<string> SheetsNotRequiringColumnHeadings
		{
			get
			{
				return new string[]
				{
						"Template"
				};
			}
		}
	}

	[TemplateName("MAWB - Borrowed Out Bills")]
	public class TestMAWB_BorrowedOutBills : TemplateTestCase
	{
		protected override IEnumerable<string> SheetsNotRequiringColumnHeadings
		{
			get
			{
				return new string[]
				{
						"Template"
				};
			}
		}
	}
}
