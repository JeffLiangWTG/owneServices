using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.AMS.Messaging.Interface;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Moq;

namespace Enterprise.Customs.US.AMS.Messaging.Business.Testing
{
	sealed class ACECommonMessageBlockBuilderTest : TestCaseWithFactory
	{
		public void TestBuild()
		{
			TestBuild_Manifest1();

			TestBuild_Manifest2();
		}

		public void TestM02_WhenBillOfLadingLongerThan12()
		{
			var billMock = new Mock<IACEBillOfLading>();
			billMock.Setup(m => m.BillOfLadingSequenceNumber).Returns("123456789123456789");
			var manifestMock = new Mock<IACEBillManifestMessageAttachee>();
			manifestMock.Setup(m => m.BillOfLadingDetails).Returns(billMock.Object);
			var builder = new ACECommonMessageBlockBuilder(manifestMock.Object);
			var messageBlocks = new List<MessageBlock>(builder.Build());
			var messageDataBuilder = new ZStringBuilder();
			messageBlocks.ForEach(x => messageDataBuilder.Append(x.Serialise()));
			var msg = messageDataBuilder.ToStringWithNewLineBetweenAppends();
			AssertContains("Message Data has M02 block", "M02789123456789_<MSG PLACEHOLDER>                                               ", msg);
			manifestMock.VerifyAll();
			billMock.VerifyAll();
		}

		void TestBuild_Manifest1()
		{
			var manifestMock = new Mock<IACEBillManifestMessageAttachee>();
			AMSInterfaceTestHelper.PopulateINPM01(manifestMock);

			var portMock = AMSInterfaceTestHelper.PopulateBasicPortMock();
			manifestMock.Setup(m => m.PortDetails).Returns(portMock.Object);

			var billMock1 = new Mock<IACEBillOfLading>();
			billMock1.Setup(m => m.BillOfLadingSequenceNumber).Returns("BDKLHB1");

			manifestMock.Setup(m => m.BillOfLadingDetails).Returns(billMock1.Object);
			var builder = new ACECommonMessageBlockBuilder(manifestMock.Object);
			var messageBlocks = new List<MessageBlock>(builder.Build());
			var messageDataBuilder = new ZStringBuilder();
			messageBlocks.ForEach(x => messageDataBuilder.Append(x.Serialise()));
			AssertMultilineASCIIEquals("Message Data", ExpectedMessageNoVessel, messageDataBuilder.ToStringWithNewLineBetweenAppends());
			manifestMock.VerifyAll();
			portMock.VerifyAll();
			billMock1.VerifyAll();
		}

		void TestBuild_Manifest2()
		{
			var manifestMock = new Mock<IACEBillManifestMessageAttachee>();

			AMSInterfaceTestHelper.PopulateINPM01(manifestMock);
			manifestMock.Setup(m => m.ConveyanceName).Returns("DUMMY VESSEL");
			manifestMock.Setup(m => m.ConveyanceCode).Returns("");

			var portMock = AMSInterfaceTestHelper.PopulateBasicPortMock();
			manifestMock.Setup(m => m.PortDetails).Returns(portMock.Object);

			var billMock1 = new Mock<IACEBillOfLading>();
			billMock1.Setup(m => m.BillOfLadingSequenceNumber).Returns("BDKLHB1");

			manifestMock.Setup(m => m.BillOfLadingDetails).Returns(billMock1.Object);

			var builder = new ACECommonMessageBlockBuilder(manifestMock.Object);
			var messageBlocks = new List<MessageBlock>(builder.Build());
			var messageDataBuilder = new ZStringBuilder();
			messageBlocks.ForEach(x => messageDataBuilder.Append(x.Serialise()));
			AssertMultilineASCIIEquals("Message Data", ExpectedMessageWithVessel, messageDataBuilder.ToStringWithNewLineBetweenAppends());

			manifestMock.VerifyAll();
			portMock.VerifyAll();
			billMock1.VerifyAll();
		}

		string ExpectedMessageNoVessel
		{
			get
			{
				return @"M01SD2340AU                       V234      1      1234567                      
M02BDKLHB1_<MSG PLACEHOLDER>                                                    
P015946091871                                                                   ";
			}
		}

		string ExpectedMessageWithVessel
		{
			get
			{
				return @"M01SD2340AUDUMMY VESSEL           V234      1                                   
M02BDKLHB1_<MSG PLACEHOLDER>                                                    
P015946091871                                                                   ";
			}
		}
	}
}
