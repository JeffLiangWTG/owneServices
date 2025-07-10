using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.ContainerYard.Business
{
	public class YardUnitMovementCollection : ActiveBusinessObjectCollection<YardUnitMovement>
	{
		public YardUnitMovementCollection(YardUnit parent)
			: base(parent.Factory, parent, null, YardUnitMovementSchema.GTM_GTY_YardUnit)
		{
		}
	}
}
