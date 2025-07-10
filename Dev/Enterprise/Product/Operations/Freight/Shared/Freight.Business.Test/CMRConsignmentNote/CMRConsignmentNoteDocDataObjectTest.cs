using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Moq;
using NUnit.Framework;
using WTG.NUnit;
using static Enterprise.Freight.Business.Testing.DocDataObjectTestUtility;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(CMRConsignmentNoteDocDataObject))]
	sealed class CMRConsignmentNoteDocDataObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Should throw exception when wrapper is null", () => new CMRConsignmentNoteDocDataObject(null));
		}

		[ExpectNoExceptions]
		public void TestSupplierAddress()
		{
			var cmrDocDataObject = GetNewDataObject();
			NUnit.Framework.Assert.That(cmrDocDataObject.SupplierAddress, Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "When no address has been provided for supplier, doc data object is empty");

			var expectedSupplierAddress = "TEST EXPORTER COMPANY\nTEST EXPORTER ADDRESS LINE\nSYDNEY 2015\nAUSTRALIA";
			cmrConsignmentNoteMock.Setup(x => x.SupplierAddress).Returns(expectedSupplierAddress);
			cmrDocDataObject = GetNewDataObject();
			NUnit.Framework.Assert.That(cmrDocDataObject.SupplierAddress, Is.EqualTo(expectedSupplierAddress).Using(CustomComparers.TypeComparison), "When address has been provided for supplier, doc data object is filled");
		}

		[ExpectNoExceptions]
		public void TestImporterAddress()
		{
			var cmrDocDataObject = GetNewDataObject();
			NUnit.Framework.Assert.That(cmrDocDataObject.ImporterAddress, Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "When no address has been provided for importer, doc data object is empty");

			var expectedImporterAddress = "TEST IMPORTER COMPANY\nTEST IMPORTER ADDRESS LINE\nSEOUL 58321\nKOREA, REPUBLIC OF";
			cmrConsignmentNoteMock.Setup(x => x.ImporterAddress).Returns(expectedImporterAddress);
			cmrDocDataObject = GetNewDataObject();
			NUnit.Framework.Assert.That(cmrDocDataObject.ImporterAddress, Is.EqualTo(expectedImporterAddress).Using(CustomComparers.TypeComparison), "When address has been provided for importer, doc data object is filled");
		}

		[ExpectNoExceptions]
		public void TestInternationalConsignementNote()
		{
			var cmrDocDataObject = GetNewDataObject();
			NUnit.Framework.Assert.That(cmrDocDataObject.InternationalConsignmentNote, Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "MasterBIll");

			var expectedMasterBill = "ARG001";
			cmrConsignmentNoteMock.Setup(x => x.InternationalConsignmentNote).Returns(expectedMasterBill);
			cmrDocDataObject = GetNewDataObject();
			NUnit.Framework.Assert.That(cmrDocDataObject.InternationalConsignmentNote, Is.EqualTo(expectedMasterBill).Using(CustomComparers.TypeComparison), "MasterBIll");
		}

		[ExpectNoExceptions]
		public void TestPlaceOfDelivery()
		{
			var cmrDocDataObject = GetNewDataObject();
			NUnit.Framework.Assert.That(cmrDocDataObject.PlaceOfDelivery, Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "When no address has been provided to importer, doc data object is empty");

			var expectedPlaceOfDelivery = "58321 SEOUL KR";
			cmrConsignmentNoteMock.Setup(y => y.PlaceOfDelivery).Returns(expectedPlaceOfDelivery);
			cmrDocDataObject = GetNewDataObject();
			NUnit.Framework.Assert.That(cmrDocDataObject.PlaceOfDelivery, Is.EqualTo(expectedPlaceOfDelivery).Using(CustomComparers.TypeComparison), "When place of delivery has been provided, it must be available in doc data object");
		}

		[ExpectNoExceptions]
		public void TestCityCountryDateOfGoodsTakingOver()
		{
			var cmrDocDataObject = GetNewDataObject();
			NUnit.Framework.Assert.That(cmrDocDataObject.CityCountryDateOfGoodsTakingOver, Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "When no address has been provided, doc data object is empty");

			var today = ZDate.Today;
			var todayAsString = today.ToString("dd/MM/yyyy");

			var expectedCityCountryDateOfGoodsTakingOver = $"SYDNEY AU {todayAsString}";
			cmrConsignmentNoteMock.Setup(x => x.CityCountryDateOfGoodsTakingOver).Returns(expectedCityCountryDateOfGoodsTakingOver);
			cmrDocDataObject = GetNewDataObject();
			NUnit.Framework.Assert.That(cmrDocDataObject.CityCountryDateOfGoodsTakingOver, Is.EqualTo(expectedCityCountryDateOfGoodsTakingOver).Using(CustomComparers.TypeComparison), "When Country City and JE_DateAtFinalDestination have been provided to declaration, they are available in doc data object");
		}

		[ExpectNoExceptions]
		public void TestGoodsAttachedDocuments()
		{
			var cmrDocDataObject = GetNewDataObject();
			NUnit.Framework.Assert.That(cmrDocDataObject.GoodsAttachedDocuments, Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "When no attached documents have been provided, doc data object is empty");

			var expectedGoodsAttachedDocuments = "A, B, C";
			cmrConsignmentNoteMock.Setup(x => x.GoodsAttachedDocuments).Returns(expectedGoodsAttachedDocuments);
			cmrDocDataObject = GetNewDataObject();
			NUnit.Framework.Assert.That(cmrDocDataObject.GoodsAttachedDocuments, Is.EqualTo("A, B, C").Using(CustomComparers.TypeComparison), "Three attached documents references are expected");
		}

		[ExpectNoExceptions]
		public void TestCarrierAddress()
		{
			var cmrDocDataObject = GetNewDataObject();
			NUnit.Framework.Assert.That(cmrDocDataObject.SupplierAddress, Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "When no address has been provided for supplier, doc data object is empty");

			var expectedCarrierAddress = "THE BEST CARRIER COMPANY\r\n109 MAIN ST\r\nAUSTRALIA";
			cmrConsignmentNoteMock.Setup(x => x.CarrierAddress).Returns(expectedCarrierAddress);

			cmrDocDataObject = GetNewDataObject();
			NUnit.Framework.Assert.That(cmrDocDataObject.CarrierAddress, Is.EqualTo(expectedCarrierAddress).Using(CustomComparers.TypeComparison), "When address has been provided for carrier, doc data object is filled");
		}

		[ExpectNoExceptions]
		public void TestLineDetailsBox6_7_8_9()
		{
			var cmrDocDataObject = GetNewDataObject();
			NUnit.Framework.Assert.That(cmrDocDataObject.LineDetailsBox6_7_8_9, Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "When no line details are available, doc data object is empty");

			var expectedLineDetails = "arg01, 1, GG; THIS IS A DUMMY LINE";
			cmrConsignmentNoteMock.Setup(x => x.LineDetailsBox6_7_8_9).Returns(expectedLineDetails);

			cmrDocDataObject = GetNewDataObject();
			NUnit.Framework.Assert.That(cmrDocDataObject.LineDetailsBox6_7_8_9, Is.EqualTo(expectedLineDetails).Using(CustomComparers.TypeComparison), "When line details are available, doc data object is filled");
		}

		[ExpectNoExceptions]
		public void TestLineDetailsTariffCodeBox10()
		{
			var cmrDocDataObject = GetNewDataObject();
			NUnit.Framework.Assert.That(cmrDocDataObject.LineDetailsTariffCodeBox10, Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "When no tariffs are available, doc data object is empty");

			var expectedLineDetailsBox10 = "1234567890";
			cmrConsignmentNoteMock.Setup(x => x.LineDetailsTariffCodeBox10).Returns(expectedLineDetailsBox10);

			cmrDocDataObject = GetNewDataObject();
			NUnit.Framework.Assert.That(cmrDocDataObject.LineDetailsTariffCodeBox10, Is.EqualTo(expectedLineDetailsBox10).Using(CustomComparers.TypeComparison), "When tariffs are available, doc data object is filled");
		}

		[ExpectNoExceptions]
		public void TestLineDetailsGrossWeightInKGBox11()
		{
			var cmrDocDataObject = GetNewDataObject();
			NUnit.Framework.Assert.That(cmrDocDataObject.LineDetailsGrossWeightInKGBox11, Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "When no weights are available, doc data object is empty");

			var expectedLineDetailsBox11 = "101.200324\r\n10.34567";
			cmrConsignmentNoteMock.Setup(x => x.LineDetailsGrossWeightInKGBox11).Returns(expectedLineDetailsBox11);

			cmrDocDataObject = GetNewDataObject();
			NUnit.Framework.Assert.That(cmrDocDataObject.LineDetailsGrossWeightInKGBox11, Is.EqualTo(expectedLineDetailsBox11).Using(CustomComparers.TypeComparison), "When weights are available, doc data object is filled");
		}

		[ExpectNoExceptions]
		public void TestLineDetailsVolumeInM3Box12()
		{
			var cmrDocDataObject = GetNewDataObject();
			NUnit.Framework.Assert.That(cmrDocDataObject.LineDetailsVolumeInM3Box12, Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "When no volumes are available, doc data object is empty");

			var expectedLineDetailsBox12 = "11.200324\r\n100.34567";
			cmrConsignmentNoteMock.Setup(x => x.LineDetailsVolumeInM3Box12).Returns(expectedLineDetailsBox12);

			cmrDocDataObject = GetNewDataObject();
			NUnit.Framework.Assert.That(cmrDocDataObject.LineDetailsVolumeInM3Box12, Is.EqualTo(expectedLineDetailsBox12).Using(CustomComparers.TypeComparison), "When volumes are available, doc data object is filled");
		}

		[ExpectNoExceptions]
		public void TestIncotermAndTextBox14()
		{
			var cmrDocDataObject = GetNewDataObject();
			NUnit.Framework.Assert.That(cmrDocDataObject.IncotermAndTextBox14, Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "When no Incoterm is available, doc data object is empty");

			var expectedIncotermAndText = "FOB - ";
			cmrConsignmentNoteMock.Setup(x => x.IncotermAndTextBox14).Returns(expectedIncotermAndText);

			cmrDocDataObject = GetNewDataObject();
			NUnit.Framework.Assert.That(cmrDocDataObject.IncotermAndTextBox14, Is.EqualTo(expectedIncotermAndText).Using(CustomComparers.TypeComparison), "When incoterm is available, doc data object is filled");
		}

		[ExpectNoExceptions]
		public void TestTransportIDBox23()
		{
			var cmrDocDataObject = GetNewDataObject();
			NUnit.Framework.Assert.That(cmrDocDataObject.TransportIDBox23, Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "When no transportID is available, doc data object is empty");

			var expectedTrasportID = "ARG099 ";
			cmrConsignmentNoteMock.Setup(x => x.TransportIDBox23).Returns(expectedTrasportID);

			cmrDocDataObject = GetNewDataObject();
			NUnit.Framework.Assert.That(cmrDocDataObject.TransportIDBox23, Is.EqualTo(expectedTrasportID).Using(CustomComparers.TypeComparison), "When transprtID is available, doc data object is filled");
		}

		[ExpectNoExceptions]
		public void TestFreeEnfranchisement()
		{
			var cmrDocDataObject = GetNewDataObject();
			NUnit.Framework.Assert.That(cmrDocDataObject.FreeEnfranchisement, Is.EqualTo(false).Using(CustomComparers.TypeComparison), "Free enfranchisement is always false");
		}

		[ExpectNoExceptions]
		public void TestAssignedEnfranchisement()
		{
			var cmrDocDataObject = GetNewDataObject();
			NUnit.Framework.Assert.That(cmrDocDataObject.AssignedEnfranchisement, Is.EqualTo(false).Using(CustomComparers.TypeComparison), "Assigned enfranchisement is always false");
		}

		[ExpectNoExceptions]
		public void TestJobNumber()
		{
			var cmrDocDataObject = GetNewDataObject();
			NUnit.Framework.Assert.That(cmrDocDataObject.JobNumber, Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "When no JobNumber is available, doc data object is empty");

			var expectedJobNumber = "ARG123456789";
			cmrConsignmentNoteMock.Setup(x => x.JobNumber).Returns(expectedJobNumber);

			cmrDocDataObject = GetNewDataObject();
			NUnit.Framework.Assert.That(cmrDocDataObject.JobNumber, Is.EqualTo(expectedJobNumber).Using(CustomComparers.TypeComparison), "When JobNumber is available, doc data object is filled");
		}

		[ExpectNoExceptions]
		public void TestSubsequentCarriers()
		{
			var cmrDocDataObject = GetNewDataObject();
			NUnit.Framework.Assert.That(cmrDocDataObject.SubsequentCarriers, Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "SubsequentCarriers empty by default");
		}

		[ExpectNoExceptions]
		public void TestCarriersReservations()
		{
			var cmrDocDataObject = GetNewDataObject();
			NUnit.Framework.Assert.That(cmrDocDataObject.CarriersReservations, Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "CarriersReservations empty by default");
		}

		[ExpectNoExceptions]
		public void TestSendersInstructions()
		{
			var cmrDocDataObject = GetNewDataObject();
			NUnit.Framework.Assert.That(cmrDocDataObject.SendersInstructions, Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "SendersInstructions empty by default");
		}

		[ExpectNoExceptions]
		public void TestEstablishedInPlace()
		{
			var cmrDocDataObject = GetNewDataObject();
			NUnit.Framework.Assert.That(cmrDocDataObject.EstablishedInPlace, Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "EstablishedInPlace empty by default");
		}

		[ExpectNoExceptions]
		public void TestEstablishedInDate()
		{
			var cmrDocDataObject = GetNewDataObject();
			NUnit.Framework.Assert.That(cmrDocDataObject.EstablishedInDate, Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "EstablishedInDate empty by default");
		}

		[ExpectNoExceptions]
		public void TestSpecialAgreements()
		{
			var cmrDocDataObject = GetNewDataObject();
			NUnit.Framework.Assert.That(cmrDocDataObject.SpecialAgreements, Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "SpecialAgreements empty by default");
		}

		[ExpectNoExceptions]
		public void TestGoodsReceivedPlace()
		{
			var cmrDocDataObject = GetNewDataObject();
			NUnit.Framework.Assert.That(cmrDocDataObject.GoodsReceivedPlace, Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "GoodsReceivedplace empty by default");
		}

		[ExpectNoExceptions]
		public void TestGoodsReceivedDate()
		{
			var cmrDocDataObject = GetNewDataObject();
			NUnit.Framework.Assert.That(cmrDocDataObject.GoodsReceivedDate, Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "GoodsReceivedDate empty by default");
		}

		[ExpectNoExceptions]
		public void TestTrailerID()
		{
			var cmrDocDataObject = GetNewDataObject();
			NUnit.Framework.Assert.That(cmrDocDataObject.TrailerID, Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "TrailerID empty by default");
		}

		[ExpectNoExceptions]
		public void TestDangerousGoodsClass()
		{
			var cmrDocDataObject = GetNewDataObject();
			NUnit.Framework.Assert.That(cmrDocDataObject.DangerousGoodsClass, Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "DangerousGoodsClass empty by default");
		}

		[ExpectNoExceptions]
		public void TestDangerousGoodsNumber()
		{
			var cmrDocDataObject = GetNewDataObject();
			NUnit.Framework.Assert.That(cmrDocDataObject.DangerousGoodsNumber, Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "DangerousGoodsNumber empty by default");
		}

		[ExpectNoExceptions]
		public void TestDangerousGoodsLetter()
		{
			var cmrDocDataObject = GetNewDataObject();
			NUnit.Framework.Assert.That(cmrDocDataObject.DangerousGoodsLetter, Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "DangerousGoodsLetter empty by default");
		}

		[ExpectNoExceptions]
		public void TestPropertiesAreAlterable()
		{
			var exporterDetails = @"FREDS SUPPLY CO
367 GEORGE ST
SYDNEY NSW 2000
AUSTRALIA";

			var importerDetails = @"JIMS IMPORT CO
PETUELRING 130
80809 MÜNCHEN
GERMANY";

			var carrierAddress = @"ENZO CARRIER 
VIALE MANFROTTO
22036 ERBA
ITALIA";

			var goodsAttachedDocuments = "A, B, C";
			var today = ZDate.Today;
			var todayAsString = today.ToString("dd/MM/yyyy");

			var cityCountryDateOfGoodsTakingOver = $"SYDNEY AU {todayAsString}";
			var internationalConsignmentNote = "WHATEVER";
			var placeOfDelivery = "58321 SEOUL KR";
			var lineDetailsBox6_7_8_9 = "arg01, 1 GG; THIS IS A DUMMY LINE";
			var lineDetailsTariffCodeBox10 = "1234567890";
			var lineDetailsGrossWeightInKGBox11 = "101.200324\r\n10.34567";
			var lineDetailsVolumeInM3Box12 = "11.200324\r\n100.34567";
			var incotermAndTextBox14 = "FOB - ";
			var transportIDBox23 = "ARG89776";
			var jobNumber = "ARG1234567890";

			cmrConsignmentNoteMock.Setup(x => x.SupplierAddress).Returns(exporterDetails);
			cmrConsignmentNoteMock.Setup(x => x.ImporterAddress).Returns(importerDetails);
			cmrConsignmentNoteMock.Setup(x => x.CarrierAddress).Returns(carrierAddress);
			cmrConsignmentNoteMock.Setup(x => x.GoodsAttachedDocuments).Returns(goodsAttachedDocuments);
			cmrConsignmentNoteMock.Setup(x => x.CityCountryDateOfGoodsTakingOver).Returns(cityCountryDateOfGoodsTakingOver);
			cmrConsignmentNoteMock.Setup(x => x.InternationalConsignmentNote).Returns(internationalConsignmentNote);
			cmrConsignmentNoteMock.Setup(x => x.PlaceOfDelivery).Returns(placeOfDelivery);
			cmrConsignmentNoteMock.Setup(x => x.LineDetailsBox6_7_8_9).Returns(lineDetailsBox6_7_8_9);
			cmrConsignmentNoteMock.Setup(x => x.LineDetailsTariffCodeBox10).Returns(lineDetailsTariffCodeBox10);
			cmrConsignmentNoteMock.Setup(x => x.LineDetailsGrossWeightInKGBox11).Returns(lineDetailsGrossWeightInKGBox11);
			cmrConsignmentNoteMock.Setup(x => x.LineDetailsVolumeInM3Box12).Returns(lineDetailsVolumeInM3Box12);
			cmrConsignmentNoteMock.Setup(x => x.IncotermAndTextBox14).Returns(incotermAndTextBox14);
			cmrConsignmentNoteMock.Setup(x => x.TransportIDBox23).Returns(transportIDBox23);
			cmrConsignmentNoteMock.Setup(x => x.JobNumber).Returns(jobNumber);

			CombineAssertions(() =>
			{
				var cmrDocDataObject = GetNewDataObject();
				AssertDataObjectPropertyIsAlterable(cmrDocDataObject, exporterDetails, nameof(cmrDocDataObject.SupplierAddress));
				AssertDataObjectPropertyIsAlterable(cmrDocDataObject, importerDetails, nameof(cmrDocDataObject.ImporterAddress));
				AssertDataObjectPropertyIsAlterable(cmrDocDataObject, carrierAddress, nameof(cmrDocDataObject.CarrierAddress));
				AssertDataObjectPropertyIsAlterable(cmrDocDataObject, goodsAttachedDocuments, nameof(cmrDocDataObject.GoodsAttachedDocuments));
				AssertDataObjectPropertyIsAlterable(cmrDocDataObject, cityCountryDateOfGoodsTakingOver, nameof(cmrDocDataObject.CityCountryDateOfGoodsTakingOver));
				AssertDataObjectPropertyIsAlterable(cmrDocDataObject, internationalConsignmentNote, nameof(cmrDocDataObject.InternationalConsignmentNote));
				AssertDataObjectPropertyIsAlterable(cmrDocDataObject, placeOfDelivery, nameof(cmrDocDataObject.PlaceOfDelivery));
				AssertDataObjectPropertyIsAlterable(cmrDocDataObject, lineDetailsBox6_7_8_9, nameof(cmrDocDataObject.LineDetailsBox6_7_8_9));
				AssertDataObjectPropertyIsAlterable(cmrDocDataObject, lineDetailsTariffCodeBox10, nameof(cmrDocDataObject.LineDetailsTariffCodeBox10));
				AssertDataObjectPropertyIsAlterable(cmrDocDataObject, lineDetailsGrossWeightInKGBox11, nameof(cmrDocDataObject.LineDetailsGrossWeightInKGBox11));
				AssertDataObjectPropertyIsAlterable(cmrDocDataObject, lineDetailsVolumeInM3Box12, nameof(cmrDocDataObject.LineDetailsVolumeInM3Box12));
				AssertDataObjectPropertyIsAlterable(cmrDocDataObject, incotermAndTextBox14, nameof(cmrDocDataObject.IncotermAndTextBox14));
				AssertDataObjectPropertyIsAlterable(cmrDocDataObject, transportIDBox23, nameof(cmrDocDataObject.TransportIDBox23));
				AssertDataObjectPropertyIsAlterable(cmrDocDataObject, false, nameof(cmrDocDataObject.FreeEnfranchisement));
				AssertDataObjectPropertyIsAlterable(cmrDocDataObject, false, nameof(cmrDocDataObject.AssignedEnfranchisement));
				AssertDataObjectPropertyIsAlterable(cmrDocDataObject, jobNumber, nameof(cmrDocDataObject.JobNumber));
				AssertDataObjectPropertyIsAlterable(cmrDocDataObject, string.Empty, nameof(cmrDocDataObject.SubsequentCarriers));
				AssertDataObjectPropertyIsAlterable(cmrDocDataObject, string.Empty, nameof(cmrDocDataObject.CarriersReservations));
				AssertDataObjectPropertyIsAlterable(cmrDocDataObject, string.Empty, nameof(cmrDocDataObject.SendersInstructions));
				AssertDataObjectPropertyIsAlterable(cmrDocDataObject, string.Empty, nameof(cmrDocDataObject.EstablishedInPlace));
				AssertDataObjectPropertyIsAlterable(cmrDocDataObject, string.Empty, nameof(cmrDocDataObject.EstablishedInDate));
				AssertDataObjectPropertyIsAlterable(cmrDocDataObject, string.Empty, nameof(cmrDocDataObject.SpecialAgreements));
				AssertDataObjectPropertyIsAlterable(cmrDocDataObject, string.Empty, nameof(cmrDocDataObject.GoodsReceivedPlace));
				AssertDataObjectPropertyIsAlterable(cmrDocDataObject, string.Empty, nameof(cmrDocDataObject.GoodsReceivedDate));
				AssertDataObjectPropertyIsAlterable(cmrDocDataObject, string.Empty, nameof(cmrDocDataObject.TrailerID));
				AssertDataObjectPropertyIsAlterable(cmrDocDataObject, string.Empty, nameof(cmrDocDataObject.DangerousGoodsClass));
				AssertDataObjectPropertyIsAlterable(cmrDocDataObject, string.Empty, nameof(cmrDocDataObject.DangerousGoodsNumber));
				AssertDataObjectPropertyIsAlterable(cmrDocDataObject, string.Empty, nameof(cmrDocDataObject.DangerousGoodsLetter));
			});
		}

		[ExpectNoExceptions]
		public void TestBoxNumbers()
		{
			var dataObject = GetNewDataObject();

			NUnit.Framework.Assert.That(dataObject.Box14PaymentCarriage, Is.EqualTo("14").Using(CustomComparers.TypeComparison), "Box 14");
			NUnit.Framework.Assert.That(dataObject.Box19SpecialAgreements, Is.EqualTo("19").Using(CustomComparers.TypeComparison), "Box 19");
			NUnit.Framework.Assert.That(dataObject.Box20ToBePaydBy, Is.EqualTo("20").Using(CustomComparers.TypeComparison), "Box 20");
			NUnit.Framework.Assert.That(dataObject.Box15CashOnDelivery, Is.EqualTo("15").Using(CustomComparers.TypeComparison), "Box 15");
			NUnit.Framework.Assert.That(dataObject.Box23TransportAndTrailerID, Is.EqualTo("23").Using(CustomComparers.TypeComparison), "Box 23");
		}

		protected override BusinessObject GetNewBusinessObject() => new CMRConsignmentNoteDocDataObject(cmrConsignmentNoteMock.Object);

		protected override void SetUp()
		{
			base.SetUp();

			cmrConsignmentNoteMock = new Mock<ICMRConsignmentNote>();

			cmrConsignmentNoteMock.Setup(x => x.Box14PaymentCarriage).Returns("14");
			cmrConsignmentNoteMock.Setup(x => x.Box15CashOnDelivery).Returns("15");
			cmrConsignmentNoteMock.Setup(x => x.Box19SpecialAgreements).Returns("19");
			cmrConsignmentNoteMock.Setup(x => x.Box20ToBePaidBy).Returns("20");
			cmrConsignmentNoteMock.Setup(x => x.Box23TransportAndTrailerID).Returns("23");
			cmrConsignmentNoteMock.Setup(x => x.TextLimitCalculator.CalculateLimits(It.IsAny<ZString>(), It.IsAny<int>(), It.IsAny<int>()))
				.Returns((ZString text, int maxLines, int maxLineLength) => text);
		}
		Mock<ICMRConsignmentNote> cmrConsignmentNoteMock;

		CMRConsignmentNoteDocDataObject GetNewDataObject() => GetNewBusinessObject() as CMRConsignmentNoteDocDataObject;
	}
}
