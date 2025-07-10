using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccChargeTypeOverride))]
	sealed class AccChargeTypeOverrideTest : EnterpriseBusinessObjectTestCase
	{
		public void TestHasOveriddenInvoiceType()
		{
			AccChargeTypeOverride @override = Factory.New<AccChargeTypeOverride>();
			AssertEquals(false, @override.HasOveriddenInvoiceType);

			@override.AN_InvoiceType = "XXX";
			AssertEquals(true, @override.HasOveriddenInvoiceType);

			@override.AN_InvoiceType = "DEF";
			AssertEquals(false, @override.HasOveriddenInvoiceType);
		}

		public void TestMarginPercentage()
		{
			AccChargeCode charge = Factory.New<AccChargeCode>();
			AccChargeTypeOverride @override = charge.ChargeTypeOverrides.AddNew();
			AssertEquals(0m, @override.AN_MarginPercentage);

			@override.AN_ChargeType = "REV";
			AssertEquals(0m, @override.AN_MarginPercentage);
			Assert(@override.AN_MarginPercentageInfo.ReadOnly);

			@override.AN_ChargeType = "DSB";
			AssertEquals(100m, @override.AN_MarginPercentage);
			Assert(@override.AN_MarginPercentageInfo.ReadOnly);

			@override.AN_ChargeType = "OVR";
			AssertEquals(0m, @override.AN_MarginPercentage);
			Assert(@override.AN_MarginPercentageInfo.ReadOnly);

			@override.AN_ChargeType = "MRG";
			AssertEquals(100m, @override.AN_MarginPercentage);
			Assert(!@override.AN_MarginPercentageInfo.ReadOnly);

			@override.AN_ChargeType = "XYZ";
			AssertEquals(0m, @override.AN_MarginPercentage);
			Assert(@override.AN_MarginPercentageInfo.ReadOnly);
		}

		public void TestIsDuplicate()
		{
			AccChargeCode charge = Factory.New<AccChargeCode>();
			AccChargeTypeOverride type1 = charge.ChargeTypeOverrides.AddNew();
			AccChargeTypeOverride type2 = charge.ChargeTypeOverrides.AddNew();

			type1.AN_JobDirection = "A";
			type1.AN_JobType = "B";

			type2.AN_JobDirection = "A";
			type2.AN_JobType = "B";

			Assert(type1.IsDuplicate(type2));
			Assert(type2.IsDuplicate(type1));

			type2.AN_JobDirection = "D";
			Assert(!type1.IsDuplicate(type2));
			Assert(!type2.IsDuplicate(type1));

			type1.AN_JobDirection = "D";
			Assert(type1.IsDuplicate(type2));
			Assert(type2.IsDuplicate(type1));

			type1.AN_InvoiceType = "XXX";
			Assert(!type1.IsDuplicate(type2));
			Assert(!type2.IsDuplicate(type1));

			type2.AN_InvoiceType = "XXX";
			Assert(type1.IsDuplicate(type2));
			Assert(type2.IsDuplicate(type1));
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			AccChargeCode code = Factory.New<AccChargeCode>();
			return code.ChargeTypeOverrides.AddNew();
		}

		#endregion
	}
}
