using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.SG.V4.Business;
using NUnit.Framework;

namespace Enterprise.Customs.SG.Testing
{
	[TestedType(typeof(SGCPCCollection))]
	public class SGCPCCollectionTest : ActiveBusinessObjectCollectionTestCase<SGCPCCollection>
	{
		public void TestCollectionContainAIILine()
		{
			var cpc1 = Declaration.CPCs.AddNew();
			var cpc2 = Declaration.CPCs.AddNew();
			var cpc3 = Declaration.CPCs.AddNew();
			var collection = new SGCPCCollection(Declaration);
			AssertContainsExactElementsInAnyOrder(new TypedEnumerable<SGCPC>(new SGCPC[] { cpc1, cpc2, cpc3 }), collection);
		}

		#region Implementation
		protected override SGCPCCollection GetCollectionToTest()
		{
			return new SGCPCCollection(Declaration);
		}

		JobDeclaration Declaration
		{
			get
			{
				return declaration ?? (declaration = Factory.New<JobDeclaration>());
			}
		}

		JobDeclaration declaration;
		#endregion
	}
}
