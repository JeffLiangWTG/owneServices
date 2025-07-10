using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V3.Business.Testing
{
	[TestedType(typeof(V3MessagesCollection))]
	public class V3MessagesCollectionTest : ActiveBusinessObjectCollectionTestCase<V3MessagesCollection>
	{
		public void TestAllowNew()
		{
			V3MessagesCollection collection = GetCollectionToTest();
			AssertEquals(false, ((IBindingList)collection).AllowNew);
		}

		#region implementation
		protected override V3MessagesCollection GetCollectionToTest()
		{
			return new V3MessagesCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<V3Message>();
		}
		#endregion
	}
}
