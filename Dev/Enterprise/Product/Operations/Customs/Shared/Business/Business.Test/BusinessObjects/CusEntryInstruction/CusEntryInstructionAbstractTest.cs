using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestsSubclassesOf(typeof(CusEntryInstruction))]
	public abstract class CusEntryInstructionAbstractTest : EnterpriseBusinessObjectTestCase
	{
		public virtual void TestIsChangeOfOwnershipWarehousing()
		{
			var owner = Factory.NewWithValidTestData<OrgHeader>();
			owner.OH_Code = "AAAA";

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryInstruction = Factory.New<CusEntryInstruction>();
			entryInstruction.CEI_JE = declaration.PK;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;

			invoiceLine1.JI_Procedure = GetInvoiceProcedure();

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;

			var helper = new WhsDataTestHelper(Factory);
			entryInstruction.CEI_OA_Warehouse = helper.Warehouse.MainAddress.PK;

			AssertEquals("Prerequisite: IsChangeOfOwnershipWarehousingEnabled should be false.", false, entryInstruction.IsChangeOfOwnershipWarehousingEnabled);
			AssertEquals("Prerequisite: there should not be any invoice line with IsChangeOfOwnershipWarehousing.", false, entryInstruction.InvoiceLines.Any(x => x.IsChangeOfOwnershipWarehousing));
			AssertEquals("IsChangeOfOwnershipWarehousing should be false because IsChangeOfOwnershipWarehousingEnabled is false and no invoice line has IsChangeOfOwnershipWarehousing set to true.", false, entryInstruction.IsChangeOfOwnershipWarehousing);

			entryInstruction.CEI_OH_Owner = ZGuid.BrettsGuid;
			AssertEquals("Prerequisite: IsChangeOfOwnershipWarehousingEnabled should be true.", true, entryInstruction.IsChangeOfOwnershipWarehousingEnabled);
			AssertEquals("Prerequisite: there should not be any invoice line with IsChangeOfOwnershipWarehousing.", false, entryInstruction.InvoiceLines.Any(x => x.IsChangeOfOwnershipWarehousing));
			AssertEquals("IsChangeOfOwnershipWarehousing should be false because no invoice line has IsChangeOfOwnershipWarehousing set to true.", false, entryInstruction.IsChangeOfOwnershipWarehousing);

			entryInstruction.CEI_OH_Owner = ZGuid.Invalid;
			helper.Warehouse.CompanyData.OB_IMUsedBondedWhs = true;
			AssertEquals("Prerequisite: IsChangeOfOwnershipWarehousingEnabled should be false.", false, entryInstruction.IsChangeOfOwnershipWarehousingEnabled);
			AssertEquals("Prerequisite: there should be at least one invoice line with IsChangeOfOwnershipWarehousing.", true, entryInstruction.InvoiceLines.Any(x => x.IsChangeOfOwnershipWarehousing));
			AssertEquals("IsChangeOfOwnershipWarehousing should be false because IsChangeOfOwnershipWarehousingEnabled is false.", false, entryInstruction.IsChangeOfOwnershipWarehousing);

			entryInstruction.CEI_OH_Owner = ZGuid.BrettsGuid;
			AssertEquals("Prerequisite: IsChangeOfOwnershipWarehousingEnabled should be true.", true, entryInstruction.IsChangeOfOwnershipWarehousingEnabled);
			AssertEquals("Prerequisite: there should be at least one invoice line with IsChangeOfOwnershipWarehousing.", true, entryInstruction.InvoiceLines.Any(x => x.IsChangeOfOwnershipWarehousing));
			AssertEquals("IsChangeOfOwnershipWarehousing should be true because IsChangeOfOwnershipWarehousing is true and at least one invoice line has IsChangeOfOwnershipWarehousing set to true.", true, entryInstruction.IsChangeOfOwnershipWarehousing);
		}

		protected virtual RefCusProcedure CreateRefCusProcedure()
		{
			var procedure = Factory.New<RefCusProcedure>();
			procedure.ZZ6_ProcedureCode = "40";
			procedure.ZZ6_PreviousProcedureCode = "00";
			procedure.ZZ6_Concession = "000";
			procedure.ZZ6_IntoWarehouse = WarehouseMoveStatus.Codes.Yes;
			procedure.ZZ6_OutOfWarehouse = WarehouseMoveStatus.Codes.Yes;
			procedure.ZZ6_ZZZ_NKDataGrouping = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			procedure.ZZ6_Description = "DESCRIPTION";
			procedure.ZZ6_ShipmentType = "IMP";
			return procedure;
		}

		protected ZString GetInvoiceProcedure()
		{
			var procedure = CreateRefCusProcedure();
			return procedure.ZZ6_ProcedureCode + procedure.ZZ6_PreviousProcedureCode + procedure.ZZ6_Concession;
		}
	}
}
