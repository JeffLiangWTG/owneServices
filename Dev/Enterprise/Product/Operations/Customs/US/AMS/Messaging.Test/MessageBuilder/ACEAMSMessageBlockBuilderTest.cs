using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.AMS.Messaging.Interface;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common;
using Moq;
using NUnit.Framework;
using AESInput = Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AES.Input;
using AMSCommon = Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AMS.Common;

namespace Enterprise.Customs.US.AMS.Messaging.Business.Testing
{
	sealed class ACEAMSMessageBlockBuilderTest : TestCaseWithFactory
	{
		public void TestBuild()
		{
			TestBuild_Manifest1();

			TestBuild_Manifest2();

			TestBuild_Manifest3();
		}

		public void TestB01_WhenBillOfLadingLongerThan12()
		{
			var billMock = new Mock<IACEBillOfLading>();
			billMock.Setup(m => m.BillOfLadingSequenceNumber).Returns("123456789123456789");
			var manifestMock = new Mock<IACEBillManifestMessageAttachee>();
			manifestMock.Setup(m => m.BillOfLadingDetails).Returns(billMock.Object);
			var builder = new ACEAMSMessageBlockBuilder(manifestMock.Object, ActionCode.Creating);
			var messageBlocks = new List<MessageBlock>(builder.Build());
			var messageDataBuilder = new ZStringBuilder();
			messageBlocks.ForEach(x => messageDataBuilder.Append(x.Serialise()));
			var msg = messageDataBuilder.ToStringWithNewLineBetweenAppends();
			AssertContains("Message Data has B01 block", "B01789123456789     0000000000     0000000000                                   ", msg);
			manifestMock.VerifyAll();
			billMock.VerifyAll();
		}

		void TestBuild_Manifest1()
		{
			var manifestMock = new Mock<IACEBillManifestMessageAttachee>();
			AMSInterfaceTestHelper.PopulateINPM01(manifestMock);
			manifestMock.Setup(m => m.UniqueVoyageIdentifier).Returns("SD23DJV234");

			var portMock = AMSInterfaceTestHelper.PopulateBasicPortMock();
			manifestMock.Setup(m => m.PortDetails).Returns(portMock.Object);

			var billMock1 = new Mock<IACEBillOfLading>();
			billMock1.Setup(m => m.BillActionCode).Returns(AMSBillSendingActionCodeList.Codes.AddBill);
			billMock1.Setup(m => m.IssuerCode).Returns("BDKL");
			billMock1.Setup(m => m.BillOfLadingSequenceNumber).Returns("BDKLHB1");
			billMock1.Setup(m => m.ForeignPort).Returns("56842");
			billMock1.Setup(m => m.ManifestQuantity).Returns(245m);
			billMock1.Setup(m => m.ManifestUnits).Returns("NO");
			billMock1.Setup(m => m.Weight).Returns(15250m);
			billMock1.Setup(m => m.WeightUnit).Returns("KG");
			billMock1.Setup(m => m.BillOfLadingStatusIndicator).Returns("B");
			billMock1.Setup(m => m.IsMasterInbond).Returns(true);
			billMock1.Setup(m => m.PlaceOfReceiptByCarrier).Returns("SYDNEY");
			billMock1.Setup(m => m.SecondNotifyParty1).Returns("SC53");
			billMock1.Setup(m => m.SecondNotifyParty2).Returns("SC98");
			billMock1.Setup(m => m.LastForeignPortBeforeDepartingForTheUS).Returns("56975");
			billMock1.Setup(m => m.ModeOfTransportationFromThePlacePriorToLoading).Returns("20");
			billMock1.Setup(m => m.MethodOfPaymentForTransportation).Returns("CHK");
			billMock1.Setup(m => m.ContractualPossessionForeignPort).Returns("56231");

			billMock1.Setup(m => m.Volume).Returns(23m);
			billMock1.Setup(m => m.VolumeUnit).Returns("CM");
			var shipmentReferenceDetail1Mock = new Mock<IShipmentReferenceDetail>();
			shipmentReferenceDetail1Mock.Setup(m => m.Qualifier).Returns("M");
			shipmentReferenceDetail1Mock.Setup(m => m.ReferenceIdentifier).Returns("MASTERXTN1");
			var shipmentReferenceDetail2Mock = new Mock<IShipmentReferenceDetail>();
			shipmentReferenceDetail2Mock.Setup(m => m.Qualifier).Returns("H");
			shipmentReferenceDetail2Mock.Setup(m => m.ReferenceIdentifier).Returns("HOUSEXTN1");
			billMock1.Setup(m => m.ShipmentReferenceDetails(It.IsAny<ActionCode>())).Returns(new IShipmentReferenceDetail[] { shipmentReferenceDetail1Mock.Object, shipmentReferenceDetail2Mock.Object });

			var entity1Mock = AMSInterfaceTestHelper.GetEntity("SH", "SHIPPER NAME", "17", "ID23423", "SHIPPER ADDRESS 1", "SHIPPER ADDRESS 2", "SHIPPER ADDRESS 3", "SHIPPER ADDRESS 4", "SHIPPER CITY", "NS", "2000", "AU");
			var entity2Mock = AMSInterfaceTestHelper.GetEntity("CN", "CONSIGNEE NAME", "1", "ID89564", "CONSIGNEE ADDRESS 1", "CONSIGNEE ADDRESS 2", "CONSIGNEE ADDRESS 3", "CONSIGNEE ADDRESS 4", "CONSIGNEE CITY", "CA", "60025", "US");
			var entity3Mock = AMSInterfaceTestHelper.GetEntity("N1", "NOTIFY PARTY 1 NAME", "98", "ID986575", "NOTIFY PARTY 1 ADDRESS 1", "NOTIFY PARTY 1 ADDRESS 2", "NOTIFY PARTY 1 ADDRESS 3", "NOTIFY PARTY 1 ADDRESS 4", "NOTIFY PARTY 1 CITY", "NY", "96632", "US");
			var entity4Mock = AMSInterfaceTestHelper.GetEntity("N2", "NOTIFY PARTY 2 NAME", "36", "ID876534", "NOTIFY PARTY 2 ADDRESS 1", "NOTIFY PARTY 2 ADDRESS 2", "NOTIFY PARTY 2 ADDRESS 3", "NOTIFY PARTY 2 ADDRESS 4", "NOTIFY PARTY 2 CITY", "VI", "3005", "AU");
			billMock1.Setup(m => m.Entities(It.IsAny<ActionCode>())).Returns(new IEntity[] { entity1Mock.Object, entity2Mock.Object, entity3Mock.Object, entity4Mock.Object });
			var inBOndMock = new Mock<IMovemenDetails>();
			inBOndMock.Setup(m => m.InbondEntryType).Returns("62");
			inBOndMock.Setup(m => m.IsBTAFDA).Returns(true);
			inBOndMock.Setup(m => m.ConventionalInbondNumber).Returns("INB12346");
			inBOndMock.Setup(m => m.InbondCarrierCode).Returns("SD98");
			inBOndMock.Setup(m => m.USPortOfDestination).Returns("2354");
			inBOndMock.Setup(m => m.ForeignDestination).Returns("69542");
			inBOndMock.Setup(m => m.Value).Returns(23212212);
			inBOndMock.Setup(m => m.BondedCarrierID).Returns("965-63-5685");
			inBOndMock.Setup(m => m.PaperlessInbondNumber).Returns("VINB53264");
			inBOndMock.Setup(m => m.ExportVesselName).Returns("EXPORT VESSEL NAME");
			billMock1.Setup(m => m.MovemenDetails).Returns(inBOndMock.Object);
			var container1Mock = AMSInterfaceTestHelper.GetACEContainer("C1" + "BDKLHB1", true);
			var container2Mock = AMSInterfaceTestHelper.GetACEContainer("C2" + "BDKLHB1", false);
			billMock1.Setup(m => m.Containers).Returns(new IACEContainer[] { container1Mock.Object, container2Mock.Object });

			manifestMock.Setup(m => m.BillOfLadingDetails).Returns(billMock1.Object);

			var builder = new ACEAMSMessageBlockBuilder(manifestMock.Object, ActionCode.Creating);
			var messageBlocks = new List<MessageBlock>(builder.Build());
			var messageDataBuilder = new ZStringBuilder();
			messageBlocks.ForEach(x => messageDataBuilder.Append(x.Serialise()));
			AssertMultilineASCIIEquals("Message Data", ExpectedAMSCreateMessage1, messageDataBuilder.ToStringWithNewLineBetweenAppends());
			manifestMock.VerifyAll();
			portMock.VerifyAll();
			billMock1.VerifyAll();
			shipmentReferenceDetail1Mock.VerifyAll();
			shipmentReferenceDetail2Mock.VerifyAll();
			entity1Mock.VerifyAll();
			entity2Mock.VerifyAll();
			entity3Mock.VerifyAll();
			entity4Mock.VerifyAll();
			inBOndMock.VerifyAll();
			container1Mock.VerifyAll();
			container2Mock.VerifyAll();
		}

