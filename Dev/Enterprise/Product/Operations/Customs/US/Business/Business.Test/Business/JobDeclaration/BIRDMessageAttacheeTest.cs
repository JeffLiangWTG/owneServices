using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	class BIRDMessageAttacheeTest : TestCaseWithFactory
	{
		public void TestTransportMode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			var msgAttachee = new BIRDMessageAttachee(declaration) as IMessageAttacheeWithCBPSenderReference;
			AssertEquals(Core.Constants.TransportModes.Sea, msgAttachee.TransportMode);
		}
	}
}
