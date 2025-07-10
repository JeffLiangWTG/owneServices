using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class JobComInvoiceLineLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestInvoiceLine()
		{
			JobComInvoiceLine parent = Factory.New<JobComInvoiceLine>();
			AssertEquals(parent.Lookups.InvoiceLine, parent);
		}

		public void TestHazardousMaterialCodeQualifierList()
		{
			AssertNotNull(Lookups.HazardousMaterialCodeQualifierList);
			Assert(Lookups.HazardousMaterialCodeQualifierList.ContainsCode(DGIndicatorCodeList.Codes.N));
		}

		public void TestCustomsUQList()
		{
			Assert(Lookups.CustomsUQList is UnitOfQuantityCodeList);
		}

		public void TestInvoiceUQList()
		{
			Assert(Lookups.InvoiceUQList is UnitOfQuantityCodeList);
		}

		public void TestDutiableUQList()
		{
			Assert(Lookups.DutiableUQList is DutiableUQList);
		}

		public void TestOriginCriterionCodeList()
		{
			AssertNotNull(Lookups.OriginCriterionCodeList);
			Assert(Lookups.OriginCriterionCodeList.ContainsCode(OriginCriterionCodeList.Codes.MISC_ACC));
			Assert(Lookups.OriginCriterionCodeList.ContainsCode(OriginCriterionCodeList.Codes.AANZFTAFormAANZ_WO));
			Assert(Lookups.OriginCriterionCodeList.ContainsCode(OriginCriterionCodeList.Codes.AIFTAFormAI_WO));
			Assert(Lookups.OriginCriterionCodeList.ContainsCode(OriginCriterionCodeList.Codes.AJCEPFormAJ_WO));
			Assert(Lookups.OriginCriterionCodeList.ContainsCode(OriginCriterionCodeList.Codes.AKFTAFormAK_WO));
			Assert(Lookups.OriginCriterionCodeList.ContainsCode(OriginCriterionCodeList.Codes.AANZFTAFormAANZ_CTC));
			Assert(Lookups.OriginCriterionCodeList.ContainsCode(OriginCriterionCodeList.Codes.AANZFTAFormAANZ_RVC_CC));
			Assert(Lookups.OriginCriterionCodeList.ContainsCode(OriginCriterionCodeList.Codes.AANZFTAFormAANZ_RVC_CTH));
			Assert(Lookups.OriginCriterionCodeList.ContainsCode(OriginCriterionCodeList.Codes.AANZFTAFormAANZ_RVC_CTSH));
			Assert(Lookups.OriginCriterionCodeList.ContainsCode(OriginCriterionCodeList.Codes.AANZFTAFormAANZ_OTHER));
		}

		public void TestPreferenceListCurrentBaseValues()
		{
			var preferentialIndicatorCodeList = new PreferentialIndicatorCodeList();
			Assert(preferentialIndicatorCodeList.ContainsCode(PreferentialIndicatorCodeList.Codes.STD));
			Assert(preferentialIndicatorCodeList.ContainsCode(PreferentialIndicatorCodeList.Codes.PRF));
			Assert(preferentialIndicatorCodeList.ContainsCode(PreferentialIndicatorCodeList.Codes.PRI));
		}

		public void TestPreferenceCodeListBuiltIsBespoke()
		{
			var exportDec = Factory.New<JobDeclaration>();
			exportDec.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			var invoice = exportDec.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			Assert(invoiceLine.Lookups.PreferenceList.ContainsCode(PreferentialIndicatorCodeList.Codes.STD));
			Assert(invoiceLine.Lookups.PreferenceList.ContainsCode(PreferentialIndicatorCodeList.Codes.PRI));
			AssertEquals("PRF is not a valid code for Export", false, invoiceLine.Lookups.PreferenceList.ContainsCode(PreferentialIndicatorCodeList.Codes.PRF));
			var importDec = Factory.New<JobDeclaration>();
			importDec.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			invoice = importDec.Invoices.AddNew();
			invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			Assert(invoiceLine.Lookups.PreferenceList.ContainsCode(PreferentialIndicatorCodeList.Codes.STD));
			Assert(invoiceLine.Lookups.PreferenceList.ContainsCode(PreferentialIndicatorCodeList.Codes.PRF));
			AssertEquals("PRI is not a valid code for Import", false, invoiceLine.Lookups.PreferenceList.ContainsCode(PreferentialIndicatorCodeList.Codes.PRI));
		}

		JobComInvoiceLineLookups Lookups
		{
			get
			{
				return InvoiceLine.Lookups;
			}
		}

		#region InvoiceLine
		JobComInvoiceLine InvoiceLine
		{
			get
			{
				return invoiceLine ?? (invoiceLine = Factory.New<JobComInvoiceLine>());
			}
		}

		JobComInvoiceLine invoiceLine;
		#endregion
	}
}
