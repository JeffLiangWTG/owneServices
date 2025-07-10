using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public sealed class AddInfoJobComInvoiceHeader : EU.Business.Declaration.AddInfoJobComInvoiceHeader
	{
		public AddInfoJobComInvoiceHeader(ZPropertyInfo addInfoPropertyInfo)
			: base(addInfoPropertyInfo)
		{
		}

		public new JobComInvoiceHeader Parent => (JobComInvoiceHeader)base.Parent;

		public new AddInfoJobComInvoiceHeaderValidation Validation => (AddInfoJobComInvoiceHeaderValidation)base.Validation;

		public new AddInfoJobComInvoiceHeaderLookups Lookups => (AddInfoJobComInvoiceHeaderLookups)base.Lookups;

		[MaxLength(2)]
		[List(nameof(Lookups) + "." + nameof(AddInfoJobComInvoiceHeaderLookups.InvoicePaymentCodeList))]
		public override ZString ZG_CommercialPaymentCode
		{
			get => base.ZG_CommercialPaymentCode;
			set
			{
				var oldValue = base.ZG_CommercialPaymentCode;
				base.ZG_CommercialPaymentCode = value;
				if (oldValue != value && !IsCopying)
				{
					Parent?.MarkAsNeedingValidation();
				}
			}
		}

		protected override EUAddInfoValidation GetNewValidation() => new AddInfoJobComInvoiceHeaderValidation(this);

		protected override EUAddInfoLookups GetNewLookups() => new AddInfoJobComInvoiceHeaderLookups(this);
	}
}
