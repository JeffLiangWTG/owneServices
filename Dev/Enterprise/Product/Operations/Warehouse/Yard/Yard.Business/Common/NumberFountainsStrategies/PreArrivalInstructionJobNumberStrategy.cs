using CargoWise.EntityFramework;
using Enterprise.Environment;

namespace Enterprise.Warehouse.Yard.Business
{
	public class PreArrivalInstructionJobNumberStrategy : BaseContainerYardNumberFountainsStrategy
	{
		public PreArrivalInstructionJobNumberStrategy(BusinessObjectFactory factory)
			: base(factory, Env.Instance.NumberFountains.CYDReceiveAdviceJobNumber) { }

		public string GetJobNumber()
		{
			return GetNumber();
		}
	}
}
