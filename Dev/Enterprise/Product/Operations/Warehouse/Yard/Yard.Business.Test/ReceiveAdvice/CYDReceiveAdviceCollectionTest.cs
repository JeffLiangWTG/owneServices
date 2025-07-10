using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	[TestedType(typeof(CYDReceiveAdviceCollection))]
	public class CYDReceiveAdviceCollectionTest : ActiveBusinessObjectCollectionTestCase<CYDReceiveAdviceCollection>
	{
		#region Implementation

		protected override CYDReceiveAdviceCollection GetCollectionToTest()
		{
			return new CYDReceiveAdviceCollection(Factory);
		}

		#endregion
	}
}
