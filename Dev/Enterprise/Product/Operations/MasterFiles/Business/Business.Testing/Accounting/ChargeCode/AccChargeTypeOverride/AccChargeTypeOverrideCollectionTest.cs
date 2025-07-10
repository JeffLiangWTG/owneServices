using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccChargeTypeOverrideCollection))]
	sealed class AccChargeTypeOverrideCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestNoAuditLogsWithAttibute()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			Factory.Save();

			var chargeTypeOverride = chargeCode.ChargeTypeOverrides.AddNew();
			chargeTypeOverride.AN_JobDirection = "ABC";
			chargeTypeOverride.AN_JobType = "XXX";
			chargeTypeOverride.AN_ChargeType = "YYY";
			chargeTypeOverride.AN_MarginPercentage = 99.99;
			chargeTypeOverride.AN_InvoiceType = InvoiceTypesList.Codes.DoNotPost;
			Factory.Save();

			CombineAssertions(() =>
			{
				var query = new ZQuery(StmALogSchema.SL_Parent, chargeCode.PK);
				AssertEquals("Not expecting Add event.", 0, Factory.Load<StmALog>(query).Length);
				chargeTypeOverride.AN_InvoiceType = InvoiceTypesList.Codes.DoNotPost;
				Factory.Save();
				AssertEquals("Not expecting Edit event", 0, Factory.Load<StmALog>(query).Length);
				chargeCode.ChargeTypeOverrides.RemoveAndDeleteAll();
				Factory.Save();
				AssertEquals("Not expecting Delete event", 0, Factory.Load<StmALog>(query).Length);
			});
		}

		public void TestGetTypeOverridePermutation()
		{
			var code = Factory.NewWithValidTestData<AccChargeCode>();
			code.AC_Code = "CCODE1";
			code.AC_ChargeType = "MRG";
			code.AC_MarginPercentage = 8m;

			var code2 = Factory.NewWithValidTestData<AccChargeCode>();
			code2.AC_Code = "CCODE2";
			code2.AC_ChargeType = "MRG";
			code2.AC_MarginPercentage = 23m;

			var override1 = CreateTypeOverride(code, "IMP", "SHP", "REV", 0m);
			var override2 = CreateTypeOverride(code, "EXP", "SHP", "MRG", 15m);
			var override3 = CreateTypeOverride(code, "ALL", "BRK", "DSB", 0m);
			var override4 = CreateTypeOverride(code2, "IMP", "TRN", "REV", 0m);

			Factory.Save();

			var result = code.ChargeTypeOverrides.GetChargeType(JobInvoicingConsumerTypes.Shipment, Directions.Import);
			AssertEquals("Exact match found", "REV", result.AN_ChargeType);
			AssertEquals("Exact match found", 0m, result.AN_MarginPercentage);

			result = code.ChargeTypeOverrides.GetChargeType(JobInvoicingConsumerTypes.Shipment, Directions.Export);
			AssertEquals("Exact match found", "MRG", result.AN_ChargeType);
			AssertEquals("Exact match found", 15m, result.AN_MarginPercentage);

			result = code.ChargeTypeOverrides.GetChargeType(JobInvoicingConsumerTypes.Brokerage, Directions.Export);
			AssertEquals("Exact match found", "DSB", result.AN_ChargeType);
			AssertEquals("Exact match found", 0m, result.AN_MarginPercentage);

			result = code.ChargeTypeOverrides.GetChargeType(JobInvoicingConsumerTypes.CFSLoadList, Directions.Export);
			AssertEquals("Default charge type found", "MRG", result.AN_ChargeType);
			AssertEquals("Default charge margin percentage found", 8m, result.AN_MarginPercentage);

			result = code.ChargeTypeOverrides.GetChargeType(JobInvoicingConsumerTypes.LocalCartage, Directions.Import);
			AssertEquals("Default charge type found", "MRG", result.AN_ChargeType);
			AssertEquals("Default charge margin percentage found", 8m, result.AN_MarginPercentage);

			result = code2.ChargeTypeOverrides.GetChargeType(JobInvoicingConsumerTypes.LocalCartage, Directions.Import);
			AssertEquals("Exact match found", "REV", result.AN_ChargeType);
			AssertEquals("Margin", 0m, result.AN_MarginPercentage);

			result = code.ChargeTypeOverrides.GetChargeType(null, Directions.Import);
			AssertEquals("Default charge type found", "MRG", result.AN_ChargeType);
			AssertEquals("Default charge margin percentage found", 8m, result.AN_MarginPercentage);
		}

		#region Implementation

		AccChargeTypeOverride CreateTypeOverride(AccChargeCode chargeCode, ZString direction, ZString jobType, ZString chargeType, ZDecimal marginPercentage)
		{
			AccChargeTypeOverride @override = chargeCode.ChargeTypeOverrides.AddNew();
			@override.AN_JobDirection = direction;
			@override.AN_JobType = jobType;
			@override.AN_ChargeType = chargeType;
			@override.AN_MarginPercentage = marginPercentage;

			return @override;
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return Factory.New<AccChargeCode>().ChargeTypeOverrides;
		}

		#endregion
	}
}
