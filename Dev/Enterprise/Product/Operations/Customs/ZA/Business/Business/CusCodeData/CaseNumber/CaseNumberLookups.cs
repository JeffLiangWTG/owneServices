using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ZA.Business
{
	public class CaseNumberLookups : CusCodeDataLookups
	{
		public CaseNumberLookups(AutoCusCodeData parent)
			: base(parent)
		{
		}

		public override CodeDescriptionPairList CY_CodeList
		{
			get { return Factory.GetCachedValue<CaseNumberTypeList>(); }
		}

		public CodeDescriptionPairList DocumentStatusCodeList
		{
			get { return Factory.GetCachedValue<DocumentStatusCodes>(); }
		}
	}
}
