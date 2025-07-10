using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.BIRD.Common;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common.Testing
{
	sealed class AENS21Test : TestCaseWithFactory
	{
		public void TestUpdateDeclaration()
		{
			var notifications = new NotificationBuffer();
			var declaration = Factory.New<JobDeclaration>();
			var aens21 = new AENS21
			{
				TripIdentifier = "123A"
			};

			((IBIRDHeaderRecord)aens21).Update(declaration, notifications);
			AssertEquals("123A", declaration.JE_VoyageFlightNo);
		}
	}
}
