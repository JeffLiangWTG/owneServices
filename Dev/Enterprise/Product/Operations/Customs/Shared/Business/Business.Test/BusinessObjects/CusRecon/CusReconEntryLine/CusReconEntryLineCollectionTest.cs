using System.ComponentModel;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusReconEntryLineCollection))]
	class CusReconEntryLineCollectionTest : ActiveBusinessObjectCollectionTestCase<CusReconEntryLineCollection>
	{
		public void TestDoNotAllowNew()
		{
			AssertEquals(false, ((IBindingList)GetCollectionToTest()).AllowNew);
		}

		protected override CusReconEntryLineCollection GetCollectionToTest()
		{
			var master = Factory.New<CusReconEntry>();
			return new CusReconEntryLineCollection(master);
		}
	}
}
