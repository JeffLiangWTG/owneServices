using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class JobDeclarationEventDataModelTest : TestCaseWithFactory
	{
		public void TestOrigin() => AssertEquals("NZACK", eventDataModel.Origin);

		public void TestDestination() => AssertEquals("AUSYD", eventDataModel.Destination);

		public void TestFirstLeg() => AssertTransportLeg(eventDataModel.FirstLeg, "NZACK", "NZCHC");

		public void TestSecondLeg() => AssertTransportLeg(eventDataModel.SecondLeg, "NZCHC", "NZDUD");

		public void TestThirdLeg() => AssertTransportLeg(eventDataModel.ThirdLeg, "NZDUD", "AUHBH");

		public void TestFourthLeg() => AssertTransportLeg(eventDataModel.FourthLeg, "AUHBH", "AUMEL");

		public void TestLastLeg() => AssertTransportLeg(eventDataModel.LastLeg, "AUMEL", "AUSYD");

		void AssertTransportLeg(TransportEventDataModel transportLeg, string expectedOrigin, string expectedDestination) => CombineAssertions(() =>
		{
			AssertEquals("Origin", expectedOrigin, transportLeg.Origin);
			AssertEquals("Destination", expectedDestination, transportLeg.Destination);
		});

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_RL_NKOrigin = "NZACK";
			declaration.JE_RL_NKFinalDestination = "AUSYD";

			var transport1 = declaration.Transports.AddNew();
			transport1.JW_RL_NKLoadPort = "NZACK";
			transport1.JW_RL_NKDiscPort = "NZCHC";

			var transport2 = declaration.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "NZCHC";
			transport2.JW_RL_NKDiscPort = "NZDUD";

			var transport3 = declaration.Transports.AddNew();
			transport3.JW_RL_NKLoadPort = "NZDUD";
			transport3.JW_RL_NKDiscPort = "AUHBH";

			var transport4 = declaration.Transports.AddNew();
			transport4.JW_RL_NKLoadPort = "AUHBH";
			transport4.JW_RL_NKDiscPort = "AUMEL";

			var transport5 = declaration.Transports.AddNew();
			transport5.JW_RL_NKLoadPort = "AUMEL";
			transport5.JW_RL_NKDiscPort = "AUSYD";

			eventDataModel = new JobDeclarationEventDataModel(declaration);
		}
		BaseJobDeclaration declaration;
		JobDeclarationEventDataModel eventDataModel;
	}
}
