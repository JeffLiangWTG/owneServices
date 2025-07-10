//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class CusEntryHeaderChargesLookups : Customs.Business.CusEntryHeaderChargesLookups
	{
		public CusEntryHeaderChargesLookups(CusEntryHeaderCharges parent)
			: base(parent)
		{
		}

		public CusEntryHeaderCharges EntryHeaderCharges
		{
			get { return Parent; }
		}

		protected new CusEntryHeaderCharges Parent
		{
			get { return (CusEntryHeaderCharges)base.Parent; }
		}

		public CodeDescriptionPairList C1_ChargeTypeList => CusFeeCodeConstants.GetReconEntryHeaderFeeChargeCodeList(Factory);
	}
}
