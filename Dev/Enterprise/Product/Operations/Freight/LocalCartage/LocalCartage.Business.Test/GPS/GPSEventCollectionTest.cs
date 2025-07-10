using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.Business.GPS
{
	[TestedType(typeof(GPSEventCollection))]
	public class GPSEventCollectionTest : NonPersistentBusinessObjectCollectionTestCase<GPSEventCollection>
	{
		protected override GPSEventCollection GetCollectionToTest()
		{
			return new GPSEventCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new GPSEvent();
		}
	}
}
