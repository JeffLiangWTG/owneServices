using CargoWise.EntityFramework;
using Enterprise.Integration;

namespace Enterprise.MasterFiles.Business.ServiceTasks
{
	public abstract class DataConsistencyChecker
	{
		public abstract void Check(ILogger serviceLogger);
		public abstract string Description { get; }

		protected BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory? factory;
	}
}
