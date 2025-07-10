using CargoWise.EntityFramework;

namespace Enterprise.Freight.Business
{
	public class ContainerNonDependentCollection : BusinessObjectCollection<CommonContainer>
	{
		public ContainerNonDependentCollection(BusinessObjectFactory factory) : base(factory)
		{
		}
	}
}
