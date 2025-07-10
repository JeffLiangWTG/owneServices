using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	[TestedType(typeof(TraderCollection))]
	class TraderCollectionTest : ActiveBusinessObjectCollectionTestCase<TraderCollection>
	{
		protected override TraderCollection GetCollectionToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			return new TraderCollection(declaration);
		}

		public void TestSetDefaultsForNewElementCore()
		{
			var traders = GetCollectionToTest();
			var trader = traders.AddNew();
			AssertEquals(JobDeclarationSchema.Constants.Prefix, trader.E2_ParentTableCode);
			AssertEquals(DocAddressTypes.Codes.BuyerDocumentaryAddress, trader.E2_AddressType);
		}
	}
}