		void TestBuild_Manifest2()
		{
			var manifestMock = AMSInterfaceTestHelper.GetManifestForACE(false);
			var builder = new ACEAMSMessageBlockBuilder(manifestMock.Object, ActionCode.Creating);
			var messageBlocks = new List<MessageBlock>(builder.Build());
			var messageDataBuilder = new ZStringBuilder();
			messageBlocks.ForEach(x => messageDataBuilder.Append(x.Serialise()));
			AssertMultilineASCIIEquals("Message Data", ExpectedAMSCreateMessage2, messageDataBuilder.ToStringWithNewLineBetweenAppends());
			manifestMock.VerifyAll();
		}

		void TestBuild_Manifest3()
		{
			var manifestMock = new Mock<IACEBillManifestMessageAttachee>();
			AMSInterfaceTestHelper.PopulateINPM01(manifestMock);
			manifestMock.Setup(m => m.UniqueVoyageIdentifier).Returns("SD23DJV234");

			var portMock = AMSInterfaceTestHelper.PopulateBasicPortMock();
			manifestMock.Setup(m => m.PortDetails).Returns(portMock.Object);

			var billMock = new Mock<IACEBillOfLading>();
			billMock.Setup(m => m.BillActionCode).Returns(AMSBillSendingActionCodeList.Codes.AddBill);
			billMock.Setup(m => m.IssuerCode).Returns("BDKL");
			billMock.Setup(m => m.BillOfLadingSequenceNumber).Returns("BDKLHB1");
			billMock.Setup(m => m.ForeignPort).Returns("56842");
			billMock.Setup(m => m.ManifestQuantity).Returns(245m);
			billMock.Setup(m => m.ManifestUnits).Returns("NO");
			billMock.Setup(m => m.Weight).Returns(15250m);
			billMock.Setup(m => m.WeightUnit).Returns("KG");
			billMock.Setup(m => m.BillOfLadingStatusIndicator).Returns("B");
			billMock.Setup(m => m.IsMasterInbond).Returns(false);
			billMock.Setup(m => m.PlaceOfReceiptByCarrier).Returns("SYDNEY");
			billMock.Setup(m => m.SecondNotifyParty1).Returns("SC53");
			billMock.Setup(m => m.SecondNotifyParty2).Returns("SC98");
			billMock.Setup(m => m.LastForeignPortBeforeDepartingForTheUS).Returns("56975");
			billMock.Setup(m => m.ModeOfTransportationFromThePlacePriorToLoading).Returns("20");
			billMock.Setup(m => m.MethodOfPaymentForTransportation).Returns("CHK");
			billMock.Setup(m => m.ContractualPossessionForeignPort).Returns("56231");
			billMock.Setup(m => m.Volume).Returns(0m);
			billMock.Setup(m => m.ShipmentReferenceDetails(It.IsAny<ActionCode>())).Returns((IEnumerable<IShipmentReferenceDetail>)null);

			var entity1Mock = AMSInterfaceTestHelper.GetEntity("SH", "SHIPPER NAME", "17", "ID23423", "SHIPPER ADDRESS 1", "SHIPPER ADDRESS 2", "SHIPPER ADDRESS 3", "SHIPPER ADDRESS 4", "SHIPPER CITY", "NS", "2000", "AU");
			var entity2Mock = AMSInterfaceTestHelper.GetEntity("CN", "CONSIGNEE NAME", "1", "ID89564", "CONSIGNEE ADDRESS 1", "CONSIGNEE ADDRESS 2", "CONSIGNEE ADDRESS 3", "CONSIGNEE ADDRESS 4", "CONSIGNEE CITY", "CA", "60025", "US");
			var entity3Mock = AMSInterfaceTestHelper.GetEntity("N1", "NOTIFY PARTY 1 NAME", "98", "ID986575", "NOTIFY PARTY 1 ADDRESS 1", "NOTIFY PARTY 1 ADDRESS 2", "NOTIFY PARTY 1 ADDRESS 3", "NOTIFY PARTY 1 ADDRESS 4", "NOTIFY PARTY 1 CITY", "NY", "96632", "US");
			var entity4Mock = AMSInterfaceTestHelper.GetEntity("N2", "NOTIFY PARTY 2 NAME", "36", "ID876534", "NOTIFY PARTY 2 ADDRESS 1", "NOTIFY PARTY 2 ADDRESS 2", "NOTIFY PARTY 2 ADDRESS 3", "NOTIFY PARTY 2 ADDRESS 4", "NOTIFY PARTY 2 CITY", "VI", "3005", "AU");
			billMock.Setup(m => m.Entities(It.IsAny<ActionCode>())).Returns(new IEntity[] { entity1Mock.Object, entity2Mock.Object, entity3Mock.Object, entity4Mock.Object });
			var inBOndMock = new Mock<IMovemenDetails>();
			inBOndMock.Setup(m => m.InbondEntryType).Returns("62");

			inBOndMock.Setup(m => m.IsBTAFDA).Returns(false);
			inBOndMock.Setup(m => m.ConventionalInbondNumber).Returns("INB12346");
			inBOndMock.Setup(m => m.InbondCarrierCode).Returns("SD98");
			inBOndMock.Setup(m => m.USPortOfDestination).Returns("2354");
			inBOndMock.Setup(m => m.ForeignDestination).Returns("69542");
			inBOndMock.Setup(m => m.Value).Returns(23212212);
			inBOndMock.Setup(m => m.BondedCarrierID).Returns("965-63-5685");
			inBOndMock.Setup(m => m.PaperlessInbondNumber).Returns("VINB53264");
			inBOndMock.Setup(m => m.ExportVesselName).Returns(string.Empty);
			billMock.Setup(m => m.MovemenDetails).Returns(inBOndMock.Object);
			var container1Mock = AMSInterfaceTestHelper.GetACEContainer("C1" + "BDKLHB1", false);
			var container2Mock = AMSInterfaceTestHelper.GetACEContainer("C2" + "BDKLHB1", true);
			billMock.Setup(m => m.Containers).Returns(new IACEContainer[] { container1Mock.Object, container2Mock.Object });
			billMock.Setup(m => m.BillOfLadingStatusIndicator).Returns(BillOfLadingStatusIndicatorList.Codes.SimpleRegularInBondType62_63WithISF);
			manifestMock.Setup(m => m.BillOfLadingDetails).Returns(billMock.Object);
			var builder = new ACEAMSMessageBlockBuilder(manifestMock.Object, ActionCode.Creating);
			var messageBlocks = new List<MessageBlock>(builder.Build());
			var messageDataBuilder = new ZStringBuilder();
			messageBlocks.ForEach(x => messageDataBuilder.Append(x.Serialise()));
			AssertMultilineASCIIEquals("Message Data", ExpectedAMSCreateMessage3, messageDataBuilder.ToStringWithNewLineBetweenAppends());

			manifestMock.VerifyAll();
			portMock.VerifyAll();
			billMock.VerifyAll();
			inBOndMock.VerifyAll();
			entity1Mock.VerifyAll();
			entity2Mock.VerifyAll();
			entity3Mock.VerifyAll();
			entity4Mock.VerifyAll();
			container1Mock.VerifyAll();
			container2Mock.VerifyAll();
		}

