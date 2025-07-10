using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public interface ISailingSynchronisationTargetCollection<TSailingSynchronisationSource, TSailingSynchronisationTarget> : IEnumerable<TSailingSynchronisationTarget>
		where TSailingSynchronisationSource : BusinessObject
		where TSailingSynchronisationTarget : ISailingSynchronisationTarget<TSailingSynchronisationSource>
	{
		TSailingSynchronisationTarget AddNew();
		void Delete(TSailingSynchronisationTarget target);
	}
}
