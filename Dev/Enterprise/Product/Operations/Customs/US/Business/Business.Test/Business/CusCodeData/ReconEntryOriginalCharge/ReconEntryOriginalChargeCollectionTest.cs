using System.ComponentModel;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(ReconEntryOriginalChargeCollection))]
	sealed class ReconEntryOriginalChargeCollectionTest : Customs.Business.Testing.CusCodeDataCollectionTest<ReconEntryOriginalCharge>
	{
		public void TestSetDefaultValues()
		{
			var declaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			var originalEntry = declaration.OriginalEntries.AddNew();
			originalEntry.US_R_NoLineDetails = true;
			AssertEquals(true, originalEntry.OriginalCharges.AddNew().CY_IsOverridden);
			var originalEntry2 = declaration.OriginalEntries.AddNew();
			originalEntry2.US_R_NoLineDetails = true;
			AssertEquals(true, originalEntry2.OriginalCharges.AddNew().CY_IsOverridden);
			var originalEntry3 = declaration.OriginalEntries.AddNew();
			originalEntry3.US_R_NoLineDetails = false;
			originalEntry3.Invoice.InvoiceLines.AddNew();
			AssertEquals(false, originalEntry3.OriginalCharges.AddNew().CY_IsOverridden);
			var invoiceLine = originalEntry.Invoice.JobComInvoiceLines.AddNew();
			AssertEquals(false, invoiceLine.ReconOriginalCharges.AddNew().CY_IsOverridden);
		}

		public void TestUpdateFromReadOnlyState()
		{
			ReconEntryOriginalChargeCollection coll = new ReconEntryOriginalChargeCollection(Entry);
			coll.SetReadOnlyIncludingChildren(true);
			ReconEntryOriginalCharge charge = coll.AddNew();
			AssertEquals("PreCondition", false, charge.CY_IsOverridden);
			coll.UpdateFromReadOnlyState(false);
			AssertEquals("CY_IsOverridden", true, charge.CY_IsOverridden);
			coll.UpdateFromReadOnlyState(true);
			AssertEquals("CY_IsOverridden", false, charge.CY_IsOverridden);
		}

		public void TestAddingOriginalFeeWillAddRecoFee()
		{
			JobDeclaration reconInnerDec = Factory.New<JobDeclaration>();
			ReconDeclaration reconDeclaration = new ReconDeclaration(reconInnerDec);
			ReconOriginalEntryHeader reconOriginalEntry = reconDeclaration.OriginalEntries.AddNew();
			ReconEntryOriginalChargeCollection originalCharges = reconOriginalEntry.OriginalCharges;
			ICancelAddNew collection = originalCharges;
			var charge1 = originalCharges.AddNew();
			AssertEquals(0, reconOriginalEntry.ReconCharges.Count);
			charge1.CY_Code = "AAA";
			collection.EndNew(0);
			AssertEquals(0, reconOriginalEntry.ReconCharges.Count);
			ReconEntryOriginalCharge charge2 = (ReconEntryOriginalCharge)((IBindingList)originalCharges).AddNew();
			AssertEquals(0, reconOriginalEntry.ReconCharges.Count);
			charge2.CY_Code = "AAA";
			collection.EndNew(1);
			AssertEquals(1, reconOriginalEntry.ReconCharges.Count);
			AssertEquals("AAA", reconOriginalEntry.ReconCharges[0].C1_ChargeType);
			var invoice = reconOriginalEntry.Invoice;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			originalCharges = invoiceLine.ReconOriginalCharges;
			collection = originalCharges;
			var charge3 = originalCharges.AddNew();
			AssertEquals(0, invoiceLine.FeeCusCodes.Count);
			charge3.CY_Code = "AAA";
			collection.EndNew(0);
			AssertEquals(0, invoiceLine.FeeCusCodes.Count);
			ReconEntryOriginalCharge charge4 = (ReconEntryOriginalCharge)((IBindingList)originalCharges).AddNew();
			AssertEquals(0, invoiceLine.FeeCusCodes.Count);
			charge4.CY_Code = "AAA";
			collection.EndNew(1);
			AssertEquals(1, invoiceLine.FeeCusCodes.Count);
			AssertEquals("AAA", invoiceLine.FeeCusCodes[0].CY_Code);
		}

		public void TestHasDuplicate()
		{
			ReconEntryOriginalChargeCollection coll = new ReconEntryOriginalChargeCollection(Entry);
			coll.AddNew(Core.Constants.USCustoms.FeeCodes.Beef, 1m);
			AssertEquals(false, coll.HasDuplicate(Core.Constants.USCustoms.FeeCodes.Beef));
			coll.AddNew(Core.Constants.USCustoms.FeeCodes.Beef, 2m);
			AssertEquals(true, coll.HasDuplicate(Core.Constants.USCustoms.FeeCodes.Beef));
		}

		public void TestTotalFeeAmount()
		{
			var coll = new ReconEntryOriginalChargeCollection(Entry);
			coll.AddNew(Core.Constants.USCustoms.FeeCodes.Beef, 1m);
			coll.AddNew(Core.Constants.USCustoms.FeeCodes.Duty, 2m);
			coll.AddNew(Core.Constants.USCustoms.FeeCodes.DistilledSpirits, 4m);
			AssertEquals("TotalFeeAmount", 1m, coll.TotalFeeAmount);
		}

		public void TestIFees()
		{
			ReconEntryOriginalChargeCollection coll = new ReconEntryOriginalChargeCollection(Entry);
			IFees fees = coll;
			AssertNull(fees.GetFeeFor("AAA"));
			IFee fee = fees.AddNew();
			AssertNotNull(fee);
			fee.Code = "AAA";
			IFee fee2 = fees.AddNew();
			fee2.Code = "BBB";
			AssertEquals(fee, fees.GetFeeFor("AAA"));
		}

		public void TestGetAmount()
		{
			ReconEntryOriginalChargeCollection coll = new ReconEntryOriginalChargeCollection(Entry);
			ReconEntryOriginalCharge charge1 = coll.AddNew();
			charge1.CY_Code = "AAA";
			charge1.CY_Amount = 10m;
			ReconEntryOriginalCharge charge2 = coll.AddNew();
			charge2.CY_Code = "BBB";
			charge2.CY_Amount = 20m;
			AssertEquals(10m, coll.GetAmount("AAA"));
			AssertEquals(0m, coll.GetAmount("CCC"));
		}

		public void TestGetTotal()
		{
			ReconEntryOriginalChargeCollection coll = new ReconEntryOriginalChargeCollection(Entry);
			ReconEntryOriginalCharge charge1 = coll.AddNew();
			charge1.CY_Code = "AAA";
			charge1.CY_Amount = 10m;
			ReconEntryOriginalCharge charge2 = coll.AddNew();
			charge2.CY_Code = "BBB";
			charge2.CY_Amount = 20m;
			ReconEntryOriginalCharge charge3 = coll.AddNew();
			charge3.CY_Code = "CCC";
			charge3.CY_Amount = 40m;
			ReconEntryOriginalCharge charge4 = coll.AddNew();
			charge4.CY_Code = "DDD";
			charge4.CY_Amount = 80m;
			AssertEquals(30m, coll.GetTotal(new string[] { "AAA", "BBB" }));
			AssertEquals(40m, coll.GetTotal(new string[] { "CCC" }));
			AssertEquals(0m, coll.GetTotal(new string[] { "EEE" }));
		}

		public void TestGetCharge()
		{
			ReconEntryOriginalChargeCollection coll = new ReconEntryOriginalChargeCollection(Entry);
			ReconEntryOriginalCharge originalCharge = coll.AddNew();
			originalCharge.CY_Code = Core.Constants.USCustoms.FeeCodes.AntidumpingDuty;
			AssertNull(coll.GetCharge(Core.Constants.USCustoms.FeeCodes.Beef));
			AssertEquals(originalCharge, coll.GetCharge(Core.Constants.USCustoms.FeeCodes.AntidumpingDuty));
		}

		public void TestSetAmount()
		{
			ReconEntryOriginalChargeCollection coll = new ReconEntryOriginalChargeCollection(Entry);
			coll.SetAmount(Core.Constants.USCustoms.FeeCodes.AntidumpingDuty, 0m);
			AssertEquals("no element is created", 0, coll.Count);
			coll.SetAmount(Core.Constants.USCustoms.FeeCodes.AntidumpingDuty, 10m);
			AssertEquals("There should be one element created", 1, coll.Count);
			AssertEquals("ADD", 10m, coll[0].CY_Amount);
			coll.SetAmount(Core.Constants.USCustoms.FeeCodes.AntidumpingDuty, 0m);
			AssertEquals("There should be one element created", 1, coll.Count);
			AssertEquals("ADD", 0m, coll[0].CY_Amount);
		}

		public void TestTotalOriginalAmount()
		{
			ReconEntryOriginalChargeCollection coll = new ReconEntryOriginalChargeCollection(Entry);
			coll.SetAmount(Core.Constants.USCustoms.FeeCodes.Avocado, 1m);
			coll.SetAmount(Core.Constants.USCustoms.FeeCodes.Duty, 2m);
			AssertEquals(3m, coll.TotalOriginalAmount);
			coll.SetAmount(Core.Constants.USCustoms.FeeCodes.Honey, 4m);
			AssertEquals(7m, coll.TotalOriginalAmount);
			coll.SetAmount(Core.Constants.USCustoms.FeeCodes.MPC, 5m);
			AssertEquals(7m, coll.TotalOriginalAmount);
		}

		public void TestGetFirstFeeOtherThanTaxHMFOrMPF()
		{
			var coll = new ReconEntryOriginalChargeCollection(Entry);
			coll.SetAmount(Core.Constants.USCustoms.FeeCodes.HMF, 1m);
			coll.SetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 2m);
			coll.SetAmount(Core.Constants.USCustoms.FeeCodes.Wines, 5.6m);
			coll.SetAmount(Core.Constants.USCustoms.FeeCodes.Sugar, 4.1m);
			var fee = coll.GetFirstFeeOtherThanTaxHMFOrMPF();
			AssertEquals(Core.Constants.USCustoms.FeeCodes.Sugar, fee.CY_Code);
		}

		public void TestAllowNewAndRemove()
		{
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			var reconEntry = reconDeclaration.OriginalEntries.AddNew();
			reconEntry.Invoice.InvoiceLines.AddNew();
			reconEntry.US_R_NoLineDetails = true;
			Assert(reconEntry.OriginalCharges.AllowNew);
			Assert(reconEntry.OriginalCharges.AllowRemove);
			reconEntry.US_R_NoLineDetails = false;
			Assert(!reconEntry.OriginalCharges.AllowNew);
			Assert(!reconEntry.OriginalCharges.AllowRemove);
		}

		protected override Customs.Business.CusCodeDataCollection<ReconEntryOriginalCharge> GetCusCodeDataCollection() => new ReconEntryOriginalChargeCollection(Entry);

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = Factory.New<ReconEntryOriginalCharge>();
			result.Parent = Entry;
			return result;
		}

		CusEntryHeader entry;
		CusEntryHeader Entry
		{
			get
			{
				if (entry == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Recon;
					entry = declaration.CustomsEntryHeaders.AddNew();
					entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ReconOriginalEntry;
				}

				return entry;
			}
		}
	}
}
