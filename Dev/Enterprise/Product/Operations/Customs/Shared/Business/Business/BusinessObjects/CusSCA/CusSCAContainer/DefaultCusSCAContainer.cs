using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	internal class DefaultCusSCAContainer : BaseCusSCAContainer
	{
		public DefaultCusSCAContainer(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
