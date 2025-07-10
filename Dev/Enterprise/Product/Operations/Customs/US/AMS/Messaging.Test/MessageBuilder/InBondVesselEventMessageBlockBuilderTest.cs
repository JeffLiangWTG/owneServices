using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.AMS.Messaging.Interface;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Moq;

namespace Enterprise.Customs.US.AMS.Messaging.Business.Testing
{
	class InBondVesselEventMessageBlockBuilderTest : TestCaseWithFactory
	{
		public void TestArriveInBondMessage()
		{
			ZString expectedMessage = @"-----------------INPM01-----------------
 Carrier Code (4-7)                :SD23
 Mode Of Transportation Code (8-9) :40
 Vessel Country Code (10-11)       :AU
 Voyage Number (35-39)             :V234
 Manifest Sequence Number (45-50)  :1
 Vessel Code (52-58)               :1234567

-----------------INPM02-----------------
 Carrier Assigned Batch Number (4-33) :BDKLHB1_<MSG PLACEHOLDER>

-----------------INPP01-----------------
 Port Of Unlading Code (4-7)    :5946
 Original Estimated Date (8-13) :18-Sep-71

-----------------ICMH01-----------------
 Message Code (4-4)                            :1
 Inbond Entity (5-18)                          :VINB53264
 Date (19-24)                                  :03-Feb-12
 C B P Port (25-28)                            :2354
 Time (33-38)                                  :1844
 F I R M S Location On In Bond Arrival (76-79) :Z456
";
			var billActionCode = InBondAndVesselEventMessageCodeList.Codes.ArriveInBond;

			var manifestMock = new Mock<IACEBillManifestMessageAttachee>();
			PopulateINPM01(manifestMock);

			PopulateINPP01(manifestMock);

			var billMock1 = PopulateINPM02(manifestMock, billActionCode);
			billMock1.Setup(m => m.FIRMS).Returns("Z456");

			var inBOndMock = new Mock<IMovemenDetails>();
			inBOndMock.Setup(m => m.USPortOfDestination).Returns("2354");
			inBOndMock.Setup(m => m.PaperlessInbondNumber).Returns("VINB53264");
			inBOndMock.Setup(m => m.ArrivalDateTime).Returns(new ZDateTime(2012, 2, 3, 18, 44, 53));
			billMock1.Setup(m => m.MovemenDetails).Returns(inBOndMock.Object);
			manifestMock.Setup(m => m.BillOfLadingDetails).Returns(billMock1.Object);
			var builder = new ZStringBuilder();
			foreach (var messageBlock in new InBondVesselEventMessageBlockBuilder(manifestMock.Object).Build())
			{
				builder.Append(messageBlock.Serialise(true));
			}

			AssertMultilineASCIIEquals("Expected Message for " + billActionCode, expectedMessage, builder.ToStringWithNewLineBetweenAppends());

			manifestMock.VerifyAll();
			billMock1.VerifyAll();
			inBOndMock.VerifyAll();
		}

		public void TestArriveInBondByBillOfLadingMessage()
		{
			var billActionCode = InBondAndVesselEventMessageCodeList.Codes.ArriveInBondByBillOfLading;
			ZString expectedMessage = @"-----------------INPM01-----------------
 Carrier Code (4-7)                :SD23
 Mode Of Transportation Code (8-9) :40
 Vessel Country Code (10-11)       :AU
 Voyage Number (35-39)             :V234
 Manifest Sequence Number (45-50)  :1
 Vessel Code (52-58)               :1234567

-----------------INPM02-----------------
 Carrier Assigned Batch Number (4-33) :BDKLHB1_<MSG PLACEHOLDER>

-----------------INPP01-----------------
 Port Of Unlading Code (4-7)    :5946
 Original Estimated Date (8-13) :18-Sep-71

-----------------ICMH01-----------------
 Message Code (4-4)                            :2
 Inbond Entity (5-18)                          :BDKLHB1
 Date (19-24)                                  :03-Feb-12
 C B P Port (25-28)                            :2354
 Issuer Code (29-32)                           :BDKL
 Time (33-38)                                  :1844
 F I R M S Location On In Bond Arrival (76-79) :Z456
";

			var manifestMock = new Mock<IACEBillManifestMessageAttachee>();
			PopulateINPM01(manifestMock);

			PopulateINPP01(manifestMock);

			var billMock1 = PopulateINPM02(manifestMock, billActionCode);

			billMock1.Setup(m => m.IssuerCode).Returns("BDKL");
			billMock1.Setup(m => m.FIRMS).Returns("Z456");

			var inBOndMock = new Mock<IMovemenDetails>();
			inBOndMock.Setup(m => m.USPortOfDestination).Returns("2354");
			inBOndMock.Setup(m => m.ArrivalDateTime).Returns(new ZDateTime(2012, 2, 3, 18, 44, 53));
			billMock1.Setup(m => m.MovemenDetails).Returns(inBOndMock.Object);

			manifestMock.Setup(m => m.BillOfLadingDetails).Returns(billMock1.Object);
			var builder = new ZStringBuilder();
			foreach (var messageBlock in new InBondVesselEventMessageBlockBuilder(manifestMock.Object).Build())
			{
				builder.Append(messageBlock.Serialise(true));
			}

			AssertMultilineASCIIEquals("Expected Message for " + billActionCode, expectedMessage, builder.ToStringWithNewLineBetweenAppends());

			manifestMock.VerifyAll();
			billMock1.VerifyAll();
			inBOndMock.VerifyAll();
		}

		public void TestArriveInBondByContainerMessage()
		{
			ZString expectedMessage = @"-----------------INPM01-----------------
 Carrier Code (4-7)                :SD23
 Mode Of Transportation Code (8-9) :40
 Vessel Country Code (10-11)       :AU
 Voyage Number (35-39)             :V234
 Manifest Sequence Number (45-50)  :1
 Vessel Code (52-58)               :1234567

-----------------INPM02-----------------
 Carrier Assigned Batch Number (4-33) :BDKLHB1_<MSG PLACEHOLDER>

-----------------INPP01-----------------
 Port Of Unlading Code (4-7)    :5946
 Original Estimated Date (8-13) :18-Sep-71

-----------------ICMH01-----------------
 Message Code (4-4)                            :3
 Inbond Entity (5-18)                          :C1BDKLHB1
 Date (19-24)                                  :03-Feb-12
 C B P Port (25-28)                            :2354
 Time (33-38)                                  :1844
 F I R M S Location On In Bond Arrival (76-79) :Z456

-----------------ICMH02-----------------
 Reference Identifier Qualifier (4-5) :IB
 Reference Identifier (6-21)          :VINB53264
";

			var billActionCode = InBondAndVesselEventMessageCodeList.Codes.ArriveInBondByContainer;

			var manifestMock = new Mock<IACEBillManifestMessageAttachee>();
			PopulateINPM01(manifestMock);

			PopulateINPP01(manifestMock);

			var billMock1 = PopulateINPM02(manifestMock, billActionCode);

			billMock1.Setup(m => m.FIRMS).Returns("Z456");

			var inBOndMock = new Mock<IMovemenDetails>();
			inBOndMock.Setup(m => m.USPortOfDestination).Returns("2354");
			inBOndMock.Setup(m => m.PaperlessInbondNumber).Returns("VINB53264");
			inBOndMock.Setup(m => m.ArrivalDateTime).Returns(new ZDateTime(2012, 2, 3, 18, 44, 53));
			billMock1.Setup(m => m.MovemenDetails).Returns(inBOndMock.Object);
			var container1Mock = PopulateContainer("C1BDKLHB1");
			var container2Mock = PopulateContainer();
			billMock1.Setup(m => m.Containers).Returns(new IACEContainer[] { container1Mock.Object, container2Mock.Object });

			manifestMock.Setup(m => m.BillOfLadingDetails).Returns(billMock1.Object);
			var builder = new ZStringBuilder();
			foreach (var messageBlock in new InBondVesselEventMessageBlockBuilder(manifestMock.Object).Build())
			{
				builder.Append(messageBlock.Serialise(true));
			}

			AssertMultilineASCIIEquals("Expected Message for " + billActionCode, expectedMessage, builder.ToStringWithNewLineBetweenAppends());

			manifestMock.VerifyAll();
			billMock1.VerifyAll();
			inBOndMock.VerifyAll();
			container1Mock.VerifyAll();
			container2Mock.VerifyAll();
		}

