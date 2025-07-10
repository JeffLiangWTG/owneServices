using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.Business
{
	public class HouseBillRefNoLookups : Customs.Business.CusCodeDataLookups
	{
		public HouseBillRefNoLookups(HouseBillRefNo parent)
			: base(parent)
		{
		}

		public override ZArchitecture.Core.CodeDescriptionPairList CY_CodeList
		{
			get { return new ReferenceQualifierList(); }
		}
	}
}
