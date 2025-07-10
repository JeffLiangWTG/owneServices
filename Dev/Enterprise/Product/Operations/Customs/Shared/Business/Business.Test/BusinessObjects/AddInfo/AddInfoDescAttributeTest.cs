using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class AddInfoDescAttributeTest : TestCase
	{
		public class TestObject
		{
			public TestObject(ZString code, ZString description)
			{
				Code = code;
				Description = description;
			}

			public ZString Code { get; set; }
			public ZString Description { get; set; }
		}

		public void TestGetDescription()
		{
			var testObject = new TestObject("testCode", "testDesc");
			var addInfoDescAttribute = new AddInfoDescAttribute("Description");
			var description = addInfoDescAttribute.GetDescription(testObject);
			AssertEquals("testDesc", description);
		}
	}
}