		public void TestVesselArrivalMessage()
		{
			ZString expectedMessage = @"-----------------INPM01-----------------
 Carrier Code (4-7)                :SD23
 Mode Of Transportation Code (8-9) :40
 Vessel Country Code (10-11)       :AU
 Voyage Number (35-39)             :V234
 Manifest Sequence Number (45-50)  :1
 Vessel Code (52-58)               :1234567

-----------------INPM02-----------------
 Carrier Assigned Batch Number (4-33) :BDKLHB1_<MSG PLACEHOLDER>

-----------------INPP01-----------------
 Port Of Unlading Code (4-7)    :5946
 Original Estimated Date (8-13) :18-Sep-71

-----------------ICMH01-----------------
 Message Code (4-4) :4
 Date (19-24)       :07-May-12
 C B P Port (25-28) :5946
 Time (33-38)       :1305
";
			var billActionCode = InBondAndVesselEventMessageCodeList.Codes.VesselArrival;

			var manifestMock = new Mock<IACEBillManifestMessageAttachee>();
			PopulateINPM01(manifestMock);

			PopulateINPP01(manifestMock);

			var billMock1 = PopulateINPM02(manifestMock, billActionCode);
			manifestMock.Setup(m => m.EventDateTime).Returns(new ZDateTime(2012, 05, 07, 13, 05, 02));

			manifestMock.Setup(m => m.BillOfLadingDetails).Returns(billMock1.Object);
			var builder = new ZStringBuilder();
			foreach (var messageBlock in new InBondVesselEventMessageBlockBuilder(manifestMock.Object).Build())
			{
				builder.Append(messageBlock.Serialise(true));
			}

			AssertMultilineASCIIEquals("Expected Message for " + billActionCode, expectedMessage, builder.ToStringWithNewLineBetweenAppends());

			manifestMock.VerifyAll();
			billMock1.VerifyAll();
		}

		public void TestExportInBondMessage()
		{
			ZString expectedMessage = @"-----------------INPM01-----------------
 Carrier Code (4-7)                :SD23
 Mode Of Transportation Code (8-9) :40
 Vessel Country Code (10-11)       :AU
 Voyage Number (35-39)             :V234
 Manifest Sequence Number (45-50)  :1
 Vessel Code (52-58)               :1234567

-----------------INPM02-----------------
 Carrier Assigned Batch Number (4-33) :BDKLHB1_<MSG PLACEHOLDER>

-----------------INPP01-----------------
 Port Of Unlading Code (4-7)    :5946
 Original Estimated Date (8-13) :18-Sep-71

-----------------ICMH01-----------------
 Message Code (4-4)   :5
 Inbond Entity (5-18) :VINB53264
 Date (19-24)         :06-Apr-13
 Time (33-38)         :1736

-----------------ICMH02-----------------
 Vessel Name (22-44)           :EXPORT VESSEL NAME
 Transportation Method (45-45) :S
";
			var billActionCode = InBondAndVesselEventMessageCodeList.Codes.ExportInBond;

			var manifestMock = new Mock<IACEBillManifestMessageAttachee>();
			PopulateINPM01(manifestMock);

			PopulateINPP01(manifestMock);

			var billMock1 = PopulateINPM02(manifestMock, billActionCode);

			var inBOndMock = new Mock<IMovemenDetails>();
			inBOndMock.Setup(m => m.PaperlessInbondNumber).Returns("VINB53264");
			inBOndMock.Setup(m => m.ExportVesselName).Returns("EXPORT VESSEL NAME");
			inBOndMock.Setup(m => m.ExportDateTime).Returns(new ZDateTime(2013, 4, 6, 17, 36, 46));
			billMock1.Setup(m => m.MovemenDetails).Returns(inBOndMock.Object);

			manifestMock.Setup(m => m.BillOfLadingDetails).Returns(billMock1.Object);
			var builder = new ZStringBuilder();
			foreach (var messageBlock in new InBondVesselEventMessageBlockBuilder(manifestMock.Object).Build())
			{
				builder.Append(messageBlock.Serialise(true));
			}

			AssertMultilineASCIIEquals("Expected Message for " + billActionCode, expectedMessage, builder.ToStringWithNewLineBetweenAppends());

			manifestMock.VerifyAll();
			billMock1.VerifyAll();
			inBOndMock.VerifyAll();
		}

		public void TestExportInBondByBillOfLadingMessage()
		{
			ZString expectedMessage = @"-----------------INPM01-----------------
 Carrier Code (4-7)                :SD23
 Mode Of Transportation Code (8-9) :40
 Vessel Country Code (10-11)       :AU
 Voyage Number (35-39)             :V234
 Manifest Sequence Number (45-50)  :1
 Vessel Code (52-58)               :1234567

-----------------INPM02-----------------
 Carrier Assigned Batch Number (4-33) :BDKLHB1_<MSG PLACEHOLDER>

-----------------INPP01-----------------
 Port Of Unlading Code (4-7)    :5946
 Original Estimated Date (8-13) :18-Sep-71

-----------------ICMH01-----------------
 Message Code (4-4)   :6
 Inbond Entity (5-18) :BDKLHB1
 Date (19-24)         :06-Apr-13
 Issuer Code (29-32)  :BDKL
 Time (33-38)         :1736

-----------------ICMH02-----------------
 Vessel Name (22-44)           :EXPORT VESSEL NAME
 Transportation Method (45-45) :S
";
			var billActionCode = InBondAndVesselEventMessageCodeList.Codes.ExportInBondByBillOfLading;

			var manifestMock = new Mock<IACEBillManifestMessageAttachee>();
			PopulateINPM01(manifestMock);

			PopulateINPP01(manifestMock);

			var billMock1 = PopulateINPM02(manifestMock, billActionCode);
			billMock1.Setup(m => m.IssuerCode).Returns("BDKL");

			var inBOndMock = new Mock<IMovemenDetails>();
			inBOndMock.Setup(m => m.ExportVesselName).Returns("EXPORT VESSEL NAME");
			inBOndMock.Setup(m => m.ExportDateTime).Returns(new ZDateTime(2013, 4, 6, 17, 36, 46));
			billMock1.Setup(m => m.MovemenDetails).Returns(inBOndMock.Object);

			manifestMock.Setup(m => m.BillOfLadingDetails).Returns(billMock1.Object);
			var builder = new ZStringBuilder();
			foreach (var messageBlock in new InBondVesselEventMessageBlockBuilder(manifestMock.Object).Build())
			{
				builder.Append(messageBlock.Serialise(true));
			}

			AssertMultilineASCIIEquals("Expected Message for " + billActionCode, expectedMessage, builder.ToStringWithNewLineBetweenAppends());

			manifestMock.VerifyAll();
			billMock1.VerifyAll();
			inBOndMock.VerifyAll();
		}

