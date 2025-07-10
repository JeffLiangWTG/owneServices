using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business.Testing
{
	partial class ExportJobComInvoiceLineValidationTest
	{
		public void TestCheckJI_BondedGoodsCode()
		{
			InvoiceLine.JI_BondedGoodsCode = "XX";
			AssertHasMessageErrorContaining(InvoiceLine.JI_BondedGoodsCodeInfo, ListValidation.InvalidCodeMessageError);
			InvoiceLine.JI_BondedGoodsCode = BondedGoodsCodeList.Codes.NB;
			AssertNoMessageErrorContaining(InvoiceLine.JI_BondedGoodsCodeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckJI_BondedGoodsCodesBasedOnDeclarationType()
		{
			string messageError = "Bonded Goods Code cannot be empty when the Declaration Type is 'B9'.";
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var jobDeclaration = Factory.New<JobDeclaration>();
				jobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
				var entryInstruction = jobDeclaration.CusEntryInstruction;
				entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.B1;
				var invoiceLine = (JobComInvoiceLine)jobDeclaration.Invoices.AddNew().InvoiceLines.AddNew();
				invoiceLine.JI_CEI = entryInstruction.PK;
				invoiceLine.JI_BondedGoodsCode = ZString.Empty;
				AssertNoMessageError(invoiceLine.JI_BondedGoodsCodeInfo, messageError);
				entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.B9;
				invoiceLine.JI_BondedGoodsCode = ZString.Empty;
				AssertHasMessageError(invoiceLine.JI_BondedGoodsCodeInfo, messageError);
				invoiceLine.JI_BondedGoodsCode = BondedGoodsCodeList.Codes.CN;
				AssertNoMessageError(invoiceLine.JI_BondedGoodsCodeInfo, messageError);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		}
	}
}
