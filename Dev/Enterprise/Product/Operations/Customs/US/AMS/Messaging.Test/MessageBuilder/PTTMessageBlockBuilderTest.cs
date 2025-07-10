using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.AMS.Messaging.Interface;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Moq;

namespace Enterprise.Customs.US.AMS.Messaging.Business.Testing
{
	class PTTMessageBlockBuilderTest : TestCaseWithFactory
	{
		public void TestT02IsNotIncluded()
		{
			TestT02IsNotIncluded_Manifest1();
			TestT02IsNotIncluded_Manifest2();
		}

		void TestT02IsNotIncluded_Manifest1()
		{
			ZDecimal manifestQuantity = 245m;
			var manifestMock = SetupTestT02IsNotIncluded(manifestQuantity.ToZInt(), out Mock<IACEBillOfLading> billMock1);
			AMSInterfaceTestHelper.PopulateINPM01(manifestMock);
			billMock1.Setup(m => m.ManifestQuantity).Returns(manifestQuantity);

			var actual = MessageBuilder(manifestMock.Object);

			AssertMultilineASCIIEquals("Message Data", ExpectedMessageWithoutT02, actual);
			manifestMock.VerifyAll();
			billMock1.VerifyAll();
		}

		void TestT02IsNotIncluded_Manifest2()
		{
			var manifestMock = SetupTestT02IsNotIncluded(ZInt.Zero, out Mock<IACEBillOfLading> billMock1);

			var actual = MessageBuilder(manifestMock.Object);
			AssertMultilineASCIIEquals("Message Data", ExpectedMessageWithoutT02, actual);
			manifestMock.VerifyAll();
			billMock1.VerifyAll();
		}

		public void TestBuild()
		{
			TestBuild_Manifest1();
			TestBuild_Manifest2();
		}

		void TestBuild_Manifest1()
		{
			var manifestMock = SetupTestBuildManifest(200);

			var actual = MessageBuilder(manifestMock.Object);

			AssertMultilineASCIIEquals("Message Data", ExpectedMessage, actual);
			manifestMock.VerifyAll();
		}

		void TestBuild_Manifest2()
		{
			var manifestMock = SetupTestBuildManifest(245);

			var actual = MessageBuilder(manifestMock.Object);

			AssertMultilineASCIIEquals("Message Data", ExpectedMessage2, actual);
			manifestMock.VerifyAll();
		}

		Mock<IACEBillManifestMessageAttachee> SetupTestT02IsNotIncluded(ZInt inBondQuantity, out Mock<IACEBillOfLading> billMock1)
		{
			var manifestMock = new Mock<IACEBillManifestMessageAttachee>();
			AMSInterfaceTestHelper.PopulateINPM01(manifestMock);

			var portMock = new Mock<IPort>();
			portMock.Setup(m => m.DistrictPortOfUnladingCode).Returns("5946");
			portMock.Setup(m => m.OriginalEstimatedDate).Returns(ZDate.BrettsBirthday);
			manifestMock.Setup(m => m.PortDetails).Returns(portMock.Object);

			billMock1 = new Mock<IACEBillOfLading>();
			billMock1.Setup(m => m.IssuerCode).Returns("BDKL");
			billMock1.Setup(m => m.BillOfLadingSequenceNumber).Returns("BDKLHB1");
			billMock1.Setup(m => m.FIRMS).Returns("Z456");

			var inBOndMock = new Mock<IMovemenDetails>();
			inBOndMock.Setup(m => m.InBondQuantity).Returns(inBondQuantity);
			inBOndMock.Setup(m => m.BondedCarrierID).Returns("965-63-5685");
			billMock1.Setup(m => m.MovemenDetails).Returns(inBOndMock.Object);
			manifestMock.Setup(m => m.BillOfLadingDetails).Returns(billMock1.Object);

			return manifestMock;
		}

		Mock<IACEBillManifestMessageAttachee> SetupTestBuildManifest(ZInt inBondQuantity)
		{
			var manifestMock = new Mock<IACEBillManifestMessageAttachee>();
			AMSInterfaceTestHelper.PopulateINPM01(manifestMock);

			var portMock = new Mock<IPort>();
			portMock.Setup(m => m.DistrictPortOfUnladingCode).Returns("5946");
			portMock.Setup(m => m.OriginalEstimatedDate).Returns(ZDate.BrettsBirthday);
			manifestMock.Setup(m => m.PortDetails).Returns(portMock.Object);

			var billMock1 = new Mock<IACEBillOfLading>();
			billMock1.Setup(m => m.IssuerCode).Returns("BDKL");
			billMock1.Setup(m => m.BillOfLadingSequenceNumber).Returns("BDKLHB1");
			billMock1.Setup(m => m.ManifestQuantity).Returns(245m);
			billMock1.Setup(m => m.FIRMS).Returns("Z456");

			var inBOndMock = new Mock<IMovemenDetails>();
			inBOndMock.Setup(m => m.InBondQuantity).Returns(inBondQuantity);
			inBOndMock.Setup(m => m.BondedCarrierID).Returns("965-63-5685");
			billMock1.Setup(m => m.MovemenDetails).Returns(inBOndMock.Object);
			manifestMock.Setup(m => m.BillOfLadingDetails).Returns(billMock1.Object);

			return manifestMock;
		}

		string MessageBuilder(IACEBillManifestMessageAttachee data)
		{
			var builder = new PTTMessageBlockBuilder(data);
			var messageBlocks = new List<MessageBlock>(builder.Build());
			var messageDataBuilder = new ZStringBuilder();
			messageBlocks.ForEach(x => messageDataBuilder.Append(x.Serialise()));
			return messageDataBuilder.ToStringWithNewLineBetweenAppends();
		}

		string ExpectedMessage
		{
			get
			{
				return @"M01SD2340AU                       V234      1      1234567                      
M02BDKLHB1_<MSG PLACEHOLDER>                                                    
P015946091871                                                                   
J01BDKL                                                                         
T01BDKLHB1                   Z456965-63-5685                                    
T020000000200                                                                   ";
			}
		}

		string ExpectedMessage2
		{
			get
			{
				return @"M01SD2340AU                       V234      1      1234567                      
M02BDKLHB1_<MSG PLACEHOLDER>                                                    
P015946091871                                                                   
J01BDKL                                                                         
T01BDKLHB1                   Z456965-63-5685                                    ";
			}
		}

		string ExpectedMessageWithoutT02
		{
			get
			{
				return @"M01SD2340AU                       V234      1      1234567                      
M02BDKLHB1_<MSG PLACEHOLDER>                                                    
P015946091871                                                                   
J01BDKL                                                                         
T01BDKLHB1                   Z456965-63-5685                                    ";
			}
		}
	}
}