		public void TestExportInBondByContainerMessage()
		{
			ZString expectedMessage = @"-----------------INPM01-----------------
 Carrier Code (4-7)                :SD23
 Mode Of Transportation Code (8-9) :40
 Vessel Country Code (10-11)       :AU
 Voyage Number (35-39)             :V234
 Manifest Sequence Number (45-50)  :1
 Vessel Code (52-58)               :1234567

-----------------INPM02-----------------
 Carrier Assigned Batch Number (4-33) :BDKLHB1_<MSG PLACEHOLDER>

-----------------INPP01-----------------
 Port Of Unlading Code (4-7)    :5946
 Original Estimated Date (8-13) :18-Sep-71

-----------------ICMH01-----------------
 Message Code (4-4)   :7
 Inbond Entity (5-18) :C1BDKLHB1
 Date (19-24)         :06-Apr-13
 Time (33-38)         :1736

-----------------ICMH02-----------------
 Reference Identifier Qualifier (4-5) :IB
 Reference Identifier (6-21)          :VINB53264
 Vessel Name (22-44)                  :EXPORT VESSEL NAME
 Transportation Method (45-45)        :S
";
			var billActionCode = InBondAndVesselEventMessageCodeList.Codes.ExportInBondByContainer;

			var manifestMock = new Mock<IACEBillManifestMessageAttachee>();
			PopulateINPM01(manifestMock);

			PopulateINPP01(manifestMock);

			var billMock1 = PopulateINPM02(manifestMock, billActionCode);

			var inBOndMock = new Mock<IMovemenDetails>();
			inBOndMock.Setup(m => m.PaperlessInbondNumber).Returns("VINB53264");
			inBOndMock.Setup(m => m.ExportVesselName).Returns("EXPORT VESSEL NAME");
			inBOndMock.Setup(m => m.ExportDateTime).Returns(new ZDateTime(2013, 4, 6, 17, 36, 46));
			billMock1.Setup(m => m.MovemenDetails).Returns(inBOndMock.Object);

			var container1Mock = PopulateContainer("C1BDKLHB1");
			var container2Mock = PopulateContainer();

			billMock1.Setup(m => m.Containers).Returns(new IACEContainer[] { container1Mock.Object, container2Mock.Object });

			manifestMock.Setup(m => m.BillOfLadingDetails).Returns(billMock1.Object);
			var builder = new ZStringBuilder();
			foreach (var messageBlock in new InBondVesselEventMessageBlockBuilder(manifestMock.Object).Build())
			{
				builder.Append(messageBlock.Serialise(true));
			}

			AssertMultilineASCIIEquals("Expected Message for " + billActionCode, expectedMessage, builder.ToStringWithNewLineBetweenAppends());

			manifestMock.VerifyAll();
			billMock1.VerifyAll();
			inBOndMock.VerifyAll();
			container1Mock.VerifyAll();
			container2Mock.VerifyAll();
		}

		public void TestVesselDepartureMessage()
		{
			ZString expectedMessage = @"-----------------INPM01-----------------
 Carrier Code (4-7)                :SD23
 Mode Of Transportation Code (8-9) :40
 Vessel Country Code (10-11)       :AU
 Voyage Number (35-39)             :V234
 Manifest Sequence Number (45-50)  :1
 Vessel Code (52-58)               :1234567

-----------------INPM02-----------------
 Carrier Assigned Batch Number (4-33) :BDKLHB1_<MSG PLACEHOLDER>

-----------------INPP01-----------------
 Port Of Unlading Code (4-7)    :5946
 Original Estimated Date (8-13) :18-Sep-71

-----------------ICMH01-----------------
 Message Code (4-4) :9
 Date (19-24)       :07-May-12
 Time (33-38)       :1305

-----------------ICMH02-----------------
 Foreign Departure Port (46-50) :51023
";
			var billActionCode = InBondAndVesselEventMessageCodeList.Codes.VesselDeparture;

			var manifestMock = new Mock<IACEBillManifestMessageAttachee>();
			PopulateINPM01(manifestMock);

			PopulateINPP01(manifestMock);

			var billMock1 = PopulateINPM02(manifestMock, billActionCode);
			manifestMock.Setup(m => m.EventDateTime).Returns(new ZDateTime(2012, 05, 07, 13, 05, 02));
			manifestMock.Setup(m => m.ForeignDeparturePort).Returns("51023");

			manifestMock.Setup(m => m.BillOfLadingDetails).Returns(billMock1.Object);
			var builder = new ZStringBuilder();
			foreach (var messageBlock in new InBondVesselEventMessageBlockBuilder(manifestMock.Object).Build())
			{
				builder.Append(messageBlock.Serialise(true));
			}

			AssertMultilineASCIIEquals("Expected Message for " + billActionCode, expectedMessage, builder.ToStringWithNewLineBetweenAppends());

			manifestMock.VerifyAll();
			billMock1.VerifyAll();
		}

		public void TestVesselDepartureMessageWithInvalidDate()
		{
			ZString expectedMessage = @"-----------------INPM01-----------------
 Carrier Code (4-7)                :SD23
 Mode Of Transportation Code (8-9) :40
 Vessel Country Code (10-11)       :AU
 Voyage Number (35-39)             :V234
 Manifest Sequence Number (45-50)  :1
 Vessel Code (52-58)               :1234567

-----------------INPM02-----------------
 Carrier Assigned Batch Number (4-33) :BDKLHB1_<MSG PLACEHOLDER>

-----------------INPP01-----------------
 Port Of Unlading Code (4-7)    :5946
 Original Estimated Date (8-13) :18-Sep-71

-----------------ICMH01-----------------
 Message Code (4-4) :9

-----------------ICMH02-----------------
 Foreign Departure Port (46-50) :51023
";
			var billActionCode = InBondAndVesselEventMessageCodeList.Codes.VesselDeparture;

			var manifestMock = new Mock<IACEBillManifestMessageAttachee>();
			PopulateINPM01(manifestMock);

			PopulateINPP01(manifestMock);

			var billMock1 = PopulateINPM02(manifestMock, billActionCode);
			manifestMock.Setup(m => m.EventDateTime).Returns(ZDateTime.Invalid);
			manifestMock.Setup(m => m.ForeignDeparturePort).Returns("51023");

			manifestMock.Setup(m => m.BillOfLadingDetails).Returns(billMock1.Object);

			var builder = new ZStringBuilder();
			foreach (var messageBlock in new InBondVesselEventMessageBlockBuilder(manifestMock.Object).Build())
			{
				builder.Append(messageBlock.Serialise(true));
			}

			AssertMultilineASCIIEquals("Expected Message for " + billActionCode, expectedMessage, builder.ToStringWithNewLineBetweenAppends());

			manifestMock.VerifyAll();
			billMock1.VerifyAll();
		}

