using Enterprise.Customs.Business;

namespace Enterprise.Customs.ZA.Business
{
	public class CaseNumberCollection : CusCodeDataCollection<CaseNumber>
	{
		public CaseNumberCollection(ICaseNumberCollectionProvider provider)
			: base(provider.Master, CusCodeDataTypeList.Codes.CaseNumber)
		{
		}
	}
}