		[ExpectNoExceptions]
		public void TestINPN04ContactNameMaxLength()
		{
			var manifestMock = AMSInterfaceTestHelper.GetManifestForACE(true);
			var builder = new ACEAMSMessageBlockBuilder(manifestMock.Object, ActionCode.Creating);
			var messageBlocks = new List<MessageBlock>(builder.Build());
			messageBlocks.Add(new INPN04()
			{
				ContactName = "This is a string greater than 23 characters",
			});
			var messageDataBuilder = new ZStringBuilder();
			messageBlocks.ForEach(x => messageDataBuilder.Append(x.Serialise()));
			manifestMock.VerifyAll();
		}

		[ExpectNoExceptions]
		public void TestINPC01SealNumber1MaxLength()
		{
			var manifestMock = AMSInterfaceTestHelper.GetManifestForACE(true);
			var builder = new ACEAMSMessageBlockBuilder(manifestMock.Object, ActionCode.Creating);
			var messageBlocks = new List<MessageBlock>(builder.Build());
			messageBlocks.Add(new AMSCommon.INPC01()
			{
				// Length = 16
				SealNumber1 = "US 0123456789012"
			});
			var messageDataBuilder = new ZStringBuilder();
			messageBlocks.ForEach(x => messageDataBuilder.Append(x.Serialise()));
			AssertContains("Message Data has C01 block with SealNumber", "C01              ***************", messageDataBuilder.ToStringWithNewLineBetweenAppends());
			manifestMock.VerifyAll();
		}

		[ExpectNoExceptions]
		public void TestINPC01SealNumber2MaxLength()
		{
			var manifestMock = AMSInterfaceTestHelper.GetManifestForACE(true);
			var builder = new ACEAMSMessageBlockBuilder(manifestMock.Object, ActionCode.Creating);
			var messageBlocks = new List<MessageBlock>(builder.Build());
			messageBlocks.Add(new AMSCommon.INPC01()
			{
				// Length = 16
				SealNumber1 = "US 123",
				SealNumber2 = "US 1234567890123"
			});
			var messageDataBuilder = new ZStringBuilder();
			messageBlocks.ForEach(x => messageDataBuilder.Append(x.Serialise()));
			AssertContains("Message Data has C01 block with SealNumber", "C01              US 123         ***************                                ", messageDataBuilder.ToStringWithNewLineBetweenAppends());
			manifestMock.VerifyAll();
		}

		[ExpectNoExceptions]
		public void TestAESCommShipSC2XPEntryNumberMaxLength()
		{
			var manifestMock = AMSInterfaceTestHelper.GetManifestForACE(true);
			var builder = new ACEAMSMessageBlockBuilder(manifestMock.Object, ActionCode.Creating);
			var messageBlocks = new List<MessageBlock>(builder.Build());
			messageBlocks.Add(new AESInput.AESCommShipSC2XP()
			{
				// Length = 16
				EntryNumber = "US 0123456789012",
			});
			var messageDataBuilder = new ZStringBuilder();
			messageBlocks.ForEach(x => messageDataBuilder.Append(x.Serialise()));
			AssertContains("Message Data has C01 block with SealNumber1", "SC2  ***************", messageDataBuilder.ToStringWithNewLineBetweenAppends());
			manifestMock.VerifyAll();
		}