		public void TestTransferOfInBondLiabilityMessage()
		{
			ZString expectedMessage = @"-----------------INPM01-----------------
 Carrier Code (4-7)                :SD23
 Mode Of Transportation Code (8-9) :40
 Vessel Country Code (10-11)       :AU
 Voyage Number (35-39)             :V234
 Manifest Sequence Number (45-50)  :1
 Vessel Code (52-58)               :1234567

-----------------INPM02-----------------
 Carrier Assigned Batch Number (4-33) :BDKLHB1_<MSG PLACEHOLDER>

-----------------INPP01-----------------
 Port Of Unlading Code (4-7)    :5946
 Original Estimated Date (8-13) :18-Sep-71

-----------------ICMH01-----------------
 Message Code (4-4)          :A
 Inbond Entity (5-18)        :VINB53264
 Date (19-24)                :09-Jul-12
 Time (33-38)                :1022
 Inbond Carrier Code (39-42) :TOL1
 Bonded Carrier I D (43-54)  :TOL-INBCARID
 City Name (55-73)           :BOB'S CITY
 State Code (74-75)          :CA
";
			var billActionCode = InBondAndVesselEventMessageCodeList.Codes.TransferOfInBondLiability;

			var manifestMock = new Mock<IACEBillManifestMessageAttachee>();
			PopulateINPM01(manifestMock);

			PopulateINPP01(manifestMock);

			var billMock1 = PopulateINPM02(manifestMock, billActionCode);

			var inBOndMock = new Mock<IMovemenDetails>();
			inBOndMock.Setup(m => m.PaperlessInbondNumber).Returns("VINB53264");
			inBOndMock.Setup(m => m.TOLInBondCarrierCode).Returns("TOL1");
			inBOndMock.Setup(m => m.TOLBondedCarrierID).Returns("TOL-INBCARID");
			inBOndMock.Setup(m => m.TOLDateTime).Returns(new ZDateTime(2012, 7, 9, 10, 22, 8));
			inBOndMock.Setup(m => m.TOLCityName).Returns("BOB'S CITY");
			inBOndMock.Setup(m => m.TOLStateCode).Returns("CA");
			billMock1.Setup(m => m.MovemenDetails).Returns(inBOndMock.Object);

			manifestMock.Setup(m => m.BillOfLadingDetails).Returns(billMock1.Object);
			var builder = new ZStringBuilder();
			foreach (var messageBlock in new InBondVesselEventMessageBlockBuilder(manifestMock.Object).Build())
			{
				builder.Append(messageBlock.Serialise(true));
			}

			AssertMultilineASCIIEquals("Expected Message for " + billActionCode, expectedMessage, builder.ToStringWithNewLineBetweenAppends());

			manifestMock.VerifyAll();
			billMock1.VerifyAll();
			inBOndMock.VerifyAll();
		}

		public void TestCancelInBondArrivalMessage()
		{
			ZString expectedMessage = @"-----------------INPM01-----------------
 Carrier Code (4-7)                :SD23
 Mode Of Transportation Code (8-9) :40
 Vessel Country Code (10-11)       :AU
 Voyage Number (35-39)             :V234
 Manifest Sequence Number (45-50)  :1
 Vessel Code (52-58)               :1234567

-----------------INPM02-----------------
 Carrier Assigned Batch Number (4-33) :BDKLHB1_<MSG PLACEHOLDER>

-----------------INPP01-----------------
 Port Of Unlading Code (4-7)    :5946
 Original Estimated Date (8-13) :18-Sep-71

-----------------ICMH01-----------------
 Message Code (4-4)   :F
 Inbond Entity (5-18) :VINB53264
 Date (19-24)         :03-Feb-12
 Time (33-38)         :1844
";

			var billActionCode = InBondAndVesselEventMessageCodeList.Codes.CancelInBondArrival;

			var manifestMock = new Mock<IACEBillManifestMessageAttachee>();
			PopulateINPM01(manifestMock);

			PopulateINPP01(manifestMock);

			var billMock1 = PopulateINPM02(manifestMock, billActionCode);

			var inBOndMock = new Mock<IMovemenDetails>();
			inBOndMock.Setup(m => m.PaperlessInbondNumber).Returns("VINB53264");
			inBOndMock.Setup(m => m.ArrivalDateTime).Returns(new ZDateTime(2012, 2, 3, 18, 44, 53));
			billMock1.Setup(m => m.MovemenDetails).Returns(inBOndMock.Object);

			manifestMock.Setup(m => m.BillOfLadingDetails).Returns(billMock1.Object);
			var builder = new ZStringBuilder();
			foreach (var messageBlock in new InBondVesselEventMessageBlockBuilder(manifestMock.Object).Build())
			{
				builder.Append(messageBlock.Serialise(true));
			}

			AssertMultilineASCIIEquals("Expected Message for " + billActionCode, expectedMessage, builder.ToStringWithNewLineBetweenAppends());

			manifestMock.VerifyAll();
			billMock1.VerifyAll();
			inBOndMock.VerifyAll();
		}

		public void TestCancelInBondArrivalByBillOfLadingMessage()
		{
			ZString expectedMessage = @"-----------------INPM01-----------------
 Carrier Code (4-7)                :SD23
 Mode Of Transportation Code (8-9) :40
 Vessel Country Code (10-11)       :AU
 Voyage Number (35-39)             :V234
 Manifest Sequence Number (45-50)  :1
 Vessel Code (52-58)               :1234567

-----------------INPM02-----------------
 Carrier Assigned Batch Number (4-33) :BDKLHB1_<MSG PLACEHOLDER>

-----------------INPP01-----------------
 Port Of Unlading Code (4-7)    :5946
 Original Estimated Date (8-13) :18-Sep-71

-----------------ICMH01-----------------
 Message Code (4-4)   :G
 Inbond Entity (5-18) :BDKLHB1
 Date (19-24)         :03-Feb-12
 Issuer Code (29-32)  :BDKL
 Time (33-38)         :1844
";
			var billActionCode = InBondAndVesselEventMessageCodeList.Codes.CancelInBondArrivalByBillOfLading;

			var manifestMock = new Mock<IACEBillManifestMessageAttachee>();
			PopulateINPM01(manifestMock);

			PopulateINPP01(manifestMock);

			var billMock1 = PopulateINPM02(manifestMock, billActionCode);
			billMock1.Setup(m => m.IssuerCode).Returns("BDKL");

			var inBOndMock = new Mock<IMovemenDetails>();
			inBOndMock.Setup(m => m.ArrivalDateTime).Returns(new ZDateTime(2012, 2, 3, 18, 44, 53));
			billMock1.Setup(m => m.MovemenDetails).Returns(inBOndMock.Object);

			manifestMock.Setup(m => m.BillOfLadingDetails).Returns(billMock1.Object);
			var builder = new ZStringBuilder();
			foreach (var messageBlock in new InBondVesselEventMessageBlockBuilder(manifestMock.Object).Build())
			{
				builder.Append(messageBlock.Serialise(true));
			}

			AssertMultilineASCIIEquals("Expected Message for " + billActionCode, expectedMessage, builder.ToStringWithNewLineBetweenAppends());

			manifestMock.VerifyAll();
			billMock1.VerifyAll();
			inBOndMock.VerifyAll();
		}

