using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration.Customs.US;

namespace Enterprise.Customs.US.Business
{
	public class ReconDeclarationLoadStrategy : IReconDeclarationLoadStrategy
	{
		public BusinessObject Load(BusinessObjectFactory factory, ZGuid pk)
		{
			return ReconDeclaration.Get(factory.Load<JobDeclaration>(pk));
		}
	}
}