		string ExpectedAMSCreateMessage1
		{
			get
			{
				return @"M01SD2340AU                       V234      1      1234567                      
M02BDKLHB1_<MSG PLACEHOLDER>                                                    
B04V3 SD23DJV234                                                                
P015946091871                                                                   
J01BDKL                                                                         
B01BDKLHB1     568420000000245NO   0000015250KGB1                               
B020000000023CMSYDNEY                       SC53SC985697520CHK56231             
B04M  MASTERXTN1                                                                
B04H  HOUSEXTN1                                                                 
N00SH SHIPPER NAME                       17ID23423                              
N02SHIPPER ADDRESS 1                  SHIPPER ADDRESS 2                         
N02SHIPPER ADDRESS 3                  SHIPPER ADDRESS 4                         
N03SHIPPER CITY       NS2000     AU                                             
N04BOB THE BUILDER        TE1059654684               WP9686448456               
N00CN CONSIGNEE NAME                     1 ID89564                              
N02CONSIGNEE ADDRESS 1                CONSIGNEE ADDRESS 2                       
N02CONSIGNEE ADDRESS 3                CONSIGNEE ADDRESS 4                       
N03CONSIGNEE CITY     CA60025    US                                             
N04BOB THE BUILDER        TE1059654684               WP9686448456               
N00N1 NOTIFY PARTY 1 NAME                98ID986575                             
N02NOTIFY PARTY 1 ADDRESS 1           NOTIFY PARTY 1 ADDRESS 2                  
N02NOTIFY PARTY 1 ADDRESS 3           NOTIFY PARTY 1 ADDRESS 4                  
N03NOTIFY PARTY 1 CITYNY96632    US                                             
N04BOB THE BUILDER        TE1059654684               WP9686448456               
N00N2 NOTIFY PARTY 2 NAME                36ID876534                             
N02NOTIFY PARTY 2 ADDRESS 1           NOTIFY PARTY 2 ADDRESS 2                  
N02NOTIFY PARTY 2 ADDRESS 3           NOTIFY PARTY 2 ADDRESS 4                  
N03NOTIFY PARTY 2 CITYVI3005     AU                                             
N04BOB THE BUILDER        TE1059654684               WP9686448456               
I0162Y INB12346 SD9823546954223212212965-63-5685 VINB53264                      
I0210EXPORT VESSEL NAME                                                         
C01C1BDKLHB1     SEAL1          SEAL2          2020   21      22      20FRLCS   
C02VIN1234658                                                                   
D001010101012 000001230000032424KG                                              
D010000000010DESCRIPTION THAT IS LONG AND SHOULD BE AT LEA              PCE     
D010000000000ST TWO LINE AND NOT ONE LINE AGAIN I SAID DES                      
D010000000000CRIPTION THAT IS LONG AND SHOULD BE AT LEAST                       
D010000000000TWO LINE AND NOT ONE LINE                                          
D02MARKS AND NUMBERS THAT SHOULD BE IN AT LEAST                                 
D02TWO LINE AND NOT ONE LINE AGAIN I SAID MARKS                                 
D02AND NUMBERS THAT SHOULD BE IN AT LEAST TWO LI                                
D02NE AND NOT ONE LINE                                                          
D010000000010DESCRIPTION THAT IS LONG AND SHOULD BE AT LEA              PCE     
D010000000000ST TWO LINE AND NOT ONE LINE AGAIN I SAID DES                      
D010000000000CRIPTION THAT IS LONG AND SHOULD BE AT LEAST                       
D010000000000TWO LINE AND NOT ONE LINE                                          
D02MARKS AND NUMBERS THAT SHOULD BE IN AT LEAST                                 
D02TWO LINE AND NOT ONE LINE AGAIN I SAID MARKS                                 
D02AND NUMBERS THAT SHOULD BE IN AT LEAST TWO LI                                
D02NE AND NOT ONE LINE                                                          
V01UN114561  CL32UHAZRDOUS DESCRIPTION THAT IS LBOB THE BUILDER                 
V02032CEN                                                                       
V03ONG AND SHOULD BE AT LEAST TWOSHIPPING NAME 1                                
V03 LINE AND NOT ONE LINE                                                       
V01R9658     CL69UHAZRDOUS DESCRIPTION          WENDY THE DESTROYER             
V02045CE                                                                        
V03                              SHIPPING NAME 2                                
V011381E     4.2 UPHOSPHORUS                    AARON                           
V02                                                                             
V03                                                                             
C01C2BDKLHB1     SEAL1          SEAL2          2020   21      22      20FRLCS   
C02VIN1234658                                                                   
D010000000010DESCRIPTION THAT IS LONG AND SHOULD BE AT LEA              PCE     
D010000000000ST TWO LINE AND NOT ONE LINE AGAIN I SAID DES                      
D010000000000CRIPTION THAT IS LONG AND SHOULD BE AT LEAST                       
D010000000000TWO LINE AND NOT ONE LINE                                          
D02MARKS AND NUMBERS THAT SHOULD BE IN AT LEAST                                 
D02TWO LINE AND NOT ONE LINE AGAIN I SAID MARKS                                 
D02AND NUMBERS THAT SHOULD BE IN AT LEAST TWO LI                                
D02NE AND NOT ONE LINE                                                          
D002013456875 000001230000032424KG                                              
D010000000010DESCRIPTION THAT IS LONG AND SHOULD BE AT LEA              PCE     
D010000000000ST TWO LINE AND NOT ONE LINE AGAIN I SAID DES                      
D010000000000CRIPTION THAT IS LONG AND SHOULD BE AT LEAST                       
D010000000000TWO LINE AND NOT ONE LINE                                          
D02MARKS AND NUMBERS THAT SHOULD BE IN AT LEAST                                 
D02TWO LINE AND NOT ONE LINE AGAIN I SAID MARKS                                 
D02AND NUMBERS THAT SHOULD BE IN AT LEAST TWO LI                                
D02NE AND NOT ONE LINE                                                          
V01UN114561  CL32U                              BOB THE BUILDER                 
V02                                                                             
V03                              SHIPPING NAME 1                                
V01R9658     CL69U                              WENDY THE DESTROYER             
V02                                                                             
V03                              SHIPPING NAME 2                                
V011381E     4.2 UPHOSPHORUS                    AARON                           
V02                                                                             
V03                                                                             ";
			}
		}

