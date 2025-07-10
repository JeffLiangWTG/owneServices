namespace Enterprise.Customs.US.AMS.Business
{
	public class IssuerAndBillOfLadingLookups : Customs.Business.CusCodeDataLookups
	{
		public IssuerAndBillOfLadingLookups(IssuerAndBillOfLading parent)
			: base(parent)
		{
		}

		public override ZArchitecture.Core.CodeDescriptionPairList CY_CodeList
		{
			get { return Factory.GetCachedValue<IssuerAndBillOfLadingStatusList>(); }
		}
	}
}
