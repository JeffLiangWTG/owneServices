using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.GPS.Business.Testing
{
	[TestedType(typeof(GPSSupporter))]
	sealed class GPSSupporterTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new GPSSupporter(Factory.New<RefEquipment>());
		}
	}
}
