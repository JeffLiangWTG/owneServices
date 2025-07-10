using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ZA.Business
{
	public partial class CusEntryHeaderLookups : Customs.Business.CusEntryHeaderLookups
	{
		public CusEntryHeaderLookups(CusEntryHeader entryHeader)
			: base(entryHeader)
		{
		}

		public CusEntryHeader EntryHeader
		{
			get { return (CusEntryHeader)Parent; }
		}

		public CodeDescriptionPairList PaymentMethodCodeList => CH_PaymentMethodCodeList;

		public PaymentMethodCodeList CH_PaymentMethodCodeList => Factory.GetCachedValue<PaymentMethodCodeList>();

		public CodeDescriptionPairList WeightUQList => Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight);
	}
}
