using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;

namespace Enterprise.Freight.CFS.Business.Testing
{
	public class PackUnpackLoadListConsolTest : CommonConsolTest2
	{
		protected override CommonConsol GetNewConsol()
		{
			return Factory.New<PackUnpackLoadListConsol>();
		}
	}
}
