using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class StmFeatureTestCollection : BusinessObjectCollection<StmFeatureTest>
	{
		public StmFeatureTestCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