		public void TestCancelInBondArrivalByContainerMessage()
		{
			ZString expectedMessage = @"-----------------INPM01-----------------
 Carrier Code (4-7)                :SD23
 Mode Of Transportation Code (8-9) :40
 Vessel Country Code (10-11)       :AU
 Voyage Number (35-39)             :V234
 Manifest Sequence Number (45-50)  :1
 Vessel Code (52-58)               :1234567

-----------------INPM02-----------------
 Carrier Assigned Batch Number (4-33) :BDKLHB1_<MSG PLACEHOLDER>

-----------------INPP01-----------------
 Port Of Unlading Code (4-7)    :5946
 Original Estimated Date (8-13) :18-Sep-71

-----------------ICMH01-----------------
 Message Code (4-4)   :H
 Inbond Entity (5-18) :C1BDKLHB1
 Date (19-24)         :03-Feb-12
 Time (33-38)         :1844

-----------------ICMH02-----------------
 Reference Identifier Qualifier (4-5) :IB
 Reference Identifier (6-21)          :VINB53264
";

			var billActionCode = InBondAndVesselEventMessageCodeList.Codes.CancelInBondArrivalByContainer;

			var manifestMock = new Mock<IACEBillManifestMessageAttachee>();
			PopulateINPM01(manifestMock);

			PopulateINPP01(manifestMock);

			var billMock1 = PopulateINPM02(manifestMock, billActionCode);

			var inBOndMock = new Mock<IMovemenDetails>();

			inBOndMock.Setup(m => m.PaperlessInbondNumber).Returns("VINB53264");
			inBOndMock.Setup(m => m.ArrivalDateTime).Returns(new ZDateTime(2012, 2, 3, 18, 44, 53));
			billMock1.Setup(m => m.MovemenDetails).Returns(inBOndMock.Object);
			var container1Mock = AMSInterfaceTestHelper.GetACEContainer("C1" + "BDKLHB1", true);
			var container2Mock = AMSInterfaceTestHelper.GetACEContainer("C2" + "BDKLHB1", false);
			billMock1.Setup(m => m.Containers).Returns(new IACEContainer[] { container1Mock.Object, container2Mock.Object });

			manifestMock.Setup(m => m.BillOfLadingDetails).Returns(billMock1.Object);
			var builder = new ZStringBuilder();
			foreach (var messageBlock in new InBondVesselEventMessageBlockBuilder(manifestMock.Object).Build())
			{
				builder.Append(messageBlock.Serialise(true));
			}

			AssertMultilineASCIIEquals("Expected Message for " + billActionCode, expectedMessage, builder.ToStringWithNewLineBetweenAppends());

			manifestMock.VerifyAll();
			billMock1.VerifyAll();
			inBOndMock.VerifyAll();
		}

		public void TestCancelInBondExportMessage()
		{
			ZString expectedMessage = @"-----------------INPM01-----------------
 Carrier Code (4-7)                :SD23
 Mode Of Transportation Code (8-9) :40
 Vessel Country Code (10-11)       :AU
 Voyage Number (35-39)             :V234
 Manifest Sequence Number (45-50)  :1
 Vessel Code (52-58)               :1234567

-----------------INPM02-----------------
 Carrier Assigned Batch Number (4-33) :BDKLHB1_<MSG PLACEHOLDER>

-----------------INPP01-----------------
 Port Of Unlading Code (4-7)    :5946
 Original Estimated Date (8-13) :18-Sep-71

-----------------ICMH01-----------------
 Message Code (4-4)   :I
 Inbond Entity (5-18) :VINB53264
 Date (19-24)         :06-Apr-13
 Time (33-38)         :1736
";
			var billActionCode = InBondAndVesselEventMessageCodeList.Codes.CancelInBondExport;

			var manifestMock = new Mock<IACEBillManifestMessageAttachee>();
			PopulateINPM01(manifestMock);

			PopulateINPP01(manifestMock);

			var billMock1 = PopulateINPM02(manifestMock, billActionCode);

			var inBOndMock = new Mock<IMovemenDetails>();
			inBOndMock.Setup(m => m.PaperlessInbondNumber).Returns("VINB53264");
			inBOndMock.Setup(m => m.ExportDateTime).Returns(new ZDateTime(2013, 4, 6, 17, 36, 46));
			billMock1.Setup(m => m.MovemenDetails).Returns(inBOndMock.Object);

			manifestMock.Setup(m => m.BillOfLadingDetails).Returns(billMock1.Object);
			var builder = new ZStringBuilder();
			foreach (var messageBlock in new InBondVesselEventMessageBlockBuilder(manifestMock.Object).Build())
			{
				builder.Append(messageBlock.Serialise(true));
			}

			AssertMultilineASCIIEquals("Expected Message for " + billActionCode, expectedMessage, builder.ToStringWithNewLineBetweenAppends());

			manifestMock.VerifyAll();
			billMock1.VerifyAll();
			inBOndMock.VerifyAll();
		}

		public void TestCancelInBondExportByBillOfLadingMessage()
		{
			ZString expectedMessage = @"-----------------INPM01-----------------
 Carrier Code (4-7)                :SD23
 Mode Of Transportation Code (8-9) :40
 Vessel Country Code (10-11)       :AU
 Voyage Number (35-39)             :V234
 Manifest Sequence Number (45-50)  :1
 Vessel Code (52-58)               :1234567

-----------------INPM02-----------------
 Carrier Assigned Batch Number (4-33) :BDKLHB1_<MSG PLACEHOLDER>

-----------------INPP01-----------------
 Port Of Unlading Code (4-7)    :5946
 Original Estimated Date (8-13) :18-Sep-71

-----------------ICMH01-----------------
 Message Code (4-4)   :J
 Inbond Entity (5-18) :BDKLHB1
 Date (19-24)         :06-Apr-13
 Issuer Code (29-32)  :BDKL
 Time (33-38)         :1736
";
			var billActionCode = InBondAndVesselEventMessageCodeList.Codes.CancelInBondExportByBillOfLading;

			var manifestMock = new Mock<IACEBillManifestMessageAttachee>();
			PopulateINPM01(manifestMock);

			PopulateINPP01(manifestMock);

			var billMock1 = PopulateINPM02(manifestMock, billActionCode);
			billMock1.Setup(m => m.IssuerCode).Returns("BDKL");

			var inBOndMock = new Mock<IMovemenDetails>();
			inBOndMock.Setup(m => m.ExportDateTime).Returns(new ZDateTime(2013, 4, 6, 17, 36, 46));
			billMock1.Setup(m => m.MovemenDetails).Returns(inBOndMock.Object);

			manifestMock.Setup(m => m.BillOfLadingDetails).Returns(billMock1.Object);
			var builder = new ZStringBuilder();
			foreach (var messageBlock in new InBondVesselEventMessageBlockBuilder(manifestMock.Object).Build())
			{
				builder.Append(messageBlock.Serialise(true));
			}

			AssertMultilineASCIIEquals("Expected Message for " + billActionCode, expectedMessage, builder.ToStringWithNewLineBetweenAppends());

			manifestMock.VerifyAll();
			billMock1.VerifyAll();
			inBOndMock.VerifyAll();
		}

