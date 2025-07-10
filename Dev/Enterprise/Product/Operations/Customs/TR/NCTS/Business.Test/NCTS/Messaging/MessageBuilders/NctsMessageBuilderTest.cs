using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business.MessageGeneration;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	public class NctsMessageBuilderTest : TestCaseWithFactory
	{
		public void TestNoExceptionThrownForFunctionalityNotImplemented()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			AssertNoExceptionThrown(() =>
			{
				new Messaging.NctsMessageBuilder().NativeMessage(nctsHeader, new NctsMessageFunctionSet.DeclarationDataMessage(), new ErrorCollector());
			});
		}
	}
}
