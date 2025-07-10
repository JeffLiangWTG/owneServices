using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.PL.Business.Declaration;

public class ExportJobComInvoiceHeaderValidation : JobComInvoiceHeaderValidation
{
	public ExportJobComInvoiceHeaderValidation(JobComInvoiceHeader invoiceHeader) : base(invoiceHeader)
	{
	}

	protected override void CheckJZ_OH_Supplier()
	{
		base.CheckJZ_OH_Supplier();

		var supplierCustomsCodes = Parent.Supplier?.CustomsCodes;
		PLOrgHeaderValidationHelper.ValidateHasCustomsRegNo(supplierCustomsCodes, OrgCusCode.PolandCodeTypes.TIN, CountryCodes.Poland, Parent.JZ_OH_SupplierInfo);
		PLOrgHeaderValidationHelper.ValidateHasCustomsRegNo(supplierCustomsCodes, OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, ZString.Empty, Parent.JZ_OH_SupplierInfo);
	}
}
