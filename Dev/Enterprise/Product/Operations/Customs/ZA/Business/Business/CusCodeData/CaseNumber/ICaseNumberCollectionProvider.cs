using CargoWise.EntityFramework;

namespace Enterprise.Customs.ZA.Business
{
	public interface ICaseNumberCollectionProvider
	{
		CaseNumberCollection CaseNumbers { get; }
		BusinessObject Master { get; }
	}
}
