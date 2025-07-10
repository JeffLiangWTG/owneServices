using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business
{
	public partial class InvoiceCharge : AutoInvoiceCharge
	{
		public InvoiceCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override ZBool AllowNonWesternEuropeanCharacterForChargeDescription => true;

		[ResourceStringData("Enterprise.Customs.TW.Business.InvoiceCharge|J7_Calc_IsIncludedInInvoiceAmount", Caption = "Included in Inv. Amt", FullDescription = "It indicates whether the charge is included in the invoice amount. The Incoterm and charge code determine whether the charge is included by default.")]
		public override ZBool J7_Calc_IsIncludedInInvoiceAmount { get => base.J7_Calc_IsIncludedInInvoiceAmount; set => base.J7_Calc_IsIncludedInInvoiceAmount = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.InvoiceCharge|J7_IsStatisticalValueApplicable", Caption = "Incl. in FOB", FullDescription = "It indicates whether the charge is included in the FOB price. The Incoterm and charge code determine whether the charge is included by default.")]
		public override ZBool J7_IsStatisticalValueApplicable { get => base.J7_IsStatisticalValueApplicable; set => base.J7_IsStatisticalValueApplicable = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.InvoiceCharge|J7_IsGSTApplicable", Caption = "Included in Total Disbursed Amount", ShortCaption = "Incl. in Total Disbursed Amt", FullDescription = "It indicates whether the charge is included in the invoice total amount. The Incoterm and charge code determine whether the charge is included by default.")]
		public override ZBool J7_IsGSTApplicable { get => base.J7_IsGSTApplicable; set => base.J7_IsGSTApplicable = value; }

		protected override bool ShouldResetDefaultIsIncludedInITOT(ZString incoTerm) => CommonHelper.ShouldDefaultIsIncludedInIvoiceLineToTrue(incoTerm, J7_ChargeType);

		protected override bool ShouldResetDefaultIsIncludedInAmount(ZString incoTerm, Common.ICustomsChargeCode charge) => CommonHelper.ShouldResetDefaultIsIncludedInAmount(incoTerm, charge?.Code);

		protected override ZBool GetDefaultIsIncludedInITOT(Common.ICustomsChargeCode customsChargeCode)
		{
			var result = ZBool.False;
			if (customsChargeCode.IsIncludedInITOTDeemedForThisCharge)
			{
				result = customsChargeCode.IsIncludedInITOTIfDeemed.HasValue && customsChargeCode.IsIncludedInITOTIfDeemed.Value;
			}
			else if (ShouldResetDefaultIsIncludedInITOT(IncoTerm))
			{
				result = IncoTermAndChargeFactory.GetDefaultIsIncludedInInvoice(IncoTerm, customsChargeCode);
			}
			return result;
		}

		protected override ZDecimal DefaultPercentageForOverseasInsurance()
		{
			OrgSupplierBuyerLink buyerLink = null;
			if (Invoice?.Buyer is OrgHeader buyer)
			{
				buyerLink = GetBuyerLinkByBuyer(buyer.PK);
			}
			else if (Invoice?.JobDeclaration?.ImporterDocumentaryAddress is JobDocAddress importerAddress && !importerAddress.E2_AddressOverride && importerAddress.Organisation is OrgHeader importer)
			{
				buyerLink = GetBuyerLinkByBuyer(importer.PK);
			}

			var insuranceUplift = buyerLink?.OL_InsuranceUplift ?? ZDecimal.Zero;
			return insuranceUplift == ZDecimal.Zero ? base.DefaultPercentageForOverseasInsurance() : insuranceUplift;
		}

		OrgSupplierBuyerLink GetBuyerLinkByBuyer(ZGuid buyerPk) => Invoice.Supplier?.BuyerLinks.Where(link => link.OL_OH_Buyer == buyerPk).FirstOrDefault();
	}
}
