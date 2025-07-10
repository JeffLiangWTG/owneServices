using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class FZUnconcurrenceMessageBuilderTest : TestCaseWithFactory
	{
		[TestDate(2020, 06, 09)]
		public void TestUnconcurrenceMessage()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			declaration.US_US_NKLocationOfGoods = "W004";
			declaration.JE_DateOfArrival = new ZDateTime(2020, 05, 11);
			declaration.FTZAdmissionNumber = "1530001|12|00000001";

			var action = new FZEventAction(declaration, FZEventType.Unconcur);
			action.US_FTZContactName = "Jack Brown";
			action.US_FTZContactPhone = "0266887543";
			action.US_ReasonCode = FTZUnconcurrenceReasonCodeList.Codes._02;
			action.US_Reasons = "257700052";
			var builder = new FZUnconcurrenceMessageBuilder(action);
			var message = builder.PopulateMessage();
			AssertEquals(
@"B  8888XJ5FZ                                               <<MSGNO PLACEHOLDER>>
10115300011200000001                  J                                         
13JACK BROWN                              0266887543     02                     
14IB 257700052                                                                  
Y  8888XJ5FZ", message.EM_FormattedMessageText);
		}
	}
}