		public void TestCancelInBondExportByContainerMessage()
		{
			ZString expectedMessage = @"-----------------INPM01-----------------
 Carrier Code (4-7)                :SD23
 Mode Of Transportation Code (8-9) :40
 Vessel Country Code (10-11)       :AU
 Voyage Number (35-39)             :V234
 Manifest Sequence Number (45-50)  :1
 Vessel Code (52-58)               :1234567

-----------------INPM02-----------------
 Carrier Assigned Batch Number (4-33) :BDKLHB1_<MSG PLACEHOLDER>

-----------------INPP01-----------------
 Port Of Unlading Code (4-7)    :5946
 Original Estimated Date (8-13) :18-Sep-71

-----------------ICMH01-----------------
 Message Code (4-4)   :K
 Inbond Entity (5-18) :C1BDKLHB1
 Date (19-24)         :06-Apr-13
 Time (33-38)         :1736

-----------------ICMH02-----------------
 Reference Identifier Qualifier (4-5) :IB
 Reference Identifier (6-21)          :VINB53264
";
			var billActionCode = InBondAndVesselEventMessageCodeList.Codes.CancelInBondExportByContainer;

			var manifestMock = new Mock<IACEBillManifestMessageAttachee>();
			PopulateINPM01(manifestMock);

			PopulateINPP01(manifestMock);

			var billMock1 = PopulateINPM02(manifestMock, billActionCode);

			var inBOndMock = new Mock<IMovemenDetails>();
			inBOndMock.Setup(m => m.PaperlessInbondNumber).Returns("VINB53264");
			inBOndMock.Setup(m => m.ExportDateTime).Returns(new ZDateTime(2013, 4, 6, 17, 36, 46));
			billMock1.Setup(m => m.MovemenDetails).Returns(inBOndMock.Object);
			var container1Mock = PopulateContainer("C1BDKLHB1");
			var container2Mock = PopulateContainer();
			billMock1.Setup(m => m.Containers).Returns(new IACEContainer[] { container1Mock.Object, container2Mock.Object });

			manifestMock.Setup(m => m.BillOfLadingDetails).Returns(billMock1.Object);
			var builder = new ZStringBuilder();
			foreach (var messageBlock in new InBondVesselEventMessageBlockBuilder(manifestMock.Object).Build())
			{
				builder.Append(messageBlock.Serialise(true));
			}

			AssertMultilineASCIIEquals("Expected Message for " + billActionCode, expectedMessage, builder.ToStringWithNewLineBetweenAppends());

			manifestMock.VerifyAll();
			inBOndMock.VerifyAll();
			billMock1.VerifyAll();
			container1Mock.VerifyAll();
			container2Mock.VerifyAll();
		}

		public void TestCancelTransferOfLiabilityMessage()
		{
			ZString expectedMessage = @"-----------------INPM01-----------------
 Carrier Code (4-7)                :SD23
 Mode Of Transportation Code (8-9) :40
 Vessel Country Code (10-11)       :AU
 Voyage Number (35-39)             :V234
 Manifest Sequence Number (45-50)  :1
 Vessel Code (52-58)               :1234567

-----------------INPM02-----------------
 Carrier Assigned Batch Number (4-33) :BDKLHB1_<MSG PLACEHOLDER>

-----------------INPP01-----------------
 Port Of Unlading Code (4-7)    :5946
 Original Estimated Date (8-13) :18-Sep-71

-----------------ICMH01-----------------
 Message Code (4-4)   :L
 Inbond Entity (5-18) :VINB53264
 Date (19-24)         :09-Jul-12
 Time (33-38)         :1022
";
			var billActionCode = InBondAndVesselEventMessageCodeList.Codes.CancelTransferOfLiability;

			var manifestMock = new Mock<IACEBillManifestMessageAttachee>();
			PopulateINPM01(manifestMock);

			PopulateINPP01(manifestMock);

			var billMock1 = PopulateINPM02(manifestMock, billActionCode);

			var inBOndMock = new Mock<IMovemenDetails>();
			inBOndMock.Setup(m => m.PaperlessInbondNumber).Returns("VINB53264");
			inBOndMock.Setup(m => m.TOLDateTime).Returns(new ZDateTime(2012, 7, 9, 10, 22, 8));
			billMock1.Setup(m => m.MovemenDetails).Returns(inBOndMock.Object);

			manifestMock.Setup(m => m.BillOfLadingDetails).Returns(billMock1.Object);
			var builder = new ZStringBuilder();
			foreach (var messageBlock in new InBondVesselEventMessageBlockBuilder(manifestMock.Object).Build())
			{
				builder.Append(messageBlock.Serialise(true));
			}

			AssertMultilineASCIIEquals("Expected Message for " + billActionCode, expectedMessage, builder.ToStringWithNewLineBetweenAppends());

			manifestMock.VerifyAll();
			billMock1.VerifyAll();
			inBOndMock.VerifyAll();
		}

		public void TestCancelPermitsToTransferByContainerMessage()
		{
			ZString expectedMessage = @"-----------------INPM01-----------------
 Carrier Code (4-7)                :SD23
 Mode Of Transportation Code (8-9) :40
 Vessel Country Code (10-11)       :AU
 Voyage Number (35-39)             :V234
 Manifest Sequence Number (45-50)  :1
 Vessel Code (52-58)               :1234567

-----------------INPM02-----------------
 Carrier Assigned Batch Number (4-33) :BDKLHB1_<MSG PLACEHOLDER>

-----------------INPP01-----------------
 Port Of Unlading Code (4-7)    :5946
 Original Estimated Date (8-13) :18-Sep-71

-----------------ICMH01-----------------
 Message Code (4-4)   :P
 Inbond Entity (5-18) :C1BDKLHB1

-----------------ICMH02-----------------
 Reference Identifier Qualifier (4-5) :IB
 Reference Identifier (6-21)          :VINB53264
";

			var billActionCode = InBondAndVesselEventMessageCodeList.Codes.CancelPermitsToTransferByContainer;

			var manifestMock = new Mock<IACEBillManifestMessageAttachee>();
			PopulateINPM01(manifestMock);

			PopulateINPP01(manifestMock);

			var billMock1 = PopulateINPM02(manifestMock, billActionCode);

			var inBOndMock = new Mock<IMovemenDetails>();

			inBOndMock.Setup(m => m.PaperlessInbondNumber).Returns("VINB53264");
			billMock1.Setup(m => m.MovemenDetails).Returns(inBOndMock.Object);
			var container1Mock = PopulateContainer("C1BDKLHB1");
			var container2Mock = PopulateContainer();
			billMock1.Setup(m => m.Containers).Returns(new IACEContainer[] { container1Mock.Object, container2Mock.Object });

			manifestMock.Setup(m => m.BillOfLadingDetails).Returns(billMock1.Object);
			var builder = new ZStringBuilder();
			foreach (var messageBlock in new InBondVesselEventMessageBlockBuilder(manifestMock.Object).Build())
			{
				builder.Append(messageBlock.Serialise(true));
			}

			AssertMultilineASCIIEquals("Expected Message for " + billActionCode, expectedMessage, builder.ToStringWithNewLineBetweenAppends());

			manifestMock.VerifyAll();
			billMock1.VerifyAll();
			inBOndMock.VerifyAll();
			container1Mock.VerifyAll();
			container2Mock.VerifyAll();
		}