		string ExpectedAMSCreateMessage2
		{
			get
			{
				return @"M01SD2340AU                       V234      1      1234567                      
M02BDKLHB1_<MSG PLACEHOLDER>                                                    
B04V3 SD23DJV234                                                                
P015946091871                                                                   
J01BDKL                                                                         
B01BDKLHB1     568420000000245NO   0000015250KGB                                
B02            SYDNEY                       SC53SC985697520CHK56231             
N00SH SHIPPER NAME                       17ID23423                              
N02SHIPPER ADDRESS 1                  SHIPPER ADDRESS 2                         
N02SHIPPER ADDRESS 3                  SHIPPER ADDRESS 4                         
N03SHIPPER CITY       NS2000     AU                                             
N04BOB THE BUILDER        TE1059654684               WP9686448456               
N00CN CONSIGNEE NAME                     1 ID89564                              
N02CONSIGNEE ADDRESS 1                CONSIGNEE ADDRESS 2                       
N02CONSIGNEE ADDRESS 3                CONSIGNEE ADDRESS 4                       
N03CONSIGNEE CITY     CA60025    US                                             
N04BOB THE BUILDER        TE1059654684               WP9686448456               
N00N1 NOTIFY PARTY 1 NAME                98ID986575                             
N02NOTIFY PARTY 1 ADDRESS 1           NOTIFY PARTY 1 ADDRESS 2                  
N02NOTIFY PARTY 1 ADDRESS 3           NOTIFY PARTY 1 ADDRESS 4                  
N03NOTIFY PARTY 1 CITYNY96632    US                                             
N04BOB THE BUILDER        TE1059654684               WP9686448456               
N00N2 NOTIFY PARTY 2 NAME                36ID876534                             
N02NOTIFY PARTY 2 ADDRESS 1           NOTIFY PARTY 2 ADDRESS 2                  
N02NOTIFY PARTY 2 ADDRESS 3           NOTIFY PARTY 2 ADDRESS 4                  
N03NOTIFY PARTY 2 CITYVI3005     AU                                             
N04BOB THE BUILDER        TE1059654684               WP9686448456               
C01C1BDKLHB1     SEAL1          SEAL2          2020   21      22      20FRLCS   
C02VIN1234658                                                                   
D010000000010DESCRIPTION THAT IS LONG AND SHOULD BE AT LEA              PCE     
D010000000000ST TWO LINE AND NOT ONE LINE AGAIN I SAID DES                      
D010000000000CRIPTION THAT IS LONG AND SHOULD BE AT LEAST                       
D010000000000TWO LINE AND NOT ONE LINE                                          
D02MARKS AND NUMBERS THAT SHOULD BE IN AT LEAST                                 
D02TWO LINE AND NOT ONE LINE AGAIN I SAID MARKS                                 
D02AND NUMBERS THAT SHOULD BE IN AT LEAST TWO LI                                
D02NE AND NOT ONE LINE                                                          
D002013456875 000001230000032424KG                                              
D010000000010DESCRIPTION THAT IS LONG AND SHOULD BE AT LEA              PCE     
D010000000000ST TWO LINE AND NOT ONE LINE AGAIN I SAID DES                      
D010000000000CRIPTION THAT IS LONG AND SHOULD BE AT LEAST                       
D010000000000TWO LINE AND NOT ONE LINE                                          
D02MARKS AND NUMBERS THAT SHOULD BE IN AT LEAST                                 
D02TWO LINE AND NOT ONE LINE AGAIN I SAID MARKS                                 
D02AND NUMBERS THAT SHOULD BE IN AT LEAST TWO LI                                
D02NE AND NOT ONE LINE                                                          
V01UN114561  CL32U                              BOB THE BUILDER                 
V02                                                                             
V03                              SHIPPING NAME 1                                
V01R9658     CL69U                              WENDY THE DESTROYER             
V02                                                                             
V03                              SHIPPING NAME 2                                
V011381E     4.2 UPHOSPHORUS                    AARON                           
V02                                                                             
V03                                                                             
C01C2BDKLHB1     SEAL1          SEAL2          2020   21      22      20FRLCS   
C02VIN1234658                                                                   
D001010101012 000001230000032424KG                                              
D010000000010DESCRIPTION THAT IS LONG AND SHOULD BE AT LEA              PCE     
D010000000000ST TWO LINE AND NOT ONE LINE AGAIN I SAID DES                      
D010000000000CRIPTION THAT IS LONG AND SHOULD BE AT LEAST                       
D010000000000TWO LINE AND NOT ONE LINE                                          
D02MARKS AND NUMBERS THAT SHOULD BE IN AT LEAST                                 
D02TWO LINE AND NOT ONE LINE AGAIN I SAID MARKS                                 
D02AND NUMBERS THAT SHOULD BE IN AT LEAST TWO LI                                
D02NE AND NOT ONE LINE                                                          
D010000000010DESCRIPTION THAT IS LONG AND SHOULD BE AT LEA              PCE     
D010000000000ST TWO LINE AND NOT ONE LINE AGAIN I SAID DES                      
D010000000000CRIPTION THAT IS LONG AND SHOULD BE AT LEAST                       
D010000000000TWO LINE AND NOT ONE LINE                                          
D02MARKS AND NUMBERS THAT SHOULD BE IN AT LEAST                                 
D02TWO LINE AND NOT ONE LINE AGAIN I SAID MARKS                                 
D02AND NUMBERS THAT SHOULD BE IN AT LEAST TWO LI                                
D02NE AND NOT ONE LINE                                                          
V01UN114561  CL32UHAZRDOUS DESCRIPTION THAT IS LBOB THE BUILDER                 
V02032CEN                                                                       
V03ONG AND SHOULD BE AT LEAST TWOSHIPPING NAME 1                                
V03 LINE AND NOT ONE LINE                                                       
V01R9658     CL69UHAZRDOUS DESCRIPTION          WENDY THE DESTROYER             
V02045CE                                                                        
V03                              SHIPPING NAME 2                                
V011381E     4.2 UPHOSPHORUS                    AARON                           
V02                                                                             
V03                                                                             ";
			}
		}

