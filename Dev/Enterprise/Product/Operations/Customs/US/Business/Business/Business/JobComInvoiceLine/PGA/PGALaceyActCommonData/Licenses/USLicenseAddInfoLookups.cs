namespace Enterprise.Customs.US.Business
{
	public class USLicenseAddInfoLookups : AutoUSLicenseAddInfoLookups
	{
		public USLicenseAddInfoLookups(AutoUSLicenseAddInfo parent) : base(parent)
		{
		}

		public LPCOTransactionTypeList LPCOTransactionTypeList
		{
			get { return Factory.GetCachedValue<LPCOTransactionTypeList>(); }
		}

		public LPCODateQualifierList DateQualifierList
		{
			get { return Factory.GetCachedValue<LPCODateQualifierList>(); }
		}

		public LaceyActLPCOTypeList LPCOTypeList
		{
			get { return Factory.GetCachedValue<LaceyActLPCOTypeList>(); }
		}
	}
}