		public void TestReplaceTheUniqueVoyageIdentifierMessage()
		{
			ZString expectedMessage = @"-----------------INPM01-----------------
 Carrier Code (4-7)                :SD23
 Mode Of Transportation Code (8-9) :40
 Vessel Country Code (10-11)       :AU
 Voyage Number (35-39)             :V234
 Manifest Sequence Number (45-50)  :1
 Vessel Code (52-58)               :1234567

-----------------INPM02-----------------
 Carrier Assigned Batch Number (4-33) :BDKLHB1_<MSG PLACEHOLDER>

-----------------INPP01-----------------
 Port Of Unlading Code (4-7)    :5946
 Original Estimated Date (8-13) :18-Sep-71

-----------------ICMH01-----------------
 Message Code (4-4) :U

-----------------ICMH02-----------------
";

			var billActionCode = InBondAndVesselEventMessageCodeList.Codes.ReplaceTheUniqueVoyageIdentifier;

			var manifestMock = new Mock<IACEBillManifestMessageAttachee>();
			PopulateINPM01(manifestMock);

			PopulateINPP01(manifestMock);

			var billMock1 = PopulateINPM02(manifestMock, billActionCode);

			manifestMock.Setup(m => m.BillOfLadingDetails).Returns(billMock1.Object);
			var builder = new ZStringBuilder();
			foreach (var messageBlock in new InBondVesselEventMessageBlockBuilder(manifestMock.Object).Build())
			{
				builder.Append(messageBlock.Serialise(true));
			}

			AssertMultilineASCIIEquals("Expected Message for " + billActionCode, expectedMessage, builder.ToStringWithNewLineBetweenAppends());

			manifestMock.VerifyAll();
			billMock1.VerifyAll();
		}

		public void TestCancelPermitsToTransferArrivalByBillOfLadingMessage()
		{
			ZString expectedMessage = @"-----------------INPM01-----------------
 Carrier Code (4-7)                :SD23
 Mode Of Transportation Code (8-9) :40
 Vessel Country Code (10-11)       :AU
 Voyage Number (35-39)             :V234
 Manifest Sequence Number (45-50)  :1
 Vessel Code (52-58)               :1234567

-----------------INPM02-----------------
 Carrier Assigned Batch Number (4-33) :BDKLHB1_<MSG PLACEHOLDER>

-----------------INPP01-----------------
 Port Of Unlading Code (4-7)    :5946
 Original Estimated Date (8-13) :18-Sep-71

-----------------ICMH01-----------------
 Message Code (4-4)                            :V
 Inbond Entity (5-18)                          :BDKLHB1
 Issuer Code (29-32)                           :BDKL
 F I R M S Location On In Bond Arrival (76-79) :Z456
";
			var billActionCode = InBondAndVesselEventMessageCodeList.Codes.CancelPermitsToTransferArrivalByBillOfLading;

			var manifestMock = new Mock<IACEBillManifestMessageAttachee>();
			PopulateINPM01(manifestMock);

			PopulateINPP01(manifestMock);

			var billMock1 = PopulateINPM02(manifestMock, billActionCode);
			billMock1.Setup(m => m.IssuerCode).Returns("BDKL");
			billMock1.Setup(m => m.FIRMS).Returns("Z456");

			manifestMock.Setup(m => m.BillOfLadingDetails).Returns(billMock1.Object);
			var builder = new ZStringBuilder();
			foreach (var messageBlock in new InBondVesselEventMessageBlockBuilder(manifestMock.Object).Build())
			{
				builder.Append(messageBlock.Serialise(true));
			}

			AssertMultilineASCIIEquals("Expected Message for " + billActionCode, expectedMessage, builder.ToStringWithNewLineBetweenAppends());

			manifestMock.VerifyAll();
			billMock1.VerifyAll();
		}

		public void TestChangeInTheEstimatedDateOfArrivalMessage()
		{
			ZString expectedMessage = @"-----------------INPM01-----------------
 Carrier Code (4-7)                :SD23
 Mode Of Transportation Code (8-9) :40
 Vessel Country Code (10-11)       :AU
 Voyage Number (35-39)             :V234
 Manifest Sequence Number (45-50)  :1
 Vessel Code (52-58)               :1234567

-----------------INPM02-----------------
 Carrier Assigned Batch Number (4-33) :BDKLHB1_<MSG PLACEHOLDER>

-----------------INPP01-----------------
 Port Of Unlading Code (4-7)    :5946
 Original Estimated Date (8-13) :18-Sep-71

-----------------ICMH01-----------------
 Message Code (4-4) :Y
 Date (19-24)       :07-May-12
 Time (33-38)       :1305
";
			var billActionCode = InBondAndVesselEventMessageCodeList.Codes.ChangeInTheEstimatedDateOfArrival;

			var manifestMock = new Mock<IACEBillManifestMessageAttachee>();
			PopulateINPM01(manifestMock);

			PopulateINPP01(manifestMock);

			var billMock1 = PopulateINPM02(manifestMock, billActionCode);
			manifestMock.Setup(m => m.EventDateTime).Returns(new ZDateTime(2012, 05, 07, 13, 05, 02));

			manifestMock.Setup(m => m.BillOfLadingDetails).Returns(billMock1.Object);
			var builder = new ZStringBuilder();
			foreach (var messageBlock in new InBondVesselEventMessageBlockBuilder(manifestMock.Object).Build())
			{
				builder.Append(messageBlock.Serialise(true));
			}

			AssertMultilineASCIIEquals("Expected Message for " + billActionCode, expectedMessage, builder.ToStringWithNewLineBetweenAppends());

			manifestMock.VerifyAll();
			billMock1.VerifyAll();
		}