		string ExpectedAMSCreateMessage3
		{
			get
			{
				return @"M01SD2340AU                       V234      1      1234567                      
M02BDKLHB1_<MSG PLACEHOLDER>                                                    
B04V3 SD23DJV234                                                                
P015946091871                                                                   
J01BDKL                                                                         
B01BDKLHB1     568420000000245NO   0000015250KGT                                
B02            SYDNEY                       SC53SC985697520CHK56231             
N00SH SHIPPER NAME                       17ID23423                              
N02SHIPPER ADDRESS 1                  SHIPPER ADDRESS 2                         
N02SHIPPER ADDRESS 3                  SHIPPER ADDRESS 4                         
N03SHIPPER CITY       NS2000     AU                                             
N04BOB THE BUILDER        TE1059654684               WP9686448456               
N00CN CONSIGNEE NAME                     1 ID89564                              
N02CONSIGNEE ADDRESS 1                CONSIGNEE ADDRESS 2                       
N02CONSIGNEE ADDRESS 3                CONSIGNEE ADDRESS 4                       
N03CONSIGNEE CITY     CA60025    US                                             
N04BOB THE BUILDER        TE1059654684               WP9686448456               
N00N1 NOTIFY PARTY 1 NAME                98ID986575                             
N02NOTIFY PARTY 1 ADDRESS 1           NOTIFY PARTY 1 ADDRESS 2                  
N02NOTIFY PARTY 1 ADDRESS 3           NOTIFY PARTY 1 ADDRESS 4                  
N03NOTIFY PARTY 1 CITYNY96632    US                                             
N04BOB THE BUILDER        TE1059654684               WP9686448456               
N00N2 NOTIFY PARTY 2 NAME                36ID876534                             
N02NOTIFY PARTY 2 ADDRESS 1           NOTIFY PARTY 2 ADDRESS 2                  
N02NOTIFY PARTY 2 ADDRESS 3           NOTIFY PARTY 2 ADDRESS 4                  
N03NOTIFY PARTY 2 CITYVI3005     AU                                             
N04BOB THE BUILDER        TE1059654684               WP9686448456               
I0162N INB12346 SD9823546954223212212965-63-5685 VINB53264                      
C01C1BDKLHB1     SEAL1          SEAL2          2020   21      22      20FRLCS   
C02VIN1234658                                                                   
D010000000010DESCRIPTION THAT IS LONG AND SHOULD BE AT LEA              PCE     
D010000000000ST TWO LINE AND NOT ONE LINE AGAIN I SAID DES                      
D010000000000CRIPTION THAT IS LONG AND SHOULD BE AT LEAST                       
D010000000000TWO LINE AND NOT ONE LINE                                          
D02MARKS AND NUMBERS THAT SHOULD BE IN AT LEAST                                 
D02TWO LINE AND NOT ONE LINE AGAIN I SAID MARKS                                 
D02AND NUMBERS THAT SHOULD BE IN AT LEAST TWO LI                                
D02NE AND NOT ONE LINE                                                          
D002013456875 000001230000032424KG                                              
D010000000010DESCRIPTION THAT IS LONG AND SHOULD BE AT LEA              PCE     
D010000000000ST TWO LINE AND NOT ONE LINE AGAIN I SAID DES                      
D010000000000CRIPTION THAT IS LONG AND SHOULD BE AT LEAST                       
D010000000000TWO LINE AND NOT ONE LINE                                          
D02MARKS AND NUMBERS THAT SHOULD BE IN AT LEAST                                 
D02TWO LINE AND NOT ONE LINE AGAIN I SAID MARKS                                 
D02AND NUMBERS THAT SHOULD BE IN AT LEAST TWO LI                                
D02NE AND NOT ONE LINE                                                          
V01UN114561  CL32U                              BOB THE BUILDER                 
V02                                                                             
V03                              SHIPPING NAME 1                                
V01R9658     CL69U                              WENDY THE DESTROYER             
V02                                                                             
V03                              SHIPPING NAME 2                                
V011381E     4.2 UPHOSPHORUS                    AARON                           
V02                                                                             
V03                                                                             
C01C2BDKLHB1     SEAL1          SEAL2          2020   21      22      20FRLCS   
C02VIN1234658                                                                   
D001010101012 000001230000032424KG                                              
D010000000010DESCRIPTION THAT IS LONG AND SHOULD BE AT LEA              PCE     
D010000000000ST TWO LINE AND NOT ONE LINE AGAIN I SAID DES                      
D010000000000CRIPTION THAT IS LONG AND SHOULD BE AT LEAST                       
D010000000000TWO LINE AND NOT ONE LINE                                          
D02MARKS AND NUMBERS THAT SHOULD BE IN AT LEAST                                 
D02TWO LINE AND NOT ONE LINE AGAIN I SAID MARKS                                 
D02AND NUMBERS THAT SHOULD BE IN AT LEAST TWO LI                                
D02NE AND NOT ONE LINE                                                          
D010000000010DESCRIPTION THAT IS LONG AND SHOULD BE AT LEA              PCE     
D010000000000ST TWO LINE AND NOT ONE LINE AGAIN I SAID DES                      
D010000000000CRIPTION THAT IS LONG AND SHOULD BE AT LEAST                       
D010000000000TWO LINE AND NOT ONE LINE                                          
D02MARKS AND NUMBERS THAT SHOULD BE IN AT LEAST                                 
D02TWO LINE AND NOT ONE LINE AGAIN I SAID MARKS                                 
D02AND NUMBERS THAT SHOULD BE IN AT LEAST TWO LI                                
D02NE AND NOT ONE LINE                                                          
V01UN114561  CL32UHAZRDOUS DESCRIPTION THAT IS LBOB THE BUILDER                 
V02032CEN                                                                       
V03ONG AND SHOULD BE AT LEAST TWOSHIPPING NAME 1                                
V03 LINE AND NOT ONE LINE                                                       
V01R9658     CL69UHAZRDOUS DESCRIPTION          WENDY THE DESTROYER             
V02045CE                                                                        
V03                              SHIPPING NAME 2                                
V011381E     4.2 UPHOSPHORUS                    AARON                           
V02                                                                             
V03                                                                             ";
			}
		}

