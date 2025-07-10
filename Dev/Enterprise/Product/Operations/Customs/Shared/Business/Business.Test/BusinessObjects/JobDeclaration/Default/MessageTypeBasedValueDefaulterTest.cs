using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class MessageTypeBasedValueDefaulterTest : TestCaseWithFactory
	{
		public void TestDefaultAll()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.DisableMessageTypeChangeOnSupplierChangeForTesting = true;
			var importer = OrgHeader.New(Factory);
			importer.OH_FullName = "Importer";
			importer.OH_RL_NKClosestPort = "Z!XXX";
			importer.MiscServ.OM_IMMergeCustomsInvoiceLinesBy = OrgConstants.MergeInvoiceLines.TariffAndDescription;

			var supplier = OrgHeader.New(Factory);
			supplier.OH_FullName = "Supplier";
			supplier.OH_RL_NKClosestPort = "V!XXX";
			supplier.MiscServ.OM_IMMergeCustomsInvoiceLinesBy = OrgConstants.MergeInvoiceLines.PartNumber;

			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_OH_Importer = importer.PK;
			var defaulter = new MessageTypeBasedValueDefaulter(declaration);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = ZString.Empty;
			defaulter.DefaultAll();
			AssertEquals("declaration.JE_MergeBy", OrgConstants.MergeInvoiceLines.TariffAndDescription, declaration.JE_MergeBy);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_MergeBy = ZString.Empty;
			defaulter.DefaultAll();
			AssertEquals("declaration.JE_MergeBy", OrgConstants.MergeInvoiceLines.PartNumber, declaration.JE_MergeBy);
		}
	}
}
