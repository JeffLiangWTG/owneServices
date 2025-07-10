using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USCDataVersionCollection))]
	public class USCDataVersionCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new USCDataVersionCollection(Factory);
		}

		public void TestDefaultCollection()
		{
			var dataversion = Factory.Load<USCDataVersion>(new ZQuery(USCDataVersionSchema.UZ_Name, "ScheduleBDataVersion"));
			if (dataversion == null)
			{
				var newversion = Factory.New<USCDataVersion>();
				newversion.UZ_Name = "ScheduleBDataVersion";
				Factory.Save();
			}

			BusinessObjectCollection collection = this.GetCollectionToTest();
			AssertEquals("Count", 0, collection.Count);
		}
	}
}
