using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.BIRD.Common;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common.Testing
{
	sealed class AENS33Test : TestCaseWithFactory
	{
		public void TestUpdateDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			var notifications = new NotificationBuffer();
			var aens33 = new AENS33()
			{
				MissingDocumentCode1 = "17",
				MissingDocumentCode2 = "99"
			};

			((IBIRDHeaderRecord)aens33).Update(declaration, notifications);
			AssertEquals("17", declaration.US_MissingDocument1);
			AssertEquals("99", declaration.US_MissingDocument2);
		}
	}
}
