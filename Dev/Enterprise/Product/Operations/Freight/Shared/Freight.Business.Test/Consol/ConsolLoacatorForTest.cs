using CargoWise.EntityFramework;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ConsolLoacatorForTest : ConsolLocator<CommonConsol>
	{
		public new CommonConsol[] GetConsols(BusinessObjectFactory factory, ZQuery filter)
		{
			return base.GetConsols(factory, filter);
		}
	}
}
