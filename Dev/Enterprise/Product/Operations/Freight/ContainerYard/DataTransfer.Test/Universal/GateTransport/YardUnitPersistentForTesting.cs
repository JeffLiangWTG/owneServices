using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Freight.ContainerYard.Business.Testing
{
	sealed class YardUnitPersistentForTesting : YardUnit
	{
		public YardUnitPersistentForTesting(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
		{
		}

		public override bool IsSavedByFactory => true;
	}
}
