using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class AddInfoJobComInvoiceLine : EU.Business.Declaration.AddInfoJobComInvoiceLine
	{
		public AddInfoJobComInvoiceLine(ZPropertyInfo addInfoProperty) : base(addInfoProperty)
		{
		}

		public new AddInfoJobComInvoiceLineLookups Lookups => (AddInfoJobComInvoiceLineLookups)base.Lookups;

		public new AddInfoJobComInvoiceLineValidation Validation => (AddInfoJobComInvoiceLineValidation)base.Validation;

		protected override EUAddInfoLookups GetNewLookups() => new AddInfoJobComInvoiceLineLookups(this);

		protected override EUAddInfoValidation GetNewValidation() => new AddInfoJobComInvoiceLineValidation(this);

		public JobComInvoiceLine InvoiceLine => (JobComInvoiceLine)Parent;

		[List(nameof(Lookups) + "." + nameof(AddInfoJobComInvoiceLineLookups.UsedGoodsCodeList))]
		public override ZString ZG_UsedGoodsCode
		{
			get => base.ZG_UsedGoodsCode;
			set => base.ZG_UsedGoodsCode = value;
		}

		[List(nameof(Lookups) + "." + nameof(AddInfoJobComInvoiceLineLookups.ReturningGoodsReasonCodeList))]
		public override ZString ZG_ReturningGoodsReasonCode
		{
			get => base.ZG_ReturningGoodsReasonCode;
			set => base.ZG_ReturningGoodsReasonCode = value;
		}

		[List(nameof(Lookups) + "." + nameof(AddInfoJobComInvoiceLineLookups.ExportUnionPackageCodeList))]
		public override ZString ZG_ExportUnionPackCode
		{
			get => base.ZG_ExportUnionPackCode;
			set => base.ZG_ExportUnionPackCode = value;
		}

		[List(nameof(Lookups) + "." + nameof(AddInfoJobComInvoiceLineLookups.ThreadCodeList))]
		public override ZString ZG_ExportUnionThreadCode
		{
			get => base.ZG_ExportUnionThreadCode;
			set => base.ZG_ExportUnionThreadCode = value;
		}

		[List(nameof(Lookups) + "." + nameof(AddInfoJobComInvoiceLineLookups.ExportUnionAdditionalTariffList))]
		public override ZString ZG_ExportUnionAdditionalTariffCode
		{
			get => base.ZG_ExportUnionAdditionalTariffCode;
			set => base.ZG_ExportUnionAdditionalTariffCode = value;
		}

		[List(nameof(Lookups) + "." + nameof(AddInfoJobComInvoiceLineLookups.EntryExitPurposeCodeList))]
		public override ZString ZG_EntryExitPurposeCode
		{
			get => base.ZG_EntryExitPurposeCode;
			set => base.ZG_EntryExitPurposeCode = value;
		}

		[List(nameof(Lookups) + "." + nameof(AddInfoJobComInvoiceLineLookups.InvoicePaymentCodeList))]
		[MaxLength(2)]
		public override ZString ZG_CommercialPaymentCode
		{
			get => base.ZG_CommercialPaymentCode;
			set => base.ZG_CommercialPaymentCode = value;
		}

		[List(nameof(Lookups) + "." + nameof(AddInfoJobComInvoiceLineLookups.PriceTypeList))]
		public override ZString ZG_PriceType
		{
			get => base.ZG_PriceType;
			set => base.ZG_PriceType = value;
		}
	}
}