		public void TestRequestForInBondDiversionMessage()
		{
			ZString expectedMessage = @"-----------------INPM01-----------------
 Carrier Code (4-7)                :SD23
 Mode Of Transportation Code (8-9) :40
 Vessel Country Code (10-11)       :AU
 Voyage Number (35-39)             :V234
 Manifest Sequence Number (45-50)  :1
 Vessel Code (52-58)               :1234567

-----------------INPM02-----------------
 Carrier Assigned Batch Number (4-33) :BDKLHB1_<MSG PLACEHOLDER>

-----------------INPP01-----------------
 Port Of Unlading Code (4-7)    :5946
 Original Estimated Date (8-13) :18-Sep-71

-----------------ICMH01-----------------
 Message Code (4-4)         :Z
 Inbond Entity (5-18)       :VINB53264
 Date (19-24)               :03-Feb-12
 C B P Port (25-28)         :2354
 Time (33-38)               :1844
 Bonded Carrier I D (43-54) :965-63-5685
";
			var billActionCode = InBondAndVesselEventMessageCodeList.Codes.RequestForInBondDiversion;

			var manifestMock = new Mock<IACEBillManifestMessageAttachee>();
			PopulateINPM01(manifestMock);

			PopulateINPP01(manifestMock);

			var billMock1 = PopulateINPM02(manifestMock, billActionCode);

			var inBOndMock = new Mock<IMovemenDetails>();
			inBOndMock.Setup(m => m.USPortOfDestination).Returns("2354");
			inBOndMock.Setup(m => m.BondedCarrierID).Returns("965-63-5685");
			inBOndMock.Setup(m => m.PaperlessInbondNumber).Returns("VINB53264");
			inBOndMock.Setup(m => m.ArrivalDateTime).Returns(new ZDateTime(2012, 2, 3, 18, 44, 53));
			billMock1.Setup(m => m.MovemenDetails).Returns(inBOndMock.Object);

			manifestMock.Setup(m => m.BillOfLadingDetails).Returns(billMock1.Object);
			var builder = new ZStringBuilder();
			foreach (var messageBlock in new InBondVesselEventMessageBlockBuilder(manifestMock.Object).Build())
			{
				builder.Append(messageBlock.Serialise(true));
			}

			AssertMultilineASCIIEquals("Expected Message for " + billActionCode, expectedMessage, builder.ToStringWithNewLineBetweenAppends());

			manifestMock.VerifyAll();
			billMock1.VerifyAll();
			inBOndMock.VerifyAll();
		}

		void PopulateINPM01(Mock<IACEBillManifestMessageAttachee> manifestMock)
		{
			manifestMock.Setup(m => m.CarrierCode).Returns("SD23");
			manifestMock.Setup(m => m.ModeOfTransportationCode).Returns("40");
			manifestMock.Setup(m => m.ConveyanceCountryCode).Returns("AU");
			manifestMock.Setup(m => m.VoyageNumber).Returns("V234");
			manifestMock.Setup(m => m.ManifestSequenceNumber).Returns("1");
			manifestMock.Setup(m => m.ConveyanceCode).Returns("1234567");
		}

		Mock<IACEBillOfLading> PopulateINPM02(Mock<IACEBillManifestMessageAttachee> manifestMock, string billActionCode)
		{
			var billMock1 = new Mock<IACEBillOfLading>();
			billMock1.Setup(m => m.BillActionCode).Returns(billActionCode);
			billMock1.Setup(m => m.BillOfLadingSequenceNumber).Returns("BDKLHB1");

			return billMock1;
		}

		void PopulateINPP01(Mock<IACEBillManifestMessageAttachee> manifestMock)
		{
			var portMock = new Mock<IPort>();
			portMock.Setup(m => m.DistrictPortOfUnladingCode).Returns("5946");
			portMock.Setup(m => m.OriginalEstimatedDate).Returns(ZDate.BrettsBirthday);
			manifestMock.Setup(m => m.PortDetails).Returns(portMock.Object);
		}

		Mock<IACEContainer> PopulateContainer(ZString containerNumber = new ZString())
		{
			var containerMock = new Mock<IACEContainer>();

			if (!containerNumber.IsEmpty)
			{
				containerMock.Setup(m => m.ContainerEquipmentNo).Returns(containerNumber);
			}

			var vehicleDetail1Mock = new Mock<IVehicleDetails>();
			vehicleDetail1Mock.Setup(m => m.VIN).Returns("VIN1234658");
			var vehicleDetail2Mock = new Mock<IVehicleDetails>();
			vehicleDetail2Mock.Setup(m => m.VIN).Returns("");
			var hazardous1Mock = new Mock<IHazardousMaterial>();
			hazardous1Mock.Setup(m => m.HazMatCode).Returns("UN114561");
			hazardous1Mock.Setup(m => m.HazMatClass).Returns("CL32");
			hazardous1Mock.Setup(m => m.HazMatQualifier).Returns("U");
			hazardous1Mock.Setup(m => m.HazMatClassificationDesc).Returns("SHIPPING NAME 1");
			hazardous1Mock.Setup(m => m.ContactName).Returns("BOB THE BUILDER");

			hazardous1Mock.Setup(m => m.FlashPointTemp).Returns(-32m);
			hazardous1Mock.Setup(m => m.IsFlashPointTempRelevant).Returns(ZBool.True);
			hazardous1Mock.Setup(m => m.HazMatDesc).Returns("HAZRDOUS DESCRIPTION THAT IS LONG AND SHOULD BE AT LEAST TWO LINE AND NOT ONE LINE");
			hazardous1Mock.Setup(m => m.IsHazRelevant).Returns(ZBool.True);

			var hazardous2Mock = new Mock<IHazardousMaterial>();
			hazardous2Mock.Setup(m => m.HazMatCode).Returns("R9658");
			hazardous2Mock.Setup(m => m.HazMatClass).Returns("CL69");
			hazardous2Mock.Setup(m => m.HazMatQualifier).Returns("U");
			hazardous2Mock.Setup(m => m.HazMatClassificationDesc).Returns("SHIPPING NAME 2");
			hazardous2Mock.Setup(m => m.ContactName).Returns("WENDY THE DESTROYER");

			hazardous2Mock.Setup(m => m.FlashPointTemp).Returns(45m);
			hazardous2Mock.Setup(m => m.IsFlashPointTempRelevant).Returns(ZBool.True);
			hazardous2Mock.Setup(m => m.HazMatDesc).Returns("HAZRDOUS DESCRIPTION");
			hazardous2Mock.Setup(m => m.IsHazRelevant).Returns(ZBool.True);

			var hazardous3Mock = new Mock<IHazardousMaterial>();
			hazardous3Mock.Setup(m => m.HazMatCode).Returns("1381E");
			hazardous3Mock.Setup(m => m.HazMatClass).Returns("4.2");
			hazardous3Mock.Setup(m => m.HazMatQualifier).Returns("U");
			hazardous3Mock.Setup(m => m.HazMatDesc).Returns("PHOSPHORUS");
			hazardous3Mock.Setup(m => m.ContactName).Returns("AARON");
			hazardous3Mock.Setup(m => m.FlashPointTemp).Returns(0m);
			hazardous3Mock.Setup(m => m.HazMatClassificationDesc).Returns(string.Empty);

			return containerMock;
		}
	}
}
