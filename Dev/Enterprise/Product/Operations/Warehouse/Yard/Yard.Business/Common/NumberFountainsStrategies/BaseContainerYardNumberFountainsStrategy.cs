using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Warehouse.Yard.Business
{
	public class BaseContainerYardNumberFountainsStrategy
	{
		readonly BusinessObjectFactory Factory;
		string JobNumber;
		readonly INumberFountainProxy NumberFountain;

		public BaseContainerYardNumberFountainsStrategy(BusinessObjectFactory factory, INumberFountainProxy numberFountain)
		{
			Factory = factory;
			NumberFountain = numberFountain;
		}

		protected string GetNumber()
		{
			if (JobNumber == null)
			{
				using (var manager = ((IDbConnected)Factory).Connection.BeginTransactionWithManager())
				{
					JobNumber = NumberFountain.GetNextFormatted(Factory);
					manager.CommitTransaction();
				}
			}
			return JobNumber;
		}
	}
}
