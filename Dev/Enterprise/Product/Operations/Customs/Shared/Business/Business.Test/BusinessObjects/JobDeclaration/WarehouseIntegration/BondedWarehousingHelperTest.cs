using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BondedWarehousingHelperTest : WhsDataTestHelper
	{
		public void TestHasBondedWarehouseEntryDetails()
		{
			var declaration = Factory.New<BaseJobDeclarationWithEntryInstructions>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var helper = new BondedWarehousingHelper(declaration);
			AssertEquals(false, helper.HasBondedWarehouseEntryDetails(invoiceLine, false, false, false));
			AssertEquals(false, helper.HasBondedWarehouseEntryDetails(invoiceLine, true, false, false));
			AssertEquals(false, helper.HasBondedWarehouseEntryDetails(invoiceLine, false, true, false));
			AssertEquals(false, helper.HasBondedWarehouseEntryDetails(invoiceLine, false, false, true));
			var entryLine = Factory.New<CusEntryLine>();
			invoiceLine.JI_CL = entryLine.PK;
			AssertEquals(false, helper.HasBondedWarehouseEntryDetails(invoiceLine, true, false, false));
			AssertEquals(false, helper.HasBondedWarehouseEntryDetails(invoiceLine, false, true, false));
			AssertEquals(false, helper.HasBondedWarehouseEntryDetails(invoiceLine, false, false, true));
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.MergedLines.Add(entryLine);
			AssertEquals(true, helper.HasBondedWarehouseEntryDetails(invoiceLine, true, false, false));
			AssertEquals(false, helper.HasBondedWarehouseEntryDetails(invoiceLine, false, true, false));
			AssertEquals(false, helper.HasBondedWarehouseEntryDetails(invoiceLine, false, false, true));
			invoiceLine.JI_PreviousEntryNumber = "ENST";
			AssertEquals(true, helper.HasBondedWarehouseEntryDetails(invoiceLine, true, false, false));
			AssertEquals(true, helper.HasBondedWarehouseEntryDetails(invoiceLine, false, true, false));
			AssertEquals(true, helper.HasBondedWarehouseEntryDetails(invoiceLine, false, false, true));
			invoiceLine.JI_CL = ZGuid.Empty;
			invoiceLine.ComponentInventoryCollection.AddNew();
			AssertEquals(false, helper.HasBondedWarehouseEntryDetails(invoiceLine, true, false, false));
			AssertEquals(true, helper.HasBondedWarehouseEntryDetails(invoiceLine, false, true, false));
			AssertEquals(false, helper.HasBondedWarehouseEntryDetails(invoiceLine, false, false, true));
			invoiceLine.JI_PreviousEntryNumber = "";
			AssertEquals(true, helper.HasBondedWarehouseEntryDetails(invoiceLine, true, false, false));
			AssertEquals(true, helper.HasBondedWarehouseEntryDetails(invoiceLine, false, true, false));
			AssertEquals(true, helper.HasBondedWarehouseEntryDetails(invoiceLine, false, false, true));
		}

		public void TestIsMarkedForBondedWarehousing()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(GlbCompany.CurrentCompany.Country.Code, ZString.Empty, "AA", "BB", ZString.Empty, "OP DESC1", "EXW", outOfWarehouse: true);
			var procedure2 = helper.CreateRefCusProcedure(GlbCompany.CurrentCompany.Country.Code, ZString.Empty, "CC", "DD", ZString.Empty, "OP DESC2", "EXW");
			procedure2.ZZ6_OutOfInwardProcessing = "Y";
			var procedure3 = helper.CreateRefCusProcedure(GlbCompany.CurrentCompany.Country.Code, ZString.Empty, "EE", "FF", ZString.Empty, "OP DESC3", "EXW");
			procedure3.ZZ6_OutofOutwardProcessing = "Y";
			helper.CreateRefCusProcedure(GlbCompany.CurrentCompany.Country.Code, ZString.Empty, "GG", "HH", ZString.Empty, "OP DESC4", "IMP", intoWarehouse: true);
			var procedure5 = helper.CreateRefCusProcedure(GlbCompany.CurrentCompany.Country.Code, ZString.Empty, "II", "JJ", ZString.Empty, "OP DESC5", "IMP");
			procedure5.ZZ6_IntoInwardProcessing = "Y";
			var procedure6 = helper.CreateRefCusProcedure(GlbCompany.CurrentCompany.Country.Code, ZString.Empty, "KK", "LL", ZString.Empty, "OP DESC6", "IMP");
			procedure6.ZZ6_IntoOutwardProcessing = "Y";
			helper.CreateRefCusProcedure(GlbCompany.CurrentCompany.Country.Code, ZString.Empty, "MM", "NN", ZString.Empty, "OP DESC4", "EXW");
			Factory.Save();

			var declaration = Factory.New<BaseJobDeclarationWithEntryInstructions>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = "AB";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_Procedure = "AABB";
			var whshelper = new BondedWarehousingHelper(declaration);
			AssertEquals(true, whshelper.IsMarkedForBondedWarehousing(invoiceLine));

			invoiceLine.JI_Procedure = "CCDD";
			AssertEquals(true, whshelper.IsMarkedForBondedWarehousing(invoiceLine));

			invoiceLine.JI_Procedure = "EEFF";
			AssertEquals(true, whshelper.IsMarkedForBondedWarehousing(invoiceLine));

			invoiceLine.JI_Procedure = "GGHH";
			AssertEquals(true, whshelper.IsMarkedForBondedWarehousing(invoiceLine));

			invoiceLine.JI_Procedure = "IIJJ";
			AssertEquals(true, whshelper.IsMarkedForBondedWarehousing(invoiceLine));

			invoiceLine.JI_Procedure = "MMNN";
			AssertEquals(false, whshelper.IsMarkedForBondedWarehousing(invoiceLine));

			invoiceLine.JI_Procedure = "AB11";
			AssertEquals(false, whshelper.IsMarkedForBondedWarehousing(invoiceLine));
			entryInstruction.CEI_Style = "AC";
			invoiceLine.JI_Procedure = "AC10";
			AssertEquals(false, whshelper.IsMarkedForBondedWarehousing(invoiceLine));
			entryInstruction.CEI_Style = "AB";
			invoiceLine.JI_Procedure = "AB10";
		}
	}
}
