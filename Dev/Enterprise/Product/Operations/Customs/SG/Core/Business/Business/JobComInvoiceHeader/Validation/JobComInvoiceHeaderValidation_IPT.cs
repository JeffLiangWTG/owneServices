namespace Enterprise.Customs.SG.V4.Business
{
	public class JobComInvoiceHeaderValidation_IPT : JobComInvoiceHeaderValidation_Inward
	{
		public JobComInvoiceHeaderValidation_IPT(JobComInvoiceHeader invoiceHeader)
			: base(invoiceHeader)
		{
		}

		protected override void CheckJZ_OH_Supplier()
		{
			base.CheckJZ_OH_Supplier();

			if (IsSupplierRequired && Parent.JZ_OH_Supplier.IsEmpty)
			{
				Parent.JZ_OH_SupplierInfo.AddMessageError(SupplierManufactuerRequired);
			}
		}
		internal const string SupplierManufactuerRequired = "Supplier/Manufacturer has to be specified for an IPT declaration, except for the following:\r\ni) short payment where Place of Receipt = SPSTK, SPNOSTK\r\nii) declaration type = BKT (Blanket)\r\niii) recovery payment where Place of Receipt = RCNOSTK\r\niv) payment for goods previously exempted from duties/taxes (e.g. Place of Release = EM, EXEMPT)\r\nv) supplementary declaration for schemes under AISS, IGDS such as Place of Receipt = AISSLOC, SPIGDS";

		protected override bool IsSupplierRequired
		{
			get { return Parent.JobDeclaration.JE_MessageSubType != DeclarationTypeCodeList.Codes.BKT && !SupplierExemptPlaceOfReceipt && !IsSupplierExemptPlaceOfReleaseWhenPaymentForGoodsPreviouslyExempted; }
		}

		bool SupplierExemptPlaceOfReceipt => Parent.JobDeclaration.PlaceOfReceipt.SupplierExemptPlaceOfReceipt();

		public bool IsSupplierExemptPlaceOfReleaseWhenPaymentForGoodsPreviouslyExempted
		{
			get
			{
				var result = false;
				var placeOfRelease = Parent.JobDeclaration.PlaceOfRelease;
				if (placeOfRelease != null && Parent.JobDeclaration.SG_GoodsPreviouslyExemptedFromDuties)
				{
					result = placeOfRelease.IsSupplierExemptPlaceOfRelease();
				}
				return result;
			}
		}

		protected override void CheckJZ_IncoTerm()
		{
			base.CheckJZ_IncoTerm();

			if (Parent.JZ_IncoTerm == UnitPriceTermTypeCodeList.Codes.FOB)
			{
				if (Parent.OverseasFreight.Amount == 0)
				{
					Parent.JZ_IncoTermInfo.AddMessageError("Freight charge amount is required for this Declaration Type/Incoterm.");
				}
			}
		}
	}
}
