using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccCashAdvanceDefaultingConfigurationLinkedChargeCodeCollection))]
	sealed class AccCashAdvanceDefaultingConfigurationLinkedChargeCodeCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestRelationshipFilter()
		{
			var chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			var chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();

			var config1 = Factory.New<AccCashAdvanceDefaultingConfiguration>();
			config1.CAC_JobType = "SHP";
			config1.CAC_ServiceDirection = Core.Constants.FreightShipmentDirection.Code.Export;
			config1.CAC_DefaultingOption = CashAdvanceDefaultingOption.All;

			var config2 = Factory.New<AccCashAdvanceDefaultingConfiguration>();
			config1.CAC_JobType = "SHP";
			config1.CAC_ServiceDirection = Core.Constants.FreightShipmentDirection.Code.Import;
			config2.CAC_DefaultingOption = CashAdvanceDefaultingOption.SelectedChargeCodesAndGroups;
			var chargeCodePivot1 = config2.ChargeCodes.AddNew();
			chargeCodePivot1.JCT_ParentId = chargeCode1.PK;

			var config3 = Factory.New<AccCashAdvanceDefaultingConfiguration>();
			config3.CAC_JobType = "SHP";
			config3.CAC_ServiceDirection = Core.Constants.FreightShipmentDirection.Code.Domestic;
			config3.CAC_DefaultingOption = CashAdvanceDefaultingOption.SelectedChargeCodesAndGroups;
			var chargeCodePivot2 = config3.ChargeCodes.AddNew();
			chargeCodePivot2.JCT_ParentId = chargeCode2.PK;

			Factory.Save();

			AssertEquals(0, config1.ChargeCodes.Count);
			AssertContainsExactElementsInAnyOrder(new[] { chargeCode1.PK }, config2.ChargeCodes.Cast<CashAdvanceDefaultingChargeCode>().Select(x => x.JCT_ParentId));
			AssertContainsExactElementsInAnyOrder(new[] { chargeCode2.PK }, config3.ChargeCodes.Cast<CashAdvanceDefaultingChargeCode>().Select(x => x.JCT_ParentId));

			var collection1 = new AccCashAdvanceDefaultingConfigurationLinkedChargeCodeCollection(config1);
			collection1.Load();
			AssertEquals(0, collection1.Count);

			var collection2 = new AccCashAdvanceDefaultingConfigurationLinkedChargeCodeCollection(config2);
			collection2.Load();
			AssertContainsExactElementsInAnyOrder(new[] { chargeCode1 }, collection2);

			var collection3 = new AccCashAdvanceDefaultingConfigurationLinkedChargeCodeCollection(config3);
			collection3.Load();
			AssertContainsExactElementsInAnyOrder(new[] { chargeCode2 }, collection3);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			var config = Factory.New<AccCashAdvanceDefaultingConfiguration>();
			config.CAC_DefaultingOption = CashAdvanceDefaultingOption.SelectedChargeCodesAndGroups;
			var chargeCodePivot = config.ChargeCodes.AddNew();
			chargeCodePivot.JCT_ParentId = chargeCode.PK;
			Factory.Save();

			return new AccCashAdvanceDefaultingConfigurationLinkedChargeCodeCollection(config);
		}
	}
}
