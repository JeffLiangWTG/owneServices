using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefComplianceCommodityAlertFilterBusinessObject))]
	class RefComplianceCommodityAlertFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region TextFilters

		public void TestAlertName()
		{
			var alert1 = Factory.NewWithValidTestData<RefComplianceCommodityAlert>();
			var alert2 = Factory.NewWithValidTestData<RefComplianceCommodityAlert>();

			alert1.RCR_AlertCode = "exportControl2022";
			alert2.RCR_AlertCode = "importControl2022";
			alert1.RCR_AlertName = "Export Control List";
			alert2.RCR_AlertName = "Import Control List";

			Factory.Save();

			var filter = (ModuleTextFilter)FilterStripBizO["AlertName"];
			AssertNotNull("AlertName filter", filter);

			filter.Property = "Export Control List";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			var collection = Factory.Load<RefComplianceCommodityAlert>(FilterStripBizO.Filter);

			CombineAssertions(() =>
			{
				AssertCollectionContains("Equal 'Export Control List': Expect collection to contains alert1", alert1, collection);
				AssertCollectionNotContains("Equal 'Export Control List': Expect collection does not contain alert2", alert2, collection);
			});

			filter.Property = "Control List";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.IsActive = true;
			collection = Factory.Load<RefComplianceCommodityAlert>(FilterStripBizO.Filter);

			CombineAssertions(() =>
			{
				AssertCollectionContains("Contains 'Control List': Expect collection to contain alert1", alert1, collection);
				AssertCollectionContains("Contains 'Control List': Expect collection to contain alert2", alert2, collection);
			});

			filter.Property = "Import";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.IsActive = true;
			collection = Factory.Load<RefComplianceCommodityAlert>(FilterStripBizO.Filter);

			CombineAssertions(() =>
			{
				AssertCollectionNotContains("StartsWith 'Import': Expect collection does not contain alert1", alert1, collection);
				AssertCollectionContains("StartsWith 'Import': Expect collection to contain alert2", alert2, collection);
			});
		}

		public void TestAlertType()
		{
			var alert1 = Factory.NewWithValidTestData<RefComplianceCommodityAlert>();
			var alert2 = Factory.NewWithValidTestData<RefComplianceCommodityAlert>();
			var alert3 = Factory.NewWithValidTestData<RefComplianceCommodityAlert>();

			alert1.RCR_AlertCode = "exportControl2022";
			alert2.RCR_AlertCode = "importControl2022";
			alert3.RCR_AlertCode = "importControl2023";
			alert1.RCR_AlertType = "NOM";
			alert2.RCR_AlertType = "COM";
			alert3.RCR_AlertType = "LOC";

			Factory.Save();

			var filter = (ModuleTextFilter)FilterStripBizO["AlertType"];
			AssertNotNull("AlertType filter", filter);

			filter.Property = "NOM";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			var collection = Factory.Load<RefComplianceCommodityAlert>(FilterStripBizO.Filter);

			CombineAssertions(() =>
			{
				AssertCollectionContains("Equal 'NOM': Expect collection to contains alert1", alert1, collection);
				AssertCollectionNotContains("Equal 'NOM': Expect collection does not contain alert2", alert2, collection);
				AssertCollectionNotContains("Equal 'NOM': Expect collection does not contain alert3", alert3, collection);
			});

			filter.Property = "OM";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.IsActive = true;
			collection = Factory.Load<RefComplianceCommodityAlert>(FilterStripBizO.Filter);

			CombineAssertions(() =>
			{
				AssertCollectionContains("Contains 'OM': Expect collection to contain alert1", alert1, collection);
				AssertCollectionContains("Contains 'OM': Expect collection to contain alert2", alert2, collection);
				AssertCollectionNotContains("Contains 'OM': Expect collection to does not contain alert3", alert3, collection);
			});
		}

		public void TestRiskStatus()
		{
			var alert1 = Factory.NewWithValidTestData<RefComplianceCommodityAlert>();
			var alert2 = Factory.NewWithValidTestData<RefComplianceCommodityAlert>();

			alert1.RCR_AlertCode = "exportControl2022";
			alert2.RCR_AlertCode = "importControl2022";
			alert1.RCR_CommodityRiskStatus = "HSK";
			alert2.RCR_CommodityRiskStatus = "PRS";

			Factory.Save();

			var filter = (ModuleTextFilter)FilterStripBizO["RiskStatus"];
			AssertNotNull("RiskStatus filter", filter);

			filter.Property = "HSK";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			var collection = Factory.Load<RefComplianceCommodityAlert>(FilterStripBizO.Filter);

			CombineAssertions(() =>
			{
				AssertCollectionContains("Equal 'HSK': Expect collection to contains alert1", alert1, collection);
				AssertCollectionNotContains("Equal 'HSK': Expect collection does not contain alert2", alert2, collection);
			});
		}

		public void TestDirection()
		{
			var alert1 = Factory.NewWithValidTestData<RefComplianceCommodityAlert>();
			var alert2 = Factory.NewWithValidTestData<RefComplianceCommodityAlert>();

			alert1.RCR_TradeDirection = "EXP";
			alert2.RCR_TradeDirection = "IMP";

			Factory.Save();

			var filter = (ModuleTextFilter)FilterStripBizO["TradeDirection"];
			AssertNotNull("TradeDirection filter", filter);

			filter.Property = "EXP";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			var collection = Factory.Load<RefComplianceCommodityAlert>(FilterStripBizO.Filter);

			CombineAssertions(() =>
			{
				AssertCollectionContains("Equal 'EXP': Expect collection to contains alert1", alert1, collection);
				AssertCollectionNotContains("Equal 'EXP': Expect collection does not contain alert2", alert2, collection);
			});

			filter.Property = "P";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.IsActive = true;
			collection = Factory.Load<RefComplianceCommodityAlert>(FilterStripBizO.Filter);

			CombineAssertions(() =>
			{
				AssertCollectionContains("Contains 'P': Expect collection to contain alert1", alert1, collection);
				AssertCollectionContains("Contains 'P': Expect collection to contain alert2", alert2, collection);
			});
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			TestConnection.ExecuteNonQuery("delete from dbo.RefComplianceCommodityAlert"); //to fix "...very large number for a test. Change the test so it uses fewer objects" error
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new RefComplianceCommodityAlertFilterBusinessObject();
		}

		protected virtual FilterStripBusinessObject FilterStripBizO
		{
			get
			{
				if (fFilterStripBizO == null)
				{
					fFilterStripBizO = GetNewFilterStripBusinessObject();
				}
				return fFilterStripBizO;
			}
		}

		FilterStripBusinessObject fFilterStripBizO;

		#endregion
	}
}
