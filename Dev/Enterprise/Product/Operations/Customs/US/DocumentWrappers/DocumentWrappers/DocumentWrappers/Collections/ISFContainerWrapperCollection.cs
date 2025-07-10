using CargoWise.EntityFramework;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.DocumentWrappers.GenericWrappers;

namespace Enterprise.Customs.US.DocumentWrappers
{
	public class ISFContainerWrapperCollection : ContainerWrapperCollection
	{
		public ISFContainerWrapperCollection(CusISFHeader headerBO, BusinessObjectFactory factory)
		: base(factory)
		{
			if (headerBO != null)
			{
				foreach (var containerBO in headerBO.Equipments)
				{
					Add(new ContainerWrapperFromCusISFEquip(containerBO, factory));
				}
			}
		}
	}
}
