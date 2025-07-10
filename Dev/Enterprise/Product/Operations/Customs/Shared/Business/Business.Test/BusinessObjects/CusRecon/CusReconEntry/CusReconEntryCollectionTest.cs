using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusReconEntryCollection))]
	class CusReconEntryCollectionTest : ActiveBusinessObjectCollectionTestCase<CusReconEntryCollection>
	{
		protected override CusReconEntryCollection GetCollectionToTest()
		{
			var master = Factory.New<CusReconDeclaration>();
			return new CusReconEntryCollection(master);
		}
	}
}
