using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CusEntryHeaderCharges))]
	sealed class CusEntryHeaderChargeTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			return entry.Charges.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var charge = entry.Charges.AddNew();
			charge.C1_ChargeAmount = 10m;
			return charge;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			return entry.Charges.AddNew();
		}

		[ExpectNoExceptions]
		public void TestC1_ChargeTypeDesc()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var refCusRateTypeTW = helper.CreateCusRateType(Core.Constants.CountryCodes.Taiwan, "DTY");
			var refCusRateCode1 = helper.LoadOrCreateNewCusRateCode(Factory, "B40", refCusRateTypeTW.PK, description: "Business Tax");
			var refCusTaxOrFeeTw = helper.CreateTaxOrFee("TPF", 0.00040000m, Core.Constants.CountryCodes.Taiwan, description: "Trade Promotion Fee");
			Factory.Save();
			var jobDeclaration = Factory.New<JobDeclaration>();
			var customsEntryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
			var cusEntryHeaderCharge = customsEntryHeader.Charges.AddNew();
			var chargeTypeList = cusEntryHeaderCharge.Lookups.ChargeTypeList;
			CombineAssertions(() =>
			{
				cusEntryHeaderCharge.C1_ChargeType = "B40";
				NUnit.Framework.Assert.That(cusEntryHeaderCharge.C1_ChargeTypeDesc, NUnit.Framework.Is.EqualTo("Business Tax").Using(CustomComparers.TypeComparison));
				cusEntryHeaderCharge.C1_ChargeType = "TPF";
				NUnit.Framework.Assert.That(cusEntryHeaderCharge.C1_ChargeTypeDesc, NUnit.Framework.Is.EqualTo("Trade Promotion Fee").Using(CustomComparers.TypeComparison));
			}

			);
		}

		[ExpectNoExceptions]
		public void TestC1_MethodOfPaymentDesc()
		{
			CusEntryHeaderCharges charge = (CusEntryHeaderCharges)GetNewBusinessObject();
			CombineAssertions(() =>
			{
				charge.C1_MethodOfPayment = EntryChargePaymentMethod.Codes.CAS;
				NUnit.Framework.Assert.That(charge.C1_MethodOfPaymentDesc, NUnit.Framework.Is.EqualTo("Cash").Using(CustomComparers.TypeComparison));
				charge.C1_MethodOfPayment = EntryChargePaymentMethod.Codes.DEF;
				NUnit.Framework.Assert.That(charge.C1_MethodOfPaymentDesc, NUnit.Framework.Is.EqualTo("Deferred").Using(CustomComparers.TypeComparison));
			}

			);
		}

		[ExpectNoExceptions]
		public void TestCheckChargeTypeUniqueness()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			var charge1 = entryHeader.Charges.AddNew();
			charge1.C1_ChargeType = "AA";
			charge1.C1_MethodOfPayment = "BB";
			NUnit.Framework.Assert.That(ErrorReporter.LastMessageReported, NUnit.Framework.Is.Not.EqualTo("AA-BB is not unique in this CusEntryHeaderChargesCollection."));
			var charge2 = entryHeader.Charges.AddNew();
			charge2.C1_ChargeType = "BB";
			charge2.C1_MethodOfPayment = "BB";
			NUnit.Framework.Assert.That(ErrorReporter.LastMessageReported, NUnit.Framework.Is.Not.EqualTo("BB-BB is not unique in this CusEntryHeaderChargesCollection."));
			var charge3 = entryHeader.Charges.AddNew();
			charge3.C1_ChargeType = "BB";
			charge3.C1_MethodOfPayment = "CC";
			NUnit.Framework.Assert.That(ErrorReporter.LastMessageReported, NUnit.Framework.Is.Not.EqualTo("BB-CC is not unique in this CusEntryHeaderChargesCollection."));
		}

		public override List<ZString> GetExcludedColumns_OnlySomeSubclassesAreSetDefaultValues()
		{
			return new List<ZString>() { CusEntryHeaderCharges.Schema.C1_Source };
		}
	}
}
