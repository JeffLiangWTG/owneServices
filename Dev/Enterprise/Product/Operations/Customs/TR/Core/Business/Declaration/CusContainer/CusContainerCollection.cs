using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class CusContainerCollection : BaseCusContainerCollection<CusContainer>
	{
		public CusContainerCollection(JobDeclaration jobDeclaration, BusinessObjectFactory factory)
			: base(jobDeclaration, factory)
		{
		}
	}
}
