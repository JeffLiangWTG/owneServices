using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Business.Testing
{
	[TestedType(typeof(GatePassPackLineManyToManyCollection))]
	public class GatePassPackLineManyToManyCollectionBOCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			GatePassContainer parent = Factory.New<GatePassContainer>();
			return new GatePassPackLineManyToManyCollection(parent);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<GatePassPackLine>();
		}
	}
}