		public void TestBuildSubsequentInBondOriginal()
		{
			var manifestMock = new Mock<IACEBillManifestMessageAttachee>();
			AMSInterfaceTestHelper.PopulateINPM01(manifestMock);

			var portMock = AMSInterfaceTestHelper.PopulateBasicPortMock();
			manifestMock.Setup(m => m.PortDetails).Returns(portMock.Object);

			ZString issuerCode = "BDKL";
			var billOfLadingMock = new Mock<IACEBillOfLading>();
			billOfLadingMock.Setup(m => m.BillActionCode).Returns(AMSBillSendingActionCodeList.Codes.AddBill);
			billOfLadingMock.Setup(m => m.IssuerCode).Returns(issuerCode);
			billOfLadingMock.Setup(m => m.BillOfLadingSequenceNumber).Returns("BDKLHB1");
			billOfLadingMock.Setup(m => m.SecondNotifyParty1).Returns("SC53");
			billOfLadingMock.Setup(m => m.SecondNotifyParty2).Returns("SC98");
			var shipmentReferenceDetail1Mock = new Mock<IShipmentReferenceDetail>();
			shipmentReferenceDetail1Mock.Setup(m => m.Qualifier).Returns("M");
			shipmentReferenceDetail1Mock.Setup(m => m.ReferenceIdentifier).Returns("MASTERXTN1");
			var shipmentReferenceDetail2Mock = new Mock<IShipmentReferenceDetail>();
			shipmentReferenceDetail2Mock.Setup(m => m.Qualifier).Returns("H");
			shipmentReferenceDetail2Mock.Setup(m => m.ReferenceIdentifier).Returns("HOUSEXTN1");
			billOfLadingMock.Setup(m => m.ShipmentReferenceDetails(It.IsAny<ActionCode>())).Returns(new IShipmentReferenceDetail[] { shipmentReferenceDetail1Mock.Object, shipmentReferenceDetail2Mock.Object });

			var entities = new Mock<IEntity>[]
			{
				new Mock<IEntity>(),
				new Mock<IEntity>(),
				new Mock<IEntity>(),
				new Mock<IEntity>()
			};

			entities[0].Setup(m => m.EntityCode).Returns("SH");
			entities[1].Setup(m => m.EntityCode).Returns("CN");
			entities[2].Setup(m => m.EntityCode).Returns("N1");
			entities[3].Setup(m => m.EntityCode).Returns("N2");

			billOfLadingMock.Setup(m => m.Entities(It.IsAny<ActionCode>())).Returns(entities.Select(x => x.Object));
			var inBOndMock = new Mock<IMovemenDetails>();
			inBOndMock.Setup(m => m.PreviousInBondNumber).Returns("ITPRV234");
			inBOndMock.Setup(m => m.InbondEntryType).Returns("62");
			inBOndMock.Setup(m => m.InBondQuantity).Returns(245);
			inBOndMock.Setup(m => m.IsBTAFDA).Returns(true);
			inBOndMock.Setup(m => m.ConventionalInbondNumber).Returns("INB12346");
			inBOndMock.Setup(m => m.InbondCarrierCode).Returns("SD98");
			inBOndMock.Setup(m => m.USPortOfDestination).Returns("2354");
			inBOndMock.Setup(m => m.ForeignDestination).Returns("69542");
			inBOndMock.Setup(m => m.Value).Returns(23212212);
			inBOndMock.Setup(m => m.BondedCarrierID).Returns("965-63-5685");
			inBOndMock.Setup(m => m.PaperlessInbondNumber).Returns("VINB53264");
			inBOndMock.Setup(m => m.ExportVesselName).Returns("EXPORT VESSEL NAME");
			billOfLadingMock.Setup(m => m.MovemenDetails).Returns(inBOndMock.Object);

			manifestMock.Setup(m => m.BillOfLadingDetails).Returns(billOfLadingMock.Object);
			var builder = new ACEAMSMessageBlockBuilder(manifestMock.Object, ActionCode.SubsequentInBondOriginal);
			var messageBlocks = new List<MessageBlock>(builder.Build());
			var messageDataBuilder = new ZStringBuilder();
			messageBlocks.ForEach(x => messageDataBuilder.Append(x.Serialise()));
			AssertMultilineASCIIEquals("Message Data", ExpectedSubsequentInBondOriginalMessage, messageDataBuilder.ToStringWithNewLineBetweenAppends());
			manifestMock.VerifyAll();
			portMock.VerifyAll();
			billOfLadingMock.VerifyAll();
			for (var i = 0; i < entities.Length; i++)
			{
				entities[i].VerifyAll();
			}
			inBOndMock.VerifyAll();
		}

		string ExpectedSubsequentInBondOriginalMessage
		{
			get
			{
				return @"M01SD2340AU                       V234      1      1234567                      
M02BDKLHB1_<MSG PLACEHOLDER>                                                    
P015946091871                                                                   
J01BDKL                                                                         
B03BDKLHB1                                ITPRV234   0000000245    SC53SC98     
B04M  MASTERXTN1                                                                
B04H  HOUSEXTN1                                                                 
I0162Y INB12346 SD9823546954223212212965-63-5685 VINB53264                      
I0210EXPORT VESSEL NAME                                                         ";
			}
		}

