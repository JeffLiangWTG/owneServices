
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business
{
	public class ActiveServiceLevelProvider : IActiveServiceLevelProvider
	{
		public RegistryServiceLevelCollection GetActiveServiceLevels(BusinessObjectFactory factory)
		{
			RegistryServiceLevelCollection result = new RegistryServiceLevelCollection();

			ActiveServiceLevelCollection levels = new ActiveServiceLevelCollection(factory);
			int count = levels.Count;

			foreach (RefServiceLevel level in levels)
			{
				result.Add(new RegistryServiceLevel(level.PK, level.RS_Code, level.RS_DescriptionMultilingual, true));
			}

			return result;
		}
	}
}
