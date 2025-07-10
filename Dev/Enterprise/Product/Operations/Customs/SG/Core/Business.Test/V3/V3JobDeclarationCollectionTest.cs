using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V3.Business.Testing
{
	[TestedType(typeof(V3JobDeclarationCollection))]
	public class V3JobDeclarationCollectionTest : ActiveBusinessObjectCollectionTestCase<V3JobDeclarationCollection>
	{
		public void TestAllowNew()
		{
			V3JobDeclarationCollection collection = GetCollectionToTest();
			AssertEquals(false, ((IBindingList)collection).AllowNew);
		}

		#region implementation
		protected override V3JobDeclarationCollection GetCollectionToTest()
		{
			return new V3JobDeclarationCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<V3JobDeclaration>();
		}
		#endregion
	}
}