		public void TestBuildSubsequentInBondAmendmentMessage()
		{
			var manifestMock = new Mock<IACEBillManifestMessageAttachee>();
			AMSInterfaceTestHelper.PopulateINPM01(manifestMock);

			var portMock = AMSInterfaceTestHelper.PopulateBasicPortMock();
			manifestMock.Setup(m => m.PortDetails).Returns(portMock.Object);

			var billOfLadingMock = new Mock<IACEBillOfLading>();
			billOfLadingMock.Setup(m => m.BillActionCode).Returns(AMSBillSendingActionCodeList.Codes.AddBill);
			billOfLadingMock.Setup(m => m.AmendmentCode).Returns(AMSAmendmentCodeList.Codes._01);
			billOfLadingMock.Setup(m => m.IssuerCode).Returns("BDKL");
			billOfLadingMock.Setup(m => m.BillOfLadingSequenceNumber).Returns("BDKLHB1");
			billOfLadingMock.Setup(m => m.SecondNotifyParty1).Returns("SC53");
			billOfLadingMock.Setup(m => m.SecondNotifyParty2).Returns("SC98");
			var shipmentReferenceDetail1Mock = new Mock<IShipmentReferenceDetail>();
			shipmentReferenceDetail1Mock.Setup(m => m.Qualifier).Returns("M");
			shipmentReferenceDetail1Mock.Setup(m => m.ReferenceIdentifier).Returns("MASTERXTN1");
			var shipmentReferenceDetail2Mock = new Mock<IShipmentReferenceDetail>();
			shipmentReferenceDetail2Mock.Setup(m => m.Qualifier).Returns("H");
			shipmentReferenceDetail2Mock.Setup(m => m.ReferenceIdentifier).Returns("HOUSEXTN1");
			billOfLadingMock.Setup(m => m.ShipmentReferenceDetails(It.IsAny<ActionCode>())).Returns(new IShipmentReferenceDetail[] { shipmentReferenceDetail1Mock.Object, shipmentReferenceDetail2Mock.Object });

			var entity1Mock = AMSInterfaceTestHelper.GetEntity("SH", "SHIPPER NAME", "17", "ID23423", "SHIPPER ADDRESS 1", "SHIPPER ADDRESS 2", "SHIPPER ADDRESS 3", "SHIPPER ADDRESS 4", "SHIPPER CITY", "NS", "2000", "AU");
			var entity2Mock = AMSInterfaceTestHelper.GetEntity("CN", "CONSIGNEE NAME", "1", "ID89564", "CONSIGNEE ADDRESS 1", "CONSIGNEE ADDRESS 2", "CONSIGNEE ADDRESS 3", "CONSIGNEE ADDRESS 4", "CONSIGNEE CITY", "CA", "60025", "US");
			var entity3Mock = AMSInterfaceTestHelper.GetEntity("N1", "NOTIFY PARTY 1 NAME", "98", "ID986575", "NOTIFY PARTY 1 ADDRESS 1", "NOTIFY PARTY 1 ADDRESS 2", "NOTIFY PARTY 1 ADDRESS 3", "NOTIFY PARTY 1 ADDRESS 4", "NOTIFY PARTY 1 CITY", "NY", "96632", "US");
			var entity4Mock = AMSInterfaceTestHelper.GetEntity("N2", "NOTIFY PARTY 2 NAME", "36", "ID876534", "NOTIFY PARTY 2 ADDRESS 1", "NOTIFY PARTY 2 ADDRESS 2", "NOTIFY PARTY 2 ADDRESS 3", "NOTIFY PARTY 2 ADDRESS 4", "NOTIFY PARTY 2 CITY", "VI", "3005", "AU");
			billOfLadingMock.Setup(m => m.Entities(It.IsAny<ActionCode>())).Returns(new IEntity[] { entity1Mock.Object, entity2Mock.Object, entity3Mock.Object, entity4Mock.Object });
			var inBOndMock = new Mock<IMovemenDetails>();
			inBOndMock.Setup(m => m.PreviousInBondNumber).Returns("ITPRV234");
			inBOndMock.Setup(m => m.InbondEntryType).Returns("62");
			inBOndMock.Setup(m => m.InBondQuantity).Returns(245);

			inBOndMock.Setup(m => m.IsBTAFDA).Returns(true);
			inBOndMock.Setup(m => m.ConventionalInbondNumber).Returns("INB12346");
			inBOndMock.Setup(m => m.InbondCarrierCode).Returns("SD98");
			inBOndMock.Setup(m => m.USPortOfDestination).Returns("2354");
			inBOndMock.Setup(m => m.ForeignDestination).Returns("69542");
			inBOndMock.Setup(m => m.Value).Returns(23212212);
			inBOndMock.Setup(m => m.BondedCarrierID).Returns("965-63-5685");
			inBOndMock.Setup(m => m.PaperlessInbondNumber).Returns("VINB53264");
			inBOndMock.Setup(m => m.ExportVesselName).Returns("EXPORT VESSEL NAME");
			billOfLadingMock.Setup(m => m.MovemenDetails).Returns(inBOndMock.Object);

			manifestMock.Setup(m => m.BillOfLadingDetails).Returns(billOfLadingMock.Object);
			var builder = new ACEAMSMessageBlockBuilder(manifestMock.Object, ActionCode.SubsequentInBondAmendment);
			var messageBlocks = new List<MessageBlock>(builder.Build());
			var messageDataBuilder = new ZStringBuilder();
			messageBlocks.ForEach(x => messageDataBuilder.Append(x.Serialise()));
			AssertMultilineASCIIEquals("Message Data", ExpectedSubsequentInBondAmendmentMessage, messageDataBuilder.ToStringWithNewLineBetweenAppends());
			manifestMock.VerifyAll();
			portMock.VerifyAll();
			billOfLadingMock.VerifyAll();
			entity1Mock.VerifyAll();
			entity2Mock.VerifyAll();
			entity3Mock.VerifyAll();
			entity4Mock.VerifyAll();
			inBOndMock.VerifyAll();
		}

		string ExpectedSubsequentInBondAmendmentMessage
		{
			get
			{
				return @"M01SD2340AU                       V234      1      1234567                      
M02BDKLHB1_<MSG PLACEHOLDER>                                                    
P015946091871                                                                   
J01BDKL                                                                         
A01SD235946ABDKLHB1               01                                            
B03BDKLHB1                                ITPRV234   0000000245    SC53SC98     
B04M  MASTERXTN1                                                                
B04H  HOUSEXTN1                                                                 
N00SH SHIPPER NAME                       17ID23423                              
N02SHIPPER ADDRESS 1                  SHIPPER ADDRESS 2                         
N02SHIPPER ADDRESS 3                  SHIPPER ADDRESS 4                         
N03SHIPPER CITY       NS2000     AU                                             
N04BOB THE BUILDER        TE1059654684               WP9686448456               
N00CN CONSIGNEE NAME                     1 ID89564                              
N02CONSIGNEE ADDRESS 1                CONSIGNEE ADDRESS 2                       
N02CONSIGNEE ADDRESS 3                CONSIGNEE ADDRESS 4                       
N03CONSIGNEE CITY     CA60025    US                                             
N04BOB THE BUILDER        TE1059654684               WP9686448456               
N00N1 NOTIFY PARTY 1 NAME                98ID986575                             
N02NOTIFY PARTY 1 ADDRESS 1           NOTIFY PARTY 1 ADDRESS 2                  
N02NOTIFY PARTY 1 ADDRESS 3           NOTIFY PARTY 1 ADDRESS 4                  
N03NOTIFY PARTY 1 CITYNY96632    US                                             
N04BOB THE BUILDER        TE1059654684               WP9686448456               
N00N2 NOTIFY PARTY 2 NAME                36ID876534                             
N02NOTIFY PARTY 2 ADDRESS 1           NOTIFY PARTY 2 ADDRESS 2                  
N02NOTIFY PARTY 2 ADDRESS 3           NOTIFY PARTY 2 ADDRESS 4                  
N03NOTIFY PARTY 2 CITYVI3005     AU                                             
N04BOB THE BUILDER        TE1059654684               WP9686448456               
I0162Y INB12346 SD9823546954223212212965-63-5685 VINB53264                      
I0210EXPORT VESSEL NAME                                                         ";
			}
		}
	}
}
