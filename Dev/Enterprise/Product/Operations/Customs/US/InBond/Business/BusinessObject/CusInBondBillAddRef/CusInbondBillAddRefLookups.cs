using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.InBond.Business
{
	public class CusInbondBillAddRefLookups : Customs.Business.CusInbondBillAddRefLookups
	{
		public CusInbondBillAddRefLookups(CusInbondBillAddRef parent)
			: base(parent)
		{
		}

		public ReferenceQualifierList AdditionalReferenceList
		{
			get { return Factory.GetCachedValue<ReferenceQualifierList>(); }
		}
	}
}
