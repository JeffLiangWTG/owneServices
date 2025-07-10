using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public interface ISailingSynchronisationTarget<TSailingSynchronisationSource> where TSailingSynchronisationSource : BusinessObject
	{
		TSailingSynchronisationSource Source { get; }
		bool IsMatched(TSailingSynchronisationSource sailingTarget);
		void Set(TSailingSynchronisationSource sailingTarget);
		void Synchronise();
	}
}
