using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.BIRD;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(CusEntryHeaderCharges))]
	sealed class CusEntryHeaderChargeTest : ZArchitecture.Business.Testing.EnterpriseBusinessObjectTestCase
	{
		public void TestIFee()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			CusEntryHeaderCharges charge = entry.Charges.AddNew();
			charge.C1_ChargeAmount = 890324m;
			charge.C1_ChargeType = "AAA";
			IFee fee = charge;
			AssertEquals(890324m, fee.Amount);
			AssertEquals("AAA", fee.Code);
			AssertEquals(false, fee.IsOverridden);
		}

		public void TestChargeTypeUniqueness()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			CusEntryHeaderCharges charge1 = entry.Charges.AddNew();
			charge1.C1_ChargeType = "AAA";
			CusEntryHeaderCharges charge2 = entry.Charges.AddNew();
			AssertNoExceptionThrown(delegate
			{
				charge2.C1_ChargeType = "AAA";
			});
		}

		public void TestWhenDutyFeeCalculationIsManualMode()
		{
			//informal fee $2
			string messageText = "AA7501XJ58888B00001001                  200901011212120100  6004772             10A888891-01319900091-013199000                 8         XJ5 6001767611891  DC 20     APL EMERALD         102809                           V123W               22            OBL66                               00000001PK         AAAA       30                                  0 P             1                   AAAA    3431100000200                                                                   40001HK00000030000000009000                    000000005060267                  50 74199930000000009000            KG                               AU120908    51                                                                              521                                                                             60                                        KNBEREQU6LON                          8931100000000200                                                                9000000009000           0                       0000000020000000003000          ZZ7501000000010                                                                 ";
			ABIInputBlockControlGenerator<BRDAA, BRDZZ> generator = new ABIInputBlockControlGenerator<BRDAA, BRDZZ>();
			generator.Deserialise(messageText);
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			NotificationCollection notifications = new NotificationCollection();
			new BIRDDeclarationDataAdapter().DoImport(declaration, generator, notifications);
			AssertEquals(true, declaration.US_NoDutyCalc);
			AssertEquals(2m, declaration.ActiveEntryHeaders.EntrySummaryEntry.InformalFee);
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			//The informal fee should have been retained
			AssertEquals(2m, declaration.ActiveEntryHeaders.EntrySummaryEntry.InformalFee);
		}

		public void TestShouldDeleteIfChargeAmountIsZero()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var charge1 = entry.Charges.AddNew();
			charge1.C1_ChargeType = "AAA";
			var charge2 = entry.Charges.AddNew();
			charge2.C1_ChargeType = "BBB";
			charge2.C1_ChargeAmount = 10m;
			Factory.Save();
			AssertEquals("Should have been deleted, because Amount is zero and not reconciliation", true, charge1.IsDeleted);
			AssertEquals("Should have not been deleted", false, charge2.IsDeleted);
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			var reconEntry = reconDeclaration.OriginalEntries.AddNew();
			var reconCharge1 = reconEntry.ReconCharges.AddNew();
			reconCharge1.C1_ChargeType = "AAA";
			var reconCharge2 = reconEntry.ReconCharges.AddNew();
			reconCharge2.C1_ChargeType = "BBB";
			reconCharge2.C1_ChargeAmount = 15.2m;
			Factory.Save();
			AssertEquals("Should have not been deleted, because reconciliation, even if amount is zero", false, reconCharge1.IsDeleted);
			AssertEquals("Should have not been deleted", false, reconCharge2.IsDeleted);
		}

		public void TestChargeTypeAndChargeAmount_ReadOnly()
		{
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			var reconEntry = reconDeclaration.OriginalEntries.AddNew();
			reconEntry.US_R_MonthlyFiling = true;
			var invoiceLine = reconEntry.Invoice.InvoiceLines.AddNew();
			var reconCharge1 = reconEntry.ReconCharges.AddNew();
			reconCharge1.C1_ChargeType = "499";
			var reconCharge2 = reconEntry.ReconCharges.AddNew();
			reconCharge2.C1_ChargeType = "501";
			reconEntry.UpdateChargesReadOnlyState();
			Assert(!reconCharge1.ReadOnly);
			Assert("Amount of 499 should be editable", !reconCharge1.C1_ChargeAmount_ReadOnly);
			Assert("Type of 499 should not be editable", reconCharge1.C1_ChargeType_ReadOnly);
			Assert("Amount of 501 should not be editable", reconCharge2.C1_ChargeAmount_ReadOnly);
			Assert("Code of 501 should not be editable", reconCharge2.C1_ChargeType_ReadOnly);
			reconEntry.US_R_NoLineDetails = true;
			Assert("editable", !reconCharge1.C1_ChargeAmount_ReadOnly);
			Assert("editable", !reconCharge1.C1_ChargeType_ReadOnly);
			Assert("editable", !reconCharge2.C1_ChargeAmount_ReadOnly);
			Assert("editable", !reconCharge2.C1_ChargeType_ReadOnly);
		}

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

		public override List<ZString> GetExcludedColumns_OnlySomeSubclassesAreSetDefaultValues()
		{
			return new List<ZString>() { CusEntryHeaderCharges.Schema.C1_Source };
		}
	}
}
