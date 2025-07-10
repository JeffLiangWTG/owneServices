using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class IPGADispositionProviderExtensionMethodsTest : TestCaseWithFactory
	{
		public void TestGetFirstPGADispositionBlock()
		{
			var message1 = Factory.New<MQEDIMessage>();
			message1.EM_MessageType = ApplicationIdentifierCodeList.Codes.CargoReleaseProcessingResults;
			message1.EM_Status = EDIMessage.Status.Received;
			message1.EM_MessageText = "B018888XJ5RR                                                                    R18888XJ5 010000480195-210138700S00105139YMLUCSCL SPRING         0031E021716    R4            W231509435                          00002100PK   YMLU             R5021516020305PAPERLESS                                                         R6FDA    021516020306FDA MAY PROCEED                                            R6FDA    0215160203  FDA MAY PROCEED                  07001 001THRU001 001      Y  8888XJ5RR00010";

			AssertNotNull(message1.GetFirstPGADispositionBlock());
			var message2 = Factory.New<MQEDIMessage>();
			message2.EM_MessageType = ApplicationIdentifierCodeList.Codes.CargoReleaseProcessingResults;
			message2.EM_Status = EDIMessage.Status.Received;
			message2.EM_MessageText = "B018888XJ5RR                                                                    R18888XJ5 010000480195-210138700S00105139YMLUCSCL SPRING         0031E021716    R4            W231509435                          00002100PK   YMLU             R5021516020305PAPERLESS                                                         Y  8888XJ5RR00010";

			AssertNull(message2.GetFirstPGADispositionBlock());
		}

		public void TestHasEarlierDispositionDateTime()
		{
			var incomingMessage = Factory.New<MQEDIMessage>();
			incomingMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.CargoReleaseProcessingResults;
			incomingMessage.EM_Status = EDIMessage.Status.Received;
			incomingMessage.EM_MessageText = "B018888XJ5RR                                                                    R18888XJ5 010000480195-210138700S00105139YMLUCSCL SPRING         0031E021716    R4            W231509435                          00002100PK   YMLU             R5021516020305PAPERLESS                                                         R6FDA    021516020306FDA MAY PROCEED                                            R6FDA    0215160203  FDA MAY PROCEED                  07001 001THRU001 001      Y  8888XJ5RR00010";

			var incomingMessage2 = Factory.New<MQEDIMessage>();
			incomingMessage2.EM_MessageType = ApplicationIdentifierCodeList.Codes.CargoReleaseProcessingResults;
			incomingMessage2.EM_Status = EDIMessage.Status.Received;
			incomingMessage2.EM_MessageText = "B018888XJ5RR                                                                    R18888XJ5 010000480195-210138700S00105139YMLUCSCL SPRING         0031E021716    R4            W231509435                          00002100PK   YMLU             R5021316001105PAPERLESS                                                         R5021316001122RELEASE DATE UPDATE                     02131601                  R6FDA    021316001101FDA REVIEW                                                 Y  8888XJ5RR00025";

			var blockR6_1 = incomingMessage.GetFirstPGADispositionBlock();
			Assert(!blockR6_1.HasEarlierDispositionDateTime(new MQEDIMessage[] { incomingMessage2 }));

			var blockR6_2 = incomingMessage2.GetFirstPGADispositionBlock();
			Assert(blockR6_2.HasEarlierDispositionDateTime(new MQEDIMessage[] { incomingMessage }));
		}
	}
}
