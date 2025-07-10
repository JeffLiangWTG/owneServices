using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.DataTransfer.Universal.SeaManifest.Testing
{
	sealed class CusSCAContainerForTest : BaseCusSCAContainer
	{
		public CusSCAContainerForTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
