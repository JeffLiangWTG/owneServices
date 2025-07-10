namespace Enterprise.ReportTesting.Customs.Shared
{
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Customs.Business;
	using Enterprise.Customs.Module;
	using Enterprise.ReportTesting;

	[TemplateName("Import Cartage and Delivery Report")]
	public class TestImportCartageandDeliveryReport : TemplateTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			BusinessObjectFactory factory = new BusinessObjectFactory();

			BaseJobDeclarationCollection declarations = new BaseJobDeclarationCollection(factory);
			Declaration1 = declarations.AddNew();
			Declaration2 = declarations.AddNew();

			Declaration1.JE_DateAtFinalDestination = new ZDate(2006, 12, 1);
			Declaration2.JE_DateAtFinalDestination = new ZDate(2006, 12, 2);

			Declaration1.JE_RL_NKPortOfArrival = "AUSYD";
			Declaration1.JE_RL_NKFinalDestination = "AUSYD";

			Declaration2.JE_RL_NKPortOfArrival = "AUMEL";
			Declaration2.JE_RL_NKFinalDestination = "AUMEL";

			Declaration1.JE_ContainerMode = "LCL";
			Declaration2.JE_ContainerMode = "LCL";

			factory.Save();
		}
		BaseJobDeclaration Declaration1;
		BaseJobDeclaration Declaration2;

		string GetQueryWithSpecificFromAndTo(ZDateTime from, ZDateTime to)
		{
			return @"
                        SELECT
                            *
                        FROM
                            Report_ImportCartageAndDelivery
                                  (
                                    'AU',
                                    '" + Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany.PK + @"',
                                    'ALL',
                                    'ALL',
                                    'Other',
                                    '',
                                    '" + from.ToISO8601String() + @"',
                                    '" + to.ToISO8601String() + @"',
                                    '',
                                    '',
                                    'N',
									NULL,
									NULL,
									NULL,
									NULL
                                   )
                    ";
		}

		string GetQueryWithSpecificFinalDestination(string location)
		{
			return @"
                        SELECT
                            *
                        FROM
                            Report_ImportCartageAndDelivery
                                  (
                                    'AU',
                                    '" + Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany.PK + @"',
                                    'ALL',
                                    'ALL',
                                    'Other',
                                    '" + location + @"',
                                    '" + ZDateTime.MinSmallDateTimeValue.ToISO8601String() + @"',
                                    '" + ZDateTime.MaxSmallDateTimeValue.ToISO8601String() + @"',
                                    '',
                                    '',
                                    'N',
									NULL,
									NULL,
									NULL,
									NULL
                                   )
                    ";
		}

		public void TestDeclarationsFilterForGivenDates()
		{
			string query = GetQueryWithSpecificFromAndTo(new ZDateTime(2006, 12, 1, 0, 0, 0), new ZDateTime(2006, 12, 2, 0, 0, 0));
			System.Data.DataTable table = Enterprise.ZArchitecture.Core.Utilities.GetDataTableFromQuery(query);
			AssertEquals("Should return 1 row only", 1, table.Rows.Count);
			AssertEquals("If date range filtering is working should be Dec1", Declaration1.JE_DeclarationReference, table.Rows[0]["Shipment"]);

			query = GetQueryWithSpecificFromAndTo(new ZDateTime(2006, 12, 2, 0, 0, 0), new ZDateTime(2006, 12, 3, 0, 0, 0));
			table = Enterprise.ZArchitecture.Core.Utilities.GetDataTableFromQuery(query);
			AssertEquals("Should return 1 row only", 1, table.Rows.Count);
			AssertEquals("If date range filtering is working it should be Dec2", Declaration2.JE_DeclarationReference, table.Rows[0]["Shipment"]);
		}

		public void TestDeclarationsFilterForSpecificPortsOfArrival()
		{
			string query = GetQueryWithSpecificFinalDestination("AUSYD");
			System.Data.DataTable table = Enterprise.ZArchitecture.Core.Utilities.GetDataTableFromQuery(query);
			AssertEquals("Should return 1 row only", 1, table.Rows.Count);
			AssertEquals("If location filtering is working should be Dec1", Declaration1.JE_DeclarationReference, table.Rows[0]["Shipment"]);

			query = GetQueryWithSpecificFinalDestination("AUMEL");
			table = Enterprise.ZArchitecture.Core.Utilities.GetDataTableFromQuery(query);
			AssertEquals("Should return 1 row only", 1, table.Rows.Count);
			AssertEquals("If location filtering is working it should be Dec2", Declaration2.JE_DeclarationReference, table.Rows[0]["Shipment"]);
		}
	}

	public class TestImportCartageandDeliveryReportMenuSetup : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new CustomsReportModule(); }
		}

		public override string MenuName
		{
			get { return "Cartage - Import Delivery Report"; }
		}

		public override string Hint
		{
			get
			{
				return

					@"The Cartage Import Delivery Report lists all delivery dates by shipment/declaration/cartage company.
It can be run to report by Consignee/Importer; Vessel/Voyage/Flight or Cartage Company.
 
Run for a selected ETA date range, this report will show:
Container No, Importer, Carrier Ref, Vessel, Carrier, Est Arrival, Discharge Port, Available from date, Storage Start date, Issue date, Docs to Carrier date, Slot date, Estimated Delivery and Delivery date.

Note: This report works for both FORWARDING SHIPMENT and STANDALONE DECLARATION records";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestImportCartageandDeliveryReport();
		}
	}
}
